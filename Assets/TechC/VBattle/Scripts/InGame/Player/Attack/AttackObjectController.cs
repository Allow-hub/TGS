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
        private int playerID;
        private GameObject character;
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

                var characterController = other.GetComponentInParent<CharacterController>();
                if (characterController == null) return;

                if (characterController.PlayerID == playerID) return;// 自分自身への接触は無視
                foreach (var behaviour in behaviours)
                {
                    if (behaviour == null) continue;
                    behaviour?.OnTriggerEnter(other);
                }
            }
        }

        public void SetPlayer(int id, GameObject characterObj)
        {
            if (id < 0) return; // 無効なIDは無視
            playerID = id;
            character = characterObj;
            if (behaviours == null) return;
            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;
                behaviour?.Activate(character);
            }
        }
    }
}