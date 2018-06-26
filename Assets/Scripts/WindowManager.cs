using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowManager : SingletonMonoBehavior<WindowManager>
{
    //子要素のゲームオブジェクトとそのコンポーネントの宣言
    private static Image nextPageImage;
    public static GameObject MessageWindow;
    private static GameObject NextPageIcon;
    private static GameObject MessageText;
    private static Text messageText;

    //メッセージテキストをスタックするリスト
    public static List<string> stackedMessageList = new List<string>();

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
        //子要素のゲームオブジェクトとそのコンポーネントの取得
        MessageWindow = transform.Find("MessageWindow").gameObject;
        NextPageIcon = MessageWindow.transform.Find("NextPageIcon").gameObject;
        MessageText = MessageWindow.transform.Find("MessageText").gameObject;
        messageText = MessageText.GetComponent<Text>();

        //初期メッセージテキストの設定
        messageText.text = "HELLO \n" +
                           "THIS TXET IS \n" +
                           "LOREM IPSUM.";

        //ゲーム開始時はメッセージテキストを非表示にする
        MessageWindow.SetActive(false);
    }

    //windowStateの列挙体と変数の宣言
    public enum WindowState
    {
        field,
        menu
    }
    public static WindowState windowState;


    //遅延処理後にメッセージウィンドウを非表示状態にする連続処理関数関数
    public static IEnumerator SetWindowInactiveWithDelay(float waitTime)
    {
        //引数の時間に従い遅延処理を行う
        yield return new WaitForSeconds(waitTime);

        //メッセージウィンドウを非表示状態にする
        MessageWindow.SetActive(false);
    }

    //メッセージテキストを表示する関数
    public static void DisplayMessageText()
    {
        //メッセージウィンドウを有効化する
        MessageWindow.SetActive(true);

        //メッセージテキストを表示する
        messageText.text = stackedMessageList[0];

        //今表示したメッセージリストの要素を消去する
        stackedMessageList.RemoveAt(0);

        //もしまだメッセージリストの要素が残っている場合、ページ送り画像を有効化し次のメッセージ表示を促す
        if(stackedMessageList.Count > 0)
        {
            NextPageIcon.SetActive(true);
        }
        //そうでない場合、メッセージスタッキング真偽値を偽にし、ページ送り画像を無効化する
        else
        {
            NextPageIcon.SetActive(false);
            GameManager.messageIsStacking = false;
        }
    }

    //引数のメッセージを蓄積メッセージテキストに追加する関数
    private static void StackMessageText(string displayMessageText)
    {
        //引数のメッセージテキストを蓄積メッセージリストに追加する
        stackedMessageList.Add(displayMessageText);

        //メッセージスタッキング真偽値を真にする
        GameManager.messageIsStacking = true;
    }

    //ダメージメッセージを作成する関数
    public static void MakeDamageText(string attackUnitName, string damagedUnitName, int damage)
    {
        //与えられた引数に従ってメッセージを作成する
        string displayMessageText = attackUnitName + "は" + damagedUnitName + "に\n" +
                    damage.ToString() + "のダメージを与えた。";

        //作成したメッセージを蓄積メッセージテキストに追加する
        StackMessageText(displayMessageText);
    }

    //死亡メッセージを作成する関数
    public static void MakeDeathText(string attackUnitName, string damagedUnitName)
    {
        //与えられた引数に従ってメッセージを表示する
        string displayMessageText = damagedUnitName + "をやっつけた\n";

        //作成したメッセージを蓄積メッセージテキストに追加する
        StackMessageText(displayMessageText);
    }
}
