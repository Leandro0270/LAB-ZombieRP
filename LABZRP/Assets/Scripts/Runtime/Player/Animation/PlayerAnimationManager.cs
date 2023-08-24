using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Player.Animation
{
    public class PlayerAnimationManager : MonoBehaviourPunCallbacks
    {
        [FormerlySerializedAs("_animator")] [SerializeField] private Animator animator;

        public void setIsIdle(bool idle)
        {
            animator.SetBool("isIdle", idle);
        }
        public void setIsWalkingForward(bool walking)
        {
            animator.SetBool("isWalkingForward", walking);
        }
        public void setIsWalkingBackward(bool walking)
        {
            animator.SetBool("isWalkingBackward", walking);
        }
        public void setIsWalkingLeft(bool walking)
        {
            animator.SetBool("isWalkingLeft", walking);
        }
        public void setIsWalkingRight(bool walking)
        {
            animator.SetBool("isWalkingRight", walking);
        }
        public void setAttack()
        {
            animator.SetTrigger("Melee");
        }
        public void setDown(bool down)
        {
            animator.SetBool("isDown", down);
        }
    
        public void setDowning()
        {
            animator.SetTrigger("Downing");
        }
    
    
    
    }
}
