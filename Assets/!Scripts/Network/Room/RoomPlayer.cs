using System;
using System.Threading.Tasks;
using JamesFrowen.MirrorExamples;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class RoomPlayer : NetworkRoomPlayer
{
     private RoomManager _roomManager;
     private PrefabPoolManager _roomPlayerPool;
     
     [SyncVar] public RoomPlayerUI roomPlayerUI;
     [SyncVar (hook = nameof(UpdatePlayerName))] public string playerName;
     [SyncVar] public Color playerColor;
     [SyncVar] public int indexInvaderSprite;

     private void Awake()
     {
          Init();
     }

     public void Init()
     {
          if (!_roomPlayerPool) _roomPlayerPool = FindObjectOfType<PrefabPoolManager>();
          if (!_roomManager) _roomManager = FindObjectOfType<RoomManager>();
     }

     public override async void OnStartClient()
     {
          base.OnStartClient();

          if (isLocalPlayer)
          {
               CmdSetUIRoomPlayer();
               //roomPlayerUI = FindObjectOfType<RoomPlayerUI>();
               
               CmdUpdatePlayerName(_roomManager.playerName);
               CmdUpdatePlayerColor(ColorPicker.Color == Color.clear ? Random.ColorHSV() : _roomManager.playerColor);
               CmdUpdateIndexInvaderSprite(PlayerPrefs.GetInt("indexInvaderSprite", 0));

               if (GetComponent<NetworkIdentity>().netId == 1) NetworkManager.singleton.hostPlayerName = playerName;
          }

          SetDataPlayer();
          
          await Task.Delay(250);
          
          SetDataPlayer();
     }

     public override void OnStopServer()
     {
          base.OnStopServer();

          if (_roomPlayerPool != null)
          {
               _roomPlayerPool.PutBackInPool(roomPlayerUI.gameObject);
               NetworkServer.UnSpawn(roomPlayerUI.gameObject);
          }
     }

     public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
     {
          base.ReadyStateChanged(oldReadyState, newReadyState);
          roomPlayerUI.ChangeReadyIcon(oldReadyState, newReadyState);
     }

     [Server]
     private void SetUIRoomPlayer()
     {
          var uiPlayer = _roomPlayerPool.GetFromPool(Vector3.zero,Quaternion.identity);
          NetworkServer.Spawn(uiPlayer);
          roomPlayerUI = uiPlayer.GetComponent<RoomPlayerUI>();
     }
     [Command]
     public void CmdSetUIRoomPlayer()
     {
          SetUIRoomPlayer();
     }
     
     [Client]
     public void SetDataPlayer()
     {
          if (roomPlayerUI == null || roomPlayerUI.readyButton.onClick.GetPersistentEventCount() > 0) return;
          
          roomPlayerUI.leaveButton.onClick.RemoveAllListeners();
          roomPlayerUI.readyButton.onClick.RemoveAllListeners();
          
          roomPlayerUI.leaveButton.gameObject.SetActive(false);
          roomPlayerUI.readyButton.interactable = false;

          if (isLocalPlayer)
          {
               roomPlayerUI.readyButton.interactable = true;
               roomPlayerUI.readyButton.onClick.AddListener(() => CmdChangeReadyState(!readyToBegin));
               
               roomPlayerUI.leaveButton.gameObject.SetActive(true);
               roomPlayerUI.leaveButton.onClick.AddListener(ClientDisconnect);
          }

          if (isServer || isServerOnly)
          {
               roomPlayerUI.leaveButton.onClick.RemoveAllListeners();
               roomPlayerUI.leaveButton.onClick.AddListener(HostDisconnectClient);
               roomPlayerUI.leaveButton.gameObject.SetActive(true);
          }
     }
     
     [Command]
     private void CmdUpdatePlayerColor(Color colorHSV)
     {
          playerColor = colorHSV;
     }

     [Command]
     private void CmdUpdatePlayerName(string pName)
     {
          playerName = pName;
     }
     
     [Command]
     private void CmdUpdateIndexInvaderSprite(int indexSprite)
     {
          indexInvaderSprite = indexSprite;
     }

     private void ClientDisconnect()
     {
          GetComponent<NetworkIdentity>().connectionToServer.Disconnect();
     }
     
     private void HostDisconnectClient()
     {
          if (index == 0) NetworkManager.singleton.StopHost();
          else GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
     }

     #region Hook
     
     private void UpdatePlayerName(string oldS, string newS)
     {
          if (isOwned) roomPlayerUI.CmdSetNickname(newS);
     }
     #endregion
}
