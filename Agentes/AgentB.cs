using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agentes
{
    class AgentB : Robot
    {
        protected RobotGoal goal;
        protected int goalX;
        protected int goalY;
        protected int goalDist;

        public AgentB(int x, int y, Cell[,] env) : base(x, y, env)
        {
            goal = RobotGoal.Nothing;
            goalX = -1;
            goalY = -1;
            goalDist = int.MaxValue;
        }

        protected override (RobotAction action, (int x, int y)[] pos) SelectAction(Cell[,] env)
        {
            //If his Cell it's dirty, He will clean it
            if (env[PosX, PosY].HasDirt)
            {
                return (RobotAction.Clean, new (int, int)[] { });
            }

            //Otherwise, find the posible moves and goals
            ReviewGoal();
            var posiblePos = PosibleMoves(env);

            //Nothing to do or no place to go
            if (goal == RobotGoal.Nothing || posiblePos.Count == 0) return (RobotAction.NothingToDo, new (int, int)[] { });

            //Robot has a Kid
            if (goal == RobotGoal.LeaveKid)
            {
                var posibleDouble = PosibleDoubleMoves(posiblePos, env);
                //Sort the posible Positions depending on how far are they from the goal
                var distPosCorral = new List<(int dist, (int x, int y)[] path)>();
                foreach (var path in posibleDouble)
                {
                    var dist = Distance(goalX, goalY, path[path.Length - 1].x, path[path.Length - 1].y);
                    distPosCorral.Add((dist, path));
                }
                distPosCorral.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));
                
                var (factCorral, d, closestCorral) = FirstFactible(distPosCorral, goalX, goalY, env);
                if (factCorral && d == 0) return (RobotAction.Move_Leave, closestCorral);

                //See if there is any cell between the Robot and the goals
                var dirtInWay = dirtEnv.Where(e => InBetweenGoal(e.dirt.PosX, e.dirt.PosY)).ToList();
                if (dirtInWay.Count > 0)
                {
                    var dirt = dirtInWay[0].dirt;
                    var distPosDirt = new List<(int dist, (int x, int y)[] path)>();
                    foreach (var path in posibleDouble)
                    {
                        var dist = Distance(dirt.PosX, dirt.PosY, path[path.Length - 1].x, path[path.Length - 1].y);
                        distPosDirt.Add((dist, path));
                    }
                    distPosDirt.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));

                    var (factDirt,_, closestDirt) = FirstFactible(distPosDirt, dirt.PosX, dirt.PosY, env);
                    if(factDirt) return (RobotAction.Move, closestDirt);
                }

                //No In Between Cell to clean -> Return the position closest to the Corral
                if(factCorral) return (RobotAction.Move, closestCorral);
                else return (RobotAction.NothingToDo, new (int, int)[] { });
            }
            else if (goal == RobotGoal.Kidnap)
            {
                //See If The Robot Can Reach the Kid on this Move
                var distPosKid = new List<(int dist, (int x, int y))>();
                foreach (var pos in posiblePos)
                {
                    var distance = Distance(goalX, goalY, pos.x, pos.y);
                    distPosKid.Add((distance, pos));
                }
                distPosKid.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));

                var (factKid, d, closestKid) = FirstFactible(distPosKid, goalX, goalY, env);
                if (factKid && d == 0) return (RobotAction.Move, new (int, int)[] { closestKid });

                //See if there is any cell between the Robot and the goals
                var dirtInWay = dirtEnv.Where(e => InBetweenGoal(e.dirt.PosX, e.dirt.PosY)).ToList();
                if (dirtInWay.Count > 0)
                {
                    var dirt = dirtInWay[0].dirt;
                    var distPosDirt = new List<(int dist, (int x, int y))>();
                    foreach (var path in posiblePos)
                    {
                        var dist = Distance(dirt.PosX, dirt.PosY, path.x, path.y);
                        distPosDirt.Add((dist, path));
                    }
                    distPosDirt.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));

                    var (factDirt, _, closestDirt) = FirstFactible(distPosDirt, dirt.PosX, dirt.PosY, env);
                    if (factDirt) return (RobotAction.Move, new (int, int)[] { closestDirt });
                }

                //No In Between Cell to clean -> Return the position closest to the Corral
                if (factKid) return (RobotAction.Move, new (int, int)[] { closestKid });
                else return (RobotAction.NothingToDo, new (int, int)[] { });
            }
            else //goal == Clean
            {
                var distPos = new List<(int dist, (int x, int y))>();
                foreach (var pos in posiblePos)
                {
                    var distance = Distance(goalX, goalY, pos.x, pos.y);
                    distPos.Add((distance, pos));
                }
                distPos.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));

                var (factible, _, closest) = FirstFactible(distPos, goalX, goalY, env);
                if(factible) return (RobotAction.Move, new (int, int)[] { closest });
                else return (RobotAction.NothingToDo, new (int, int)[] { });
            }
        }

        protected void ReviewGoal()
        {
            if (HasKid)
            {
                goal = RobotGoal.LeaveKid;
                goalX = edgeCorrals.Count > 0 ? edgeCorrals[0].PosX : normalCorrals[0].PosX;
                goalY = edgeCorrals.Count > 0 ? edgeCorrals[0].PosY : normalCorrals[0].PosY;
                goalDist = Distance(goalX, goalY);
            }
            else if (kidsEnv.Count > 0)
            {
                goal = RobotGoal.Kidnap;
                goalX = kidsEnv[0].kid.PosX;
                goalY = kidsEnv[0].kid.PosY;
                goalDist = kidsEnv[0].dist;
            }
            else if (dirtEnv.Count > 0)
            {
                goal = RobotGoal.Clean;
                goalX = dirtEnv[0].dirt.PosX;
                goalY = dirtEnv[0].dirt.PosY;
                goalDist = dirtEnv[0].dist;
            }
            else //No goals
            {
                goal = RobotGoal.Nothing;
                goalX = -1;
                goalY = -1;
                goalDist = int.MaxValue;
            }
        }

        protected bool InBetweenGoal(int x, int y)
        {
            if (PosX <= x && x <= goalX && PosY <= y && y <= goalY) return true;
            if (goalX <= x && x <= PosX && goalY <= y && y <= PosY) return true;
            return false;
        }
    }
}
