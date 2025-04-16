# 💡 バフシステム クラス構成

このプロジェクトでは、画面に流れるコメントにバフを仕込んで、プレイヤーがそれに当たるとバフが発動する仕組みになってる。

---

## 📦 クラス一覧と役割

| クラス名                     | 役割説明                                                                 |
|------------------------------|--------------------------------------------------------------------------|
| `BuffBase`                   | 🔹 すべてのバフの共通処理を持つ基底クラス。バフの適用・解除・更新の処理を書くところ。 |
| `SpeedBuff` / `PowerBuff` / `JumpBuff` | 🔹 `BuffBase` を継承したバフごとのクラス。それぞれ固有の処理を実装してる。     |
| `BuffType`                   | 🔹 バフの種類（Speed, Power, Jumpなど）を定義した列挙型。                       |
| `BuffCommentData`            | 🔹 `BuffType` とそれに対応するコメント文をまとめた `ScriptableObject`。         |
| `CommentDisplay`             | 🔹 コメントを画面上に流すクラス。バフコメントもここから出すけど、バフ処理自体はやらない。 |
| `BuffComment`                | 🔸 ★重要！流れてくるバフコメントにアタッチされてて、**プレイヤーが当たるとバフを適用**するやつ。 |

---

## 🧩 関係図（簡略）

```mermaid
classDiagram
    BuffComment --> BuffBase : バフを生成
    BuffBase <|-- SpeedBuff
    BuffBase <|-- PowerBuff
    BuffBase <|-- JumpBuff
    BuffCommentData --> BuffType
    CommentDisplay --> BuffCommentData
    CommentDisplay --> BuffComment
