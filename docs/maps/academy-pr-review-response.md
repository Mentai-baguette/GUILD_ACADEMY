# AcademyマップPR 再レビュー対応メモ

## 1. Blocking指摘への対応

### 1-1. Academyシーンのmeta/GUID整合
- `Assets/Scenes/Academy/Academy_Dormitory.unity` を追加。
- Academy配下シーンの `.unity` / `.meta` ペア欠落がないことを確認。
- `ProjectSettings/EditorBuildSettings.asset` へ Academy シーンを登録し、各 `guid` が対応する `.meta` と一致することを確認。

### 1-2. Battle GUID破壊リスク
- `Assets/Scenes/Battle.unity.meta` の `guid: 3a3225b6e2f714a4b8718e355fc9074e` を維持。
- Build Settings 側の Battle 参照 GUID は同値。

### 1-3. 要件「9エリア」と差分不一致
- `docs/maps/academy-9areas-implementation-checklist.md` に「9エリアは論理区分、実装シーンは導線シーンを含む」ことを明記。
- Dormitory を実装対象に追加。

### 1-4. スコープ外変更混在
- 本PRは一旦継続し、理由を明示する方針。
- 次回分離候補: Battleシーン本体変更、dialogue文言更新、MCP安定化パッチ。

### 1-5. テスト根拠不足
- `Assets/Tests/EditMode/Field/PlayerControllerTests.cs` を追加。
- `CanMove` の停止挙動と `FixedUpdate` での速度反映を自動検証。

## 2. 自動検証ログ（2026-04-10）

### 2-1. Academyシーン meta 同梱確認
- コマンド: `find Assets/Scenes/Academy -maxdepth 1 -name '*.unity'` と対応 `.meta` 存在確認
- 結果: `ALL_ACADEMY_SCENES_HAVE_META`

### 2-2. Build Settings GUID一致確認
- コマンド: `ProjectSettings/EditorBuildSettings.asset` の path/guid を抽出して対象 `.meta` と照合
- 結果: `BUILDSETTINGS_GUID_MATCH_OK`

## 3. 手動確認項目（再レビュー前）
- [ ] Dormitory シーンをUnityで開き、最低限の遷移導線が成立している
- [ ] Academy各シーン間の遷移で `ScenePortal2D` / SpawnPoint の整合が崩れていない
- [ ] カメラ追従が各シーンで破綻しない
- [ ] Battle遷移経路に回帰不能や参照切れがない
