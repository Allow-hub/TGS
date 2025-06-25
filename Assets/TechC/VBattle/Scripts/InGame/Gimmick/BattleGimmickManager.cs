using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// バトル全体のギミックの管理
    /// </summary>
    public class BattleGimmickManager : Singleton<BattleGimmickManager>
    {
        [Header("通知タブ")]
        private readonly List<IGimmick> gimmicks = new();
        [SerializeField] private TabGimickController tabGimickController;
        protected override bool UseDontDestroyOnLoad => false;
        protected override void Init()
        {
            base.Init();
            gimmicks.Add(tabGimickController);
            foreach (var gimmick in gimmicks)
                gimmick.OnEnter();
        }
        protected override void OnRelease()
        {
            base.OnRelease();
            foreach (var gimmick in gimmicks)
                gimmick.OnExit();
        }


        private void Update()
        {
                 foreach (var gimmick in gimmicks)
                gimmick.OnUpdate(Time.deltaTime);
        }
    }
}
