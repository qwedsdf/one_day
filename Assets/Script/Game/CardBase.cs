using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Photon.Pun;

public class CardBase : MonoBehaviour, IPunObservable
{
    [SerializeField]
    private Image _illust;

    public ReactiveProperty<bool> IsOpen { private set;  get; } = new ReactiveProperty<bool>(false);

    public bool IsGot { private set; get; }

    public int Id = -1;

    public int UniqId = -1;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
        BindEvent();
    }

    protected virtual void Initialize(){
        IsOpen.Value = false;
        IsGot = false;
        Refresh();
    }

    protected virtual void Refresh() {
        Close();
    }

    protected virtual void Open(){
        IsOpen.Value = true;
    }

    protected virtual void OpenProcess(){
        var color = _illust.color;
        color.a = 1;
        _illust.color = color;
    }

    protected virtual void Close(){
        IsOpen.Value = false;
    }

    protected virtual void CloseProcess(){
        var color = _illust.color;
        color.a = 0;
        _illust.color = color;
    }

    public void SetGetFlg (bool isGet) {
        IsGot = isGet;
    }

    public void ChangeEnd() {
        if(IsGot) {
            return;
        }
        Close();
    }

    public void SetIllust(Sprite sp) {
        _illust.sprite = sp;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        // オーナーの場合
        if (stream.IsWriting)
        {
            stream.SendNext(IsOpen.Value);
        }
        // オーナー以外の場合
        else
        {
            IsOpen.Value = (bool)stream.ReceiveNext();
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        
    }

    private void BindEvent() {
        IsOpen.Subscribe(isOpen =>{
            if(isOpen) {
                OpenProcess();
                return;
            }

            CloseProcess();

        }).AddTo(this);
    }
}
