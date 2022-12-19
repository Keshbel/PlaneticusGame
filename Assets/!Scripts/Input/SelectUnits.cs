using System.Collections.Generic;
using System.Linq;
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

	[Client]
	private void Awake()
	{
		if (!skin) skin = AllSingleton.Instance.skin;
	}

	[Client]
	private void Update()
	{
		if (!isOwned || !AllSingleton.Instance.cameraController.isEnable || !Input.GetMouseButtonDown(0)) return;

		Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

		//при попадании по объекту с колайдером
		if (hit.collider)
		{
			var invader = hit.collider.GetComponent<SpaceInvaderController>();
			targetPlanet = hit.collider.GetComponent<PlanetController>();

			if (isLogisticMode) //режим передачи ресурсов
			{
				if (targetPlanet != null && Player.PlayerPlanets.Contains(targetPlanet))
				{
					var donorPlanet = AllSingleton.Instance.selectablePlanets[0].GetComponent<PlanetController>();
					if (donorPlanet != targetPlanet && isClient) donorPlanet.CmdLogisticResource
						(donorPlanet.PlanetResources[donorPlanet.indexCurrentResource], targetPlanet);
				}

				ClearDistanceInfo();
			}
			else
			{
				//если клик был по одному захватчику, то выделяем его
				if (invader != null && Player.PlayerInvaders.Contains(invader))
				{
					Deselect(); //снимаем выделение с прошлого захватчика, если был
					invaderControllers = new List<SpaceInvaderController>{invader}; //назначаем нового захватчика
					Select();
				}

				if (targetPlanet == null || invaderControllers.Count <= 0) return;

				// если есть цель, выбран захватчик и дистанция не слишком маленькая, то движемся к цели
				foreach (var invaderController in invaderControllers.Where(invaderController =>
					Vector2.Distance(invaderController.transform.position, targetPlanet.transform.position) > 1.5))
				{
					invaderController.MoveTowards(targetPlanet.gameObject);
				}

				Deselect();
			}
		}
		else //при попадании по объекту без коллайдера, снимаем выделение
		{
			Deselect();
			isLogisticMode = false;
			ClearDistanceInfo();
		}
	}

	[Client]
	bool CheckUnit (SpaceInvaderController unit) // проверка, добавлен объект или нет
	{
		bool result = false;
		foreach(SpaceInvaderController u in invaderControllers)
		{
			if(u == unit) result = true;
		}
		return result;
	}

	[Client]
	void Select()
	{
		if (invaderControllers.Count == 0) return;
		
		foreach (var invader in invaderControllers) // делаем что-либо с выделенными объектами
		{
			if (!invader.isSelecting) invader.CmdSelecting(true);
		}
		
		CalculateDistanceInfo(invaderControllers[0]);
	}

	[Client]
	private void Deselect()
	{
		if (invaderControllers.Count <= 0) return;
		
		foreach (var invader in invaderControllers) // отменяем то, что делали с объектоми
		{
			invader.CmdSelecting(false);
		}
		
		ClearDistanceInfo();
		invaderControllers.Clear();
	}

	[Client]
	private void CalculateDistanceInfo(SpaceInvaderController invader)
	{
		var invaders = invaderControllers;
		var invaderRandom = invaders[Random.Range(0, invaders.Count)];
		
		foreach (var planet in MainPlanetController.Instance.listPlanet)
		{
			planet.CalculateDistance(invaderRandom.transform.position, invader.speed);
		}
	}
	
	[Client]
	private void ClearDistanceInfo()
	{
		foreach (var planet in MainPlanetController.Instance.listPlanet)
		{
			planet.ClearDistanceText();
		}

		isLogisticMode = false;
	}
	
	[Client]
	void OnGUI ()
	{
		if (isOwned && AllSingleton.Instance.cameraController.isEnable)
		{
			GUI.skin = skin;
			GUI.depth = 99;

			if (Input.GetMouseButtonDown(0)) _startPos = Input.mousePosition;

			if (Input.GetMouseButton(0)) draw = true;

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
