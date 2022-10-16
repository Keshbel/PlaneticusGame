using System.Collections.Generic;
using System.Linq;
using Mirror;
using RandomNameGen;
using UnityEngine;
using Random = UnityEngine.Random;

public class MainPlanetController : NetworkBehaviour
{
    //Планеты
    public Transform parentTransform;
    public List<Sprite> listSpritePlanet;
    
    //public SyncList<PlanetController> syncListPlanet = new SyncList<PlanetController>();
    [SyncVar]
    [SerializeField]
    public List<PlanetController> listPlanet;
    
    //Границы видимости камеры
    public Vector2 xBounds;
    public Vector2 yBounds;

    public override void OnStartServer()
    {
        base.OnStartServer();

        Generation();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        Invoke(nameof(FillingListPlanet), 0.2f);
    }

    /*public override void OnStartClient()
    {
        base.OnStartClient();

        syncListPlanet.Callback += SyncListPlanet; //вместо hook, для SyncList используем подписку на Callback

        listPlanet = new List<GameObject>(syncListPlanet.Count); //так как Callback действует только на изменение массива,  
        for (int i = 0; i < syncListPlanet.Count; i++) //а у нас на момент подключения уже могут быть какие-то данные в массиве, нам нужно эти данные внести в локальный массив
        {
            SyncListPlanet(SyncList<PlanetController>.Operation.OP_ADD, i, planetPrefab.AddComponent<PlanetController>(), syncListPlanet[i]);
            listPlanet.Add(syncListPlanet[i].gameObject);
        }
    }

    [Server]
    void ChangeListPlanet(PlanetController newGO) //метод добавление в лист на сервере
    {
        syncListPlanet.Add(newGO);
        NetworkServer.Spawn(newGO.gameObject);
    }
    
    [Command]
    void CmdChangeListPlanet(PlanetController newGO) //метод добавление в лист запрос на сервер
    {
        ChangeListPlanet(newGO);
    }
    
    private void SyncListPlanet(SyncList<PlanetController>.Operation op, int index, PlanetController oldItem, PlanetController newItem) //обработчик ля синхронизации планет
    {
        switch (op)
        {
            case SyncList<PlanetController>.Operation.OP_ADD:
            {
                listPlanet.Add(newItem.gameObject);
                break;
            }
            case SyncList<PlanetController>.Operation.OP_CLEAR:
            {

                break;
            }
            case SyncList<PlanetController>.Operation.OP_INSERT:
            {

                break;
            }
            case SyncList<PlanetController>.Operation.OP_REMOVEAT:
            {

                break;
            }
            case SyncList<PlanetController>.Operation.OP_SET:
            {

                break;
            }
        }
    }*/
    
    public void Generation() //Генерация планет, если они не были сгенерированы
    {
        var countPlanet = Random.Range(50, 70);

        RandomName nameGen = new RandomName(); // create a new instance of the RandomName class
        List<string> allRandomNames = nameGen.RandomNames(countPlanet, 0); // generate 100 random names with up to two middle names

        while (listPlanet.Count < countPlanet)
        {
            //создание планеты
            var planet = Instantiate(AllSingleton.instance.planetPrefab, parentTransform);
            NetworkServer.Spawn(planet);
            var planetController = planet.GetComponent<PlanetController>();

            //рандомные параметры для неё
            planetController.indSpritePlanet = Random.Range(0, listSpritePlanet.Count); //присвоение номера вида планеты
            planetController.namePlanet = allRandomNames[listPlanet.Count]; //имя
            planetController.textName.text = planetController.namePlanet;
            planetController.AddResourceForPlanetGeneration(); //ресурсы
            SetRandomPosition(planet);
            var randomScale = Random.Range(0.1f, 0.2f); // случайный размер
            
            planet.transform.localScale = new Vector3(randomScale, randomScale, randomScale); //присвоение размера
            
            listPlanet.Add(planetController);
            
        }
    }

    public void FillingListPlanet()
    {
        var planets = FindObjectsOfType<PlanetController>();
        
        listPlanet = planets.Reverse().ToList();
    }

    [Server]
    public void SetRandomPosition(GameObject planet)
    {
        //рандомные параметры
        float x = 0, y = 0;
        RandomXY(ref x, ref y);
        
        //присвоение рандомной позиции
        planet.transform.position = new Vector3(x, y, 0); //позиция
    }
    [Command]
    public void CmdSetRandomPosition(GameObject planet)
    {
        SetRandomPosition(planet);
    }
    
    public void RandomXY(ref float x, ref float y)
    {
        x = Random.Range(xBounds.x, xBounds.y);
        y = Random.Range(yBounds.x, yBounds.y);
    }
}
