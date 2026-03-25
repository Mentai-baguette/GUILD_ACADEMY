# AGENTS.md — AIエージェント共通プロジェクト指示書

## プロジェクト概要

GUILD ACADEMY: ABYSS CODE — 2D学園ダークファンタジーRPG（Unity 6.4 + C#）
千葉工業大学ハッカソン「システム開発における生成AI活用ワークショップ」出展作品（3/20〜4/10）

## 現在のリポジトリ状態

**Unity プロジェクトは初期化済み。** Unity 6.4 + Universal 2D (URP) テンプレート。
操作前に `ls` 等で実体を確認すること。

```
GUILD_ACADEMY/
├── AGENTS.md                  # 本ファイル（AIエージェント共通指示）
├── CLAUDE.md                  # Claude Code 固有の指示
├── README.md                  # ゲーム概要
├── SECURITY.md                # セキュリティポリシー
├── game_design_document.md    # GDD（設計の正とする）
├── Assets/                    # Unity アセット
├── Packages/                  # Unity パッケージ定義
├── ProjectSettings/           # Unity プロジェクト設定
└── docs/
    ├── README.md              # ドキュメントインデックス
    ├── game-design.md         # ゲームデザイン決定事項
    ├── architecture.md        # 技術アーキテクチャ
    ├── branching-endings.md   # 分岐・エンディングシステム
    ├── team-workflow.md       # チーム運用・ワークフロー
    └── presentation-plan.md   # 発表資料構成計画
```

## エージェント運用ルール

### 基本原則

- **存在しないファイルやディレクトリを前提にしない。** 操作前に必ず実体を確認する
- コード生成・変更時は必ず差分を提示し、確認を取ってからコミットする
- 秘密情報（APIキー、パスワード、Notion URL、個人情報）をコミットしない

### ドキュメント変更時の整合性ルール

設計に関する正（Single Source of Truth）は **`game_design_document.md`（GDD）**。
以下のドキュメントはGDDの要約・派生であり、矛盾する場合はGDDに合わせる：

- `docs/game-design.md`
- `docs/branching-endings.md`
- `AGENTS.md` 内のゲームシステム記述

ドキュメントを変更したら、関連する他ドキュメントとの整合を確認すること。

### レビュー時の確認事項

- SECURITY.md のルールに違反していないか（秘密情報、内部URL、個人名）
- Git運用ルール通りのブランチ・PR・コミットメッセージか
- 既存ドキュメント間で矛盾が生じていないか

## Git ルール（最重要）

### ブランチ保護

- **`main` への直接 push は禁止**
- **`develop` への直接 push は禁止**
- 必ず `feature/*`、`fix/*`、`docs/*` ブランチを作成し、**PR 経由でマージ**すること

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

- **push する前に必ずテストを実行すること**（テスト環境が整っている場合）
- Edit Mode テストが全て pass していることを確認してから push
- テスト環境が未構築の場合は、その旨をPRに明記する

## プラットフォーム

- **macOS 主体**（開発・テスト・プレイの主環境）
- Windows でも開発可能な構成にする（`.gitattributes` で改行コード統一）
- ビルドターゲット: StandaloneOSX（主）、StandaloneWindows64（副）
- CI/CD: GitHub Actions を使用

## コーディング規約

### C# スタイル

- PascalCase: クラス名、メソッド名、プロパティ名、public フィールド
- camelCase: ローカル変数、パラメータ
- _camelCase: private フィールド（アンダースコアプレフィクス）
- UPPER_SNAKE_CASE: 定数
- 名前空間: `GuildAcademy.Core.Battle`, `GuildAcademy.UI`, `GuildAcademy.Data` 等

### アーキテクチャ方針（Unity プロジェクト構築時に適用）

- **Pure C# 分離**: `Core/` 配下は MonoBehaviour を継承しない。Unity API に依存しない
- **データ駆動**: キャラ・敵・スキル・アイテムは ScriptableObject で管理
- **イベント駆動**: クラス間通信は C# event / delegate を使用。直接参照を避ける
- **Interface ベース**: テスト時にモック差し替え可能な設計

### 計画中のファイル配置（Unity 初期化後に適用）

```
Assets/_Project/Scripts/
├── Core/           # 純粋C#ロジック（テスト対象）
├── MonoBehaviours/ # Unity依存スクリプト
└── ScriptableObjects/ # ScriptableObject定義

Assets/Tests/
├── EditMode/       # Edit Modeテスト（NUnit）
└── PlayMode/       # Play Modeテスト
```

## テスト（Unity プロジェクト構築後に有効）

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
# macOS
/Applications/Unity/Hub/Editor/6000.1.*/Unity.app/Contents/MacOS/Unity \
  -runTests -testPlatform EditMode -projectPath . -testResults results.xml

# Windows (PowerShell)
& "C:\Program Files\Unity\Hub\Editor\6000.1.*\Editor\Unity.exe" `
  -runTests -testPlatform EditMode -projectPath . -testResults results.xml

# GitHub Actions では game-ci/unity-test-runner を使用
```

## ゲームシステム概要

詳細は `game_design_document.md`（GDD）を正とする。以下は要約。

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
