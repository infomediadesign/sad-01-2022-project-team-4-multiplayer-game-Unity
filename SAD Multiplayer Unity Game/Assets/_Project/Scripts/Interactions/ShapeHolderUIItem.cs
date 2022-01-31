using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Interactions
{
    public class ShapeHolderUIItem : MonoBehaviour, IDropHandler
    {
        public ShapeType slotType;
        public ShapeTask shapeTask;
        
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                if (eventData.pointerDrag.GetComponent<ShapeUIItem>().shapeType == slotType)
                {
                    eventData.pointerDrag.GetComponent<ShapeUIItem>().SetLockState(true);
                    eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition =
                        GetComponent<RectTransform>().anchoredPosition;
                    
                    shapeTask.CorrectSlot();
                }
                else
                {
                    eventData.pointerDrag.GetComponent<ShapeUIItem>().ThrowToStartingPos();
                }
            }
        }
    }
}
