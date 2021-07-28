using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家控制類別：玩家滑鼠、鍵盤的輸入資訊以及跟 Base Person 溝通
/// </summary>
public class ControllerPlayer : MonoBehaviour
{
    #region 欄位
    /// <summary>
    /// 人物基底類別
    /// </summary>
    private BasePerson basePerson;
    /// <summary>
    /// 要移動的座標資訊
    /// </summary>
    private Vector3 v3Move;
    /// <summary>
    /// 要旋轉的值
    /// </summary>
    private Vector3 v3Turn;
    /// <summary>
    /// 攝影機
    /// </summary>
    private Transform traCamera;
    /// <summary>
    /// 目前子彈數量
    /// </summary>
    private Text textBulletCurrent;
    /// <summary>
    /// 子彈總數
    /// </summary>
    private Text textBulletTotal;
    #endregion

    /// <summary>
    /// 旋轉攝影機：面向目標物件
    /// </summary>
    private void TurnCamera()
    {
        traCamera.LookAt(basePerson.traTarget);
    }

    /// <summary>
    /// 血條
    /// </summary>
    private Image imgHp;
    /// <summary>
    /// 血量
    /// </summary>
    private Text textHp;

    private float hpMax;

    #region 事件
    private void Start()
    {
        Cursor.visible = false;                     // 隱藏滑鼠
        basePerson = GetComponent<BasePerson>();
        traCamera = transform.Find("攝影機");
        textBulletCurrent = GameObject.Find("目前子彈數量").GetComponent<Text>();
        textBulletTotal = GameObject.Find("子彈總數").GetComponent<Text>();
        imgHp = GameObject.Find("血條").GetComponent<Image>();
        textHp = GameObject.Find("血量").GetComponent<Text>();
        hpMax = basePerson.hp;
        UpdateUIBullet();
    }

    private void Update()
    {
        if (basePerson.dead) return;

        GetMoveInput();
        GetTurnInput();
        TurnCamera();
        Fire();
        Reload();
        Jump();

        // 呼叫基底類別 旋轉
        basePerson.Turn(v3Turn.y, v3Turn.x);
    }

    // 固定更新事件：50 FPS 物理行為在此事件內執行
    private void FixedUpdate()
    {
        if (basePerson.dead) return;

        // 呼叫基底類別移動(傳入角色方向)
        basePerson.Move(transform.forward * v3Move.z + transform.right * v3Move.x);
    }
    #endregion

    /// <summary>
    /// 受傷：更新血條與血量介面
    /// </summary>
    public void Hit()
    {
        imgHp.fillAmount = basePerson.hp / hpMax;
        textHp.text = "HP " + basePerson.hp;
    }

    #region 方法
    /// <summary>
    /// 取得移動輸入資訊
    /// </summary>
    private void GetMoveInput()
    {
        float h = Input.GetAxis("Horizontal");  // 水平值 A -1，D 1
        float v = Input.GetAxis("Vertical");    // 垂直值 S -1，W 1
        v3Move.x = h;                           // 左右為 X 軸
        v3Move.z = v;                           // 前後為 Z 軸
    }

    /// <summary>
    /// 取得旋轉輸入資訊
    /// </summary>
    private void GetTurnInput()
    {
        float mouseX = Input.GetAxis("Mouse X");    // 取得滑鼠 X 值
        float mouseY = Input.GetAxis("Mouse Y");    // 取得滑鼠 Y 值
        v3Turn.x = mouseY;                          // 物件 X 軸對應滑鼠 Y
        v3Turn.y = mouseX;                          // 物件 Y 軸對應滑鼠 X
    }

    /// <summary>
    /// 玩家開槍的方式：按下左鍵
    /// </summary>
    private void Fire()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        { 
            basePerson.Fire();
            UpdateUIBullet();
        }
    }

    /// <summary>
    /// 更新子彈介面：目前與總數
    /// </summary>
    private void UpdateUIBullet()
    {
        textBulletCurrent.text = basePerson.bulletCurrent.ToString();
        textBulletTotal.text = basePerson.bulletTotal.ToString();
    }

    /// <summary>
    /// 換彈匣
    /// </summary>
    private void Reload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        { 
            basePerson.ReloadBullet();
            UpdateUIBullet();
        }
    }

    /// <summary>
    /// 按下空白鍵跳躍
    /// </summary>
    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space)) basePerson.Jump();
    }
    #endregion
}