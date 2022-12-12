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
        var size = 9.5f + NetworkServer.connections.Count * 9.5f;
        MainPlanetController.Instance.xBounds.Set(-size, size);
        MainPlanetController.Instance.yBounds.Set(-size, size);
    }

    [ServerCallback]
    private async void Start()
    {
        await Task.Delay(250);
        var xSize = 5.75f + NetworkManager.singleton.numPlayers * 5.75f;
        var ySize = 7.7f + NetworkManager.singleton.numPlayers * 7.7f;
        RpcSetBorder(xSize, ySize);
        
        var size = 9.5f + NetworkServer.connections.Count * 9.5f;
        
        RpcSetMapObject(size);
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
    private void RpcSetBorder(float xSize, float ySize)
    {
        CameraController.xMinBorder = -xSize;
        CameraController.xMaxBorder = xSize;
        
        CameraController.yMinBorder = -ySize;
        CameraController.yMaxBorder = ySize;
    }
}
