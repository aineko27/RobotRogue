using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    //移動時間についての設定
    public float moveTime = 0.1f;
    private float inverseMoveTime;


    //ゲームオブジェクト、コンポーネントの宣言
    private BoxCollider2D bc2D;
    private Rigidbody2D rb2D;
    private Animator anim;
    private RaycastHit2D hit;
    private GameObject Arrow;
    private Animator arrowAnim;
    public LayerMask blockingLayer;

    //変数の宣言
    public bool isAlive = true;
    public int hitPoint = 0;
    [Watch] public Vector2 moveDirection;
    public Vector2 faceDirection;
    public bool isMoved = false;
    private bool canMove;

    //scheduledBehaviorの設定
    public enum ScheduledBehavior
    {
        NotDecided,
        Attack,
        Move,
        Pass
    }
    [Watch]public ScheduledBehavior scheduledBehavior;

    public enum AttackType
    {
        Punch
    }
    public AttackType attackType;

    //PlayerManagerをシングルトンで生成する準備
    public static PlayerManager instance = null;

    //Awake
    void Awake()
    {
        //PlayerManagerをシングルトンで生成する
        if (this != Instance)
        {
            //既にPlayerManagerが存在していた場合、これを破壊する
            Destroy(this);
            return;
        }

        //変数の計算
        inverseMoveTime = 1f / moveTime;
    }

    //Start
    void Start()
    {
        //各種コンポーネントの取得
        bc2D = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Arrow = transform.Find("Arrow").gameObject;
        arrowAnim = Arrow.GetComponent<Animator>();
    }

    //プレイヤーの振る舞いを決定する関数
    public void DecideBehave()
    {
        //scheduledBehaviorを未定の状態にする
        scheduledBehavior = ScheduledBehavior.NotDecided;

        //もしキー入力がされていなかったらすぐにreturnする
        if (IpnutFunction.AnyKey() == false) return;

        //ウィンドウステートによってキー入力の対応を変える
        switch (WindowManager.windowState)
        {
            //ウィンドウステートがFieldだった場合
            case WindowManager.WindowState.field:
                DecideBehaveOnField();
                break;
        }

        //もしプレイヤーの振る舞い予定が移動だった場合、gameStateをDecideEnemyBehave(敵の振る舞いを決める)に遷移させる
        if (scheduledBehavior == ScheduledBehavior.Move)
        {
            GameManager.gameState = GameManager.GameState.DecideEnemyBehavior;
        }
        //もしプレイヤーの振る舞い予定が攻撃だった場合、gameStateをDecideEnemyBehave(敵の振る舞いを決める)に遷移させる
        if (scheduledBehavior == ScheduledBehavior.Attack)
        {
            GameManager.gameState = GameManager.GameState.StartPlayerAttack;
        }
    }

    //ウィンドウステートがFieldの場合における、プレイヤーの振る舞いを決める
    private void DecideBehaveOnField()
    {
        //pが押された場合パスし、振る舞い予定をパスにする
        if (IpnutFunction.GetKey("P"))
        {
            scheduledBehavior = ScheduledBehavior.Pass;
        }

        //まずは移動判定を検知する。移動可能であれば振る舞い予定を移動にする。
        if (scheduledBehavior == ScheduledBehavior.NotDecided)
        {
            DetectMoveInput();
        }

        //次に攻撃判定を検知する。攻撃後、振る舞い予定を攻撃にする
        if (scheduledBehavior == ScheduledBehavior.NotDecided)
        {
            DetectAttackInput();
        }
    }

    //移動に関する入力を感知する関数。入力された移動が可能な場合、振る舞い予定を移動にする
    private void DetectMoveInput()
    {
        //移動方向の検知
        moveDirection.x = (int)IpnutFunction.GetAxisRaw("Horizontal");
        moveDirection.y = (int)IpnutFunction.GetAxisRaw("Vertical");

        //左右同時押しなどでmoveDirection.x,yがともに0であれば、ここでreturnする
        if (moveDirection.x == 0 && moveDirection.y == 0) return;

        //shiftが押されているのであれば、斜め方向にしか向く(移動)することができない
        if (IpnutFunction.GetKey("LeftShift") && (moveDirection.x == 0 || moveDirection.y == 0)) return;

        //プレイヤーの向きを変える(この時点では振る舞いをしたことにならない)
        if (moveDirection.x != 0 || moveDirection.y != 0)
        {
            if(moveDirection.x != 0 || moveDirection.y != 0)
            {
                faceDirection = moveDirection;
            }
            arrowAnim.SetFloat("DirectionX", faceDirection.x);
            arrowAnim.SetFloat("DirectionY", faceDirection.y);
        }

        //spaceが押されている場合は向きを変えることしかできない(振る舞いをしていない判定で戻り値を返す)
        if (IpnutFunction.GetKey("LeftControl")) return;

        //プレイヤーが移動可能かを判定する(障害物がないかのチェック)
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

    //プレイヤーの連続的な移動を行わせる関数
    public void AnimatePlayerMovement()
    {
        if (scheduledBehavior != ScheduledBehavior.Move) return;
        isMoved = false;
        StartCoroutine(MoveSmoothly());
    }

    //プレイヤーを連続的に移動させる関数
    public IEnumerator MoveSmoothly()
    {
        //終点の位置を計算
        Vector3 endPosition = transform.position + (Vector3)moveDirection;

        //現在値と終点の距離を求める
        float sqrRemainingDistance = (transform.position - endPosition).sqrMagnitude;

        //２点間の距離が0になったらループを抜ける
        while (sqrRemainingDistance > float.Epsilon)
        {
            //時間に合わせてなめらかにプレイヤーを移動させる
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, endPosition, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);

            //当たり判定を移動先の地点になるように連続的にずらしていく(こうしないとUnitの当たり判定がずれていく)
            bc2D.offset = endPosition - transform.position;

            sqrRemainingDistance = (transform.position - endPosition).sqrMagnitude;
            yield return null;
        }
        isMoved = true;
    }

    private void DetectAttackInput()
    {
        //攻撃ボタン"z"が押されていたら攻撃
        if (IpnutFunction.GetKey("Z"))
        {
            //攻撃を発射する
            attackType = AttackType.Punch;
            LaunchAttack();

            //振る舞い予定を攻撃にして戻り値を返す
            scheduledBehavior = ScheduledBehavior.Attack;
            return;
        }
    }

    private void LaunchAttack()
    {
        RaycastHit2D hit = new RaycastHit2D();
        int attackBaseDamage = 0;
        
        //攻撃方法によって攻撃の処理の仕方を変える
        switch (attackType)
        {

            case AttackType.Punch:
                //基礎攻撃ダメージ量を設定する
                attackBaseDamage = 20;

                //攻撃方法はパンチ。Unitの位置から今向いている方向１マスに向けて当たり判定をとる
                Vector2 startPosition = transform.position;
                Vector2 endPosition = startPosition + faceDirection;

                //始点と終点の間に障害物がないかを確認する。自分自身との衝突判定を取らないように工夫してある
                bc2D.enabled = false;
                hit = Physics2D.Linecast(startPosition, endPosition, blockingLayer);
                bc2D.enabled = true;
                break;

            default:
                Debug.LogError("Error: PlayerManager.LauchAttack");
                break;
        }
        if (hit.transform != null)
        {
            //殴った相手の情報で処理の仕方を変える
            switch (hit.collider.gameObject.tag)
            {
                //敵を攻撃した場合
                case "Enemy":
                    //攻撃した敵の情報を取得し、ダメージを受けた際の関数を処理する
                    EnemyController attackedEnemy = hit.collider.gameObject.GetComponent<EnemyController>();
                    FightFunction.DealDamage(this, attackedEnemy, attackBaseDamage);
                    attackedEnemy.TakeDamage(attackBaseDamage);
                    break;

                //壁を攻撃した場合
                case "OuterWall":
                    break;

                //それ以外の場合(Errorメッセージを流す)
                default:
                    break;
            }
        }
        else
        {

        }        
    }

    //プレイヤーの攻撃のアニメーションを始める
    public void AnimatePlayerAttack()
    {
        //攻撃アニメーションのトリガーをtrueにする
        anim.SetTrigger("AttackTrigger");
        //gameStateをWaitPlayerAttackComplete(プレイヤーの攻撃終了待機)に遷移させる
        GameManager.gameState = GameManager.GameState.WaitPlayerAttackComplete;
    }

    public void DetectAttackComplete()
    {
        //もしPlayerAttackのアニメーションが終了していた場合
        if(anim.GetCurrentAnimatorStateInfo(0).IsName("PlayerAttack") == false)
        {
            //gameStateをDecideEnemyBehavior(敵の振る舞いを決める)に遷移させる
            GameManager.gameState = GameManager.GameState.DecideEnemyBehavior;
        }
    }

    public void Update()
    {
        return;
    }
}
