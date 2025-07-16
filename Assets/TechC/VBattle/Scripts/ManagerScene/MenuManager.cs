using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    /// <summary>
    /// menuの管理クラス
    /// </summary>
    public class MenuManager : Singleton<MenuManager>
    {
        [SerializeField] private GameObject menuCanvasObj;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button minusButton;
        [SerializeField] private Slider audioSlider;
        private float volumeRatio = 0.1f;
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
            //ボタンの購読
            homeButton.onClick.AddListener(() => OnHome());
            plusButton.onClick.AddListener(() => OnSoundVolumeChange(volumeRatio));
            minusButton.onClick.AddListener(() => OnSoundVolumeChange(-volumeRatio));
            audioSlider.onValueChanged.AddListener(OnAudioSliderChanged);
        }

        /// <summary>
        /// メニューの音量バーを変更したとき
        /// </summary>
        /// <param name="value"></param>
        private void OnAudioSliderChanged(float value)
        {
            AudioManager.I.SetMasterVolume(value);
        }

        /// <summary>
        /// 音の変更ボタンを押したとき
        /// </summary>
        /// <param name="value">+か-か</param>
        private void OnSoundVolumeChange(float value)
        {
            AudioManager.I.PlaySE(SEID.ButtonClick);
            audioSlider.value += value;
        }

        /// <summary>
        /// メニューを開く
        /// </summary>
        public void OpenMenu()
        {
            menuCanvasObj.SetActive(!isMenu);
            isMenu = !isMenu;
            if (isMenu)
                AudioManager.I.PlaySE(SEID.MenuOpen);
            if (BattleJudge.I == null) return;
            BattleJudge.I.SetPause(!BattleJudge.I.IsPaused);

        }

        /// <summary>
        /// タイトルに戻る
        /// </summary>
        private void OnHome()
        {
            GameManager.I.ChangeTitleState();
            OpenMenu();
        }
    }
}
