// MIT License
//
// Copyright (c) 2021 cocodding0723
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maze
{
    public class MazeGenerator : MonoBehaviour
    {
        public Vector2Int mazeSize = new Vector2Int(25, 25);

        private Vector2Int BlockSize => mazeSize / 2;

        private Block[,] _blocks;
        private bool[,] _existWalls;
        private DisjointSet _disjointSet;
        private readonly Dictionary<int, List<int>> _lastRowBlocks = new Dictionary<int, List<int>>();

        [SerializeField] private float delayCreateTime = 0.25f;
        [SerializeField] private bool isDelayCreate;
        [SerializeField] private bool isDrawGizmo;

        [SerializeField] private GameObject wallPrefab;

        private void Awake()
        {
            var size = BlockSize;
            var disjointSetSize = BlockSize.x * BlockSize.y;

            _blocks = new Block[size.x, size.y];
            _existWalls = new bool[mazeSize.x, mazeSize.y];
            _disjointSet = new DisjointSet(disjointSetSize);
        }

        private void Start()
        {
            InitBlocks();

            if (isDelayCreate && isDrawGizmo)
            {
                StartCoroutine(DelayCreateMaze());
            }
            else
            {
                for (int y = 0; y < BlockSize.y - 1; y++)
                {
                    RandomMergeRowBlocks(y);
                    DropDownGroups(y);
                }

                OrganizeLastLine();
                MakeHoleInPath();

                if (!isDrawGizmo)
                {
                    BuildWalls();
                }
            }
        }

        /// <summary>
        /// 블럭을 초기화 하는 함수.
        /// A function that initializes blocks.
        /// </summary>
        private void InitBlocks()
        {
            for (int x = 0; x < BlockSize.x; x++)
            {
                for (int y = 0; y < BlockSize.y; y++)
                {
                    _blocks[x, y] = new Block();
                }
            }
        }

        /// <summary>
        /// 행의 블럭을 순차적으로 접근하면서 선택한 블럭과 오른쪽 블럭을 랜덤하게 합치는 함수.
        /// A function of randomly merge the selected block and the right block while sequentially approaching the blocks of the row.
        /// </summary>
        /// <param name="row">현재 행 (current row)</param>
        private void RandomMergeRowBlocks(int row)
        {
            for (int x = 0; x < BlockSize.x - 1; x++)
            {
                var canMerge = Random.Range(0, 2) == 1;
                var currentBlockNumber = _blocks[x, row].BlockNumber;
                var nextBlockNumber = _blocks[x + 1, row].BlockNumber;

                if (canMerge && !_disjointSet.IsUnion(currentBlockNumber, nextBlockNumber))
                {
                    _disjointSet.Merge(currentBlockNumber, nextBlockNumber);
                    _blocks[x, row].OpenWay[(int)Direction.Right] = true;
                }
            }
        }

        /// <summary>
        /// 현재 행에서 가지를 내리는 함수
        /// A function of branching off the current row.
        /// </summary>
        /// <param name="row">현재 행 (current row)</param>
        private void DropDownGroups(int row)
        {
            _lastRowBlocks.Clear();

            for (int x = 0; x < BlockSize.x; x++)
            {
                var blockNumber = _blocks[x, row].BlockNumber;
                var parentNumber = _disjointSet.Find(_blocks[x, row].BlockNumber);

                if (!_lastRowBlocks.ContainsKey(parentNumber))
                {
                    _lastRowBlocks.Add(parentNumber, new List<int>());
                }

                _lastRowBlocks[parentNumber].Add(blockNumber);
            }

            foreach (var group in _lastRowBlocks)
            {
                if (group.Value.Count == 0) continue;

                var randomDownCount = Random.Range(1, group.Value.Count);

                for (int i = 0; i < randomDownCount; i++)
                {
                    var randomBlockIndex = Random.Range(0, group.Value.Count);

                    var currentBlockNumber = group.Value[randomBlockIndex];
                    var currentBlockPosition = Block.GetPosition(currentBlockNumber, BlockSize);

                    var currentBlock = _blocks[currentBlockPosition.x, currentBlockPosition.y];
                    var underBlock = _blocks[currentBlockPosition.x, currentBlockPosition.y + 1];

                    _disjointSet.Merge(currentBlock.BlockNumber, underBlock.BlockNumber);
                    currentBlock.OpenWay[(int)Direction.Down] = true;

                    group.Value.RemoveAt(randomBlockIndex);
                }
            }
        }
        
        /// <summary>
        /// 마지막 줄을 정리하는 함수
        /// A function that organizes the last line.
        /// </summary>
        private void OrganizeLastLine()
        {
            var lastRow = BlockSize.y - 1;

            for (int x = 0; x < BlockSize.x - 1; x++)
            {
                var currentBlock = _blocks[x, lastRow];
                var nextBlock = _blocks[x + 1, lastRow];

                if (!_disjointSet.IsUnion(currentBlock.BlockNumber, nextBlock.BlockNumber))
                {
                    currentBlock.OpenWay[(int)Direction.Right] = true;
                }
            }
        }

        private IEnumerator DelayCreateMaze()
        {
            for (int y = 0; y < BlockSize.y - 1; y++)
            {
                yield return StartCoroutine(DelayRandomMergeBlocks(y));
                yield return StartCoroutine(DelayDropDownGroups(y));

                MakeHoleInPath();

                yield return new WaitForSeconds(delayCreateTime);
            }

            yield return new WaitForSeconds(delayCreateTime);

            yield return StartCoroutine(DelayCleanUpLastLine());
            MakeHoleInPath();
        }

        private IEnumerator DelayRandomMergeBlocks(int row)
        {
            for (int x = 0; x < BlockSize.x - 1; x++)
            {
                var canMerge = Random.Range(0, 2) == 1;
                var currentBlockNumber = _blocks[x, row].BlockNumber;
                var nextBlockNumber = _blocks[x + 1, row].BlockNumber;

                if (canMerge && !_disjointSet.IsUnion(currentBlockNumber, nextBlockNumber))
                {
                    _disjointSet.Merge(currentBlockNumber, nextBlockNumber);
                    _blocks[x, row].OpenWay[(int)Direction.Right] = true;
                }

                MakeHoleInPath();

                yield return new WaitForSeconds(delayCreateTime);
            }
        }

        private IEnumerator DelayDropDownGroups(int row)
        {
            _lastRowBlocks.Clear();

            for (int x = 0; x < BlockSize.x; x++)
            {
                var blockNumber = _blocks[x, row].BlockNumber;
                var parentNumber = _disjointSet.Find(_blocks[x, row].BlockNumber);

                if (!_lastRowBlocks.ContainsKey(parentNumber))
                {
                    _lastRowBlocks.Add(parentNumber, new List<int>());
                }

                _lastRowBlocks[parentNumber].Add(blockNumber);
            }

            foreach (var group in _lastRowBlocks)
            {
                if (group.Value.Count == 0) continue;

                var randomDownCount = Random.Range(1, group.Value.Count);

                for (int i = 0; i < randomDownCount; i++)
                {
                    var randomBlockIndex = Random.Range(0, group.Value.Count);

                    var currentBlockNumber = group.Value[randomBlockIndex];
                    var currentBlockPosition = Block.GetPosition(currentBlockNumber, BlockSize);

                    var currentBlock = _blocks[currentBlockPosition.x, currentBlockPosition.y];
                    var underBlock = _blocks[currentBlockPosition.x, currentBlockPosition.y + 1];

                    _disjointSet.Merge(currentBlock.BlockNumber, underBlock.BlockNumber);
                    currentBlock.OpenWay[(int)Direction.Down] = true;

                    group.Value.RemoveAt(randomBlockIndex);

                    MakeHoleInPath();

                    yield return new WaitForSeconds(delayCreateTime);
                }
            }
        }

        private IEnumerator DelayCleanUpLastLine()
        {
            var lastRow = BlockSize.y - 1;

            for (int x = 0; x < BlockSize.x - 1; x++)
            {
                var currentBlock = _blocks[x, lastRow];
                var nextBlock = _blocks[x + 1, lastRow];

                if (!_disjointSet.IsUnion(currentBlock.BlockNumber, nextBlock.BlockNumber))
                {
                    currentBlock.OpenWay[(int)Direction.Right] = true;
                }

                MakeHoleInPath();

                yield return new WaitForSeconds(delayCreateTime);
            }
        }

        private void MakeHoleInPath()
        {
            for (int x = 0; x < BlockSize.x; x++)
            {
                for (int y = 0; y < BlockSize.y; y++)
                {
                    var adjustPosition = new Vector2Int(x * 2 + 1, y * 2 + 1);
                    _existWalls[adjustPosition.x, adjustPosition.y] = true;

                    if (_blocks[x, y].OpenWay[(int)Direction.Down])
                        _existWalls[adjustPosition.x, adjustPosition.y + 1] = true;
                    if (_blocks[x, y].OpenWay[(int)Direction.Right])
                        _existWalls[adjustPosition.x + 1, adjustPosition.y] = true;
                }
            }
        }

        private void BuildWalls()
        {
            for (int x = 0; x < mazeSize.x; x++)
            {
                for (int y = 0; y < mazeSize.y; y++)
                {
                    if (_existWalls[x, y]) continue;

                    var myTransform = transform;
                    var mazeHalfSize = new Vector3(mazeSize.x, 0, mazeSize.y) / 2;
                    var wallPosition = new Vector3(x, 0.5f, y) - mazeHalfSize + myTransform.position;

                    Instantiate(wallPrefab, wallPosition, Quaternion.identity, myTransform);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && isDrawGizmo)
            {
                Gizmos.color = Color.red;

                for (int x = 0; x < mazeSize.x; x++)
                {
                    for (int y = 0; y < mazeSize.y; y++)
                    {
                        if (!_existWalls[x, y])
                        {
                            var mazeHalfSize = new Vector3(mazeSize.x, 0, mazeSize.y) / 2;
                            var wallPosition = new Vector3(x, 0.5f, y) - mazeHalfSize + transform.position;
                            Gizmos.DrawCube(wallPosition, Vector3.one);
                        }
                    }
                }
            }
        }
    }
}