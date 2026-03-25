# GUILD ACADEMY: ABYSS CODE

2D学園ダークファンタジーRPG（マルチエンディング）

千葉工業大学ハッカソン「システム開発における生成AI活用ワークショップ」出展作品（2026/3/20〜4/10）

## 技術スタック

- **エンジン**: Unity 6 (6000.x) + C#
- **プラットフォーム**: macOS（開発・テスト・プレイすべてMac）
- **CI/CD**: GitHub Actions + game-ci/unity-test-runner

## 環境構築

1. [Unity Hub](https://unity.com/download) をインストール
2. Unity Hub から **Unity 6 (6000.x)** をインストール（macOS Build Support を含める）
3. Git が入っていなければ: `xcode-select --install`
4. リポジトリをクローン

```bash
git clone https://github.com/Mentai-baguette/GUILD_ACADEMY.git
cd GUILD_ACADEMY
git checkout develop
```

5. Unity Hub の「Add」ボタンでプロジェクトフォルダを開く

> **Note:** Unity プロジェクトの初期化（Assets/, Packages/, ProjectSettings/ の作成）はまだ完了していません。初回セットアップ時にチームで行います。

## リポジトリ構成

```
GUILD_ACADEMY/
├── CLAUDE.md                  # Claude Code 用プロジェクト指示書
├── SECURITY.md                # セキュリティポリシー
├── game_design_document.md    # ゲームデザインドキュメント（GDD）
└── docs/
    ├── game-design.md         # ゲームデザイン決定事項
    ├── architecture.md        # 技術アーキテクチャ
    ├── branching-endings.md   # 分岐・エンディングシステム
    ├── team-workflow.md       # チーム運用・ワークフロー
    └── presentation-plan.md   # 発表資料構成計画
```

## Git 運用

- `main` / `develop` への直接 push は禁止（GitHub 側で強制）
- 作業は `feature/*` または `fix/*` ブランチで行い、PR 経由で `develop` にマージ
- push 前にテストを実行すること

詳細は [CLAUDE.md](./CLAUDE.md) と [docs/team-workflow.md](./docs/team-workflow.md) を参照。

## ドキュメント

詳細は [docs/](./docs/README.md) を参照。
