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

- Unity MCP はローカル開発環境のみで使用する
- MCP 経由の操作対象は `Assets/` 配下に限定する
- 秘密情報を含むログやファイルを AI に送信しない
- AI が生成したコードは差分を確認してからコミットする

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

### CI/CD（未実装）

- GitHub Actions の workflow はまだ作成されていない
- テスト自動実行は workflow 作成後に有効になる

### 推奨設定（未確認）

- [ ] Secret scanning を有効化
- [ ] Dependabot alerts を有効化
- [ ] GitHub Actions の permissions を最小権限に設定

## セーブデータ・ゲームデータ

- ローカル保存のセーブデータは改ざんされる前提で設計する
- 将来オンライン要素を追加する場合、クライアント保存値を信頼しない
- Steam Cloud 連携時は別途脅威分析を実施する

## 脆弱性の報告

セキュリティ上の問題を見つけた場合は、public issue ではなくチーム内の Slack で報告すること。
