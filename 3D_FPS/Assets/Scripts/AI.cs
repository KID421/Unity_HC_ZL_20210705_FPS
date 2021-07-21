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
                break;
            case StateAI.Fire:
                break;
        }
    }

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
    /// 隨機走動：在角色半徑以內選取隨機座標並移動
    /// </summary>
    private void RandomWalk()
    {
        if (!randomWalking)
        {
            print("隨機走動中...");
            // 隨機座標 = 隨機球體座標 * 半徑 + 角色中心點 - 以角色為中心取得半徑內的隨機座標
            posRandom = Random.insideUnitSphere * radiusRandomWalk + transform.position;

            // 網格.取得樣本座標(隨機座標，在網格內的隨機座標，半徑，區域)
            NavMesh.SamplePosition(posRandom, out hitRandomWalk, radiusRandomWalk, NavMesh.AllAreas);
            posRandom = hitRandomWalk.position;

            randomWalking = true;
        }
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
    #endregion

    #region 事件
    private void Start()
    {
        basePerson = GetComponent<BasePerson>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        CheckState();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, radiusRandomWalk);

        Gizmos.color = new Color(0, 0.8f, 1, 0.8f);
        Gizmos.DrawSphere(posRandom, 0.5f);
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