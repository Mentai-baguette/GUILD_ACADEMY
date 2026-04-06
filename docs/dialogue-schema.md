# 会話JSONスキーマ

`ResourcesDialogueJsonLoader` が読み込む会話JSONの仕様。

## ルート構造

```json
{
  "entries": [
    {
      "id": "ch1_start_intro",
      "speaker": "Narration",
      "text": "...",
      "next": "ch1_choice",
      "choices": []
    }
  ]
}
```

- `entries` は必須
- `entries` は1件以上必要

## Entry仕様

- `id: string` 必須（空文字・空白のみは禁止）
- `speaker: string` 任意
- `text: string` 任意
- `next: string` 任意
- `choices: DialogueChoice[]` 任意

`next` と `choices` の同時利用は技術的には可能だが、運用上はどちらか片方に統一する。

## Choice仕様

- `text: string` 任意
- `next: string` 任意
- `flag: string` 任意
- `trustEffects` 任意

`trustEffects` は2形式を受け付ける。

配列形式（標準）:

```json
"trustEffects": [
  { "characterId": "Yuna", "value": 10 },
  { "characterId": "Mio", "value": -5 }
]
```

オブジェクト形式（互換）:

```json
"trustEffects": {
  "Yuna": 10,
  "Mio": -5
}
```

ローダーはオブジェクト形式を内部で配列形式へ正規化して扱う。

## バリデーションと例外

`LoadEntries`:

- `sourceKey` が null/空/空白のみ:
  - `ArgumentException`
  - `Source key cannot be null or empty.`
- `Resources` にファイルがない:
  - `FileNotFoundException`
  - `Dialogue JSON was not found in Resources: {sourceKey}`

`Parse`:

- `json` が null/空/空白のみ:
  - `ArgumentException`
  - `JSON content is empty.`
- JSONの解析失敗:
  - `InvalidDataException`
  - `Failed to parse dialogue JSON: {sourceName}`
- `entries` が null または空:
  - `InvalidDataException`
  - `Dialogue JSON has no entries: {sourceName}`
- entry `id` が null/空/空白のみ:
  - `InvalidDataException`
  - `Dialogue entry id is empty: {sourceName}`
- `trustEffects[].characterId` が null/空/空白のみ:
  - `InvalidDataException`
  - `trustEffects.characterId cannot be empty.`

## 運用ルール

- `id` はファイル内で一意にする
- `next` / `choices[].next` で参照するIDは実在させる
- 新章追加時はこのファイルを更新し、異常系テストを追加する