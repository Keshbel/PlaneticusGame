using System;
using Mirror;
using UnityEngine;

[Serializable]
public class CurrentPlayer : NetworkBehaviour
{
    [SyncVar]
    public string playerName;
    
    public SyncList<GameObject> playerPlanets = new SyncList<GameObject>();
    public SyncList<GameObject> playerInvaders = new SyncList<GameObject>();

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
                Invoke(nameof(CameraToHome), 0.6f);
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
    public void ChangeListWithPlanets(GameObject planet, bool isAdding)
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
    public void CmdChangeListWithPlanets(GameObject planet, bool isAdding)
    {
        ChangeListWithPlanets(planet, isAdding);
    }
    
    
    [Server]
    public void ChangeListWithInvaders(GameObject invader, bool isAdding)
    {
        if (isAdding)
        {
            playerPlanets.Add(invader);
        }
        else
        {
            playerPlanets.Remove(invader);
        }
    }
    [Command]
    public void CmdChangeListWithInvaders(GameObject invader, bool isAdding)
    {
        ChangeListWithPlanets(invader, isAdding);
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
                ChangeListWithPlanets(homePlanet.gameObject, true);
                homePlanet.HomingPlanetShow();
                homePlanet.ResourceIconShow();
                homePlanet.SetColonizedBoolTrue();
            }
            else
            {
                CmdChangeListWithPlanets(homePlanet.gameObject, true);
                homePlanet.CmdHomingPlanetShow();
                homePlanet.RcpResourceIconShow();
                homePlanet.CmdSetColonizedBoolTrue();
            }
            
            CameraToHome();
        }
    }

    [Command]
    public void CmdHomePlanetAddingToPlayer()
    {
        HomePlanetAddingToPlayer();
    }
    
    [Client]
    public void CameraToHome()
    {
        var position = playerPlanets[0].transform.position;
        if (hasAuthority && NetworkClient.connection.identity.GetComponent<CurrentPlayer>() == this)
            AllSingleton.instance.cameraMove.DoMove(position.x, position.y, 1f);
    }
}
