using System;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// タブのギミック管理
    /// </summary>
    [Serializable]
    public class TabGimickController : IGimmick
    {
        [SerializeField] private GameObject normalTabObj;
        private NormalTab normalTab;
        [SerializeField] private Vector2 intervalRange;
        private float currentInterval;
        private int tabTypeLength;
        private TabType currnetTabType;
        private float timer;
        public void OnEnter()
        {
            Lottery();
            timer = 0f;
            tabTypeLength = System.Enum.GetNames(typeof(TabType)).Length;
            normalTab = normalTabObj.GetComponent<NormalTab>();
        }

        public void OnUpdate(float deltaTime)
        {
            timer += deltaTime;
            if (timer >= currentInterval)
            {
                ExecuteTabEvent();
                Lottery();
                timer = 0f;
            }
        }
        public void OnExit()
        {

        }


        /// <summary>
        /// タブの抽選とインターバルの抽選
        /// </summary>
        private void Lottery()
        {
            currentInterval = UnityEngine.Random.Range(intervalRange.x, intervalRange.y);
            currnetTabType = (TabType)UnityEngine.Random.Range(0, tabTypeLength);
        }

        private void ExecuteTabEvent()
        {
            switch (currnetTabType)
            {
                case TabType.Normal:
                    normalTab.Show();
                    currentInterval += normalTab.VisibleTime;
                    break;
            }   
        }
    }
}
