using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agentes
{
    abstract class Element
    {
        public int PosX { get; protected set; }
        public int PosY { get; protected set; }

        public (int x, int y)[] AdjacentPos => Adjacents(PosX, PosY);

        public Element(int x, int y)
        {
            PosX = x;
            PosY = y;
        }

        public int Distance(int posX, int posY)
        {
            var result = Distance(posX, posY, PosX, PosY);
            return result;
        }

        public static int Distance(int x1, int y1, int x2, int y2)
        {
            var result = Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
            return result;
        }

        public static int Distance(Element a, Element b)
        {
            var result = Distance(a.PosX, a.PosY, b.PosX, b.PosY);
            return result;
        }

        public static (int x, int y)[] Adjacents(int x, int y)
        {
            var adjacents = new (int x, int y)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            var result = adjacents.Select(e => (e.x + x, e.y + y)).ToArray();
            return result;
        }

        public static bool OutOfRange(int x, int y, int dimensionX, int dimensionY)
        {
            if (x < 0 || x >= dimensionX) return true;
            if (y < 0 || y >= dimensionY) return true;
            return false;
        }
    }
}
