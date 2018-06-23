using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    //移動時間についての設定
    public float moveTime = 0.1f;
    private float inverseMoveTime;

    //変数の宣言
    private BoxCollider2D bc2D;
    private Rigidbody2D rb2D;
    private Animator anim;
    private RaycastHit2D hit;
    public LayerMask blockingLayer;

    //変数の宣言
    public int indexNumber;
    public EnemyManager.EnemyName enemyName;
    [Watch]public bool isAlive = false;
    [Watch] public int hitPoint = 0;

    public Vector2 moveDirection;
    public bool isMoved = false;
    public  bool canMove;

    public enum ScheduledBehavior
    {
        NotDecided,
        Attack,
        Move,
        Pass
    }
    public ScheduledBehavior scheduledBehavior;

    //Awake
    void Awake()
    {
        //変数の計算
        inverseMoveTime = 1f / moveTime;
    }

    //Start
    void Start()
    {
        bc2D = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    //敵ユニットの振る舞いを決定する関数
    public void DecideBehave()
    {
        scheduledBehavior = ScheduledBehavior.NotDecided;

        //まずは攻撃できるかを確かめる

        //移動することにする。
        DecideMove();
    }

    public void DecideMove()
    {
        //移動する方向を決める
        moveDirection.x = Random.Range(-1, 2);
        moveDirection.y = Random.Range(-1, 2);
        
        //敵ユニットが移動可能かを判定する(障害物がないかのチェック)
        canMove = CheckCanMove();

        //移動可能の場合
        if (canMove == true)
        {
            //Unitの当たり判定を先んじて移動先にずらしておく(こうしないと次のUnitと移動先がかぶることがある)
            bc2D.offset = moveDirection;
            //移動できる場合、振る舞い予定をmoveにして戻り値を返す。
            scheduledBehavior = ScheduledBehavior.Move;
            return;
        }
        //移動不可能の場合
        else
        {
            scheduledBehavior = ScheduledBehavior.Pass;
            return;
        }
    }

    //移動可能かどうかを判定する関数。可能な場合true、不可能な場合falseを返す
    private bool CheckCanMove()
    {
        //移動の始点と終点を計算する
        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + moveDirection;

        //始点と終点の間に障害物がないかを確認する。自分自身との衝突判定を取らないように工夫してある
        bc2D.enabled = false;
        RaycastHit2D hit = Physics2D.Linecast(startPosition, endPosition, blockingLayer);
        bc2D.enabled = true;

        //障害物がない場合trueを返し、障害物がある場合falseを返す
        if (hit.transform == null) return true;
        else return false;
    }

    //敵を連続的に動かす関数
    public IEnumerator MoveSmoothly()
    {
        //終点の位置を計算する
        Vector3 endPosition = transform.position + (Vector3)moveDirection;

        //現在値と終点の距離を求める
        float sqrRemainingDistance = (transform.position - endPosition).sqrMagnitude;

        //斜め移動の場合は移動距離が長くなるのでより早く移動してほしい
        float moveSpeedAdjustment = 1;
        if(moveDirection.x != 0 && moveDirection.y != 0)
        {
            moveSpeedAdjustment = Mathf.Sqrt(2);
        }


        //２点間の距離が0になったらループを抜ける
        while (sqrRemainingDistance > float.Epsilon)
        {
            //時間に合わせてなめらかに敵ユニットを移動させる
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, endPosition, inverseMoveTime * Time.deltaTime * moveSpeedAdjustment);
            rb2D.MovePosition(newPosition);

            //当たり判定を移動先の地点になるように連続的にずらしていく(こうしないとUnitの当たり判定がずれていく)
            bc2D.offset = endPosition - transform.position;

            sqrRemainingDistance = (transform.position - endPosition).sqrMagnitude;
            yield return null;
        }
        isMoved = true;
    }

    //ダメージ処理を行う関数
    public void TakeDamage(int attackBaseDamage)
    {
        int damage = attackBaseDamage;
        //ダメージの分だけヒットポイントを減らす
        hitPoint -= damage;
        if (hitPoint < 1)
        {
            isAlive = false;
        }
        
        WindowManager.Instance.DisplayDamageMessage("Player", this.enemyName.ToString(), damage, !isAlive);
        anim.SetTrigger("TakeDamageTrigger");
        ////キャラクターが死亡したかの判定を行う
        //CheckEnemyDeath();
    }

    //敵が死亡したかどうかを判定する関数
    //public void CheckEnemyDeath()
    //{
    //    //もしHPが1よりも小さかった場合
    //    if (hitPoint < 1)
    //    {
    //        isAlive = false;
    //    }
    //}

    public void ResolveEnemyDeath()
    {
        //敵オブジェクトを破壊し、EnemiesContoroller配列から消去する
        Destroy(gameObject);
        EnemyManager.Instance.EnemiesController.Remove(this);
        WindowManager.Instance.DisplayDeathMessage();
    }
}