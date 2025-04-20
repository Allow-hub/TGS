using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class MapChangeBuff : BuffBase
    {
       private int currentMapIndex = -1;

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
            if(MapChangeManager.Instance != null && MapChangeManager.Instance.mapObjects.Length > 0)
            {
                int randomIndex = Random.Range(0, MapChangeManager.Instance.mapObjects.Length);
                currentMapIndex = randomIndex;
                MapChangeManager.Instance.ChangeMap(currentMapIndex);
            }
            else
            {
                Debug.LogError("マップオブジェクトが未設定");
                return;
            }
        }

        public override void Remove(GameObject target)
        {
            base.Remove(target);
            Debug.Log("マップ変形バフが解除されました");
            
            if(MapChangeManager.Instance != null)
            {
                MapChangeManager.Instance.ChangeMap(-1); /* ここでMapを非表示 */
            }

            currentMapIndex = -1;
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
