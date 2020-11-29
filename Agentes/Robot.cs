using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agentes
{
    abstract class Robot : Element
    {
        public Kid Kid { get; protected set; }
        public bool HasKid => Kid != null;

        protected List<Corral> edgeCorrals;
        protected List<Corral> normalCorrals;
        protected List<(int dist, Kid kid)> kidsEnv;
        protected List<(int dist, Dirt dirt)> dirtEnv;

        public Robot(int x, int y, Cell[,] env) : base(x, y)
        {
            var newCorrals = new List<(bool xEdge, bool yEdge, Corral corral)>();

            for (int i = 0; i < env.GetLength(0); i++)
                for (int j = 0; j < env.GetLength(1); j++)
                {
                    if (env[i, j] != null && env[i, j].HasCorral)
                    {
                        var xEdge = i == 0 || i == env.GetLength(0) - 1;
                        xEdge = xEdge || (env[i - 1, j] != null && env[i - 1, j].HasObstacle);
                        xEdge = xEdge || (env[i + 1, j] != null && env[i + 1, j].HasObstacle);
                        var yEdge = j == 0 || j == env.GetLength(1) - 1;
                        yEdge = yEdge || (env[i, j - 1] != null && env[i, j - 1].HasObstacle);
                        yEdge = yEdge || (env[i, j + 1] != null && env[i, j + 1].HasObstacle);
                        newCorrals.Add((xEdge, yEdge, env[i, j].Corral));
                    }
                }

            var firstEdge = newCorrals.FindAll(e => e.xEdge && e.yEdge);
            var secondEdge = newCorrals.FindAll(e => e.xEdge || e.yEdge);
            var normalOnes = newCorrals.FindAll(e => !e.xEdge && !e.yEdge);
            edgeCorrals = new List<Corral>(firstEdge.Concat(secondEdge).Select(e => e.corral));
            normalCorrals = new List<Corral>(normalOnes.Select(e => e.corral));
        }

        public (RobotAction action, (int x, int y)[] pos) Action(Cell[,] env)
        {
            See(env);
            var (action, pos) = SelectAction(env);
            return (action, pos);
        }

        protected abstract (RobotAction action, (int x, int y)[] pos) SelectAction(Cell[,] env);

        protected void See(Cell[,] env)
        {
            var newCorrals = new List<(bool xEdge, bool yEdge, Corral corral)>();
            kidsEnv = new List<(int dist, Kid kid)>();
            dirtEnv = new List<(int dist, Dirt dirt)>();

            for (int x = 0; x < env.GetLength(0); x++)
                for (int y = 0; y < env.GetLength(1); y++)
                {
                    if (env[x, y] == null) continue;
                    if (env[x, y].HasKid)
                    {
                        var distance = Distance(x, y, PosX, PosY);
                        kidsEnv.Add((distance, env[x, y].Kid));
                    }
                    else if (env[x, y].HasDirt)
                    {
                        var distance = Distance(x, y, PosX, PosY);
                        dirtEnv.Add((distance, env[x, y].Dirt));
                    }
                    if (env[x, y].HasCorral && !env[x,y].Corral.HasKid)
                    {
                        var xEdge = x == 0 || x == env.GetLength(0) - 1;
                        xEdge = xEdge || (env[x - 1, y] != null && env[x - 1, y].HasObstacle);
                        xEdge = xEdge || (env[x + 1, y] != null && env[x + 1, y].HasObstacle);
                        xEdge = xEdge || (env[x - 1, y] != null && env[x - 1, y].HasCorral);
                        xEdge = xEdge || (env[x + 1, y] != null && env[x + 1, y].HasCorral);
                        var yEdge = y == 0 || y == env.GetLength(1) - 1;
                        yEdge = yEdge || (env[x, y - 1] != null && env[x, y - 1].HasObstacle);
                        yEdge = yEdge || (env[x, y + 1] != null && env[x, y + 1].HasObstacle);
                        yEdge = yEdge || (env[x, y - 1] != null && env[x, y - 1].HasCorral);
                        yEdge = yEdge || (env[x, y + 1] != null && env[x, y + 1].HasCorral);
                        newCorrals.Add((xEdge, yEdge, env[x, y].Corral));
                    }
                }

            kidsEnv.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));
            dirtEnv.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));

            //Corrals
            var firstEdge = newCorrals.FindAll(e => e.xEdge && e.yEdge);
            var secondEdge = newCorrals.FindAll(e => e.xEdge || e.yEdge);
            var normalOnes = newCorrals.FindAll(e => !e.xEdge && !e.yEdge);
            edgeCorrals = new List<Corral>(firstEdge.Concat(secondEdge).Select(e => e.corral));
            normalCorrals = new List<Corral>(normalOnes.Select(e => e.corral));
        }

        public void Move(int x, int y)
        {
            if (x == PosX && y == PosY) throw new Exception("The Robot is staying on the same position");
            if (Math.Abs(PosX - x) + Math.Abs(PosY - y) > 1) throw new Exception("A Robot has to walk one step at a time");

            PosX = x;
            PosY = y;

            if (HasKid) Kid.UpdateLocation(x, y, this);
        }

        public void AddKid(Kid kid)
        {
            if (HasKid) throw new Exception("The Robot has already a Kid");
            Kid = kid;
        }

        public Kid DepositKid(Cell[,] env)
        {
            if (!HasKid) throw new Exception("The Robot doesn't have a kid");
            if (!env[PosX, PosY].HasCorral || env[PosX, PosY].Corral.HasKid) throw new Exception("Incorrect Delivery of Kid");

            //Updating the Corrals without Kids
            var corral = edgeCorrals.Count > 0 ? edgeCorrals[0] : normalCorrals[0];
            if (env[PosX, PosY].Corral != corral) throw new Exception("Delivering to the Wrong Corral");

            var result = Kid;
            Kid = null;
            return result;
        }

        protected List<(int x, int y)> PosibleMoves(Cell[,] env)
        {
            var result = new List<(int, int)>();
            foreach (var (x, y) in AdjacentPos)
            {
                if (x < 0 || x >= env.GetLength(0) || y < 0 || y >= env.GetLength(1)) continue;
                if (env[x, y] != null && (env[x, y].HasObstacle || env[x, y].HasCorralWithKid)) continue;
                result.Add((x, y));
            }
            return result;
        }

        protected List<(int x, int y)[]> PosibleDoubleMoves(List<(int x, int y)> posibleFirst, Cell[,] env)
        {
            var already = new List<(int x, int y)>();
            var result = new List<(int x, int y)[]>(posibleFirst.Select(e => new (int, int)[] { e }));
            foreach (var (x, y) in posibleFirst)
            {
                foreach (var pos in Adjacents(x, y))
                {
                    if (pos.x == PosX && pos.y == PosY) continue;
                    if (posibleFirst.Contains(pos)) continue;
                    if (already.Contains(pos)) continue;
                    if (pos.x < 0 || pos.x >= env.GetLength(0) || pos.y < 0 || pos.y >= env.GetLength(1)) continue;
                    if (env[pos.x, pos.y] != null && (env[pos.x, pos.y].HasObstacle || env[pos.x, pos.y].HasCorralWithKid)) continue;
                    result.Add(new (int, int)[] { (x, y), pos });
                }
            }
            return result;
        }

        protected List<(int x, int y)> PosibleMoves(int posX, int posY, Cell[,] env)
        {
            var result = new List<(int, int)>();
            foreach (var (x, y) in Adjacents(posX, posY))
            {
                if (x < 0 || x >= env.GetLength(0) || y < 0 || y >= env.GetLength(1)) continue;
                if (env[x, y] != null && (env[x, y].HasObstacle || env[x, y].HasCorralWithKid)) continue;
                result.Add((x, y));
            }
            return result;
        }

        protected (bool factible, int dist, (int x, int y)) FirstFactible(List<(int, (int, int))> posDist, int goalX, int goalY, Cell[,] env)
        {
            foreach (var (d, (x, y)) in posDist)
            {
                var check = new bool[env.GetLength(0), env.GetLength(1)];
                check[PosX, PosY] = true;
                if (ExistsPath(x, y, goalX, goalY, env, ref check)) return (true, d, (x, y));
            }
            return (false, -1, (-1, -1));
        }

        protected (bool factible, int dist, (int x, int y)[] pos) FirstFactible(List<(int, (int x, int y)[] pos)> posDist, int goalX, int goalY, Cell[,] env)
        {
            foreach (var (d, pos) in posDist)
            {
                var x = pos[pos.Length - 1].x;
                var y = pos[pos.Length - 1].y;
                var check = new bool[env.GetLength(0), env.GetLength(1)];
                check[PosX, PosY] = true;
                if (pos.Length > 1) check[pos[0].x, pos[0].y] = true;
                if (ExistsPath(x, y, goalX, goalY, env, ref check)) return (true, d, pos);
            }
            return (false, -1, new (int, int)[] { });
        }
        
        protected bool ExistsPath(int x1, int y1, int x2, int y2, Cell[,] env, ref bool[,] check)
        {
            if (x1 == x2 && y1 == y2) return true;
            check[x1, y1] = true;

            //Creating the Next Moves
            var posibles = PosibleMoves(x1, y1, env);
            List<(int dist, (int, int))> disPos = posibles.Select(e => (Distance(e.x, e.y, x2, y2), e)).ToList();
            disPos.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));

            foreach (var (_, (x, y)) in disPos)
            {
                if (OutOfRange(x, y, env.GetLength(0), env.GetLength(1))) continue;
                if (check[x, y]) continue;
                if (ExistsPath(x, y, x2, y2, env, ref check)) return true;
            }

            return false;
        }

        public override string ToString()
        {
            string robot = "Robot(" + PosX.ToString() + ", " + PosY.ToString() + ")";
            if (HasKid) robot += " <<with " + Kid.ToString() + ">>";
            return robot;
        }
    }
}
