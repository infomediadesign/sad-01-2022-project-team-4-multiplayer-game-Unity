using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Player
{
    public class PlayerSetup : MonoBehaviour
    { 
        public bool isLocalPlayer;
        [SerializeField] private Behaviour[] behavioursToToggle;
        [SerializeField] private GameObject[] gameObjectsToToggle;
        [SerializeField] private GameObject defaultModel;

        public void SetupPlayer(bool isLocalPlayer, int modelIndex)
        {
            Debug.Log("Setup Player");
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
            
            //Assign a Cool Character
            defaultModel.SetActive(false);
            if (!isLocalPlayer)
            {
                GameObject coolModel = ReferenceManager.GetInstance().availableModels[modelIndex];
                GameObject coolModelClone = Instantiate(coolModel, Vector3.zero, Quaternion.identity, transform);
            }
        }
    }
}
