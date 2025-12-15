using System.Collections;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
[RequireComponent(typeof(AudioSource))]
public class VideoAreaController : MonoBehaviour
{
    [Header("【必需】拖入你的音频文件")]
    public AudioClip targetAudioClip;

    [Header("音频设置")]
    [Tooltip("点击播放后，延迟多少秒才开始播放声音")]
    [Range(0f, 5f)] // 加个滑块限制，方便调节
    public float audioDelay = 0.5f;

    [Header("区域设置")]
    [Tooltip("勾选以显示绿色的调试框")]
    public bool debugMode = true;
    [Tooltip("设置点击区域：X, Y (左下角为原点), 宽度, 高度")]
    public Rect clickArea = new Rect(100, 100, 400, 300);

    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    // 用于标记是否已经完成了初始化（刷出了第一帧）
    private bool isInitialized = false;
    // 用于存储当前的音频延迟计时器，方便随时取消
    private Coroutine audioCoroutine;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        audioSource = GetComponent<AudioSource>();

        // --- 关键设置 ---
        videoPlayer.playOnAwake = false;     
        videoPlayer.waitForFirstFrame = true;
        
        // 音频设置
        audioSource.playOnAwake = false;
        audioSource.clip = targetAudioClip;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
    }

    void Start()
    {
        // 启动强制刷帧的流程
        StartCoroutine(ForceShowFirstFrame());
    }

    // --- 核心黑科技：强制刷出第一帧画面 ---
    IEnumerator ForceShowFirstFrame()
    {
        Debug.Log("正在准备视频...");
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        Debug.Log("准备完毕，强制刷新画面...");

        // 1. 开始播放 (为了让画面动一下)
        videoPlayer.Play();

        // 2. 等待极短的时间 (0.1秒)
        yield return new WaitForSeconds(0.1f);

        // 3. 立刻暂停
        videoPlayer.Pause();

        isInitialized = true; // 标记完成
        Debug.Log("第一帧已就位，等待点击。");
    }

    void Update()
    {
        if (!isInitialized) return;

        if (Input.GetMouseButtonDown(0))
        {
            // 检查鼠标是否在区域内
            if (clickArea.Contains(Input.mousePosition))
            {
                TogglePlayState();
            }
        }
    }

    // 切换播放/暂停状态
    void TogglePlayState()
    {
        if (videoPlayer.isPlaying)
        {
            // === 暂停逻辑 ===
            videoPlayer.Pause();
            audioSource.Pause();

            // 【关键】如果此时音频还在读秒等待播放，立刻取消它！
            // 否则会出现：用户点了暂停，结果0.5秒后声音突然响了的bug
            if (audioCoroutine != null)
            {
                StopCoroutine(audioCoroutine);
                audioCoroutine = null;
            }
        }
        else
        {
            // === 播放逻辑 ===
            videoPlayer.Play();

            // 开启一个新的协程来处理音频延迟
            // 如果之前有正在跑的协程，先停掉（双重保险）
            if (audioCoroutine != null) StopCoroutine(audioCoroutine);
            
            audioCoroutine = StartCoroutine(PlayAudioDelayed());
        }
    }

    // 专门处理音频延迟的协程
    IEnumerator PlayAudioDelayed()
    {
        // 如果设置了延迟，就等待
        if (audioDelay > 0)
        {
            yield return new WaitForSeconds(audioDelay);
        }

        // 等待结束，播放音频
        // 再次检查视频是否还在播放（防止极端情况下用户手速过快导致的状态不同步）
        if (videoPlayer.isPlaying)
        {
            audioSource.Play();
        }
    }

    // 绘制调试框
    void OnGUI()
    {
        if (!debugMode) return;

        Rect visualRect = new Rect(clickArea.x, Screen.height - clickArea.y - clickArea.height, clickArea.width, clickArea.height);

        if (!isInitialized)
        {
            GUI.color = Color.gray;
            GUI.Box(visualRect, "初始化中...");
        }
        else if (videoPlayer.isPlaying)
        {
            GUI.color = Color.green;
            GUI.Box(visualRect, $"播放中\n(音频延迟: {audioDelay}s)");
        }
        else
        {
            GUI.color = Color.yellow;
            GUI.Box(visualRect, "已就绪 (点击播放)");
        }
    }
}