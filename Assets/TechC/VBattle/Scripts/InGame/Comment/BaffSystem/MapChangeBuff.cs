using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// マップ変更バフ。適用されたターゲットのマップをランダムに変更する。
    /// </summary>
    public class MapChangeBuff : BuffBase
    {
        private int currentMapIndex = -1;

        public MapChangeBuff()
        {
            buffName = "MapChangeBuff";
            description = "マップが変化する";
            buffDuration = 7.0f;
            remainingTime = buffDuration;
        }

        /// <summary>
        /// バフが適用されたとき、ランダムでマップを変更する。
        /// </summary>
        public override void Apply(GameObject target)
        {
            base.Apply(target);
            // Debug.Log("マップ変化のバフを発動");

            var manager = MapChangeManager.Instance;
            if (manager != null && manager.mapObjects != null && manager.mapObjects.Count > 0)
            {
                // currentMapIndex = Random.Range(0, manager.mapObjects.Count);
                manager.SetMapIndex();
            }
            else
            {
                Debug.LogError("MapChangeManager または mapObjects が未設定です。");
            }
        }

        /// <summary>
        /// バフが解除されたとき、マップを非表示にする。
        /// </summary>
        public override void Remove(GameObject target)
        {
            base.Remove(target);
            // Debug.Log("マップ変化バフが解除されました");

            var manager = MapChangeManager.Instance;
            if (manager != null)
            {
                // -1を渡してすべて非表示にする場合
                manager.SetMapIndex();
            }

            currentMapIndex = -1;
        }

        /// <summary>
        /// バフの残り時間を更新し、終了時にバフを解除する。
        /// </summary>
        public override void UpdateBuff(float deltaTime, GameObject target)
        {
            base.UpdateBuff(deltaTime, target);

            remainingTime -= deltaTime;
            if (remainingTime <= 0f)
            {
                Remove(target);
            }
        }
    }
}
