using System.Linq;
using System.Net;
using System.Net.Sockets;
using Lean.Localization;
using Mirror;
using TMPro;
using UnityEngine;

public class HostIpAdress : MonoBehaviour
{
    private NetworkManager _networkManager;
    public TMP_Text textIpAddress;

    private void Awake()
    {
        if (!_networkManager) _networkManager = FindObjectOfType<NetworkManager>();
        SetTextIpAddress();
    }

    private void SetTextIpAddress()
    {
        var ipText = LeanLocalization.GetFirstCurrentLanguage() == "Russian" ? "IP-адрес " : "IP address ";
        textIpAddress.text = ipText + GetLocalIPv4Address();
    }

    private string GetLocalIPv4Address()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
        
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}
