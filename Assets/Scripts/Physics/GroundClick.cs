using UnityEngine;

public class GroundClick : MonoBehaviour
{
    //主相机
    [SerializeField]
    private Camera mainCamera;
    //地面点击特效
    [SerializeField]
    private ParticleSystem effect;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main != null 
                ? Camera.main
                : FindObjectOfType<Camera>();
        }
    }

    private void Update()
    {
        //鼠标左键单击
        if (Input.GetMouseButtonDown(0) && mainCamera != null)
        {
            //从鼠标位置创建射线
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                //根据标签判断点击到的物体是否为地面
                if (hitInfo.collider.CompareTag("Ground"))
                {
                    Debug.Log(string.Format("点击地面{0}", hitInfo.point));
                    //实例化特效
                    var instance = Instantiate(effect);
                    //特效坐标设为碰撞位置加上方一定单位
                    instance.transform.position = hitInfo.point + Vector3.up * .1f;
                    //根据法线方向设置特效的上方
                    instance.transform.up = hitInfo.normal;
                    //播放
                    instance.Play();
                }
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("点击地面产生特效");
    }
}