using System;
using System.Collections.Generic;
using System.Linq;
using Graph;

namespace Graph
{
    public interface INode
    {
        object Data { get; set; }
        IEnumerable<IEdge> Incoming { get; }
        IEnumerable<IEdge> Outgoing { get; }
    }
    public interface IEdge
    {
        object Data { get; set; }
        INode From { get; }
        INode To { get; }
    }

    public static class Algorithms
    {
        public class DistanceData
        {
            public bool visited;
            public int distance;
            public INode prev;
        }
        public static void ShortestPath(IEnumerable<INode> nodes, INode start)
        {
            foreach (var s in nodes)
                s.Data = new DistanceData { visited = false, distance = int.MaxValue };
            (start.Data as DistanceData).distance = 0;

            Queue<INode> queue = new Queue<INode>();
            queue.Enqueue(start);
            int ctr = 0;
            while (queue.Any())
            {
                INode current = queue.Dequeue();
                if ((current.Data as DistanceData).visited)
                    continue;

                Console.WriteLine(ctr++ + ", " + queue.Count);

                foreach (var e in current.Outgoing)
                    if ((e.To.Data as DistanceData).distance > (current.Data as DistanceData).distance + 1)
                    {
                        (e.To.Data as DistanceData).distance = (current.Data as DistanceData).distance + 1;
                        (e.To.Data as DistanceData).prev = current;
                    }
                (current.Data as DistanceData).visited = true;

                foreach (var n in current.Outgoing.Select(s => s.To).Where(s => !(s.Data as DistanceData).visited))
                    queue.Enqueue(n);
            }
        }
    }
}

namespace Blocks
{
    public interface IBlock
    {
        int Fix { get; }
        int Variable { get; }
        int Size { get; }
        bool Occupies(int x, int y);
    }

    public class BlockV : IBlock, IEquatable<BlockV>
    {
        public int Fix { get; set; }
        public int Variable { get; set; }
        public int Size { get; set; }
        public bool Occupies(int x, int y) { return x == Fix && Variable <= y && y < Variable + Size; }
        public override int GetHashCode() { return (this as IBlock).Order(); }
        public BlockV Copy() { return (BlockV)MemberwiseClone(); }
        public override bool Equals(object obj) { return obj is BlockV && Equals(obj as BlockV); }
        public bool Equals(BlockV other) { return Fix == other.Fix && Variable == other.Variable && Size == other.Size; }
        public override string ToString() { return "(" + Fix + ", " + Variable + "): " + Size + ", true"; }
    }
    public class BlockH : IBlock, IEquatable<BlockH>
    {
        public int Fix { get; set; }
        public int Variable { get; set; }
        public int Size { get; set; }
        public bool Occupies(int x, int y) { return y == Fix && Variable <= x && x < Variable + Size; }
        public override int GetHashCode() { return (this as IBlock).Order(); }
        public BlockH Copy() { return (BlockH)MemberwiseClone(); }
        public override bool Equals(object obj) { return obj is BlockH && Equals(obj as BlockH); }
        public bool Equals(BlockH other) { return Fix == other.Fix && Variable == other.Variable && Size == other.Size; }
        public override string ToString() { return "(" + Variable + ", " + Fix + "): " + Size + ", false"; }
    }

    public class State : IEquatable<State>, INode
    {
        private int? hash;
        public int solveX = 1;
        public List<State> connects = new List<State>();
        public List<BlockV> verticals = new List<BlockV>();
        public List<BlockH> horizontals = new List<BlockH>();
        private IEnumerable<IBlock> blocks { get { return verticals.Cast<IBlock>().Concat(horizontals.Cast<IBlock>()); } }

        public State Copy()
        {
            var s = new State { solveX = this.solveX };
            foreach (var v in this.verticals)
                s.verticals.Add(v.Copy());
            foreach (var v in this.horizontals)
                s.horizontals.Add(v.Copy());
            return s;
        }

