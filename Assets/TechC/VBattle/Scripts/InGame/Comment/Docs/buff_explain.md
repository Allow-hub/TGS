# 💡 バフシステム クラス構成

このプロジェクトでは、画面に流れるコメントにバフを仕込んで、プレイヤーがそれに当たるとバフが発動する仕組みになってる。

---

## 📦 クラス一覧と役割

| クラス名                     | 役割説明                                                                 |
|------------------------------|--------------------------------------------------------------------------|
| `BuffBase`                   | 🔹 すべてのバフの共通処理を持つ基底クラス。バフの適用・解除・更新の処理を書くところ。 |
| `SpeedBuff` / `PowerBuff` / `JumpBuff` | 🔹 `BuffBase` を継承したバフごとのクラス。それぞれ固有の処理を実装してる。     |
| `MapChangeBuff`              | 🔹 `BuffBase` を継承した新しいバフ。マップをランダムに変更する処理を持つ。         |
| `BuffType`                   | 🔹 バフの種類（Speed, Power, Jump, MapChangeなど）を定義した列挙型。            |
| `BuffCommentData`            | 🔹 `BuffType` とそれに対応するコメント文をまとめた `ScriptableObject`。         |
| `BuffManager`                | 🔹 プレイヤーにアタッチ。複数のバフを同時に管理し、時間経過で更新・削除する。     |
| `BuffFactory`                | 🔹 `BuffType` を元に対応する `BuffBase` のインスタンスを生成する工場クラス。     |
| `BuffCommentTrigger`         | 🔸 ★重要！流れてくるコメントにアタッチ。**プレイヤーが当たると該当バフを発動**する。|
| `MapChangeManager`           | 🔹 マップ切り替え専用のマネージャー。`MapChangeBuff` から呼ばれてマップを変更する。 |

---

## 🧩 関係図（簡略）

```mermaid
classDiagram
    BuffCommentTrigger --> BuffFactory : バフを生成
    BuffFactory --> BuffBase
    BuffBase <|-- SpeedBuff
    BuffBase <|-- PowerBuff
    BuffBase <|-- JumpBuff
    BuffBase <|-- MapChangeBuff
    BuffCommentData --> BuffType
    BuffManager --> BuffBase : 管理
    MapChangeBuff --> MapChangeManager : マップ変更呼び出し
    CommentDisplay --> BuffCommentData
    CommentDisplay --> BuffCommentTrigger
