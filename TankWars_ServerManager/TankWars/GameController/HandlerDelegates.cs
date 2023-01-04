using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    public delegate void NetworkEventHandler();
    public delegate void ConnectionEventHandler(World w, int playerID);
    public delegate void BeamEventHandler(Beam b);
    public delegate void DeathEventHandler(Tank s);
    public delegate void NewTankEventHandler(Tank s);
    public delegate void UpdateFrame();
}