        public bool Occupies(int x, int y)
        {
            foreach (var v in blocks)
                if (v.Occupies(x, y))
                    return true;
            return y == 3 && (x == solveX || x == solveX + 1);
        }

        public bool IsSolution { get { return solveX == 5; } }
        public void AddConnection(State s) { connects.Add(s); }

        public bool Equals(State other)
        {
            if (solveX != other.solveX)
                return false;
            for (var i = 0; i < verticals.Count; i++)
                if (verticals[i].Variable != other.verticals[i].Variable)
                    return false;
            for (var i = 0; i < horizontals.Count; i++)
                if (horizontals[i].Variable != other.horizontals[i].Variable)
                    return false;

            return true;
        }
        public override string ToString()
        {
            string str = "";

            for (var y = 1; y <= 6; y++)
            {
                for (var x = 1; x <= 6; x++)
                    str += Occupies(x, y) ? "x" : ".";
                str += "\r\n";
            }
            return str;
        }

        public override int GetHashCode()
        {
            if (hash == null)
                hash = solveX + blocks.Select((e, i) => e.Variable << (i + 1)).Sum();
            return hash.Value;
        }
        public List<State> MakeConnections()
        {
            var newStates = new List<State>();
            var s = this;

            for (var i = 0; i < verticals.Count; i++)
            {
                var b = verticals[i];
                if (b.Variable > 1 && !s.Occupies(b.Fix, b.Variable - 1))
                {
                    var n = s.Copy();
                    n.verticals[i].Variable--;
                    newStates.Add(n);
                }
                if (b.Variable + b.Size < 7 && !s.Occupies(b.Fix, b.Variable + b.Size))
                {
                    var n = s.Copy();
                    n.verticals[i].Variable++;
                    newStates.Add(n);
                }
            }

            for (var i = 0; i < horizontals.Count; i++)
            {
                var b = horizontals[i];
                if (b.Variable > 1 && !s.Occupies(b.Variable - 1, b.Fix))
                {
                    var n = s.Copy();
                    n.horizontals[i].Variable--;
                    newStates.Add(n);
                }
                if (b.Variable + b.Size < 7 && !s.Occupies(b.Variable + b.Size, b.Fix))
                {
                    var n = s.Copy();
                    n.horizontals[i].Variable++;
                    newStates.Add(n);
                }
            }

            if (s.solveX > 1 && !s.Occupies(s.solveX - 1, 3))
            {
                var n = s.Copy();
                n.solveX--;
                newStates.Add(n);
            }
            if (s.solveX < 5 && !s.Occupies(s.solveX + 2, 3))
            {
                var n = s.Copy();
                n.solveX++;
                newStates.Add(n);
            }

            return newStates;
        }

        #region Graph

        private class Edge : IEdge
        {
            public object Data { get; set; }
            public INode From { get; set; }
            public INode To { get; set; }
        }

        public object Data { get; set; }
        public IEnumerable<IEdge> Incoming { get { return connects.Select(s => new Edge { From = s, To = this }).Cast<IEdge>(); } }
        public IEnumerable<IEdge> Outgoing { get { return connects.Select(s => new Edge { From = this, To = s }).Cast<IEdge>(); } }

        #endregion
    }

    public class Solver
    {
        private HashSet<State> states = new HashSet<State>();
        private State Find(State s)
        {
            if (states.Contains(s))
            {
                foreach (var v in states)
                    if (v.Equals(s))
                        return v;
            }
            return null;
        }

