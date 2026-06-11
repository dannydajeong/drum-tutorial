using UnityEngine;

public class DrumVisual : MonoBehaviour
{
    [Header("이펙트 설정")]
    public Color hitColor = Color.yellow;
    public float hitDuration = 0.1f;
	
    private ParticleSystem particle;
    private Material mat;
    private Color originalColor;
    private Vector3 originalScale;

    void Start()
{
    mat = GetComponent<Renderer>().material;
    originalColor = mat.color;
    originalScale = transform.localScale;
    particle = GetComponentInChildren<ParticleSystem>(); // 추가
}

public void Hit()
{
    StopAllCoroutines();
    StartCoroutine(HitEffect());
    if (particle != null) particle.Play(); // 추가
}

    System.Collections.IEnumerator HitEffect()
    {
        mat.color = hitColor;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", hitColor);
        transform.localScale = originalScale * 0.85f;

        yield return new WaitForSeconds(hitDuration);

        mat.color = originalColor;
        mat.DisableKeyword("_EMISSION");
        transform.localScale = originalScale;
    }
}