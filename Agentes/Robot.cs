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

        protected List<(Corral corral, List<(int, int)> path)> corralsEnv;
        protected List<(Kid kid, List<(int, int)> path)> kidsEnv;
        protected List<(Dirt dirt, List<(int, int)> path)> dirtEnv;

        protected RobotGoal actionGoal;
        protected int actionGoalX;
        protected int actionGoalY;
        protected List<(int, int)> actgoalPath;

        //Detect Going in Circles
        protected int LastPosX;
        protected int LastPosY;
        protected bool circles;

        public Robot(int x, int y, Cell[,] env) : base(x, y)
        {
            //Action Goals Variables
            actionGoal = RobotGoal.Nothing;
            actionGoalX = -1;
            actionGoalY = -1;
            actgoalPath = null;

            //Detect Going in Circles
            LastPosX = -1;
            LastPosY = -1;
            circles = false;
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
            kidsEnv = new List<(Kid, List<(int, int)>)>();
            dirtEnv = new List<(Dirt, List<(int, int)>)>();
            var newCorrals = new List<(Corral corral, int edge, List<(int, int)> path)>();

            for (int x = 0; x < env.GetLength(0); x++)
                for (int y = 0; y < env.GetLength(1); y++)
                {
                    if (env[x, y] == null) continue;
                    if (env[x, y].HasKid)
                    {
                        var kid = env[x, y].Kid;
                        var (exist, path) = Path(kid.PosX, kid.PosY, env);
                        if (exist) kidsEnv.Add((kid, path));
                    }
                    else if (env[x, y].HasDirt)
                    {
                        var dirt = env[x, y].Dirt;
                        var (exist, path) = Path(dirt.PosX, dirt.PosY, env);
                        if (exist) dirtEnv.Add((dirt, path));
                    }
                    if (env[x, y].HasCorral && !env[x, y].Corral.HasKid)
                    {
                        var corral = env[x, y].Corral;
                        var (exist, path) = Path(corral.PosX, corral.PosY, env);
                        if (exist)
                        {
                            var edge = 0;
                            if (x == 0 || (env[x - 1, y] != null && (env[x - 1, y].HasObstacle || env[x - 1, y].HasCorralWithKid))) edge++;
                            if (x == env.GetLength(0) - 1 || (env[x + 1, y] != null && (env[x + 1, y].HasObstacle || env[x + 1, y].HasCorralWithKid))) edge++;
                            if (y == 0 || (env[x, y - 1] != null && (env[x, y - 1].HasObstacle || env[x, y - 1].HasCorralWithKid))) edge++;
                            if (y == env.GetLength(1) - 1 || (env[x, y + 1] != null && (env[x, y + 1].HasObstacle || env[x, y + 1].HasCorralWithKid))) edge++;
                            newCorrals.Add((env[x, y].Corral, edge, path));
                        }
                    }
                }

            kidsEnv.Sort((e1, e2) => e1.path.Count.CompareTo(e2.path.Count));
            dirtEnv.Sort((e1, e2) => e1.path.Count.CompareTo(e2.path.Count));

            //Corrals
            var oneEdge = newCorrals.FindAll(e => e.edge >= 3);
            oneEdge.Sort((e1, e2) => e1.path.Count.CompareTo(e2.path.Count));
            var twoEdge = newCorrals.FindAll(e => e.edge == 2);
            twoEdge.Sort((e1, e2) => e1.path.Count.CompareTo(e2.path.Count));
            var threeEdge = newCorrals.FindAll(e => e.edge == 1);
            threeEdge.Sort((e1, e2) => e1.path.Count.CompareTo(e2.path.Count));
            var fourEdge = newCorrals.FindAll(e => e.edge == 0);
            fourEdge.Sort((e1, e2) => e1.path.Count.CompareTo(e2.path.Count));

            corralsEnv = oneEdge.Concat(twoEdge).Concat(threeEdge).Concat(fourEdge).Select(e => (e.corral, e.path)).ToList(); 
        }

        public void Move(int x, int y)
        {
            if (x == PosX && y == PosY) throw new Exception("The Robot is staying on the same position");
            if (Math.Abs(PosX - x) + Math.Abs(PosY - y) > 1) throw new Exception("A Robot has to walk one step at a time");

            //Detect Going in Circles
            if (LastPosX == x && LastPosY == y)
            {
                circles = true;
            }
            LastPosX = PosX;
            LastPosY = PosY;

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
            //if (!env[PosX, PosY].HasCorral || env[PosX, PosY].Corral.HasKid) throw new Exception("Incorrect Delivery of Kid");

            //Updating the Corrals without Kids
            var corral = corralsEnv[0].corral;
            if (env[PosX, PosY].Corral != corral) throw new Exception("Delivering to the Wrong Corral");

            var result = Kid;
            Kid = null;
            return result;
        }

        protected (bool exist, List<(int x, int y)> path) Path(int goalX, int goalY, Cell[,] env)
        {
            if (PosX == goalX && PosY == goalY) return (true, new List<(int, int)>());

            //Creating the Next Moves
            var posibles = PosibleMoves(env);
            var existOne = false;
            var minDist = int.MaxValue;
            List<(int, int)> path = null;

            foreach (var (x, y) in posibles)
            {
                var check = new bool[env.GetLength(0), env.GetLength(1)];
                check[PosX, PosY] = true;
                var (exist, newPath) = ExistPath(x, y, goalX, goalY, env, ref check);
                if (exist)
                {
                    existOne = true;
                    newPath.Insert(0, (x, y));
                    if (minDist > newPath.Count)
                    {
                        minDist = newPath.Count;
                        path = newPath;
                    }
                }
            }
            return (existOne, path);
        }

        protected static (bool exist, List<(int x, int y)> path) ExistPath(int x1, int y1, int x2, int y2, Cell[,] env, ref bool[,] check)
        {
            if (x1 == x2 && y1 == y2) return (true, new List<(int, int)>());
            check[x1, y1] = true;

            //Creating the Next Moves
            var posibles = PosibleMoves(x1, y1, env);
            List<(int dist, (int, int))> disPos = posibles.Select(e => (Distance(e.x, e.y, x2, y2), e)).ToList();
            disPos.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));

            foreach (var (_, (x, y)) in disPos)
            {
                if (OutOfRange(x, y, env.GetLength(0), env.GetLength(1))) continue;
                if (check[x, y]) continue;

                var (exists, path) = ExistPath(x, y, x2, y2, env, ref check);
                if (!exists) continue;
                path.Insert(0, (x, y));
                return (true, path);
            }

            return (false, null);
        }

        protected (bool exist, int dist) FindPath(int goalX, int goalY, Cell[,] env)
        {
            if (PosX == goalX && PosY == goalY) return (true, 0);

            //Creating the Next Moves
            var posibles = PosibleMoves(env);
            var existOne = false;
            var minDist = int.MaxValue;

            foreach (var (x, y) in posibles)
            {
                var check = new bool[env.GetLength(0), env.GetLength(1)];
                check[PosX, PosY] = true;
                var (exist, dist) = FindPath(x, y, goalX, goalY, env, ref check);
                if (exist)
                {
                    dist++;
                    existOne = true;
                    minDist = dist < minDist ? dist : minDist;
                }
            }
            return (existOne, minDist);
        }

        protected static (bool exist, int dist) FindPath(int x1, int y1, int x2, int y2, Cell[,] env, ref bool[,] check)
        {
            if (x1 == x2 && y1 == y2) return (true, 0);
            check[x1, y1] = true;

            //Creating the Next Moves
            var posibles = PosibleMoves(x1, y1, env);
            List<(int dist, (int, int))> disPos = posibles.Select(e => (Distance(e.x, e.y, x2, y2), e)).ToList();
            disPos.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));

            foreach (var (_, (x, y)) in disPos)
            {
                if (check[x, y]) continue;
                var (exist, dist) = FindPath(x, y, x2, y2, env, ref check);
                if (exist) return (true, dist + 1);
            }

            return (false, int.MaxValue);
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

        protected static List<(int x, int y)> PosibleMoves(int posX, int posY, Cell[,] env)
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

        //protected List<(int x, int y)[]> PosibleDoubleMoves(List<(int x, int y)> posibleFirst, Cell[,] env)
        //{
        //    var already = new List<(int x, int y)>();
        //    var result = new List<(int x, int y)[]>(posibleFirst.Select(e => new (int, int)[] { e }));
        //    foreach (var (x, y) in posibleFirst)
        //    {
        //        foreach (var pos in Adjacents(x, y))
        //        {
        //            if (pos.x == PosX && pos.y == PosY) continue;
        //            if (posibleFirst.Contains(pos)) continue;
        //            if (already.Contains(pos)) continue;
        //            if (pos.x < 0 || pos.x >= env.GetLength(0) || pos.y < 0 || pos.y >= env.GetLength(1)) continue;
        //            if (env[pos.x, pos.y] != null && (env[pos.x, pos.y].HasObstacle || env[pos.x, pos.y].HasCorralWithKid)) continue;
        //            result.Add(new (int, int)[] { (x, y), pos });
        //        }
        //    }
        //    return result;
        //}

        public override string ToString()
        {
            string robot = "Robot(" + PosX.ToString() + ", " + PosY.ToString() + ")";
            if (HasKid) robot += " <<with " + Kid.ToString() + ">>";
            return robot;
        }
    }
}
