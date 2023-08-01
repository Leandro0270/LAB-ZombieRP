using Photon.Pun;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Animator _animator;
    [SerializeField] private PhotonView photonView;
    [SerializeField] private bool isOnline = false;
    // Start is called before the first frame update
    void Start()
    {
        if(_animator == null)
            _animator = GetComponent<Animator>();
        if(photonView == null)
            photonView = GetComponent<PhotonView>();
    }
    
    
    public void setIsIdle(bool idle)
    {
        _animator.SetBool("isIdle", idle);
    }
    public void setIsWalkingForward(bool walking)
    {
        _animator.SetBool("isWalkingForward", walking);
    }
    public void setIsWalkingBackward(bool walking)
    {
        _animator.SetBool("isWalkingBackward", walking);
    }
    public void setIsWalkingLeft(bool walking)
    {
        _animator.SetBool("isWalkingLeft", walking);
    }
    public void setIsWalkingRight(bool walking)
    {
        _animator.SetBool("isWalkingRight", walking);
    }
    public void setAttack()
    {
        _animator.SetTrigger("Melee");
    }
    public void setDown(bool down)
    {
        _animator.SetBool("isDown", down);
    }
    
    public void setDowning()
    {
        _animator.SetTrigger("Downing");
    }
    
    
    
}
