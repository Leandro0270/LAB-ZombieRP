using UnityEngine;

public class ZombieAnimationController : MonoBehaviour
{
    private Animator _animator;
    // Start is called before the first frame update
    void Awake()
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
    public void triggerDown()
    {
        _animator.SetTrigger("isDying");
    }
       
}