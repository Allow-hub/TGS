# Commentドキュメント

## 各クラスの役割（簡易まとめ）

- **CommentDisplay**
  - コメント全体の表示・管理の中枢。SpawnerやMoverなどを統括。
- **CommentSpawner**
  - コメントの生成・出現位置の管理。
- **CommentMover**
  - コメントの移動・消滅処理を担当。
- **CommentMaterialApplier**
  - コメントや文字へのマテリアル適用を担当。
- **CommentFactory**
  - コメント本体や文字オブジェクトの生成・プール返却を管理。
- **BuffCommentTrigger / FreezeCommentTrigger**
  - コメント取得時のバフ・特殊効果の発動トリガー。
- **BuffManager**
  - プレイヤーに付与されているバフの管理。
- **BuffBase / SpeedBuff / AttackBuff / MapChangeBuff**
  - バフの基底クラスと各種バフの具体的な効果処理。
- **SpecialCommentData / BuffCommentData / NormalCommentData**
  - 各種コメントデータ（ScriptableObject）を管理。