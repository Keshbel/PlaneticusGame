using System;
using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class LogisticArrow : NetworkBehaviour, IMovable, IRotateable, IPointerDownHandler
{
    [SyncVar] public LogisticRoute route;
    public Collider2D thisCollider;

    private void Start()
    {
        if (!thisCollider)
            thisCollider = GetComponent<Collider2D>();

        if (isClient)
            if (transform.rotation.z == 0)
            {
                Invoke(nameof(CmdRotateTo), 0.5f);
                Invoke(nameof(CmdRotateTo), 2.5f);
            }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("SpaceInvader"))
        {
            if (!route) return;
            
            if (AllSingleton.instance.player.playerPlanets.Contains(route.fromTransform.gameObject))
                Physics2D.IgnoreCollision(other.collider, thisCollider);
            else
            {
                if (isClient)
                    other.gameObject.GetComponent<SpaceInvaderController>().CmdStopMoving();
            }
        }
        
    }

    public void OnPointerDown(PointerEventData eventData) //нажатие на планету
    {
        if (AllSingleton.instance.player.playerPlanets.Contains(route.fromTransform.gameObject))
        {
            AllSingleton.instance.logisticRouteUI.panel.OpenPanel();
            AllSingleton.instance.logisticRouteController.OpenRoute(route);
        }
    }

    [Server]
    public void SetStartPosition()
    {
        transform.position = route.fromTransform.position;
    }
    [Command]
    public void CmdSetStartPosition()
    {
        SetStartPosition();
    }
    
    // движение в сторону
    [Server]
    public void MoveTo()
    {
        var toPosition = route.toTransform.position;
        var distance = Vector2.Distance(transform.position, toPosition);
        
        transform.DOMove(toPosition, distance / AllSingleton.instance.speed).SetEase(Ease.Linear).OnComplete(()=> Destroy(gameObject));
    }

    [Command]
    public void CmdMoveTo()
    {
        MoveTo();
    }

    //поворот в сторону + движение
    [Server]
    public void RotateTo()
    {
        var fromPosition = transform.position;
        var toPosition = route.toTransform.position;

        var an = Math.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x);
        var degAn = an * 180 / Math.PI;

        transform.DORotate(new Vector3(0, 0, (float) degAn), 0, RotateMode.LocalAxisAdd).OnComplete(MoveTo);

    }

    [Command]
    public void CmdRotateTo()
    {
        RotateTo();
    }
}
