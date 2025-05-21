using UnityEngine;

namespace TechC
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        /// <summary>
        /// 派生クラスでこの値を変更して、DontDestroyOnLoad を使うかどうかを制御する
        /// </summary>
        protected virtual bool UseDontDestroyOnLoad => true;

        /// <summary>
        /// 重複時に GameObject ごと破壊するか（false だとこのコンポーネントだけ破壊）
        /// </summary>
        protected virtual bool DestroyTargetGameObject => false;

        public static T I { get; private set; } = null;

        public static bool IsValid() => I != null;

        private void Awake()
        {
            if (I == null)
            {
                I = this as T;
                I.Init();

                if (UseDontDestroyOnLoad)
                {
                    DontDestroyOnLoad(this.gameObject);
                }
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

        /// <summary>
        /// 派生クラス用の初期化メソッド
        /// </summary>
        protected virtual void Init() { }

        /// <summary>
        /// 派生クラス用の破棄処理
        /// </summary>
        protected virtual void OnRelease() { }
    }
}
