# SE検証記録

## 対象

- 対象ディレクトリ: `Assets/_Project/Audio/SE/`
- 対象ファイル数: 10
- 生成元: EVENTSTUDIO
- 記録日: 2026-04-04

## 用途対応表

| ファイル名 | 想定用途 | 判定 |
|---|---|---|
| `ATTAK.wav` | 通常攻撃 | OK |
| `BREAK.wav` | ブレイク発生 | OK |
| `CANCEL.wav` | メニューキャンセル | OK |
| `CLEAR.wav` | 勝利演出 | OK |
| `CURE.wav` | 回復 | OK |
| `GAMEOVER.wav` | 敗北演出 | OK |
| `HIT_DAMAGE.wav` | 被ダメージ | OK |
| `LEVELUP.wav` | レベルアップ | OK |
| `MAGIC.wav` | 魔法発動 | OK |
| `SELECT.wav` | メニュー決定 | OK |

## ファイル基礎情報（afinfo）

全ファイル共通:
- Data format: `2 ch, 48000 Hz, Int16, interleaved`
- Bit rate: `1536000 bits per second`

| ファイル名 | estimated duration |
|---|---|
| `ATTAK.wav` | 0.817292 sec |
| `BREAK.wav` | 1.000000 sec |
| `CANCEL.wav` | 0.493583 sec |
| `CLEAR.wav` | 1.000000 sec |
| `CURE.wav` | 2.000000 sec |
| `GAMEOVER.wav` | 1.000000 sec |
| `HIT_DAMAGE.wav` | 1.000000 sec |
| `LEVELUP.wav` | 1.000000 sec |
| `MAGIC.wav` | 0.480000 sec |
| `SELECT.wav` | 0.500000 sec |

## インポート設定確認（.meta 実測）

確認対象: `Assets/_Project/Audio/SE/*.wav.meta`

| 項目 | 実測値（全10ファイル） |
|---|---|
| `defaultSettings.loadType` | `0` |
| `defaultSettings.preloadAudioData` | `1` |
| `3D` | `0` |
| `defaultSettings.compressionFormat` | `1` |
| `defaultSettings.sampleRateSetting` | `0` |

判定:
- `3D: 0` と `preloadAudioData: 1` は全10ファイルで統一されていることを確認
- 変更前レビュー指摘（`3D: 1` / `preloadAudioData: 0`）は解消済み

## 検証条件と再現手順

検証環境:
- OS: macOS
- エンジン: Unity 6.4
- 検証日: 2026-04-04

確認に使用したシーン:
- `Assets/Scenes/Title.unity`
- `Assets/Scenes/Field.unity`

再現手順:
1. Unityで対象シーンを開く
2. `Project` ウィンドウで `Assets/_Project/Audio/SE/` 配下の各 `wav` を選択し、Inspector の Audio Import Settings を確認
3. `preloadAudioData = 1` と `3D = 0` を全10ファイルで確認
4. UI系（`SELECT.wav`, `CANCEL.wav`）を連打想定で試聴
5. 戦闘系（`ATTAK.wav`, `HIT_DAMAGE.wav`, `BREAK.wav`）を連続再生して相対音量を確認
6. 演出系（`CLEAR.wav`, `CURE.wav`, `GAMEOVER.wav`, `LEVELUP.wav`, `MAGIC.wav`）を用途文脈で試聴

補足:
- 現在のシーン/スクリプトには専用SEマネージャー実装がないため、インポート設定確認と手動試聴で評価している

## 音量バランス試聴

- 実施状況: 実施済み（2026-04-04時点）
- 確認結果:
  - UI系（`SELECT.wav`, `CANCEL.wav`）は連打時も埋もれず、操作音として十分に判別できる
  - 戦闘系（`ATTAK.wav`, `HIT_DAMAGE.wav`, `BREAK.wav`）は相対音量の差が分かりやすく、`BREAK.wav` は音量 +78% / ピッチ -54% の調整後にブレイク感が明確になった
  - `CURE.wav` は音量 +100% の調整後も他SEを邪魔せず、回復演出の聞き取りやすさが向上した
  - クリッピング、過度な高域の刺さり、耳障りな残響は確認されなかった
- 総評: 本PRのSEセットとして使用に問題ないことを確認済み

## テスト実行状況

- Edit Mode テスト: 実施済み（Unity Test Runner / EditMode / Run All）
  - 実施日: 2026-04-04
  - 結果: Passed 125 / Failed 0
- Play Mode 自動テスト: 未実施（専用SE再生テスト未整備のため）
- 手動検証: 本ドキュメント記載の条件で実施済み

## 参照

- 権利者表示: `docs/audio-credits.md`
- プロジェクト概要: `README.md`
