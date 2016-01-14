using System.Collections.Generic;

namespace ImagePuzzle
{
    internal class BoardState
    {
        public List<int> moves;
        public int[] positions;
        public int moveTo;
        public int distance; // distance according to priority queue

        public BoardState(List<int> m, int[] p, int to, int d)
        {
            moves = m;
            positions = p;
            moveTo = to;
            distance = d;
        }

    }
}