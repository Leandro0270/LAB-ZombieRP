using Photon.Pun;
using UnityEngine;

namespace Runtime.Challenges
{
    public class ChallengeCoffeeMachine : MonoBehaviourPunCallbacks, IPunObservable
    {
        private ChallengeManager _challengeManager;
        [SerializeField] private GameObject orangeMug;
        [SerializeField] private GameObject redMug;
        [SerializeField] private GameObject greenMug;
        [SerializeField] private GameObject mugExplosionAreaEffect;
        [SerializeField] private  GameObject effectSpawnPoint;
        private bool challengeStarted = false;
        private bool _isGreenUp = true;
        private bool _isOrangeUp = true;
        [SerializeField] private GameObject damageEffect;
        [SerializeField] private float StartLife = 100f;
        private float currentLife = 100f;
        [SerializeField] private bool isOnline = false;

        private void Start()
        {
            currentLife = StartLife;
        }

        public void setChallengeManager(ChallengeManager challengeManager)
        {
            _challengeManager = challengeManager;
        }

        public void takeHit(float damage)
        {
            if (challengeStarted)
            {
                GameObject effect = Instantiate(damageEffect, transform.position, transform.rotation);
                Destroy(effect, 2f);
                currentLife -= damage;
                if (currentLife <= 0)
                {
                    explodeMug(redMug);
                    _challengeManager.destroyCoffeeMachine();
                }
                else
                {
                    if ((currentLife <= ((StartLife / 3) * 2)) && _isGreenUp)
                    {
                        _isGreenUp = false;
                        explodeMug(greenMug);
                    }
                    else if ((currentLife <= (StartLife / 3)) && _isOrangeUp)
                    {
                        _isOrangeUp = false;
                        explodeMug(orangeMug);

                    }
                }
            }
        }
        public void explodeMug(GameObject mug)
        {
            if (isOnline)
            {
                if(PhotonNetwork.IsMasterClient)
                    PhotonNetwork.Destroy(mug);
            }
            Destroy(mug);
        }
    
        public void startChallenge()
        {
            challengeStarted = true;
        }
    
    
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(currentLife);
            
            
            }
            else
            {
                currentLife = (float)stream.ReceiveNext();
           
            }
        }

    }
}
