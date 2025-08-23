using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ImpactDistortionController : MonoBehaviour
{
    public float maxRadius = 3f;
    public float waveWidth = 0.5f;
    public float duration = 0.5f;

    private Material mat;
    private float time;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        mat.SetFloat("_Radius", 0f);
        mat.SetFloat("_Width", waveWidth);
        mat.SetVector("_Center", transform.position);
    }

    private void OnDisable()
    {
        time = 0f;
        mat.SetFloat("_Radius", 0f);
    }
    void Update()
    {
        time += Time.deltaTime;
        float t = time / duration;

        mat.SetFloat("_Radius", Mathf.Lerp(0f, maxRadius, t));

        if (t >= 1f) gameObject.SetActive(false);
    }
}
