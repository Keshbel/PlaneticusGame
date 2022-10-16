using System;
using System.Collections;
using System.Numerics;
using DG.Tweening;
using Mirror;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class LogisticRoute : NetworkBehaviour
{
    public Transform FromTransform;
    public Transform ToTransform;

    public bool IsOn = true;
    public bool IsProductTransfered;

    [Server]
    public IEnumerator StartRouteRoutine()
    {
        var distance = Vector2.Distance(ToTransform.position, FromTransform.position);
        while (IsOn)
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
            logisticArrow.transform.DOMove(toPosition, distance*10).OnComplete(()=> // движение в сторону цели
            {
                IsProductTransfered = true;
                Destroy(logisticArrow);
            }); 
            
            //задержка
            yield return new WaitForSeconds(0.5f);
        }
    }

    [Command]
    public void CmdStartRouteRoutine()
    {
        StartCoroutine(StartRouteRoutine());
    }

    public void StopRouteRoutine()
    {
        IsOn = false;
    }
}
