using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardBase : MonoBehaviour
{
    [SerializeField]
    private Image _illust;

    public bool IsOpen { private set;  get; }

    public bool IsGot { private set; get; }

    public int Id = -1;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    protected virtual void Initialize(){
        IsOpen = false;
        IsGot = false;
        Refresh();
    }

    protected virtual void Refresh() {
        Close();
    }

    protected virtual void Open(){
        var color = _illust.color;
        color.a = 1;
        _illust.color = color;
        IsOpen = true;
    }

    protected virtual void Close(){
        var color = _illust.color;
        color.a = 0;
        _illust.color = color;
        IsOpen = false;
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
}
