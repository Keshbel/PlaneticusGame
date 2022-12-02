using System;
using JamesFrowen.MirrorExamples;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[Serializable]
public class RoomPlayer : NetworkRoomPlayer
{
     private RoomManager _roomManager;
     private PrefabPoolManager _roomPlayerPool;
     
     [SyncVar] public RoomPlayerUI roomPlayerUI;
     [SyncVar] public string playerName;
     [SyncVar] public Color playerColor;

     private void Awake()
     {
          Init();
     }

     public void Init()
     {
          if (!_roomPlayerPool) _roomPlayerPool = FindObjectOfType<PrefabPoolManager>();
          if (!_roomManager) _roomManager = FindObjectOfType<RoomManager>();
     }

     public override void OnStartClient()
     {
          base.OnStartClient();

          if (isLocalPlayer)
          {
               CmdUpdatePlayerName(_roomManager.playerName);
               CmdUpdatePlayerColor(ColorPicker.Color == Color.clear ? Random.ColorHSV() : _roomManager.playerColor);

               Invoke(nameof(CmdSetUIRoomPlayer), 0.01f);
               roomPlayerUI = FindObjectOfType<RoomPlayerUI>();
               
               if (GetComponent<NetworkIdentity>().netId == 1) NetworkManager.singleton.hostPlayerName = playerName;
          }
          
          Invoke(nameof(SetDataPlayer), 0.16f);
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
          roomPlayerUI.leaveButton.gameObject.SetActive(false);
          roomPlayerUI.readyButton.interactable = false;

          if (isLocalPlayer)
          {
               roomPlayerUI.CmdSetNickname(playerName);
               
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

     private void ClientDisconnect()
     {
          GetComponent<NetworkIdentity>().connectionToServer.Disconnect();
     }
     
     private void HostDisconnectClient()
     {
          if (index == 0) NetworkManager.singleton.StopHost();
          else GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
     }
}
