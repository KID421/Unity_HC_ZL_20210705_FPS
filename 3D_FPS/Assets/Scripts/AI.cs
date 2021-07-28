using System.Threading;
using UnityEngine;
using UnityEngine.AI;   // 引用 AI API

/// <summary>
/// 敵人 AI ：決定如何移動、追蹤玩家、開槍跳躍受傷死亡
/// </summary>
public class AI : MonoBehaviour
{
    #region 欄位
    /// <summary>
    /// AI 狀態
    /// </summary>
    public StateAI state;
    [Header("等待幾秒後進入隨機行走")]
    public Vector2 v2IdleToRandomWalk = new Vector2(2, 6);
    [Header("隨機走動半徑"), Range(0, 100)]
    public float radiusRandomWalk = 20;
    [Header("旋轉面向物件的速度"), Range(0, 100)]
    public float turn = 3;
    [Header("移動到隨機座標的停止距離")]
    public float distanceStop = 1.5f;
    [Header("隨機走動後隨機等待的機率")]
    public float idleProbility = 0.3f;
    [Header("檢查是否看到玩家的資訊：方體前方位移以及方體的尺寸")]
    public float checkCubeOffsetForward = 5;
    public Vector3 checkCubeSize = new Vector3(1, 1, 10);
    [Header("與玩家夾角為幾度就開槍")]
    public float angleFire = 2;

    /// <summary>
    /// 基底類別
    /// </summary>
    private BasePerson basePerson;
    /// <summary>
    /// 導覽網格 代理器
    /// </summary>
    private NavMeshAgent agent;
    /// <summary>
    /// 是否從待機等待前往隨機走動 - 預設為沒有
    /// </summary>
    private bool isWaitToRandomWalk;
    /// <summary>
    /// 隨機走動使用的隨機座標
    /// </summary>
    private Vector3 posRandom;
    /// <summary>
    /// 是否在隨機走動中
    /// </summary>
    private bool randomWalking;
    /// <summary>
    /// 導覽網格碰撞 - 在網格內的儲存隨機座標
    /// </summary>
    private NavMeshHit hitRandomWalk;
    /// <summary>
    /// 介於隨機座標與角色座標之間的位置 - 要移動到的座標
    /// </summary>
    private Vector3 posMove;
    private Transform player;
    #endregion

    #region 方法
    /// <summary>
    /// 檢查狀態
    /// </summary>
    private void CheckState()
    {
        switch (state)
        {
            case StateAI.Idle:
                Idle();
                break;
            case StateAI.RandomWalk:
                RandomWalk();
                break;
            case StateAI.TrackTarget:
                TrackTarget();
                break;
            case StateAI.Fire:
                Fire();
                break;
        }
    }

    [Header("開槍準心校正間隔")]
    public float intervalFire = 0.2f;
    [Header("每次開槍後的偏差值")]
    public float offsetFire = 0.05f;
    [Header("偏差值限制")]
    public Vector2 v2FireLimit = new Vector2(0.8f, 1.2f);

    private float timerFire;

    /// <summary>
    /// 開槍
    /// </summary>
    private void Fire()
    {
        LookTargetSmooth(player.position);

        if (basePerson.bulletCurrent == 0)                  // 如果 子彈 數量 為 零
        {
            basePerson.ReloadBullet();                      // 換彈匣
        }
        else
        {
            basePerson.Fire();                              // 否則 就開槍

            if (timerFire <= intervalFire)
            {
                timerFire += Time.deltaTime;
            }
            else
            {
                // 目標物件的偏差，每次開槍後上下晃動
                Vector3 posTargetPoint = basePerson.traTarget.localPosition;
                posTargetPoint.y += (float)(Random.Range(-1, 2) * offsetFire);
                posTargetPoint.y = Mathf.Clamp(posTargetPoint.y, v2FireLimit.x, v2FireLimit.y);
                basePerson.traTarget.localPosition = posTargetPoint;
                timerFire = 0;
            }
        }
    }

    /// <summary>
    /// 追蹤目標：面向玩家
    /// </summary>
    private void TrackTarget()
    {
        randomWalking = false;
        Quaternion angleLook = LookTargetSmooth(player.position);
        float angle = Quaternion.Angle(transform.rotation, angleLook);
        if (angle <= angleFire) state = StateAI.Fire;
    }

