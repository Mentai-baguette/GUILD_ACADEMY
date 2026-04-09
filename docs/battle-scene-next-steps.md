# バトルシーン 次にやること

## 1. シーン接続（優先度: 高）
- [ ] `Assets/Scenes/Battle.unity` を開き、`BattleSceneRoot` に `BattleSceneBootstrap` を配置する。
- [ ] `BattleUIManager` と `BattleBackdropController` の参照を作成・割り当てする。
- [ ] Inspector で味方側 root と敵側 root の Transform をバインドする。
- [ ] フィールド側から `SceneNames.Battle` の読み込み経路が動くことを確認する。

## 2. UIレイアウト設定（優先度: 高）
- [ ] 味方スロット（左側）と敵スロット（右側）を配置する。
- [ ] コマンドパネルとバトルメッセージパネルを配置する。
- [ ] `BattleCommandMenuView` のボタンとアクターラベルの参照を割り当てる。
- [ ] 各 `BattleCombatantView` の参照（name/hp/mp/atb/break のテキストと fill）を割り当てる。

## 3. 描画 / レイヤー順（優先度: 高）
- [ ] ワールド・エフェクトオブジェクトが UI Canvas より背面で描画されることを確認する。
- [ ] エフェクトオブジェクトを `EffectLayer` 配下に置き、ソート順を検証する。
- [ ] バトル用カメラ設定（orthographic size、クリアカラー、クリッピング）を確認する。

## 4. データ連携 / 実行確認（優先度: 中）
- [ ] `SceneTransitionData` のキーで味方・敵データを渡す。
  - `battle.party`
  - `battle.enemies`
- [ ] 遷移データがない場合に fallback バトルデータで動くことを確認する。
- [ ] ATB の Tick により UI が継続更新されることを確認する。

## 5. エンカウント統合（優先度: 中）
- [ ] エンカウントトリガーからバトルシーンを Additive 読み込みできるよう接続する（または現行の遷移フローに合わせる）。
- [ ] バトル終了後の戻り先（フィールド復帰または次シーン）を定義する。

## 6. オーディオ / 背景切替（優先度: 中）
- [ ] シーン遷移と AudioManager の BGM 切替を接続する。
- [ ] academy/shion/carlos 各フェーズ用の背景スプライトを割り当てる。
- [ ] バトルフェーズに応じた背景選択が正しいことを確認する。

## 7. 検証チェックリスト（PR前）
- [ ] バトルシーンが Build Settings に入り、正常に読み込める。
- [ ] 味方・敵スロットとコマンド UI が正しく表示される。
- [ ] コマンド入力（attack/skill/item/defend）が実行され、状態が更新される。
- [ ] バトル終了イベントが発火し、遷移動作が正しい。
- [ ] バトル用 MonoBehaviour にコンパイルエラーがない。
