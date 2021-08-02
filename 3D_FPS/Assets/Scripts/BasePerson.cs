using UnityEngine;
using UnityEngine.Events;               // 引用 事件 API 寫出與 OnClick 一樣的功能
using UnityEngine.Animations.Rigging;
using System.Collections;

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
    [Header("發射子彈位置")]
    public Transform traFirePoint;
    [Header("子彈預製物")]
    public GameObject objBullet;
    [Header("子彈發射速度"), Range(0, 3000)]
    public float speedBullet = 600;
    [Header("子彈發射間隔"), Range(0, 1)]
    public float intervalFire = 0.5f;
    [Header("音效")]
    public AudioClip soundFire;
    public AudioClip soundFireEmpty;
    [Header("檢查地板")]
    public float groundRadius = 0.5f;
    public Vector3 groundOffset;
    [Header("跳躍後恢復權重的時間")]
    public float timeRestoreWeight = 1.3f;
    [Header("傷害音效：一般與爆頭")]
    public AudioClip soundHit;
    public AudioClip soundHeadShot;

    // HideInInspector 可以讓公開欄位不要顯示在面板
    /// <summary>
    /// 目標物件
    /// </summary>
    [HideInInspector]
    public Transform traTarget;
    [HideInInspector]
    /// <summary>
    /// 是否死亡：記錄此角色是否死亡
    /// </summary>
    public bool dead;

    /// <summary>
    /// 血量最大值
    /// </summary>
    private float hpMax;
    private Animator ani;
    private Rigidbody rig;
    private AudioSource aud;

    [HideInInspector]       // 將公開的欄位隱藏
    /// <summary>
    /// 子彈目前數量
    /// </summary>
    public int bulletCurrent = 30;
    /// <summary>
    /// 彈匣數量
    /// </summary>
    private int bulletClip = 30;
    [HideInInspector]
    /// <summary>
    /// 子彈總數
    /// </summary>
    public int bulletTotal = 120;
    /// <summary>
    /// 開槍用計時器
    /// </summary>
    private float timerFire;
    /// <summary>
    /// 動畫設置物件
    /// </summary>
    private Rig rigging;
    /// <summary>
    /// 是否在地板上
    /// </summary>
    private bool isGround;
    #endregion

    // 定義事件：不會執行
    [Header("受傷事件")]
    public UnityEvent onHit;
    [Header("人物類型")]
    public PeopleType type;

    #region 事件
    private void Start()
    {
        #region 取得元件
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();
        #endregion

        traTarget = transform.Find("目標物件");
        rigging = transform.Find("設置物件").GetComponent<Rig>();
    }

    private void Update()
    {
        // AnimatorMove();
        CheckGround();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position + groundOffset, groundRadius);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Contains 屬於 string API 必須是字串才能使用
        // 如果 碰到物件名稱 包含 子彈 就受傷
        if (collision.gameObject.name.Contains("子彈"))
        {
            // 如果 自身被碰到的 碰撞器類型 是 球體 就爆頭
            if (collision.contacts[0].thisCollider.GetType() == typeof(SphereCollider)) Hit(100, soundHeadShot);
            // 否則 就受到子彈傷害
            else Hit(collision.gameObject.GetComponent<Bullet>().attack, soundHit);
        }
    }
    #endregion

    #region 方法
    /// <summary>
    /// 受傷
    /// </summary>
    /// <param name="damage">接收傷害值</param>
    private void Hit(float damage, AudioClip sound)
    {
        hp -= damage;
        aud.PlayOneShot(sound, Random.Range(0.8f, 1.2f));

        if (hp <= 0) Dead();

        onHit.Invoke();         // 呼叫事件
    }

    /// <summary>
    ///死亡：動畫、權重恢復、禁止其他行為
    /// </summary>
    private void Dead()
    {
        hp = 0;
        ani.SetBool("死亡開關", true);
        rigging.weight = 0;
        dead = true;
        // 關閉碰撞避免重複死亡判定與子彈碰撞
        GetComponent<SphereCollider>().enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        // 剛體加速度歸零並約束所有
        rig.velocity = Vector3.zero;
        rig.constraints = RigidbodyConstraints.FreezeAll;

        // 呼叫遊戲物件.實體物件.有人死亡(類型)
        GameManager.instance.SomeBodyDead(type);

        enabled = false;
    }

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

    /// <summary>
    /// 開槍方法
    /// </summary>
    public void Fire()
    {
        if (ani.GetBool("換彈匣開關")) return;

        if (timerFire < intervalFire) timerFire += Time.deltaTime;
        else
        {
            if (bulletCurrent > 0)
            {
                bulletCurrent--;
                timerFire = 0;
                aud.PlayOneShot(soundFire, Random.Range(0.5f, 1.2f));
                GameObject tempBullet = Instantiate(objBullet, traFirePoint.position, Quaternion.identity);

                tempBullet.AddComponent<Bullet>().attack = attack;                                          // 添加子彈腳本並賦予攻擊力
                Physics.IgnoreCollision(GetComponent<Collider>(), tempBullet.GetComponent<Collider>());     // 忽略子彈與開槍者的碰撞

                tempBullet.GetComponent<Rigidbody>().AddForce(-traFirePoint.forward * speedBullet);
            }
            else
            {
                aud.PlayOneShot(soundFireEmpty, Random.Range(0.5f, 1.2f));
                timerFire = 0;
            }
        }
    }

    /// <summary>
    /// 換彈匣
    /// </summary>
    public void ReloadBullet()
    {
        // 如果 目前子彈 等於 彈匣 或者 總數 為零 就跳出 - 不需要補子彈
        if (bulletCurrent == bulletClip || bulletTotal == 0) return;

        StartCoroutine(Reloading());

        int bulletGetCount = bulletClip - bulletCurrent;        // 計算取出數量 = 彈匣 - 目前

        if (bulletTotal >= bulletGetCount)                      // 如果 總數 大於等於 要取出的數量
        {
            bulletTotal -= bulletGetCount;                      // 總數 - 取出數量
            bulletCurrent += bulletGetCount;                    // 目前 + 取出數量
        }
        else                                                    // 總數 不夠時 直接將總數給目前子彈
        {
            bulletCurrent += bulletTotal;                       // 將剩餘子彈給目前子彈
            bulletTotal = 0;                                    // 沒有總數子彈
        }
    }

    /// <summary>
    /// 跳躍功能：利用剛體讓角色往上跳
    /// </summary>
    public void Jump()
    {
        if (isGround)                                       // 如果 在地板上 才能跳躍
        {
            rigging.weight = 0;                             // 權重歸零
            rig.AddForce(0, jump, 0);                       // 推力
            CancelInvoke("RestoreWeight");                  // 先取消延遲呼叫 恢復權重
            Invoke("RestoreWeight", timeRestoreWeight);     // 再延遲呼叫 恢復權重
        }
    }

    /// <summary>
    /// 恢復權重為 1
    /// </summary>
    private void RestoreWeight()
    {
        rigging.weight = 1;
    }

    /// <summary>
    /// 檢查地板，並且控制跳躍動畫 - 在地面上不跳躍，不在地面就跳躍
    /// </summary>
    private void CheckGround()
    {
        Collider[] hit = Physics.OverlapSphere(transform.position + groundOffset, groundRadius, 1 << 8);
        // 如果 碰撞陣列數量 > 0 並且 碰撞物件名稱為地板 就代表在地上 否則 就不再地上
        isGround = hit.Length > 0 && hit[0].name == "地板" ? true : false;
        ani.SetBool("跳躍開關", !isGround);
    }

    /// <summary>
    /// 換彈匣狀態 - 動畫以及等待動畫完畢
    /// </summary>
    private IEnumerator Reloading()
    {
        ani.SetBool("換彈匣開關", true);
        rigging.weight = 0.5f;

        // ani.GetCurrentAnimatorStateInfo(0).length 取得圖層 0 目前動畫的長度
        yield return new WaitForSeconds(ani.GetCurrentAnimatorStateInfo(0).length * 0.9f);

        ani.SetBool("換彈匣開關", false);
        rigging.weight = 1;
    }

    /// <summary>
    /// 動畫 - 移動
    /// </summary>
    private void AnimatorMove()
    {
        ani.SetBool("走路開關", rig.velocity.x != 0 || rig.velocity.z != 0);
    }
    #endregion
}
