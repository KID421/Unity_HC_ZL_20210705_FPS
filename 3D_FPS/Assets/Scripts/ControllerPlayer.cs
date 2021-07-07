using UnityEngine;

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
    #endregion

    #region 事件
    private void Start()
    {
        basePerson = GetComponent<BasePerson>();
    }

    private void Update()
    {
        GetMoveInput();
        GetTurnInput();
    }

    // 固定更新事件：50 FPS 物理行為在此事件內執行
    private void FixedUpdate()
    {
        basePerson.Move(v3Move);
    }
    #endregion

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
    #endregion
}
