# Unityエラー切り分けガイド（Import + AI Assistant）

このガイドは、以下3系統のエラーを根本原因まで切り分けるための実運用手順です。

- `No Sprites or Texture are generated...`
- `Account API did not become accessible within 30 seconds...`
- `LoadSerializedFileAndForget can only be called from the main thread...`

## 1. 事前準備

```bash
cd GUILD_ACADEMY
chmod +x tools/diagnostics/unity_error_triage.sh
```

## 2. 自動診断レポートを作成

```bash
tools/diagnostics/unity_error_triage.sh
```

出力先: `Logs/triage-report-YYYYMMDD-HHMMSS.md`

レポートで確認する項目:

- 3系統エラーの検出件数
- `com.unity.ai.assistant` のバージョン
- `.ase` の対象アセット一覧
- 各 `.ase.meta` の `importHiddenLayers` と `spritePixelsToUnits`

## 3. Sprite/Texture生成失敗の切り分け

1. Unity Console を Clear する
2. `Assets/_Project/Data/maptile/DawnLike/Commissions` 配下の `.ase` を1つずつ Reimport する
3. 警告が出たファイル名を記録する
4. 同名 `.png` を Reimport して比較する（形式依存か素材依存か判定）

追加済み対処:

- Commissions 配下 `.ase.meta` で `importHiddenLayers: 1` を適用済み

## 4. AI Assistantエラーの切り分け

1. Unity Editor を前面に固定して再現確認
2. Unity Account のサインイン状態を確認
3. フォーカス喪失後に再現するか比較
4. 同時にレポートを再生成して件数推移を確認

補足:

- 現在の依存は `com.unity.ai.assistant: 2.3.0-pre.2`（プレビュー版）
- MCP relay 設定は `.vscode/mcp.json` で有効

## 4.1 MCP背景検証エラーの安定化（実装済み）

`Background validation/approval error: LoadSerializedFileAndForget...` が継続する場合は、
UnityメニューからMCPのプロセス検証だけを無効化できます。

- `Tools/GuildAcademy/AI Assistant/Apply MCP Stability Patch`
	- `processValidationEnabled = false` を設定
	- MCP Bridge自体は維持（完全停止しない）

- `Tools/GuildAcademy/AI Assistant/Restore MCP Default Validation`
	- 既定に戻す（`processValidationEnabled = true`）

- `Tools/GuildAcademy/AI Assistant/Toggle Auto Apply Stability Patch`
	- Editor起動時に自動適用するかを切替

実装ファイル:

- MCP/AI のローカル設定はリポジトリコードから自動変更しない

## 5. 判定基準

- `.ase` のみ失敗し `.png` が成功: Importer設定または `.ase` 形式側
- `.ase` と `.png` の両方失敗: 素材データまたはプロジェクト状態
- AIエラーがフォーカス依存: Editor状態起因
- AIエラーが常時発生: ネットワーク到達性またはパッケージ側要因

## 6. 次の一手

- 失敗アセットが限定される場合: 該当 `.ase` の再保存（Aseprite）を実施
- AIエラーが継続する場合: 再現手順 + 時刻付きログで Unity package 側調査へ進む
