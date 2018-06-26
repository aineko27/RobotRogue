﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//クラス<シングルトン>の定義を行う
public class SingletonMonoBehavior<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));

                if(instance == null)
                {
                    Debug.LogError("ERROR: SingletonMonoBehavior => " + typeof(T) + " is nothing");
                }
            }
            return instance;
        }
    }
}
