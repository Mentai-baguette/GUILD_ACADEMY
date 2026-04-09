# オーディオクレジット

## BGM（背景音楽）

本プロジェクトで使用しているBGMは、deevid.aiで生成した素材を使用しています。

### 利用条件確認記録

**確認日**: 2026-04-07
**確認方法**: Deevid サポートへの問い合わせ（メール）
**回答受領日**: 2026-04-07
**確認者**: プロジェクトメンバー

**確認内容と回答要旨**:
1. **生成コンテンツの権利帰属**: 生成したコンテンツの権利はユーザーに帰属する（Deevid利用規約 https://deevid.ai/ja/terms に基づく）
2. **商用利用**: 生成コンテンツはゲーム作品に同梱して配布できる旨を確認済み
3. **クレジット表記**: 必須要件の指定なし（任意）
4. **注意事項**: 第三者素材を入力に使用した場合の権利許諾は利用者側の責任

**適用範囲**: 本確認結果は PR #34 で追加する以下の4ファイルに限定される
- `bgm_school_daily.mp3`
- `bgm_dungeon.mp3`
- `bgm_boss_battle.mp3`
- `bgm_ending.mp3`

**有効期限**: 2026-04-10（ハッカソン提出期限）。期限後に利用を継続する場合は Deevid の最新利用規約を再確認すること

**運用上の注意**:
- 本PRの扱いは例外承認対象であり、恒久運用にはしない
- 追加のBGMを生成・利用する場合は、都度本ドキュメントを更新し再確認を行うこと
- Deevid の利用規約が変更された場合は、既存ファイルの利用可否も再確認すること

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
