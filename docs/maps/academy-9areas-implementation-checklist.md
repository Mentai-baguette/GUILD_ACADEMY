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
- Academy_TrainingGround
- Academy_Courtyard

## 進捗

- [x] 9シーンの雛形を自動生成するEditorスクリプトを追加
- [x] Portal遷移コンポーネントを追加
- [x] SpawnPoint/SpawnResolverを追加
<<<<<<< HEAD
=======
- [x] 9シーンに初期タイルレイアウトとイベントトリガーを配置するEditorスクリプトを追加
- [x] 9シーンにNPCスポットを配置するEditorスクリプトを追加
- [x] Academy_TrainingGround の雛形シーンを新規作成
- [x] Hallway <-> TrainingGround 往復接続のEditorスクリプトを追加（GuildAcademy/Maps/Connect Hallway <-> TrainingGround）
- [x] Academy_Courtyard を新規生成するEditorスクリプトを追加（GuildAcademy/Maps/Create Academy Courtyard）
- [x] SceneNames に Academy_Courtyard 定数を追加
>>>>>>> d0781996 (feat: add academy map assets)
- [ ] 9シーン雛形をUnityメニューから生成
- [x] Academy_Courtyard シーンをUnityメニューから生成
- [ ] 各シーンにタイル配置（Ground/Decoration/Collision）
- [ ] NPC配置スポット配置
- [ ] イベントトリガー配置
- [ ] ポータル位置の微調整
- [ ] プレイヤーPrefabにSceneSpawnResolverを追加
- [ ] Build Settingsの遷移確認
- [x] Academy_Classroom は類似タイル・小物で先に完成させ、不足素材は後で差し替える
- [x] Academy_Library は類似タイルで仮実装し、不足素材は後で差し替える
- [ ] Academy_Cafeteria は類似タイル・小物で先に完成させ、不足素材は後で差し替える

## 遷移ネットワーク（初期案）

- Classroom <-> Hallway
- Hallway <-> Cafeteria
- Hallway <-> Library
- Hallway <-> Infirmary
- Hallway <-> StudentCouncilRoom
- Hallway <-> Rooftop
- Hallway <-> Schoolyard
- Hallway <-> Courtyard
- Schoolyard <-> Courtyard
- Schoolyard <-> SchoolGate

## 中庭追加実装（area-list 1-6 対応）

`docs/maps/area-list.md` の「1-6. 中庭」要件に対する実装チェック。

- [x] Academy_Courtyard を生成し、最低限の構成（Grid/Ground/Decoration/Collision/Camera）を成立させる
- [x] 開放感を優先した Ground レイアウトにする（中央会話余白を確保）
- [x] 噴水中心の会話余白を配置する（Fountain_Center + 周辺導線）
- [x] 花壇スペースを4箇所配置し、会話視認性を阻害しない
- [x] SLイベント用 EventTriggers を配置する（`courtyard_sl_01`〜）
- [x] 日曜特殊イベント用 EventTriggers を配置する（`courtyard_sunday_01`〜）
- [x] 滞在会話向け NPCSpots を複数配置する（噴水前・花壇脇・ベンチ想定）
- [x] Hallway <-> Courtyard の往復ポータルを成立させる
- [x] Schoolyard <-> Courtyard の往復ポータルを成立させる
- [x] SpawnPoint ID を接続先ポータルと一致させる（`from_hallway` / `from_schoolyard`）
- [x] 放課後想定で「通路よりイベント点が先に見える」カメラ画角に調整する
- [x] 日曜想定で主要イベント点が1画面目で把握できることを確認する

## 実装メモ

- タイル素材は共通パレットを使用
- Collision Tilemap は TilemapCollider2D + CompositeCollider2D を使用
- Portal は BoxCollider2D(isTrigger=true) + ScenePortal2D を使用
- SpawnPoints には SceneSpawnPoint を付与し、IDで着地位置を決定
- Academy_Classroom は from_hallway の SpawnPoint を追加し、NPCSpots を机列寄りへ再配置している。
- Academy_Classroom は EventTriggers を教室中央導線（x=-2/2, y=-0.5）へ寄せ、from_hallway の向きを上向きに統一済み。
- Academy_Classroom はまず類似タイルで仮配置する。黒板は濃色壁/掲示系、窓は既存窓タイル、教卓は机系、壁時計は円形小物、青結晶は青系発光小物で代用し、導線と会話視認性を優先する
- Classroom の本当に不足している候補は、教室専用の黒板パーツ、窓の連続フレーム、教卓専用タイル、壁時計専用小物、青結晶専用オブジェクト
- Academy_Classroom の代用オブジェクト（Blackboard / Window_Left / Window_Right / TeacherDesk / WallClock / BlueCrystal）は Decoration 配下の上側座標に配置し、Event/NPC/Spawn の導線（y=-5.5〜-0.5）と非干渉に調整済み
- Academy_Classroom の Decoration は実タイルへ置換済み（黒板帯 y=5..6、窓 x=±6..8、教卓 y=3、時計/青結晶 y=7）。導線帯（y=-5.5〜-0.5）とは分離している
- Academy_Library はまず類似タイルで仮配置する。木床は既存床タイル、石壁は既存壁タイル、書架は近い棚タイル、机と照明は既存室内小物で代用し、通路幅を優先する
- Academy_Library の Decoration は仮タイルを 5x5 に拡張済み。次は Collision の通路幅確認と、必要なら NPCSpots / EventTriggers の微調整を行う
- Academy_Library は EventTrigger 2 を NPCSpot 3 と重ならない位置に移動済み（誤発火回避）
- 現時点で不足候補として扱うものは、図書館専用の本棚連結パーツ、長尺の移動梯子、2階回廊の手すり、魔法ランプ専用小物、窓専用の飾りパーツ。これらは仮配置後に専用素材へ差し替える
- Academy_Cafeteria はまず類似タイルで完成させる。大窓は近い窓/開口表現、長机は既存テーブル系、ベンチは近い座席系、配膳カウンターは棚/カウンター系、照明と食器は既存の室内小物と Food 系で代用し、後で専用素材へ差し替える
- Cafeteria の本当に不足しているものは、連続アーチ窓、学園紋章入りバナー、食堂専用の長机・ベンチ、配膳カウンター、暖炉または厨房らしい加熱設備、皿/カップ/カトラリーの専用小物
