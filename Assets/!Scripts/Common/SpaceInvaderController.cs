using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using UnityEngine;

public class SpaceInvaderController : NetworkBehaviour
{
    public bool isSelecting;
    [SyncVar]
    public float speed;
    
    public void Move(GameObject target)
    {
        if (!isSelecting) return;
        var targetPos = target.transform.position;
        var distance = Vector2.Distance(targetPos, transform.position);
        transform.DOMove(target.transform.position, distance/speed);
    }

    public void CmdMove(GameObject target)
    {
        Move(target);
    }

    public void Attack(GameObject target)
    {
        
        //очистка и удаление объекта
        var networkIdentity = NetworkClient.connection.identity;
        var player = networkIdentity.GetComponent<CurrentPlayer>();
        player.ChangeListWithInvaders(gameObject, false);
        Destroy(gameObject);
    }
}
