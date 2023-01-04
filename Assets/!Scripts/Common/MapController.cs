using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MapController : NetworkBehaviour
{
    private CameraController CameraController => AllSingleton.Instance.cameraController;
    public List<SpriteRenderer> mapObjects;
    
    [SyncVar] public float mapObjectSize;
    [SyncVar] public int zoomMax;

    [ServerCallback]
    private void Awake()
    {
        mapObjectSize = 9.5f + (NetworkServer.connections.Count + RoomSettings.Instance.botCount) * 9.5f;
        MainPlanetController.Instance.xBounds.Set(-mapObjectSize, mapObjectSize);
        MainPlanetController.Instance.yBounds.Set(-mapObjectSize, mapObjectSize);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        var playerCount = (NetworkManager.singleton.numPlayers + RoomSettings.Instance.botCount);

        zoomMax = 10 + playerCount * 1;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        SetZoom(zoomMax);
        SetMapObject(mapObjectSize);
    }

    [Client]
    private void SetMapObject(float size)
    {
        foreach (var mapObject in mapObjects)
        {
            mapObject.size = new Vector2(size,size) * 5;
        }
        
        mapObjects[0].size = new Vector2(size, size) * 2.01f;
    }
    
    [Client]
    private void SetZoom(int zoomValue)
    {
        CameraController.zoomOutMax = zoomValue;
    }
}
