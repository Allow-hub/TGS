using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TechC
{
    public class FlyingSlashAttack : MonoBehaviour
    {
        public GameObject FlyingIceSlash; // プレハブをInspectorでセット
        public float slashSpeed = 10f;

        void Start()
        {
            // FlyingIceSlashがセットされていない場合はエラーメッセージを表示
            FlyingIceSlash.SetActive(false);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ShootSlash();
            }
        }

        void ShootSlash()
        {
            if (FlyingIceSlash != null)
            {
                // プレハブから新しい斬撃を生成
                GameObject slash = Instantiate(FlyingIceSlash, transform.position, transform.rotation);
                slash.SetActive(true);
                Rigidbody rb = slash.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = false;
                    rb.velocity = slash.transform.right * slashSpeed;
                }
            }
        }
    }
}
