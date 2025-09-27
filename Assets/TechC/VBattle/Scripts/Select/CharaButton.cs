using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

namespace TechC.Select
{
    /// <summary>
    /// キャラピックのイベントを送受信
    /// </summary>
    public class CharaButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        [SerializeField] private Image p1DisplayImage;
        [SerializeField] private Image p2DisplayImage;
        [SerializeField] private SelectPickAnim p1SelectPickAnim;
        [SerializeField] private SelectPickAnim p2SelectPickAnim;

        [SerializeField] private Sprite p1CharaSprite;       // このボタンで選べるキャラのサムネ
        [SerializeField] private Sprite p2CharaSprite;       // このボタンで選べるキャラのサムネ
        [SerializeField] private GameObject pickCharaPrefab; // このボタンで選べるキャラ
        [SerializeField] private float animCallTime = 1f;

        [Header("爆散用マテリアル")]
        [SerializeField] private Material explodeMaterial;   // ExplodeVoronoi.shader を割り当てる

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (p1DisplayImage == null)
            {
                var obj = GameObject.Find("p1DisplayImage")?.GetComponent<Image>();
                if (obj != null) p1DisplayImage = obj;
            }

            if (p2DisplayImage == null)
            {
                var obj = GameObject.Find("p2DisplayImage")?.GetComponent<Image>();
                if (obj != null) p2DisplayImage = obj;
            }
#endif
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var (device, deviceName) = ResolveDevice(eventData);
            int id = SelectUIManagerFix.I.SetCharacterPick(device, pickCharaPrefab);
            ChangePickThumbnail(id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var (device, deviceName) = ResolveDevice(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var (device, deviceName) = ResolveDevice(eventData);

            if (device != null)
            {
                int id = SelectUIManagerFix.I.SetCharacterPick(device, pickCharaPrefab);
                DicidePick(id);

                Debug.Log($"クリック - デバイス: {deviceName}");
            }
            else
            {
                Debug.Log("旧InputSystem経由のクリック");
            }
        }

        private (InputDevice, string) ResolveDevice(PointerEventData eventData)
        {
            if (eventData is UnityEngine.InputSystem.UI.ExtendedPointerEventData extended)
            {
                var device = extended.device;
                if (device is Mouse)
                {
                    return (Keyboard.current, "Keyboard");
                }
                else if (device != null)
                {
                    return (device, device.displayName);
                }
                else
                {
                    return (null, "不明");
                }
            }
            return (null, "旧InputSystem");
        }

        private void ChangePickThumbnail(int id)
        {
            if (id == 0) return;

            if (id == 1)
            {
                p1DisplayImage.sprite = p1CharaSprite;
            }
            else
            {
                p2DisplayImage.sprite = p2CharaSprite;
            }
        }

        private void DicidePick(int id)
        {
            if (id == 0) return;

            Image target = (id == 1) ? p1DisplayImage : p2DisplayImage;
            if (target == null || explodeMaterial == null) return;

            // 爆散アニメーションを開始
            StartCoroutine(PlayExplodeAnimation(target,id));
        }

        private IEnumerator PlayExplodeAnimation(Image target, int id)
        {
            var originalMat = target.material;
            var instMat = new Material(explodeMaterial);
            target.material = instMat;

            float time = 0f;
            float duration = 1.2f;
            if (id == 1 && p1SelectPickAnim != null)
            {
                p1SelectPickAnim.PlayAnim(id);
            }
            else if (id == 2 && p2SelectPickAnim != null)
            {
                p2SelectPickAnim.PlayAnim(id);
            }

            while (time < duration)
            {
                time += Time.deltaTime;
                float progress = Mathf.Clamp01(time / duration);

                instMat.SetFloat("_Progress", progress);
                yield return null;
            }

            target.enabled = false;
            target.material = originalMat;
        }

    }
}