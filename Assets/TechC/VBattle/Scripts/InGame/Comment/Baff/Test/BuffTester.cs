using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class BuffTester : MonoBehaviour
    {
        private BuffManager buffManager;
        private SpeedBuff speedBuff;
        private GameObject target;
        private bool isBuffApplied = false;
        void Start()
        {
            /* BuffManagerを取得 */
            buffManager = GetComponent<BuffManager>();
            /* Player自身が対象 */
            target = gameObject;

            /* SpeedBuffのインスタンスを作成 */
            speedBuff = new SpeedBuff();
        }

        void Update()
        {
            /* バフの更新 */
            if (isBuffApplied)
            {
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
                buffManager.ApplyBuff(speedBuff);
                isBuffApplied = true;
                Debug.Log("SpeedBuffを適用");
            }
        }
    }
}
