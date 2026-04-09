# 学園9エリア実装 明日用ランブック

このメモは、`GuildAcademy/Maps/Bootstrap Academy 9 Areas` を実行したあとに、9エリアのTilemap・遷移・コリジョンをすんなり仕上げるための手順書です。

## 現在ステータス（2026-04-09時点）

- Bootstrap実行は成功済み。
- 全9シーンで `GroundCenterTile=True`。
- 全9シーンで `CollisionBoundsColliders=4`。
- 次の主作業は「シーン別の微調整と遷移確認」。

## タブ並行作業の開き方

同時に以下4タブを開いて作業すると、往復が少なくなる。

1. このファイル（作業手順）
2. `docs/maps/academy-9areas-implementation-checklist.md`（進捗更新）
3. 対象シーン（例: `Assets/Scenes/Academy/Academy_Hallway.unity`）
4. `Assets/_Project/Scripts/MonoBehaviours/Field/ScenePortal2D.cs`（遷移先ID確認）

## シーン別クイックチェック（作業時に上から実行）

### 1. Academy_Hallway（最優先）

1. 各ポータルの位置を扉中央へ合わせる。
2. 各ポータルの BoxCollider2D が重なっていないか確認する。
3. Classroom / Cafeteria / Library / Infirmary / StudentCouncilRoom / Rooftop / Schoolyard への遷移先IDを確認する。
4. 廊下中央を走って誤遷移しないことを確認する。

### 2. Academy_Classroom

1. Hallway向けポータル位置を教室出入口に合わせる。
2. 入場地点 `from_hallway` で壁埋まりしないか確認する。
3. NPCSpotsを机配置に合わせて再配置する。

### 3. Academy_Cafeteria

1. Hallway向けポータルを出入口に合わせる。
2. テーブル間の通路幅を確認し、引っかかりがあればCollisionを削る。
3. EventTriggersを会話ポイントに寄せる。

### 4. Academy_Library

1. Hallway向けポータルを入口タイルに合わせる。
2. 書架間通路のコリジョンを確認する。
3. EventTriggersを書架前や調査ポイントに配置する。

### 5. Academy_Infirmary

1. Hallway向けポータル位置を入口へ合わせる。
2. ベッド列の横移動で引っかからないか確認する。
3. NPCSpotsをベッド脇・受付位置へ寄せる。

### 6. Academy_StudentCouncilRoom

1. Hallway向けポータル位置を入口へ合わせる。
2. 中央机周りの回り込み導線を確認する。
3. EventTriggersを会議机近辺へ配置する。

### 7. Academy_Rooftop

1. Hallway向けポータル位置を階段出口に合わせる。
2. 外周で落下しないよう境界コリジョンを再確認する。
3. 屋上中央の会話ポイントを1つ確保する。

### 8. Academy_Schoolyard

1. Hallway向けとSchoolGate向けの2ポータルを分離配置する。
2. 校庭中央を走って誤遷移しないか確認する。
3. 屋外用のEventTriggersを中央と端に配置する。

### 9. Academy_SchoolGate

1. Schoolyard向けポータル位置を校門内側へ合わせる。
2. 門柱周りのコリジョンを確認する。
3. 校門前のイベントポイントを1つ確保する。

## 1シーンあたりの作業テンプレ（コピペ用）

- [ ] ポータル位置
- [ ] ポータル当たり判定サイズ
- [ ] SpawnPoint着地点
- [ ] Collision角の引っかかり
- [ ] NPCSpots位置
- [ ] EventTriggers位置
- [ ] 往復遷移テスト

## 連続遷移の最終確認ルート

1. 教室 -> 廊下
2. 廊下 -> 校庭
3. 校庭 -> 校門
4. 校門 -> 校庭
5. 校庭 -> 廊下
6. 廊下 -> 図書館
7. 図書館 -> 廊下
8. 廊下 -> 教室

## まず前提として見るもの

- 対象チェックリスト: [academy-9areas-implementation-checklist.md](./academy-9areas-implementation-checklist.md)
- エリア仕様: [area-list.md](./area-list.md)
- 遷移基盤: [ScenePortal2D.cs](../../Assets/_Project/Scripts/MonoBehaviours/Field/ScenePortal2D.cs)
- スポーン基盤: [SceneSpawnPoint.cs](../../Assets/_Project/Scripts/MonoBehaviours/Field/SceneSpawnPoint.cs)
- スポーン解決: [SceneSpawnResolver.cs](../../Assets/_Project/Scripts/MonoBehaviours/Field/SceneSpawnResolver.cs)
- イベントトリガー: [AreaEventTrigger2D.cs](../../Assets/_Project/Scripts/MonoBehaviours/Field/AreaEventTrigger2D.cs)
- 9エリア一括配置: [AcademyMapBootstrapper.cs](../../Assets/_Project/Scripts/Editor/AcademyMapBootstrapper.cs)

## 進め方

### 1. Unityを開く前に確認すること

1. プロジェクト直下で `git status --short` を見て、未保存差分を把握する。
2. `Assets/Scenes/Academy/` に9シーンが存在することを確認する。
3. `Assets/_Project/Data/maptile/` 配下に TileBase があることを確認する。
4. もし `No Sprites or Texture are generated...` 系の再発があれば、先に [Unityエラー切り分けガイド](../unity-error-triage.md) を見る。

### 2. Bootstrap を実行する

