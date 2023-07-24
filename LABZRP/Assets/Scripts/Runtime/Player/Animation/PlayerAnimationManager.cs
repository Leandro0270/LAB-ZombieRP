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
    
    public void setMovement(bool walking)
    {
        _animator.SetBool("Walking", walking);
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
