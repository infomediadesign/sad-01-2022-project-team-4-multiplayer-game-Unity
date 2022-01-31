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
            if (taskDone || !SocketManager.isGameStarted)
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
            AlwaysOnUIManager.onUpdatePlayerInput?.Invoke(true, GameUI.TaskPanel);
        }

        private void InteractEnd()
        {
            clonedTask.gameObject.SetActive(false);
            // unlock Input
            AlwaysOnUIManager.onUpdatePlayerInput?.Invoke(false, GameUI.TaskPanel);
        }

        private void OnTaskSuccess()
        {
            taskDone = true;
            SocketManager.GetInstance().SendTaskFinished();
        }
    }
}
