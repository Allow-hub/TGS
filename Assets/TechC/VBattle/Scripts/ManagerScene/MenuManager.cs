using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class MenuManager : Singleton<MenuManager>
    {
        [SerializeField] private GameObject menuCanvasObj;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button minusButton;
        [SerializeField] private Slider audioSlider;
        private float volumeRatio=0.1f;
        private bool isMenu;
        protected override bool UseDontDestroyOnLoad => base.UseDontDestroyOnLoad;

        protected override void Init()
        {
            base.Init();
            menuCanvasObj.SetActive(false);
            isMenu = false;
        }
        private void Start()
        {
            homeButton.onClick.AddListener(() => OnHome());
            plusButton.onClick.AddListener(() => OnSoundVolumeChange(volumeRatio));
            minusButton.onClick.AddListener(() => OnSoundVolumeChange(-volumeRatio));
            audioSlider.onValueChanged.AddListener(OnAudioSliderChanged);
        }

        private void OnAudioSliderChanged(float value)
        {
            AudioManager.I.SetMasterVolume(value);
        }
        private void OnSoundVolumeChange(float value)
        {
            AudioManager.I.PlaySE(SEID.ButtonClick);
            audioSlider.value += value;
        }

        public void OpenMenu()
        {
            menuCanvasObj.SetActive(!isMenu);
            isMenu = !isMenu;
            if (isMenu)
                AudioManager.I.PlaySE(SEID.MenuOpen);
            if (BattleJudge.I == null) return;
            BattleJudge.I.SetPause(!BattleJudge.I.IsPaused);

        }

        private void OnHome()
        {
            GameManager.I.ChangeTitleState();
            OpenMenu();
        }
    }
}
