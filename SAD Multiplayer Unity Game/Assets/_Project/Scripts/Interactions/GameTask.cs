using System;
using UnityEngine;

namespace _Project.Scripts.Interactions
{
    public class GameTask : MonoBehaviour
    {
        private Action _onSuccessAction = null;
        private Action _onTaskCloseAction = null;
        
        [SerializeField] private GameObject taskBGGameObject;
        
        private void OnEnable()
        {
            taskBGGameObject.transform.localScale = Vector3.zero;
            taskBGGameObject.LeanScale(new Vector3(1, 1, 1), .5f).setEaseOutBack();
        }
        
        public void Init(Action onSuccessAction, Action onTaskCloseAction)
        {
            _onSuccessAction = onSuccessAction;
            _onTaskCloseAction = onTaskCloseAction;
        }

        protected void TaskCompleted()
        {
            _onSuccessAction?.Invoke();
        }
        
        protected void TaskClosed()
        {
            _onTaskCloseAction?.Invoke();
        }
        
        public void CloseTask()
        {
            taskBGGameObject.LeanScale(Vector3.zero, .5f).setEaseInBack().setOnComplete(() =>
            {
                TaskClosed();
            });
        }
    }
}
