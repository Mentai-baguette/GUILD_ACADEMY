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

## 音量バランス試聴

- 実施状況: 未実施（2026-04-04時点）
- 未実施理由: 本PRではSE差し替えと権利者表示・要件整合を優先したため
- フォローアップ予定:
  - UI系（`SELECT.wav`, `CANCEL.wav`）の連打試聴
  - 戦闘系（`ATTAK.wav`, `HIT_DAMAGE.wav`, `BREAK.wav`）の相対音量確認
  - クリッピング・耳障りな高域の有無確認

## 参照

- 権利者表示: `docs/audio-credits.md`
- プロジェクト概要: `README.md`
