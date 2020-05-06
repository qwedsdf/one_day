using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class TitleManager : MonoBehaviour
{

    [SerializeField]
    private InputField _inputName;

    [SerializeField]
    private Button _matchingButton;

    void Start()
    {
        Initialize();
        BindEvent();
    }

    private void Initialize() {
        LoadUserData();
    }

    private void LoadUserData(){
        var key = GameDataManager.Instance.GameDataKey;
        var json = PlayerPrefs.GetString(key);
        var data = JsonUtility.FromJson<UserDataParamater>(json);
        _inputName.text = data.Name;
    }

    private void BindEvent() {
        _matchingButton.OnClickAsObservable()
            .Subscribe(_ => { 
                SetUserData();
                SceneManager.LoadScene("Game");
            })
            .AddTo(this);
    }

    private void SetUserData () {
        var userData = new UserDataParamater() {
            Name = _inputName.text,
        };
        var key = GameDataManager.Instance.GameDataKey;
        var json = JsonUtility.ToJson(userData);
        PlayerPrefs.SetString(key,json);
        PlayerPrefs.Save();

        PhotonNetwork.NickName = userData.Name;
    }
}
