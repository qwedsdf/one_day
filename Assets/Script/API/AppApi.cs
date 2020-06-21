using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Networking;

public class AppApi
{
    public static async UniTask<UserData> GetUserData(int userId){
        var url = "http://localhost/apiTest/GetUserDataAPI.php";
        var form = new WWWForm();
        form.AddField("id", userId);
        var result = UnityWebRequest.Post(url,form);

        await result.SendWebRequest().ToUniTask();
        
        if(result.isNetworkError){
            Debug.LogError("はーや、おわりー");
        }

        return JsonUtility.FromJson<UserData>(result.downloadHandler.text);
    }

    public static async UniTask<string> CreateUserData(UserData userData){
        var url = "http://localhost/apiTest/CreateUserDataAPI.php";
        var form = new WWWForm();
        form.AddField("name", userData.Name);
        var result = UnityWebRequest.Post(url,form);

        await result.SendWebRequest().ToUniTask();
        
        if(result.isNetworkError){
            Debug.LogError("はーや、おわりー");
        }

        Debug.Log(result.downloadHandler.text);

        return result.downloadHandler.text;
    }
}
