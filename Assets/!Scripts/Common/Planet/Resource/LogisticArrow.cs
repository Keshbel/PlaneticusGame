using System;
using System.Threading.Tasks;
using DG.Tweening;
using Mirror;
using UnityEngine;

public class LogisticArrow : NetworkBehaviour
{
    public Collider2D thisCollider;
    [SyncVar] public CurrentPlayer playerOwner;
    [SyncVar] public Transform fromTransform;
    [SyncVar] public Transform toTransform;

    [Client]
    private void Start()
    {
        if (!thisCollider)
            thisCollider = GetComponent<Collider2D>();
    }

    private async void OnEnable()
    {
        if (playerOwner != null && playerOwner.isBot)
        {
            await Task.Delay(200);
            TargetStartMove(fromTransform, toTransform);
        }
    }

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

        transform.DORotate(new Vector3(0, 0, (float) degAn), 0.2f, RotateMode.LocalAxisAdd).OnComplete(()=>MoveTo(toTransform));
    }

    [TargetRpc]
    public void RpcTargetStartMove(Transform from, Transform to)
    {
        TargetStartMove(from, to);
    }
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
}
