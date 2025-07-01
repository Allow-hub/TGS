using UnityEngine;
using System.Collections;

public class ScreenBreak : MonoBehaviour
{
    [SerializeField] private bool useGravity = true;                            // 重力を有効にするかどうか
    [SerializeField] private Vector3 explodeVel = new Vector3(0, 0, 0.1f);      // 爆発の中心地
    [SerializeField] private float explodeForce = 200f;                         // 爆発の威力
    [SerializeField] private float explodeRange = 10f;                          // 爆発の範囲
    private Rigidbody[] rigidBodies;

    [SerializeField] private Material freezeMaterial;
    [SerializeField] private float freezeDuration = 3.0f;
    private float freezeProgress = 0f;
    // マジックナンバーを定数化
    private const float DelayBeforeBreak = 1f; // ここを使う
    private const float CrackEffectDuration = 0.02f;
    private const float WaitAfterCrack = 0.8f;
    private const float FirstExplosionForceDivisor = 6f;
    private const float WaitAfterExplosion = 2f; // 追加：爆発後に非表示にするまでの待機時間

    void Start()
    {
        rigidBodies = GetComponentsInChildren<Rigidbody>();                     // 子(破片)のRigidbodyを取得しておく
        StartCoroutine(FreezeStart());
    }

    IEnumerator FreezeStart()
    {
        float elapsed = 0f;
        while (elapsed < freezeDuration)
        {
            elapsed += Time.deltaTime;
            freezeProgress = Mathf.Clamp01(elapsed / freezeDuration);
            freezeMaterial.SetFloat("_FreezeAmount", freezeProgress);
            yield return null;
        }
        yield return new WaitForSeconds(DelayBeforeBreak); // ← ここで凍り終わった後に待つ
        StartCoroutine(BreakStart());
    }

    IEnumerator BreakStart()
    {
        // 破片を全てアクティブにする
        foreach (Rigidbody rb in rigidBodies)
        {
            rb.gameObject.SetActive(true);
        }

        foreach (Rigidbody rb in rigidBodies)
        {
            rb.isKinematic = false;
            rb.useGravity = useGravity;
            rb.AddExplosionForce(explodeForce / FirstExplosionForceDivisor, transform.position + explodeVel, explodeRange);
        }
        yield return new WaitForSeconds(CrackEffectDuration); // 一瞬動かすことでひび割れを演出

        foreach (Rigidbody rb in rigidBodies)
        {
            rb.isKinematic = true;
        }
        yield return new WaitForSeconds(WaitAfterCrack);

        foreach (Rigidbody rb in rigidBodies)
        {
            rb.isKinematic = false;
            rb.AddExplosionForce(explodeForce, transform.position + explodeVel, explodeRange);
        }
        // 爆発後に非表示にするまでの待機時間も定数で管理
        yield return new WaitForSeconds(WaitAfterExplosion);
        gameObject.SetActive(false);
    }
}
