using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        protected virtual bool DestroyTargetGameObject => false;

        public static T I { get; private set; } = null;

        // Singletonが有効か
        public static bool IsValid() => I != null;

        private void Awake()
        {
            if (I == null)
            {
                I = this as T;
                I.Init();
            }
            else
            {
                if (DestroyTargetGameObject)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(this);
                }
            }
        }

        private void OnDestroy()
        {
            if (I == this)
            {
                I = null;
                OnRelease();
            }
        }

        // 派生クラス用の初期化メソッド
        protected virtual void Init()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        // 派生クラス用のOnDestroy
        protected virtual void OnRelease()
        {
        }

    }
}