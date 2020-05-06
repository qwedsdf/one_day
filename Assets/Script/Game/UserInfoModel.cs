using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfoModel : MonoBehaviour
{
    public int Point {private set; get;}

    public string Name {private set; get;}

    public string UserId {private set; get;}

    private static int _currentId = 0;

    public UserInfoModel() {
        UserId = _currentId.ToString();
        _currentId++;
    }

    public void AddPoint(int point){
        Point += point;
    }

    public void SetPoint(int point){
        Point = point;
    }

    public void ResetPoint(){
        Point = 0;
    }

    public void SetName(string name){
        Name = name;
    }

    public void SetUserId(string id) {
        UserId = id;
    }
}
