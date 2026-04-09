# 学園9エリア Tilemap 実装チェックリスト

このチェックリストは docs/maps/area-list.md を参照しつつ、今回の対象9エリアを実装するための作業用メモ。

## 対象シーン

- Academy_Classroom
- Academy_Hallway
- Academy_Cafeteria
- Academy_Library
- Academy_Schoolyard
- Academy_dormitory
- Academy_StudentCouncilRoom
- Academy_SchoolGate
- Academy_TrainingGround

## 進捗

- [x] 対象シーンの雛形を自動生成するEditorスクリプトを追加
- [x] Portal遷移コンポーネントを追加
- [x] SpawnPoint/SpawnResolverを追加
- [x] 対象シーンに初期タイルレイアウトとイベントトリガーを配置するEditorスクリプトを追加
- [x] 対象シーンにNPCスポットを配置するEditorスクリプトを追加
- [x] Academy_TrainingGround の雛形シーンを新規作成
- [x] Hallway <-> TrainingGround 往復接続のEditorスクリプトを追加
- [x] Academy_dormitory を既存シーンとして Build Settings に登録
- [x] SceneNames に AcademyDormitory 定数を追加
- [ ] 9シーン雛形をUnityメニューから生成
- [x] Academy_dormitory シーンをBuild Settings に登録
- [ ] 各シーンにタイル配置（Ground/Decoration/Collision）
- [ ] NPC配置スポット配置
- [ ] イベントトリガー配置
- [ ] ポータル位置の微調整
- [ ] プレイヤーPrefabにSceneSpawnResolverを追加
- [ ] Build Settingsの遷移確認

## 遷移ネットワーク（初期案）

- Classroom <-> Hallway
- Hallway <-> Cafeteria
- Hallway <-> Library
- Hallway <-> Schoolyard
- Hallway <-> Dormitory
- Hallway <-> StudentCouncilRoom
- Hallway <-> TrainingGround
- Schoolyard <-> SchoolGate

## 寮追加実装（area-list 1-2 対応）

`docs/maps/area-list.md` の「1-2. 寮」要件に対する実装チェック。

- [x] Academy_dormitory を生成し、最低限の構成（Grid/Ground/Decoration/Collision/Camera）を成立させる
- [x] 共用ラウンジと個室導線を成立させる
- [x] 休息・浄化・夜会話に必要な配置を行う
- [x] 寮内 SLイベント用 EventTriggers を配置する
- [x] 寮内夜イベント用 EventTriggers を配置する
- [x] 滞在会話向け NPCSpots を複数配置する（ラウンジ・廊下・入口想定）
- [x] Hallway <-> Dormitory の往復ポータルを成立させる
- [x] SpawnPoint ID を接続先ポータルと一致させる（`from_hallway`）
- [x] 夜パート想定で主要イベント点が1画面目で把握できることを確認する

## 実装メモ

- タイル素材は共通パレットを使用
- Collision Tilemap は TilemapCollider2D + CompositeCollider2D を使用
- Portal は BoxCollider2D(isTrigger=true) + ScenePortal2D を使用
- SpawnPoints には SceneSpawnPoint を付与し、IDで着地位置を決定# 学園9エリア Tilemap 実装チェックリスト

このチェックリストは docs/maps/area-list.md を参照しつつ、今回の対象9エリアを実装するための作業用メモ。

## 対象シーン

- Academy_Classroom
- Academy_Hallway
- Academy_Cafeteria
- Academy_Library
- Academy_Schoolyard
- Academy_dormitory
- Academy_StudentCouncilRoom
- Academy_SchoolGate
- Academy_TrainingGround

## 進捗

- [x] 対象シーンの雛形を自動生成するEditorスクリプトを追加
- [x] Portal遷移コンポーネントを追加
- [x] SpawnPoint/SpawnResolverを追加
- [x] 対象シーンに初期タイルレイアウトとイベントトリガーを配置するEditorスクリプトを追加
- [x] 対象シーンにNPCスポットを配置するEditorスクリプトを追加
- [x] Academy_TrainingGround の雛形シーンを新規作成
- [x] Hallway <-> TrainingGround 往復接続のEditorスクリプトを追加
- [x] Academy_dormitory を既存シーンとして Build Settings に登録
- [x] SceneNames に AcademyDormitory 定数を追加
- [ ] 9シーン雛形をUnityメニューから生成
- [x] Academy_dormitory シーンをBuild Settings に登録
- [ ] 各シーンにタイル配置（Ground/Decoration/Collision）
- [ ] NPC配置スポット配置
- [ ] イベントトリガー配置
- [ ] ポータル位置の微調整
- [ ] プレイヤーPrefabにSceneSpawnResolverを追加
- [ ] Build Settingsの遷移確認

## 遷移ネットワーク（初期案）

- Classroom <-> Hallway
- Hallway <-> Cafeteria
- Hallway <-> Library
- Hallway <-> Schoolyard
- Hallway <-> Dormitory
- Hallway <-> StudentCouncilRoom
- Hallway <-> TrainingGround
- Schoolyard <-> SchoolGate

## 寮追加実装（area-list 1-2 対応）

`docs/maps/area-list.md` の「1-2. 寮」要件に対する実装チェック。

- [x] Academy_dormitory を生成し、最低限の構成（Grid/Ground/Decoration/Collision/Camera）を成立させる
- [x] 共用ラウンジと個室導線を成立させる
- [x] 休息・浄化・夜会話に必要な配置を行う
- [x] 寮内 SLイベント用 EventTriggers を配置する
- [x] 寮内夜イベント用 EventTriggers を配置する
- [x] 滞在会話向け NPCSpots を複数配置する（ラウンジ・廊下・入口想定）
- [x] Hallway <-> Dormitory の往復ポータルを成立させる
- [x] SpawnPoint ID を接続先ポータルと一致させる（`from_hallway`）
- [x] 夜パート想定で主要イベント点が1画面目で把握できることを確認する

## 実装メモ

- タイル素材は共通パレットを使用
- Collision Tilemap は TilemapCollider2D + CompositeCollider2D を使用
- Portal は BoxCollider2D(isTrigger=true) + ScenePortal2D を使用
- SpawnPoints には SceneSpawnPoint を付与し、IDで着地位置を決定
