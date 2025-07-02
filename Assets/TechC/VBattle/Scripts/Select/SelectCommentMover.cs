using UnityEngine;
using TMPro;

public class SelectCommentMover : MonoBehaviour
{
    [SerializeField] private float startOffsetX = 200f;    // 画面右端からの初期出現位置
    [SerializeField] private float destroyOffsetX = 300f;  // 画面左端を超えたときの削除位置

    private float speed;
    private RectTransform rectTransform;
    private TMP_Text textComponent;

    public void Initialize(string message, float moveSpeed, float yPos)
    {
        rectTransform = GetComponent<RectTransform>();
        textComponent = GetComponent<TMP_Text>();

        textComponent.text = message;
        speed = moveSpeed;

        rectTransform.anchoredPosition = new Vector2(Screen.width + startOffsetX, yPos);
    }

    void Update()
    {
        if (rectTransform == null) return;

        rectTransform.anchoredPosition += Vector2.left * speed * Time.deltaTime;

        if (rectTransform.anchoredPosition.x < -Screen.width - destroyOffsetX)
        {
            Destroy(gameObject);
        }
    }
}