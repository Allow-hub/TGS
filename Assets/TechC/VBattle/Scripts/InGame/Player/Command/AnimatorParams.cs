using UnityEngine;

namespace TechC
{
    public static class AnimatorParams
    {
        public static readonly int IsWalking = Animator.StringToHash("IsWalking");
        public static readonly int IsRunning = Animator.StringToHash("IsRunning");
        public static readonly int IsJumping = Animator.StringToHash("IsJumping");
        public static readonly int IsDoubleJumping = Animator.StringToHash("IsDoubleJumping");
        public static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
        public static readonly int IsGuarding = Animator.StringToHash("IsGuarding");

    }
}
