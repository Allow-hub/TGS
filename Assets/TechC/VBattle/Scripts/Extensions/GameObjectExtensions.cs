using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    using UnityEngine;

    /// <summary>
    /// GameObjectの拡張メソッドを提供するクラス
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// 自身または親階層を遡って指定した型のコンポーネントを取得します
        /// </summary>
        /// <typeparam name="T">取得したいコンポーネントの型</typeparam>
        /// <param name="obj">拡張メソッドを適用するGameObject</param>
        /// <returns>見つかったコンポーネント、見つからなかった場合はnull</returns>
        public static T GetComponentInHierarchy<T>(this GameObject obj) where T : Component
        {
            // 現在のオブジェクトでコンポーネントを検索
            T component = obj.GetComponent<T>();

            // コンポーネントが見つかった場合はそれを返す
            if (component != null)
                return component;

            // 親がない場合はnullを返す
            if (obj.transform.parent == null)
                return null;

            // 親オブジェクトに対して再帰的に検索
            return obj.transform.parent.gameObject.GetComponentInHierarchy<T>();
        }

        /// <summary>
        /// 自身または親階層を遡って指定した型のコンポーネントを取得します
        /// </summary>
        /// <typeparam name="T">取得したいコンポーネントの型</typeparam>
        /// <param name="component">拡張メソッドを適用するComponent</param>
        /// <returns>見つかったコンポーネント、見つからなかった場合はnull</returns>
        public static T GetComponentInHierarchy<T>(this Component component) where T : Component
        {
            // GameObjectバージョンの拡張メソッドを呼び出す
            return component.gameObject.GetComponentInHierarchy<T>();
        }
    }
}
