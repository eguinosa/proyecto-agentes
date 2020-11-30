using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agentes
{
    class AgentA : Robot
    {
        public AgentA(int x, int y, Cell[,] env) : base(x, y, env) { }

        protected override (RobotAction action, (int x, int y)[] pos) SelectAction(Cell[,] env)
        {
            if (HasKid && corralsEnv.Count > 0)
            {
                var (corral, path) = corralsEnv[0];

                if (actionGoal == RobotGoal.LeaveKid && actionGoalX == corral.PosX && actionGoalY == corral.PosY)
                {
                    if (actgoalPath.Count <= 2) return (RobotAction.Move_Leave, actgoalPath.ToArray());
                    var goalMove = actgoalPath.Take(2).ToArray();
                    actgoalPath.RemoveAt(0);
                    actgoalPath.RemoveAt(0);
                    return (RobotAction.Move, goalMove);
                }

                actionGoal = RobotGoal.LeaveKid;
                actionGoalX = corral.PosX;
                actionGoalY = corral.PosY;

                if (path.Count <= 2) return (RobotAction.Move_Leave, path.ToArray());

                var move = path.Take(2).ToArray();
                path.RemoveAt(0);
                path.RemoveAt(0);
                actgoalPath = path;
                return (RobotAction.Move, move);

            }
            else if (!HasKid && kidsEnv.Count > 0)
            {
                var (kid, path) = kidsEnv[0];

                if (actionGoal == RobotGoal.Kidnap && actionGoalX == kid.PosX && actionGoalY == kid.PosY)
                {
                    if (actgoalPath.Count == 1) return (RobotAction.Move, actgoalPath.ToArray());
                    var goalMove = actgoalPath.Take(1).ToArray();
                    actgoalPath.RemoveAt(0);
                    return (RobotAction.Move, goalMove);
                }

                actionGoal = RobotGoal.Kidnap;
                actionGoalX = kid.PosX;
                actionGoalY = kid.PosY;

                if (path.Count == 1) return (RobotAction.Move, path.ToArray());

                var move = path.Take(1).ToArray();
                path.RemoveAt(0);
                actgoalPath = path;
                return (RobotAction.Move, move);
            }
            else if (dirtEnv.Count > 0)
            {
                var (dirt, path) = dirtEnv[0];
                if (PosX == dirt.PosX && PosY == dirt.PosY) return (RobotAction.Clean, new (int, int)[] { });

                if (actionGoal == RobotGoal.Clean && actionGoalX == dirt.PosX && actionGoalY == dirt.PosY)
                {
                    if (actgoalPath.Count == 1) return (RobotAction.Move, actgoalPath.ToArray());
                    var goalMove = actgoalPath.Take(1).ToArray();
                    actgoalPath.RemoveAt(0);
                    return (RobotAction.Move, goalMove);
                }

                actionGoal = RobotGoal.Clean;
                actionGoalX = dirt.PosX;
                actionGoalY = dirt.PosY;

                if (path.Count == 1) return (RobotAction.Move, path.ToArray());

                var move = path.Take(1).ToArray();
                path.RemoveAt(0);
                actgoalPath = path;
                return (RobotAction.Move, move);

            }
            else
            {
                actionGoal = RobotGoal.Nothing;
                actionGoalX = -1;
                actionGoalY = -1;
                actgoalPath = null;
                return (RobotAction.NothingToDo, new (int x, int y)[] { });
            }
        }
    }
}
