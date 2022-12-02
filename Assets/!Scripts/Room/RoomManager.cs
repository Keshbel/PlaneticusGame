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
    [Header("Keshbel addition")]
    public NetworkDiscovery networkDiscovery;
    
    [Header("Transports")]
    public FizzySteamworks fizzySteamworksTransport;
    public SteamManager steamManager;
    [Space] public KcpTransport kcpTransport;
    
    [Header("PlayerPrefers")]
    public string playerName;
    public Color playerColor;

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
}
