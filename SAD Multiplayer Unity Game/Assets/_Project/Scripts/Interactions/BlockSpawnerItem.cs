using TMPro;
using UnityEngine;

namespace _Project.Scripts.Interactions
{
    public class BlockSpawnerItem : MonoBehaviour
    {
        [SerializeField] private RectTransform[] blockSpawnPoints;
        [SerializeField] private int blocksBrokenTillNow = 0;
        [SerializeField] private int blockRequiredToBeBroken = 0;
        [SerializeField] private float timeBetweenBlocks = 1;
        [SerializeField] private float currentTimeRemaining = 0;
        [SerializeField] private BreakAbleBlockItem breakAbleBlockPrefab;
        
        [SerializeField] private BlockBreakTask blockBreakTask;
        [SerializeField] private TextMeshProUGUI scoreTextMeshProUGUI;

        private void Awake()
        {
            blockRequiredToBeBroken = Random.Range(10, 20);
            scoreTextMeshProUGUI.SetText($"Blocks broken {blocksBrokenTillNow} out of {blockRequiredToBeBroken}");
        }

        private void Update()
        {
            HandleBlockSpawning();
        }

        private void HandleBlockSpawning()
        {
            currentTimeRemaining -= Time.deltaTime;
            if (currentTimeRemaining <= 0 && blocksBrokenTillNow < blockRequiredToBeBroken)
            {
                SpawnBlock();
            }
        }

        private void SpawnBlock()
        {
            currentTimeRemaining = timeBetweenBlocks;
            RectTransform parentRectTransform = blockSpawnPoints[Random.Range(0, blockSpawnPoints.Length)];
            BreakAbleBlockItem breakAbleBlockItem = Instantiate(breakAbleBlockPrefab, 
                Vector2.zero, Quaternion.identity,
                parentRectTransform);
            breakAbleBlockItem.InitBlock(this);
            breakAbleBlockItem.GetComponent<RectTransform>().anchoredPosition = parentRectTransform.anchoredPosition;
            Destroy(breakAbleBlockItem.gameObject, 3f);
        }

        public void BlockBroken()
        {
            blocksBrokenTillNow++;
            scoreTextMeshProUGUI.SetText($"Blocks broken {blocksBrokenTillNow} out of {blockRequiredToBeBroken}");
            if (blocksBrokenTillNow >= blockRequiredToBeBroken)
            {
                blockBreakTask.TaskFinish();
            }
        }
    }
}
