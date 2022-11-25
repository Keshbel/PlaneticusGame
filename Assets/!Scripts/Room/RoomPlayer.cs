using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomPlayer : NetworkBehaviour
{
     private RoomManager _roomManager;
     
     public NetworkRoomPlayer networkRoomPlayer;
     [SyncVar] public string playerName;
     [SyncVar] public Color playerColor;

     private void Awake()
     {
          if (!_roomManager) _roomManager = FindObjectOfType<RoomManager>();
     }

     public override void OnStartClient()
     {
          base.OnStartClient();
          
          if (!isLocalPlayer) return;
          
          if (!networkRoomPlayer)
               networkRoomPlayer = GetComponent<NetworkRoomPlayer>();

          CmdUpdatePlayerName(_roomManager.playerName);

          CmdUpdatePlayerColor(ColorPicker.Color == Color.clear
               ? Random.ColorHSV()
               : _roomManager.playerColor);
          
          if (GetComponent<NetworkIdentity>().netId == 1) NetworkManager.singleton.hostPlayerName = playerName;
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
}
