using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowManager : SingletonMonoBehaviour<WindowManager>
{
    //変数の宣言
    private Text messageText;
    private Image nextPageImage;
    public GameObject Panel;
    private GameObject NextPageIcon;
    private GameObject TextMessage;
    private Text textMessage;

    //WindowManagerを生成する準備
    public static WindowManager instance = null;

    //Awake
    void Awake()
    {
        //boardManagerをシングルトンで生成する
        if(this != Instance)
        {
            Destroy(this);
            return;
        }
    }

    //Start
    void Start()
    {
        //各ゲームオブジェクトの取得
        Panel = transform.Find("Panel").gameObject;
        NextPageIcon = Panel.transform.Find("NextPageIcon").gameObject;
        TextMessage = Panel.transform.Find("TextMessage").gameObject;
        textMessage = TextMessage.GetComponent<Text>();

        //メッセージテキストを取得し、初期値を代入
        textMessage.text = "WARNING: \n" +
                            "THIS TXET IS \n" +
                            "LOREM IPSUM.";
        //メッセージテキストを最初は見えないようにする
        Panel.SetActive(false);
    }

    //windowStateの設定
    public enum WindowState
    {
        field,
        menu
    }
    public static WindowState windowState;


    //ウィンドウを表示状態にする関数
    public void SetWindowActive(bool b)
    {
        Panel.SetActive(b);
    }

    //ダメージメッセージを表示する関数
    public void DisplayDamageMessage(string attackerName, string attackedName, int damage, bool hasNextPage)
    {
        //まずはパネルを表示するようにする
        Panel.SetActive(true);

        //与えられた引数に従ってメッセージを表示する
        textMessage.text = attackerName + "は" + attackedName + "に\n" +
                           damage.ToString() +  "のダメージを与えた。";

        //もし次のメッセージ文が存在する場合はNextPageIconを表示するようにする
        NextPageIcon.SetActive(hasNextPage);

    }

    //志望メッセージを表示する関数
    public void DisplayDeathMessage()
    {
        //まずはパネルを表示するようにする
        Panel.SetActive(true);

        //与えられた引数に従ってメッセージを表示する
        textMessage.text = "プレイヤーは敵を\n" +
                            "倒した。";
    }
}
