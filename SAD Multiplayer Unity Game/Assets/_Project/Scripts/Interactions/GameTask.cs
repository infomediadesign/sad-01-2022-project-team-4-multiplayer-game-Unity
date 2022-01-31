using System;
using UnityEngine;

namespace _Project.Scripts.Interactions
{
    public class GameTask : MonoBehaviour
    {
        private Action _onSuccessAction = null;
        private Action _onTaskCloseAction = null;
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
    }
}
