using UnityEngine;

public class ZombieAnimationController : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private bool haveDifferentAnimations = false;
    // Start is called before the first frame update
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    
    public void setTarget(bool haveTarget)
    {
        if(!haveDifferentAnimations)
            _animator.SetBool("HaveTarget", haveTarget);
    }
    public void setAttack()
    {
        if(!haveDifferentAnimations)
            _animator.SetTrigger("Attacking");
    }
    public void triggerDown()
    {
        if(!haveDifferentAnimations)
            _animator.SetTrigger("isDying");
    }
       
}