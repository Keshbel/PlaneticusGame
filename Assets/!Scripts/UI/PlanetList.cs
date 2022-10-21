using Mirror;
using UnityEngine;

public class PlanetList : NetworkBehaviour
{
    public GameObject contentParent;

    public void FillingList()
    {
        //очистка
        for (int i = 0; i < contentParent.transform.childCount; i++)
        {
            var child = contentParent.transform.GetChild(i);
            Destroy(child.gameObject);
        }
        
        //заполнение
        foreach (var planetGo in AllSingleton.instance.player.playerPlanets)
        {
            var planet = planetGo.GetComponent<PlanetController>();
            if (AllSingleton.instance.planetPanelUI.planetNameText.text != planet.namePlanet)
            {
                AddFieldToList(planet);
            }
            else
            {
                continue;
            }
        }
    }
    
    public void AddFieldToList(PlanetController planet)
    {
        //инициализация
        var planetButton = Instantiate(AllSingleton.instance.planetButtonPrefab, contentParent.transform);
        var data = planetButton.GetComponent<ButtonPlanetInfo>();
        var planetParent = AllSingleton.instance.selectablePlanets[0].GetComponent<PlanetController>();

        //присвоение информации
        data.planetController = planet;
        data.namePlanetText.text = planet.namePlanet;
        
        if (isServer)
        {
            data.buttonPlanet.onClick.AddListener(() =>
                StartCoroutine(planetParent.LogisticResource(planetParent.planetResources[planetParent.indexCurrentResource], planet)));
        }
        else
        {
            data.buttonPlanet.onClick.AddListener(() =>
                planetParent.CmdLogisticResource(planetParent.planetResources[planetParent.indexCurrentResource], planet));
        }
        
        data.buttonPlanet.onClick.AddListener(() => 
            AllSingleton.instance.planetPanelUI.logisticListPanel.ClosePanel());
        data.buttonPlanet.onClick.AddListener(() => 
            planetParent.UpdateInfo(0));

    }
}
