using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class SimplePageSwitcher : MonoBehaviour
{
    [Header("美化设置")]
    public Sprite prevIcon; // 如果有箭头图片就拖，没有就自动画
    public Sprite nextIcon; 
    
    [Header("配色方案")]
    public Color circleColor = new Color(1f, 1f, 1f, 0.8f); // 圆圈底色 (半透明米白，像贴纸)
    public Color arrowColor = new Color(0.4f, 0.26f, 0.13f, 1f); // 箭头颜色 (绘本深褐色)

    private GameObject canvasObj;
    private Sprite circleSprite; // 圆形背景图

    void Start()
    {
        // 0. 自动补全 EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        CreateNavigationUI();
    }

    void CreateNavigationUI()
    {
        // 清理旧的防止重叠
        GameObject oldCanvas = GameObject.Find("Nav_Canvas");
        if (oldCanvas != null) Destroy(oldCanvas);

        // 1. 创建画布
        canvasObj = new GameObject("Nav_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. 自动生成素材
        if (prevIcon == null) prevIcon = GenerateArrowSprite();
        if (nextIcon == null) nextIcon = GenerateArrowSprite();
        circleSprite = GenerateCircleSprite(); // 生成圆形背景

        // 3. 创建悬浮按钮
        CreateFloatingButton("Btn_Prev", prevIcon, true);
        CreateFloatingButton("Btn_Next", nextIcon, false);
    }

    // 🎨 画一个圆形背景 (带抗锯齿)
    Sprite GenerateCircleSprite()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f; // 留一点边距防止切边

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    // 边缘柔化 (Anti-aliasing)
                    float alpha = 1f;
                    if (dist > radius - 2f) alpha = (radius - dist) / 2f;
                    // 这里的颜色会在 Image 组件里被 circleColor 染色
                    colors[y * size + x] = new Color(1, 1, 1, alpha);
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // 🎨 画箭头
    Sprite GenerateArrowSprite()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.clear;

        Vector2 center = new Vector2(size / 2, size / 2);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float u = (x - center.x) / (size * 0.4f);
                float v = (y - center.y) / (size * 0.4f);
                float dist = Mathf.Abs(Mathf.Abs(v) - (u + 0.5f)); 
                if (u < 0.5f && Mathf.Abs(v) < 0.8f && dist < 0.25f) // 加粗一点
                {
                    float alpha = 1f - (dist / 0.25f);
                    colors[y * size + x] = new Color(1, 1, 1, alpha);
                }
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void CreateFloatingButton(string name, Sprite arrowIcon, bool isLeft)
    {
        // === 1. 外层：圆形底座 (Button本体) ===
        GameObject bgObj = new GameObject(name);
        bgObj.transform.SetParent(canvasObj.transform, false);

        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = circleSprite;
        bgImg.color = circleColor; // 半透明米白底

        RectTransform bgRect = bgImg.rectTransform;
        bgRect.sizeDelta = new Vector2(140, 140); // 圆圈大小
        
        // 统一中心锚点，防止跑偏
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);

        // 计算屏幕位置 (基于 1080x1920 的参考系)
        // 440 是横向偏移 (1080/2 - 100边距)
        // -800 是纵向偏移 (往下)
        float xPos = 440f; 
        float yPos = -800f; 

        if (isLeft)
            bgRect.anchoredPosition = new Vector2(-xPos, yPos); // 左下
        else
            bgRect.anchoredPosition = new Vector2(xPos, yPos);  // 右下

        // 添加按钮功能
        Button btn = bgObj.AddComponent<Button>();
        if (isLeft) btn.onClick.AddListener(GoToPrevPage);
        else btn.onClick.AddListener(GoToNextPage);

        // 添加呼吸动画
        bgObj.AddComponent<SimpleBreathing>();

        // === 2. 内层：箭头图标 ===
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(bgObj.transform, false);

        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.sprite = arrowIcon;
        iconImg.color = arrowColor; // 深褐色箭头
        iconImg.preserveAspect = true;
        
        // 稍微缩小一点，放在圆圈中间
        RectTransform iconRect = iconImg.rectTransform;
        iconRect.sizeDelta = new Vector2(70, 70); 
        iconRect.anchoredPosition = Vector2.zero;

        // 旋转箭头
        if (!isLeft) iconRect.localRotation = Quaternion.Euler(0, 0, 180);
    }

    public void GoToNextPage()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        if (current < SceneManager.sceneCountInBuildSettings - 1)
            SceneManager.LoadScene(current + 1);
        else Debug.Log("最后一页");
    }

    public void GoToPrevPage()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        if (current > 0)
            SceneManager.LoadScene(current - 1);
    }
}

// 呼吸小动画
public class SimpleBreathing : MonoBehaviour
{
    Vector3 baseScale;
    void Start() { baseScale = transform.localScale; }
    void Update() 
    {
        float pulse = 1.0f + Mathf.Sin(Time.time * 3f) * 0.1f; 
        transform.localScale = baseScale * pulse;
    }
}