using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetController : MonoBehaviour
{

    public CameraController mainCamera;
    public Transform jetMesh;
    public GameObject[] landingGears;

    public float engineThrust = 10000f;
    public float pitchSpeed = 30f;
    public float rollSpeed = 0.45f;
    public float yawSpeed = 25f;
    public float autoTurnAngle = 30f;

    public float Mess = 62.6f;  //质量
    public float GravityAcc = 10.0f;//重力加速度
    public float Aileron = 0;
    public float Elevator = 0;
    public float Rudder = 0;
    private Quaternion mainRot = Quaternion.identity;
    public float RotationSpeed = 50.0f;// Turn Speed
    public bool Stall = false;
    public float StallSpeed = 35.0f; //失速速度
    public float StallAngle = 30.0f; //失速迎角
    public float WingAngle0 = 3.0f; //机翼初始迎角
    public float LiftCoef = 1.0f; //升力系数
    public float Speed = 120.0f;// Speed
    public float ResistanceCoef = 0.1f; //阻力系数
    public float Brake = 1.0f;//刹车
    public float Resistance0 = 200.0f;
    public float Resistance; //阻力 = r0 + r_coef*v*v
    public float EnginePower = 2760.0f; //发动机推力
    public float AfterBurner = 1.0f; //加力
    public float Weight = 62.6f;
    public float Sensitive = 1.0f; //灵敏度
    public float MaxRollSpeed = 0.000003f; //最大滚转速率
    public float MaxPitchSpeed = 1.6f; //最大俯仰速率
    public float MaxYawSpeed = 0.4f; //最大偏航速率

    //output text
    public Vector3 lift;
    public Vector3 angle;
    public Quaternion rotation;
    public Vector3 v;
    public float v_value = 10;
    public Quaternion mainrot;


    private float normaloverload = 0f;
    public float tangentialoverload = 0f;
    public float rollangle = 0f;
    public float pitchangle = 0f;
    public float yawangle = 0f;
    public float vdot;
    public float pitchdot;
    public float yawdot;
    public bool rollflag = false;
    public bool pitchflag = false;

    public bool airturnflag = false;
    public bool airriseflag = false;


    private float turn = 0;
    private float delta = 0;
    private Vector2 aix = new Vector2(0, 0);


    public bool startInAir;
    public bool autoTakeOff;
    public bool autoLevel;

    private Camera cam;
    private Rigidbody rb;

    private float thrust;
    private float pitch;
    private float roll;
    private float yaw;
    private bool enableMouseControls;

    private bool landingGearsRetracted;

    internal float speed;
    internal float height;
    internal float throttle { get { return thrust; } }
    internal bool showCrosshairs;
    internal Vector3 crosshairPosition;


    private void Awake()
    {
        cam = Camera.main;

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        if (rb.mass == 1)
        {
            rb.mass = 20000;
            rb.drag = 0.075f;
            rb.angularDrag = 0.05f;
        }
    }

    private void Start()
    {
        if (startInAir)
        {
            transform.position = new Vector3(0, 20000, 0);
            thrust = 100f;
            rb.AddForce(transform.forward * 500f, ForceMode.VelocityChange);
        }

        if (autoTakeOff)
            thrust = 100f;
        transform.position = new Vector3(0, 1000, 0);
    }

    void Update()
    {
        //Clear out old values
        //pitch = 0f;
        //roll = 0f;
        //yaw = 0f;


        //Update control surfaces

        /*if (Input.GetKey(KeyCode.A)) turn = -1f;
        if (Input.GetKey(KeyCode.D)) turn = 1f;

        if (Input.GetKey(KeyCode.UpArrow)) aix.y = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) aix.y = -1f;
        if (Input.GetKey(KeyCode.LeftArrow)) aix.x = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) aix.x = 1f;

        if (Input.GetKey(KeyCode.W)) delta = 1f;
        if (Input.GetKey(KeyCode.S)) delta = -1f;
        */

        if (Input.GetKey(KeyCode.Alpha1))
        {
            normaloverload = 10f;
            tangentialoverload = 0f;


        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            /*   if (pitchangle == 0)
               {
                   pitchflag = false;
               }
               if (pitchflag == false)
               {
                   tangentialoverload = Mathf.Cos(3.14f * pitchangle / 180) / Mathf.Cos(3.14f * rollangle / 180) - 100f;
               }
               if (pitchangle < -30)
               {
                   pitchflag = true;
               }
               if (pitchflag == true)
               {
                   tangentialoverload = Mathf.Cos(3.14f * pitchangle / 180) / Mathf.Cos(3.14f * rollangle / 180) + 100f;

               }
               */
            airriseflag = true;
            pitchflag = false;
            normaloverload = 0f;


        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            /*if (rollangle == 0)
            {
                rollflag = false;
            }
            if (rollflag == false)
            {
                rollangle = Mathf.Clamp(rollangle - 5 * Time.deltaTime, -30f, 30f);
            }
            if (rollangle == 30f || rollangle == -30f)
            {
                rollflag = true;
            }

            if (rollflag == true)
            {
                //rollangle = 5f * v_value / (GravityAcc * tangentialoverload) * yawdot;
                rollangle = Mathf.Clamp(rollangle + 5 * Time.deltaTime, -30f, 0f);
            }
            */
            airturnflag = true;
            rollflag = false;
            normaloverload = 0f;
        }

        if (airriseflag)
        {
            airrise();
        }
        if (airturnflag)
        {
            airturn();
        }

        getdot();
        updateangle();

        UpdateCamera();


        /*AxisControl(aix);
        TurnControl(turn);
        SpeedUp(delta); 
        */

        //if (enableMouseControls) CheckMouseControls();

        //todo: update our height using a raycast
        height = transform.position.y - 1f;

        if (height > 5 && !landingGearsRetracted)
            RetractLandingGears();

        //if (landingGearsRetracted && !enableMouseControls)
        //  SetupMouseControls();
    }

    void RetractLandingGears()
    {
        landingGearsRetracted = true;
        for (int i = 0; i < landingGears.Length; i++)
        {
            landingGears[i].SetActive(false);
        }
    }



    void UpdateCamera()
    {
        mainCamera.updatePosition(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y"));
    }



    private void FixedUpdate()
    {
        if (!this.GetComponent<Rigidbody>())
            return;
        Quaternion AddRot = Quaternion.identity;
        //Vector3 velocityTarget = Vector3.zero;
        Quaternion VelocityRot = Quaternion.identity;
        /*
        Vector3 Gravity = -Mess * GravityAcc * Vector3.up;
        Vector3 Lift = Vector3.zero;//升力
        Vector3 Tail = Vector3.zero;//垂直尾翼的作用
        Vector3 Drag = Vector3.zero;//阻力
        Vector3 Push = Vector3.zero;//推力
        */
        //Vector3 velocity = rb.velocity; //速度矢量
        //姿态控制
        //{
        //VelocityRot.eulerAngles = velocity;//
        /*roll = Aileron;
        pitch = (3.1416f * (Vector3.Angle(velocity, rb.rotation * Vector3.up) - 90) / 180.0f + Elevator);
        yaw = -(3.1416f * (Vector3.Angle(velocity, rb.rotation * Vector3.right) - 90) / 180.0f - Rudder);
        */
        roll = rollangle;
        pitch = pitchangle;
        yaw = yawangle;
        AddRot.eulerAngles = new Vector3(pitch, yaw, -roll);
        mainRot = AddRot;
        angle = new Vector3(pitch, yaw, -roll);
        //mainRot = VelocityRot * AddRot;
        rb.rotation = Quaternion.Lerp(rb.rotation, mainRot, Time.fixedDeltaTime * RotationSpeed);
        rotation = rb.rotation;
        mainrot = mainRot;

        //}

        /*
        transform.RotateAround(transform.position, transform.up, yaw * Time.fixedDeltaTime * yawSpeed);     //Yaw

        transform.RotateAround(transform.position, transform.forward, roll * Time.fixedDeltaTime * rollSpeed);     //Roll

        //if (rb.velocity.magnitude > 100f)
        transform.RotateAround(transform.position, transform.right, pitch * Time.fixedDeltaTime * pitchSpeed);     //Pitch
        */

        //升力计算
        /*{
        Speed = velocity.magnitude;
        float WingAngle = WingAngle0 + Vector3.Angle(velocity, rb.rotation * Vector3.up) - 90.0f; //机翼迎角
        if (Speed < StallSpeed)
        {
            Lift = Vector3.zero;
            Stall = true;
        }
        else if (Mathf.Abs(WingAngle) > StallAngle)
        {
            Lift = -10.0f * ResistanceCoef * Speed * Speed * velocity.normalized;
            Stall = true;
        }
        else
        {
            Lift = rb.rotation * Vector3.up * LiftCoef * Speed * Speed * (WingAngle * 3.1416f / 180.0f);
            Stall = false;
        }
        //}
        lift = Lift;

        //垂直尾翼的作用
        //{
        float TailAngle = Rudder + Vector3.Angle(velocity, rb.rotation * Vector3.right) - 90.0f;
        Tail = rb.rotation * Vector3.right * LiftCoef / 20.0f * Speed * Speed * (TailAngle * 3.1416f / 180.0f);
        //}

        //阻力计算
        //{
        Resistance = Brake * Resistance0 + ResistanceCoef * Speed * Speed;
        Drag = -Resistance * velocity.normalized;
        //}

        //推力计算
        //{
        Push = AfterBurner * EnginePower * throttle * (rb.rotation * Vector3.forward);
        //}

        //动力学方程
        //{
        velocity += (Lift + Tail + Drag + Push + Gravity) / Weight * Time.deltaTime;
        */
        rb.velocity = rb.rotation * Vector3.forward * v_value;
        v_value = rb.velocity.magnitude;
        v = rb.velocity;
        //}
    }

    private void LateUpdate()
    {
        if (!enableMouseControls) return;
        crosshairPosition = cam.WorldToScreenPoint(transform.position + (transform.forward * 500f));
    }

    /*void AxisControl(Vector2 axis)
    {
        Aileron = Mathf.Clamp(axis.x * Sensitive * MaxRollSpeed, -MaxRollSpeed, MaxRollSpeed); //滚转控制
        Elevator = Mathf.Clamp(axis.y * Sensitive * MaxPitchSpeed, -MaxPitchSpeed, MaxPitchSpeed); //俯仰控制
        //roll = Mathf.Clamp(axis.x * Sensitive * MaxRollSpeed, -MaxRollSpeed, MaxRollSpeed);
        //pitch = Mathf.Clamp(axis.y * Sensitive * MaxPitchSpeed, -MaxPitchSpeed, MaxPitchSpeed);
    }
    // Input function ( yaw) 
    void TurnControl(float turn)
    {
        float YawGain = Sensitive * 30 * Time.deltaTime;
        Rudder = Mathf.Clamp(turn * YawGain, -MaxYawSpeed, MaxYawSpeed); //方向舵控制
        //yaw = Mathf.Clamp(turn * YawGain, -MaxYawSpeed, MaxYawSpeed);
    }
    // Speed up
    void SpeedUp(float delta)
    {

        if (delta > 0)
        {
            thrust += Time.deltaTime * 5f;
        }
        else if (delta < 0)
        {
            thrust -= Time.deltaTime * 5f;
        }

        thrust = Mathf.Clamp(throttle, 0, 100);
    }*/

    void getdot()
    {
        vdot = GravityAcc * (normaloverload - Mathf.Sin(3.14f * pitchangle / 180));
        pitchdot = (GravityAcc / (v_value + 1)) * (tangentialoverload * Mathf.Cos(3.14f * rollangle / 180) - Mathf.Cos(3.14f * pitchangle / 180));
        //pitchdot = 1;
        yawdot = 10 * (GravityAcc / ((v_value + 1) * Mathf.Cos(3.14f * pitchangle / 180))) * tangentialoverload * Mathf.Sin(3.14f * rollangle / 180);
        //yawdot = 1;

    }

    void updateangle()
    {
        v_value = v_value + vdot * Time.deltaTime;
        pitchangle = pitchangle + pitchdot * Time.deltaTime;
        yawangle = yawangle + yawdot * Time.deltaTime * 50f;
    }

    void airturn()
    {
        if (rollflag == false)
        {
            rollangle = Mathf.Clamp(rollangle - 5 * Time.deltaTime, -20f, 20f);
        }
        if (rollangle == 20f || rollangle == -20f)
        {
            rollflag = true;
        }

        if (rollflag == true)
        {
            //rollangle = 5f * v_value / (GravityAcc * tangentialoverload) * yawdot;
            rollangle = Mathf.Clamp(rollangle + 5 * Time.deltaTime, -20f, 0f);
            if(rollangle == 0f)
            {
                airturnflag = false;
                tangentialoverload = 0f;
                return;
            }
        }

        tangentialoverload = 10 / Mathf.Cos(3.14f * rollangle / 180);
        normaloverload = 0f;
    }

    void airrise()
    {
        if (pitchflag == false)
        {
            tangentialoverload = Mathf.Cos(3.14f * pitchangle / 180) / Mathf.Cos(3.14f * rollangle / 180) - 200f;
        }
        if (pitchangle < -30)
        {
            pitchflag = true;
        }
        if (pitchflag == true)
        {
            tangentialoverload = Mathf.Cos(3.14f * pitchangle / 180) / Mathf.Cos(3.14f * rollangle / 180) + 200f;
            if(pitchangle >= 0)
            {
                tangentialoverload = 0f;
                airriseflag = false;
                return ;
            }
        }


        normaloverload = 0f;
    }

}
