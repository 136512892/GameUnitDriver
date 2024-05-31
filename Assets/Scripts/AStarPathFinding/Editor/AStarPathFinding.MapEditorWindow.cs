using UnityEngine;
using UnityEditor;

public class AStarPathFinding_MapEditorWindow : EditorWindow
{
    [MenuItem("AStar/Map Editor")]
    public static void Open()
    {
        GetWindow<AStarPathFinding_MapEditorWindow>().Show();
    }

    private int x = 10;
    private int y = 10;
    private float nodeSize = 1f;
    private Vector3 offset;
    private bool[,] map;

    private void OnEnable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        OnMapChanged();
    }
    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        x = EditorGUILayout.IntField("Grid X", x);
        y = EditorGUILayout.IntField("Grid Y", y);
        nodeSize = EditorGUILayout.FloatField("Grid Size", nodeSize);
        offset = EditorGUILayout.Vector3Field("Offset", offset);
        if (EditorGUI.EndChangeCheck())
        {
            OnMapChanged();
            SceneView.RepaintAll();
        }
        //生成地图
        if (GUILayout.Button("Generate Map Data"))
        {
            //选择保存路径
            string filePath = EditorUtility.SaveFilePanel(
                "Save Map Data", Application.dataPath,
                "New Map Data", "asset");
            if (!string.IsNullOrEmpty(filePath))
            {
                //转化为Asset路径
                filePath = filePath.Substring(
                    filePath.IndexOf("Assets"));
                //创建地图Texture
                Texture2D bitmap = new Texture2D(x, y, 
                    TextureFormat.Alpha8, false);
                byte[] bytes = bitmap.GetRawTextureData();
                for (int m = 0; m < x; m++)
                {
                    for (int n = 0; n < y; n++)
                    {
                        //0表示障碍区域 255表示可行走区域
                        bytes[m * x + n] = (byte)(map[m, n] ? 255 : 0);
                    }
                }
                bitmap.LoadRawTextureData(bytes);
                //创建、保存资产
                AssetDatabase.CreateAsset(bitmap, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                //选中
                EditorGUIUtility.PingObject(bitmap);
            }
        }
        EditorGUILayout.HelpBox(
            "Ctrl + Left Mouse Button: Draw where is obstructive." +
            "\r\nAlt + Left Mouse Button: Draw where is walkable.", 
            MessageType.Info);
    }
    private void OnSceneGUI(SceneView sceneView)
    {
        //绘制地图网格
        Handles.color = Color.cyan;
        for (int i = 0; i <= x; i++)
        {
            Vector3 start = i * nodeSize * Vector3.right;
            Vector3 end = start + y * nodeSize * Vector3.forward;
            Handles.DrawLine(start + offset, end + offset);
        }
        for (int i = 0; i <= y; i++)
        {
            Vector3 start = i * nodeSize * Vector3.forward;
            Vector3 end = start + x * nodeSize * Vector3.right;
            Handles.DrawLine(start + offset, end + offset);
        }
        HandleUtility.AddDefaultControl(
            GUIUtility.GetControlID(FocusType.Passive));
        //Ctrl + 鼠标左键 绘制障碍区域
        //Alt + 鼠标左键 绘制可行走区域
        var e = Event.current;
        if (e != null && (e.control || e.alt)
            && (e.type == EventType.MouseDown
                || e.type == EventType.MouseDrag)
            && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                int targetX = Mathf.CeilToInt(
                    (hit.point.x - offset.x) / nodeSize);
                int targetY = Mathf.CeilToInt(
                    (hit.point.z - offset.z) / nodeSize);
                if (targetX <= x && targetX > 0 && 
                    targetY <= y && targetY > 0)
                    map[targetX - 1, targetY - 1] = !e.control;
            }
            e.Use();
        }
        //绘制障碍区域
        Handles.color = Color.red;
        for (int m = 0; m < x; m++)
        {
            for (int n = 0; n < y; n++)
            {
                if (!map[m, n])
                    Handles.DrawWireCube(new Vector3(m * nodeSize, 0f,
                        n * nodeSize) + .5f * nodeSize *
                        (Vector3.forward + Vector3.right) + offset, 
                        .9f * nodeSize * (Vector3.forward + Vector3.right));
            }
        }
    }
    private void OnMapChanged()
    {
        map = new bool[x, y];
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                map[i, j] = true;
            }
        }
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
}