        public IEnumerable<State> States { get { return states; } }
        public void Solve(State start)
        {
            var stamp = DateTime.Now;
            var remains = new HashSet<State>();
            remains.Add(start);
            states.Add(start);

            while (remains.Any())
            {
                Console.WriteLine("{0}: {1} states collected, {2} in queue", DateTime.Now - stamp, states.Count, remains.Count);

                var s = remains.First();
                var conns = s.MakeConnections();

                foreach (var v in conns)
                {
                    var found = Find(v);
                    if (found == null)
                    {
                        s.AddConnection(v);
                        v.AddConnection(s);

                        states.Add(v);
                        remains.Add(v);
                    }
                    else
                    {
                        s.AddConnection(found);
                        found.AddConnection(s);
                    }
                }

                remains.Remove(s);
            }

            var end = new State { solveX = 0 };
            states.Add(end);
            foreach (var s in states.Where(p => p.IsSolution))
            {
                end.AddConnection(s);
                s.AddConnection(end);
            }
        }
    }

    public static class Exts
    {
        public static int Order(this IBlock b) { return b.Fix * 6 + b.Variable; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var start = MakeState();

            var solution = new Solver();
            solution.Solve(start);

            var vv = solution.States.Where(s => s.IsSolution).ToArray();
            var a1 = solution.States.Where(s => s.solveX == 1).ToArray();
            var b1 = solution.States.Where(s => s.solveX == 2).ToArray();
            var c1 = solution.States.Where(s => s.solveX == 3).ToArray();
            var d1 = solution.States.Where(s => s.solveX == 4).ToArray();
            var e1 = solution.States.Where(s => s.solveX == 5).ToArray();
            var f1 = solution.States.Where(s => s.solveX == 0).ToArray();

            Graph.Algorithms.ShortestPath(solution.States.Cast<INode>(), start);

            Console.WriteLine("Done! The shortest solution is " + (f1.First().Data as Graph.Algorithms.DistanceData).distance + " steps long");
        }

        /// <summary>
        /// Returns the starting state of the puzzle to solve
        /// </summary>
        public static State MakeState()
        {
            // Verticals: | och #
            // Horizontals: - och =
            // The red piece: xx
            // Empty: .

            // 100
            var str = new char[,]{
                {'-', '-', '-', '-', '#', '.'},
                {'-', '-', '|', '.', '#', '#'},
                {'x', 'x', '|', '.', '#', '#'},
                {'.', '|', '|', '-', '-', '#'},
                {'.', '|', '|', '|', '.', '.'},
                {'=', '=', '=', '|', '.', '.'},
            };

            // 88
            //var str = new char[,]{
            //    {'-', '-', '-', '|', '|', '|'},
            //    {'.', '.', '|', '|', '|', '|'},
            //    {'x', 'x', '|', '.', '.', '.'},
            //    {'.', '.', '|', '.', '.', '.'},
            //    {'.', '.', '|', '.', '-', '-'},
            //    {'.', '.', '.', '=', '=', '='},
            //};

            var st2 = new State();
            for (var x = 1; x <= 6; x++)
            {
                for (var y = 1; y <= 6; y++)
                {
                    var c = str[y - 1, x - 1];
                    if (c == 'x')
                    {
                        st2.solveX = x;
                        str[y - 1, x] = '.';
                        if (y != 3)
                            throw new Exception("Lösningsbiten måste vara på rad 3");
                    }
                    if (c == '=')
                    {
                        st2.horizontals.Add(new BlockH { Fix = y, Size = 3, Variable = x });
                        str[y - 1, x] = '.';
                        str[y - 1, x + 1] = '.';
                    }
                    if (c == '-')
                    {
                        st2.horizontals.Add(new BlockH { Fix = y, Size = 2, Variable = x });
                        str[y - 1, x] = '.';
                    }
                    if (c == '#')
                    {
                        st2.verticals.Add(new BlockV { Fix = x, Size = 3, Variable = y });
                        str[y, x - 1] = '.';
                        str[y + 1, x - 1] = '.';
                    }
                    if (c == '|')
                    {
                        st2.verticals.Add(new BlockV { Fix = x, Size = 2, Variable = y });
                        str[y, x - 1] = '.';
                    }
                }
            }

            return st2;
        }
    }
}
