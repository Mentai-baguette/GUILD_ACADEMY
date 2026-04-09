# AGENTS.md — AIエージェント共通プロジェクト指示書

## プロジェクト概要

**GUILD ACADEMY: ABYSS CODE** — 2D学園ダークファンタジーRPG（マルチエンディング）
Unity 6.4 + C#

- 2年制学園4章構成、57週
- 全7エンディング
- 13プレイアブルキャラ + ボス4体 + 超裏ボス1体

## リポジトリ構造

```
GUILD_ACADEMY/
├── AGENTS.md                  # 本ファイル（AIエージェント共通指示）
├── CLAUDE.md                  # Claude Code 固有の指示
├── README.md                  # ゲーム概要
├── SECURITY.md                # セキュリティポリシー
├── game_design_document.md    # GDD（設計の正とする）
├── Assets/
│   ├── _Project/Scripts/
│   │   ├── Core/                    # Pure C#（MonoBehaviour依存なし）
│   │   │   ├── Battle/              # ATBSystem, DamageCalculator, BreakSystem,
│   │   │   │                        # ActionExecutor, ActionResult, BattleFlowController, IRandom
│   │   │   ├── Branch/              # FlagSystem, TrustSystem, EndingResolver, EndingContext
│   │   │   ├── Character/           # SoulLinkSystem（ErosionSystem.cs は .meta のみ、未実装）
│   │   │   ├── Dialogue/            # DialogueParser, DialogueData, DialogueRunner
│   │   │   ├── Data/                # CharacterStats, BattleCommand, BattlePhase, CharacterId,
│   │   │   │                        # ElementType, EndingType, SceneNames, SceneTransitionData, SkillData
│   │   │   ├── Inventory/           # （空ディレクトリ）
│   │   │   ├── Save/                # SaveData, ISaveSerializer, SaveSerializer
│   │   │   └── Core.asmdef
│   │   ├── MonoBehaviours/
│   │   │   ├── Battle/              # （空）
│   │   │   ├── Camera/              # （空）
│   │   │   ├── Field/               # PlayerController
│   │   │   ├── Save/                # UnitySaveSerializer
│   │   │   ├── UI/                  # SceneTransitionManager
│   │   │   └── MonoBehaviours.asmdef
│   │   └── ScriptableObjects/       # CharacterDataSO, SkillDataSO, EnemyDataSO
│   └── Tests/EditMode/              # 12テストファイル（176テスト通過）
│       ├── Battle/                  # ATBSystem, DamageCalculator, BreakSystem,
│       │                            # ActionExecutor, BattleFlowController
│       ├── Branch/                  # EndingResolver, FlagSystem, TrustSystem
│       ├── Character/               # SoulLinkSystem
│       ├── Dialogue/                # DialogueParser, DialogueRunner
│       ├── Save/                    # SaveData
│       └── TestHelpers/             # FixedRandom
├── Packages/                  # Unity パッケージ定義
├── ProjectSettings/           # Unity プロジェクト設定
└── docs/
    ├── characters/            # 14ファイル（キャラバックストーリー、13,373行）
    ├── specs/                 # 13ファイル（ゲームシステム仕様）
    ├── story/                 # 6ファイル（ストーリーアウトライン）
    ├── dungeons/              # 9ファイル（ダンジョン仕様）
    ├── enemies/               # 敵134種リスト
    ├── maps/                  # エリア構成
    ├── audio/                 # BGM36曲+SE20種
    ├── ui/                    # 画面遷移
    ├── achievements.md        # 実績38個
    ├── architecture.md        # 技術アーキテクチャ
    ├── game-design.md         # ゲームデザイン決定事項
    ├── branching-endings.md   # 分岐・エンディングシステム
    ├── team-workflow.md       # チーム運用・ワークフロー
    ├── presentation-plan.md   # 発表資料構成計画
    ├── setup-guide.md         # 環境構築ガイド
    └── README.md              # ドキュメントインデックス
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

### 仕様書の読み方

| 目的 | 参照先 |
|------|--------|
| ゲーム全体の設計 | `game_design_document.md`（GDD） |
| 個別システムの詳細 | `docs/specs/*.md` |
| ストーリーの流れ | `docs/story/*.md` |
| キャラ設定 | `docs/characters/*.md` |
| ダンジョン詳細 | `docs/dungeons/*.md` |
| 敵データ | `docs/enemies/enemy-list.md` |
| UI画面遷移 | `docs/ui/screen-flow.md` |

### レビュー時の確認事項

- SECURITY.md のルールに違反していないか（秘密情報、内部URL、個人名）
- Git運用ルール通りのブランチ・PR・コミットメッセージか
- 既存ドキュメント間で矛盾が生じていないか

## Git ルール（最重要）

### ブランチ戦略

- `main`: リリース用
- `develop`: 開発統合
- `feature/*`: 機能追加
- `fix/*`: バグ修正
- `docs/*`: ドキュメント

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

### アーキテクチャ方針

- **Pure C# 分離**: `Core/` 配下は MonoBehaviour を継承しない。Unity API に依存しない
- **データ駆動**: キャラ・敵・スキル・アイテムは ScriptableObject で管理
- **イベント駆動**: クラス間通信は C# event / delegate を使用。直接参照を避ける
- **Interface ベース**: テスト時にモック差し替え可能な設計

## テスト

### 方針

- **EditMode テスト（Pure C#）のみ**。PlayModeテストなし
- テストファイル: `Assets/Tests/EditMode/`
- 現在 **176テスト通過**

### テスト対象クラス

- `ATBSystem` — ATB ゲージ計算、ターン順管理
- `DamageCalculator` — ダメージ計算、属性相性
- `BreakSystem` — ブレイクゲージ、ブレイク状態管理
- `SoulLinkSystem` — 絆レベルの増減、効果計算
- `FlagSystem` — 情報フラグの ON/OFF、カウント
- `TrustSystem` — 信頼ポイントの増減
- `EndingResolver` — エンディングの条件判定
- `ActionExecutor` — アクション実行
- `BattleFlowController` — バトルフロー制御
- `DialogueParser` — 会話データのパース
- `DialogueRunner` — 会話の進行制御
- `SaveData` — セーブデータの整合性

### テスト実行コマンド

```bash
# macOS
/Applications/Unity/Hub/Editor/6000.4.*/Unity.app/Contents/MacOS/Unity \
  -runTests -testPlatform EditMode -projectPath . -testResults results.xml

