using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

[Serializable]
public class LogisticRoute : NetworkBehaviour
{
    [Header("Transforms")]
    public Transform fromTransform;
    public Transform toTransform;

    [Header("Options")]
    public float speed = 0.2f;
    
    [Header("Resources")]
    public SyncList<ResourceForPlanet> SaveResources = new SyncList<ResourceForPlanet>();

    [Header("States")]
    [SyncVar] public bool isOn = true;

    [Server]
    public IEnumerator StartRouteRoutine()
    {
        var distance = Vector2.Distance(toTransform.position, fromTransform.position);
        while (isOn)
        {
            //инициализация
            var logisticArrow = Instantiate(ResourceSingleton.instance.logisticArrowPrefab, transform); //спавн объекта локально
            NetworkServer.Spawn(logisticArrow); //всеобщий спавн (для всех клиентов)
            logisticArrow.GetComponent<LogisticRouteInfo>().route = this;
            
            //поворот
            var fromPosition = logisticArrow.transform.position;
            var toPosition = toTransform.position;

            var an = Math.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x);
            var deg_an = an * 180 / Math.PI;
            
            logisticArrow.transform.Rotate(0, 0, (float)deg_an, Space.Self);

            //движение
            logisticArrow.transform.DOMove(toPosition, distance / speed).
                OnComplete(()=>Destroy(logisticArrow)).SetEase(Ease.Linear); // движение в сторону цели

            //задержка
            yield return new WaitForSeconds(1f);
        }
    }
    [Command]
    public void CmdStartRouteRoutine()
    {
        StartCoroutine(StartRouteRoutine());
    }

    [Server]
    public void StopRouteRoutine()
    {
        isOn = false;
    }
    [Command]
    public void CmdStopRouteRoutine()
    {
        StopRouteRoutine();
    }

    [Server]
    public IEnumerator ReturnResource(ResourceForPlanet resourceRoute)
    {
        if (!resourceRoute.isLogistic) yield break;

        //init
        var toPlanet = toTransform.GetComponent<PlanetController>();
        var fromPlanet = fromTransform.GetComponent<PlanetController>();
        var toResource = toPlanet.PlanetResources.Find(r => r.resourcePlanet == resourceRoute.resourcePlanet);
        var fromResource = fromPlanet.PlanetResources.Find(r => r.resourcePlanet == resourceRoute.resourcePlanet);

        var copyResource = new ResourceForPlanet
            {resourcePlanet = resourceRoute.resourcePlanet, isLogistic = false, resourceMining = 1};

        //убираем ресурс на планете получателе
        if (toResource.resourceAll == 1)
            toPlanet.ChangeResourceList(toResource, false);
        else
        {
            toResource.resourceDelivery--;
            if (toResource.resourceDelivery == 0) toResource.isLogistic = false;
        }

        //убираем значение ресурса в маршруте
        resourceRoute.resourceDelivery--;

        if (resourceRoute.resourceDelivery == 0)
            SaveResources?.Remove(resourceRoute);

        //если больше нечего менять, то закрываем панель
        if (SaveResources?.Count == 0)
        {
            StopRouteRoutine();
            AllSingleton.instance.logisticRouteUI.panel.ClosePanel();
        }

        yield return new WaitForSeconds(Vector2.Distance(fromTransform.position, toTransform.position) / speed);

        fromResource = fromPlanet.PlanetResources.Find(r => r.resourcePlanet == resourceRoute.resourcePlanet);
        
        //добавляем обратно на планету отправитель
        if (fromResource != null)
        {
            fromResource.resourceMining++;
        }
        else
        {
            fromPlanet.ChangeResourceList(copyResource, true);
        }

        //уничтожаем маршрут, если больше нет передаваемых ресурсов
        if (SaveResources?.Count == 0)
        {
            fromPlanet.LogisticRoutes.Remove(this);
            Destroy(gameObject);
        }
    }
    [Command]
    public void CmdReturnResource(ResourceForPlanet resourceForPlanet)
    {
        StartCoroutine(ReturnResource(resourceForPlanet));
    }

    
    [Server]
    public void ReturnAllResource(LogisticRoute route)
    {
        foreach (var resource in route.SaveResources)
        {
            for (int i = 0; i < resource.resourceDelivery; i++)
            {
                StartCoroutine(ReturnResource(resource));
            }
        }
    }
    [Command]
    public void CmdReturnAllResource(LogisticRoute route)
    {
        ReturnAllResource(route);
    }
}
