# V-Link Battle

**Vtuberキャラクターを操作し、バトルアクションを展開するゲーム**です。  
プレイヤーはVtuberを操作して戦います。
チーム制作です。


---

## 担当者：矢萩（Allow-hub）

### 担当スクリプト
- プレイヤー制御  
  https://github.com/Allow-hub/TGS/tree/main/Assets/TechC/VBattle/Scripts/InGame/Player
- 敵NPCの挙動制御  
  https://github.com/Allow-hub/TGS/tree/main/Assets/TechC/VBattle/Scripts/InGame/Npc
- 共通インターフェース群 
  https://github.com/Allow-hub/TGS/tree/main/Assets/TechC/VBattle/Scripts/InGame/Interface

---

## 取り組んだ工夫・技術的挑戦

- **Commandパターンを導入し、NPCの行動を柔軟かつデバッグしやすく管理**
  - → 拡張しやすく、後の実装負担を軽減
- **interfaceを活用し、責務分離・テスト性向上を意識した設計**
- **継承の構造に注意し、Baseクラスの変更が不要になるよう先に設計**
  - → 依存関係を最小限に抑えるよう設計段階から工夫

---

## 制作中の課題・今後の改善
- Commandパターンの設計やinterfaceの使い方は掴めてきたが、**今後はより堅牢な状態管理・ステートマシンとの連携**が課題
- アクション性を高めつつ、**演出・爽快感の調整に時間がかかる見込み**

---

## その他
このプロジェクトでは、**実際にチームメンバーがVtuberとしてデビュー**し、ゲームにも登場するという**実験的な企画**にも挑戦中です。  
