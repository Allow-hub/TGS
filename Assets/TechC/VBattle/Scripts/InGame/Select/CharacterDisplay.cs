using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class CharacterDisplay : MonoBehaviour
    {
        private Animator characterAnimator;  // キャラクターのAnimator

        void Start()
        {
            // Animatorを取得
            characterAnimator = GetComponent<Animator>();
        }

        // 3Dキャラのアニメーションを設定（選択されたキャラのIdleアニメーション）
        public void SetCharacterAnimation(string animationName)
        {
            if (characterAnimator != null)
            {
                characterAnimator.Play(animationName);  // アニメーションを再生
            }
        }
    }

}
