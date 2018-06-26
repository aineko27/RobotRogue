using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : Unit
{
    //敵ユニットオブジェクトを統括するゲームオブジェクトの宣言
    public static GameObject Enemy;

    //Watch用に使用する変数の宣言
    [Watch]public ScheduledBehavior testWatch;
    
    //Awake(virtualの上書き)
    protected override void Awake()
    {
        //UnitクラスのAwakeを実行
        base.Awake();

        //Enemyという名前のゲームオブジェクトを生成する。このゲームオブジェクトで敵ユニットの管理を行う
        Enemy = new GameObject();
        Enemy.name = "Enemy";
    }

    //Start(virtualの上書き)
    protected override void Start()
    {
        //UnitクラスのStartを実行
        base.Start();
    }

    //LateUpdate(デバッグ用にしようがなく使う)
    void LateUpdate()
    {
        testWatch = scheduledBehavior;
    }

    public override void SetUnitStatus(UnitName name)
    {
        //Enemyをインスタンス化し、EnemyManagerの子要素にする
        transform.SetParent(Enemy.transform);

        //UnitクラスのSetUnitStatusを実行
        base.SetUnitStatus(name);
    }

    //自機ユニットの振る舞いを決定する関数(abstractの上書き)
    public override void DecideBehavior()
    {
        //scheduledBehaviorを未定の状態にする
        scheduledBehavior = ScheduledBehavior.NotDecided;

        //攻撃を試みる
        TryToAttack();

        //移動を試みる
        TryToMove();
    }

    //移動を試みる関数(abstractの上書き)
    protected override void TryToMove()
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
            boxCollider2D.offset = moveDirection;

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

    //移動可能かどうかを判定し、真偽値を返す関数
    //protected bool CheckCanMove() => Unit.CheckCanMove()を継承

    //ユニットの連続的な移動を始める関数
    //public void StartUnitMovement() => Unit.StartUnitMovement()を継承

    //ユニットを連続的に移動させる反復処理関数
    //protected IEnumerator MoveSmoothly() => Unit.MoveSmoothly()を継承

    //攻撃を試みる関数
    protected override void TryToAttack()
    {

    }

    //攻撃を発射し、攻撃衝突判定を行う関数
    //protected void LaunchAttack() => Unit.LaunchAttack()を継承

    //thisが対象にダメージを与える関数
    //protected void DealDamage() => Unit.DealDamage()を継承

    //ユニットの攻撃アニメーションを始める関数
    //public void StartAttackAnimation() => Unit.StartAttackAnimation()を継承

    //ユニットが死亡したときの処理を行う関数
    //public void ResolveDeath() => Unit.ResolveDeath()を継承

    //ユニットの消去を行う関数
    //public void RemoveUnit() => Unit.RemoveUnit()を継承
}
