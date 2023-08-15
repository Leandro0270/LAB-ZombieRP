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
        private bool _challengeStarted = false;
        private bool _isGreenUp = true;
        private bool _isOrangeUp = true;
        [SerializeField] private GameObject damageEffect;
        [SerializeField] private float StartLife = 100f;
        private float _currentLife = 100f;
        [SerializeField] private bool isOnline = false;

        private void Start()
        {
            _currentLife = StartLife;
        }

        public void setChallengeManager(ChallengeManager challengeManager)
        {
            _challengeManager = challengeManager;
        }

        public void takeHit(float damage)
        {
            if (_challengeStarted)
            {
                GameObject effect = Instantiate(damageEffect, transform.position, transform.rotation);
                Destroy(effect, 2f);
                _currentLife -= damage;
                if (_currentLife <= 0)
                {
                    explodeMug(redMug);
                    _challengeManager.destroyCoffeeMachine();
                }
                else
                {
                    if ((_currentLife <= ((StartLife / 3) * 2)) && _isGreenUp)
                    {
                        _isGreenUp = false;
                        explodeMug(greenMug);
                    }
                    else if ((_currentLife <= (StartLife / 3)) && _isOrangeUp)
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
            _challengeStarted = true;
        }
    
    
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_currentLife);
            
            
            }
            else
            {
                _currentLife = (float)stream.ReceiveNext();
           
            }
        }

    }
}
