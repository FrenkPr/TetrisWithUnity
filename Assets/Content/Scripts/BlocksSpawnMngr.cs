using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksSpawnMngr : Singleton<BlocksSpawnMngr>
{
    [SerializeField] private List<GameObject> blocks;
    public GameObject currentBlock { get; private set; }
    public BlockController currentBlockController { get { return currentBlock != null ? currentBlock.GetComponent<BlockController>() : null; } }

    // Start is called before the first frame update
    void Start()
    {
        SpawnBlock();
    }

    public void SpawnBlock()
    {
        int randBlockIndex = Random.Range(0, blocks.Count);
        //randBlockIndex = 0;

        currentBlock = Instantiate(blocks[randBlockIndex], blocks[randBlockIndex].transform.position, Quaternion.identity, this.transform);
    }
}
