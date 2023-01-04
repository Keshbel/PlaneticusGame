using System.Collections.Generic;
using Mirror;
using UnityEngine;

public static class Utils
{
    public static PlanetController FindClosestNonPlayerPlanet(Transform nativeTransform, List<PlanetController> listPlanets)
    {
        PlanetController closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = nativeTransform.position;
        foreach (PlanetController planet in listPlanets)
        {
            if (planet.isColonized) continue;
            Vector3 diff = planet.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = planet;
                distance = curDistance;
            }
        }

        if (closest != null) return closest;
        return null;
    }
    
    public static PlanetController FindClosestEnemyPlayerPlanet(Transform nativeTransform, List<PlanetController> listPlanets, CurrentPlayer currentPlayer)
    {
        PlanetController closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = nativeTransform.position;
        foreach (PlanetController planet in listPlanets)
        {
            if (!planet.isColonized || currentPlayer.PlayerPlanets.Contains(planet) || planet == null) continue;
            Vector3 diff = planet.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = planet;
                distance = curDistance;
            }
        }

        if (closest != null) return closest;
        return null;
    }

    public static void Disconnect()
    {
        NetworkManager.singleton.StopClient();
    }
    
    public static void ExitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
}
