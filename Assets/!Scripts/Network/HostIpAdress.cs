using Mirror;
using TMPro;
using UnityEngine;

public class HostIpAdress : MonoBehaviour
{
    private NetworkManager _networkManager;
    public TMP_Text textIpAddress;

    private void Awake()
    {
        if (!_networkManager) FindObjectOfType<NetworkManager>();
        SetTextIpAddress();
    }

    private void SetTextIpAddress()
    {
        textIpAddress.text = "IP address = " + _networkManager.networkAddress;
    }
}
