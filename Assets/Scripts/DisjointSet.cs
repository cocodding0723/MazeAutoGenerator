namespace Maze
{
    public class DisjointSet
    {
        public int ParentsSize { get; private set; }
        public readonly int[] Parents;

        public DisjointSet(int parentSize)
        {
            ParentsSize = parentSize;
            Parents = new int[parentSize];

            for (int i = 0; i < ParentsSize; i++)
                Parents[i] = i;
        }

        public int Find(int x)
        {
            if (x == Parents[x]) return x;

            return Parents[x] = Find(Parents[x]);
        }

        public void Merge(int a, int b)
        {
            a = Find(a);
            b = Find(b);

            if (a == b) return;

            if (a > b) Parents[a] = b;
            else Parents[b] = a;
        }

        public bool IsUnion(int a, int b)
        {
            a = Find(a);
            b = Find(b);

            if (a == b) return true;
            else return false;
        }
    }
}