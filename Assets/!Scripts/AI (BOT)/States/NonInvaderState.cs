using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

[CreateAssetMenu]
public class NonInvaderState : State
{
    private List<PlanetController> TargetPlanets => currentPlayer.targetSuperPlanets;
    private SyncList<PlanetController> PlayerPlanets => currentPlayer.PlayerPlanets;
    
    public List<ResourceForPlanet> listMissingResources = new List<ResourceForPlanet> { 
        new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Aether},
        new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Air},
        new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Earth},
        new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Fire},
        new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Water}};

    public override async void Init()
    {
        await Task.Delay(2000);

        if (TargetPlanets.Count == 0)
        {
            var newPlanet = PlayerPlanets.Find(planet => !planet.isSuperPlanet);
            if (newPlanet) TargetPlanets.Add(newPlanet);
        }

        if (TargetPlanets.Count == 0 || TargetPlanets[0]?.PlanetResources == null)
        {
            IsFinished = true;
            return;
        }

        foreach (var planetResource in TargetPlanets[0].PlanetResources) //преобразуем в список нехватающих ресурсов
        {
            listMissingResources.RemoveAll(resource => resource.resourcePlanet == planetResource.resourcePlanet);
        }

        //PlayerPlanets.Find(planet => planet.PlanetResources.Any(res => res.resourcePlanet == listMissingResources.))
        
        foreach (var planet in PlayerPlanets) // ищем нужные ресурсы среди своих планет для логистики
        {
            if (planet.isHomePlanet || planet.isSuperPlanet || planet == TargetPlanets[0]) continue;
            var missingResource = listMissingResources.Find(resource => planet.PlanetResources.Find(res => res.resourcePlanet == resource.resourcePlanet) != null);
            if (missingResource != null)
            {
                planet.CmdLogisticResource(missingResource, TargetPlanets[0]);
                listMissingResources.Remove(missingResource);
                IsFinished = true;
            }
        }

        if (listMissingResources.Count == 0 || TargetPlanets[0].PlanetResources.Count == 5) TargetPlanets.Clear();
        
        await Task.Delay(2000);
        IsFinished = true;
    }

    public override void Run()
    {
        if (!IsFinished) return;
        
        Destroy(this);
        return;
    }
}
