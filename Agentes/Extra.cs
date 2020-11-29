using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agentes
{
    enum KidAction
    {
        Move,
        Stay
    }

    enum RobotAction
    {
        Move,
        Clean,
        Move_Leave,
        NothingToDo
    }

    enum RobotGoal
    {
        Clean,
        Kidnap,
        LeaveKid,
        Nothing
    }

    enum RobotType
    {
        AgentA,
        AgentB
    }
}
