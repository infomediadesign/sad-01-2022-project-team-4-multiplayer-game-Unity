using UnityEngine;

namespace _Project.Scripts.Player
{
    public class PlayerSetup : MonoBehaviour
    { 
        public bool isLocalPlayer;
        [SerializeField] private Behaviour[] behavioursToToggle;
        [SerializeField] private GameObject[] gameObjectsToToggle;

        public void SetupPlayer(bool isLocalPlayer)
        {
            this.isLocalPlayer = isLocalPlayer;

            for (int i = 0; i < behavioursToToggle.Length; i++)
            {
                behavioursToToggle[i].enabled = isLocalPlayer;
            }

            for (int i = 0; i < gameObjectsToToggle.Length; i++)
            {
                gameObjectsToToggle[i].SetActive(isLocalPlayer);
            }

            if (!this.isLocalPlayer)
            {
                GetComponent<Rigidbody>().useGravity = false;
                GetComponent<CapsuleCollider>().enabled = false;
                GetComponent<CharacterController>().enabled = false;
            }
        }
    }
}
