using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class SelectUnits : NetworkBehaviour
{
	private CurrentPlayer Player => AllSingleton.Instance.player; 
	
	[Header("Targets")]
	public List<SpaceInvaderController> invaderControllers;
	public PlanetController targetPlanet;
	public bool isLogisticMode;

	public GUISkin skin;
	private Rect _rect;
	[SerializeField] private bool draw;
	private Vector2 _startPos;
	private Vector2 _endPos;

	private void Awake()
	{
		if (!skin) skin = AllSingleton.Instance.skin;
	}

	private void Update()
    {
        if (isOwned && AllSingleton.Instance.cameraMove.isEnable)
        {
            if (Input.GetMouseButtonDown(0)) //при клике левой кнопкой
            {
                Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
                
                //при попадании по объекту с колайдером
                if (hit.collider != null)
                {
                    var invader = hit.collider.GetComponent<SpaceInvaderController>();
                    targetPlanet = hit.collider.GetComponent<PlanetController>();

                    if (isLogisticMode) //режим передачи ресурсов
                    {
                        if (targetPlanet != null && Player.playerPlanets.Contains(targetPlanet.gameObject))
                        {
                            var planetParent = AllSingleton.Instance.selectablePlanets[0].GetComponent<PlanetController>();
                            if (planetParent != targetPlanet && isClient)
                            {
                                planetParent.CmdLogisticResource
                                    (planetParent.PlanetResources[planetParent.indexCurrentResource], targetPlanet);
                            }
                        }
                        ClearDistanceInfo();
                    }
                    else
                    {
                        if (invader != null && Player.PlayerInvaders.Contains(invader)) //если клик был по захватчику, то выделяем его
                        {
                            //снимаем выделение с прошлого захватчика, если был
                            Deselect();
                        
                            //назначаем и выделяем нового захватчика
                            invaderControllers = new List<SpaceInvaderController> {invader};
                            if (isClient)
	                            foreach (var invaderController in invaderControllers)
	                            {
		                            invaderController.CmdSelecting(true);
	                            }

                            foreach (var planet in AllSingleton.Instance.mainPlanetController.listPlanet)
                            {
                                float speed = invader.speed;

                                planet.CalculateDistance(invaderControllers[Random.Range(0, invaderControllers.Count)].transform.position, speed);
                            }

                        }
                    
                        // если есть цель, выбран захватчик и дистанция не слишком маленькая, то движемся к цели
                        if (targetPlanet != null && invaderControllers.Count > 0)
                        {
                            foreach (var invaderController in invaderControllers)
                            {
                                if (Vector2.Distance(invaderController.transform.position, targetPlanet.transform.position) > 1.5)
                                    invaderController.MoveTowards(targetPlanet.gameObject);
                            }
                            ClearDistanceInfo();
                            Deselect();
                            invaderControllers.Clear();
                        }
                    }
                }
                
                //при попадании по объекту без коллайдера, снимаем выделение
                else
                {
                    ClearDistanceInfo();
                    Deselect();
                    invaderControllers.Clear();
                }
            }
        }
    }

	// проверка, добавлен объект или нет
	bool CheckUnit (SpaceInvaderController unit) 
	{
		bool result = false;
		foreach(SpaceInvaderController u in invaderControllers)
		{
			if(u == unit) result = true;
		}
		return result;
	}

	void Select()
	{
		if(invaderControllers.Count > 0)
		{
			for(int j = 0; j < invaderControllers.Count; j++)
			{
				// делаем что-либо с выделенными объектами
				if (isServer) invaderControllers[j].Selecting(true);
				else invaderControllers[j].CmdSelecting(true);
			}
		}
	}

	public void Deselect()
	{
		if(invaderControllers.Count > 0)
		{
			for(int j = 0; j < invaderControllers.Count; j++)
			{
				// отменяем то, что делали с объектоми
				if (isServer) invaderControllers[j].Selecting(false);
				else invaderControllers[j].CmdSelecting(false);
			}
		}
		
		invaderControllers.Clear();
	}
	
	public void ClearDistanceInfo()
	{
		foreach (var planet in AllSingleton.Instance.mainPlanetController.listPlanet)
		{
			planet.ClearDistanceText();
		}

		isLogisticMode = false;
	}
	
	void OnGUI ()
	{
		if (isOwned && AllSingleton.Instance.cameraMove.isEnable)
		{
			GUI.skin = skin;
			GUI.depth = 99;

			if (Input.GetMouseButtonDown(0))
			{
				_startPos = Input.mousePosition;
			}

			if (Input.GetMouseButton(0))
			{
				draw = true;
			}

			if (Input.GetMouseButtonUp(0))
			{
				draw = false;
				Select();
			}

			if (draw)
			{
				_endPos = Input.mousePosition;
				if (_startPos == _endPos) return;

				_rect = new Rect(Mathf.Min(_endPos.x, _startPos.x),
					Screen.height - Mathf.Max(_endPos.y, _startPos.y),
					Mathf.Max(_endPos.x, _startPos.x) - Mathf.Min(_endPos.x, _startPos.x),
					Mathf.Max(_endPos.y, _startPos.y) - Mathf.Min(_endPos.y, _startPos.y)
				);

				GUI.Box(_rect, "");

				var invaders = Player.PlayerInvaders;
				for (int j = 0; j < invaders.Count; j++)
				{
					if (invaders[j] == null) break;
					
					// трансформируем позицию объекта из мирового пространства, в пространство экрана
					Vector2 tmp = new Vector2(Camera.main.WorldToScreenPoint(invaders[j].transform.position).x,
						Screen.height - Camera.main.WorldToScreenPoint(invaders[j].transform.position).y);

					if (_rect.Contains(tmp)) // проверка, находится-ли текущий объект в рамке
					{
						if (invaderControllers.Count == 0)
						{
							invaderControllers.Add(invaders[j]);
						}
						else if (!CheckUnit(invaders[j]))
						{
							invaderControllers.Add(invaders[j]);
						}
					}
				}
			}
		}
		else
		{
			draw = false;
		}
	}
}
