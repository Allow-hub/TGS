using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC.Select
{
    public class SelectUIManagerFix : Singleton<SelectUIManagerFix>
    {
        public struct CharacterPick
        {
            public int playerId;
            public GameObject characterObject;
            public InputDevice inputDevice;
        }
        [SerializeField] private IconController iconController_1p;
        [SerializeField] private IconController iconController_2p;

        private CharacterPick[] currentPicks = new CharacterPick[2];
        protected override bool UseDontDestroyOnLoad => false;
        protected override void Init()
        {
            base.Init();
            currentPicks[0].playerId = 0;
            currentPicks[1].playerId = 1;
        }

        /// <summary>
        /// キャラ選択時そのデバイスが使用中であるかどうかで値を変える
        /// </summary>
        /// <param name="inputDevice">入力が加えられたデバイス</param>
        /// <param name="pickChara">ピックされたキャラ</param>
        /// <returns>1->1p,2->2p,0->無効なデバイス</returns>
        public int SetCharacterPick(InputDevice inputDevice, GameObject pickChara)
        {
            if (iconController_1p.GetCurrentDevice() == inputDevice)
            {
                currentPicks[0].characterObject = pickChara;
                return 1;
            }
            else if (iconController_2p.GetCurrentDevice() == inputDevice)
            {
                currentPicks[1].characterObject = pickChara;
                return 2;
            }
            return 0;
        }
    }
}