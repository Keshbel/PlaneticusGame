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
    public Transform FromTransform;
    public Transform ToTransform;

    [Header("Options")]
    public float speed = 0.2f;
    
    [Header("Resources")]
    public SyncList<ResourceForPlanet> SaveResources = new SyncList<ResourceForPlanet>();

    [Header("States")]
    public bool isOn = true;

    [Server]
    public IEnumerator StartRouteRoutine()
    {
        var distance = Vector2.Distance(ToTransform.position, FromTransform.position);
        while (isOn)
        {
            //инициализация
            var logisticArrow = Instantiate(ResourceSingleton.instance.logisticArrowPrefab, FromTransform); //спавн объекта локально
            NetworkServer.Spawn(logisticArrow); //всеобщий спавн (для всех клиентов)
            
            //поворот
            var fromPosition = logisticArrow.transform.position;
            var toPosition = ToTransform.position;

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

    public void StopRouteRoutine()
    {
        isOn = false;
    }
}
