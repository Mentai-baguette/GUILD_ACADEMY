# GUILD ACADEMY: ABYSS CODE

> *"お前の中に眠るその闇、俺が引き出してやるよ。"*

**2D学園ダークファンタジーRPG** — 7つのマルチエンディング

## ゲーム概要

記憶を失った少年レイが、冒険者ギルドの養成学校「クロノス・アカデミー」に入学する。クラスメイトとの学園生活、ダンジョン実習、そして徐々に明らかになる自分の正体——彼の中には「魔王の器」としての力が封じられていた。

**「誰を信じ、何を選ぶか」——あなたの選択が7つの結末を決める。**

マーダーミステリー的な情報収集と選択が、取り返しのつかない結末へ導く学園ダークファンタジーRPG。

## 特徴

- **ATBバトル** — ゲージが溜まったら行動可能。ブレイクシステムで大ダメージチャンス
- **侵蝕システム** — 魔王の力を使うほど強くなるが、闇に飲まれるリスクも
- **ソウルリンク** — 13人の仲間との絆がバトルとエンディングに影響
- **マダミス風分岐** — 情報フラグ8種 + 信頼ポイントの組み合わせで物語が分岐
- **7マルチエンディング** — 全員生存の道はたった1つ
- **FF10式メンバー入替** — バトル中に控えメンバーと交代可能
- **57週カレンダー** — 2年制学園4章構成、時間帯に応じたイベント配置

## 技術スタック

| 項目 | 技術 |
|------|------|
| エンジン | Unity 6.4 (6000.1.x) + C# |
| プラットフォーム | macOS / Windows |
| CI/CD | GitHub Actions + game-ci/unity-test-runner |
| タスク管理 | Notion |
| AI活用 | Claude Code Max, Unity MCP, DALL-E, Suno/Udio |

## プロジェクト規模

| 指標 | 数値 |
|------|------|
| ソースファイル | 102個 |
| テストファイル | 40個 |
| テストケース | 600件以上（EditMode） |
| コミット数 | 400+ |
| 背景画像 | 27枚（AI生成） |
| BGM | 36曲（AI生成） |
| 開発期間 | 3週間（3人チーム） |

## アーキテクチャ

```
Assets/_Project/Scripts/
├── Core/                    # Pure C#（MonoBehaviour依存なし）
│   ├── Battle/              # ATBSystem, DamageCalculator, BreakSystem,
│   │                        # ActionExecutor, BattleFlowController, FormationSystem
│   ├── Branch/              # FlagSystem, TrustSystem, EndingResolver
│   ├── Calendar/            # CalendarManager（57週カレンダー）
│   ├── Character/           # SoulLinkSystem
│   ├── Dialogue/            # DialogueParser, DialogueRunner
│   ├── Events/              # EventScheduler, EventDataLoader
│   ├── Party/               # PartyManager（FF10式入替対応）
│   ├── Audio/               # AudioConfig
│   ├── Data/                # CharacterStats, BattleCommand, SkillData
│   ├── Save/                # SaveData, ISaveSerializer
│   └── UI/                  # MenuTabController
├── MonoBehaviours/          # Unity依存のMonoBehaviour
│   ├── Battle/              # BattleManager, BattleUIManager, BattleResultUI
│   ├── Audio/               # AudioManager
│   ├── Events/              # EventSchedulerMB
│   ├── Field/               # PlayerController, NPCController
│   ├── UI/                  # TitleUI, MenuUI, DialogueUI, ChoiceUI
│   └── Save/                # UnitySaveSerializer
└── ScriptableObjects/       # CharacterDataSO, SkillDataSO, EnemyDataSO
```

**設計方針**: Pure C#（Core/）とMonoBehaviour（MonoBehaviours/）を完全分離し、Core層はEditModeテストで100%テスト可能。

## 生成AI活用

本プロジェクトでは開発ライフサイクル全体でAIを活用しました。

| 工程 | AI |用途 |
|------|-----|------|
| 企画・設計 | Claude | GDD（300行超）の共同設計 |
| 実装 | Claude Code Max | Pure C#コード生成、テスト生成、レビュー |
| Unity連携 | Unity MCP | AIがUnity Editorを直接操作（シーン配置、設定変更） |
| 画像 | DALL-E | 背景画像27枚、タイトルロゴ、UIボタン素材 |
| 音楽 | Suno / Udio | BGM36曲（オーケストラ/アコースティック系） |
| タスク管理 | Notion + Claude | 140+タスクのステータス管理・更新 |

## ビルド手順

### 必要環境
- Unity 6.4 (6000.1.x)
- Git

### セットアップ
```bash
git clone https://github.com/Mentai-baguette/GUILD_ACADEMY.git
cd GUILD_ACADEMY
```

Unity Hub で `GUILD_ACADEMY` フォルダを開き、Unity 6.4 で起動してください。

### テスト実行
Unity Editor で `Window > General > Test Runner` を開き、`EditMode` テストを実行。

## チームメンバー

| 担当 | 名前 | 主な作業 |
|------|------|----------|
| リードエンジニア | yoppy | バトルシステム、分岐管理、コアロジック、AI活用 |
| フィールド/UI担当 | 蒸し焼き | マップ、移動、UI |
| アート・ストーリー | まき | AI画像/BGM生成、ストーリーテキスト |

## ドキュメント

- [ゲームデザインドキュメント（GDD）](./game_design_document.md)
- [UI画面遷移仕様書](./docs/ui/screen-flow.md)
- [環境構築ガイド](./docs/setup-guide.md)
- [発表資料構成計画](./docs/presentation-plan.md)

## ライセンス

千葉工業大学ハッカソン「システム開発における生成AI活用ワークショップ」出展作品（2026/3/20〜4/10）

## クレジット

### 音声素材
- 効果音（SE）素材は EVENTSTUDIO で生成した素材を使用
- 詳細は [docs/audio-credits.md](./docs/audio-credits.md) を参照

### マップタイル素材
本作では、以下のマップタイル素材を使用しています。各素材は CC 規約に基づき使用可能であることを確認済みです。

- [RPG Indoor Tileset Expansion 1](https://opengameart.org/content/rpg-indoor-tileset-expansion-1)
- [DawnLike 16x16 Universal Rogue-like Tileset v1.81](https://opengameart.org/content/dawnlike-16x16-universal-rogue-like-tileset-v181)
- [Mage City Arcanos](https://opengameart.org/content/mage-city-arcanos)
- [Kenney Roguelike Indoors](https://kenney.nl/assets/roguelike-indoors)
