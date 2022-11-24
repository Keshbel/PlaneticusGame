using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomPlayer : NetworkBehaviour
{
     public NetworkRoomPlayer networkRoomPlayer;
     [SyncVar] public string playerName;
     [SyncVar] public Color playerColor;
     
     public override void OnStartClient()
     {
          base.OnStartClient();
          
          if (!isLocalPlayer) return;
          
          if (!networkRoomPlayer)
               networkRoomPlayer = GetComponent<NetworkRoomPlayer>();

          CmdUpdatePlayerName(PlayerNameChanger.PlayerName);

          CmdUpdatePlayerColor(ColorPicker.Color == Color.clear
               ? Random.ColorHSV()
               : ColorPicker.Color);
          
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
