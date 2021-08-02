using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 遊戲管理器
/// 管理遊戲流程：勝利與失敗顯示結束畫面
/// </summary>
public class GameManager : MonoBehaviour
{
    #region 欄位
    [Header("結束畫面：群組")]
    public CanvasGroup groupFinal;

    /// <summary>
    /// 此類別的實體物件
    /// 靜態欄位：
    /// 1. 可以使用靜態 API 用法存取，語法：類別.靜態屬性或方法
    /// 2. 靜態成員不會顯示在屬性面板上
    /// 3. 重新載入後不會恢復為預設值
    /// </summary>
    public static GameManager instance;
    /// <summary>
    /// 是否遊戲結束
    /// </summary>
    public static bool isGameOver;

    /// <summary>
    /// AI 總數
    /// </summary>
    private int countAI;
    /// <summary>
    /// 死亡的 AI 數量
    /// </summary>
    private int deadAI;
    /// <summary>
    /// 結束畫面的標題
    /// </summary>
    private Text textTitleFinal;
    #endregion

    #region 事件
    private void Start()
    {
        // 實體物件 指定為 此類別 (this 為此類別)
        instance = this;
        // 靜態欄位不會恢復預設值，必須自行指定
        isGameOver = false;
        // 取得場景內所有貼 敵人 標籤 的物件數量
        countAI = GameObject.FindGameObjectsWithTag("敵人").Length;
        textTitleFinal = GameObject.Find("標題").GetComponent<Text>();
    }

    private void Update()
    {
        ReplayGame();
        QuitGame();
    }
    #endregion

    #region 方法
    /// <summary>
    /// 按 R 重新遊戲
    /// </summary>
    private void ReplayGame()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.Q)) SceneManager.LoadScene("遊戲場景");
    }

    /// <summary>
    /// 按 ESC 離開遊戲
    /// </summary>
    private void QuitGame()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    /// <summary>
    /// 有人死亡，判定此類型死亡後要做的處理
    /// </summary>
    /// <param name="type">死亡的類型</param>
    public void SomeBodyDead(PeopleType type)
    {
        // 判斷類型決定如何結束：玩家死亡結束，敵人必須全死才結束
        switch (type)
        {
            case PeopleType.player:
                StartCoroutine(ShowFinal("You Lose"));
                break;
            case PeopleType.ai:
                deadAI++;
                if (deadAI == countAI) StartCoroutine(ShowFinal("You Win!"));
                break;
        }
    }

    /// <summary>
    /// 顯示結束畫面：淡入
    /// </summary>
    private IEnumerator ShowFinal(string title)
    {
        isGameOver = true;
        textTitleFinal.text = title;

        for (int i = 0; i < 40; i++)
        {
            groupFinal.alpha += (1f / 40f);
            yield return new WaitForSeconds(0.02f);
        }
    }
    #endregion
}

/// <summary>
/// 人的類型：玩家或電腦
/// </summary>
public enum PeopleType
{
    player, ai
}
