# 学園9エリア Tilemap 実装チェックリスト

このチェックリストは docs/maps/area-list.md を参照しつつ、今回の対象9エリアを実装するための作業用メモ。

## 対象シーン

- Academy_Classroom
- Academy_Hallway
- Academy_Cafeteria
- Academy_Library
- Academy_Schoolyard
- Academy_Rooftop
- Academy_StudentCouncilRoom
- Academy_Infirmary
- Academy_SchoolGate

## 進捗

- [x] 9シーンの雛形を自動生成するEditorスクリプトを追加
- [x] Portal遷移コンポーネントを追加
- [x] SpawnPoint/SpawnResolverを追加
- [ ] 9シーン雛形をUnityメニューから生成
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
- Hallway <-> Infirmary
- Hallway <-> StudentCouncilRoom
- Hallway <-> Rooftop
- Hallway <-> Schoolyard
- Schoolyard <-> SchoolGate

## 実装メモ

- タイル素材は共通パレットを使用
- Collision Tilemap は TilemapCollider2D + CompositeCollider2D を使用
- Portal は BoxCollider2D(isTrigger=true) + ScenePortal2D を使用
- SpawnPoints には SceneSpawnPoint を付与し、IDで着地位置を決定
