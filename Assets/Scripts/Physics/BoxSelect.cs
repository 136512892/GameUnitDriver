using UnityEngine;
using QuickOutline;
using System.Collections.Generic;

public class BoxSelect : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField] 
    private LineRenderer lineRenderer;
    private Vector3 screenP1;
    private Vector3 screenP2;

    private Vector3 worldP1;
    private Vector3 worldP2;
    private RaycastHit hitInfo;
    //缓存列表
    private readonly List<Outline> cacheList = new List<Outline>();

    private void Update()
    {
        //鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            //激活LineRenderer组件，开始绘制
            lineRenderer.enabled = true;
            //根据鼠标位置获得第一个顶点的坐标
            screenP1 = Input.mousePosition;
            screenP1.z = 1f;
            //通过射线投射检测获取世界空间中的第一个顶点
            if (Physics.Raycast(mainCamera.ScreenPointToRay(
                Input.mousePosition), out hitInfo, 
                100f, 1 << LayerMask.NameToLayer("Ground")))
            {
                worldP1 = hitInfo.point;
            }
        }
        //鼠标过拽过程中
        if (Input.GetMouseButton(0))
        {
            //根据鼠标位置获得第二个顶点的坐标
            screenP2 = Input.mousePosition;
            screenP2.z = 1f;
            //另外两个顶点的坐标通过p1和p2获得
            Vector3 screenP3 = new Vector3(screenP2.x, screenP1.y, 1f);
            Vector3 screenP4 = new Vector3(screenP1.x, screenP2.y, 1f);
            //屏幕坐标转世界坐标 设置到LineRenderer中
            lineRenderer.SetPosition(0, 
                mainCamera.ScreenToWorldPoint(screenP1));
            lineRenderer.SetPosition(1, 
                mainCamera.ScreenToWorldPoint(screenP3));
            lineRenderer.SetPosition(2, 
                mainCamera.ScreenToWorldPoint(screenP2));
            lineRenderer.SetPosition(3, 
                mainCamera.ScreenToWorldPoint(screenP4));
        }
        //鼠标左键抬起 框选结束
        if (Input.GetMouseButtonUp(0))
        {
            //取消激活LineRenderer组件
            lineRenderer.enabled = false;
            //清除上一次框选内容
            for (int i = 0; i < cacheList.Count; i++)
                cacheList[i].enabled = false;
            cacheList.Clear();
            //通过射线投射检测获取世界空间中的第二个顶点
            if (Physics.Raycast(mainCamera.ScreenPointToRay(
                Input.mousePosition), out hitInfo,
                100f, 1 << LayerMask.NameToLayer("Ground")))
            {
                worldP2 = hitInfo.point;
                //盒体的中心
                Vector3 center = new Vector3(
                    (worldP1.x + worldP2.x) * .5f,
                    .5f,
                    (worldP1.z + worldP2.z) * .5f);
                //盒体各维度大小的一半
                Vector3 halfExtents = new Vector3(
                    Mathf.Abs(worldP2.x - worldP1.x) * .5f,
                    .5f,
                    Mathf.Abs(worldP2.z - worldP1.z) * .5f);
                //盒体重叠检测
                Collider[] colliders = Physics.OverlapBox(center, halfExtents);
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].TryGetComponent<Outline>(out var outline))
                    {
                        outline.enabled = true;
                        cacheList.Add(outline);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Input.GetMouseButton(0))
        {
            Vector3 center = new Vector3(
                (worldP1.x + worldP2.x) * .5f,
                .5f,
                (worldP1.z + worldP2.z) * .5f);
            //盒体各维度大小的一半
            Vector3 size = new Vector3(
                Mathf.Abs(worldP2.x - worldP1.x),
                1f,
                Mathf.Abs(worldP2.z - worldP1.z));
            Gizmos.DrawWireCube(center, size);
        }
    }
}