using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sekiyuking : MonoBehaviour
{
    [SerializeField]
    private GameObject _sekiyu;

    [SerializeField]
    private Transform _point;

    public static bool _on = false;

    private int counter = 0;

    [SerializeField]
    private int kankaku;

    void Update(){
        if(!_on){
            return;
        }
        Instantiate(_sekiyu,_point);
    }

    public void on(){
        _on = true;
    }
}
