using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agentes
{
    class Corral : Element
    {
        public Kid Kid { get; protected set; }

        public bool HasKid => Kid != null;

        public Corral(int x, int y) : base(x, y) { }

        public void DepositKid(Kid kid)
        {
            if (HasKid) throw new Exception("The Corral has already a Kid");
            Kid = kid;
        }

        public override string ToString()
        {
            string corral = "Corral(" + PosX.ToString() + ", " + PosY.ToString() + ")";
            if (HasKid) corral += " <<with " + Kid.ToString() + ">>";
            return corral;
        }
    }
}
