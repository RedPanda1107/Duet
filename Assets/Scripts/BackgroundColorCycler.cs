using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class BackgroundColorCycler : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The UI Graphic (Image or RawImage) that will be color-cycled. If left empty, the first child Graphic will be used.")]
    public Graphic targetGraphic;

    [Header("Timing")]
    [Tooltip("Time between the start of consecutive color changes (seconds).")]
    public float cycleInterval = 3f;
    [Tooltip("Duration of the cross-fade transition (seconds).")]
    public float transitionDuration = 1f;

    [Header("Color constraints")]
    [Tooltip("Minimum brightness (Value in HSV). Lower = darker.")]
    [Range(0f, 1f)]
    public float minValue = 0.25f;
    [Tooltip("Maximum brightness (Value in HSV).")]
    [Range(0f, 1f)]
    public float maxValue = 0.60f;
    [Tooltip("Minimum saturation (HSV). Lower = less vivid.")]
    [Range(0f, 1f)]
    public float minSaturation = 0.25f;
    [Tooltip("Maximum saturation (HSV).")]
    [Range(0f, 1f)]
    public float maxSaturation = 0.75f;

    [Header("Easing")]
    [Tooltip("Animation curve for the transition (0..1). Default is ease in/out.")]
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    Coroutine cycleCoroutine;

    void Start()
    {
        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>();
        }

        if (targetGraphic == null)
        {
            Debug.LogWarning("[BackgroundColorCycler] No Graphic found on Canvas or children. Attach an Image/RawImage and assign it to Target Graphic.");
            return;
        }

        // Initialize to a compatible dark/desaturated color immediately
        Color initial = GenerateRandomDarkDesaturatedColor();
        initial.a = targetGraphic.color.a; // preserve alpha
        targetGraphic.color = initial;

        // Start cycling
        cycleCoroutine = StartCoroutine(CycleRoutine());
    }

    void OnDisable()
    {
        if (cycleCoroutine != null)
        {
            StopCoroutine(cycleCoroutine);
            cycleCoroutine = null;
        }
    }

    IEnumerator CycleRoutine()
    {
        // Ensure sane timing
        float waitBeforeTransition = Mathf.Max(0f, cycleInterval - transitionDuration);

        while (true)
        {
            // Wait until it's time to start the next transition
            yield return new WaitForSeconds(waitBeforeTransition);

            Color from = targetGraphic.color;
            Color to = GenerateRandomDarkDesaturatedColor();
            to.a = from.a; // keep alpha unchanged

            float t = 0f;
            while (t < transitionDuration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / transitionDuration);
                float eased = transitionCurve.Evaluate(normalized);
                targetGraphic.color = Color.Lerp(from, to, eased);
                yield return null;
            }

            // Ensure exact final color
            targetGraphic.color = to;
        }
    }

    Color GenerateRandomDarkDesaturatedColor()
    {
        // Random hue across the spectrum, but keep saturation and value low to avoid vivid/brash colors
        float h = Random.value;
        float s = Random.Range(minSaturation, maxSaturation);
        float v = Random.Range(minValue, maxValue);
        Color col = Color.HSVToRGB(h, s, v);
        return col;
    }
}

