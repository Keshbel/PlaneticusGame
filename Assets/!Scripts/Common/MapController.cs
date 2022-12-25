using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MapController : NetworkBehaviour
{
    private CameraController CameraController => AllSingleton.Instance.cameraController;
    public List<SpriteRenderer> mapObjects;

    [SyncVar] public float xSize;
    [SyncVar] public float ySize;
    [SyncVar] public float mapObjectSize;
    [SyncVar] public int zoomMax;

    [ServerCallback]
    private void Awake()
    {
        var sizeBounds = 9.5f + (NetworkServer.connections.Count + RoomSettings.Instance.botCount) * 9.5f;
        MainPlanetController.Instance.xBounds.Set(-sizeBounds, sizeBounds);
        MainPlanetController.Instance.yBounds.Set(-sizeBounds, sizeBounds);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        xSize = 5.75f + (NetworkManager.singleton.numPlayers + RoomSettings.Instance.botCount) * 5.75f;
        ySize = 7.7f + (NetworkManager.singleton.numPlayers + RoomSettings.Instance.botCount) * 7.7f;
        mapObjectSize = 9.5f + (NetworkServer.connections.Count + RoomSettings.Instance.botCount) * 9.5f;
        zoomMax = 10 + (NetworkManager.singleton.numPlayers + RoomSettings.Instance.botCount) * 2;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        SetBorderAndZoom(xSize, ySize, zoomMax);
        SetMapObject(mapObjectSize);
    }

    [Client]
    private void SetMapObject(float size)
    {
        foreach (var mapObject in mapObjects)
        {
            mapObject.size = new Vector2(size,size) * 4;
        }
    }
    
    [Client]
    private void SetBorderAndZoom(float xSize, float ySize, int zoomMax)
    {
        CameraController.xMinBorder = -xSize;
        CameraController.xMaxBorder = xSize;
        
        CameraController.yMinBorder = -ySize;
        CameraController.yMaxBorder = ySize;

        CameraController.zoomMax = zoomMax;
    }
}
