using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    public class World
    {
        public World world;
        public int worldSize;
        public Dictionary<int, Tank> Tanks;
        public Dictionary<int, Powerup> Powerups;
        public Dictionary<int, Wall> Walls;
        public Dictionary<int, Beam> Beams;
        public Dictionary<int, Projectile> Projectiles;
        public ControlCommands ControlCommands;
        public int tanksize { get; set; } = 60;
        public int wallsize { get; set; } = 50;


        public World()
        {
            worldSize = 1200;
            Tanks = new Dictionary<int, Tank>();
            Powerups = new Dictionary<int, Powerup>();
            Walls = new Dictionary<int, Wall>();
            Beams = new Dictionary<int, Beam>();
            Projectiles = new Dictionary<int, Projectile>();
            ControlCommands = new ControlCommands();
        }

        public void WorldSize(int _size) { worldSize = _size; }

        public Dictionary<int, Tank> getTanks() { return Tanks; }
        public Dictionary<int, Powerup> getPowerups() { return Powerups; }
        public Dictionary<int, Wall> getWalls() { return Walls; }
        public Dictionary<int, Beam> getBeams() { return Beams; }
        public Dictionary<int, Projectile> getProjectiles() { return Projectiles; }
        public ControlCommands getControlCommands() { return ControlCommands; }
        public int getWorldSize() { return worldSize; }
        public void getWorld() {  }

        public void addTank(Tank t)
        {
            Tanks.Add(t.tank, t);
        }
    }
}
