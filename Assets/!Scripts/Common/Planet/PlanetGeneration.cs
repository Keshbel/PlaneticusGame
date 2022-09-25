using System.Collections;
using System.Collections.Generic;
using Mirror;
using RandomNameGen;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlanetGeneration : MonoBehaviour
{
    //Планеты
    public Transform parentTransform;
    public GameObject planetPrefab;
    public List<Sprite> listSpritePlanet;
    public List<GameObject> listPlanet;
    
    //Границы видимости камеры
    public Vector2 xBounds;
    public Vector2 yBounds;

    private void Start()
    {
        StartCoroutine(Generation());
    }

    [Server]
    public IEnumerator Generation() //Генерация планет, если они не были сгенерированы
    {
        yield return new WaitForSeconds(0.5f);
        AllSingleton.instance.currentPlayer = FindObjectOfType<CurrentPlayer>().GetComponent<CurrentPlayer>();
        var countPlanet = Random.Range(50, 70);
        //Random rand = new Random.Range(0, DateTime.Now.Second); // we need a random variable to select names randomly

        RandomName nameGen = new RandomName(); // create a new instance of the RandomName class
        List<string> allRandomNames = nameGen.RandomNames(countPlanet, 0); // generate 100 random names with up to two middle names

        while (listPlanet.Count < countPlanet)
        {
            //создание планеты
            var planet = Instantiate(planetPrefab, parentTransform);

            var planetController = planet.GetComponent<PlanetController>();

            planetController.namePlanet = allRandomNames[listPlanet.Count];
            planetController.AddResourcesForPlanet();

            //рандомные параметры для неё
            float x = 0, y = 0;
            RandomXY(ref x, ref y);

            planet.transform.position = new Vector3(x, y, 0);
            var randomScale = Random.Range(0.1f, 0.2f);
            planet.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            planet.GetComponent<SpriteRenderer>().sprite = listSpritePlanet[Random.Range(0, listSpritePlanet.Count)];

            //добавление планеты в список
            listPlanet.Add(planet);
            yield return null;
        }

        yield return null;
    }

    public void HomePlanetAddingToPlayer()
    {
        //домашняя планета
        var homePlanet = listPlanet[Random.Range(0, listPlanet.Count)].GetComponent<PlanetController>();
        homePlanet.SetHomePlanet();
        AllSingleton.instance.currentPlayer.playerPlanets.Add(homePlanet);
        homePlanet.isHomePlanet = true;
        homePlanet.HomingPlanetShow();
        AllSingleton.instance.cameraMove.DoMove(homePlanet.transform.position.x, homePlanet.transform.position.y, 1f);
        homePlanet.isColonized = true;
        homePlanet.ResourceIconShow();
    }

    private void RandomXY(ref float x, ref float y)
    {
        x = Random.Range(xBounds.x, xBounds.y);
        y = Random.Range(yBounds.x, yBounds.y);
    }
}
