using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class MenuManager : Singleton<MenuManager>
    {
        [SerializeField] private GameObject menuCanvasObj;
        private bool isMenu;
        protected override bool UseDontDestroyOnLoad => base.UseDontDestroyOnLoad;

        protected override void Init()
        {
            base.Init();
            menuCanvasObj.SetActive(false);
            isMenu = false;
        }


        public void OpenMenu()
        {
            menuCanvasObj.SetActive(!isMenu);
            isMenu = !isMenu;
            if (BattleJudge.I == null) return;
            BattleJudge.I.SetPause(!BattleJudge.I.IsPaused);
            
        }
    }
}
