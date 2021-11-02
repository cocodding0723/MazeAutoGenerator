# 미로 자동 생성기 Maze Auto Generator

참고 사이트 : http://weblog.jamisbuck.org/2010/12/29/maze-generation-eller-s-algorithm

다음은 생성하는 과정을 나타낸 이미지입니다. 

![ProcedualCreate](ProcedualCreate.gif)

## 방법

미로 자동 생성기는 `엘러` 알고리즘을 기반으로 제작되었습니다.  
엘러 알고리즘은 미로를 제작하는 알고리즘으로 시간복잡도 O(N) 의 속도를 가지고 있습니다.   

방법은 다음과 같습니다.

1. 전체 블럭을 각 번호로 초기화 시킵니다. 그리고 블럭 갯수만큼 집합을 만듭니다.
2. 인접한 블럭을 무작위로 병합합니다. 단 같은 집합일 경우 합치지 않습니다.
3. 해당 행의 집합중 최소 1개 이상 무작위로 아래 블럭과 병합시킵니다.
4. 마지막 행에 도달할 때까지 반복합니다.
5. 마지막 행일 경우 같은 집합에 속하지 않는 집합과의 벽을 허뭅니다.

## 팁

저는 이 알고리즘을 유니티로 제작하였으며 추가적으로 그룹핑을 하기 위해서 유니온 파인드(Disjoint Set) 알고리즘을 사용하여 그룹핑 작업하는 시간을 줄였습니다.

유니온 파인드 알고리즘을 사용할때 다음과 같은 로직을 썻습니다.

블럭의 집합 번호는 2차원 배열의 열과 행을 이용하여 도출하였습니다.

```
집합 번호 : i * 미로 크기 + j % 미로 크기
```

블럭의 위치는 미로의 크기를 이용해 도출했습니다.

```
i = 집합번호 / 미로 크기
j = 집합번호 % 미로 크기
```
---

## Method

The maze automatic generator is built on the basis of the Eller algorithm.
The Eller algorithm is an algorithm that produces labyrinths and has a speed of time complexity O(N).

The method is as follows.

1. Initialize the entire block to each number. And make a set by the number of blocks.
2. Randomly merge adjacent blocks. However, if it is the same set, it does not merge.
3. Randomly merge at least one set of rows with the block below.
4. Repeat until the last row is reached.
5. In the last row, break the wall with the set that does not belong to the same set.

## Tip

I made this algorithm as Unity and reduced the time to group using the UnionFind algorithm to further group.

When using the UnionFind algorithm, the following logic is written.

The set number of blocks was derived using columns and rows in a two-dimensional array.

```
Set number: i * Maze size + j % Maze size
```

The location of the block was derived using the size of the maze.

```
i = Set number / Maze size
j = Set number % Maze Size
```