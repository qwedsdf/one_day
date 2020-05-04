using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TestScene : MonoBehaviourPunCallbacks
{
    private static readonly string BasePath = "Card/";

    [SerializeField]
    private Transform _parentTrans;
    [SerializeField]
    private List<GameObject> _cardList;

    

    
}
