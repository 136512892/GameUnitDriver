using UnityEngine;

/// <summary>
/// 事件响应系统
/// </summary>
public class EventResponseSystem : MonoBehaviour
{
    //主相机
    [SerializeField] 
    private Camera mainCamera; 
    //启用时自动查找相机（主相机为空时起作用）
    [SerializeField] 
    private bool autoFindCameraOnEnable = true;
    //检测层级
    [SerializeField] 
    private LayerMask eventResponserLayerMask = -1;
    //检测的最大距离
    [SerializeField]
    private float maxDistance = 10f;

    /// <summary>
    /// 当前事件响应器
    /// </summary>
    public IEventResponser CurrentEventResponser { get; private set; }

    private void OnEnable()
    {
        if (mainCamera == null && autoFindCameraOnEnable)
            //如果Tag为MainCamera的主相机不存在 根据类型查找相机
            mainCamera = Camera.main != null ? Camera.main
                : FindObjectOfType<Camera>();
    }

    private void Update()
    {
        if (mainCamera == null) return;

        //在鼠标位置进行射线投射检测
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,  out RaycastHit hit,
            maxDistance, eventResponserLayerMask))
        {
            //检测到的碰撞器挂载事件响应器组件
            var eventResponser = hit.collider
                .GetComponent<IEventResponser>();
            if (eventResponser != null)
            {
                //首先执行当前的事件响应器的退出事件
                CurrentEventResponser?.OnExit();
                //更新当前的事件响应器
                CurrentEventResponser = eventResponser;
                //更新后执行当前的事件响应器的进入事件
                CurrentEventResponser.OnEnter();
            }
            //检测到的碰撞器未挂载事件响应器
            else
            {
                //如果当前事件响应器不为空，执行其退出事件并置为空
                if (CurrentEventResponser != null)
                {
                    CurrentEventResponser.OnExit();
                    CurrentEventResponser = null;
                }
            }
        }
        //未检测到任何碰撞器
        else
        {
            //如果当前事件响应器不为空，执行其退出事件并置为空
            if (CurrentEventResponser != null)
            {
                CurrentEventResponser.OnExit();
                CurrentEventResponser = null;
            }
        }

        //当前事件响应器不为空
        if (CurrentEventResponser != null)
        {
            //执行其停留事件
            CurrentEventResponser.OnStay();

            //鼠标点击执行其点击事件
            if (Input.GetMouseButtonDown(0))
                CurrentEventResponser.OnClick();
        }
    }

    private void OnDisable()
    {
        if (CurrentEventResponser != null)
        {
            CurrentEventResponser.OnExit();
            CurrentEventResponser = null;
        }
    }
}