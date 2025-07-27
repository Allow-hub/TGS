using System;
using UnityEngine;

namespace TechC.CommentSystem
{
    [Serializable]
    public class GrassAbility : ICommentAbility
    {
        private SpecialCommentTrigger trigger;

        public void Init(SpecialCommentTrigger trigger)
        {
            this.trigger = trigger;
        }

        public void Release() { }

        public void OnTriggerEnter(Collider collider)
        {
            var grassController = trigger.GetComponent<GrassController>();
            grassController?.Throw();
        }
    }
}
