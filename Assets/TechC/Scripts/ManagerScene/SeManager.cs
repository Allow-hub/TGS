using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{

    public class SeManager : Singleton<SeManager>
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] clip;

        public void PlaySE(int clipIndex)
        {
            audioSource.PlayOneShot(clip[clipIndex]);
        }

        public void SetSEVolume(float volume)
        {
            audioSource.volume = volume;
        }
    }
}
