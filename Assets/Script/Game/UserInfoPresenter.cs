using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfoPresenter : MonoBehaviour
{
    private UserInfoModel _model = new UserInfoModel();
    [SerializeField]
    private UserInfoView _view;

    public string UserId => _model.UserId;

    public int Point => _model.Point;

    private void Initialize(){
        
    }

    public void SetUserInfo(string name, int point) {
        SetName(name);
        SetPoint(point);
    }

    public void SetPoint(int point){
        _model.SetPoint(point);
        _view.SetPointText(point);
    }

    public void AddPoint(int point) {
        _model.AddPoint(point);
        _view.SetPointText(_model.Point);
    }

    public void SetName(string name){
        _model.SetName(name);
        _view.SetNameText(name);
    }

    public void SetActiveMyTurnIcon(bool isActive){
        _view.SetActiveMyTurnIcon(isActive);
    }

    public void SetUserId (string id) {
        _model.SetUserId(id);
    }
}
