using System.Collections.Generic;
using _Project.Scripts.Interactions;
using UnityEngine;

namespace _Project.Scripts.Player
{
    public class PlayerInteractionController : MonoBehaviour
    {
        [SerializeField] private float interactionRadius = 5f;
        [SerializeField] private float lookPercentThreshold = .9f;
        [SerializeField] private Transform playerCameraTransform;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] List<Interactable> interactables = new List<Interactable>();
        [SerializeField] private Interactable currentHighlightedInteractable = null;
        [SerializeField] private Interactable currentSelectedInteractable = null;

        [SerializeField] private Material lastMat;
        [SerializeField] private Material highlightedMaterial;

        private void Update()
        {
            DetectInteractables();
            UpdateLookAtInteractables();
            Interact();
        }

        private void DetectInteractables()
        {
            interactables.Clear();
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRadius, interactableLayer);
            if (colliders == null || colliders.Length == 0)
            {
                return;
            }
        
            for (int i = 0; i < colliders.Length; i++)
            {
                //Check if an Object is an interactable?
                if (colliders[i].TryGetComponent(out Interactable interactable))
                {
                    interactables.Add(interactable);
                }
            }
        }
    
        private void UpdateLookAtInteractables()
        {
            if (interactables == null || interactables.Count == 0)
            {
                if(currentHighlightedInteractable != null && lastMat != null)
                {
                    currentHighlightedInteractable.GetComponent<Renderer>().material =
                        lastMat;
                }
                return;
            }
        
            for (int i = 0; i < interactables.Count; i++)
            {
                float lookPercentage = Vector3.Dot(playerCameraTransform.forward.normalized,
                    (interactables[i].transform.position - playerCameraTransform.position).normalized);
                if (lookPercentage >= lookPercentThreshold)
                {
                    if(currentHighlightedInteractable != null && lastMat != null)
                    {
                        currentHighlightedInteractable.GetComponent<Renderer>().material =
                            lastMat;
                    }
                    currentHighlightedInteractable = interactables[i];
                    lastMat = currentHighlightedInteractable.GetComponent<Renderer>().material;
                    currentHighlightedInteractable.GetComponent<Renderer>().material =
                        highlightedMaterial;
                    return;
                }
            }
        
            if(currentHighlightedInteractable != null && lastMat != null)
            {
                currentHighlightedInteractable.GetComponent<Renderer>().material =
                    lastMat;
                currentHighlightedInteractable = null;
            }
        }

        private void Interact()
        {
            if (currentHighlightedInteractable == null)
            {
                return;
            }
        
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentSelectedInteractable = currentHighlightedInteractable;
                currentSelectedInteractable.Interact();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
