using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agentes
{
    class Obstacle: Element
    {
        public Obstacle(int x, int y) : base(x, y) { }

        public override string ToString()
        {
            return "Obstacle(" + PosX.ToString() + ", " + PosY.ToString() + ")";
        }
    }
}
