# CLAUDE.md — Claude Code 用プロジェクト指示書

## プロジェクト概要

GUILD ACADEMY: ABYSS CODE — 2D学園ダークファンタジーRPG（Unity 6.4 + C#）
千葉工業大学ハッカソン「システム開発における生成AI活用ワークショップ」出展作品（3/20〜4/10）

## Git ルール（最重要）

### ブランチ保護

- **`main` への直接 push は禁止**
- **`develop` への直接 push は禁止**
- 必ず `feature/*` または `fix/*` ブランチを作成し、**PR 経由でマージ**すること

### マージフロー

```
feature/* or fix/*  →  PR  →  develop  →  テスト確認  →  PR  →  main
```

1. 作業は `feature/xxx` or `fix/xxx` ブランチで行う
2. PR を作成して `develop` にマージ
3. `develop` 上でテストを実行し、バグがないことを確認
4. `develop` → `main` への PR を作成する際、**develop 側の PR 番号を PR 本文に記載**する
5. `main` にマージ

### テスト必須

- **push する前に必ずテストを実行すること**
- Edit Mode テストが全て pass していることを確認してから push
- テストなしの push は禁止

## プラットフォーム

- **macOS 対応**（開発・テスト・プレイすべて Mac）
- ビルドターゲット: StandaloneOSX
- CI/CD: GitHub Actions の macOS runner を使用

## コーディング規約

### C# スタイル

- PascalCase: クラス名、メソッド名、プロパティ名、public フィールド
- camelCase: ローカル変数、パラメータ
- _camelCase: private フィールド（アンダースコアプレフィクス）
- UPPER_SNAKE_CASE: 定数
- 名前空間: `GuildAcademy.Core.Battle`, `GuildAcademy.UI`, `GuildAcademy.Data` 等

### アーキテクチャ

- **Pure C# 分離**: `Core/` 配下は MonoBehaviour を継承しない。Unity API に依存しない
- **データ駆動**: キャラ・敵・スキル・アイテムは ScriptableObject で管理
- **イベント駆動**: クラス間通信は C# event / delegate を使用。直接参照を避ける
- **Interface ベース**: テスト時にモック差し替え可能な設計

### ファイル配置

```
Assets/_Project/Scripts/
├── Core/           # 純粋C#ロジック（テスト対象）
├── MonoBehaviours/ # Unity依存スクリプト
└── ScriptableObjects/ # ScriptableObject定義

Assets/Tests/
├── EditMode/       # Edit Modeテスト（NUnit）
└── PlayMode/       # Play Modeテスト
```

## テスト

### Edit Mode テスト（必須）

テスト対象の Pure C# クラス:
- `ATBSystem` — ATB ゲージ計算、ターン順管理
- `DamageCalculator` — ダメージ計算、属性相性
- `BreakSystem` — ブレイクゲージ、ブレイク状態管理
- `ErosionSystem` — 侵蝕値の増減、閾値判定
- `SoulLinkSystem` — 絆レベルの増減、効果計算
- `FlagSystem` — 情報フラグの ON/OFF、カウント
- `TrustSystem` — 信頼ポイントの増減
- `EndingResolver` — 6 エンディングの条件判定

### テスト実行コマンド

```bash
# Unity Test Runner (CLI)
# macOS の Unity Hub インストールでは以下のパスを使用（バージョンは適宜変更）
/Applications/Unity/Hub/Editor/6000.1.*/Unity.app/Contents/MacOS/Unity \
  -runTests -testPlatform EditMode -projectPath . -testResults results.xml

# GitHub Actions では game-ci/unity-test-runner を使用
```

## 重要なゲームシステム

### 6 マルチエンディング

| END | 名称 | 概要 |
|-----|------|------|
| END1 | 裏ハッピー | 学園に行かない選択 |
| END2 | トゥルー | シオン第1形態勝利→レイ死亡（意識残る） |
| END3 | シオンルート | シオン第2形態勝利→罪悪感で崩壊 |
| END4 | ノーマル/メリバ | シオン第2形態勝利だが救出条件未達→カルロスがシオン殺害 |
| END5 | 表ハッピー | 全員信頼MAX + シオン救出 + カルロス撃破（全員生存の唯一の道） |
| END6 | 全滅 | シオン第2形態戦で全員HP0 / 選択ミス蓄積 |

### マダミス分岐

- 情報フラグ 8 種（FlagSystem）
- 信頼ポイント 4 人分（TrustSystem）
- BranchManager が統合管理 → EndingResolver で判定

## ドキュメント

- `docs/game-design.md` — ゲームデザイン決定事項
- `docs/architecture.md` — 技術アーキテクチャ
- `docs/branching-endings.md` — 分岐・エンディングシステム詳細
- `docs/team-workflow.md` — チーム運用・Git運用
- `docs/presentation-plan.md` — 発表資料の構成計画

## 外部リンク

Notion のプロジェクトページ・タスクボード・学習リソースはチーム内で共有。
URLは公開リポジトリには載せない（SECURITY.md 参照）。

## AI / MCP 利用ルール

- **Unity MCP はローカル開発環境のみ**で使用する。CI/共有マシンでは使用しない
- MCP経由でのファイル操作は **Assets/ 配下のみ** に限定する
- **秘密情報（API キー、パスワード、個人情報）を含むログやファイルを AI に送信しない**
- AI が生成・変更したコードは必ず差分を確認してからコミットする
- AI ツールのローカル設定（`.claude/settings.local.json` 等）はコミットしない
