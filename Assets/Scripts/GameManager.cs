using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    //ファイルの宣言
    StreamReader sr;

    //フレームカウントを宣言、各フレームごとにインクリメントする
    [Watch]public static int frameCount = 0;
    //ランダムシードを宣言する
    public int randomSeed;

    //gameStateの設定
    public enum GameState
    {
        TitleWindow,
        DecidePlayerBehavior,
        DecideEnemyBehavior,
        StartUnitMovement,
        StartPlayerAttack,
        WaitPlayerAttackComplete,
        WaitMovementComplete,
    }
    [Watch] public static GameState gameState;

    //playStateの設定
    public enum PlayState
    {
        Neutral,
        Save,
        Replay
    }
    [Watch]public PlayState playState;

    //GameManagerを生成する準備
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
            //まずは既存のInputtedKeyData.txtファイルを消す
            System.IO.File.Delete("./InputtedKeyData.txt");
            //ランダムシードを設定し、InitState関数に入れる
            randomSeed = UnityEngine.Random.Range(0, 10000);
            //randomSeed = 42;
            UnityEngine.Random.InitState(randomSeed);

            //設定したランダムシードをInputtedKeyData.txtの１行目に保存する
            StreamWriter sw = new StreamWriter("./InputtedKeyData.txt", true);
            sw.WriteLine(randomSeed.ToString());
            sw.Flush();
            sw.Close();

        }
        //もしプレイステートがReplayの場合
        else if (playState == PlayState.Replay)
        {
            //まずはInputtedKeyData.txtからランダムシードを読み込み、InitState関数に入れる
            sr = new StreamReader("./InputtedKeyData.txt");
            randomSeed = int.Parse(sr.ReadLine());
            UnityEngine.Random.InitState(randomSeed);
        }

        //ゲーム開始
        InitGame();
    }

    //ゲーム開始起動時に処理する関数
    private void InitGame()
    {
        //gameStateをタイトルウィンドウにする
        gameState = GameState.TitleWindow;

        //ダンジョンをセットアップする
        SetupDungeon();
    }

    //ダンジョンをセットアップする関数
    public void SetupDungeon()
    {
        //gameStateとwindowStateの更新
        gameState = GameState.DecidePlayerBehavior;
        WindowManager.windowState = WindowManager.WindowState.field;

        //ボードのセットアップ
        BoardManager.Instance.SetupBoard();

        //敵の配置
        EnemyManager.Instance.DeployEnemy(EnemyManager.EnemyName.Skelton, 2);
    }

    private void Update()
    {
        //################################################デバッグ用################################################

        //プレイステートがSaveの場合、キーが押されているキーコードの情報を取得し、InputtedKeyData.txtファイルに書き込む
        if (playState == PlayState.Save)
        {
            IpnutFunction.WriteInputtedKey();
        }
        //プレイステートがReadの場合、InputtedKeyData.txtファイルを読み込み、キーが押されているキーコードの情報を取得し、savedKeyArray配列として保存する
        if (playState == PlayState.Replay)
        {
            IpnutFunction.ReadInputtedKey(sr);
        }
        //################################################################################################################################




        //################################################ターン処理を行う################################################

        //================================テキストウィンドウが表示されている場合================================
        if(WindowManager.Instance.Panel.activeSelf == true)
        {
            if(IpnutFunction.AnyKey() == true)
            {

            }
            else
            {
                return;
            }
        }

        //================================ プレイヤーの振る舞い予定を決定する ================================

        //現在のゲームステートによってキー受付を変える
        //ゲームステートがプレイヤーの振る舞い前だったら
        if (gameState == GameState.DecidePlayerBehavior)
        {
            if(PlayerManager.Instance.scheduledBehavior != PlayerManager.ScheduledBehavior.NotDecided)
            {
                WindowManager.Instance.SetWindowActive(false);

            }
            //プレイヤーの振る舞い予定を決める
            PlayerManager.Instance.DecideBehave();
            
        }
        
        //================================プレイヤーの振る舞い予定によって、今後のゲーム処理の順番を変える================================

        if (PlayerManager.Instance.scheduledBehavior != PlayerManager.ScheduledBehavior.NotDecided)
        {
            //****************プレイヤーの振る舞い予定が、移動だった場合****************
            if (PlayerManager.Instance.scheduledBehavior == PlayerManager.ScheduledBehavior.Move)
            {
                // 01.//敵ユニットの振る舞い予定を決める
                if (gameState == GameState.DecideEnemyBehavior)
                {
                    EnemyManager.Instance.DecideEnemyBehave();
                }
                // 02.プレイヤーと敵ユニットの移動を始める
                if (gameState == GameState.StartUnitMovement)
                {
                    if (PlayerManager.Instance.scheduledBehavior == PlayerManager.ScheduledBehavior.Move) PlayerManager.Instance.AnimatePlayerMovement();
                    EnemyManager.Instance.AnimateEnemyMovement();
                    gameState = GameState.WaitMovementComplete;
                }
                // 03.各ユニットの移動アニメーション終了の検知
                if (gameState == GameState.WaitMovementComplete)
                {
                    DetectMovementComplete();
                }
                // 04.罠や状態異常などの特殊効果の解決を行う

                // 05.
            }
            //********************************************************************************

            //****************プレイヤーの振る舞い予定が、攻撃だった場合****************
            if (PlayerManager.Instance.scheduledBehavior == PlayerManager.ScheduledBehavior.Attack)
            {

                // 01.プレイヤーの攻撃アニメーションと、攻撃を受けた敵ユニットの怯みアニメーションの開始
                if (gameState == GameState.StartPlayerAttack)
                {
                    PlayerManager.Instance.AnimatePlayerAttack();
                    return;
                }

                // 02.アニメーション終了の検知
                if (gameState == GameState.WaitPlayerAttackComplete)
                {
                    PlayerManager.Instance.DetectAttackComplete();
                }

                // 敵ユニットの死亡判定を調べる
                if (gameState == GameState.DecideEnemyBehavior)
                {
                    for (int i = 0; i < EnemyManager.Instance.EnemiesController.Count; i++)
                    {
                        if (EnemyManager.Instance.EnemiesController[i].isAlive == false)
                        {
                            EnemyManager.Instance.EnemiesController[i].ResolveEnemyDeath();
                        }
                    }
                }

                // 03.攻撃されたユニットが反撃できるのであれば、反撃する

                // 04.反撃アニメーションと、怯みアニメーションの開始

                // 05.アニメーション終了の検知

                // 06.その他の敵ユニットの振る舞い予定を決める
                if (gameState == GameState.DecideEnemyBehavior)
                {
                    EnemyManager.Instance.DecideEnemyBehave();
                }
                // 07.プレイヤーと敵ユニットの移動を始める
                else if (gameState == GameState.StartUnitMovement)
                {
                    PlayerManager.Instance.AnimatePlayerMovement();
                    EnemyManager.Instance.AnimateEnemyMovement();
                    gameState = GameState.WaitMovementComplete;
                }
                // 08.各ユニットの移動アニメーション終了の検知
                else if (gameState == GameState.WaitMovementComplete)
                {
                    DetectMovementComplete();
                }

                // 09.移動後の罠や状態以上の解決

                // 10.移動しなかった敵ユニットの攻撃処理
            }
            //********************************************************************************
        }

        //frameCountをインクリメントする
        frameCount++;

    }

    public void GameOver()
    {
        //ゲームオーバー時
    }

    //各ユニットの移動が完了したことを検知する関数
    private void DetectMovementComplete()
    {
        //プレイヤーの移動が完了していなかったら戻り値を返す
        if (PlayerManager.Instance.scheduledBehavior == PlayerManager.ScheduledBehavior.Move && PlayerManager.Instance.isMoved == false) return;

        //敵ユニットに対して、移動が完了していたらxをインクリメントする
        int x = 1;
        for(int i = 0; i < EnemyManager.Instance.EnemiesController.Count; i++)
        {
            if (EnemyManager.Instance.EnemiesController[i].isMoved == true) x++;
        }
        //xが敵ユニットの数だけインクリメントされていたらすべての敵の移動が終わっているのでゲームステートを遷移させる
        if (x > EnemyManager.Instance.EnemiesController.Count)
        {
            gameState = GameState.DecidePlayerBehavior;
        }
    }
}
