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
            var posiblePos = PosibleMoves(env);

            if (posiblePos.Count == 0) return (RobotAction.NothingToDo, new (int, int)[] { });

            if (HasKid)
            {
                var posibleDouble = PosibleDoubleMoves(posiblePos, env);
                var corral = edgeCorrals.Count > 0 ? edgeCorrals[0] : normalCorrals[0];
                var distPos = new List<(int dist, (int x, int y)[] path)>();
                foreach (var path in posibleDouble)
                {
                    var dist = Distance(corral.PosX, corral.PosY, path[path.Length - 1].x, path[path.Length - 1].y);
                    distPos.Add((dist, path));
                }
                distPos.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));

                var (factible, d, closest) = FirstFactible(distPos, corral.PosX, corral.PosY, env);
                if (factible && d == 0) return (RobotAction.Move_Leave, closest);
                else if (factible) return (RobotAction.Move, closest);
                else return (RobotAction.NothingToDo, new (int, int)[] { });
            }
            else if (kidsEnv.Count > 0)
            {
                var kid = kidsEnv[0].kid;
                var distPos = new List<(int dist, (int x, int y))>();
                foreach (var pos in posiblePos)
                {
                    var distance = Distance(kid.PosX, kid.PosY, pos.x, pos.y);
                    distPos.Add((distance, pos));
                }
                distPos.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));

                var (factible, _, closest) = FirstFactible(distPos, kid.PosX, kid.PosY, env);
                if (factible) return (RobotAction.Move, new (int, int)[] { closest });
                else return (RobotAction.NothingToDo, new (int, int)[] { });
            }
            else if (dirtEnv.Count > 0)
            {
                var dirt = dirtEnv[0].dirt;
                if (Distance(dirt.PosX, dirt.PosY) == 0)
                {
                    return (RobotAction.Clean, new (int, int)[] { });
                }

                var distPos = new List<(int dist, (int x, int y))>();
                foreach (var pos in posiblePos)
                {
                    var distance = Distance(dirt.PosX, dirt.PosY, pos.x, pos.y);
                    distPos.Add((distance, pos));
                }
                distPos.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));

                var (factible, _, closest) = FirstFactible(distPos, dirt.PosX, dirt.PosY, env);
                if (factible) return (RobotAction.Move, new (int, int)[] { closest });
                else return (RobotAction.NothingToDo, new (int, int)[] { });
            }
            else
            {
                return (RobotAction.NothingToDo, new (int x, int y)[] { });
            }
        }
    }
}
