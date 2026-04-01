# 環境構築ガイド

開発環境は **macOS / Windows** 両対応です。

## 1. 必要なツール

| ツール | バージョン | 用途 |
|--------|----------|------|
| Unity Hub | 最新版 | Unity のバージョン管理・プロジェクト管理 |
| Unity | 6.4 (6000.4.x) | ゲームエンジン |
| Git | 最新版 | バージョン管理 |
| GitHub Desktop | （任意） | Git の GUI クライアント |

## 2. セットアップ手順

### Unity Hub のインストール

[https://unity.com/download](https://unity.com/download) から自分のOS版をダウンロード・インストール。

### Unity 6.4 のインストール

Unity Hub から Unity 6.4 をインストール。モジュールは以下を含める：

- **macOS Build Support**（macOSで開発する場合）
- **Windows Build Support (IL2CPP)**（Windowsで開発する場合）

### Git のインストール

ターミナルで `git --version` を実行して入っているか確認。入っていなければ：

- **macOS**: `xcode-select --install` または [Homebrew](https://brew.sh/) で `brew install git`
- **Windows**: [https://git-scm.com/downloads/win](https://git-scm.com/downloads/win) からインストール

### GitHub Desktop（任意）

コマンドが苦手な人は GUI で操作できる: [https://desktop.github.com/](https://desktop.github.com/)

## 3. リポジトリのクローン

```bash
git clone https://github.com/Mentai-baguette/GUILD_ACADEMY.git
cd GUILD_ACADEMY
git checkout develop
```

## 4. Unity プロジェクトを開く

Unity Hub の「Add」ボタンでプロジェクトフォルダを選択して開く。

> **Note:** Unity プロジェクトは初期化済みです（Unity 6.4 + Universal 2D テンプレート）。clone 後にマップタイルアセット（下記手順5）を配置してから Unity Hub でプロジェクトを開いてください。

## 5. マップタイルアセットのダウンロード

マップタイルは再配布禁止のため、リポジトリに含まれていません（実体・.meta とも除外）。
各自ダウンロードし、以下の手順でフォルダを作成・配置してください。

### 5.1 アセットの入手先

| マップ名 | アセットパック | 入手先 |
|----------|---------------|--------|
| Academy | Pixel Crawler - FREE | [itch.io](https://anokolisa.itch.io/free-pixel-art-asset-pack-topdown-tileset-rpg-16x16-sprites) |
| Emerald | Legacy Fantasy Bundle - High Forest 2.0 | [itch.io](https://anokolisa.itch.io/sidescroller-pixelart-sprites-asset-pack-forest-16x16) |
| Fuuin_no_ma | Pixel Crawler - FREE（Esoteric のみ） | 上記と同じ |
| kiniki | Pixel Crawler - FREE（Dungeon 系のみ） | 上記と同じ |

### 5.2 フォルダ構成

ダウンロードしたアセットを以下のディレクトリ構成で `Assets/Project/Data/MapTiles/` に配置してください。
Unity がプロジェクトを開いたときに `.meta` ファイルを自動生成します。

```
Assets/Project/Data/MapTiles/
├── Academy/
│   ├── Ground/
│   │   ├── Floors.png
│   │   └── Floors_Tiles.png
│   ├── Props/
│   │   ├── Alchemy/
│   │   │   └── Alchemy/
│   │   │       ├── Alchemy_Table_01-Sheet.png
│   │   │       ├── Alchemy_Table_02-Sheet.png
│   │   │       └── Alchemy_Table_03-Sheet.png
│   │   ├── Anvil/
│   │   │   └── Anvil/
│   │   │       ├── Anvil.png
│   │   │       ├── Anvil_01-Sheet.png
│   │   │       ├── Anvil_02-Sheet.png
│   │   │       └── Anvil_03-Sheet.png
│   │   ├── Bonfire/
│   │   │   └── Bonfire/
│   │   │       ├── Bonfire.png
│   │   │       ├── Bonfire_01-Sheet.png 〜 Bonfire_10-Sheet.png
│   │   │       ├── Fire_01-Sheet.png, Fire_02-Sheet.png
│   │   │       └── Smoke-Sheet.png
│   │   ├── CookingStation/
│   │   │   └── CookingStation/
│   │   │       ├── Cooking Station.png
│   │   │       ├── Estructure.png
│   │   │       ├── Butchery/
│   │   │       │   ├── Butchery_01-Sheet.png
│   │   │       │   └── Butchery_02.png 〜 Butchery_04.png
│   │   │       ├── Cooker/
│   │   │       │   ├── Cooker_01.png, Cooker_02.png
│   │   │       │   └── Cooker_03-Sheet.png, Cooker_04-Sheet.png
│   │   │       └── Grill/
│   │   │           └── Grill_01-Sheet.png 〜 Grill_04-Sheet.png
│   │   ├── Furnace/
│   │   │   └── Furnace/
│   │   │       ├── Furnace.png
│   │   │       ├── Bricks_01-Sheet.png 〜 Bricks_03-Sheet.png
│   │   │       ├── Iron_01-Sheet.png 〜 Iron_03-Sheet.png
│   │   │       └── Stone_01-Sheet.png 〜 Stone_03-Sheet.png
│   │   ├── Sawmill/
│   │   │   └── Sawmill/
│   │   │       ├── Base.png, Level_1.png
│   │   │       ├── Level_2-Sheet.png
│   │   │       └── Level_3-Sheet.png
│   │   ├── Trees/
│   │   │   └── Trees/
│   │   │       ├── Model_01/ (Size_02.png 〜 Size_05.png)
│   │   │       ├── Model_02/ (Size_02.png 〜 Size_05.png)
│   │   │       └── Model_03/ (Size_02.png 〜 Size_05.png, Size_03-export.png, Size_04-export.png, Size_04-export-export.png)
│   │   └── Workbench/
│   │       └── Workbench/
│   │           └── Workbench.png
│   ├── Roof/
│   │   └── Roofs.png
│   ├── Shadow/
│   │   └── Shadows.png
│   └── Wall/
│       ├── Wall_Tiles.png
│       ├── Wall_Variations.png
│       └── Walls.png
├── Emerald/
│   ├── BackGround/
│   │   ├── Background.png, Background_2.png
│   │   ├── Dark-Tree.png, Golden-Tree.png, Green-Tree.png
│   │   ├── Red-Tree.png, Yellow-Tree.png
│   │   └── Tree-Assets.png
│   ├── Ground/
│   │   ├── Hive.png
│   │   └── Tiles.png
│   ├── Props/
│   │   ├── Props-Rocks.png
│   │   ├── Rocks.png
│   │   └── Vegetation.png
│   └── Water/
│       └── Water_tiles.png
├── Fuuin_no_ma/
│   └── Props/
│       └── Esoteric.png
└── kiniki/
    ├── Ground/
    │   └── Dungeon_Tiles.png
    └── Props/
        └── Dungeon_Props.png
```

### 5.3 Unity インポート設定

アセットを配置して Unity でプロジェクトを開いたあと、各 `.png` ファイルの Inspector で以下を設定してください。

| 項目 | 値 | 備考 |
|------|-----|------|
| Texture Type | Sprite (2D and UI) | |
| Sprite Mode | Multiple | スプライトシートを自動スライスする |
| Pixels Per Unit (PPU) | **16** | |
| Filter Mode | **Point (no filter)** | ピクセルアートなのでぼかし無し |
| Compression | **None** | 劣化防止 |
| MipMap | 無効 | |
| Max Size | 2048 | |

### 5.4 スプライトスライス手順

1. Unity で `.png` ファイルを選択し、Inspector の **Sprite Editor** を開く
2. **Slice** → Type: **Automatic** でスライスを実行
3. 各スプライトの名前が `ファイル名_0`, `ファイル名_1`, ... となることを確認
4. **Apply** をクリック

> **Note:** `.png` / `.aseprite` / `.gif` ファイルは `.gitignore` で除外されています。コミットしないでください。

## 6. ビルドターゲットの確認

Unity でプロジェクトを開いたら File > Build Settings で自分のOSが選択されていることを確認。

## テスト実行

```bash
# macOS
/Applications/Unity/Hub/Editor/6000.4.*/Unity.app/Contents/MacOS/Unity \
  -runTests -testPlatform EditMode -projectPath . -testResults results.xml
```

```powershell
# Windows (PowerShell)
& "C:\Program Files\Unity\Hub\Editor\6000.4.*\Editor\Unity.exe" `
  -runTests -testPlatform EditMode -projectPath . -testResults results.xml
```

## Git 運用

- `main` / `develop` への直接 push は禁止（GitHub 側で強制）
- 作業は `feature/*` または `fix/*` ブランチで行い、PR 経由で `develop` にマージ
- push 前にテストを実行すること

詳細は [team-workflow.md](./team-workflow.md) を参照。
