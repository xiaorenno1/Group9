using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class SimpleClickVideo : MonoBehaviour
{
    [Header("资源设置")]
    public string videoName = "video"; // Resources中的视频文件名
    public string audioName = "audio"; // Resources中的音频文件名
    public bool loopVideo = false; // 是否循环播放
    public bool loopAudio = false; // 是否循环播放音频

    [Header("音频延迟设置")]
    public float audioDelay = 2.0f; // 音频延迟播放时间（秒）

    [Header("点击区域设置")]
    [Tooltip("播放区域(屏幕坐标，左下角为0,0)")]
    public Rect playArea = new Rect(0, 0, 200, 200);

    [Tooltip("是否显示点击区域调试框")]
    public bool showDebugAreas = true;

    [Header("显示设置")]
    public UnityEngine.UI.RawImage displayUI; // 在UI上显示（可选）

    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    private AudioClip audioClip;
    private bool isVideoReady = false;
    private Coroutine delayedAudioCoroutine;

    void Start()
    {
        SetupVideoPlayer();
        SetupAudioSource();
        LoadAndPrepareVideo();
        LoadAudioClip();
    }

    void SetupVideoPlayer()
    {
        // 获取或添加VideoPlayer组件
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
            videoPlayer = gameObject.AddComponent<VideoPlayer>();

        // 基础设置
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = loopVideo;

        // 显示设置
        if (displayUI != null)
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        }
        else
        {
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            videoPlayer.targetCamera = Camera.main;
        }
    }

    void SetupAudioSource()
    {
        // 添加AudioSource组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 基础设置
        audioSource.playOnAwake = false;
        audioSource.loop = loopAudio;
    }

    void LoadAndPrepareVideo()
    {
        // 从Resources加载视频
        VideoClip clip = Resources.Load<VideoClip>(videoName);
        if (clip == null)
        {
            Debug.LogError($"找不到视频: Resources/{videoName}");
            return;
        }

        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void LoadAudioClip()
    {
        // 从Resources加载音频
        audioClip = Resources.Load<AudioClip>(audioName);
        if (audioClip == null)
        {
            Debug.LogError($"找不到音频: Resources/{audioName}");
            return;
        }

        audioSource.clip = audioClip;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        isVideoReady = true;
        Debug.Log("视频准备就绪");

        // 如果使用UI显示，设置纹理
        if (displayUI != null)
        {
            displayUI.texture = vp.texture;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;

            // 检测是否点击在播放区域内
            if (playArea.Contains(mousePos))
            {
                PlayMedia();
            }
        }
    }

    void PlayMedia()
    {
        if (!isVideoReady)
        {
            Debug.Log("视频正在准备中...");
            return;
        }

        // 停止之前的延迟播放协程（如果存在）
        if (delayedAudioCoroutine != null)
        {
            StopCoroutine(delayedAudioCoroutine);
        }

        // 立即播放视频
        if (isVideoReady && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
            Debug.Log("播放视频");
        }

        // 延迟播放音频
        if (audioClip != null && !audioSource.isPlaying)
        {
            delayedAudioCoroutine = StartCoroutine(PlayAudioWithDelay());
        }
    }

    IEnumerator PlayAudioWithDelay()
    {
        if (audioDelay > 0)
        {
            Debug.Log($"音频将在 {audioDelay} 秒后播放");
            yield return new WaitForSeconds(audioDelay);
        }

        if (!audioSource.isPlaying && audioClip != null)
        {
            audioSource.Play();
            Debug.Log("播放音频");
        }
    }

    // 停止所有播放（包括延迟中的音频）
    public void StopAll()
    {
        // 停止视频
        if (videoPlayer.isPlaying)
            videoPlayer.Stop();

        // 停止音频
        if (audioSource.isPlaying)
            audioSource.Stop();

        // 停止延迟播放协程
        if (delayedAudioCoroutine != null)
        {
            StopCoroutine(delayedAudioCoroutine);
            delayedAudioCoroutine = null;
        }

        Debug.Log("停止所有播放");
    }

    // 在Scene视图中绘制调试区域
    void OnDrawGizmos()
    {
        if (!showDebugAreas) return;

        // 绘制播放区域
        DrawRectGizmo(playArea, Color.green, "播放区域");
    }

    void DrawRectGizmo(Rect screenRect, Color color, string label)
    {
#if UNITY_EDITOR
        // 将屏幕坐标转换为世界坐标以便在Scene视图中显示
        Gizmos.color = color;

        if (Camera.main != null)
        {
            // 计算矩形的四个角
            Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(screenRect.x, screenRect.y, 10));
            Vector3 bottomRight = Camera.main.ScreenToWorldPoint(new Vector3(screenRect.x + screenRect.width, screenRect.y, 10));
            Vector3 topLeft = Camera.main.ScreenToWorldPoint(new Vector3(screenRect.x, screenRect.y + screenRect.height, 10));
            Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(screenRect.x + screenRect.width, screenRect.y + screenRect.height, 10));

            // 绘制矩形边框
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);

            // 绘制对角线
            Gizmos.DrawLine(bottomLeft, topRight);
            Gizmos.DrawLine(topLeft, bottomRight);

            // 计算中心点并显示标签
            Vector3 center = Camera.main.ScreenToWorldPoint(new Vector3(screenRect.x + screenRect.width / 2, screenRect.y + screenRect.height / 2, 10));
            UnityEditor.Handles.Label(center, label);
        }
#endif
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.Stop();
        }

        // 停止所有协程
        if (delayedAudioCoroutine != null)
        {
            StopCoroutine(delayedAudioCoroutine);
        }
    }
}