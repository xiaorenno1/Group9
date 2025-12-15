using UnityEngine;
using System.Collections.Generic;

public class StarsManager : MonoBehaviour
{
    [Header("Star Settings")]
    public GameObject starPrefab;
    public int starCount = 30;
    public float minScale = 0.1f;
    public float maxScale = 0.3f;
    public float appearDuration = 2f;

    [Header("Position Range")]
    public Vector2 xRange = new Vector2(-800f, 800f);
    public Vector2 yRange = new Vector2(-400f, 400f);

    private List<GameObject> stars = new List<GameObject>();
    private bool starsVisible = false;
    private Coroutine[] fadeCoroutines;

    void Start()
    {
        // Check if prefab is assigned
        if (starPrefab == null)
        {
            Debug.LogError("Star Prefab is not assigned in the Inspector!");
            return;
        }

        fadeCoroutines = new Coroutine[starCount];
        CreateStars();
    }

    private void CreateStars()
    {
        if (starPrefab == null)
        {
            Debug.LogError("Cannot create stars: Star Prefab is null");
            return;
        }

        for (int i = 0; i < starCount; i++)
        {
            GameObject star = Instantiate(starPrefab, transform);
            SetupStar(star);
            stars.Add(star);
        }
    }

    private void SetupStar(GameObject star)
    {
        // Set random position
        RectTransform rectTransform = star.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = GetRandomPosition();
        }

        // Set random scale
        float randomScale = Random.Range(minScale, maxScale);
        star.transform.localScale = Vector3.one * randomScale;

        // Initially hidden
        CanvasGroup canvasGroup = star.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = star.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;
    }

    public void ShowStars()
    {
        if (starsVisible || stars.Count == 0) return;
        starsVisible = true;

        for (int i = 0; i < stars.Count; i++)
        {
            if (stars[i] != null)
            {
                fadeCoroutines[i] = StartCoroutine(FadeStar(stars[i], 0f, 1f, appearDuration));
            }
        }
    }

    public void HideStars()
    {
        if (!starsVisible) return;
        starsVisible = false;

        // Stop all active fade animations
        if (fadeCoroutines != null)
        {
            for (int i = 0; i < fadeCoroutines.Length; i++)
            {
                if (fadeCoroutines[i] != null)
                {
                    StopCoroutine(fadeCoroutines[i]);
                }
            }
        }

        // Hide all stars immediately
        foreach (GameObject star in stars)
        {
            if (star != null)
            {
                CanvasGroup canvasGroup = star.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0;
                }
            }
        }
    }

    public void ToggleStars()
    {
        if (starsVisible)
            HideStars();
        else
            ShowStars();
    }

    private System.Collections.IEnumerator FadeStar(GameObject star, float fromAlpha, float toAlpha, float duration)
    {
        if (star == null) yield break;

        CanvasGroup canvasGroup = star.GetComponent<CanvasGroup>();
        if (canvasGroup == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = toAlpha;
    }

    private Vector2 GetRandomPosition()
    {
        return new Vector2(
            Random.Range(xRange.x, xRange.y),
            Random.Range(yRange.x, yRange.y)
        );
    }

    // Rearrange all stars randomly
    public void RearrangeStars()
    {
        foreach (GameObject star in stars)
        {
            if (star != null)
            {
                RectTransform rectTransform = star.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = GetRandomPosition();
                }

                float randomScale = Random.Range(minScale, maxScale);
                star.transform.localScale = Vector3.one * randomScale;
            }
        }
    }

    // Clean up resources
    private void OnDestroy()
    {
        foreach (GameObject star in stars)
        {
            if (star != null)
            {
                Destroy(star);
            }
        }
        stars.Clear();
    }
}