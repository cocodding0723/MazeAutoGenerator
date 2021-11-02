using UnityEngine;

namespace Maze
{
    public class Block
    {
        public int BlockNumber { get; private set; }
        public readonly bool[] OpenWay = new bool[4];

        private static int s_increaseGroupNumber;

        public Block()
        {
            BlockNumber = s_increaseGroupNumber++;
        }

        public static Vector2Int GetPosition(int blockNumber, Vector2Int size)
        {
            return new Vector2Int(blockNumber / size.x, blockNumber % size.y);
        }

        public Vector2Int GetPosition(Vector2Int size) => GetPosition(BlockNumber, size);
        public int GetParentIndex(Vector2Int size) => BlockNumber * size.x + BlockNumber % size.y;
    }
}