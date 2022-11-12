using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectablePlanet : NetworkBehaviour, IPointerDownHandler
{
    public PlanetController planetController;
    
    public GameObject selectingObject;
    public bool isSelecting;

    private void OnEnable()
    {
        if (!selectingObject)
            selectingObject = gameObject.transform.GetChild(0).gameObject;

        if (!planetController)
            planetController = GetComponent<PlanetController>();
    }

    private void SelectingProcess() //процесс выделяемости/развыделяемости для других планет
    {
        if (AllSingleton.Instance.selectablePlanets.Count != 0)
        {
            AllSingleton.Instance.selectablePlanets[0].SelectingChange();
            AllSingleton.Instance.selectablePlanets.Clear();
        }

        SelectingChange();
        
        AllSingleton.Instance.selectablePlanets.Add(this);
    }
    
    private void SelectingChange() //изменение выделяемости планеты
    {
        selectingObject.SetActive(!selectingObject.activeSelf);
        isSelecting = !isSelecting;
    }

    public void OnPointerDown(PointerEventData eventData) //нажатие на планету
    {
        if (!AllSingleton.Instance.player.playerPlanets.Contains(planetController.gameObject) 
            || AllSingleton.Instance.player.selectUnits.invaderControllers.Count > 0
            || AllSingleton.Instance.player.selectUnits.isLogisticMode
            || planetController.PlanetResources.Count == 0) return;

        SelectingProcess();
        planetController.OpenPlanet();
        AllSingleton.Instance.planetPanelController.OpenPanel();
    }
}
