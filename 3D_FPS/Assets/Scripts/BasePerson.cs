using UnityEngine;

public class BasePerson : MonoBehaviour
{
    #region 欄位
    [Header("移動速度"), Range(0, 1000)]
    public float speed = 10;
    [Header("跳躍高度"), Range(0, 1000)]
    public float jump = 100;
    [Header("血量"), Range(0, 1000)]
    public float hp = 100;
    [Header("攻擊力"), Range(0, 100)]
    public float attack = 10;
    [Header("旋轉速度"), Range(0, 1000)]
    public float turn = 5;
    [Header("上下旋轉靈敏度"), Range(0, 100)]
    public float mouseUpDown = 1.5f;
    [Header("目標物件上下範圍限制")]
    public Vector2 v2TargetLimit = new Vector2(0, 3);

    /// <summary>
    /// 目標物件
    /// </summary>
    [HideInInspector]
    public Transform traTarget;

    /// <summary>
    /// 血量最大值
    /// </summary>
    private float hpMax;
    private Animator ani;
    private Rigidbody rig;
    private AudioSource aud;
    #endregion

    #region 事件
    private void Start()
    {
        #region 取得元件
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();
        #endregion

        traTarget = transform.Find("目標物件");
    }

    private void Update()
    {
        AnimatorMove();
    }
    #endregion

    #region 方法
    /// <summary>
    /// 移動，必須在 FixedUpdate 呼叫
    /// </summary>
    /// <param name="movePosition">要移動的座標資訊</param>
    public void Move(Vector3 movePosition)
    {
        // 剛體.移動座標(物件座標 + 移動座標 * 速度)
        rig.MovePosition(transform.position + movePosition * speed);
    }

    /// <summary>
    /// 旋轉
    /// </summary>
    /// <param name="turnValueY">要旋轉的值</param>
    /// <param name="moveTarget">要位移目標物件的值</param>
    public void Turn(float turnValueY, float moveTarget)
    {
        // 變形.旋轉(上方 * 旋轉值 * 旋轉速度)
        transform.Rotate(transform.up * turnValueY * turn * Time.deltaTime);
        // 目標物件.位移(x, y, z)
        traTarget.Translate(0, moveTarget * mouseUpDown * Time.deltaTime, 0);
        // 取得目標物件區域座標 並限制在範圍內 最後更新座標
        Vector3 posTarget = traTarget.localPosition;
        posTarget.y = Mathf.Clamp(posTarget.y, v2TargetLimit.x, v2TargetLimit.y);
        traTarget.localPosition = posTarget;
    }

    Vector3 posRig;

    /// <summary>
    /// 動畫 - 移動
    /// </summary>
    private void AnimatorMove()
    {
        bool move = rig.position != posRig;
        ani.SetBool("走路開關", move);
        posRig = rig.position;
    }
    #endregion
}
