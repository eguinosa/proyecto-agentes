using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agentes
{
    class Kid : Element
    {
        protected Random r;
        public int Id { get; protected set; }

        public Kid(int x, int y, int id) : base(x, y)
        {
            Id = id;
            r = new Random(DateTime.Now.Millisecond);
        }

        public void Move(int x, int y)
        {
            if (x == PosX && y == PosY) throw new Exception("The Kid is staying on the same position");
            if (Math.Abs(PosX - x) + Math.Abs(PosY - y) > 1) throw new Exception("A Kid can walk more than 1 step");

            PosX = x;
            PosY = y;
        }

        public void UpdateLocation(int x, int y, Robot kidnapper)
        {
            if (!kidnapper.HasKid) throw new Exception("The Robot doesn't have any Kid");
            if (kidnapper.Kid != this) throw new Exception("The Kid the kidnapper has, it's not me");

            PosX = x;
            PosY = y;
        }

        public (KidAction action, (int x, int y) pos) NextAction(Cell[,] env)
        {
            var posiblePos = new List<(int x, int y)>();

            foreach (var (x, y) in AdjacentPos)
            {
                if (x < 0 || y < 0 || x >= env.GetLength(0) || y >= env.GetLength(1)) continue;
                if (env[x, y] == null || env[x, y].HasObstacle || env[x, y].Empty)
                {
                    posiblePos.Add((x, y));
                }
            }

            if (posiblePos.Count == 0) return (KidAction.Stay, (PosX, PosY));

            int i = r.Next(posiblePos.Count);
            var nextPosition = posiblePos[i];
            return (KidAction.Move, nextPosition);
        }

        public override string ToString()
        {
            return "Kid<" + Id + ">(" + PosX.ToString() + ", " + PosY.ToString() + ")";
        }
    }
}
