using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class BuffTester : MonoBehaviour
    {
        private TechC.SpeedBuff speedBuff;
        private GameObject target;
        private bool isBuffApplied = false;
        void Start()
        {
            /* Player自身が対象 */
            target = gameObject;

            /* SpeedBuffのインスタンスを作成 */
            speedBuff = new TechC.SpeedBuff();
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.T))
            {
                /* バフを適用する */
                speedBuff.Apply(target);

                /* できているかどうかの確認用 */
                CharacterController cc = target.GetComponent<CharacterController>();
            if (cc != null)
            {
                if (!isBuffApplied)
                {
                    speedBuff.Apply(target);
                    isBuffApplied = true;
                    Debug.Log("SpeedBuff 適用！");
                }
                else
                {
                    speedBuff.Remove(target);
                    isBuffApplied = false;
                    Debug.Log("SpeedBuff 解除！");
                }

                // 現在の移動速度を表示（GroundかAirどちらかのスピード）
                float baseSpeed = cc.GetCharacterData().MoveSpeed;
                float actualSpeed = baseSpeed * (isBuffApplied ? 1.5f : 1.0f);
                Debug.Log($"現在のスピード: {actualSpeed}");
            }
        }
    }
}
