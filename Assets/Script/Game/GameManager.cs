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
    private static readonly int WaitTIme = 500;

    [SerializeField]
    private GameObject _cardPrefab;

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
    private bool _isMatching = false;
    private CardBase _firstOpenCard;
    private CardBase _secondCard;
    private UserInfoPresenter _currentUserInfo;
    private ReactiveProperty<string> _currentUserId = new ReactiveProperty<string>(string.Empty);
    private int _currentUserIndex = 0;
    private List<CardBase> _cardList = new List<CardBase>();
    private List<UserInfoPresenter> _userList = new List<UserInfoPresenter>();

    private void Start(){
        Initialize();
        BindEvent();
        // PhotonServerSettingsに設定した内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// マスターサーバーへの接続が成功した時に呼ばれるコールバック
    /// </summary>
    public override void OnConnectedToMaster() {
        RoomOptions options = new RoomOptions();
        options.PublishUserId = true; // お互いにユーザＩＤが見えるようにする。
        options.MaxPlayers = 2; // 最大人数もきちんと定義しておく。

        // "room"という名前のルームに参加する（ルームが無ければ作成してから参加する）
        PhotonNetwork.JoinOrCreateRoom("room", options, TypedLobby.Default);
    }

    /// <summary>
    /// マッチングが成功した時に呼ばれるコールバック
    /// </summary>
    public override void OnJoinedRoom() {
        OnMatch().Forget();
    }

    private void CreateField() {
        if (PhotonNetwork.IsMasterClient) {
            CreateCards();
            LotteryUserTurn();
        }
    }

    private async UniTask OnMatch(){
        await UniTask.WaitWhile(() => PhotonNetwork.PlayerList.Length == 1);
        _isMatching = true;
        SetupPlayerInfo();
        CreateField();
    }

    private void SetupPlayerInfo(){
        foreach (var (player, index) in PhotonNetwork.PlayerList.Select((player, index) => (player, index)))
        {
            if(player.IsLocal){
                _playerInfo.SetUserId(player.UserId);
                continue;
            }
            _enemyInfo.SetName(player.NickName);
            _enemyInfo.SetUserId(player.UserId);
        }

        _userList = new List<UserInfoPresenter>() {
            _playerInfo,
            _enemyInfo,
        };
    }



    /// <summary>
    /// 初期処理
    /// </summary>
    private void Initialize() {
        SetUserData();
        _isMatching = false;
    }

    private void BindEvent() {
        _currentUserId
            .Where(_ => _isMatching)
            .Subscribe(SetUserTurn)
            .AddTo(this);
    }

    private void LotteryUserTurn() {
        _currentUserIndex = Random.Range(0,_userList.Count);
        _currentUserId.Value = _userList[_currentUserIndex].UserId;
    }

    private void SetUserTurn(string userId) {
        // タッチ不可に
        _cover.gameObject.SetActive(true);

        _userList.ForEach(user => {
            user.SetActiveMyTurnIcon(false);
        });
        
        var info = _userList.FirstOrDefault(user => user.UserId == userId);
        if(info == null) {
            Debug.LogError($"一致するUserIDがありません。UserId:{userId}");
            return;
        }
        _currentUserInfo = info;
        _currentUserInfo.SetActiveMyTurnIcon(true);

        // 自分のターンであれば、カードを選択可能に
        if(_playerInfo.UserId == _currentUserInfo.UserId) {
            _cover.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// プレイヤーデータの表示
    /// </summary>
    private void SetUserData() {
        var key = GameDataManager.Instance.GameDataKey;
        var json = PlayerPrefs.GetString(key);
        var userData = JsonUtility.FromJson<UserDataParamater>(json);
        _playerInfo.SetName(userData.Name);
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
        _currentUserIndex++;
        _currentUserIndex %= _userList.Count;
        _currentUserId.Value = _userList[_currentUserIndex].UserId;
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
    private void CreateCards(){
        var cardSprites = Resources.LoadAll("Card/CardSprite",typeof(Sprite));
        
        foreach (var (sprite, index) in cardSprites.Select((item, index) => (item, index)))
        {
            for(int f = 0; f < 2; f++){
                var card = PhotonNetwork.Instantiate("Card/Card",transform.position, Quaternion.identity);
                var cardInfo = card.GetComponent<NormalCard>();
                _cardList.Add(cardInfo);
            }

        }

        photonView.RPC("SetupCard", RpcTarget.All);        
    }

    [PunRPC]
    private void SetupCard() {
        var cardSprites = Resources.LoadAll("Card/CardSprite",typeof(Sprite));
        foreach (var (sprite, index) in cardSprites.Select((item, index) => (item, index)))
        {
            for(int f = 0; f < 2; f++){
                var card = _cardList[index * 2 + f];
                card.transform.parent = _parentTrans;
                card.transform.localScale = Vector3.one;
                card.Id = index;
                card.UniqId = index * 2 + f;
                card.SetIllust((Sprite)sprite);
                var normal = (NormalCard)card;
                normal.OnOpenCard
                    .Subscribe(info => OpenCard(info))
                    .AddTo(this);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        // オーナーの場合
        if (stream.IsWriting)
        {
            stream.SendNext(_currentUserId.Value);
        }
        // オーナー以外の場合
        else
        {
            _currentUserId.Value = (string)stream.ReceiveNext();
        }
    }
}
