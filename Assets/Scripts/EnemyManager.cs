using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : SingletonMonoBehaviour<EnemyManager>
{
    //ゲームオブジェクト、コンポーネントの宣言
    public List<GameObject> Enemies = new List<GameObject>();
    public List<EnemyController> EnemiesController = new List<EnemyController>();
    public GameObject Enemy;
    
    //変数の宣言
    private Vector2 moveDirection;

    //EnemyManagerをシングルトンで生成する準備
    public static EnemyManager instance = null;

    public enum EnemyName
    {
        Skelton,
        Skull
    }

    //Awake
    void Awake()
    {
        //EnemyManagerをシングルトンで生成する
        if(this != Instance)
        {
            Destroy(this);
            return;
        }
    }

    //Start
    private void Start()
    {

    }

    //敵を(num個)生成する関数
    public void DeployEnemy(EnemyName enemyName, int num)
    {
        for (int i = 0; i < num; i++)
        {
            //Enemyをインスタンス化し、EnemyManagerの子要素にする
            GameObject instance = Instantiate(Enemy, new Vector3(i+1, i+1, 0), Quaternion.identity);
            instance.transform.SetParent(transform);

            //インスタンス化したEnemyをEnemiesという配列に加え、Enemyを動かすスクリプトをEnemiesControllerという配列に加える
            EnemyController enemyScript = instance.GetComponent<EnemyController>();
            EnemiesController.Add(enemyScript);
            EnemiesController[EnemiesController.Count-1].indexNumber = EnemiesController.Count-1;

            //敵の名前に合わせて敵のパラメータを変える
            switch (enemyName)
            {
                case EnemyName.Skull:
                    enemyScript.enemyName = enemyName;
                    enemyScript.isAlive = true;
                    enemyScript.hitPoint = 20;
                    break;

                case EnemyName.Skelton:
                    enemyScript.enemyName = enemyName;
                    enemyScript.isAlive = true;
                    enemyScript.hitPoint = 20;
                    break;

                default:
                    Debug.LogError("Error: EnemyManagerDeployEnemy");
                    break;
            }
        }

    }

    //敵の振る舞いを決定する関数
    public void DecideEnemyBehave()
    {
        //それぞれの敵ユニットに対して判定を行う
        for(int i = 0; i < EnemiesController.Count; i++)
        {
            //すでにこの敵ユニットの振る舞い予定が決定している場合、次の敵ユニットの振る舞い決定に移る
            //if (EnemiesController[i].scheduledBehavior != EnemyController.ScheduledBehavior.NotDecided) continue;
            //そうでない場合、この敵ユニットの振る舞い予定を決める
            EnemiesController[i].DecideBehave();
        }

        //プレイヤーの行動によって次のゲームステートを決定する
        //プレイヤーが移動をした場合はユニットの移動を先に行う
        if(PlayerManager.Instance.scheduledBehavior == PlayerManager.ScheduledBehavior.Move)
        {
            GameManager.gameState = GameManager.GameState.StartUnitMovement;
        }

        //プレイヤーが攻撃をした場合はユニットの移動を先に行う
        if (PlayerManager.Instance.scheduledBehavior == PlayerManager.ScheduledBehavior.Attack)
        {
            GameManager.gameState = GameManager.GameState.StartUnitMovement;
        }
    }

    //敵ユニットの連続的な移動を行わせる関数
    public void AnimateEnemyMovement()
    {
        for (int i = 0; i < EnemiesController.Count; i++)
        {
            if (EnemiesController[i].scheduledBehavior == EnemyController.ScheduledBehavior.Move)
            {
                EnemiesController[i].isMoved = false;
                StartCoroutine(EnemiesController[i].MoveSmoothly());
            }
            else
            {
                EnemiesController[i].isMoved = true;
            }
        }
    }
}
