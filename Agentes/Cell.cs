using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agentes
{
    class Cell
    {
        public Corral Corral { get; protected set; }
        public Dirt Dirt { get; protected set; }
        public Kid Kid { get; protected set; }
        public Obstacle Obstacle { get; protected set; }
        public Robot Robot { get; protected set; }

        public bool HasCorral => Corral != null;
        public bool HasDirt => Dirt != null;
        public bool HasKid => Kid != null;
        public bool HasObstacle => Obstacle != null;
        public bool HasRobot => Robot != null;
        public bool HasElement => HasCorral || HasDirt || HasKid || HasObstacle || HasRobot;
        public bool Empty => !HasElement;
        public bool HasCorralWithKid => HasCorral && Corral.HasKid;
        public bool HasRobotWithKid => HasRobot && Robot.HasKid;
        
        public Cell(Corral corral)
        {
            Corral = corral;
        }

        public Cell(Dirt dirt)
        {
            Dirt = dirt;
        }

        public Cell(Kid kid)
        {
            Kid = kid;
        }

        public Cell(Obstacle obstacle)
        {
            Obstacle = obstacle;
        }

        public Cell(Robot robot)
        {
            Robot = robot;
        }

        public void Add(Robot robot)
        {
            if (HasObstacle) throw new Exception("Can't go to a Cell with an Obstacle");
            if (HasCorral && Corral.HasKid) throw new Exception("Can't go to a Cell with a Corral and a Kid");
            if (HasKid && !robot.HasKid)
            {
                robot.AddKid(Kid);
                Kid = null;
            }
            Robot = robot;
        }

        public Kid ExitKid()
        {
            if (!HasKid) throw new Exception("There is no Kid on this Cell");
            var result = Kid;
            Kid = null;
            return result;
        }

        public Robot ExitRobot()
        {
            if (!HasRobot) throw new Exception("There is no Robot on this Cell");
            var result = Robot;
            Robot = null;
            return result;
        }

        public void CleanCell(Robot robot)
        {
            if (!HasDirt) throw new Exception("The Cell is Clean");
            if (robot != Robot) throw new Exception("The Robot is not on this Cell");
            Dirt = null;
        }

        public string ElementsToString()
        {
            if (Empty) return "";

            var elements = new List<string>();
            if (HasRobot) elements.Add(Robot.ToString());
            if (HasCorral) elements.Add(Corral.ToString()); 
            if (HasKid) elements.Add(Kid.ToString());
            if (HasDirt) elements.Add(Dirt.ToString());
            if (HasObstacle) elements.Add(Obstacle.ToString());

            var result = "[" + elements[0];
            for (int i = 1; i < elements.Count; i++)
            {
                result += ", " + elements[i];
            }
            result += "]";

            return result;
        }
    }
}
