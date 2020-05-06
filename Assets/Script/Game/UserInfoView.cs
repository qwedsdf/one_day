using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoView : MonoBehaviour
{
    [SerializeField]
    private Text _nameText;

    [SerializeField]
    private Text _pointText;

    [SerializeField]
    private Image _myTurnIcon;

    public void SetNameText(string name) {
        _nameText.text = name;
    }

    public void SetPointText(int point) {
        _pointText.text = $"Point:{point}";
    }

    public void SetActiveMyTurnIcon(bool isActive){
        _myTurnIcon.gameObject.SetActive(isActive);
    }
}
