using System.Collections.Generic;
using System.Threading.Tasks;
using kcp2k;
using Mirror;
using Mirror.Discovery;
using Mirror.FizzySteam;
using Steamworks;
using UnityEngine;

public class RoomManager : NetworkRoomManager
{
    public static RoomManager Instance;
    
    [Space(10)]
    [Header("Keshbel's additions")]
    public NetworkDiscovery networkDiscovery;

    [Header("Transports")]
    public FizzySteamworks fizzySteamworksTransport;
    public SteamManager steamManager;
    [Space] public KcpTransport kcpTransport;

    [Header("Bots")] 
    public int botCount;
    //public readonly SyncList<CurrentPlayer> Bots = new SyncList<CurrentPlayer>();
    
    [Header("PlayerPrefers")]
    public string playerName;
    public Color playerColor;
    public List<Color> colorList = new List<Color>();

    public override void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        if (SteamAPI.Init() && SteamAPI.IsSteamRunning())
        {
            fizzySteamworksTransport.enabled = true;
            steamManager.enabled = true;
            transport = fizzySteamworksTransport;
            networkDiscovery.transport = fizzySteamworksTransport;
        }
        else
        {
            kcpTransport.enabled = true;
            transport = kcpTransport;
            networkDiscovery.transport = kcpTransport;
        }
        
        Transport.active = transport;
        
        base.Awake();
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        var roomPlayerComponent = roomPlayer.GetComponent<RoomPlayer>();
        if (roomPlayerComponent) colorList?.Add(roomPlayerComponent.playerColor);
        
        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        base.OnRoomServerSceneChanged(sceneName);

        botCount = 2;
        if (sceneName == GameplayScene)
        {
            for (int i = 0; i < botCount; i++)
            {
                var indexBot = i + 1;

                var botGO = Instantiate(playerPrefab);
                var botPlayer = botGO.GetComponent<CurrentPlayer>();

                botPlayer.isBot = true;
                botPlayer.playerName = "Bot " + indexBot;

                var botPlayerColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                /*while (colorList.Contains(botPlayerColor)) //исключаем взятие одинакового цвета игроков или ботов
                {
                    print("Катча! Попался одинаковый цвет!");
                    botPlayerColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                }*/

                colorList.Add(botPlayerColor);
                botPlayer.playerColor = botPlayerColor;

                var player = new NetworkConnectionToClient(100 + indexBot);
                NetworkServer.AddPlayerForConnection(player, botGO);
            }

            //Bots.Add(botPlayer.GetComponent<CurrentPlayer>());
        }
    }

}