1. Unity を起動してプロジェクトを開く。
2. メニューから `GuildAcademy/Maps/Bootstrap Academy 9 Areas` を実行する。
3. 失敗したら、まず以下を確認する。
4. `TileBase が見つかりません` が出る場合は、共通タイル素材のImportが済んでいない。
5. `Grid が見つかりません` が出る場合は、シーン雛形が壊れているか、手で構造を変更している。
6. 実行後、各シーンに `NPCSpots` と `EventTriggers` が作られているか確認する。
7. Console に出る `AcademyMapBootstrapper` のサマリログを確認する。
8. `GroundCenterTile=False` が出たシーンは、Tile配置失敗として再確認する。
9. `CollisionBoundsColliders=0` が出たシーンは、境界コライダーが欠損しているので先に復旧する。

### 2.1 青画面だった時の最短切り分け

1. Console に `Selected tile:` が出ているか確認する。
2. 出ていない場合は Bootstrap 未実行か、Editorのコンパイル待ち状態を疑う。
3. `Selected tile:` はあるのに `GroundCenterTile=False` の場合は Tile配置失敗なので再実行する。
4. `GroundCenterTile=True` なのに床が点々表示になる場合は `Floor.png` の PPU が `16` か確認する。
5. `GroundCenterTile=True` なのに青画面なら Camera を確認する。
6. Camera は Orthographic かつ Size が極端に小さくないかを確認する。
7. Camera の Z は `-10` 付近を維持する。

### 3. 9シーンを順番に開いて見るポイント

順番は以下で固定すると迷いにくい。

1. `Academy_Classroom`
2. `Academy_Hallway`
3. `Academy_Cafeteria`
4. `Academy_Library`
5. `Academy_Schoolyard`
6. `Academy_Rooftop`
7. `Academy_StudentCouncilRoom`
8. `Academy_Infirmary`
9. `Academy_SchoolGate`

各シーンで見るのは次の4点だけ。

1. Ground が空ではないこと。
2. Collision が壁外周になっていること。
3. NPCSpots が適切な位置にあること。
4. EventTriggers が通路や会話点の近くにあること。

### 4. ポータル位置の微調整

1. `Portals` オブジェクトを開く。
2. 扉、階段、出入口の中心に BoxCollider2D が来るように調整する。
3. 目安として、プレイヤーが 1 ステップ踏み込むと発火する位置に置く。
4. 厚すぎると誤遷移、薄すぎると当たらないので、幅は最小限から始める。
5. Hallway のようなハブは、ポータルの重なりを避ける。

### 5. コリジョン境界の微調整

1. `Collision` Tilemap の外周を優先して見る。
2. 壁の角で引っかかる場合は、角の1マスを調整する。
3. `TilemapCollider2D` + `CompositeCollider2D` の前提を壊さない。
4. すり抜ける箇所があれば、壁より先に当たり判定を太くする。
5. ポータルの近くは「入れるが壁を越えない」状態に寄せる。

### 6. イベントトリガーを既存システムへ接続する

今は `AreaEventTrigger2D` がログ出力するだけなので、明日は次の順で差し替える。

1. `AreaEventTrigger2D` の `Debug.Log` を既存イベント発火処理に置き換える。
2. まずは1種類だけ接続し、会話開始やフラグ更新が動くか確認する。
3. うまく動いたら、各シーンのイベントトリガーへ横展開する。
4. 迷ったら、ログ出力は残したまま二重発火しない構造にする。

### 7. 連続遷移テストのやり方

テストルートは固定する。

1. 教室 -> 廊下
2. 廊下 -> 校庭
3. 校庭 -> 校門
4. 校門 -> 校庭
5. 校庭 -> 廊下
6. 廊下 -> 教室

確認項目は3つだけ。

1. ロード先が正しいこと。
2. 着地位置が壁に埋まっていないこと。
3. 戻り遷移でも同じ不具合が再発しないこと。

## 明日の着手順

1. Unityで Bootstrap を実行する。
2. Classroom と Hallway だけ先に開いて、ポータルとコリジョンを合わせる。
3. Schoolyard と SchoolGate をつないで、外周の遷移を確認する。
4. イベントトリガーを1つだけ既存イベントへ接続する。
5. 連続遷移テストを通して、問題が出た箇所だけ直す。

## 詰まった時の優先順位

1. SceneSpawnResolver が Player に付いているか確認する。PlayerController が自動追加する前提だが、Prefab側に残っていない場合は要確認。
2. Portal の `TargetScene` と `TargetSpawnPointId` が一致しているか確認する。
3. SpawnPoint の `spawnId` が遷移元の指定と一致しているか確認する。
4. Collision が空なら、Bootstrap を再実行する。
5. EventTriggers が効かないなら、Collider2D の `isTrigger` と Player タグを確認する。

## 参考資料

- [docs/maps/area-list.md](./area-list.md)
- [docs/maps/academy-9areas-implementation-checklist.md](./academy-9areas-implementation-checklist.md)
- [Assets/_Project/Scripts/Editor/AcademyMapBootstrapper.cs](../../Assets/_Project/Scripts/Editor/AcademyMapBootstrapper.cs)
- [Assets/_Project/Scripts/MonoBehaviours/Field/ScenePortal2D.cs](../../Assets/_Project/Scripts/MonoBehaviours/Field/ScenePortal2D.cs)
- [Assets/_Project/Scripts/MonoBehaviours/Field/SceneSpawnResolver.cs](../../Assets/_Project/Scripts/MonoBehaviours/Field/SceneSpawnResolver.cs)
- [Assets/_Project/Scripts/MonoBehaviours/Field/AreaEventTrigger2D.cs](../../Assets/_Project/Scripts/MonoBehaviours/Field/AreaEventTrigger2D.cs)
- [docs/unity-error-triage.md](../unity-error-triage.md)
