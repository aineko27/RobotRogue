using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    //フレームカウントを宣言、各フレームごとにインクリメントする
    [Watch] public static int frameCount = 0;
    [Watch] public static int turnCount = 0;

    //キー入力が保存されているファイルの宣言
    private static StreamReader streamReader;

    //私用するプレハブの宣言(Inspectorで取得する)
    public GameObject EnemyPrefab;



    //ランダムシードを宣言する
    private static int randomSeed;

    //が移動中であるユニットを数える整数値の宣言
    public static int movingUnitsCount = 0;

    //アニメーションが作動しているかを示す真偽値の宣言
    public static bool animationIsRunning = false;
    public static Animator runningUnitAnimator = new Animator();
    public static string runningAnimationName = "";

    //メッセージテキストがスタックしているかを示す真偽値の宣言
    public static bool messageIsStacking = false;

    //コマンドウィンドウが表示されているかを示す真偽値の宣言
    public static bool commandWindowIsDisplaying = false;

    //現在表示されているコマンドウィンドウの列挙体と変数の宣言
    public enum CommandWindowState
    {
        Window1
    }
    public static CommandWindowState commandWindowState;

    //キー入力が制限されているかを示す真偽値の宣言
    public static int keyInputRestrictionCount = 0;

    //gameStateの列挙体と変数の宣言
    public enum GameState
    {
        TitleWindow,
        InDungeon
    }
    private static GameState gameState;

    //turnStateの列挙体と変数の宣言
    public enum TurnState
    {
        DecidePlayerBehavior,
        DecideEnemyBehavior,
        StartUnitMovement,
        StartPlayerAttack,
        AfterPlayerAttack,
        WaitPlayerAttackComplete,
        WaitMovementComplete,
    }
    [Watch] public static TurnState turnState;

    //playStateの列挙体と変数の宣言
    public enum PlayState
    {
        Neutral,
        Save,
        Replay
    }
    public PlayState playState;

    //GameManagerをシングルトンで生成する準備
    public static GameManager instance = null;

    //Awake
    void Awake()
    {
        //GameManagerをシングルトンで生成する
        if (this != Instance){
            Destroy(this);
            return;
        }

        //シーン遷移時にこのObjectを受け継ぐ
        DontDestroyOnLoad(gameObject);
    }
    
    //Start
    void Start()
    {
        //ファイル読み込み、書き込みの設定
        //もしプレイステートがSaveの場合
        if (playState == PlayState.Save)
        {
            //まずは既存のInputtedKeyData.txtファイルを消去する
            System.IO.File.Delete("./InputtedKeyData.txt");

            //ランダムシードを設定し、Random.InitState関数に代入し実行する
            randomSeed = UnityEngine.Random.Range(0, 10000);
            //randomSeed = 42;
            UnityEngine.Random.InitState(randomSeed);

            //InputtedKeyData.txtを開き、設定したランダムシードを１行目に書き込み、ファイルを閉じる
            StreamWriter streamWriter = new StreamWriter("./InputtedKeyData.txt", true);
            streamWriter.WriteLine(randomSeed.ToString());
            streamWriter.Flush();
            streamWriter.Close();

        }
        //もしプレイステートがReplayの場合
        else if (playState == PlayState.Replay)
        {
            //InputtedKeyData.txtを開き、ファイルからランダムシードを読み込み、InitState関数に代入し実行する
            streamReader = new StreamReader("./InputtedKeyData.txt");
            randomSeed = int.Parse(streamReader.ReadLine());
            UnityEngine.Random.InitState(randomSeed);
        }

        //ゲーム開始
        InitGame();
    }

    //ゲーム開始起動時に処理する関数
    private void InitGame()
    {
        //turnStateをタイトルウィンドウにする
        gameState = GameState.TitleWindow;

        //ダンジョンをセットアップする
        SetupDungeon();
    }

    //ダンジョンをセットアップする関数
    public void SetupDungeon()
    {
        //各種ステートの更新
        gameState = GameState.InDungeon;
        turnState = TurnState.DecidePlayerBehavior;

        //ボードのセットアップ
        BoardManager.Instance.SetupBoard();

        //自機ユニットのステータスを設定し、自機ユニットををユニットリストの0番目に加える
        //PlayerManager.SetUnitStatus();
        Unit.unitList.Add(PlayerManager.instance.GetComponent<PlayerManager>());
        
        //敵の配置
        GameObject instance = Instantiate(EnemyPrefab);
        instance.GetComponent<EnemyManager>().SetUnitStatus(EnemyManager.UnitName.EliteSkelton);
    }

    //ゲームオーバー時の処理
    public static void GameOver()
    {

    }

    void Update()
    {
        //frameCountのインクリメント
        frameCount++;
        //SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS【開始】デバッグ関連のシステム処理【開始】SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS
                                                                                                                                                                                                //SSSSS
        //ssssssssssssssssssssssssssssssssssssssssssssssss【始】デバッグ用の処理sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss
        //プレイステートがSaveの場合、押されているキーコードをInputtedKeyData.txtファイルに書き込む
        if (playState == PlayState.Save)
        {
            InputFunction.WriteInputtedKey();
        }
        //プレイステートがReadの場合、InputtedKeyData.txtファイルから保存されたキーコードを読み込む
        else if (playState == PlayState.Replay)
        {
            InputFunction.ReadInputtedKey(streamReader);
        }
        //eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee【終】デバッグ用の処理eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee
        //EEEEE
        //EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE【終了】デバッグ関連のシステム処理【終了】EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE



        //SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS【開始】ターン前の演出・操作関連の処理【開始】SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS
        //SSSSS
        //ssssssssssssssssssssssssssssssssssssssssssssssss【始】ユニットが移動中の処理sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss
        //ユニット移動中真偽値が真である場合
        if (movingUnitsCount > 0)
        {
            return;
        }
        //eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee【終】ユニットが移動中の処理eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee


        //ssssssssssssssssssssssssssssssssssssssssssssssss【始】アニメーションが作動している場合sssssssssssssssssssssssssssssssssssssssssssssssssssss
        //アニメーション再生中真偽値が真である場合
        if (animationIsRunning == true)
        {
            //今フレームもアニメーションが再生中であるばあい、ターン処理をここで打ち切る
            if (runningUnitAnimator.GetCurrentAnimatorStateInfo(0).IsName(runningAnimationName) == true)
            {
                return;
            }
            //アニメーションが再生終了した場合、アニメーション再生中真偽値を偽にし、アニメーション再生情報変数を初期化する
            else
            {
                animationIsRunning = false;
                runningUnitAnimator = new Animator();
                runningAnimationName = "";

            }
        }
        //eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee【終】アニメーションが作動している場合eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee


        //ssssssssssssssssssssssssssssssssssssssssssssssss【始】テキストウィンドウが表示されている場合sssssssssssssssssssssssssssssssssssssssssssssss
        //メッセージ蓄積真偽値がもし真である場合、メッセージを表示する
        if (messageIsStacking == true)
        {
            //まだメッセージウィンドウが表示されていない場合(又は)zキーが押された場合、メッセージテキストを更新し表示する
            if (MessageWindowController.MessageWindow.activeSelf == false || InputFunction.GetKeyDown("Z") == true)

            {
                //メッセージテキストを更新し表示
                MessageWindowController.DisplayMessageText();

                //もしこれ以上メッセージの蓄積がない場合、キー入力制限真偽値を真にし、遅延処理後にメッセージウィンドウを非表示にする
                if (messageIsStacking == false)
                {
                    keyInputRestrictionCount = 24;
                    StartCoroutine(MessageWindowController.SetWindowInactiveWithDelay(1.5f));
                }
                return;
            }
            //zキーが押されない場合、ターン処理をここで打ち切る
            else
            {
                return;
            }
        }
        //eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee【終】テキストウィンドウが表示されている場合eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee
        

        //ssssssssssssssssssssssssssssssssssssssssssssssss【始】キー入力制限がかかっている場合sssssssssssssssssssssssssssssssssssssssssssssssssssssss
        //キー入力制限カウントが0より大きかった場合、カウントをディクリメントし、ターン処理をここで打ち切る
        if (keyInputRestrictionCount > 0)
        {
            keyInputRestrictionCount--;
            return;
        }
        //eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee【終】キー入力制限がかかっている場合eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee
        
                                                                                                                                                                                                //EEEEE
        //EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE【終了】ターン前の演出・操作関連の処理【終了】EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE



        //SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS【開始】ターン処理【開始】SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS
                                                                                                                                                                                                //SSSSS

        //ssssssssssssssssssssssssssssssssssssssssssssssss【始】自機ユニットの振る舞い予定を決定するsssssssssssssssssssssssssssssssssssssssssssssssss

        //現在のゲームステートによってキー受付を変える
        //ゲームステートが自機ユニットの振る舞い前だったら
        if (turnState == TurnState.DecidePlayerBehavior)
        {
            //自機ユニットの振る舞い予定を決める
            Unit.unitList[0].DecideBehavior();

            //もし自機ユニットが行動を決定した場合、turnCountをインクリメントする
            if (Unit.unitList[0].scheduledBehavior != PlayerManager.ScheduledBehavior.NotDecided)
            {
                //turnCountのインクリメント
                turnCount++;
            }
            
            //もし自機ユニットの振る舞い予定が移動だった場合、turnStateをDecideEnemyBehaveに遷移させる
            if (Unit.unitList[0].scheduledBehavior == PlayerManager.ScheduledBehavior.Move)
            {
                turnState = TurnState.DecideEnemyBehavior;
            }
            //もし自機ユニットの振る舞い予定が攻撃だった場合、turnStateをDecideEnemyBehave(に遷移させる
            else if (Unit.unitList[0].scheduledBehavior == PlayerManager.ScheduledBehavior.Attack)
            {
                turnState = TurnState.StartPlayerAttack;
            }
        }
        //eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee【終】自機ユニットの振る舞い予定を決定するeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee


        //ssssssssssssssssssssssssssssssssssssssssssssssss【始】自機ユニットの振る舞い予定に従いターン処理を場合分けするsssssssssssssssssssssssssssss
        //もし自機ユニットが振る舞い予定を決定していた場合
        if (Unit.unitList[0].scheduledBehavior != PlayerManager.ScheduledBehavior.NotDecided)
        {
            //================================CASE.1:自機ユニットの振る舞い予定が移動だった場合========================
            if (Unit.unitList[0].scheduledBehavior == PlayerManager.ScheduledBehavior.Move)
            {
                // 01.//全敵ユニットの振る舞い予定を決め、turnSateを遷移させる
                if (turnState == TurnState.DecideEnemyBehavior)
                {
                    for(int i = 1; i < Unit.unitList.Count; i++)
                    {
                        Unit.unitList[i].DecideBehavior();
                    }
                    turnState = TurnState.StartUnitMovement;
                }
                // 02.ユニット移動中真偽値を真にし、振る舞い予定が移動である全ユニットについて移動を始め、turnStateを遷移させる
                if (turnState == TurnState.StartUnitMovement)
                {
                    for(int i = 0; i < Unit.unitList.Count; i++)
                    {
                        if(Unit.unitList[i].scheduledBehavior == PlayerManager.ScheduledBehavior.Move)
                        {
                            movingUnitsCount++;
                            Unit.unitList[i].StartUnitMovement();
                        }
                    }
                    turnState = TurnState.DecidePlayerBehavior;
                }
            }
            //================================CASE.1:自機ユニットの振る舞い予定が移動だった場合========================

            //================================CASE.2:自機ユニットの振る舞い予定が攻撃だった場合========================
            if (PlayerManager.instance.scheduledBehavior == PlayerManager.ScheduledBehavior.Attack)
            {
                // 01.自機ユニットの攻撃アニメーション開始し(敵の受撃アニメーションの開始も行う)、trunStateを遷移させ、ターン処理をここで打ち切る
                if (turnState == TurnState.StartPlayerAttack)
                {
                    Unit.unitList[0].StartAttackAnimation();
                    turnState = TurnState.AfterPlayerAttack;
                    return;
                }
                // 02.戦闘の結果を解決し、turnStateを遷移させる
                if (turnState == TurnState.AfterPlayerAttack)
                {
                    Unit.ResolveBattleResult();
                    turnState = TurnState.DecideEnemyBehavior;
                }
                // 0x.敵ユニットの振る舞い予定を決め、turnStateを遷移させる
                if (turnState == TurnState.DecideEnemyBehavior)
                {
                    for (int i = 1; i < Unit.unitList.Count; i++)
                    {
                        Unit.unitList[i].DecideBehavior();
                    }
                    turnState = TurnState.StartUnitMovement;
                }
                // 0x.ユニット移動中真偽値を真にし、振る舞い予定が移動である全ユニットについて移動を始め、turnStateを遷移させる
                if (turnState == TurnState.StartUnitMovement)
                {
                    //unitsAreMoving = true;
                    for (int i = 0; i < Unit.unitList.Count; i++)
                    {
                        if (Unit.unitList[i].scheduledBehavior == PlayerManager.ScheduledBehavior.Move)
                        {
                            movingUnitsCount++;
                            Unit.unitList[i].StartUnitMovement();
                        }
                    }
                    turnState = TurnState.DecidePlayerBehavior;
                }
            }
            //================================CASE.2:自機ユニットの振る舞い予定が攻撃だった場合========================
        }
        //eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee【終】自機ユニットの振る舞い予定に従いターン処理を場合分けするeeeeeeeeeeeeeeeeeeeeeeeeeeeee
        
                                                                                                                                                                                                //EEEEE
        //EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE【終了】ターン処理【終了】EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
    }
}
