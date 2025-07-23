using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player.Attack
{
    /// <summary>
    /// 各攻撃のオブジェクトの管理クラス
    /// それぞれの機能を組み立てて実行する
    /// </summary>
    public class AttackObjectController : MonoBehaviour
    {
        [SerializeReference] private List<IAttackBehaviour> behaviours;
        private string playerTag = "Player";

        private void Start()
        {
            if (behaviours == null) return;

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;
                behaviour?.Initialize(gameObject);
            }
        }

        private void OnDisable()
        {
            if (behaviours == null) return;

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;
                behaviour?.OnRelease();
            }
        }

        private void Update()
        {
            if (behaviours == null) return;

            float delta = Time.deltaTime;
            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;
                behaviour?.OnUpdate(delta);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(playerTag))
            {
                if (behaviours == null) return;
                foreach (var behaviour in behaviours)
                {
                    if (behaviour == null) continue;
                    behaviour?.OnTriggerEnter(other);
                }
            }
        }
    }
}