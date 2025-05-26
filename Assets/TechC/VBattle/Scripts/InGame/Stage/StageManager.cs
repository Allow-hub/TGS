using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ステージを管理するマネージャー
    /// ステージスプライトの変更とCameraManagerの設定調整を行う
    /// </summary>
    public class StageManager : Singleton<StageManager>
    {
        private const string LOGTAG = "stage";
        
        [Header("ステージ設定")]
        [SerializeField] private SpriteRenderer stageRenderer;
        [SerializeField] private StageData[] stageDataList; // 利用可能なステージのリスト
        [SerializeField] private int currentStageIndex = 0;
        
        // 現在のステージデータ
        private StageData currentStageData;
        
        // イベント
        public System.Action<StageData> OnStageChanged;
        
        // プロパティ
        public StageData CurrentStage => currentStageData;
        public int CurrentStageIndex => currentStageIndex;
        public int StageCount => stageDataList?.Length ?? 0;
        public string CurrentStageName => currentStageData?.stageName ?? "No Stage";
        
        protected override bool UseDontDestroyOnLoad => false;

        // protected override InitPhase GetInitPhase() => InitPhase.Late;
        protected override void Init()
        {
            base.Init();
            InitializeStage();
        }
        
        /// <summary>
        /// ステージの初期化
        /// </summary>
        private void InitializeStage()
        {
            // StageRendererが設定されていない場合は自動検索
            if (stageRenderer == null)
            {
                stageRenderer = FindObjectOfType<SpriteRenderer>();
                if (stageRenderer == null)
                {
                    CustomLogger.Warning("SpriteRendererが見つかりません。ステージスプライトの変更ができません。", LOGTAG);
                }
            }
            
            // 現在のステージを適用
            if (stageDataList != null && stageDataList.Length > 0)
            {
                currentStageIndex = Mathf.Clamp(currentStageIndex, 0, stageDataList.Length - 1);
                ApplyStageData(stageDataList[currentStageIndex]);
            }
            else
            {
                CustomLogger.Warning("ステージデータが設定されていません。", LOGTAG);
            }
        }
        
        /// <summary>
        /// ステージデータを適用する
        /// </summary>
        /// <param name="stageData">適用するステージデータ</param>
        public void ApplyStageData(StageData stageData)
        {
            if (stageData == null)
            {
                CustomLogger.Warning("ステージデータがnullです", LOGTAG);
                return;
            }
            
            currentStageData = stageData;
            
            // ステージスプライトを変更
            ApplyStageSprite(stageData);
            
            // カメラ設定をCameraManagerに反映
            ApplyCameraSettings(stageData);
            
            // ステージ変更イベントを発火
            OnStageChanged?.Invoke(stageData);
            
            CustomLogger.Info($"ステージ '{stageData.stageName}' を適用しました", LOGTAG);
        }
        
        /// <summary>
        /// ステージスプライトを適用
        /// </summary>
        private void ApplyStageSprite(StageData stageData)
        {
            if (stageRenderer == null)
            {
                CustomLogger.Warning("SpriteRendererが設定されていません", LOGTAG);
                return;
            }
            
            if (stageData.stageSprite != null)
            {
                stageRenderer.sprite = stageData.stageSprite;
                CustomLogger.Info($"ステージスプライトを '{stageData.stageSprite.name}' に変更しました", LOGTAG);
            }
            else
            {
                CustomLogger.Warning($"ステージ '{stageData.stageName}' にスプライトが設定されていません", LOGTAG);
            }
        }
        
        /// <summary>
        /// カメラ設定をCameraManagerに適用
        /// </summary>
        private void ApplyCameraSettings(StageData stageData)
        {
            if (CameraManager.I == null)
            {
                CustomLogger.Warning("CameraManagerが見つかりません", LOGTAG);
                return;
            }
            
            // ズーム設定の適用
            if (stageData.overrideZoomSettings)
            {
                CameraManager.I.SetZoomSettings(
                    stageData.minFOV, 
                    stageData.maxFOV, 
                    stageData.minCameraDistance, 
                    stageData.maxCameraDistance
                );
                CustomLogger.Info($"カメラのズーム設定を変更しました (FOV: {stageData.minFOV}-{stageData.maxFOV}, Distance: {stageData.minCameraDistance}-{stageData.maxCameraDistance})", LOGTAG);
            }
            
            // ステージ境界の適用
            if (stageData.useCustomBounds)
            {
                CameraManager.I.SetStageBounds(stageData.customBounds);
                CustomLogger.Info($"ステージ境界を設定しました (Center: {stageData.customBounds.center}, Size: {stageData.customBounds.size})", LOGTAG);
            }
            
            // カメラオフセットの適用（CameraManagerに対応メソッドがある場合）
            if (stageData.overrideCameraPosition)
            {
                SetCameraOffset(stageData.cameraOffset);
                SetCameraDeadZone(stageData.cameraDeadZone);
            }
        }
        
        /// <summary>
        /// カメラオフセットを設定（CameraManagerの拡張が必要）
        /// </summary>
        private void SetCameraOffset(Vector3 offset)
        {
            // TODO: CameraManagerにSetCameraOffsetメソッドを追加する必要があります
            CustomLogger.Info($"カメラオフセット設定 {offset} (未実装)", LOGTAG);
        }
        
        /// <summary>
        /// カメラデッドゾーンを設定（CameraManagerの拡張が必要）
        /// </summary>
        private void SetCameraDeadZone(Vector2 deadZone)
        {
            // TODO: CameraManagerにSetDeadZoneメソッドを追加する必要があります
            CustomLogger.Info($"カメラデッドゾーン設定 {deadZone} (未実装)", LOGTAG);
        }
        
        #region パブリックメソッド
        
        /// <summary>
        /// ステージを変更する（インデックス指定）
        /// </summary>
        /// <param name="stageIndex">ステージインデックス</param>
        public void ChangeStage(int stageIndex)
        {
            if (stageDataList == null || stageDataList.Length == 0)
            {
                CustomLogger.Warning("ステージデータが設定されていません", LOGTAG);
                return;
            }
            
            if (stageIndex < 0 || stageIndex >= stageDataList.Length)
            {
                CustomLogger.Warning($"無効なステージインデックス {stageIndex} (有効範囲: 0-{stageDataList.Length - 1})", LOGTAG);
                return;
            }
            
            if (stageIndex == currentStageIndex)
            {
                CustomLogger.Info($"既に同じステージ（インデックス {stageIndex}）が選択されています", LOGTAG);
                return;
            }
            
            currentStageIndex = stageIndex;
            ApplyStageData(stageDataList[stageIndex]);
        }
        
        /// <summary>
        /// ステージを変更する（ステージデータ直接指定）
        /// </summary>
        /// <param name="stageData">適用するステージデータ</param>
        public void ChangeStage(StageData stageData)
        {
            if (stageData == null)
            {
                CustomLogger.Warning("ステージデータがnullです", LOGTAG);
                return;
            }
            
            // 配列内のインデックスを検索
            int foundIndex = -1;
            if (stageDataList != null)
            {
                for (int i = 0; i < stageDataList.Length; i++)
                {
                    if (stageDataList[i] == stageData)
                    {
                        foundIndex = i;
                        break;
                    }
                }
            }
            
            if (foundIndex >= 0)
            {
                currentStageIndex = foundIndex;
            }
            
            ApplyStageData(stageData);
        }
        
        /// <summary>
        /// 次のステージに変更
        /// </summary>
        public void NextStage()
        {
            if (stageDataList == null || stageDataList.Length == 0) 
            {
                CustomLogger.Warning("ステージデータが設定されていません", LOGTAG);
                return;
            }
            
            int nextIndex = (currentStageIndex + 1) % stageDataList.Length;
            ChangeStage(nextIndex);
        }
        
        /// <summary>
        /// 前のステージに変更
        /// </summary>
        public void PreviousStage()
        {
            if (stageDataList == null || stageDataList.Length == 0)
            {
                CustomLogger.Warning("ステージデータが設定されていません", LOGTAG);
                return;
            }
            
            int prevIndex = currentStageIndex - 1;
            if (prevIndex < 0) prevIndex = stageDataList.Length - 1;
            ChangeStage(prevIndex);
        }
        
        /// <summary>
        /// ランダムなステージに変更
        /// </summary>
        public void RandomStage()
        {
            if (stageDataList == null || stageDataList.Length <= 1) 
            {
                CustomLogger.Warning("ランダム選択できるステージが不足しています", LOGTAG);
                return;
            }
            
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, stageDataList.Length);
            } while (randomIndex == currentStageIndex);
            
            ChangeStage(randomIndex);
        }
        
        /// <summary>
        /// ステージ名でステージを検索して変更
        /// </summary>
        /// <param name="stageName">ステージ名</param>
        public void ChangeStageByName(string stageName)
        {
            if (string.IsNullOrEmpty(stageName))
            {
                CustomLogger.Warning("ステージ名が空です", LOGTAG);
                return;
            }
            
            if (stageDataList == null)
            {
                CustomLogger.Warning("ステージデータが設定されていません", LOGTAG);
                return;
            }
            
            for (int i = 0; i < stageDataList.Length; i++)
            {
                if (stageDataList[i] != null && stageDataList[i].stageName == stageName)
                {
                    ChangeStage(i);
                    return;
                }
            }
            
            CustomLogger.Warning($"ステージ '{stageName}' が見つかりません", LOGTAG);
        }
        /// <summary>
        /// 利用可能なステージ名のリストを取得
        /// </summary>
        public string[] GetStageNames()
        {
            if (stageDataList == null || stageDataList.Length == 0) 
                return new string[] { "No Stages Available" };
            
            string[] names = new string[stageDataList.Length];
            for (int i = 0; i < stageDataList.Length; i++)
            {
                names[i] = stageDataList[i]?.stageName ?? $"Stage {i}";
            }
            return names;
        }
        
        /// <summary>
        /// ステージレンダラーを手動で設定
        /// </summary>
        /// <param name="renderer">使用するSpriteRenderer</param>
        public void SetStageRenderer(SpriteRenderer renderer)
        {
            stageRenderer = renderer;
            CustomLogger.Info($"StageRendererを {renderer?.name} に設定しました", LOGTAG);
        }
        
        /// <summary>
        /// ステージデータリストを動的に設定
        /// </summary>
        /// <param name="stages">新しいステージデータリスト</param>
        public void SetStageDataList(StageData[] stages)
        {
            stageDataList = stages;
            currentStageIndex = 0;
            
            if (stages != null && stages.Length > 0)
            {
                ApplyStageData(stages[0]);
            }
            
            CustomLogger.Info($"{stages?.Length ?? 0} 個のステージデータを設定しました", LOGTAG);
        }
        
        /// <summary>
        /// ステージデータを追加
        /// </summary>
        /// <param name="stageData">追加するステージデータ</param>
        public void AddStageData(StageData stageData)
        {
            if (stageData == null) return;
            
            if (stageDataList == null)
            {
                stageDataList = new StageData[] { stageData };
            }
            else
            {
                var newList = new StageData[stageDataList.Length + 1];
                System.Array.Copy(stageDataList, newList, stageDataList.Length);
                newList[stageDataList.Length] = stageData;
                stageDataList = newList;
            }
            
            CustomLogger.Info($"ステージ '{stageData.stageName}' を追加しました", LOGTAG);
        }
        
        #endregion
        
        #region デバッグ用
        
        private void OnDrawGizmosSelected()
        {
            // 現在のステージの境界を描画
            if (currentStageData != null && currentStageData.useCustomBounds)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(currentStageData.customBounds.center, currentStageData.customBounds.size);
                
                // ラベルを表示
                UnityEditor.Handles.Label(
                    currentStageData.customBounds.center, 
                    $"Stage: {currentStageData.stageName}\nBounds: {currentStageData.customBounds.size}"
                );
            }
            
            // カメラオフセットとデッドゾーンを描画
            if (currentStageData != null && currentStageData.overrideCameraPosition)
            {
                Vector3 centerPos = transform.position + currentStageData.cameraOffset;
                
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(centerPos, 0.5f);
                
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(centerPos, new Vector3(currentStageData.cameraDeadZone.x, currentStageData.cameraDeadZone.y, 1f));
            }
        }
        
        /// <summary>
        /// デバッグ情報を表示
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogDebugInfo()
        {
            CustomLogger.Info("=== StageManager Debug Info ===", LOGTAG);
            CustomLogger.Info($"Current Stage: {CurrentStageName} (Index: {currentStageIndex})", LOGTAG);
            CustomLogger.Info($"Total Stages: {StageCount}", LOGTAG);
            CustomLogger.Info($"Stage Renderer: {(stageRenderer != null ? stageRenderer.name : "Not Set")}", LOGTAG);
            CustomLogger.Info($"Camera Manager: {(CameraManager.I != null ? "Available" : "Not Found")}", LOGTAG);
            
            if (currentStageData != null)
            {
                CustomLogger.Info($"Override Zoom: {currentStageData.overrideZoomSettings}", LOGTAG);
                CustomLogger.Info($"Custom Bounds: {currentStageData.useCustomBounds}", LOGTAG);
                CustomLogger.Info($"Override Camera Position: {currentStageData.overrideCameraPosition}", LOGTAG);
            }
        }
        
        #endregion
    }
}