using System.Collections;
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

        OnOpenCard.Subscribe(_ => {
            Open();
            }).AddTo(this);

        OnClickedCard
            .Where(_ => !IsGot)
            .Where(_ => !IsOpen.Value)
            .Subscribe(_ => {
                GameDealer.Instance.OnNextSelectCard(UniqId);
            })
            .AddTo(this);
    }
    
    public void OnNext() {
        _openCardSubject.OnNext(this);
    }
}
