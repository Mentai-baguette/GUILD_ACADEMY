# オーディオクレジット

## BGM（背景音楽）

本プロジェクトで使用しているBGMは、deevid.aiで生成した素材を使用しています。

### 利用条件メモ

- 生成サービス: https://deevid.ai/
- 確認日: 2026-04-07
- 参照規約: https://deevid.ai/ja/terms
- 生成コンテンツの権利: 規約上はユーザーに帰属
- 商用利用: Deevidサポート回答で「生成コンテンツは商用利用可能」と明示
- 再配布: Deevidサポート回答で「生成者はコンテンツの配布・利用が可能」と明示
- クレジット表記: Deevidサポート回答で必須要件の指定なし
- 本PRの扱い: 例外承認対象として記録し、恒久運用にはしない
- 商用利用に関する注意: 第三者素材を入力に使う場合は、利用者側で事前に権利許諾を取得する（Deevidサポート回答の注意事項）
- 証跡: Deevidサポートからの回答メール（2026-04-07受領）

### 例外承認管理（PR #34）

- 承認主体: Repo Maintainer 1名 + PM/タスクオーナー 1名（両者承認）
- 承認記録: PR #34 の Review Approval および承認コメントで記録
- 有効期限: 2026-04-10（ハッカソン提出期限）
- 適用範囲: `Assets/_Project/Audio/BGM/` 配下の4ファイル（`bgm_school_daily.mp3`, `bgm_dungeon.mp3`, `bgm_boss_battle.mp3`, `bgm_ending.mp3`）
- 失効条件: 期限到来、対象ファイル変更、Deevid利用条件変更のいずれか
- 失効後対応: 再承認または素材差し替えを実施するまで `develop` へマージしない

| ファイル名 | 用途 | 出典 | 形式 |
|---|---|---|---|
| `bgm_school_daily.mp3` | 学園日常BGM | deevid.ai | MP3 |
| `bgm_dungeon.mp3` | ダンジョンBGM | deevid.ai | MP3 |
| `bgm_boss_battle.mp3` | ボス戦BGM | deevid.ai | MP3 |
| `bgm_ending.mp3` | エンディングBGM | deevid.ai | MP3 |

- 対象ディレクトリ: `Assets/_Project/Audio/BGM/`
- 検証記録: `docs/audio-validation.md`
- 運用: 追加・差し替え時は本ページと検証記録を同時更新する

## SE（効果音）

本プロジェクトで使用している効果音（SE）は、EVENTSTUDIOで生成した素材を使用しています。

| ファイル名 | 用途 | 出典 | 形式 |
|---|---|---|---|
| `ATTAK.wav` | 通常攻撃 | EVENTSTUDIO | WAV |
| `BREAK.wav` | ブレイク発生 | EVENTSTUDIO | WAV |
| `CANCEL.wav` | メニューキャンセル | EVENTSTUDIO | WAV |
| `CLEAR.wav` | 勝利演出 | EVENTSTUDIO | WAV |
| `CURE.wav` | 回復 | EVENTSTUDIO | WAV |
| `GAMEOVER.wav` | 敗北演出 | EVENTSTUDIO | WAV |
| `HIT_DAMAGE.wav` | 被ダメージ | EVENTSTUDIO | WAV |
| `LEVELUP.wav` | レベルアップ | EVENTSTUDIO | WAV |
| `MAGIC.wav` | 魔法発動 | EVENTSTUDIO | WAV |
| `SELECT.wav` | メニュー決定 | EVENTSTUDIO | WAV |

- 対象ディレクトリ: `Assets/_Project/Audio/SE/`
- 検証記録: `docs/audio-validation.md`
- 運用: 追加・差し替え時は本ページと検証記録を同時更新する
- 商用利用に関する注意: 現在のSE素材は商用利用不可のため、商用利用する場合はEVENTSTUDIOの有償プラン契約（課金）を行うか、商用利用可能な素材へ差し替えること

## 備考

本ページはチームメンバー向けの権利者表示管理用ドキュメントです。
