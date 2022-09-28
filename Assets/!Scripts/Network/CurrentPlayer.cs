using System;
using Mirror;

[Serializable]
public class CurrentPlayer : NetworkBehaviour
{
    [SyncVar]
    public string playerName;

    public SyncList<PlanetController> playerPlanets = new SyncList<PlanetController>();

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        ChangeCurrentUser();
        
        if (hasAuthority)
            AllSingleton.instance.planetGeneration.Invoke(nameof(MainPlanetController.HomePlanetAddingToPlayer), 1f);
    }
    

    [Client]
    public void ChangeCurrentUser()
    {
        if (hasAuthority)
            AllSingleton.instance.currentPlayer = this;
    }
    
    public void ChangeListWithPlanet(PlanetController planet, bool isAdding)
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
}
