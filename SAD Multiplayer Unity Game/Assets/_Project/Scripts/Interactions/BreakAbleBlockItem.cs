using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Interactions
{
    public class BreakAbleBlockItem : MonoBehaviour, IPointerDownHandler
    {
        private BlockSpawnerItem _blockSpawnerItem;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            _rectTransform.anchoredPosition += Vector2.left * Random.Range(800, 1200) * Time.deltaTime;
            _rectTransform.eulerAngles = new Vector3(0, 0, _rectTransform.eulerAngles.z + 
                                                           700 * Time.deltaTime);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _blockSpawnerItem.BlockBroken();
            Destroy(gameObject);
        }

        public void InitBlock(BlockSpawnerItem blockSpawnerItem)
        {
            _blockSpawnerItem = blockSpawnerItem;
        }
    }
}
