using System.Collections.Generic;
using UnityEngine;

namespace TechC.CommentSystem
{
    /// <summary>
    /// 特殊コメントを管理するクラス
    /// </summary>
    public class SpecialCommentManager : Singleton<SpecialCommentManager>
    {
        [SerializeField] private FreezeController freezeController;
        protected override bool UseDontDestroyOnLoad => false;

        public void HandleFreeze(GameObject comment, List<GameObject> chars)
        {
            freezeController.ReturnCommentAndChars(comment, chars);
        }
    }
}
