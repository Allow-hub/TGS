using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TechC
{
    [RequireComponent(typeof(RectTransform))]
    public class BaseTab : MonoBehaviour,ITab
    {
        [SerializeField] protected float slideDuration = 0.5f;    // スライドアニメ時間
        [SerializeField] protected float visibleTime = 3f;        // 表示持続時間

        protected RectTransform rectTransform;
        [SerializeField] protected Vector2 hiddenPos = new Vector2(0, 100);   // 画面外（上）
        [SerializeField] protected Vector2 visiblePos = new Vector2(0, -50);  // 表示位置

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = hiddenPos;
            gameObject.SetActive(false);
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(SlideIn());
        }

        public virtual void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(SlideOut());
        }

        public virtual void Excute()
        {
            
        }

        protected IEnumerator SlideIn()
        {
            float time = 0f;
            while (time < slideDuration)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(hiddenPos, visiblePos, time / slideDuration);
                time += Time.deltaTime;
                yield return null;
            }
            rectTransform.anchoredPosition = visiblePos;

            yield return new WaitForSeconds(visibleTime);
            Hide();
        }

        protected IEnumerator SlideOut()
        {
            float time = 0f;
            while (time < slideDuration)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(visiblePos, hiddenPos, time / slideDuration);
                time += Time.deltaTime;
                yield return null;
            }
            rectTransform.anchoredPosition = hiddenPos;
            gameObject.SetActive(false);
        }
    }
}