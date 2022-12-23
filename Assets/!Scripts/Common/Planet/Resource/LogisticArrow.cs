using System;
using DG.Tweening;
using Mirror;
using UnityEngine;

public class LogisticArrow : NetworkBehaviour
{
    //public Collider2D thisCollider;
    [SyncVar (hook = nameof(UpdateOwner))] public CurrentPlayer playerOwner;
    public NetworkTransform networkTransform;

    /*[Client]
    private void Start()
    {
        if (!thisCollider) thisCollider = GetComponent<Collider2D>();
    }*/

    [Client]
    public void SetStartPosition(Transform from)
    {
        transform.position = from.position;
    }
    
    [Client] // движение в сторону
    private void MoveTo(Transform to)
    {
        var toPosition = to.position;
        var distance = Vector2.Distance(transform.position, toPosition);
        
        transform.DOMove(toPosition, distance / AllSingleton.Instance.speed).SetEase(Ease.Linear).OnComplete(CmdUnSpawn);
    }

    [Client] //поворот в сторону + движение
    private void RotateTo(Transform to)
    {
        var fromPosition = transform.position;
        var toPosition = to.position;
        var an = Math.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x);
        var degAn = an * 180 / Math.PI;

        transform.DORotate(new Vector3(0, 0, (float) degAn), 0.2f, RotateMode.LocalAxisAdd).OnComplete(()=>MoveTo(to));
    }

    [TargetRpc]
    public void RpcTargetStartMove(Transform from, Transform to)
    {
        TargetStartMove(from, to);
    }
    [Client]
    public void TargetStartMove(Transform from, Transform to)
    {
        SetStartPosition(from);
        RotateTo(to);
    }

    [Command (requiresAuthority = false)]
    private void CmdUnSpawn()
    {
        NetworkServer.UnSpawn(gameObject);
        ResourceSingleton.Instance.arrowPoolManager.PutBackInPool(gameObject);
    }
    
    [Client]
    public void UpdateOwner(CurrentPlayer oldPlayer, CurrentPlayer newPlayer)
    {
        if (newPlayer != null) networkTransform.syncDirection = newPlayer.isBot ? SyncDirection.ServerToClient : SyncDirection.ClientToServer;
        else if (oldPlayer != null) networkTransform.syncDirection = oldPlayer.isBot ? SyncDirection.ServerToClient : SyncDirection.ClientToServer;
    }
}
