using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Utils
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance = null;
        private readonly Queue<Action> _mainThreadActionsQueue = new Queue<Action>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public static UnityMainThreadDispatcher GetInstance()
        {
            return _instance;
        }

        private void Update()
        {
            lock (_mainThreadActionsQueue)
            {
                while (_mainThreadActionsQueue.Count > 0)
                {
                    _mainThreadActionsQueue.Dequeue()?.Invoke();
                }
            }
        }

        public void AddActionToMainThread(Action action)
        {
            lock (_mainThreadActionsQueue)
            {
                _mainThreadActionsQueue.Enqueue(action);
            }
        }
    }
}
