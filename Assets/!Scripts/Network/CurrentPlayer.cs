using System.Collections.Generic;
using Mirror;

public class CurrentPlayer : NetworkBehaviour
{
    public string playerName;
    public List<PlanetController> playerPlanets;

    private void Start()
    {
        AllSingleton.instance.planetGeneration.Invoke(nameof(PlanetGeneration.HomePlanetAddingToPlayer), 1f);
    }

    void Update()
    {
        if (hasAuthority) //проверяем, есть ли у нас права изменять этот объект
        {
            
        }
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
