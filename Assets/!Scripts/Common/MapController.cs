using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

public class MapController : NetworkBehaviour
{
    private CameraController CameraController => AllSingleton.Instance.cameraController;
    public List<SpriteRenderer> mapObjects;

    [ServerCallback]
    private void Awake()
    {
        var sizeBounds = 9.5f + (NetworkServer.connections.Count + RoomManager.Instance.botCount) * 9.5f;
        MainPlanetController.Instance.xBounds.Set(-sizeBounds, sizeBounds);
        MainPlanetController.Instance.yBounds.Set(-sizeBounds, sizeBounds);
    }

    [ServerCallback]
    private async void Start()
    {
        await Task.Delay(250);
        var xSize = 5.75f + (NetworkManager.singleton.numPlayers + RoomManager.Instance.botCount) * 5.75f;
        var ySize = 7.7f + (NetworkManager.singleton.numPlayers + RoomManager.Instance.botCount) * 7.7f;
        var zoomMax = 10 + (NetworkManager.singleton.numPlayers + RoomManager.Instance.botCount) * 2;
        RpcSetBorderAndZoom(xSize, ySize, zoomMax);
        
        var sizeMapObject = 9.5f + (NetworkServer.connections.Count + RoomManager.Instance.botCount) * 9.5f;
        
        RpcSetMapObject(sizeMapObject);
    }
    
    [ClientRpc]
    private void RpcSetMapObject(float size)
    {
        foreach (var mapObject in mapObjects)
        {
            mapObject.size = new Vector2(size,size) * 4;
        }
    }
    
    [ClientRpc]
    private void RpcSetBorderAndZoom(float xSize, float ySize, int zoomMax)
    {
        CameraController.xMinBorder = -xSize;
        CameraController.xMaxBorder = xSize;
        
        CameraController.yMinBorder = -ySize;
        CameraController.yMaxBorder = ySize;

        CameraController.zoomMax = zoomMax;
    }
}
