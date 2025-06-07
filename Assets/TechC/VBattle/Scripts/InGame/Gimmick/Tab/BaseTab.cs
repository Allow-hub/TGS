using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// タブの基底クラス
    /// </summary>
    public class BaseTab : MonoBehaviour, ITab 
    {
        /// <summary>
        /// タブを表示
        /// </summary>
        public virtual void Show()
        {
            Debug.Log("AAA");
        }
        /// <summary>
        /// タブを隠す
        /// </summary>
        public virtual void Hide()
        {

        }

        /// <summary>
        /// タブの能力
        /// </summary>
        public virtual void Excute()
        {

        }
    }
}
