using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class CommentFactory : Singleton<CommentFactory>
    {
        [SerializeField]
        private ObjectPool commentPool;

        public void PlayComment()
        {
            // commentPool.GetObjectByName()
        }
    }
}
