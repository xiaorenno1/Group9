using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class PageTwoStretch : MonoBehaviour, IPointerClickHandler
{
    [Header("设置")]
    // 确保这里拖的是物体自己的Image组件
    public Image targetImage;       
    public Sprite idleSprite;     
    public Sprite[] animFrames;    
    public float frameRate = 0.04f; 

    [Header("音效与台词")]
    public AudioSource audioSource;
    public AudioClip stretchSound; 
    public AudioClip dialogueAudio;

    // 用来存储当前正在运行的协程，以便打断它
    private Coroutine currentCoroutine;

    void Start()
    {
        // 初始化显示静态图
        if (idleSprite != null && targetImage != null)
        {
            targetImage.sprite = idleSprite;
        }
        
        // 【重要】防遮挡设置
        // 确保你的图片在 Import Settings 里勾选了 "Read/Write Enabled"
        // 这行代码会让图片的透明区域不响应点击，让鼠标能“穿透”大兔子的透明肚子点到后面的小兔子
        if (targetImage != null)
        {
            targetImage.alphaHitTestMinimumThreshold = 0.1f;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 【修改点1：打断机制】
        // 如果之前有动画在播，立刻停止它，不用等待isPlaying了
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // 停止之前的声音，避免重叠（可选）
        if (audioSource != null)
        {
            audioSource.Stop(); 
        }

        // 重新开启新的动画，并记录下来
        currentCoroutine = StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        // 1. 播放声音
        if (audioSource != null)
        {
            // 使用PlayOneShot可以叠加简短的音效
            if (stretchSound != null) audioSource.PlayOneShot(stretchSound);
            // 如果台词较长，希望打断旧台词播新台词，可以用 audioSource.clip = dialogueAudio; audioSource.Play();
            if (dialogueAudio != null) audioSource.PlayOneShot(dialogueAudio);
        }

        // 2. 循环播放每一张图
        if (animFrames != null && animFrames.Length > 0)
        {
            foreach (Sprite frame in animFrames)
            {
                if (targetImage != null) targetImage.sprite = frame;
                yield return new WaitForSeconds(frameRate);
            }
        }

        // 3. 播完后，变回待机状态
        if (idleSprite != null && targetImage != null)
        {
            targetImage.sprite = idleSprite;
        }
        
        // 动画自然结束，清空记录
        currentCoroutine = null;
    }
}