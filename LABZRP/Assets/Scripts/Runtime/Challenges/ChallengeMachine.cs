using Photon.Pun;
using Runtime.Enemy.ScriptObjects.HordeMode.Challenges;
using Runtime.Player.Combat.PlayerStatus;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Challenges
{
    public class ChallengeMachine : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Transform ModelSpawnPoint;
        private ChallengeManager _challengeManager;
        private bool _isActivated = true;
        [SerializeField] private ScObChallengesSpecs[] _challenges;
        [Range(1, 3)]
        [SerializeField] private int difficulty = 1;
        private int _currentChallenge = 0;
        private int _currentPlayersInFrontOfMachine = 0;
        private GameObject _current3dModel;
        [SerializeField] private TextMeshPro ScreenPoints;
        [SerializeField] private bool changeChallenges = false;
        [SerializeField] private bool isOnline = false;
        [SerializeField] private TextMeshPro challengeDescription;
        [SerializeField] private GameObject challengeDescriptionPanel;


        private void Start()
        {
            _challengeManager = GameObject.FindGameObjectWithTag("HorderManager").GetComponent<ChallengeManager>();
            if(!isOnline || PhotonNetwork.IsMasterClient){
                InitializeChallengeMachine();
            }
        
        }

        private void Update()
        {
            if (changeChallenges)
            {
                DebugRandomizeChallenge();
            }
        }

        public void InitializeChallengeMachine()
        {
            _currentChallenge = Random.Range(0, _challenges.Length);
            if(isOnline){
                if(_current3dModel)
                    photonView.RPC("Model3dChallengeMachine", RpcTarget.All, true, _currentChallenge);
                else
                {
                    photonView.RPC("Model3dChallengeMachine", RpcTarget.All, false, _currentChallenge);
                }

            }
            else
            {
                if (_current3dModel)
                    Model3dChallengeMachine(true, _currentChallenge);
                else
                    Model3dChallengeMachine(false, _currentChallenge);
            }
        }

        [PunRPC]
        public void Model3dChallengeMachine(bool destroy, int _currentChallengeRPC)
        {
            if(destroy)
                Destroy(_current3dModel);
            else
            {
                challengeDescription.text = _challenges[_currentChallengeRPC].ChallengeDescription;
                _current3dModel = Instantiate(_challenges[_currentChallengeRPC].Model3dChallengeMachine,
                    ModelSpawnPoint.position, ModelSpawnPoint.rotation);
                _current3dModel.transform.parent = transform;
                ScreenPoints.text = "Recompensa:\n" +_challenges[_currentChallengeRPC].ChallengeReward;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _currentPlayersInFrontOfMachine++;
                if(_currentPlayersInFrontOfMachine == 1){
                    challengeDescriptionPanel.SetActive(true);
                    _current3dModel.SetActive(false);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.CompareTag("Player"))
                _currentPlayersInFrontOfMachine--;
            if(_currentPlayersInFrontOfMachine == 0){
                challengeDescriptionPanel.SetActive(false);
                _current3dModel.SetActive(false);
            }
        }


        private void OnTriggerStay(Collider other)
        {
            if (!isOnline || PhotonNetwork.IsMasterClient)
            {
                PlayerStats playerStats = other.GetComponent<PlayerStats>();
                if (playerStats && _isActivated)
                {
                    if (playerStats.GetInteracting() && !playerStats.GetIsDown())
                    {
                        Debug.Log("Desafio começou");
                        if (_challengeManager.StartChallenge(_challenges[_currentChallenge], this))
                        {
                            _isActivated = false;
                            ScObChallengesSpecs challenge = _challenges[_currentChallenge];
                            challenge.zombiesToKill = (_challenges[_currentChallenge].zombiesToKill * difficulty) +
                                                      (_challengeManager.getPlayerCount() * 15);
                            challenge.ChallengeReward = (_challenges[_currentChallenge].ChallengeReward * difficulty);
                            _challengeManager.StartChallenge(_challenges[_currentChallenge], this);

                        }
                    }
                }
            }
        }
    
        public ScObChallengesSpecs getCurrentChallenge()
        {
            return _challenges[_currentChallenge];
        }

        public void setIsActivated(bool value)
        {
            if (isOnline)
                photonView.RPC("setIsActivatedRPC", RpcTarget.All, value);
            else
                _isActivated = value;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_currentChallenge);
            }
            else
            {
                _currentChallenge = (int) stream.ReceiveNext();
            }
        }

        [PunRPC]
        public void setIsActivatedRPC(bool value)
        {
            _isActivated = value;
        }

        private void DebugRandomizeChallenge()
        {
            changeChallenges = false;
            InitializeChallengeMachine();
        }
    
    
    }
}
