using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [Header("视频设置")]
    public string video1Name = "video1";
    public string video2Name = "video2";
    public bool loopVideo = false;

    [Header("音频设置")]
    public string audio1Name = "audio1";
    public string audio2Name = "audio2";
    public bool loopAudio = false;

    [Header("点击区域设置")]
    [Tooltip("视频1/音频1的点击区域(屏幕坐标，左下角为0,0)")]
    public Rect area1 = new Rect(0, 0, 200, 200);

    [Tooltip("视频2/音频2的点击区域(屏幕坐标，左下角为0,0)")]
    public Rect area2 = new Rect(300, 0, 200, 200);

    [Tooltip("是否显示点击区域调试框")]
    public bool showDebugAreas = true;

    [Header("控制模式")]
    public ControlMode controlMode = ControlMode.VideoAndAudio;

    private VideoPlayer videoPlayer;
    private AudioSource audioSource1;
    private AudioSource audioSource2;

    private bool isVideoReady = false;
    private VideoClip targetClip;
    private string targetVideoName;
    private bool isSwitchingVideo = false;

    private AudioClip audioClip1;
    private AudioClip audioClip2;

    public enum ControlMode
    {
        VideoOnly,      // 仅视频
        AudioOnly,      // 仅音频
        VideoAndAudio   // 视频和音频都控制
    }

    void Start()
    {
        // 设置视频播放器
        SetupVideoPlayer();

        // 设置音频源
        SetupAudioSources();

        // 加载音频
        LoadAudioClips();

        // 默认预加载第一个视频
        if (controlMode != ControlMode.AudioOnly)
        {
            LoadAndPrepareVideo(video1Name, "视频1");
        }
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

        // 使用相机渲染模式
        videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
        videoPlayer.targetCamera = Camera.main;
    }

    void SetupAudioSources()
    {
        // 创建两个AudioSource组件
        audioSource1 = gameObject.AddComponent<AudioSource>();
        audioSource2 = gameObject.AddComponent<AudioSource>();

        // 设置AudioSource属性
        audioSource1.playOnAwake = false;
        audioSource1.loop = loopAudio;

        audioSource2.playOnAwake = false;
        audioSource2.loop = loopAudio;
    }

    void LoadAudioClips()
    {
        // 从Resources加载音频文件
        audioClip1 = Resources.Load<AudioClip>(audio1Name);
        audioClip2 = Resources.Load<AudioClip>(audio2Name);

        if (audioClip1 == null)
            Debug.LogError($"找不到音频1: Resources/{audio1Name}");
        else
            audioSource1.clip = audioClip1;

        if (audioClip2 == null)
            Debug.LogError($"找不到音频2: Resources/{audio2Name}");
        else
            audioSource2.clip = audioClip2;
    }

    void LoadAndPrepareVideo(string videoName, string displayName)
    {
        // 从Resources加载视频
        VideoClip clip = Resources.Load<VideoClip>(videoName);
        if (clip == null)
        {
            Debug.LogError($"找不到视频: Resources/{videoName}");
            return;
        }

        targetClip = clip;
        targetVideoName = displayName;
        videoPlayer.clip = clip;

        // 移除之前的准备完成事件
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.prepareCompleted += OnVideoPrepared;

        videoPlayer.Prepare();
        isVideoReady = false;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        isVideoReady = true;
        Debug.Log($"{targetVideoName} 准备就绪");

        // 如果是切换视频过程中准备的，自动开始播放
        if (isSwitchingVideo)
        {
            videoPlayer.Play();
            Debug.Log($"播放: {targetVideoName}");
            isSwitchingVideo = false;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;

            // 检测是否点击在区域1内
            if (area1.Contains(mousePos))
            {
                HandleArea1Click();
            }
            // 检测是否点击在区域2内
            else if (area2.Contains(mousePos))
            {
                HandleArea2Click();
            }
        }
    }

    void HandleArea1Click()
    {
        switch (controlMode)
        {
            case ControlMode.VideoOnly:
                HandleVideoClick(video1Name, "视频1");
                break;

            case ControlMode.AudioOnly:
                ToggleAudioPlayback(audioSource1, audioClip1, "音频1");
                break;

            case ControlMode.VideoAndAudio:
                // 同时控制视频和音频
                HandleVideoClick(video1Name, "视频1");
                ToggleAudioPlayback(audioSource1, audioClip1, "音频1");
                break;
        }
    }

    void HandleArea2Click()
    {
        switch (controlMode)
        {
            case ControlMode.VideoOnly:
                HandleVideoClick(video2Name, "视频2");
                break;

            case ControlMode.AudioOnly:
                ToggleAudioPlayback(audioSource2, audioClip2, "音频2");
                break;

            case ControlMode.VideoAndAudio:
                // 同时控制视频和音频
                HandleVideoClick(video2Name, "视频2");
                ToggleAudioPlayback(audioSource2, audioClip2, "音频2");
                break;
        }
    }

    void HandleVideoClick(string videoName, string displayName)
    {
        if (!isVideoReady) return;

        // 检查是否是同一个视频
        bool isSameVideo =
            (videoName == video1Name && targetVideoName == "视频1") ||
            (videoName == video2Name && targetVideoName == "视频2");

        // 如果是同一个视频，切换播放/暂停状态
        if (isSameVideo && isVideoReady)
        {
            ToggleVideoPlayback();
            return;
        }

        // 如果是不同视频，切换到新视频
        if (!isSameVideo)
        {
            isSwitchingVideo = true;
            isVideoReady = false;

            // 停止当前播放
            if (videoPlayer.isPlaying)
                videoPlayer.Stop();

            // 加载新视频
            LoadAndPrepareVideo(videoName, displayName);
        }
    }

    void ToggleVideoPlayback()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            Debug.Log($"{targetVideoName} 已暂停");
        }
        else
        {
            videoPlayer.Play();
            Debug.Log($"播放: {targetVideoName}");
        }
    }

    void ToggleAudioPlayback(AudioSource source, AudioClip clip, string audioName)
    {
        if (clip == null || source == null)
        {
            Debug.LogError($"{audioName} 未加载成功");
            return;
        }

        // 切换播放/暂停状态
        if (source.isPlaying)
        {
            source.Pause();
            Debug.Log($"{audioName} 已暂停");
        }
        else
        {
            source.Play();
            Debug.Log($"播放: {audioName}");
        }
    }

    // 停止所有音频（可选功能）
    public void StopAllAudio()
    {
        if (audioSource1 != null) audioSource1.Stop();
        if (audioSource2 != null) audioSource2.Stop();
    }

    // 停止所有视频（可选功能）
    public void StopAllVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();
    }

    // 在Scene视图中绘制调试区域
    void OnDrawGizmos()
    {
        if (!showDebugAreas) return;

        // 绘制区域1
        DrawRectGizmo(area1, Color.green, "区域1");

        // 绘制区域2
        DrawRectGizmo(area2, Color.blue, "区域2");
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
    }
}