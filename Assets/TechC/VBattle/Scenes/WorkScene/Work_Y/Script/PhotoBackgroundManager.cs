using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 写真撮影用背景管理システム
    /// 複数の背景GameObjectを管理し、切り替えを行う
    /// </summary>
    public class PhotoBackgroundManager : MonoBehaviour
    {
        [Header("背景設定")]
        [SerializeField] private GameObject[] backgrounds;
        [SerializeField] private int currentIndex = 0;

        void Start()
        {
            if (backgrounds.Length > 0)
            {
                ShowBackground(currentIndex);
            }
        }

        /// <summary>
        /// 指定したインデックスの背景を表示
        /// </summary>
        /// <param name="index">表示する背景のインデックス</param>
        public void ShowBackground(int index)
        {
            if (backgrounds == null || backgrounds.Length == 0) return;
            
            // インデックスをクランプ
            index = Mathf.Clamp(index, 0, backgrounds.Length - 1);
            
            // 全ての背景を非表示にして、指定したもののみ表示
            for (int i = 0; i < backgrounds.Length; i++)
            {
                if (backgrounds[i] != null)
                {
                    backgrounds[i].SetActive(i == index);
                }
            }
            
            currentIndex = index;
            Debug.Log($"背景を変更: {index} ({backgrounds[index]?.name})");
        }

        /// <summary>
        /// 次の背景に切り替え
        /// </summary>
        public void NextBackground()
        {
            if (backgrounds == null || backgrounds.Length == 0) return;
            
            int nextIndex = (currentIndex + 1) % backgrounds.Length;
            ShowBackground(nextIndex);
        }

        /// <summary>
        /// 前の背景に切り替え
        /// </summary>
        public void PreviousBackground()
        {
            if (backgrounds == null || backgrounds.Length == 0) return;
            
            int prevIndex = (currentIndex - 1 + backgrounds.Length) % backgrounds.Length;
            ShowBackground(prevIndex);
        }

        /// <summary>
        /// 現在の背景インデックスを取得
        /// </summary>
        public int GetCurrentIndex()
        {
            return currentIndex;
        }

        /// <summary>
        /// 背景の総数を取得
        /// </summary>
        public int GetBackgroundCount()
        {
            return backgrounds?.Length ?? 0;
        }

        /// <summary>
        /// 指定したインデックスの背景名を取得
        /// </summary>
        /// <param name="index">背景のインデックス</param>
        /// <returns>背景名</returns>
        public string GetBackgroundName(int index)
        {
            if (backgrounds == null || index < 0 || index >= backgrounds.Length)
            {
                return "Invalid";
            }
            
            return backgrounds[index]?.name ?? "Null";
        }
    }
}
