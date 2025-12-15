using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class PlayOnceAnim : MonoBehaviour, IPointerClickHandler
{
    [Header("显示组件")]
    public Image targetImage;       // 拖入显示图片的 Image 组件

    [Header("核心设置")]
    public Sprite idleSprite;       // 静态待机图（封面）
    public float frameRate = 0.04f; // 播放速度

    [Header("动画序列帧")]
    public Sprite[] animFrames;     // 动画帧数组

    [Header("音效与台词 (新修改)")]
    public AudioSource audioSource; // 拖入 Audio Source 组件
    
    // --- 这里是你要求修改的部分 ---
    [Tooltip("这里拖入小兔子的音频")]
    public AudioClip smallRabbitClip; // 1. 先播放：小兔子台词
    
    [Tooltip("这里拖入大兔子的音频")]
    public AudioClip bigRabbitClip;   // 2. 后播放：大兔子台词
    
    [Range(0f, 2f)]
    public float dialogueInterval = 0.5f; // 中间的间隔时间（秒）
    // ---------------------------

    private bool isPlaying = false; 

    void Start()
    {
        if (idleSprite != null && targetImage != null)
        {
            targetImage.sprite = idleSprite;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isPlaying)
        {
            // 启动主流程
            StartCoroutine(PlayRoutine());
        }
    }

    IEnumerator PlayRoutine()
    {
        isPlaying = true;

        // 【关键修改】：开启一个独立的协程专门处理“对话接龙”
        // 这样声音在那边排队播放，下面的动画可以同时顺畅播放，互不卡顿
        StartCoroutine(PlayDialogueSequence());

        // --- 动画播放逻辑 (保持不变) ---
        if (animFrames != null)
        {
            foreach (Sprite frame in animFrames)
            {
                if(targetImage != null) targetImage.sprite = frame;
                yield return new WaitForSeconds(frameRate);
            }
        }

        // 动画播完回正
        if (idleSprite != null && targetImage != null)
        {
            targetImage.sprite = idleSprite;
        }

        isPlaying = false;
    }

    // --- 新增：专门处理对话顺序的协程 ---
    IEnumerator PlayDialogueSequence()
    {
        if (audioSource == null) yield break;

        // 1. 先播小兔子 (如果有)
        if (smallRabbitClip != null)
        {
            audioSource.PlayOneShot(smallRabbitClip);
            // 等待小兔子说完 (等待音频的长度)
            yield return new WaitForSeconds(smallRabbitClip.length);
        }

        // 2. 中间稍微停顿一下
        yield return new WaitForSeconds(dialogueInterval);

        // 3. 再播大兔子 (如果有)
        if (bigRabbitClip != null)
        {
            audioSource.PlayOneShot(bigRabbitClip);
        }
    }
}