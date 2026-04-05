# 技術アーキテクチャ決定事項

## フォルダ構成方針

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/              # Unityに依存しない純粋C#（テスト容易）
│   │   │   ├── Battle/        # ATBSystem, DamageCalculator, BreakSystem
│   │   │   ├── Character/     # Stats, SoulLink, ErosionSystem
│   │   │   ├── Branch/        # FlagSystem, TrustSystem, BranchManager, EndingResolver
│   │   │   ├── Dialogue/      # 会話エンジン
│   │   │   ├── Inventory/     # アイテム、装備
│   │   │   ├── Save/          # SaveLoadSystem
│   │   │   └── Data/          # データモデル定義
│   │   ├── MonoBehaviours/    # Unityに依存するスクリプト
│   │   │   ├── UI/            # 各種UI Controller
│   │   │   ├── Field/         # PlayerController, NPCController
│   │   │   ├── Battle/        # BattleSceneController
│   │   │   └── Camera/
│   │   └── ScriptableObjects/ # ScriptableObject定義
│   ├── Data/                  # ScriptableObjectインスタンス
│   │   ├── Characters/
│   │   ├── Enemies/
│   │   ├── Skills/
│   │   ├── Dialogues/
│   │   └── BranchConditions/  # 分岐条件定義
│   ├── Sprites/
│   ├── Audio/
│   │   ├── BGM/
│   │   └── SE/
│   └── Scenes/
├── Resources/
│   ├── Dialogues/             # 会話データ（JSON）
│   └── StoryFlags/            # フラグ定義データ
└── Tests/
    ├── EditMode/              # NUnit Edit Modeテスト
    └── PlayMode/              # Play Modeテスト
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

## 対応プラットフォーム

- 開発環境: macOS / Windows 両対応
- Unity 6.4 (Apple Silicon / x86_64)
- ビルドターゲット: StandaloneOSX, StandaloneWindows64
- テスト実行: Edit Mode / Play Mode テスト（両OSで実行可能）
- CI/CD: GitHub Actions を使用
- 改行コード: `.gitattributes` で LF に統一（macOS/Windows混在対策）

## AI活用ツール

| 用途 | ツール |
|------|--------|
| 設計・実装・テスト | Claude Code Max + Unity MCP |
| コンセプトアート | Midjourney / DALL-E |
| ドット絵 | Piskel / Aseprite |
| BGM | deevid.ai |
| SE | EVENTSTUDIO |
| タスク管理 | Notion |

### アセット生成記録の扱い

- 音源素材の採用ツールは、GDDに記載された計画値と実際の採用値が異なる場合がある
- 実際に採用したツール名、利用条件、例外承認の有無は [docs/audio-credits.md](./audio-credits.md) と [docs/audio-validation.md](./audio-validation.md) に記録する
- GDDとの差分がある場合は、PRごとの例外承認として追跡し、設計意図と実装実績を分離して管理する
