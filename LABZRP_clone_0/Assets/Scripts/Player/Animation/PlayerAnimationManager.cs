using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    private Animator _animator;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
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
