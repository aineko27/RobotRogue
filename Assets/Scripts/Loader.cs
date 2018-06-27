using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    //各GameObjectの宣言
    public GameObject gameManager;
    public GameObject boardManager;
    public GameObject canvas;
    public GameObject player;
    private GameObject CanvasObject;

    void Awake()
    {
        //各GameObjectのインスタンス化
        if (GameManager.instance == null) Instantiate(gameManager);
        if (BoardManager.instance == null) Instantiate(boardManager);
        Instantiate(canvas);
        if (PlayerManager.instance == null) Instantiate(player);
    }
}
