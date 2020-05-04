using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UniRx;
using Photon.Realtime;
using UniRx.Async;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks,IPunObservable
{
    private static readonly int EmptyCardId = -1;
    private static readonly string CardObjPath = "Card/Card";
    private static readonly int WaitTIme = 500;

    [SerializeField]
    private UserInfoPresenter _playerInfo;

    [SerializeField]
    private UserInfoPresenter _enemyInfo;

    [SerializeField]
    private Transform _parentTrans;

    [SerializeField]
    private Image _matchImage;

    [SerializeField]
    private Image _notMatchImage;
    
    [SerializeField]
    private Image _cover;
    private bool _isOpenedFirstCard = false;
    private CardBase _firstOpenCard;
    private CardBase _secondCard;
    private UserInfoPresenter _currentUserInfo;
    private int _currentUserIndex;
    private List<CardBase> _cardList = new List<CardBase>();

    private List<UserInfoPresenter> _userList = new List<UserInfoPresenter>();

    private void Start(){
        Initialize();
        
        // PhotonServerSettingsに設定した内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// マスターサーバーへの接続が成功した時に呼ばれるコールバック
    /// </summary>
    public override void OnConnectedToMaster() {
        // "room"という名前のルームに参加する（ルームが無ければ作成してから参加する）
        PhotonNetwork.JoinOrCreateRoom("room", new RoomOptions(), TypedLobby.Default);
    }

    /// <summary>
    /// マッチングが成功した時に呼ばれるコールバック
    /// </summary>
    public override void OnJoinedRoom() {
        if (PhotonNetwork.IsMasterClient) {
            photonView.RPC("CreateCards", RpcTarget.All);
        }
    }



    /// <summary>
    /// 初期処理
    /// </summary>
    private void Initialize() {
        SetDummyPlayerData();
        _userList = new List<UserInfoPresenter>() {
            _playerInfo,
            _enemyInfo,
        };
        _currentUserIndex = Random.Range(0,_userList.Count);
        _currentUserInfo = _userList[_currentUserIndex];
        _currentUserInfo.SetActiveMyTurnIcon(true);
    }

    /// <summary>
    /// ダミー用ユーザーデータを設定
    /// </summary>
    private void SetDummyPlayerData() {
        _playerInfo.SetUserInfo("卍太郎",0);
        _enemyInfo.SetUserInfo("カニ太郎",0);
    }

    /// <summary>
    /// カードを開く
    /// </summary>
    /// <param name="card">開いたカードの情報</param>
    private void OpenCard(CardBase card){
        if(_isOpenedFirstCard == false) {
            _firstOpenCard = card;
            _isOpenedFirstCard = true;
            return;
        }

        _secondCard = card;

        // 違うカードであれば
        if(_firstOpenCard.Id != _secondCard.Id) {
            NotMatchProcess().Forget();
            return;
        }

        // 同じカードであれば
        MatchProcess().Forget();
    }

    /// <summary>
    /// カードが一致しなかった時の処理
    /// </summary>
    private async UniTask NotMatchProcess(){
        await OpenTwoCardEffect(false);
        ChangeNextUser();
        Reset();
    }

    /// <summary>
    /// カードが一致した時の処理
    /// </summary>
    private async UniTask MatchProcess(){
        _firstOpenCard.SetGetFlg(true);
        _secondCard.SetGetFlg(true);
        _currentUserInfo.AddPoint(1);
        await OpenTwoCardEffect(true);
        Reset();
    }

    /// <summary>
    /// カードを2つ開いた時の演出
    /// </summary>
    /// <param name="isMatch">開いたカードが一致しているか</param>
    private async UniTask OpenTwoCardEffect(bool isMatch){
        var effectObj = isMatch ? _matchImage.gameObject : _notMatchImage.gameObject;
        var soundName = isMatch ? "match" : "but";
        SoundManager.Instance.Play(soundName);
        effectObj.SetActive(true);
        await Task.Delay(WaitTIme);
        effectObj.SetActive(false);
    }

    /// <summary>
    /// ターンを次のメンバーに移動する
    /// </summary>
    private void ChangeNextUser() {
        _currentUserInfo.SetActiveMyTurnIcon(false);
        _currentUserIndex++;
        _currentUserIndex %= _userList.Count;
        _currentUserInfo = _userList[_currentUserIndex];
        _currentUserInfo.SetActiveMyTurnIcon(true);
    }

    /// <summary>
    /// 切り替え
    /// </summary>
    private void Reset(){
        _isOpenedFirstCard = false;
        _firstOpenCard.ChangeEnd();
        _secondCard.ChangeEnd();
    }

    /// <summary>
    /// カードを生成
    /// </summary>
    [PunRPC]
    private void CreateCards(){
        var cardSprites = Resources.LoadAll("Card/CardSprite",typeof(Sprite));
        
        foreach (var (sprite, index) in cardSprites.Select((item, index) => (item, index)))
        {
            for(int f = 0; f < 2; f++){
                var card = PhotonNetwork.Instantiate(CardObjPath,transform.position, Quaternion.identity);
                card.transform.parent = _parentTrans;
                card.transform.localScale = Vector3.one;
                var cardInfo = card.GetComponent<NormalCard>();
                _cardList.Add(cardInfo);
                cardInfo.Id = index;
                cardInfo.SetIllust((Sprite)sprite);
                cardInfo.OnOpenCard
                    .Subscribe(info => OpenCard(info))
                    .AddTo(this);
            }

        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        // オーナーの場合
        if (stream.IsWriting)
        {
            stream.SendNext(_currentUserIndex);
        }
        // オーナー以外の場合
        else
        {
            _currentUserIndex = (int)stream.ReceiveNext();
            Debug.LogError(_currentUserIndex);
        }
    }
}
