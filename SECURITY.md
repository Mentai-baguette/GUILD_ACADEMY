# セキュリティポリシー

## リポジトリの公開範囲

このリポジトリは **public** です。以下のルールを厳守すること。

### コミットしてはいけないもの

- API キー、パスワード、トークン、秘密鍵
- `.env` ファイル（`.gitignore` で除外済み）
- Notion / Slack / その他サービスの内部URL・ページID
- 個人情報（本名、メールアドレス、学籍番号等）
- Unity のライセンスファイル

### 内部リンクの扱い

Notion、Slack、Google Drive 等の内部URLは公開リポジトリに載せない。
チーム内の共有は Slack や Notion 内で行い、リポジトリのドキュメントでは「チーム内で共有」と記載する。

## AI / MCP の利用

- Unity MCP はローカル開発環境のみで使用する。CI/共有マシンでは使用しない
- MCP 経由の操作対象は `Assets/` 配下に限定する
- AI が生成したコードは差分を確認してからコミットする
- MCP/AI 関連のローカル設定（`.mcp.json`, `ProjectSettings/AI.Assistant/` 等）はコミットしない

### AI に渡してよい範囲

- `Assets/` 配下のスクリプト・シーン・設定ファイル
- `Packages/manifest.json`（パッケージ一覧）
- `ProjectSettings/` 配下の共有設定（AI.Assistant/ を除く）
- `docs/` 配下のドキュメント
- `game_design_document.md`, `AGENTS.md`, `CLAUDE.md`, `README.md`

### AI に渡してはいけないもの

- `Library/`, `Logs/`, `Temp/`, `UserSettings/`（ローカル生成物）
- `.env`, `.env.*`, `*.pem`, `*.key`, `*.p12`（秘密情報）
- Notion / Slack / Google Drive の内部URL・ページID
- 個人情報（本名、メールアドレス、学籍番号等）
- Unity ライセンスファイル（`*.ulf`, `*.alf`）

## アカウント管理

- GitHub アカウントは 2FA（二要素認証）必須
- 1人1アカウント、共有アカウント禁止
- メンバー離脱時は全サービスのアクセスを即 revoke
- パスワード・トークンは個人管理、他人に共有しない

## GitHub 設定

### Branch Protection（GitHub 側で要確認）

- `main` / `develop`: 直接push禁止、PR必須（1名以上のapproval）
- enforce admins: 有効（管理者も例外なし）
- force push: 禁止

> **Note:** 上記は GitHub リポジトリ設定で管理。設定状況は GitHub Settings → Branches で確認すること。

### CI/CD

- GitHub Actions で Edit Mode テストを自動実行（`test.yml`）
- workflow の permissions は `contents: read`（最小権限）
- 全アクションは commit SHA pinning で固定
- `UNITY_LICENSE` 未設定時はテストをスキップ（赤にならない）
- `UNITY_LICENSE` は GitHub Secrets で管理し、リポジトリにコミットしない
- `activation.yml` で生成する `.alf` ファイルは Actions artifact で一時共有のみ。コミット禁止・長期保存禁止

#### UNITY_LICENSE 設定手順

1. GitHub → Actions → "Request Unity Activation File" → Run workflow
2. Artifacts から `.alf` ファイルをダウンロード
3. https://license.unity3d.com/manual で `.alf` をアップロードし `.ulf` を取得
4. GitHub → Settings → Secrets and variables → Actions → `UNITY_LICENSE` に `.ulf` の中身を登録

### 推奨設定（未確認）

- [ ] Secret scanning を有効化
- [ ] Dependabot alerts を有効化
- [ ] UNITY_LICENSE 設定後に required check を有効化

## セーブデータ・ゲームデータ

- ローカル保存のセーブデータは改ざんされる前提で設計する
- 将来オンライン要素を追加する場合、クライアント保存値を信頼しない
- Steam Cloud 連携時は別途脅威分析を実施する

## 脆弱性の報告

セキュリティ上の問題を見つけた場合は、public issue ではなくチーム内の Slack で報告すること。
