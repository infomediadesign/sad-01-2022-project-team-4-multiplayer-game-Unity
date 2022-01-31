using _Project.Scripts.Networking;
using UnityEngine;

namespace _Project.Scripts.Interactions
{
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private GameTask taskPrefab;
        [SerializeField] private GameTask clonedTask;
        public bool taskDone = false;
        
        public void Interact()
        {
            if (taskDone)
            {
                return;
            }

            if (clonedTask == null)
            {
                clonedTask = Instantiate(taskPrefab, transform);
                clonedTask.Init(OnTaskSuccess, InteractEnd);
            }
            else
            {
                clonedTask.gameObject.SetActive(true);
            }
            SocketManager.GetInstance().GetLocalPlayerController().SetLockInput(true);
        }

        private void InteractEnd()
        {
            clonedTask.gameObject.SetActive(false);
            // unlock Input
            SocketManager.GetInstance().GetLocalPlayerController().SetLockInput(false);
        }

        private void OnTaskSuccess()
        {
            taskDone = true;
        }
    }
}
