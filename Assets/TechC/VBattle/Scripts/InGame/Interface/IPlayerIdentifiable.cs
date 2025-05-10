using UnityEngine;

namespace TechC
{
    /// <summary>
    /// プレイヤーを識別するためのインターフェース
    /// </summary>
    public interface IPlayerIdentifiable
    {
        /// <summary>
        /// プレイヤーIDを取得
        /// </summary>
        /// <returns>プレイヤーID</returns>
        int GetPlayerID();
    }
}