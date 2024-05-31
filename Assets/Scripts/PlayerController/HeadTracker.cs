using UnityEngine;

public class HeadTracker : MonoBehaviour
{
    private Camera mainCamera; //主相机
    private Transform head; //头部
    [SerializeField] private Animator animator;
    [Tooltip("水平方向上的角度限制"), SerializeField] 
    private Vector2 horizontalAngleLimit = new Vector2(-70f, 70f);
    [Tooltip("垂直方向上的角度限制"), SerializeField] 
    private Vector2 verticalAngleLimit = new Vector2(-60f, 60f);

    private float angleX;
    private float angleY;
    [Tooltip("插值速度"), SerializeField] 
    private float lerpSpeed = 5f;

    private void Start()
    {
        mainCamera = Camera.main != null
            ? Camera.main : FindObjectOfType<Camera>();
        head = animator.GetBoneTransform(HumanBodyBones.Head);
    }

    private void LateUpdate()
    {
        LookAtPosition(GetLookAtPosition());
    }
    /// <summary>
    /// 看向某点
    /// </summary>
    /// <param name="position">看向的点</param>
    public void LookAtPosition(Vector3 position)
    {
        Quaternion lookRotation = Quaternion.LookRotation(
            position - head.position);
        Vector3 eulerAngles = lookRotation.eulerAngles 
            - animator.transform.rotation.eulerAngles;
        float x = NormalizeAngle(eulerAngles.x);
        float y = NormalizeAngle(eulerAngles.y);
        angleX = Mathf.Clamp(Mathf.Lerp(angleX, x, 
            Time.deltaTime * lerpSpeed),
            verticalAngleLimit.x, verticalAngleLimit.y);
        angleY = Mathf.Clamp(Mathf.Lerp(angleY, y, 
            Time.deltaTime * lerpSpeed),
            horizontalAngleLimit.x, horizontalAngleLimit.y);
        Quaternion rotY = Quaternion.AngleAxis(
            angleY, head.InverseTransformDirection(animator.transform.up));
        head.rotation *= rotY;
        Quaternion rotX = Quaternion.AngleAxis(
            angleX, head.InverseTransformDirection(
                animator.transform.TransformDirection(Vector3.right)));
        head.rotation *= rotX;
    }

    //获取看向的位置
    private Vector3 GetLookAtPosition()
    {
        //相机前方一定单位的位置
        Vector3 position = mainCamera.transform.position 
            + mainCamera.transform.forward * 100f;
        //看向的方向
        Quaternion lookRotation = Quaternion.LookRotation(
            position - head.position, animator.transform.up);
        Vector3 angle = lookRotation.eulerAngles 
            - animator.transform.eulerAngles;
        float x = NormalizeAngle(angle.x);
        float y = NormalizeAngle(angle.y);
        //是否在限制值范围内
        bool isInRange = x >= verticalAngleLimit.x && x <= verticalAngleLimit.y
            && y >= horizontalAngleLimit.x && y <= horizontalAngleLimit.y;
        return isInRange ? position 
            : (head.position + animator.transform.forward);
    }
    //角度标准化
    private float NormalizeAngle(float angle)
    {
        if (angle > 180) angle -= 360f;
        else if (angle < -180) angle += 360f;
        return angle;
    }
}