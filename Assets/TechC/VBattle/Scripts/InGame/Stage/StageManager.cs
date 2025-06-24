using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class StageManager : Singleton<StageManager>
    {
        private const string LOGTAG = "stage";

        [Header("ステージ設定")]
        [SerializeField] private SpriteRenderer stageRenderer;
        [SerializeField] private StageData[] stageDataList;
        [SerializeField] private int currentStageIndex = 0;

        private StageData currentStageData;

        public System.Action<StageData> OnStageChanged;

        public StageData CurrentStage => currentStageData;
        public int CurrentStageIndex => currentStageIndex;
        public int StageCount => stageDataList?.Length ?? 0;
        public string CurrentStageName => currentStageData?.stageName ?? "No Stage";

        protected override bool UseDontDestroyOnLoad => false;

        protected override void Init()
        {
            base.Init();
            DelayUtility.StartDelayedAction(this, 0.5f, () =>
            {
                InitializeStage();
            });
        }

        private void InitializeStage()
        {
            if (stageRenderer == null)
            {
                stageRenderer = FindObjectOfType<SpriteRenderer>();
                if (stageRenderer == null)
                {
                    CustomLogger.Warning("SpriteRendererが見つかりません。ステージスプライトの変更ができません。", LOGTAG);
                }
            }

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

        public void ApplyStageData(StageData stageData)
        {
            if (stageData == null)
            {
                CustomLogger.Warning("ステージデータがnullです", LOGTAG);
                return;
            }

            currentStageData = stageData;
            ApplyStageSprite(stageData);
            OnStageChanged?.Invoke(stageData);
            CustomLogger.Info($"ステージ '{stageData.stageName}' を適用しました", LOGTAG);
        }

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

                if (stageData.spriteScale != Vector2.zero)
                {
                    stageRenderer.transform.localScale = new Vector3(
                        stageData.spriteScale.x,
                        stageData.spriteScale.y,
                        1f
                    );
                }
            }
            else
            {
                CustomLogger.Warning($"ステージ '{stageData.stageName}' にスプライトが設定されていません", LOGTAG);
            }
        }

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

        public void ChangeStage(StageData stageData)
        {
            if (stageData == null)
            {
                CustomLogger.Warning("ステージデータがnullです", LOGTAG);
                return;
            }

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

        public void SetStageRenderer(SpriteRenderer renderer)
        {
            stageRenderer = renderer;
            CustomLogger.Info($"StageRendererを {renderer?.name} に設定しました", LOGTAG);
        }

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
    }
}
