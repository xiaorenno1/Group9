using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Text))]
[RequireComponent(typeof(AudioSource))]
public class AutoStoryText : MonoBehaviour, IPointerClickHandler
{
    [Header("=== 1. 字体与位置 ===")]
    [Tooltip("必须拖入中文字体，否则中文显示方块")]
    public Font displayFont;
    
    [Tooltip("上下拖动这个滑条来调整文字位置 (0=底部, 1=顶部)")]
    [Range(0f, 1f)] public float verticalPos = 0.85f; // ✨ 新增：位置控制滑条

    [Header("=== 2. 英文设置 ===")]
    [TextArea(2, 5)] public string enText; 
    public AudioClip enVoice;
    [Range(20, 300)] public int enFontSize = 80; 

    [Header("=== 3. 中文设置 ===")]
    [TextArea(2, 5)] public string cnText; 
    public AudioClip cnVoice;
    [Range(20, 300)] public int cnFontSize = 80; 

    private Text uiText;
    private AudioSource audioSource;
    private bool isEnglish = true; 

    void Start()
    {
        uiText = GetComponent<Text>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        SetupStyle();
        UpdateContent(); 
    }

    void SetupStyle()
    {
        if (uiText == null) uiText = GetComponent<Text>();

        // 1. 强制关闭自适应，使用我们设定的大字号
        uiText.resizeTextForBestFit = false;
        
        // 2. 允许溢出，防止框太小导致字消失
        uiText.horizontalOverflow = HorizontalWrapMode.Wrap; 
        uiText.verticalOverflow = VerticalWrapMode.Overflow; 

        if (displayFont != null) uiText.font = displayFont;
        
        uiText.color = new Color(0.2f, 0.2f, 0.2f, 1f); 
        uiText.alignment = TextAnchor.MiddleCenter;     
        uiText.raycastTarget = true;                    

        // 初始化位置
        UpdatePosition();
    }

    // ✨ 专门用来计算位置的函数
    void UpdatePosition()
    {
        if (uiText == null) return;
        RectTransform rect = GetComponent<RectTransform>();
        
        // 设定文字框高度固定占屏幕的 20% (0.2)
        float height = 0.2f; 
        
        // 限制滑条范围，防止文字滑出屏幕外
        // 这里的 math 计算是为了保证文字框永远在屏幕内
        float clampedY = Mathf.Clamp(verticalPos, 0f + height/2, 1f - height/2);

        // 应用位置 (左右固定占 80%)
        rect.anchorMin = new Vector2(0.1f, clampedY - height/2);
        rect.anchorMax = new Vector2(0.9f, clampedY + height/2);
        
        // 清零偏移，确保贴合锚点
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isEnglish = !isEnglish;
        UpdateContent();
    }

    void UpdateContent()
    {
        if (uiText == null) return;
        audioSource.Stop();

        if (isEnglish)
        {
            uiText.fontSize = enFontSize; 
            uiText.text = enText;
            if (enVoice != null && Application.isPlaying) audioSource.PlayOneShot(enVoice);
        }
        else
        {
            uiText.fontSize = cnFontSize; 
            uiText.text = cnText;
            if (cnVoice != null && Application.isPlaying) audioSource.PlayOneShot(cnVoice);
        }
    }

    // ✨ 实时刷新：你在 Inspector 动滑条，位置和字号就会立刻变
    void OnValidate()
    {
        uiText = GetComponent<Text>();
        if (uiText != null)
        {
            uiText.resizeTextForBestFit = false; 
            uiText.verticalOverflow = VerticalWrapMode.Overflow; 
            uiText.fontSize = isEnglish ? enFontSize : cnFontSize;
            
            // 实时更新位置
            UpdatePosition();
        }
    }
}