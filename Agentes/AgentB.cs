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
        protected List<(int x, int y)> goalPath;

        public AgentB(int x, int y, Cell[,] env) : base(x, y, env)
        {
            goal = RobotGoal.Nothing;
            goalX = -1;
            goalY = -1;
            goalPath = null;
        }

        protected override (RobotAction action, (int x, int y)[] pos) SelectAction(Cell[,] env)
        {
            //If his Cell it's dirty, He will clean it
            if (env[PosX, PosY].HasDirt)
            {
                actionGoal = RobotGoal.Nothing;
                actionGoalX = -1;
                actionGoalY = -1;
                actgoalPath = null;
                return (RobotAction.Clean, new (int, int)[] { });
            }

            //Update the Goals of the Robot
            ReviewGoal();
            if (goal == RobotGoal.Nothing) return (RobotAction.NothingToDo, new (int, int)[] { });

            //Robot has a Kid
            if (goal == RobotGoal.LeaveKid)
            {
                if (actionGoal == RobotGoal.Clean && InBetweenGoal(actionGoalX, actionGoalY))
                {
                    if (actgoalPath.Count <= 2) return (RobotAction.Move, actgoalPath.ToArray());
                    var cleanMove = actgoalPath.Take(2).ToArray();
                    actgoalPath.RemoveAt(0);
                    actgoalPath.RemoveAt(0);
                    return (RobotAction.Move, cleanMove);
                }
                if (actionGoal == RobotGoal.LeaveKid && actionGoalX == goalX && actionGoalY == goalY)
                {
                    if (actgoalPath.Count <= 2) return (RobotAction.Move_Leave, actgoalPath.ToArray());
                    var goalMove = actgoalPath.Take(2).ToArray();
                    actgoalPath.RemoveAt(0);
                    actgoalPath.RemoveAt(0);
                    return (RobotAction.Move, goalMove);
                }

                //Find if I can Move to a Corral on this Cicle
                if (goalPath.Count <= 2) return (RobotAction.Move_Leave, goalPath.ToArray());

                //See if there is any cell between the Robot and the goals
                var dirtInWay = dirtEnv.Where(e => InBetweenGoal(e.dirt.PosX, e.dirt.PosY)).ToList();
                if (dirtInWay.Count > 0)
                {
                    var (dirt, pathDirt) = dirtInWay[0];

                    if (pathDirt.Count <= 2) return (RobotAction.Move, pathDirt.ToArray());

                    var dirtMove = pathDirt.Take(2).ToArray();
                    pathDirt.RemoveAt(0);
                    pathDirt.RemoveAt(0);

                    actionGoal = RobotGoal.Clean;
                    actionGoalX = dirt.PosX;
                    actionGoalY = dirt.PosY;
                    actgoalPath = pathDirt;
                    return (RobotAction.Move, dirtMove);
                }

                var corralMove = goalPath.Take(2).ToArray();
                goalPath.RemoveAt(0);
                goalPath.RemoveAt(0);

                actionGoal = RobotGoal.LeaveKid;
                actionGoalX = goalX;
                actionGoalY = goalY;
                actgoalPath = goalPath;
                return (RobotAction.Move, corralMove);
            }
            else if (goal == RobotGoal.Kidnap)
            {
                if (actionGoal == RobotGoal.Clean && InBetweenGoal(actionGoalX, actionGoalY))
                {
                    if (actgoalPath.Count == 1) return (RobotAction.Move, actgoalPath.ToArray());
                    var cleanMove = actgoalPath.Take(1).ToArray();
                    actgoalPath.RemoveAt(0);
                    return (RobotAction.Move, cleanMove);
                }
                if (actionGoal == RobotGoal.Kidnap && actionGoalX == goalX && actionGoalY == goalY)
                {
                    if (actgoalPath.Count == 1) return (RobotAction.Move, actgoalPath.ToArray());
                    var goalMove = actgoalPath.Take(1).ToArray();
                    actgoalPath.RemoveAt(0);
                    return (RobotAction.Move, goalMove);
                }

                //Find if I can Move to Kid on this Cicle
                if (goalPath.Count == 1) return (RobotAction.Move, goalPath.ToArray());

                //See if there is any cell between the Robot and the goals
                var dirtInWay = dirtEnv.Where(e => InBetweenGoal(e.dirt.PosX, e.dirt.PosY)).ToList();
                if (dirtInWay.Count > 0)
                {
                    var (dirt, pathDirt) = dirtInWay[0];

                    if (pathDirt.Count == 1) return (RobotAction.Move, pathDirt.ToArray());
                    var dirtMove = pathDirt.Take(1).ToArray();
                    pathDirt.RemoveAt(0);

                    actionGoal = RobotGoal.Clean;
                    actionGoalX = dirt.PosX;
                    actionGoalY = dirt.PosY;
                    actgoalPath = pathDirt;
                    return (RobotAction.Move, dirtMove);
                }
                var kidMove = goalPath.Take(1).ToArray();
                goalPath.RemoveAt(0);

                actionGoal = RobotGoal.Kidnap;
                actionGoalX = goalX;
                actionGoalY = goalY;
                actgoalPath = goalPath;
                return (RobotAction.Move, kidMove);
            }
            else //goal == Clean
            {
                //if (HasKid)
                //{
                //    if (goalPath.Count <= 2) return (RobotAction.Move, goalPath.ToArray());
                //    var dirtMoveDouble = goalPath.Take(2).ToArray();
                //    goalPath.RemoveAt(0);
                //    goalPath.RemoveAt(0);

                //    actionGoal = RobotGoal.Clean;
                //    actionGoalX = goalX;
                //    actionGoalY = goalY;
                //    actgoalPath = goalPath;
                //    return (RobotAction.Move, dirtMoveDouble);
                //}

                if (goalPath.Count == 1) return (RobotAction.Move, goalPath.ToArray());
                var dirtMove = goalPath.Take(1).ToArray();
                goalPath.RemoveAt(0);

                actionGoal = RobotGoal.Clean;
                actionGoalX = goalX;
                actionGoalY = goalY;
                actgoalPath = goalPath;
                return (RobotAction.Move, dirtMove);
            }
        }

        protected void ReviewGoal()
        {
            if (HasKid && corralsEnv.Count > 0)
            {
                goal = RobotGoal.LeaveKid;
                goalX = corralsEnv[0].corral.PosX;
                goalY = corralsEnv[0].corral.PosY;
                goalPath = corralsEnv[0].path;
            }
            else if (!HasKid && kidsEnv.Count > 0)
            {
                goal = RobotGoal.Kidnap;
                goalX = kidsEnv[0].kid.PosX;
                goalY = kidsEnv[0].kid.PosY;
                goalPath = kidsEnv[0].path;
            }
            else if (dirtEnv.Count > 0)
            {
                goal = RobotGoal.Clean;
                goalX = dirtEnv[0].dirt.PosX;
                goalY = dirtEnv[0].dirt.PosY;
                goalPath = dirtEnv[0].path;
            }
            else //No goals
            {
                goal = RobotGoal.Nothing;
                goalX = -1;
                goalY = -1;
                goalPath = null;
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
