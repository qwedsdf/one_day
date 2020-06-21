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

public enum BattleResult {
    Win,
    Lose,
    Draw,
}

public enum State {
    WaitMatch,
    CreateField,
    Battle,
    Result,
}

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
    private Text _resultText;
    
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
    private List<string> _userIdList = new List<string>();
    private State _state;
    private bool _isInitialize = false;
    private bool _isOtherReady = false;

    private void Start(){
        Initialize();
        BindEvent();
        // PhotonServerSettingsに設定した内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
        _isInitialize = true;
        if(!PhotonNetwork.IsMasterClient) {
            photonView.RPC(nameof(SetOtherReady), RpcTarget.All,true);
        }
    }

    /// <summary>
    /// 初期処理
    /// </summary>
    private void Initialize() {
        SetUserData();
        _state = State.WaitMatch;
        _isMatching = false;
        _isInitialize = true;
    }

    [PunRPC]
    private void SetOtherReady(bool isReady){
        _isOtherReady = isReady;
    }

    private bool IsAllReady(){
        return _isOtherReady && _isInitialize;
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
        MainLoop().Forget();
    }

    private async UniTask MainLoop() {
        await OnMatch();
        await OnBattle();
        OnResult();
    }

    private void CreateField() {
        if (PhotonNetwork.IsMasterClient) {
            photonView.RPC(nameof(CreateCards), RpcTarget.All);
            CardShuffle();
            LotteryUserTurn();
            photonView.RPC(nameof(SetState), RpcTarget.All,State.Battle);
        }
    }

    private async UniTask OnMatch(){
        await UniTask.WaitWhile(() => PhotonNetwork.PlayerList.Length == 1 || !IsAllReady());
        SetState(State.CreateField);
        _isMatching = true;
        SetupPlayerInfo();
        CreateField();
    }

    private async UniTask OnBattle() {
        await UniTask.WaitWhile(() => _state != State.Battle);
        await UniTask.WaitWhile(() => _cardList.Any(card => !card.IsGot) || _state == State.CreateField);
    }

    [PunRPC]
    private void SetState(State state) {
        _state = state;
    }

    private void OnResult() {
        _state = State.Result;
        var result = GetResult();
        ResultProcess(result);
    }

    private void ResultProcess(BattleResult result){
        if(result == BattleResult.Win) {
            _resultText.text = "You Win";
            return;
        }

        if(result == BattleResult.Lose) {
            _resultText.text = "You Lose";
            return;
        }

        _resultText.text = "Drow";

    }

    private BattleResult GetResult() {
        var player = _playerInfo.Point;
        var enemy = _enemyInfo.Point;

        if(player > enemy) {
            return BattleResult.Win;
        }

        if(enemy > player) {
            return BattleResult.Lose;
        }

        return BattleResult.Draw;
    }

    private void SetupPlayerInfo(){
        foreach (var (player, index) in PhotonNetwork.PlayerList.Select((player, index) => (player, index)))
        {
            if (PhotonNetwork.IsMasterClient) {
                photonView.RPC(nameof(SetUserIdList), RpcTarget.All,player.UserId);
            }

            if(player.IsLocal){
                _playerInfo.SetUserId(player.UserId);
                _playerInfo.SetPoint(0);
                continue;
            }
            _enemyInfo.SetName(player.NickName);
            _enemyInfo.SetUserId(player.UserId);
            _enemyInfo.SetPoint(0);
        }

        _userList = new List<UserInfoPresenter>() {
            _playerInfo,
            _enemyInfo,
        };
    }

    private void BindEvent() {
        _currentUserId
            .Where(_ => _isMatching)
            .Subscribe(SetUserTurn)
            .AddTo(this);

        GameDealer.Instance.OnSelectCard.Subscribe(uniqId => photonView.RPC(nameof(OpenCardRPC), RpcTarget.All,uniqId));
    }

    private void LotteryUserTurn() {
        _currentUserIndex = Random.Range(0,_userList.Count);
        photonView.RPC(nameof(SetCurrentUserIndex), RpcTarget.Others,_currentUserIndex);
        photonView.RPC(nameof(SetCurrentUserId), RpcTarget.All,_userIdList[_currentUserIndex]);
    }

    [PunRPC]
    private void SetUserIdList(string id){
        _userIdList.Add(id);
    }

    [PunRPC]
    private void SetCurrentUserId(string id) {
        _currentUserId.Value = id;
    }

    [PunRPC]
    private void SetCurrentUserIndex(int index) {
        _currentUserIndex = index;
    }

    private void SetUserTurn(string userId) {
        Debug.Log("ターン切り替え");
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
        PhotonNetwork.NickName = RunTimeData.PlayerData.Name;
        _playerInfo.SetName(RunTimeData.PlayerData.Name);
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
        await UniTask.Delay(700);
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
        var id = _currentUserId.Value;
        _currentUserIndex++;
        _currentUserIndex %= _userIdList.Count;
        _currentUserId.Value = _userIdList[_currentUserIndex];
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
                var card = Instantiate(_cardPrefab,transform.position, Quaternion.identity);
                card.transform.parent = _parentTrans;
                card.transform.localScale = Vector3.one;
                var cardInfo = card.GetComponent<NormalCard>();
                _cardList.Add(cardInfo);
                cardInfo.Id = index;
                cardInfo.UniqId = index * 2 + f;
                cardInfo.SetPoint(index);
                cardInfo.SetIllust((Sprite)sprite);
                cardInfo.OnOpenCard
                    .Subscribe(info => OpenCard(info))
                    .AddTo(this);
            }

        }
    }

    private void CardShuffle(){
        for(int i = 0; i < 100; i++){
            foreach(var (card, index) in _cardList.Select((card,index) => (card,index))) {
                int sibIndex = Random.Range(0,_cardList.Count);

                object[] param = new object[]{
                    index,
                    sibIndex,
                };

                photonView.RPC(nameof(SetSiblingIndex), RpcTarget.All,param);
            }
        }
    }

    [PunRPC]
    private void SetSiblingIndex(int index,int sibIndex){
        _cardList[index].transform.SetSiblingIndex(sibIndex);
    }

    [PunRPC]
    private void OpenCardRPC(int uniqId){
        var selectCard = _cardList.FirstOrDefault(card => card.UniqId == uniqId);
        NormalCard normal = selectCard as NormalCard;
        normal.OnNext();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){

    }
}
