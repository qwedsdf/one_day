﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class NormalCard : CardBase
{
    [SerializeField]
    private Button _selectButton;
    private Subject<NormalCard> _openCardSubject = new Subject<NormalCard>();
    public IObservable<Unit> OnClickedCard => _selectButton.OnClickAsObservable();
    public IObservable<NormalCard> OnOpenCard => _openCardSubject.AsObservable();

    protected override void Initialize(){
        base.Initialize();

        OnClickedCard
            .Where(_ => !IsGot)
            .Where(_ => !IsOpen)
            .Subscribe(_ => {
                Open();
                _openCardSubject.OnNext(this);   
            })
            .AddTo(this);
    }
}
