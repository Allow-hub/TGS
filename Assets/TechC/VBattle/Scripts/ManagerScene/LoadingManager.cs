using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

namespace TechC
{
    public class LoadingManager : MonoBehaviour
    {
        [SerializeField] private Image progressImage;
        [SerializeField] private TextMeshProUGUI loadTex;
        private static int targetSceneIndex = -1;
        public async UniTask UpdateProgressAsync(AsyncOperation op)
        {
            op.allowSceneActivation = false;

            float displayedProgress = 0f;

            while (op.progress < 0.9f)
            {
                // 実際のロード進捗（0〜0.9）
                float target = Mathf.Clamp01(op.progress / 0.9f);

                // 補間（Lerp で徐々に表示値を更新）
                displayedProgress = Mathf.MoveTowards(displayedProgress, target, Time.deltaTime * 2f);

                if (progressImage != null)
                    progressImage.fillAmount = displayedProgress;

                if (loadTex != null)
                    loadTex.text = $"{(displayedProgress * 100f):0}%";

                await UniTask.Yield();
            }

            // 最後は100%に
            if (progressImage != null)
                progressImage.fillAmount = 1f;
            if (loadTex != null)
                loadTex.text = "100%";

            await UniTask.Delay(500);
        }

        /// <summary>
        /// 実際にターゲットシーンを読み込む処理
        /// </summary>
        public async UniTask LoadTargetSceneAsync()
        {
            var op = SceneManager.LoadSceneAsync(targetSceneIndex, LoadSceneMode.Single);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                if (progressImage != null)
                {
                    progressImage.fillAmount = Mathf.Clamp01(op.progress / 0.9f);
                }
                await UniTask.Yield();
            }

            if (progressImage != null)
                progressImage.fillAmount = 1f;

            await UniTask.Delay(500);
            op.allowSceneActivation = true;
        }
    }
}
