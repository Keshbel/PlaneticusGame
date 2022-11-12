using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionHUD : MonoBehaviour
{
    private NetworkDiscoveryHUD _networkHud;

    [Header("Components")] 
    public PanelController panel;
    public GameObject buttonPrefab;
    [SerializeField] private Transform contentParent;
    public List<ServerResponse> ServerResponses = new List<ServerResponse>();

    private void Awake()
    {
        if (!_networkHud) _networkHud = FindObjectOfType<NetworkDiscoveryHUD>();
    }

    private void Update()
    {
        if (!contentParent) return;
        
        foreach (ServerResponse info in _networkHud.discoveredServers.Values)
        {
            if (ServerResponses.Contains(info)) return;

            var buttonConnection = Instantiate(buttonPrefab, contentParent);
            buttonConnection.GetComponent<Button>().onClick.AddListener(() => _networkHud.Connect(info));
            buttonConnection.transform.GetChild(0).GetComponent<TMP_Text>().text = info.HostPlayerName;
            buttonConnection.transform.GetChild(1).GetComponent<TMP_Text>().text =
                info.CurrentPlayers + "/" + info.TotalPlayers;

            ServerResponses.Add(info);
        }
    }

    public void UI_StartHost()
    {
        _networkHud.discoveredServers.Clear();
        NetworkManager.singleton.StartHost();
        _networkHud.networkDiscovery.AdvertiseServer();
    }

    public void UI_FindServers()
    {
        panel.OpenPanel();
        UI_UpdateServerList();
    }

    public void UI_UpdateServerList()
    {
        _networkHud.discoveredServers.Clear();
        ServerResponses?.Clear();
        
        while(contentParent.transform.childCount>0)
        {
            DestroyImmediate(contentParent.transform.GetChild(0).gameObject);
        }

        _networkHud.networkDiscovery.StartDiscovery();
    }
}
