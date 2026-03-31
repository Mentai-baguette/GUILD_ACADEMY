# 環境構築ガイド

開発環境は **macOS / Windows** 両対応です。

## 1. 必要なツール

| ツール | バージョン | 用途 |
|--------|----------|------|
| Unity Hub | 最新版 | Unity のバージョン管理・プロジェクト管理 |
| Unity | 6.4 (6000.1.x) | ゲームエンジン |
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

> **Note:** Unity プロジェクトは初期化済みです（Unity 6.4 + Universal 2D テンプレート）。clone 後に Unity Hub でプロジェクトフォルダを開けばそのまま使えます。

## 5. マップタイルアセットのダウンロード

マップタイルはライセンスの都合上リポジトリに含まれていません。各自ダウンロードして配置してください。

1. 以下からアセットをダウンロード:
   - [Legacy Fantasy Bundle - High Forest 2.0](https://anokolisa.itch.io/sidescroller-pixelart-sprites-asset-pack-forest-16x16)
   - [Pixel Crawler - Free Pack](https://anokolisa.itch.io/free-pixel-art-asset-pack-topdown-tileset-rpg-16x16-sprites)
2. ダウンロードしたファイルを `Assets/Project/Data/MapTiles/` の各フォルダに配置
3. 配置先のフォルダ構成は `Assets/Project/Data/MapTiles/README.md` を参照

> **Note:** `.png` / `.aseprite` / `.gif` ファイルは `.gitignore` で除外されています。コミットしないでください。

## 6. ビルドターゲットの確認

Unity でプロジェクトを開いたら File > Build Settings で自分のOSが選択されていることを確認。

## テスト実行

```bash
# macOS
/Applications/Unity/Hub/Editor/6000.1.*/Unity.app/Contents/MacOS/Unity \
  -runTests -testPlatform EditMode -projectPath . -testResults results.xml
```

```powershell
# Windows (PowerShell)
& "C:\Program Files\Unity\Hub\Editor\6000.1.*\Editor\Unity.exe" `
  -runTests -testPlatform EditMode -projectPath . -testResults results.xml
```

## Git 運用

- `main` / `develop` への直接 push は禁止（GitHub 側で強制）
- 作業は `feature/*` または `fix/*` ブランチで行い、PR 経由で `develop` にマージ
- push 前にテストを実行すること

詳細は [team-workflow.md](./team-workflow.md) を参照。
