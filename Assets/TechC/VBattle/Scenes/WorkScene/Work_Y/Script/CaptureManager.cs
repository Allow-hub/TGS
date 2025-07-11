using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;

namespace TechC
{
    /// <summary>
    /// 統合キャプチャシステム（最終版）
    /// スクリーンショットとカスタムカメラ撮影の両方に対応
    /// 背景切り替え、VRMプレイヤー中心撮影、SNSサイズ対応
    /// </summary>
    public class CaptureManager : MonoBehaviour
    {
        [Header("キャプチャ設定")]
        [SerializeField] private CaptureMode captureMode = CaptureMode.FullScreen;
        [SerializeField] private RectTransform captureArea; // カスタム範囲用
        [SerializeField] private Button captureButton;
        
        [Header("カメラ撮影設定")]
        [SerializeField] private Camera targetCamera; // 撮影用カメラ
        [SerializeField] private bool useCustomCamera = true; // カメラ撮影を使用するか
        
        [Header("VRM/Player設定")]
        [SerializeField] private Transform playerTransform; // VRMプレイヤー
        [SerializeField] private Vector3 playerCenterOffset = Vector3.zero; // プレイヤーの中心オフセット
        [SerializeField] private float cameraDistance = 3f; // カメラとプレイヤーの距離
        
        [Header("背景管理")]
        [SerializeField] private PhotoBackgroundManager backgroundManager; // 背景管理システム
        
        [Header("レイヤー撮影設定")]
        [SerializeField] private GameObject[] foregroundObjects; // 前景オブジェクト
        [SerializeField] private GameObject[] backgroundObjects; // 背景オブジェクト
        [SerializeField] private bool useMainCamera = true; // MainCameraを使用するか
        
        [Header("保存設定")]
        [SerializeField] private string saveFileName = "Screenshot";
        [SerializeField] private bool addTimestamp = true;
        private string saveDirectory = "TechC/VBattle/Scenes/WorkScene/Work_Y/Screenshots"; // 保存ディレクトリ

