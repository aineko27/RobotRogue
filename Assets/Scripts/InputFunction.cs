using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class IpnutFunction : SingletonMonoBehaviour<IpnutFunction>
{
    //変数の宣言
    private static string inputtedKeyWrite;
    private static string inputtedKeyRead;
    private static string[] savedKeyArray;
    

    //IpnutFunctionを生成する準備
    public static IpnutFunction instance = null;

    //Awake
    void Awake()
    {
        //IpnutFunctionをシングルトンで生成する
        if (this != Instance)
        {
            Destroy(this);
            return;
        }
    }

    //現フレームで押されているキーコードを文字列としてファイルに保存する関数
    public static void WriteInputtedKey()
    {
        ////配列を初期化する
        inputtedKeyWrite = "";

        //各キーコードに対して、ボタンが押されているかの確認を行い、押されているボタンを文字列として記録する
        foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(code))
            {
                inputtedKeyWrite += "," + code.ToString();
            }
        }

        //InputtedKeyData.txtを開き、記録した文字列を１行ごとに書き込む
        StreamWriter sw = new StreamWriter("./InputtedKeyData.txt", true);
        sw.WriteLine(inputtedKeyWrite.ToString());
        sw.Flush();
        sw.Close();
    }
    
    //ファイルを読み込み、現フレームで押されたものとして再現したいキーコードを配列として読み込む関数
    public static void ReadInputtedKey(StreamReader sr)
    {
        //もしファイルの一番下の行にまで達していた場合、再生する記録がないと警告して戻り値を返す
        inputtedKeyRead = sr.ReadLine();
        if (inputtedKeyRead == null)
        {
            Debug.Log("Gamedata to replay is no exist");
            return;
        }
        //保存されているデータを読み込み、配列として保存する
        savedKeyArray = inputtedKeyRead.Split(',');
    }

    //何らかのキーが押されているかの真偽値を返す関数
    public static bool AnyKey()
    {
        //もしプレイステートがニュートラルかセーブの場合、既存のInput.anyKeyの真偽値を返す
        if (GameManager.Instance.playState == GameManager.PlayState.Save || GameManager.Instance.playState == GameManager.PlayState.Neutral)
        {
            return Input.anyKey;
        }

        //もしプレイステートがリプレイの場合、保存されたキー入力の記録に従って真偽値を返す
        else
        {
            if (savedKeyArray.Length > 1) return true;
            else return false;
        }
    }

    //与えられた(string)引数について、押されているかの真偽値を返す関数
    public static bool GetKey(string str)
    {
        //もしプレイステートがニュートラルかセーブの場合、既存のInput.GetKeyの真偽値を返す
        if(GameManager.Instance.playState == GameManager.PlayState.Save || GameManager.Instance.playState == GameManager.PlayState.Neutral)
        {
            //(string)の引数を(KeyCode)に変換する
            KeyCode kc = (KeyCode)System.Enum.Parse(typeof(KeyCode), str);

            //Input.GetKey関数に引数のキーコードを入れ、返ってきた真偽値をこの関数の真偽値にする
            return Input.GetKey(kc);
        }

        //もしプレイステートがリプレイの場合、保存されたキー入力の記録に従って真偽値を返す
        else 
        {
            //１つ目の配列には""が入っているので無視する
            for (int i = 1; i < savedKeyArray.Length; i++)
            {
                //配列の中に引数のキーコードと一致するものがあればtrueを返す
                if (str == savedKeyArray[i])
                {
                    return true;
                }
            }
            return false;
        }
    }

    //現在の入力情報から、上下左右("Horizontal","Vertical")の移動情報(-1,0,1)の実数値を返す関数
    public static float GetAxisRaw(string direction)
    {
        //もしプレイステートがニュートラルかセーブの場合、既存のGetAxisRawの真偽値を返す
        if (GameManager.Instance.playState == GameManager.PlayState.Save || GameManager.Instance.playState == GameManager.PlayState.Neutral)
        {
            return Input.GetAxisRaw(direction);
        }
        //もしプレイステートがリプレイの場合、保存されたキー入力の記録に従って真偽値を返す
        else
        {
            float value = 0;
            //directionがHorizontalの場合
            if (direction == "Horizontal")
            {
                if (GetKey("D") == true || GetKey("RightArrow") == true) value++;
                if (GetKey("A") == true || GetKey("LeftArrow") == true) value--;
            }
            else if(direction == "Vertical")
            {
                if (GetKey("W") == true || GetKey("UpArrow") == true) value++;
                if (GetKey("S") == true || GetKey("DownArrow") == true) value--;
            }

            return value;
        }
    }
}
