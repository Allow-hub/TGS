using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommentManager : MonoBehaviour
{
    [SerializeField] private List<string> commentList = new List<string>();
    [SerializeField] private GameObject commentPrefab;
    [SerializeField] private RectTransform commentParent;
    [SerializeField] private float spawnInterval = 1.0f;

    [SerializeField] private float minYPosition = -50f;
    [SerializeField] private float maxYPosition = 50f;
    [SerializeField] private float minSpeed = 150f;
    [SerializeField] private float maxSpeed = 250f;

    void Start()
    {   
        StartCoroutine(SpawnCommentsRoutine());
    }

    IEnumerator SpawnCommentsRoutine()
    {
        while (true)
        {
            if (commentList.Count > 0)
            {
                string message = commentList[Random.Range(0, commentList.Count)];
                GameObject obj = Instantiate(commentPrefab, commentParent);
                float yPos = Random.Range(minYPosition, maxYPosition);
                float speed = Random.Range(minSpeed, maxSpeed);

                obj.GetComponent<CommentMover>().Initialize(message, speed, yPos);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}