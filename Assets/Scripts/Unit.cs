using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    //Unitを統括するリストの宣言
    public static List<Unit> unitList = new List<Unit>();

    //インデックスナンバーの宣言
    public int indexNumber;

    //移動時間についての変数
    protected float moveTime = 0.1f;
    protected float inverseMoveTime;

    //本体に付属しているコンポーネントの宣言
    protected BoxCollider2D boxCollider2D;
    protected Rigidbody2D rigidbody2D;
    protected Animator animator;

    //コンポーネントに付属している変数の宣言
    [SerializeField]
    protected LayerMask blockingLayer;


    //変数の宣言
    public int hitPoint = 0;
    public bool isAlive = false;
    public bool canMove = false;
    public bool isMoving = false;
    protected Vector2 faceDirection = new Vector2(0, -1);
    protected Vector2 moveDirection = new Vector2(0, 0);

    //ユニット名の列挙体と変数の宣言
    public enum UnitName
    {
        Player,
        Skelton,
        EliteSkelton
    }
    public UnitName unitName;

    //振る舞い予定の列挙体と変数の宣言
    public enum ScheduledBehavior
    {
        NotDecided,
        Attack,
        Move,
        Pass
    }
    public ScheduledBehavior scheduledBehavior;

    //攻撃種類の列挙体と変数の宣言
    public enum AttackType
    {
        Punch
    }
    public AttackType attackType;

    //Awake
    protected virtual void Awake()
    {
        //変数の計算
        inverseMoveTime = 1f / moveTime;
    }

    //Start
    protected virtual void Start()
    {
        //各種コンポーネントの取得
        boxCollider2D = GetComponent<BoxCollider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    //戦闘の結果を解決する関数(static)
    public static void ResolveBattleResult()
    {
        for (int i = 0; i < Unit.unitList.Count; i++)
        {
            if (Unit.unitList[i].isAlive == false)
            {
                unitList[i].RemoveUnit();
            }
        }
    }

    //ユニットのステータスを設定する関数
    public virtual void SetUnitStatus(UnitName name)
    {
        //位置情報を設定する
        transform.position = new Vector3(1, 0, 0);

        //thisをUnitというリストに加え、indexNumberを設定する
        unitList.Add(this);
        this.indexNumber = unitList.Count - 1;

        //全unitNameで共通のステータス設定を行う
        unitName = name;
        isAlive = true;

        //個別unitNameで異なるステータス設定を行う
        switch (name)
        {
            case UnitName.Player:
                hitPoint = 15;
                break;

            case UnitName.Skelton:
                hitPoint = 13;
                break;

            case UnitName.EliteSkelton:
                hitPoint = 20;
                break;

            default:
                Debug.Log("ERROR: Unit.SetUnitStatus => This unitName is invalid");
                break;
        }
    }

    //振る舞い予定を決定する関数(abstractにする)
    public abstract void DecideBehavior();

    //移動を試みる関数
    protected abstract void TryToMove();

    //移動可能かどうかを判定し、真偽値を返す関数
    protected bool CheckCanMove()
    {
        RaycastHit2D hit;

        //移動の始点と終点を計算する
        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + moveDirection;

        //始点と終点の間に障害物がないかを確認する。自分自身との衝突判定を取らないように自分の衝突判定を偽にしてから判定を行う
        boxCollider2D.enabled = false;
        hit = Physics2D.Linecast(startPosition, endPosition, blockingLayer);
        boxCollider2D.enabled = true;

        //障害物がない場合trueを返し、障害物がある場合falseを返す
        if (hit.transform == null) return true;
        else return false;
    }

    //ユニットの連続的な移動を始める関数
    public void StartUnitMovement()
    {
        if (scheduledBehavior == ScheduledBehavior.Move)
        {
            isMoving = true;
            StartCoroutine(MoveSmoothly());
        }
    }

    //ユニットを連続的に移動させる反復処理関数
    protected IEnumerator MoveSmoothly()
    {
        //終点の位置を計算
        Vector3 endPosition = transform.position + (Vector3)moveDirection;

        //現在値と終点の距離を求める
        float sqrRemainingDistance = (transform.position - endPosition).sqrMagnitude;

        //２点間の距離が0になったらループを抜ける
        while (sqrRemainingDistance > float.Epsilon)
        {
            //時間に合わせてなめらかに自機ユニットを移動させる
            Vector3 newPosition = Vector3.MoveTowards(rigidbody2D.position, endPosition, inverseMoveTime * Time.deltaTime);
            rigidbody2D.MovePosition(newPosition);

            //当たり判定を移動先の地点になるように連続的にずらしていく(こうしないとUnitの当たり判定がずれていく)
            boxCollider2D.offset = endPosition - transform.position;

            sqrRemainingDistance = (transform.position - endPosition).sqrMagnitude;
            yield return null;
        }
        isMoving = false;
    }

    //攻撃を試みる関数
    protected abstract void TryToAttack();
    
    //攻撃を発射し、攻撃衝突判定を行う関数
    protected void LaunchAttack()
    {
        //変数の宣言
        RaycastHit2D hit = new RaycastHit2D();
        int baseAttackDamage = 0;

        //攻撃種類によって攻撃判定の処理の仕方を変える
        switch (attackType)
        {
            case AttackType.Punch:
                //基礎攻撃ダメージ量を設定する
                baseAttackDamage = 20;

                //攻撃種類はパンチ。Unitの位置から今向いている方向１マスに向けて当たり判定をとる
                Vector2 startPosition = transform.position;
                Vector2 endPosition = startPosition + faceDirection;

                //始点と終点の間に障害物がないかを確認する。自分自身との衝突判定を取らないように工夫してある
                boxCollider2D.enabled = false;
                hit = Physics2D.Linecast(startPosition, endPosition, blockingLayer);
                boxCollider2D.enabled = true;
                break;

            default:
                Debug.LogError("Error: PlayerManager.LauchAttack");
                break;
        }
        
        //もし攻撃を食らった相手がいた場合
        if (hit.transform != null)
        {
            //殴った相手の情報で処理の仕方を変える
            switch (hit.collider.gameObject.tag)
            {
                //敵を攻撃した場合
                case "Enemy":
                    //攻撃した敵の情報を取得し、ダメージを受けた際の関数を処理する
                    Unit attackedEnemy = hit.collider.gameObject.GetComponent<Unit>();
                    DealDamage(attackedEnemy, baseAttackDamage);
                    break;

                //壁を攻撃した場合
                case "OuterWall":
                    break;

                //それ以外の場合(Errorメッセージを流す)
                default:
                    break;
            }
        }
    }

    //thisが対象にダメージを与える関数
    protected void DealDamage(Unit damagedUnit, int baseAttackDamage)
    {
        //ダメージ計算のための変数宣言
        int attackDamage;
        int damage;

        //ダメージ計算を行う
        attackDamage = baseAttackDamage;
        damage = attackDamage;

        //被攻撃ユニットのヒットポイントをダメージの分だけ減らす
        damagedUnit.hitPoint -= damage;

        //ダメージ処理の結果をメッセージウィンドウで表示する
        WindowManager.MakeDamageText(this.unitName.ToString(), damagedUnit.unitName.ToString(), baseAttackDamage);

        //ダメージを受けた際のアニメーションを起動する
        damagedUnit.animator.SetTrigger("triggerTakeDamage");

        //もし被攻撃ユニットのヒットポイントが1より小さかった場合、生存判別真偽値を偽にし、経験値を与え、死亡結果をメッセージウィンドウで表示する
        if(damagedUnit.hitPoint < 1)
        {
            damagedUnit.isAlive = false;
            WindowManager.MakeDeathText(this.unitName.ToString(), damagedUnit.unitName.ToString());
            damagedUnit.animator.SetBool("isDead", true);
        }
    }
    
    //ユニットの攻撃アニメーションを始める関数
    public void StartAttackAnimation()
    {
        //攻撃アニメーションのトリガーをtrueにする
        animator.SetTrigger("triggerAttack");

        //現在作動しているアニメーションの情報を更新する
        GameManager.runningUnitAnimator = animator;
        GameManager.runningAnimationName = "Attack";
        GameManager.animationIsRunning = true;
    }

    //ユニットが死亡したときの処理を行う関数
    public void ResolveDeath()
    {
        //ユニットの消去を行う
        RemoveUnit();
    }

    //ユニットの消去を行う関数
    public void RemoveUnit()
    {
        //ユニットオブジェクトを破壊し、unitList配列から消去する
        Destroy(gameObject);
        unitList.Remove(this);
    }
}
