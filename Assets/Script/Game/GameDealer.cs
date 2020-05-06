using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class GameDealer : SingletonMonoBehaviour<GameDealer>
{
    public int OpenCardIndex { private set; get; } = -1;

    private Subject<int> _selectCard = new Subject<int>();

    public IObservable<int> OnSelectCard => _selectCard.AsObservable();

    public void OnNextSelectCard(int uniqId){
        _selectCard.OnNext(uniqId);
    }
}
