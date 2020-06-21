using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserDataParamater {
    public string Name = string.Empty;
}

public class GameDataManager : SingletonMonoBehaviour<GameDataManager>
{
    public string GameDataKey { private set; get; } = "oasdfjklkjasjg";

    private void Start() {
        DontDestroyOnLoad(this);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadScene(){
        SceneManager.LoadScene("DataScene",LoadSceneMode.Additive);
    }
}
