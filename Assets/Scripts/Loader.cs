using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    //各GameObjectの宣言
    public GameObject gameManager;
    public GameObject boardManager;
    public GameObject windowManager;
    public GameObject player;
    //public GameObject enemyManager;

    void Awake()
    {
        GameObject instance;
        //各GameObjectのインスタンス化
        if (GameManager.instance == null)  instance = Instantiate(gameManager);
        if (BoardManager.instance == null)  Instantiate(boardManager);
        if (WindowManager.instance == null)  Instantiate(windowManager);
        if (PlayerManager.instance == null) Instantiate(player);
        //if (EnemyManager.instance == null) Instantiate(enemyManager);
    }
}
