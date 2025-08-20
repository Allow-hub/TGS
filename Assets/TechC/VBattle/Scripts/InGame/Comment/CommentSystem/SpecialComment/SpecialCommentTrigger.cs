using System.Collections.Generic;
using UnityEngine;

namespace TechC.CommentSystem
{
    /// <summary>
    /// 特殊コメントの当たり判定を取り、Inspectorで設定した能力リストを実行するクラス
    /// </summary>
    public class SpecialCommentTrigger : MonoBehaviour
    {
        [SerializeReference]
        public List<ICommentAbility> abilities = new();

        private void Awake()
        {
            // abilitiesの各要素にInitを呼ぶ
            foreach (var ability in abilities)
            {
                ability?.Init(this);
                if (ability is HoldAbility hold)
                {
                    hold.Init(this);
                }
                var hold2 = ability as HoldAbility;
                hold2.Init(this);
            }
        }

        private void OnDestroy()
        {
            // abilitiesの各要素にReleaseを呼ぶ
            foreach (var ability in abilities)
            {
                ability?.Release();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            foreach (var ability in abilities)
            {
                ability?.OnTriggerEnter(other);
            }
        }
    }
}
