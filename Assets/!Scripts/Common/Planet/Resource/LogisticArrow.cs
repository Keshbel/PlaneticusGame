using System;
using DG.Tweening;
using Mirror;
using UnityEngine;

public class LogisticArrow : NetworkBehaviour
{
    public Collider2D thisCollider;

    [Client]
    private void Start()
    {
        if (!thisCollider)
            thisCollider = GetComponent<Collider2D>();

        /*if (isClient)
            if (transform.rotation.z == 0)
            {
                Invoke(nameof(CmdRotateTo), 0.5f);
                Invoke(nameof(CmdRotateTo), 2.5f);
            }*/
    }

    /*private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("SpaceInvader"))
        {
            var spaceInvaderController = other.gameObject.GetComponent<SpaceInvaderController>();
            
            if (spaceInvaderController.hasAuthority)
                Physics2D.IgnoreCollision(other.collider, thisCollider);
            else
            {
                if (isClient) other.gameObject.GetComponent<SpaceInvaderController>().CmdStopMoving();
            }
        }
        
    }*/

    /*public void OnPointerDown(PointerEventData eventData) //нажатие на стрелочки
    {
        if (AllSingleton.instance.player.playerPlanets.Contains(route.fromTransform.gameObject))
        {
            AllSingleton.instance.logisticRouteUI.panel.OpenPanel();
            AllSingleton.instance.logisticRouteController.OpenRoute(route);
        }
    }*/
    
    [Client]
    private void SetStartPosition(Transform fromTransform)
    {
        transform.position = fromTransform.position;
    }
    
    [Client] // движение в сторону
    private void MoveTo(Transform toTransform)
    {
        var toPosition = toTransform.position;
        var distance = Vector2.Distance(transform.position, toPosition);
        
        transform.DOMove(toPosition, distance / AllSingleton.Instance.speed).SetEase(Ease.Linear).OnComplete(()=> CmdUnSpawn());
    }

    [Client] //поворот в сторону + движение
    private void RotateTo(Transform toTransform)
    {
        var fromPosition = transform.position;
        var toPosition = toTransform.position;
        var an = Math.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x);
        var degAn = an * 180 / Math.PI;

        transform.DORotate(new Vector3(0, 0, (float) degAn), 0.2f, RotateMode.LocalAxisAdd).OnComplete(()=>MoveTo(toTransform));
    }

    [TargetRpc]
    public void TargetStartMove(Transform fromTransform, Transform toTransform)
    {
        SetStartPosition(fromTransform);
        RotateTo(toTransform);
    }

    [Command]
    private void CmdUnSpawn()
    {
        NetworkServer.UnSpawn(gameObject);
        ResourceSingleton.Instance.arrowPoolManager.PutBackInPool(gameObject);
    }
}
