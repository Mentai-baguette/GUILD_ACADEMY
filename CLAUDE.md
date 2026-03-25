# CLAUDE.md — Claude Code 用プロジェクト指示書

プロジェクト共通ルールは [AGENTS.md](./AGENTS.md) を参照。以下は Claude Code 固有の指示。

## AI / MCP 利用ルール

- **Unity MCP はローカル開発環境のみ**で使用する。CI/共有マシンでは使用しない
- MCP経由でのファイル操作は **Assets/ 配下のみ** に限定する
- **秘密情報（API キー、パスワード、個人情報）を含むログやファイルを AI に送信しない**
- AI が生成・変更したコードは必ず差分を確認してからコミットする
- AI ツールのローカル設定（`.claude/settings.local.json` 等）はコミットしない
