using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MenuItemManager : MonoBehaviour
{

    static void SetLevel(int level)
    {
        LevelManager.Instance.currentLevel = level;
        LevelManager.Instance.currentLevelText.text = "Level " + level;
    }

    [MenuItem("SetCurrentLevel/%#1")]
    static void SetLevel1()
    {
        SetLevel(1);
    }

    [MenuItem("SetCurrentLevel/%#2")]
    static void SetLevel2()
    {
        SetLevel(2);
    }

    [MenuItem("SetCurrentLevel/%#3")]
    static void SetLevel3()
    {
        SetLevel(3);
    }

    [MenuItem("SetCurrentLevel/%#4")]
    static void SetLevel4()
    {
        SetLevel(4);
    }

    [MenuItem("SetCurrentLevel/%#5")]
    static void SetLevel5()
    {
        SetLevel(5);
    }

    [MenuItem("SetCurrentLevel/%#6")]
    static void SetLevel6()
    {
        SetLevel(6);
    }

    [MenuItem("SetCurrentLevel/%#7")]
    static void SetLevel7()
    {
        SetLevel(7);
    }

    [MenuItem("SetCurrentLevel/%#8")]
    static void SetLevel8()
    {
        SetLevel(8);
    }

    [MenuItem("SetCurrentLevel/%#9")]
    static void SetLevel9()
    {
        SetLevel(9);
    }

    [MenuItem("SetCurrentLevel/%#10")]
    static void SetLevel10()
    {
        SetLevel(10);
    }

    
}