# Windows (PowerShell)
& "C:\Program Files\Unity\Hub\Editor\6000.4.*\Editor\Unity.exe" `
  -runTests -testPlatform EditMode -projectPath . -testResults results.xml

# GitHub Actions では game-ci/unity-test-runner を使用
```

## コード改修が必要な項目

| # | 対象 | 内容 | 状態 |
|---|------|------|------|
| 1 | `CharacterStats.cs` | HP/MP/ATK/DEF/SPD → +INT/RES/AGI/DEX/LUK 追加 | 未着手 |
| 2 | `DamageCalculator.cs` | INT vs RES の魔法ダメージ追加、クリティカルを DEX 依存に | 未着手 |
| 3 | `EndingResolver` | 6END → 7END（END4.5 追加） | 未着手 |
| 4 | `SoulLinkSystem` | 3人 → 10人対応 | 未着手 |
| 5 | `chapter1_dialogue.json` | 現在の設計に合わせて書き直し | PR#36で進行中 |

## ゲームシステム概要

詳細は `game_design_document.md`（GDD）を正とする。以下は要約。

### 7 マルチエンディング

| END | 名称 | 概要 |
|-----|------|------|
| END1 | 裏ハッピー | 学園に行かない選択 |
| END2 | トゥルー | シオン第1形態勝利→レイ死亡（意識残る） |
| END3 | シオンルート | シオン第2形態勝利→罪悪感で崩壊 |
| END4 | ノーマル/メリバ | シオン第2形態勝利だが救出条件未達→カルロスがシオン殺害 |
| END4.5 | （新規追加予定） | 詳細は GDD 参照 |
| END5 | 表ハッピー | 情報8/8 + 全員信頼80+ + シオン救出 + カルロス撃破（全員生存の唯一の道） |
| END6 | 全滅 | シオン第2形態戦で全員HP0 / 選択ミス蓄積 |

### バトルシステム

ATB（Active Time Battle）方式のコマンドバトル。ブレイクシステム・侵蝕システム・ソウルリンクシステムが連動。

- ATBゲージ: MAX 100、蓄積速度 = AGI x 0.1 / フレーム
- 物理ダメージ: `(ATK x 2 - DEF) x スキル倍率 x 属性相性 x クリティカル x ブレイク x 乱数`
- 魔法ダメージ: `(INT x 2 - RES) x スキル倍率 x 属性相性 x クリティカル x ブレイク x 乱数`

### マダミス分岐

- 情報フラグ 8 種（FlagSystem）
- 信頼ポイント 4 人分（TrustSystem）
- BranchManager が統合管理 → EndingResolver で判定

## AI 利用ルール

- **Unity MCP はローカル開発環境のみ**で使用する。CI/共有マシンでは使用しない
- MCP 経由でのファイル操作は **Assets/ 配下のみ** に限定する
- **秘密情報（API キー、パスワード、個人情報）を含むログやファイルを AI に送信しない**
- AI が生成・変更したコードは必ず差分を確認してからコミットする
- AI ツールのローカル設定（`.claude/settings.local.json` 等）はコミットしない
