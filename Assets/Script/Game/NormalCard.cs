using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;
using Photon.Pun;

public class NormalCard : CardBase
{
    [SerializeField]
    private Button _selectButton;
    private Subject<NormalCard> _openCardSubject = new Subject<NormalCard>();
    private IObservable<Unit> OnClickedCard => _selectButton.OnClickAsObservable();
    public IObservable<NormalCard> OnOpenCard => _openCardSubject.AsObservable();

    protected override void Initialize(){
        base.Initialize();

        OnOpenCard.Subscribe(_ => {
            GameDealer.Instance.SetOpenCardIndex(UniqId);
            Open();
            }).AddTo(this);

        OnClickedCard
            .Where(_ => !IsGot)
            .Where(_ => !IsOpen.Value)
            .Subscribe(_ => {
                photonView.RPC("InvokeOpenCardEvent", RpcTarget.All);
            })
            .AddTo(this);
    }

    [PunRPC]
    public void InvokeOpenCardEvent() {
        _openCardSubject.OnNext(this);
    }
}
