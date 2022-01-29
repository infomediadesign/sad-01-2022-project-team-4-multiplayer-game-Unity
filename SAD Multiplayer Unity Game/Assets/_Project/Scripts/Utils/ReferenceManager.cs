using System;
using UnityEngine;

namespace _Project.Scripts.Utils
{
    public class ReferenceManager : MonoBehaviour
    {
        private static ReferenceManager Instance = null;
        public GameObject[] availableModels;

        public static ReferenceManager GetInstance()
        {
            return Instance;
        }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
    }
}
