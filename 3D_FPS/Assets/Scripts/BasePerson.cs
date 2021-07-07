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
    }
    #endregion

    #region 方法
    /// <summary>
    /// 移動
    /// </summary>
    /// <param name="movePosition">要移動的座標資訊</param>
    public void Move(Vector3 movePosition)
    {
        // 剛體.移動座標(物件座標 + 移動座標 * 速度)
        rig.MovePosition(transform.position + movePosition * speed);
    }

    [Header("旋轉速度"), Range(0, 1000)]
    public float turn = 5;

    /// <summary>
    /// 旋轉
    /// </summary>
    /// <param name="turnValue">要旋轉的值</param>
    public void Turn(Vector3 turnValue)
    {
        // 變形.旋轉(三維向量 * 旋轉速度)
        transform.Rotate(turnValue * turn);
    }
    #endregion
}
