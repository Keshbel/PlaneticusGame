using UnityEngine;

public class PlanetList : MonoBehaviour
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

        //присвоение информации
        data.planetController = planet;
        data.namePlanetText.text = planet.namePlanet;
        //data.buttonPlanet.onClick.AddListener();
    }
}