        void Start()
        {
            if (captureButton != null)
            {
                captureButton.onClick.AddListener(Capture);
            }
            
            // MainCameraを使用する場合、自動取得
            if (useMainCamera && targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        /// <summary>
        /// キャプチャモードを設定する（UI等から呼び出し用）
        /// </summary>
        /// <param name="m">CaptureMode enum値</param>
        public void SetCaptureMode(int m)
        {
            captureMode = (CaptureMode)m;
            
            // カメラ位置を調整
            if (useCustomCamera && targetCamera != null)
            {
                PositionCameraForPlayer();
            }
        }

        /// <summary>
        /// 次の背景に変更
        /// </summary>
        public void NextBackground()
        {
            if (backgroundManager != null)
            {
                backgroundManager.NextBackground();
            }
        }

        /// <summary>
        /// 前の背景に変更
        /// </summary>
        public void PreviousBackground()
        {
            if (backgroundManager != null)
            {
                backgroundManager.PreviousBackground();
            }
        }

        /// <summary>
        /// 指定した背景に変更
        /// </summary>
        /// <param name="index">背景のインデックス</param>
        public void SetBackground(int index)
        {
            if (backgroundManager != null)
            {
                backgroundManager.ShowBackground(index);
            }
        }

        /// <summary>
        /// プレイヤーを中心にカメラを配置
        /// </summary>
        private void PositionCameraForPlayer()
        {
            if (playerTransform == null || targetCamera == null) return;
            
            Vector3 playerCenter = playerTransform.position + playerCenterOffset;
            Vector3 cameraPosition = playerCenter + Vector3.back * cameraDistance;
            
            targetCamera.transform.position = cameraPosition;
            targetCamera.transform.LookAt(playerCenter);
        }

        /// <summary>
        /// 前景オブジェクトの表示/非表示を制御
        /// </summary>
        /// <param name="visible">表示するかどうか</param>
        public void SetForegroundVisible(bool visible)
        {
            if (foregroundObjects != null)
            {
                foreach (GameObject obj in foregroundObjects)
                {
                    if (obj != null)
                        obj.SetActive(visible);
                }
            }
        }

        /// <summary>
        /// 背景オブジェクトの表示/非表示を制御
        /// </summary>
        /// <param name="visible">表示するかどうか</param>
        public void SetBackgroundVisible(bool visible)
        {
            if (backgroundObjects != null)
            {
                foreach (GameObject obj in backgroundObjects)
                {
                    if (obj != null)
                        obj.SetActive(visible);
                }
            }
        }

        /// <summary>
        /// レイヤー撮影用のセットアップ
        /// </summary>
        private void SetupLayersForCapture()
        {
            Debug.Log("レイヤーセットアップ開始");
            
            // 前景を表示
            SetForegroundVisible(true);
            Debug.Log($"前景オブジェクト表示: {foregroundObjects?.Length ?? 0}個");
            
            // 背景を表示
            SetBackgroundVisible(true);
            Debug.Log($"背景オブジェクト表示: {backgroundObjects?.Length ?? 0}個");
            
            // プレイヤーを中心にカメラを配置
            PositionCameraForPlayer();
            Debug.Log($"カメラ位置設定完了: {targetCamera?.transform.position}");
        }

        /// <summary>
        /// キャプチャを実行
        /// </summary>
        public void Capture()
        {
            // MainCameraを使用する場合、自動取得
            if (useMainCamera && targetCamera == null)
            {
                targetCamera = Camera.main;
                Debug.Log($"MainCameraを自動取得: {targetCamera?.name}");
            }
            
            Debug.Log($"撮影開始 - UseCustomCamera: {useCustomCamera}, TargetCamera: {targetCamera?.name}");
            Debug.Log($"CaptureMode: {captureMode}");
            
            if (useCustomCamera && targetCamera != null)
            {
                StartCoroutine(CaptureWithCamera());
            }
            else
            {
                StartCoroutine(CaptureScreenshot());
            }
        }

        /// <summary>
        /// カメラを使用した撮影
        /// </summary>
        IEnumerator CaptureWithCamera()
        {
            yield return new WaitForEndOfFrame();
            
            Debug.Log("カメラ撮影処理開始");
            
            // レイヤー撮影のセットアップ
            SetupLayersForCapture();
            
            Vector2Int size = GetCaptureSize();
            Debug.Log($"撮影サイズ: {size.x}x{size.y}");
            
            // RenderTextureを作成
            RenderTexture rt = new RenderTexture(size.x, size.y, 24);
            RenderTexture previousRT = targetCamera.targetTexture;
            
            targetCamera.targetTexture = rt;
            targetCamera.Render();
            
            // ピクセルデータを取得
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
            tex.Apply();
            
            // 保存
            SaveTexture(tex);
            
            // 後片付け
            targetCamera.targetTexture = previousRT;
            RenderTexture.active = null;
            Destroy(rt);
            Destroy(tex);
            
            Debug.Log("レイヤー撮影完了！");
        }

        /// <summary>
        /// スクリーンショット撮影
        /// </summary>
        IEnumerator CaptureScreenshot()
        {
            yield return new WaitForEndOfFrame();

            // 画面全体をキャプチャ
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();

            // カスタム範囲モードの場合、切り抜き処理
            if (captureMode == CaptureMode.CustomArea && captureArea != null)
            {
                tex = CropTexture(tex, captureArea);
            }
            else if (IsSNSMode(captureMode))
            {
                // SNSサイズにリサイズ
                Vector2Int targetSize = CaptureSizes.GetSize(captureMode);
                tex = ResizeTexture(tex, targetSize);
            }

            SaveTexture(tex);
            Destroy(tex);

            Debug.Log("スクリーンショット保存完了！");
        }

        /// <summary>
        /// キャプチャサイズを取得
        /// </summary>
        private Vector2Int GetCaptureSize()
        {
            if (IsSNSMode(captureMode))
            {
                return CaptureSizes.GetSize(captureMode);
            }
            return new Vector2Int(Screen.width, Screen.height);
        }

        /// <summary>
        /// SNSモードかどうかを判定
        /// </summary>
        private bool IsSNSMode(CaptureMode mode)
        {
            return mode == CaptureMode.TwitterSize || 
                   mode == CaptureMode.InstagramSize || 
                   mode == CaptureMode.FacebookSize;
        }

        /// <summary>
        /// テクスチャを切り抜く
        /// </summary>
        private Texture2D CropTexture(Texture2D original, RectTransform area)
        {
            Vector2 pos = area.position;
            Vector2 size = area.sizeDelta;
            Rect rect = new Rect(pos.x, pos.y, size.x, size.y);

            Texture2D cropped = new Texture2D((int)size.x, (int)size.y, TextureFormat.RGB24, false);
            cropped.SetPixels(original.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
            cropped.Apply();

            Destroy(original);
            return cropped;
        }

        /// <summary>
        /// テクスチャをリサイズ
        /// </summary>
        private Texture2D ResizeTexture(Texture2D original, Vector2Int targetSize)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetSize.x, targetSize.y);
            Graphics.Blit(original, rt);
            
            RenderTexture.active = rt;
            Texture2D resized = new Texture2D(targetSize.x, targetSize.y, TextureFormat.RGB24, false);
            resized.ReadPixels(new Rect(0, 0, targetSize.x, targetSize.y), 0, 0);
            resized.Apply();
            
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            Destroy(original);
            
            return resized;
        }

        /// <summary>
        /// テクスチャを保存
        /// </summary>
        private void SaveTexture(Texture2D tex)
        {
            byte[] bytes = tex.EncodeToPNG();
            string fileName = saveFileName;
            
            if (addTimestamp)
            {
                fileName += "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            }
            
            string modeText = captureMode.ToString();
            fileName += "_" + modeText + ".png";
            
            // 保存ディレクトリを作成
            string directoryPath = Path.Combine(Application.dataPath, saveDirectory);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"ディレクトリ作成: {directoryPath}");
            }
            
            string path = Path.Combine(directoryPath, fileName);
            File.WriteAllBytes(path, bytes);
            
            Debug.Log($"保存完了: {path}");
            Debug.Log($"ファイルサイズ: {bytes.Length} bytes");
            Debug.Log($"テクスチャサイズ: {tex.width}x{tex.height}");
            
            // Windows Explorerで表示
            System.Diagnostics.Process.Start("explorer.exe", "/select," + path.Replace('/', '\\'));
        }

