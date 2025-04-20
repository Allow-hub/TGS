using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class MapChangeBuff : BuffBase
    {
        private bool isMapChanged = false;

        public MapChangeBuff()
        {
            buffName = "MapChangeBuff";
            description = "マップが変化する";
            buffDuration = 2.0f;
            remainingTime = buffDuration;
        }

        public override void Apply(GameObject target)
        {
            base.Apply(target);
            Debug.Log("マップ変化のバフを発動");

            /* マップの変更をまだ行っていない場合は、マップを変更する */
            if (!isMapChanged)
            {
                if (MapChangeManager.Instance != null)
                {
                    MapChangeManager.Instance.ChangeMap(); /* ChangeMapメソッドをよぶ */
                    isMapChanged = true; /* マップを変化させたフラグを立てる */
                }
                else
                {
                    Debug.LogError("MapChangeManagerのInstanceがない");
                }
            }
        }

        public override void Remove(GameObject target)
        {
            base.Remove(target);
            Debug.Log("マップ変形バフが解除されました");
            isMapChanged = false; /* フラグをリセットする */
        }

        public override void UpdateBuff(float deltaTime, GameObject target)
        {
            base.UpdateBuff(deltaTime, target);

            if (remainingTime <= 0)
            {
                Remove(target);
            }
            else
            {
                remainingTime -= deltaTime;
            }
        }
    }
}
