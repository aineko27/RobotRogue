using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Unit
{
    //子要素のゲームオブジェクトとそれに付属するコンポーネントの宣言
    private static GameObject Arrow;
    private static Animator arrowAnimator;

    //PlayerManagerをシングルトンで生成する準備
    public static PlayerManager instance = null;

    //Awake(virtualの上書き)
    protected override void Awake()
    {
        //UnitクラスのAwakeを実行
        base.Awake();

        //ゲーム開始時にGameManagerをinstanceに指定
        if (instance == null)
        {
            instance = this;
        }
        //このオブジェクト以外にGameManagerが存在する時
        else if (instance != this)
        {
            //このオブジェクトを破壊する
            Destroy(gameObject);
        }
    }

    //Start(virtualの上書き)
    protected override void Start()
    {
        //UnitクラスのStartを実行
        base.Start();

        //子要素のゲームオブジェクトとそれに付属するコンポーネントの宣言
        Arrow = transform.Find("Arrow").gameObject;
        arrowAnimator = Arrow.GetComponent<Animator>();
    }

    //自機ユニットの振る舞いを決定する関数(abstractの上書き)
    public override void DecideBehavior()
    {
        //scheduledBehaviorを未定の状態にする
        scheduledBehavior = ScheduledBehavior.NotDecided;

        //もしキー入力がされていなかったらすぐにreturnする
        if (InputFunction.AnyKey() == false) return;


        //もしコマンドウィンドウが表示されている場合
        if (GameManager.commandWindowIsDisplaying == true)
        {

        }
        //そうでない場合
        else
        {
            //pが押された場合パスし、振る舞い予定をパスにする
            if (InputFunction.GetKey("P"))
            {
                scheduledBehavior = ScheduledBehavior.Pass;
            }

            //まずは移動判定を検知する。移動可能であれば振る舞い予定を移動にする。
            if (scheduledBehavior == ScheduledBehavior.NotDecided)
            {
                TryToMove();
            }

            //次に攻撃判定を検知する。攻撃後、振る舞い予定を攻撃にする
            if (scheduledBehavior == ScheduledBehavior.NotDecided)
            {
                TryToAttack();
            }
        }
    }

    //移動を試みる関数(abstractの上書き)
    protected override void TryToMove()
    {
        //移動方向の検知
        moveDirection.x = (int)InputFunction.GetAxisRaw("Horizontal");
        moveDirection.y = (int)InputFunction.GetAxisRaw("Vertical");

        //左右同時押しなどでmoveDirection.x,yがともに0であれば、ここでreturnする
        if (moveDirection.x == 0 && moveDirection.y == 0) return;

        //shiftが押されているのであれば、斜め方向にしか向く(移動)することができない
        if (InputFunction.GetKey("LeftShift") && (moveDirection.x == 0 || moveDirection.y == 0)) return;

        //自機ユニットの向きを変える(この時点では振る舞いをしたことにならない)
        if (moveDirection.x != 0 || moveDirection.y != 0)
        {
            //faceDirectionにmoveDirectionを代入する
            faceDirection = moveDirection;

            //子要素のコンポーネントarrowAnimatorの変数にfaceDirectionを代入する
            arrowAnimator.SetFloat("faceDirectionX", faceDirection.x);
            arrowAnimator.SetFloat("faceDirectionY", faceDirection.y);
        }

        //spaceが押されている場合は移動することができない(振る舞いをしていない判定で戻り値を返す)
        if (InputFunction.GetKey("LeftControl")) return;

        //ユニットが移動可能かを判定する(障害物がないかのチェック)
        canMove = CheckCanMove();
        if (canMove == true)
        {
            //Unitの当たり判定を先んじて移動先にずらしておく(こうしないと次のUnitと移動先がかぶることがある)
            boxCollider2D.offset = moveDirection;

            //振る舞い予定をmoveにする
            scheduledBehavior = ScheduledBehavior.Move;
        }
    }

    //移動可能かどうかを判定し、真偽値を返す関数
    //protected bool CheckCanMove() => Unit.CheckCanMove()を継承

    //ユニットの連続的な移動を始める関数
    //public void StartUnitMovement() => Unit.StartUnitMovement()を継承

    //ユニットを連続的に移動させる反復処理関数
    //protected IEnumerator MoveSmoothly() => Unit.MoveSmoothly()を継承

    //移動を試みる関数
    protected override void TryToAttack()
    {
        //攻撃ボタン"z"が押されていたら攻撃
        if (InputFunction.GetKey("Z"))
        {
            attackType = AttackType.Punch;
        }
        else return;

        //振る舞い予定を攻撃にする
        scheduledBehavior = ScheduledBehavior.Attack;

        //攻撃を発射する
        LaunchAttack();
    }

    //攻撃を発射し、攻撃衝突判定を行う関数
    //protected void LaunchAttack() => Unit.LaunchAttack()を継承

    //thisが対象にダメージを与える関数
    //protected void DealDamage() => Unit.DealDamage()を継承

    //ユニットの攻撃アニメーションを始める関数
    //protected void StartAttackAnimation() => Unit.StartAttackAnimation()を継承

    //ユニットが死亡したときの処理を行う関数
    //public void ResolveDeath() => Unit.ResolveDeath()を継承

    //ユニットの消去を行う関数
    //public void RemoveUnit() => Unit.RemoveUnit()を継承
}