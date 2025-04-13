using UnityEngine;

namespace TechC
{
    public class CommentCollider : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            Debug.Log("コメントに当たった");
            if(other.gameObject.CompareTag("Player"))
            {
                Debug.Log("Playerがコメントに当たった。");
            }
        }

    }
}
