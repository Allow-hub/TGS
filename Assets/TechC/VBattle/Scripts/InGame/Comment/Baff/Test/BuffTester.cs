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
            /* バフの更新 */
            if(isBuffApplied)
            {
                speedBuff.UpdateBuff(Time.deltaTime, target);

                if (speedBuff.remainingTime <= 0)
                {
                    isBuffApplied = false;
                    Debug.Log("SpeedBuff 解除！");
                    var characterController = target.GetComponent<Player.CharacterController>();
                if (characterController != null)
                {
                    float baseSpeed = characterController.GetCharacterData().MoveSpeed;
                    Debug.Log($"現在のスピード: {baseSpeed}");
                }
                }
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                speedBuff.ResetDuration();
                speedBuff.Apply(target);
                isBuffApplied = true;
                Debug.Log("SpeedBuffを適用");
            }
        }
    }
}
