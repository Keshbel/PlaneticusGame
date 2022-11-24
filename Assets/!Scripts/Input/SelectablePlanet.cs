using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectablePlanet : NetworkBehaviour, IPointerDownHandler
{
    private CurrentPlayer Player => AllSingleton.Instance.player;
    private List<SelectablePlanet> SelectablePlanets => AllSingleton.Instance.selectablePlanets;
    
    public PlanetController planetController;
    
    public GameObject selectingObject;
    public bool isSelecting;

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
        
        if (isSelecting) AllSingleton.Instance.planetPanelController.ClosePanel();
    }

    [Client]
    private void OnEnable()
    {
        if (!selectingObject)
            selectingObject = gameObject.transform.GetChild(0).gameObject;

        if (!planetController)
            planetController = GetComponent<PlanetController>();
    }

    [Client]
    private void SelectingProcess() //процесс выделяемости/развыделяемости для других планет
    {
        if (SelectablePlanets.Count != 0)
        {
            SelectablePlanets[0].SelectingChange();
            SelectablePlanets.Clear();
        }

        SelectingChange();
        SelectablePlanets.Add(this);
    }
    
    [Client]
    private void SelectingChange() //изменение выделяемости планеты
    {
        selectingObject.SetActive(!selectingObject.activeSelf);
        isSelecting = !isSelecting;
    }

    [Client]
    public void OnPointerDown(PointerEventData eventData) //нажатие на планету
    {
        if (!isOwned || Player.selectUnits.invaderControllers.Count > 0 || Player.selectUnits.isLogisticMode
                     || planetController.PlanetResources.Count == 0 || planetController.isHomePlanet) return;

        SelectingProcess();
        planetController.OpenPlanet();
        AllSingleton.Instance.planetPanelController.OpenPanel();
    }
}
