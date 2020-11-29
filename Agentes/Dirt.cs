using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agentes
{
    class Dirt : Element
    {
        public Dirt(int x, int y) : base(x, y) { }

        public override string ToString()
        {
            return "Dirt(" + PosX.ToString() + ", " + PosY.ToString() + ")";
        }
    }
}
