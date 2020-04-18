using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//设置游戏状态
public enum GameState
{
    easy,
    diff,
}


public class Buttons : MonoBehaviour
{
    //存储游戏主界面UI与游戏内UI
    public GameObject mainUI;
    public GameObject gameUI;

    //创建一个全局的静态变量
    public static GameState state;

    //状态文本
    public Text text;

    public void Start()
    {
        reText();

        if (mainUI != null)
        {
            mainUI.SetActive(true);
        }
        if (gameUI != null)
        {
            gameUI.SetActive(false);
        }
    }

    //主界面按钮方法
    public void StartGame()
    {
        mainUI.SetActive(false);
        gameUI.SetActive(true);
    }

    public void SwitchGame()
    {
        if (state == GameState.easy)
        {
            state = GameState.diff;
        }
        else
        {
            state = GameState.easy;
        }

        reText();
    }

    public void LeaveGame()
    {

    }

    private void reText()
    {
        switch (state)
        {
            case GameState.easy:
                text.text = "游戏模式 : 简单";
                break;
            case GameState.diff:
                text.text = "游戏模式 : 困难";
                break;
        }
    }

    //其他界面按钮方法
    public void BackMain()
    {
        mainUI.SetActive(true);
        gameUI.SetActive(false);
    }
}
