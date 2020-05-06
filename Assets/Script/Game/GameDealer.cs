using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDealer : SingletonMonoBehaviour<GameDealer>
{
    public int OpenCardIndex { private set; get; } = -1;

    public void SetOpenCardIndex(int num) {
        OpenCardIndex = num;
    }
}
