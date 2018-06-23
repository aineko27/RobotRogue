using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightFunction : MonoBehaviour {

    public static void DealDamage<T, U>(T attackUnit, U attackedUnit, int attackBaseDamage) where T : class where U : class
    {
        //Debug.Log(attackedUnit.isAlive);



    }
}
