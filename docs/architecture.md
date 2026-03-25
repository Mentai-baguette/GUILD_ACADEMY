# 技術アーキテクチャ決定事項

## フォルダ構成方針

```
Assets/
├── Scripts/
│   ├── Core/              # Unityに依存しない純粋C#（テスト容易）
│   │   ├── Battle/        # ATBSystem, DamageCalculator, BreakSystem
│   │   ├── Branch/        # FlagSystem, TrustSystem, BranchManager, EndingResolver
│   │   ├── Erosion/       # ErosionSystem
│   │   ├── SoulLink/      # SoulLinkSystem
│   │   ├── Save/          # SaveLoadSystem
│   │   └── Item/          # Inventory
│   ├── MonoBehaviours/    # Unityに依存するスクリプト
│   │   ├── UI/            # 各種UI Controller
│   │   ├── Field/         # PlayerController, NPCController
│   │   ├── Battle/        # BattleSceneController
│   │   ├── Audio/         # AudioManager
│   │   └── Scene/         # SceneTransitionManager
│   └── Data/              # ScriptableObject定義
│       ├── Characters/    # CharacterData
│       ├── Enemies/       # EnemyData
│       ├── Skills/        # SkillData
│       └── Items/         # ItemData
├── Resources/
│   ├── Dialogues/         # 会話データ（JSON）
│   └── StoryFlags/        # フラグ定義データ
├── Scenes/
│   ├── TitleScene
│   ├── FieldScene
│   ├── BattleScene
│   └── EndingScene
└── Tests/
    ├── EditMode/          # NUnit Edit Modeテスト
    └── PlayMode/          # Play Modeテスト
```

## 設計原則

### Pure C# 分離
- `Core/` 配下のクラスは MonoBehaviour を継承しない
- Unity の API に依存しないため、Edit Mode テストで高速にテスト可能
- Interface ベースで依存注入（テスト時にモックに差し替え可能）

### データ駆動設計
- キャラ・敵・スキル・アイテムのパラメータは **ScriptableObject** で管理
- 会話テキスト・フラグ定義は **JSON** で管理
- コードを変更せずにバランス調整が可能

### データ改ざん耐性
- ローカルのセーブデータ・JSON は**改ざんされる前提**で設計する
- 将来ランキング・実績・オンライン要素を追加する場合、クライアント保存値を信頼しない
- セーブデータの検証（チェックサム等）は実装フェーズで検討する

### イベント駆動通信
- クラス同士は直接参照せず、**C# event / delegate** で通信
- 疎結合を維持し、テスト・拡張が容易

### テスト戦略
- **Edit Mode テスト**: ATBSystem, DamageCalculator, FlagSystem, TrustSystem, EndingResolver
- **Play Mode テスト**: シーン遷移、UI操作の統合テスト
- **手動プレイテスト**: マップ移動、バトル流れ、会話・選択肢、全エンド到達確認
- **CI/CD**: GitHub Actions + game-ci/unity-test-runner で自動テスト
  - Actions は **commit SHA pinning** でバージョンを固定する（タグ指定は避ける）
  - workflow の permissions は **最小権限**（`contents: read` 等）に設定する
  - Secrets は environment で分離し、workflow から直接参照しない

## Mac 対応

- 開発環境: macOS + Unity 6 (Apple Silicon 対応)
- ビルドターゲット: macOS (StandaloneOSX)
- テスト実行: Mac上でのEdit Mode / Play Mode テスト
- CI/CD: GitHub Actions の macOS runner を使用

## AI活用ツール

| 用途 | ツール |
|------|--------|
| 設計・実装・テスト | Claude Code Max + Unity MCP |
| コンセプトアート | Midjourney / DALL-E |
| ドット絵 | Piskel / Aseprite |
| BGM | Suno / Udio |
| SE | ElevenLabs |
| タスク管理 | Notion |
