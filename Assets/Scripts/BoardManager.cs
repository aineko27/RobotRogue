using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : SingletonMonoBehaviour<BoardManager>
{
    //各種変数の定義
    public static Vector2Int boardWidth = new Vector2Int(-8, 8);
    public static Vector2Int boardHeight = new Vector2Int(-8, 8);

    public GameObject[] floorSprite;
    public GameObject[] outerWallSprite;

    //BoardManagerを生成する準備
    public static BoardManager instance = null;

    //Awake
    void Awake()
    {
        //boardManagerをシングルトンで生成する
        if(this != Instance)
        {
            Destroy(this);
            return;
        }
    }

    //ボードをセットアップする関数
    public void SetupBoard()
    {
        //まずはマップを生成する
        CreatMap();
    }


    public void CreatMap()
    {
        //まずは外壁を生成する
        for (int i = boardWidth.x; i < boardWidth.y + 1; i++)
        {
            for (int j = boardHeight.x; j < boardHeight.y + 1; j++)
            {
                if (i == boardWidth.x || i == boardWidth.y || j == boardHeight.x || j == boardHeight.y)
                {
                    //外壁スプライトの中からランダムでフロアを選択
                    GameObject chosenSprite = outerWallSprite[Random.Range(0, outerWallSprite.Length)];

                    //Instance関数で外壁を生成する
                    GameObject instance = Instantiate(chosenSprite, new Vector3(i, j, 0f), Quaternion.identity);
                    instance.transform.SetParent(transform);
                }
            }
        }

        //つぎにフロアを生成する
        for (int i = boardWidth.x + 1; i < boardWidth.y; i++)
        {
            for (int j = boardHeight.x + 1; j < boardHeight.y; j++)
            {
                //フロアスプライトの中からランダムでフロアを選択
                GameObject chosenSprite = floorSprite[Random.Range(0, floorSprite.Length)];

                //Instance関数で床を生成する
                GameObject instance = Instantiate(chosenSprite, new Vector3(i, j, 0f), Quaternion.identity);
                instance.transform.SetParent(transform);
            }
        }
    }

}
