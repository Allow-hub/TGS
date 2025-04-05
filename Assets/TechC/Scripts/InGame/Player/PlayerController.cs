using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [Header("Reference")]
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private CharacterState characterState;
        [Header("攻撃コンポーネント")]
        [SerializeField] private WeakAttack weakAttack;
        [SerializeField] private StrongAttack strongAttack;
        [SerializeField] private AttackSet attackSet;


        [SerializeField] private float maxGage = 100;

        [SerializeField] private float rayLength = 0.1f;
        [SerializeField] private bool isDrawingRay;
        private Rigidbody rb;
        private float currentHp;


        private void Awake()
        {
            // Reflectionを使ってprivateフィールドにアクセスする
            var attackManagerField = typeof(CharacterState).GetField("attackManager",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);

            if (attackManagerField != null)
            {
                var attackManager = attackManagerField.GetValue(characterState) as AttackManager;
                attackManager?.Initialize(weakAttack, strongAttack, playerInputManager, this);
            }
            SetupAttackData();
        }
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            currentHp = playerData.Hp;

        }

        private void Update()
        {
        }

        public void AddForcePlayer(Vector3 dir, float force, ForceMode forceMode)
            => rb.AddForce(dir * force, forceMode);

        public float GetHp() => currentHp;


        public void TakeDamage(float damage)
        {
            currentHp -= damage;
            if (currentHp > 0) return;
            currentHp = 0;
            Des();
        }
        public void Des()
        {

        }
        private void SetupAttackData()
        {
            if (attackSet == null) return;

            // 弱攻撃のデータを設定
            if (weakAttack != null)
            {
                typeof(WeakAttack).GetField("neutralAttackData",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)?.SetValue(weakAttack, attackSet.weakNeutral);

                typeof(WeakAttack).GetField("leftAttackData",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)?.SetValue(weakAttack, attackSet.weakLeft);
                typeof(WeakAttack).GetField("rightAttackData",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)?.SetValue(weakAttack, attackSet.weakRight);

                typeof(WeakAttack).GetField("downAttackData",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)?.SetValue(weakAttack, attackSet.weakDown);
                typeof(WeakAttack).GetField("upAttackData",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)?.SetValue(weakAttack, attackSet.weakUp);
            }

            // 強攻撃のデータを設定
            if (strongAttack != null)
            {
                typeof(StrongAttack).GetField("neutralAttackData",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)?.SetValue(strongAttack, attackSet.strongNeutral);

                typeof(StrongAttack).GetField("leftAttackData",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)?.SetValue(strongAttack, attackSet.strongLeft);
                typeof(StrongAttack).GetField("rightAttackData",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)?.SetValue(strongAttack, attackSet.strongRight);

                typeof(StrongAttack).GetField("downAttackData",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)?.SetValue(strongAttack, attackSet.strongDown);
                typeof(StrongAttack).GetField("upAttackData",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)?.SetValue(strongAttack, attackSet.strongUp);
            }


        }
        public bool IsGrounded()
        {
            Vector3 rayOrigin = transform.position + Vector3.up;
            RaycastHit hit;

            return Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, LayerMask.GetMask("Ground"));
        }
        private void OnDrawGizmos()
        {
            if (!isDrawingRay) return;
            // レイの発射位置
            Vector3 rayOrigin = transform.position + Vector3.up;

            // 地面に当たるときは緑、そうでないときは赤
            Gizmos.color = IsGrounded() ? Color.green : Color.red;

            // レイの描画
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * rayLength);

            // レイの終端に球を表示
            Gizmos.DrawSphere(rayOrigin + Vector3.down * rayLength, 0.05f);
        }
    }
}
