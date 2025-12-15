using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class SimpleFrameAnimation : MonoBehaviour, IPointerClickHandler
{
    [Header("设置")]
    public Image targetImage;       // 显示动画的Image组件
    public Sprite idleSprite;       // 兔子站着不动的图片
    public Sprite[] jumpFrames;     // 所有的跳跃分解动作图片
    public float frameRate = 0.05f; // 每一帧的间隔时间

    [Header("特效 (仅大兔子填)")]
    public ParticleSystem leavesParticles; // 落叶粒子
    public float particleDelay = 0.5f;     // 延迟多久播放粒子

    [Header("音效与台词 (新功能)")]
    public AudioSource audioSource; // 记得把物体身上的 AudioSource 组件拖进来！
    public AudioClip dialogueAudio; // 在这里拖入每只兔子各自的台词文件

    private bool isPlaying = false;

    void Start()
    {
        // 游戏开始时，显示站立图片
        if (idleSprite != null && targetImage != null)
        {
            targetImage.sprite = idleSprite;
        }

        // 【防遮挡建议】如果你发现点击不灵敏，可以加上这句
        if (targetImage != null)
        {
            targetImage.alphaHitTestMinimumThreshold = 0.1f;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isPlaying)
        {
            StartCoroutine(PlayAnimation());
        }
    }

    IEnumerator PlayAnimation()
    {
        isPlaying = true;

        // ---【新增：播放音效】---
        if (audioSource != null && dialogueAudio != null)
        {
            // PlayOneShot 适合播放这种一次性的台词或音效
            audioSource.PlayOneShot(dialogueAudio);
        }
        else
        {
            // 如果你测试时没声音，这行字会出现在控制台提醒你
            if(audioSource == null) Debug.LogWarning(gameObject.name + ": 忘记拖 AudioSource 组件了！");
            if(dialogueAudio == null) Debug.LogWarning(gameObject.name + ": 忘记拖音频文件了！");
        }
        // -----------------------

        // 如果有粒子特效，开启一个独立的倒计时来播放
        if (leavesParticles != null)
        {
            StartCoroutine(PlayParticlesDelayed());
        }

        // 循环播放数组里的每一张图
        if (jumpFrames != null)
        {
            foreach (Sprite frame in jumpFrames)
            {
                if (targetImage != null) targetImage.sprite = frame;
                yield return new WaitForSeconds(frameRate);
            }
        }

        // 播完了，变回站立状态
        if (idleSprite != null && targetImage != null)
        {
            targetImage.sprite = idleSprite;
        }

        isPlaying = false;
    }

    IEnumerator PlayParticlesDelayed()
    {
        yield return new WaitForSeconds(particleDelay);
        if (leavesParticles != null) leavesParticles.Play();
    }
}