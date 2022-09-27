using System.Collections.Generic;
using Mirror;

public class CurrentPlayer : NetworkBehaviour
{
    [SyncVar]
    public string playerName;
    public List<PlanetController> playerPlanets;

    private void Start()
    {
        AllSingleton.instance.planetGeneration.Invoke(nameof(MainPlanetController.HomePlanetAddingToPlayer), 1f);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        ChangeCurrentUser();
    }

    void Update()
    {
        if (hasAuthority) //проверяем, есть ли у нас права изменять этот объект
        {
            
        }
    }

    [Client]
    public void ChangeCurrentUser()
    {
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
