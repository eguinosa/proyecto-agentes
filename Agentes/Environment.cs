using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Agentes
{
    class Environment
    {
        public int KidsInCorral => corrals.FindAll(e => e.HasKid).Count;
        public int TotalKids { get; protected set; }
        public int Count { get; protected set; }
        public bool Done { get; protected set; }
        public bool Success { get; protected set; }
        public bool Fired { get; protected set; }

        protected Random r;
        protected Robot agent;
        protected List<Kid> kids;
        protected List<Corral> corrals;
        protected Cell[,] env;

        protected long waitMilliSeconds;
        protected Stopwatch stopwatch;

        public Environment(int M, int N, int Kids, double percentDirt, double percObstacles, RobotType robot, long milliSeconds)
        {
            if (Kids < 1) throw new Exception("There has to be at least one kid");
            if (M < 0 || N < 0) throw new Exception("The Dimentions of the Map can't be negative");
            if (percentDirt >= 60) throw new Exception("The dirt has to be under 60%");

            //Checking everything fits on the map
            var cells = M * N;
            var kidCorrals = 2 * Kids;
            var newDirt = (int)Math.Round(percentDirt * cells / 100);
            var newObst = (int)Math.Round(percObstacles * cells / 100);
            if (cells < kidCorrals + newDirt + newObst + 1) throw new Exception("The Map can't fit all the elements");

            //Initializing Local Variables
            Count = 0;
            Done = false;
            Success = false;
            Fired = false;
            TotalKids = Kids;
            env = new Cell[M, N];
            r = new Random(DateTime.Now.Millisecond);
            waitMilliSeconds = milliSeconds;
            stopwatch = new Stopwatch();
            kids = new List<Kid>();
            corrals = new List<Corral>();

            //Adding the Corrals to the Map
            var corralCount = Kids;
            var width = (int)Math.Sqrt(Kids);
            var height = Kids / width;
            var remains = Kids % width;
            var extra = remains > 0 ? 1 : 0;

            var startX = r.Next(M);
            var startY = r.Next(N);
            startX = startX + width + extra < M ? startX : M - width - extra - 1;
            startY = startY + height < N ? startY : N - height - 1;

            for (int i = 0; i < width + extra; i++)
                for (int j = 0; j < height && corralCount > 0; j++)
                {
                    var corral = new Corral(startX + i, startY + j);
                    env[startX + i, startY + j] = new Cell(corral);
                    corrals.Add(corral);
                }

            //Saving the Rest of Available Positions
            var pos = new List<(int x, int y)>();
            for (int i = 0; i < M; i++)
                for (int j = 0; j < N; j++)
                {
                    if (env[i, j] == null) pos.Add((i, j));
                }

            //Adding Obstacles
            while (newObst > 0)
            {
                var i = r.Next(pos.Count);
                var obstacle = new Obstacle(pos[i].x, pos[i].y);
                env[pos[i].x, pos[i].y] = new Cell(obstacle);
                pos.RemoveAt(i);
                newObst--;
            }

            //Adding Dirt
            while (newDirt > 0)
            {
                var i = r.Next(pos.Count);
                var dirt = new Dirt(pos[i].x, pos[i].y);
                env[pos[i].x, pos[i].y] = new Cell(dirt);
                pos.RemoveAt(i);
                newDirt--;
            }

            //Add Kids
            var kidCount = 1;
            while (kidCount <= Kids)
            {
                var i = r.Next(pos.Count);
                var kid = new Kid(pos[i].x, pos[i].y, kidCount);
                env[pos[i].x, pos[i].y] = new Cell(kid);
                kids.Add(kid);
                pos.RemoveAt(i);
                kidCount++;
            }

            //Add Agent
            var t = r.Next(pos.Count);
            if (robot == RobotType.AgentA) agent = new AgentA(pos[t].x, pos[t].y, env);
            else agent = new AgentB(pos[t].x, pos[t].y, env);
            env[agent.PosX, agent.PosY] = new Cell(agent);
        }

        protected Environment(RobotType robot, Cell[,] enviroment)
        {

            //Copying Data from one Enviroment to the Other
            kids = new List<Kid>();
            corrals = new List<Corral>();
            env = new Cell[enviroment.GetLength(0), enviroment.GetLength(1)];
            var foundAgent = false;
            int agentPosX = -1, agentPosY = -1;

            for (int i = 0; i < enviroment.GetLength(0); i++)
                for (int j = 0; j < enviroment.GetLength(1); j++)
                {
                    if (enviroment[i, j] == null) continue;
                    else
                    {
                        if (enviroment[i, j].HasObstacle)
                        {
                            var obs = new Obstacle(i, j);
                            env[i, j] = new Cell(obs);
                        }
                        else if (enviroment[i, j].HasDirt)
                        {
                            var dirt = new Dirt(i, j);
                            env[i, j] = new Cell(dirt);
                        }
                        else if (enviroment[i, j].HasCorral)
                        {
                            var corral = new Corral(i, j);
                            env[i, j] = new Cell(corral);
                            corrals.Add(corral);
                        }
                        else if (enviroment[i, j].HasKid)
                        {
                            var kid = new Kid(i, j, enviroment[i, j].Kid.Id);
                            env[i, j] = new Cell(kid);
                            kids.Add(kid);
                        }
                        else if (enviroment[i, j].HasRobot)
                        {
                            foundAgent = true;
                            agentPosX = i;
                            agentPosY = j;
                        }
                    }
                }

            //Initializing Local Variables
            r = new Random(DateTime.Now.Millisecond);
            Count = 0;
            Done = false;
            Success = false;
            Fired = false;
            TotalKids = kids.Count;

            //Creating Agent
            if (!foundAgent) throw new Exception("Can't Copy a Simulation without the Agent");
            if (robot == RobotType.AgentA) agent = new AgentA(agentPosX, agentPosY, env);
            else agent = new AgentB(agentPosX, agentPosY, env);
            env[agent.PosX, agent.PosY] = new Cell(agent);
        }

        public Environment CopyStartEnviroment(RobotType robot)
        {
            if (Count > 0) throw new Exception("Can't Copy an Enviroment that already has started");
            var result = new Environment(robot, env);
            return result;
        }

        public List<string> Cicle()
        {
            //Checking the Simulation hasn't finished
            if (Done || Count > 100) return new List<string>() { "The Simulation has Finish, there is nothing to do" };

            //Measuring Time
            stopwatch.Restart();

            Count++;
            List<string> reports = new List<string>();
            string report;

            //Agent Action
            var (action, nextPos) = agent.Action(env);
            switch (action)
            {
                case RobotAction.Move:
                    report = "Moving: " + agent + " -> ";
                    MoveRobot(nextPos);
                    report += agent;
                    reports.Add(report);
                    break;
                case RobotAction.Clean:
                    if (env[agent.PosX, agent.PosY] == null) throw new Exception("This is an empty cell");
                    env[agent.PosX, agent.PosY].CleanCell(agent);
                    report = "Cleaning: " + agent;
                    reports.Add(report);
                    break;
                case RobotAction.Move_Leave:
                    //Moving
                    report = "Moving: " + agent + " -> ";
                    MoveRobot(nextPos);
                    report += agent;
                    reports.Add(report);
                    //Leaving Kid
                    if (env[agent.PosX, agent.PosY] == null) throw new Exception("This is an empty cell");
                    if (!env[agent.PosX, agent.PosY].HasCorral) throw new Exception("There is no Corral to leave a Kid");
                    report = "--> Giving Kid: " + agent + " to " + env[agent.PosX, agent.PosY].Corral;
                    reports.Add(report);
                    var kid = agent.DepositKid(env);
                    env[agent.PosX, agent.PosY].Corral.DepositKid(kid);
                    reports.Add("--> Received Kid: " + env[agent.PosX, agent.PosY].Corral);
                    break;
                case RobotAction.NothingToDo:
                    reports.Add("***Robot couldn't do anything on this cycle");
                    break;
                default:
                    throw new Exception("Unknown Action sent");
            }

            //The Actions of the Kids
            var visitedKids = new bool[kids.Count];
            for (int i = 0; i < kids.Count; i++)
            {
                var kid = kids[i];

                //Neutralised Kids with Robots or inside a Corral
                if (env[kid.PosX, kid.PosY] == null) throw new Exception("There are no Kids on this Cell");
                else if (env[kid.PosX, kid.PosY].HasRobot && env[kid.PosX, kid.PosY].Robot.HasKid)
                {
                    reports.Add("Trapped: " + kid + " - in Robot");
                    continue;
                }
                else if (env[kid.PosX, kid.PosY].HasCorral && env[kid.PosX, kid.PosY].Corral.HasKid)
                {
                    reports.Add("Trapped: " + kid + " - in Corral");
                    continue;
                }
                else if (!env[kid.PosX, kid.PosY].HasKid) throw new Exception("There are no Kids on this Cell");

                var (kidAction, (x, y)) = kid.NextAction(env);
                if (kidAction == KidAction.Stay)
                {
                    reports.Add("Stay: " + kid);
                    continue;
                }

                // *** KidAction is Move ***

                //Check if there is an Obstacle on the destination
                if (env[x, y] != null && env[x, y].HasObstacle && !PushObstacle(kid.PosX, kid.PosY, x, y))
                {
                    reports.Add("Tried to Move: " + kid + " -> " + env[x, y].Obstacle);
                    continue;
                }
                var lastX = kid.PosX;
                var lastY = kid.PosY;
                report = "Kid Moving: " + kid + " -> ";
                MoveKid(kid, x, y);
                report += kid;
                reports.Add(report);

                //Generate Dirt
                if (visitedKids[i]) continue;
                visitedKids[i] = true;
                GenerateDirt(lastX, lastY, i + 1, ref visitedKids);
            }

            reports.Add("------------------");
            reports.Add("Kids in Corral: " + KidsInCorral);
            reports.Add("Total Kids: " + TotalKids);

            var dirtyCells = PercentDirty();

            string percent = dirtyCells.ToString();
            percent = (percent.Length > 5 ? percent.Substring(0, 4) : percent) + "%";
            var (dirty, total) = EnvState();
            reports.Add("Dirty Cells: " + percent + " <" + dirty + " Dirty, " + (total - dirty) + " Clean>");

            //Checking the Simulation can continue
            Success = KidsInCorral == TotalKids && dirtyCells == 0;
            Done = Success || Count >= 100;
            if (KidsInCorral == TotalKids && dirtyCells == 0)
            {
                Success = true;
                Done = true;
            }
            else if (dirtyCells >= 60)
            {
                Fired = true;
                Done = true;
            }
            else if (Count >= 100)
            {
                Done = true;
            }

            //Fisnish Measuring Time
            stopwatch.Stop();

            var remains = waitMilliSeconds - stopwatch.ElapsedMilliseconds;
            if (remains > 0) Thread.Sleep((int) remains);

            return reports;
        }

        public (int dirty, int total) EnvState()
        {
            var total = 0;
            var dirty = 0;
            for (int i = 0; i < env.GetLength(0); i++)
                for (int j = 0; j < env.GetLength(1); j++)
                {
                    if (env[i, j] == null) total++;
                    else if (env[i, j].HasDirt)
                    {
                        total++;
                        dirty++;
                    }
                }
            return (dirty, total);
        }

        public double PercentDirty()
        {
            var (dirty, total) = EnvState();
            var result = (double)dirty / total;
            return result * 100;
        }

        public double PercentClean()
        {
            var (dirty, total) = EnvState();
            var clean = total - dirty;
            var result = (double)clean / total;
            return result;
        }

        public IEnumerable<string> Cells()
        {
            for (int i = 0; i < env.GetLength(0); i++)
                for (int j = 0; j < env.GetLength(1); j++)
                {
                    var cell = "Cell" + "(" + i + ", " + j + "): ";
                    if (env[i, j] == null || env[i, j].Empty) yield return cell + "Empty";
                    else yield return cell + env[i, j].ElementsToString();
                }
        }

        protected void MoveRobot((int x, int y)[] pos)
        {
            var (x, y) = pos[0];
            if (Element.Distance(x, y, agent.PosX, agent.PosY) != 1) throw new Exception("The Robot Has to Move one Step at a time");

            //Departure Cell
            if (env[agent.PosX, agent.PosY] == null) throw new Exception("This is an empty cell");
            env[agent.PosX, agent.PosY].ExitRobot();
            if (env[agent.PosX, agent.PosY].Empty) env[agent.PosX, agent.PosY] = null;

            //Arrival Cell
            agent.Move(x, y);
            if (env[x, y] == null) env[x, y] = new Cell(agent);
            else env[x, y].Add(agent);

            if (pos.Length == 1) return;

            //*******  Second Step *******
            (x, y) = pos[1];
            if (!agent.HasKid) throw new Exception("You need to have a Kid to move 2 steps");
            if (Element.Distance(x, y, agent.PosX, agent.PosY) != 1) throw new Exception("The Robot Has to Move one Step at a time");

            //Departure Cell
            if (env[agent.PosX, agent.PosY] == null) throw new Exception("This is an empty cell");
            env[agent.PosX, agent.PosY].ExitRobot();
            if (env[agent.PosX, agent.PosY].Empty) env[agent.PosX, agent.PosY] = null;

            //Arrival Cell
            agent.Move(x, y);
            if (env[x, y] == null) env[x, y] = new Cell(agent);
            else env[x, y].Add(agent);
        }

        protected void MoveKid(Kid kid, int x, int y)
        {
            if (Element.Distance(x, y, kid.PosX, kid.PosY) > 1) throw new Exception("A Kid can only move one position");
            if (env[x, y] != null) throw new Exception("A kid can't move to an occupied position");

            //Leaving Cell
            env[kid.PosX, kid.PosY].ExitKid();
            if (env[kid.PosX, kid.PosY].Empty) env[kid.PosX, kid.PosY] = null;

            //Arrival to new Cell
            kid.Move(x, y);
            env[x, y] = new Cell(kid);
        }

        protected bool PushObstacle(int fromX, int fromY, int obsX, int obsY)
        {
            if (Element.Distance(fromX, fromY, obsX, obsY) != 1) throw new Exception("The Pusher and the Obstacle need to be adjacents");
            if (env[obsX, obsY] == null || !env[obsX, obsY].HasObstacle) throw new Exception("There is no Obstacle on his supposed location");

            var obstacle = env[obsX, obsY].Obstacle;
            var adjacents = new (int x, int y)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            var (x, y) = adjacents.First(e => e.x + fromX == obsX && e.y + fromY == obsY);

            if (obsX + x < 0 || obsX + x >= env.GetLength(0) || obsY + y < 0 || obsY + y >= env.GetLength(1)) return false;
            else if (env[obsX + x, obsY + y] == null)
            {
                env[obsX + x, obsY + y] = new Cell(obstacle);
                env[obsX, obsY] = null;
                return true;
            }
            else if (env[obsX + x, obsY + y].HasObstacle)
            {
                var push = PushObstacle(obsX, obsY, obsX + x, obsY + y);
                if (push)
                {
                    env[obsX + x, obsY + y] = new Cell(obstacle);
                    env[obsX, obsY] = null;
                    return true;
                }
                else return false;
            }
            else return false;
        }

        protected void GenerateDirt(int x, int y, int startKid, ref bool[] visitedKids)
        {
            var count = 1;
            var ps = new (int i, int j)[] { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) };
            for (int i = startKid; i < kids.Count; i++)
            {
                if (visitedKids[i]) continue;
                foreach (var item in ps)
                {
                    if (kids[i].PosX == x + item.i && kids[i].PosY == y + item.j)
                    {
                        visitedKids[i] = true;
                        count++;
                        break;
                    }
                }
            }

            var dirtyCells = count == 1 ? 1 : count == 2 ? 3 : 6;
            var freePos = new List<(int x, int y)> { (x, y) };

            foreach (var (i, j) in ps)
            {
                if (x + i < 0 || x + i >= env.GetLength(0) || y + j < 0 || y + j >= env.GetLength(1)) continue;
                if (env[x + i, y + j] == null || env[x + i, y + j].Empty) freePos.Add((x + i, y + j));
            }

            while (freePos.Count > 0 && dirtyCells > 0)
            {
                var pos = r.Next(freePos.Count);
                var next = freePos[pos];
                var dirt = new Dirt(next.x, next.y);
                env[next.x, next.y] = new Cell(dirt);
                freePos.RemoveAt(pos);
                dirtyCells--;
            }
        }
    }
}