    /// <summary>
    /// 隨機走動：在角色半徑以內選取隨機座標並移動
    /// </summary>
    private void RandomWalk()
    {
        // 如果 還沒有 進行隨機走路 就 取得隨機座標
        if (!randomWalking)
        {
            // 隨機座標 = 隨機球體座標 * 半徑 + 角色中心點 - 以角色為中心取得半徑內的隨機座標
            posRandom = Random.insideUnitSphere * radiusRandomWalk + transform.position;

            // 網格.取得樣本座標(隨機座標，在網格內的隨機座標，半徑，區域)
            NavMesh.SamplePosition(posRandom, out hitRandomWalk, radiusRandomWalk, NavMesh.AllAreas);
            posRandom = hitRandomWalk.position;

            randomWalking = true;
        }
        // 否則 正在隨機移動中 取得前方座標 並且往前移動，往前移動在 Fixed Update 內處理
        else if (randomWalking)
        {

            // 如果 與隨機座標 的距離 > 停止距離 就繼續移動
            if (Vector3.Distance(transform.position, posRandom) > distanceStop)
            {
                // 當前座標與隨機座標之間的位置 - 取得前方的位置
                posMove = Vector3.MoveTowards(transform.position, posRandom, 1.5f);

                LookTargetSmooth(posMove);
            }
            // 否則 就決定處理下一個狀態 - 隨機 等待 或 隨機走路
            else
            {
                float r = Random.Range(0f, 1f);
                if (r < idleProbility)
                {
                    state = StateAI.Idle;
                }
                else
                {
                    state = StateAI.RandomWalk;
                }

                randomWalking = false;
            }
        }
    }

    /// <summary>
    /// 平滑的面向目標物件
    /// </summary>
    /// <param name="posTarget">目標物件的座標</param>
    private Quaternion LookTargetSmooth(Vector3 posTarget)
    {
        // 計算目標與此物件的面相角度
        Quaternion quaLook = Quaternion.LookRotation(posTarget - transform.position);
        // 角度的插值
        transform.rotation = Quaternion.Lerp(transform.rotation, quaLook, turn * Time.deltaTime);
        // 傳回敵人與玩家的角度
        return quaLook;
    }

    /// <summary>
    /// 待機：隨機秒數後開始隨機走動
    /// </summary>
    private void Idle()
    {
        if (!isWaitToRandomWalk)                                                            // 如果 不是在等待前往隨機走動 中
        {
            float random = Random.Range(v2IdleToRandomWalk.x, v2IdleToRandomWalk.y);        // 取得隨機秒數
            isWaitToRandomWalk = true;                                                      // 已經在等待前往隨機走動 中
            CancelInvoke("IdleWaitToRandomWalk");
            Invoke("IdleWaitToRandomWalk", random);
        }
    }

    /// <summary>
    /// 待機等待前往隨機走動
    /// </summary>
    private void IdleWaitToRandomWalk()
    {
        state = StateAI.RandomWalk;                                                         // 切換到隨機走動狀態
        isWaitToRandomWalk = false;
    }

    /// <summary>
    /// 檢查玩家是否進入到檢查立方體內
    /// </summary>
    /// <returns>是否看到玩家</returns>
    private bool CheckPlayerInCube()
    {
        Collider[] hit = Physics.OverlapBox(transform.position + transform.forward * checkCubeOffsetForward, checkCubeSize / 2, Quaternion.identity, 1 << 9);

        bool playerInCube;

        if (hit.Length > 0) playerInCube = true;
        else playerInCube = false;

        return playerInCube;
    }
    #endregion

    #region 事件
    private void Start()
    {
        basePerson = GetComponent<BasePerson>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("玩家").transform;
    }

    private void Update()
    {
        if (basePerson.dead) return;

        CheckState();
        // 如果 玩家進入到檢查立方體內 並且 不是在開槍狀態 就進入 追蹤狀態
        if (CheckPlayerInCube() && state != StateAI.Fire ) state = StateAI.TrackTarget;
    }

    private void FixedUpdate()
    {
        if (basePerson.dead) return;

        if (randomWalking)
        {
            basePerson.Move(transform.forward);
        }
    }

    private void OnDrawGizmos()
    {
        #region 角色隨機走動的圖示
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, radiusRandomWalk);

        Gizmos.color = new Color(0, 0.8f, 1, 0.8f);
        Gizmos.DrawSphere(posRandom, 0.5f);

        Gizmos.color = new Color(1, 0, 0, 0.8f);
        Gizmos.DrawSphere(posMove, 0.6f);
        #endregion

        Gizmos.color = new Color(0.8f, 0.1f, 0.1f, 0.3f);
        // 矩陣 = 矩陣.座標角度尺寸(座標，角度，尺寸)
        Gizmos.matrix = Matrix4x4.TRS(transform.position + transform.forward * checkCubeOffsetForward, transform.rotation, transform.localScale);
        Gizmos.DrawCube(Vector3.zero, checkCubeSize);
    }
    #endregion
}

/// <summary>
/// AI 狀態：待機、隨機走動、追蹤目標物、開槍
/// </summary>
public enum StateAI
{
    Idle, RandomWalk, TrackTarget, Fire
}