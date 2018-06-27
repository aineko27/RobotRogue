using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //ゲームオブジェクトの取得
    public static GameObject ReferenceObject;

    //メインカメラとプレイヤーのオフセット値の宣言
    private static Vector3 offset;

    //Awake
    void Awake()
    {

    }

    //Start
    void Start()
    {
        //参照オブジェクトを取得する
        ReferenceObject = GameObject.FindGameObjectWithTag("Player");
        //カメラのオフセット値を求める
        offset = transform.position - ReferenceObject.transform.position;
    }

    //Update
    void Update()
    {
        if (ReferenceObject == null)
        {
            return;
        }
        else
        {
            //MainCameraの位置情報を更新する
            transform.position = ReferenceObject.transform.position + offset;
        }
    }
}
