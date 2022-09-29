using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[Serializable]
public class CurrentPlayer : NetworkBehaviour
{
    [SyncVar]
    public string playerName;
    
    public SyncList<GameObject> playerPlanets = new SyncList<GameObject>();

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        AddUserIdInList();

        if (hasAuthority)
        {
            if (isServer)
                Invoke(nameof(HomePlanetAddingToPlayer), 0.5f);
            else
            {
                Invoke(nameof(CmdHomePlanetAddingToPlayer), 0.5f);
            }
        }
    }
    
    
    [Command]
    public void AddUserIdInList()
    {
        if (isClient)
            AllSingleton.instance.CmdAddPlayer(GetComponent<NetworkIdentity>());
        if (isServer)
            AllSingleton.instance.AddPlayer(GetComponent<NetworkIdentity>());
    }

    [Server]
    public void ChangeListWithPlanet(GameObject planet, bool isAdding)
    {
        if (isAdding)
        {
            playerPlanets.Add(planet);
        }
        else
        {
            playerPlanets.Remove(planet);
        }
    }
    [Command]
    public void CmdChangeListWithPlanet(GameObject planet, bool isAdding)
    {
        ChangeListWithPlanet(planet, isAdding);
    }
    
    public void HomePlanetAddingToPlayer()
    {
        var listPlanet = AllSingleton.instance.planetGeneration.listPlanet;
        //домашняя планета
        if (listPlanet.Count > 0)
        {
            var homePlanet = listPlanet.Find(planet=>!planet.isHomePlanet);
            homePlanet.SetHomePlanet();

            if (isServer)
            {
                ChangeListWithPlanet(homePlanet.gameObject, true);
                homePlanet.HomingPlanetShow();
                homePlanet.ResourceIconShow();
                homePlanet.SetColonizedBoolTrue();
            }
            else
            {
                CmdChangeListWithPlanet(homePlanet.gameObject, true);
                homePlanet.CmdHomingPlanetShow();
                homePlanet.RcpResourceIconShow();
                homePlanet.CmdSetColonizedBoolTrue();
            }
            
            CameraToHome(homePlanet.transform.position);
        }
    }

    [Command]
    public void CmdHomePlanetAddingToPlayer()
    {
        HomePlanetAddingToPlayer();
    }
    
    [Client]
    public void CameraToHome(Vector3 position)
    {
        if (hasAuthority && NetworkClient.connection.identity.GetComponent<CurrentPlayer>() == this)
            AllSingleton.instance.cameraMove.DoMove(position.x, position.y, 1f);
    }
}