        /// <summary>
        /// カメラ撮影モードの切り替え
        /// </summary>
        public void SetUseCustomCamera(bool use)
        {
            useCustomCamera = use;
        }

        /// <summary>
        /// MainCamera使用モードの切り替え
        /// </summary>
        public void SetUseMainCamera(bool use)
        {
            useMainCamera = use;
            if (useMainCamera)
            {
                targetCamera = Camera.main;
            }
        }

        /// <summary>
        /// 前景と背景のテスト表示
        /// </summary>
        public void TestLayerVisibility()
        {
            Debug.Log($"前景オブジェクト数: {foregroundObjects?.Length ?? 0}");
            Debug.Log($"背景オブジェクト数: {backgroundObjects?.Length ?? 0}");
            Debug.Log($"MainCamera使用: {useMainCamera}");
            Debug.Log($"撮影カメラ: {targetCamera?.name ?? "null"}");
        }

        /// <summary>
        /// 撮影テスト（デバッグ用）
        /// </summary>
        [ContextMenu("撮影テスト")]
        public void TestCapture()
        {
            Debug.Log("=== 撮影テスト開始 ===");
            TestLayerVisibility();
            Capture();
        }

        /// <summary>
        /// 設定状態の確認
        /// </summary>
        [ContextMenu("設定確認")]
        public void CheckSettings()
        {
            Debug.Log("=== 設定確認 ===");
            Debug.Log($"Capture Mode: {captureMode}");
            Debug.Log($"Use Custom Camera: {useCustomCamera}");
            Debug.Log($"Use Main Camera: {useMainCamera}");
            Debug.Log($"Target Camera: {targetCamera?.name ?? "null"}");
            Debug.Log($"Player Transform: {playerTransform?.name ?? "null"}");
            Debug.Log($"Save Directory: {saveDirectory}");
            Debug.Log($"Foreground Objects: {foregroundObjects?.Length ?? 0}");
            Debug.Log($"Background Objects: {backgroundObjects?.Length ?? 0}");
        }
    }
}
