using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scenes
{
    Title,
    MakeUser,
    Game,
    Data,
}

public class SceneLoadManager : SingletonMonoBehaviour<SceneLoadManager>
{
    private void Start() {
        DontDestroyOnLoad(this);
    }
    
    public void LoadScene(Scenes scene)
    {
        switch (scene)
        {
            case Scenes.Title:
                SceneManager.LoadScene("Title", LoadSceneMode.Single);
                break;
            
            case Scenes.MakeUser:
                SceneManager.LoadScene("MakeUser",LoadSceneMode.Single);
                break;

            case Scenes.Game:
                SceneManager.LoadScene("Game",LoadSceneMode.Single);
                break;

            case Scenes.Data:
                SceneManager.LoadScene("DataScene",LoadSceneMode.Single);
                break;
        }
    }
}
