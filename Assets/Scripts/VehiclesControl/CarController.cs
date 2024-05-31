using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private Transform com;

    [Header("Wheel")]
    //车轮
    [SerializeField] private Transform w_fl;
    [SerializeField] private Transform w_fr;
    [SerializeField] private Transform w_rl;
    [SerializeField] private Transform w_rr;

    //车轮碰撞器
    [SerializeField] private WheelCollider wc_fl;
    [SerializeField] private WheelCollider wc_fr;
    [SerializeField] private WheelCollider wc_rl;
    [SerializeField] private WheelCollider wc_rr;

    [Header("Engine")]
    [SerializeField] 
    private DriverType driverType = DriverType.All;
    //驱动类型（前驱、后驱、四驱）
    public enum DriverType { Front, Rear, All };

    [SerializeField] 
    private float motorTorqueFactor = 2000f;
    [SerializeField] 
    private float brakeTorqueFactor = 1000f;

    private float currentMotorTorque;
    private float currentBrakeTorque;

    [SerializeField]
    private float steeringAngleLimit = 45f;
    private float currentSteeringAngle;

    private float km; //车速 km/h

    //驱动防滑系统
    public bool ASR = true;
    //制动防抱死系统
    public bool ABS = true;

    [SerializeField]
    private GameObject[] brakeLights;

    [SerializeField]
    private ParticleSystem[] smokes;

    [SerializeField] private AudioSource engineOnSound;
    [SerializeField] private AudioSource brakeSound;
    [SerializeField] private AudioSource impactSound;

    [SerializeField] private float deformationThreshold = 5f;
    [SerializeField] private float deformationRadius = .5f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = com.localPosition;
    }

    private void FixedUpdate()
    {
        //刹车
        currentBrakeTorque = Input.GetKey(KeyCode.Space)
            ? brakeTorqueFactor : 0f;
        for (int i = 0; i < brakeLights.Length; i++)
            brakeLights[i].SetActive(currentBrakeTorque > 0f);
        ApplyBrakeTorque(wc_fl, currentBrakeTorque);
        ApplyBrakeTorque(wc_fr, currentBrakeTorque);
        ApplyBrakeTorque(wc_rl, currentBrakeTorque);
        ApplyBrakeTorque(wc_rr, currentBrakeTorque);

        //驱动
        currentMotorTorque = currentBrakeTorque != 0f
            ? 0f
            : Input.GetAxis("Vertical") * motorTorqueFactor;
        switch (driverType)
        {
            case DriverType.Front:
                ApplyMotorTorque(wc_fl, currentMotorTorque);
                ApplyMotorTorque(wc_fr, currentMotorTorque);
                break;
            case DriverType.Rear:
                ApplyMotorTorque(wc_rl, currentMotorTorque);
                ApplyMotorTorque(wc_rr, currentMotorTorque);
                break;
            case DriverType.All:
                ApplyMotorTorque(wc_fl, currentMotorTorque * .5f);
                ApplyMotorTorque(wc_fr, currentMotorTorque * .5f);
                ApplyMotorTorque(wc_rl, currentMotorTorque * .5f);
                ApplyMotorTorque(wc_rr, currentMotorTorque * .5f);
                break;
        }

        //转向
        currentSteeringAngle = Input.GetAxis("Horizontal") * steeringAngleLimit;
        wc_fl.steerAngle = currentSteeringAngle;
        wc_fr.steerAngle = currentSteeringAngle;

        //车速计算
        float circumference = wc_rl.radius * 2 * Mathf.PI;
        float rpm = (Mathf.Abs(wc_rl.rpm) + Mathf.Abs(wc_rr.rpm)) * .5f;
        km = Mathf.Round(circumference * rpm * 60 * .001f);

        //车轮姿态
        ApplyWheelPose(w_fl, wc_fl);
        ApplyWheelPose(w_fr, wc_fr);
        ApplyWheelPose(w_rl, wc_rl);
        ApplyWheelPose(w_rr, wc_rr);

        //尾气排放
        for (int i = 0; i < smokes.Length; i++)
        {
            ParticleSystem smoke = smokes[i];
            if (km <= 20f)
            {
                var emission = smoke.emission;
                if (!emission.enabled)
                    emission.enabled = true;
                emission.rateOverTime = Input.GetAxis("Vertical") > .1f
                    ? 20 : 5;
            }
            else
            {
                if (smoke.emission.enabled)
                {
                    var emission = smoke.emission;
                    emission.enabled = false;
                }
            }
        }

        Sounds();
    }

    private void OnGUI()
    {
        GUILayout.Label(string.Format("{0} km/h", km), 
            new GUIStyle(GUI.skin.label) 
            { fontSize = 25, fontStyle = FontStyle.Bold });
    }

    //应用驱动扭矩
    private void ApplyMotorTorque(WheelCollider wc, float motorTorque)
    {
        //wc.motorTorque = motorTorque;
        wc.motorTorque = ASR 
            && wc.GetGroundHit(out WheelHit hit)
            && hit.forwardSlip > .35f
                ? 0f
                : motorTorque;
    }

    //应用制动扭矩
    private void ApplyBrakeTorque(WheelCollider wc, float brakeTorque)
    {
        //wc.brakeTorque = brakeTorque;
        wc.brakeTorque = ABS 
            && wc.GetGroundHit(out WheelHit hit) 
            && hit.forwardSlip > .35f
                ? 0f
                : brakeTorque;
    }

    //根据车轮碰撞器应用车轮的坐标及旋转
    private void ApplyWheelPose(Transform w, WheelCollider wc)
    {
        wc.GetWorldPose(out Vector3 pos, out Quaternion rot);
        w.position = pos;
        w.rotation = rot;
    }

    private void Sounds()
    {
        engineOnSound.volume = Mathf.Lerp(engineOnSound.volume, 
            Mathf.Lerp(.2f, 1f, km / 255f),
            Time.deltaTime * 15f);
        engineOnSound.pitch = Mathf.Lerp(engineOnSound.pitch,
            Mathf.Lerp(.5f, 1.5f, km / 255f),
            Time.deltaTime * 15f);

        brakeSound.volume = Mathf.Lerp(0f, 1f,
            Input.GetKey(KeyCode.Space) ? km / 255f : 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //发生撞击
        if (collision.relativeVelocity.magnitude >= deformationThreshold)
        {
            impactSound.volume = Mathf.Lerp(.2f, .5f, km / 255f);
            impactSound.Play();
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contactPoint = collision.GetContact(i);
                Vector3 world2Local = transform
                    .InverseTransformPoint(contactPoint.point); //转局部坐标
                for (int j = 0; j < vertices.Length; j++)
                {
                    float magnitude = (world2Local - vertices[j]).magnitude;
                    if (magnitude < deformationRadius)
                    {
                        float delta = (deformationRadius - magnitude) 
                            / deformationRadius * .01f;
                        vertices[j] += transform.InverseTransformDirection(
                            collision.relativeVelocity) * delta;
                    }
                }
            }
            //更新网格顶点
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            //重启碰撞
            var collider = GetComponent<Collider>();
            collider.enabled = false;
            collider.enabled = true;
        }
    }
}