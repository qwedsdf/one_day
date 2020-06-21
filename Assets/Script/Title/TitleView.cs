using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class TitleView : MonoBehaviour
{
    [SerializeField]
    private Button _changeSceneButton;
    public IObservable<Unit> OnClickChangeSceneButton => _changeSceneButton.OnClickAsObservable();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    
}
