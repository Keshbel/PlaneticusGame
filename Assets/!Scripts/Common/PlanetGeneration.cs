using System.Collections;
using System.Collections.Generic;
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

    public IEnumerator Generation() //Генерация планет, если они не были сгенерированы
    {
        var countPlanet = Random.Range(50, 70);

        while (listPlanet.Count < countPlanet)
        {
            //создание планеты
            var planet = Instantiate(planetPrefab, parentTransform);
            
            //рандомные параметры для неё
            float x = 0, y = 0;
            RandomXY(ref x, ref y);
            
            planet.transform.position = new Vector3(x, y, 0);
            var randomScale = Random.Range(0.1f, 0.2f);
            planet.transform.localScale = new Vector3(randomScale,randomScale,randomScale);
            planet.GetComponent<SpriteRenderer>().sprite = listSpritePlanet[Random.Range(0, listSpritePlanet.Count)];
            
            //добавление планеты в список
            listPlanet.Add(planet);
            yield return null;
        }

        /*List<GameObject> destroyPlanetList = new List<GameObject>();
        //расселение в другие координаты пересекающихся планет
        foreach (var planeT in listPlanet)
        {
            Bounds _bounds = planeT.GetComponent<CircleCollider2D>().;
            foreach (var planet in listPlanet)
            {
                if (_bounds.Intersects(planet.GetComponent<CircleCollider2D>().bounds))
                {
                    //listPlanet.Remove(planet);
                    destroyPlanetList.Add(planet);
                    /*float x = 0, y = 0;
                    RandomXY(ref x, ref y);
                    planet.transform.position = new Vector3(x, y, 0);
                    print("Intersect");#1#
                }
            }
        }
        
        yield return new WaitForSeconds(0.5f);

        var countDestroy = destroyPlanetList.Count;
        
        for (var index = 0; index < countDestroy; index++)
        {
            listPlanet.Remove(destroyPlanetList[index]);
            Destroy(destroyPlanetList[index]);
        }*/

        yield return null;
    }

    private void RandomXY(ref float x, ref float y)
    {
        x = Random.Range(xBounds.x, xBounds.y);
        y = Random.Range(yBounds.x, yBounds.y);
    }
}
