using Photon.Pun;
using UnityEngine;

public class ZombieAnimationController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Animator _animator;
    [SerializeField]private PhotonView _photonView;
    [SerializeField] private bool isOnline = false;
    // Start is called before the first frame update
    void Awake()
    {
     if(_animator == null)
        _animator = GetComponent<Animator>();
    }
    
    public void setTarget(bool haveTarget)
    {
        if(isOnline)
            _photonView.RPC("setTargetRPC", RpcTarget.All, haveTarget);
        else
            _animator.SetBool("HaveTarget", haveTarget);
    }
    public void setAttack()
    {
        if(isOnline)
            _photonView.RPC("setAttackRPC", RpcTarget.All);
        else
            _animator.SetTrigger("Attacking");
    }
    public void triggerDown()
    {
        if(isOnline)
            _photonView.RPC("triggerDownRPC", RpcTarget.All);
        _animator.SetTrigger("isDying");
    }
    
    [PunRPC]
    public void triggerDownRPC()
    {
        _animator.SetTrigger("isDying");
    }
    
    [PunRPC]
    public void setTargetRPC(bool haveTarget)
    {
        _animator.SetBool("HaveTarget", haveTarget);
    }
    
    [PunRPC]
    public void setAttackRPC()
    {
       
        _animator.SetTrigger("Attacking");
    }
       
}