#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Video; // 引入视频命名空间
using System.IO;

public class SnapshotTool
{
    [MenuItem("Tools/【点我】截取当前视频封面 (新版)")]
    public static void CaptureSelectedVideo()
    {
        // 1. 获取你在 Hierarchy 窗口里选中的物体
        GameObject selectedObj = Selection.activeGameObject;

        if (selectedObj == null)
        {
            EditorUtility.DisplayDialog("提示", "请先在左侧 Hierarchy 列表中\n选中那个播放视频的物体！", "好");
            return;
        }

        // 2. 尝试获取该物体上的 VideoPlayer 组件
        VideoPlayer vp = selectedObj.GetComponent<VideoPlayer>();
        
        // 如果找不到 VideoPlayer，再试试有没有 RawImage (以防你选的是 UI)
        RenderTexture rt = null;

        if (vp != null && vp.targetTexture != null)
        {
            rt = vp.targetTexture;
        }
        else 
        {
            // 没找到 VideoPlayer，尝试找 RawImage
            var rawImage = selectedObj.GetComponent<UnityEngine.UI.RawImage>();
            if (rawImage != null && rawImage.texture is RenderTexture)
            {
                rt = (RenderTexture)rawImage.texture;
            }
        }

        // 3. 如果还是没找到 RenderTexture
        if (rt == null)
        {
            EditorUtility.DisplayDialog("失败", "你选中的物体上没有找到 'Render Texture'！\n\n请确认：\n1. 你选中了挂着 VideoPlayer 的物体。\n2. VideoPlayer 的 'Target Texture' 属性里已经拖入了纹理。", "检查一下");
            return;
        }

        // --- 开始截图 (和之前一样) ---
        Debug.Log($"正在截取: {selectedObj.name} 的画面 ({rt.width}x{rt.height})");

        // 关键：临时把这个 RT 设为“激活”，强行让系统关注它
        RenderTexture.active = rt;

        Texture2D screenshot = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        screenshot.Apply();

        // 截完后释放激活状态，避免副作用
        RenderTexture.active = null;

        byte[] bytes = screenshot.EncodeToPNG();
        Object.DestroyImmediate(screenshot);

        string path = Application.dataPath + "/VideoCover_Perfect.png";
        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("成功", "截图成功！\n\n图片已保存在 Assets 根目录下。", "OK");
    }
}
#endif