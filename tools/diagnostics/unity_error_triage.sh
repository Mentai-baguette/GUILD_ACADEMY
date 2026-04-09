#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
LOG_DIR="$ROOT_DIR/Logs"
MANIFEST_FILE="$ROOT_DIR/Packages/manifest.json"
MCP_FILE="$ROOT_DIR/.vscode/mcp.json"

if [[ ! -d "$LOG_DIR" ]]; then
  echo "Logs directory not found: $LOG_DIR" >&2
  exit 1
fi

if [[ ! -f "$MANIFEST_FILE" ]]; then
  echo "manifest.json not found: $MANIFEST_FILE" >&2
  exit 1
fi

TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
REPORT_FILE="$LOG_DIR/triage-report-$TIMESTAMP.md"

HAS_RG=0
if command -v rg >/dev/null 2>&1; then
  HAS_RG=1
fi

search_matches() {
  local pattern="$1"
  shift
  if [[ "$HAS_RG" -eq 1 ]]; then
    rg -n --no-messages "$pattern" "$@"
  else
    grep -nE "$pattern" "$@" 2>/dev/null || true
  fi
}

count_pattern() {
  local pattern="$1"
  local target_glob="$2"
  local count
  count=$(search_matches "$pattern" $target_glob | wc -l | tr -d ' ')
  echo "$count"
}

find_ase_paths() {
  search_matches "path: .*\\.ase" "$LOG_DIR"/AssetImportWorker*.log \
    | sed -E 's/^.*path: //' \
    | sort -u
}

extract_value() {
  local file="$1"
  local key="$2"
  search_matches "$key" "$file" | head -n 1 | sed -E 's/^.*: *//'
}

SPRITE_ERR_COUNT=$(count_pattern "No Sprites or Texture are generated" "$LOG_DIR/AssetImportWorker*.log")
API_TIMEOUT_COUNT=$(count_pattern "Account API did not become accessible within 30 seconds" "$LOG_DIR/traces.jsonl")
THREAD_ERR_COUNT=$(count_pattern "LoadSerializedFileAndForget can only be called from the main thread" "$LOG_DIR/traces.jsonl")
ASSISTANT_VERSION=$(extract_value "$MANIFEST_FILE" "com.unity.ai.assistant")
RELAY_CMD=$(search_matches 'relay_mac_arm64|relay' "$MCP_FILE" | head -n 1 | sed -E 's/^.*: *//')

{
  echo "# Unity Error Triage Report"
  echo
  echo "- generated_at: $(date '+%Y-%m-%d %H:%M:%S %z')"
  echo "- workspace: $ROOT_DIR"
  echo
  echo "## Error Summary"
  echo
  echo "- no_sprites_or_texture: $SPRITE_ERR_COUNT"
  echo "- account_api_timeout: $API_TIMEOUT_COUNT"
  echo "- main_thread_violation: $THREAD_ERR_COUNT"
  echo
  echo "## Environment Snapshot"
  echo
  echo "- com.unity.ai.assistant: $ASSISTANT_VERSION"
  echo "- mcp_relay: ${RELAY_CMD:-not-found}"
  echo
  echo "## Aseprite Import Targets"
  echo
  ASE_LIST="$(find_ase_paths || true)"
  if [[ -z "$ASE_LIST" ]]; then
    echo "No .ase import entries were found in AssetImportWorker logs."
  else
    while IFS= read -r ase_path; do
      [[ -z "$ase_path" ]] && continue
      abs_ase="$ROOT_DIR/$ase_path"
      meta_file="$abs_ase.meta"
      png_pair="${abs_ase%.ase}.png"

      if [[ -f "$meta_file" ]]; then
        import_hidden=$(extract_value "$meta_file" "importHiddenLayers" | head -n 1)
        ppu=$(extract_value "$meta_file" "spritePixelsToUnits")
      else
        import_hidden="meta-missing"
        ppu="meta-missing"
      fi

      if [[ -f "$png_pair" ]]; then
        pair_state="yes"
      else
        pair_state="no"
      fi

      echo "- asset: $ase_path"
      echo "  - has_png_pair: $pair_state"
      echo "  - importHiddenLayers: $import_hidden"
      echo "  - spritePixelsToUnits: $ppu"
    done <<< "$ASE_LIST"
  fi
  echo
  echo "## Next Actions"
  echo
  echo "1. In Unity Console, clear logs and reproduce once to isolate fresh errors."
  echo "2. Reimport only assets that appear in this report under Aseprite Import Targets."
  echo "3. If account_api_timeout or main_thread_violation keeps increasing, capture Editor.log timestamp and test with focused editor window."
} > "$REPORT_FILE"

echo "Triage report created: $REPORT_FILE"
