using UnityEngine;

public class ZombieAnimationController : MonoBehaviour
{
    private Animator _animator;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }
    
    public void setTarget(bool haveTarget)
    {
        _animator.SetBool("HaveTarget", haveTarget);
    }
    public void setAttack()
    {
        _animator.SetTrigger("Attacking");
    }
    public void setDown(bool down)
    {
        _animator.SetBool("isDead", down);
    }
       
}