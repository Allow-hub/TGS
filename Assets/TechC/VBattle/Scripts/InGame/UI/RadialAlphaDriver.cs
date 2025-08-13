using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TechC.UIs
{
    public class RadialAlphaDriver : Singleton<RadialAlphaDriver>
    {
        [Header("Default Settings")]
        [SerializeField] private Image targetRenderer;
        [SerializeField] private float defaultStartRadius = 0.05f;
        [SerializeField] private float defaultEndRadius = 0.45f;
        [SerializeField] private float defaultFeather = 0.2f;
        [SerializeField] private float defaultLife = 0.35f; // 広がり時間
        [SerializeField] private float defaultHold = 0.05f; // 一瞬の押し付け時間
        [SerializeField] private float defaultFadeOut = 0.25f; // フェードアウト時間
        [SerializeField] private bool defaultInvert = false;

        Material _mat;
        int _RadiusID = Shader.PropertyToID("_Radius");
        int _FeatherID = Shader.PropertyToID("_Feather");
        int _AlphaID = Shader.PropertyToID("_Alpha");
        int _InvertID = Shader.PropertyToID("_Invert");
        int _CenterID = Shader.PropertyToID("_Center");

        protected override bool UseDontDestroyOnLoad => false;

        protected override void Init()
        {
            base.Init();
            _mat = targetRenderer.material;
            _mat.SetFloat(_FeatherID, defaultFeather);
            _mat.SetFloat(_InvertID, defaultInvert ? 1f : 0f);
            _mat.SetFloat(_AlphaID, 1f);
            _mat.SetFloat(_RadiusID, defaultStartRadius);
            _mat.SetVector(_CenterID, new Vector4(0.5f, 0.5f, 0, 0)); // テクスチャ中央
        }

        /// <summary>
        /// エフェクトを再生（パラメータ指定可能）
        /// </summary>
        public void Play(
            Vector2? uvCenter = null,
            float? startRadius = null,
            float? endRadius = null,
            float? feather = null,
            float? life = null,
            float? hold = null,
            float? fadeOut = null,
            bool? invert = null
        )
        {
            // 中心位置を設定
            if (uvCenter.HasValue)
                _mat.SetVector(_CenterID, new Vector4(uvCenter.Value.x, uvCenter.Value.y, 0, 0));

            // パラメータを適用（指定がなければデフォルトを使う）
            float sRadius = startRadius ?? defaultStartRadius;
            float eRadius = endRadius ?? defaultEndRadius;
            float fth = feather ?? defaultFeather;
            float lfe = life ?? defaultLife;
            float hld = hold ?? defaultHold;
            float fOut = fadeOut ?? defaultFadeOut;
            bool inv = invert ?? defaultInvert;

            _mat.SetFloat(_FeatherID, fth);
            _mat.SetFloat(_InvertID, inv ? 1f : 0f);

            StopAllCoroutines();
            StartCoroutine(CoPlay(sRadius, eRadius, lfe, hld, fOut));
        }

        IEnumerator CoPlay(float startRadius, float endRadius, float life, float hold, float fadeOut)
        {
            // 押し付け時間（Time.timeScaleに依存しない）
            _mat.SetFloat(_RadiusID, startRadius);
            _mat.SetFloat(_AlphaID, 1f);
            yield return new WaitForSecondsRealtime(hold);

            // 広げる
            float t = 0f;
            while (t < life)
            {
                t += Time.unscaledDeltaTime; // 時間スケールの影響を受けない
                float k = t / life;
                _mat.SetFloat(_RadiusID, Mathf.Lerp(startRadius, endRadius, k));
                yield return null;
            }

            // フェードアウト
            t = 0f;
            while (t < fadeOut)
            {
                t += Time.unscaledDeltaTime; // 時間スケールの影響を受けない
                float k = t / fadeOut;
                _mat.SetFloat(_AlphaID, 1f - k);
                yield return null;
            }
            _mat.SetFloat(_AlphaID, 0f);
        }

    }
}