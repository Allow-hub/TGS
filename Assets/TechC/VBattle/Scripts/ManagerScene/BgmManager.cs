using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class BgmManager : Singleton<BgmManager>
    {
        [SerializeField] private AudioSource audioSource;

        protected override void Init()
        {
            base.Init();
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        private void PlayBGM() => audioSource.Play();
        private void StopBGM() => audioSource.Stop();

        public void SetBGMVolume(float volume)
        {
            audioSource.volume = volume;
        }
    }
}
