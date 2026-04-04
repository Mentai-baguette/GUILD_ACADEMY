# オーディオ検証記録

## BGM検証記録

### 対象

- 対象ディレクトリ: `Assets/_Project/Audio/BGM/`
- 対象ファイル数: 4
- 生成元: deevid.ai
- 記録日: 2026-04-05
- 実行者: PR作成者
- 実行環境: macOS / Unity 6.4

### 本PRに紐づく検証範囲

- 本PRの差分は BGM 4曲の追加と音声ドキュメント更新であり、コード変更は含まない
- そのため、検証は BGM の手動試聴、ループ再生、ファイルサイズ、インポート設定確認を中心に実施
- 既存の Edit Mode テスト実績は本PRのベースラインとして参照する
- 本PRで追加した BGM は、シーン適合と再生品質の確認を主目的として検証している

### 用途対応表

| ファイル名 | 想定用途 | 判定 |
|---|---|---|
| `bgm_school_daily.mp3` | 学園日常 | OK |
| `bgm_dungeon.mp3` | ダンジョン探索 | OK |
| `bgm_boss_battle.mp3` | ボス戦 | OK |
| `bgm_ending.mp3` | エンディング | OK |

### 雰囲気評価（手動試聴）

| ファイル名 | 期待した雰囲気 | 試聴結果 | 判定 |
|---|---|---|---|
| `bgm_school_daily.mp3` | 学園生活の穏やかさ・日常感 | 会話や移動時に主張しすぎず、日常シーンの空気感と整合 | OK |
| `bgm_dungeon.mp3` | 緊張感・探索感 | 不安感を維持しつつテンポが崩れず、探索シーンに適合 | OK |
| `bgm_boss_battle.mp3` | 高揚感・圧迫感 | 戦闘開始から終盤まで勢いを維持し、ボス戦の強度に適合 | OK |
| `bgm_ending.mp3` | 余韻・終幕感 | 終了演出の余韻を阻害せず、エンディング遷移に適合 | OK |

### ループ再生評価

検証条件:
- Unity Editor（macOS）で各曲を3回以上連続ループ再生
- 繋ぎ目のクリックノイズ、拍ズレ、音量段差の有無を確認

| ファイル名 | ループ回数 | 違和感 | 判定 |
|---|---:|---|---|
| `bgm_school_daily.mp3` | 3回 | なし | OK |
| `bgm_dungeon.mp3` | 3回 | なし | OK |
| `bgm_boss_battle.mp3` | 3回 | なし | OK |
| `bgm_ending.mp3` | 3回 | なし | OK |

### ファイルサイズ評価

判定基準:
- BGM単体ファイルとして極端に大きくないこと
- リポジトリ運用上、4曲合計で過度な容量増加にならないこと

実測（`ls -lh Assets/_Project/Audio/BGM`）:
- `bgm_school_daily.mp3`: 1.6MB
- `bgm_dungeon.mp3`: 4.1MB
- `bgm_boss_battle.mp3`: 3.9MB
- `bgm_ending.mp3`: 2.9MB

評価:
- 4曲合計で約12.5MB。現行運用で許容範囲内と判断

### チーム確認記録

- 実施状況: 実施済み
- 確認日: 2026-04-05
- 確認方法: チーム試聴（学園日常/ダンジョン/ボス戦/エンディングの4シーン想定）
- 結果: 雰囲気OKで合意

### BGM追加後の手動再確認

- 再確認日: 2026-04-05
- 再確認対象: `bgm_school_daily.mp3`, `bgm_dungeon.mp3`, `bgm_boss_battle.mp3`, `bgm_ending.mp3`
- 再確認内容: ループ接続、シーン適合、音量感、ファイルサイズ
- 判定: OK

## SE検証記録

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
  - PR Checks: [Edit Mode Tests](https://github.com/Mentai-baguette/GUILD_ACADEMY/actions/runs/23975867310/job/69932739597)
  - Workflow run: [Unity Tests](https://github.com/Mentai-baguette/GUILD_ACADEMY/actions/runs/23975867310)
  - Test Results check: [Test Results](https://github.com/Mentai-baguette/GUILD_ACADEMY/runs/69933004538)
- Play Mode 自動テスト: 未実施（専用SE再生テスト未整備のため）
- 手動検証: 本ドキュメント記載の条件で実施済み

## 参照

- 権利者表示: `docs/audio-credits.md`
- プロジェクト概要: `README.md`
