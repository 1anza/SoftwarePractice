using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml;
using NetworkUtil;
using Newtonsoft.Json;
using System.Threading;

namespace TankWars
{
    public class ServerController
    {
        private World world;

        private Dictionary<int, SocketState> clients = new Dictionary<int, SocketState>();
        private List<int> dcClients = new List<int>();

        /// <summary>
        /// Keeps track of when new frames are drawn and any frameDelay 
        /// from the server before sending to connected clients. 
        /// </summary>
        private int newFrame;
        private int frameDelay = 0;

        /// <summary>
        /// Tracks tank status indicators such as speed, hp, size, and respawn rate.
        /// There is also a list which holds the dead tanks in queue to respawn. 
        /// </summary>
        private int tankSpeed;
        private int tankHealth;
        private int tankSize;
        private int tankRespawn;
        private List<int> tankRespawnList = new List<int>();

        /// <summary>
        /// Tracks projectile ID, speed, and firing rate. 
        /// List holds fired projectiles that are in queue to be removed. 
        /// </summary>
        private int projectileID = 0;
        private int projectileSpeed = 0;
        private int projectileRate = 0;
        private List<int> removeProjectile = new List<int>();

        /// <summary>
        /// Tracks powerup ID, how many powerups are in the world, the number of allowed powerups, and the delay between powerup respawns.
        /// List holds collected powerups to be removed.
        /// </summary>
        private int powerupID = 0;
        private int powerupNum = 0;
        private int powerupRespawn = 0;
        private int powerupWait = 0;
        private List<int> removePowerup = new List<int>();

        private int wallSize = 0;
        private int beamID = 0;

        //Tracks if a location is in the world
        private bool TrackLocation(Vector2D loc) => -world.getWorldSize() / 2 <= loc.GetX() && loc.GetX() <=
                                                    world.getWorldSize() / 2 && -world.getWorldSize() / 2 <=
                                                    loc.GetY() && loc.GetY() <= world.getWorldSize() / 2;

        /// <summary>
        /// Gets variables and data from xml file.
        /// </summary>
        /// <param name="file"></param>
        public ServerController(string file) { ReadSettingXmlFile(file); }

        /// <summary>
        /// Begins event loop that accepts incoming clients given port, 
        /// then creates new threads to send data clients each updated frame of the world
        /// </summary>
        /// <param name="port"></param>
        public void InitializeServer(int port)
        {
            Networking.StartServer(NewClient, port);
            Thread t = new Thread(UpdateFrame);
            t.Start();
        }


        /// <summary>
        /// Method that waits for the last frame to finish, once finished it updates the world.
        /// </summary>
        /// <param name="o"></param>
        private void UpdateFrame(object o)
        {
            Stopwatch w = new Stopwatch();

            while (true)
            {
                w.Start();
                while (w.ElapsedMilliseconds < frameDelay) { }
                w.Reset();

                UpdateWorld();
            }
        }


        /// <summary>
        /// Updates the world's tanks, powerups, projectiles, and beams. 
        /// These updates include any collisions that may have occured in the world.
        /// Sends the commands received by the game Controller to all connected clients.
        /// </summary>
        private void UpdateWorld()
        {
            GenerateNewPowerup();
            foreach (Powerup powerup in world.Powerups.Values)
                CollisionDetection.ObjectToTankCollision(powerup, world, removePowerup);

            foreach (Projectile proj in world.Projectiles.Values)
                UpdateProjectile(proj);

            foreach (Beam beam in world.Beams.Values)
                CollisionDetection.BeamToTankCollision(beam, world);

            lock (clients)
                foreach (SocketState client in clients.Values)
                    UpdateTank(client);

            //Send json info to clients
            SendToClients();

            //Handle died or disconnect events
            DeathOrDisconnect();

            newFrame++;
        }

        /// <summary>
        /// When a new client connects, this method retrieves the player name
        /// and control commands from the client.
        /// </summary>
        /// <param name="state"></param>
        private void NewClient(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                Console.WriteLine(state.ErrorMessage);
                return;
            }

            state.OnNetworkAction = ReceiveStartup;
            Networking.GetData(state);
        }

        /// <summary>
        /// Method proccesses data from the buffer to collect and send startup info for the player.
        /// This info includes adding the new tank to the world, receiving controls and wall positions, and adds current clients. 
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveStartup(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                Console.WriteLine("Error in Receive Startup");
            }

            //Get player name
            string[] array = ProcessData(state);
            string playerName = array[0];

            if (playerName != "")
            {
                string s = "";


                //Create tank and add to world
                Tank t = new Tank((int)state.ID, playerName, tankHealth, projectileRate, PopulateRandomTank());
                lock (world.Tanks)
                    world.Tanks.Add(t.tank, t);


                //Allow new tank to receive controls
                state.OnNetworkAction = ReceiveControls;

                //Send to connected client
                s += (int)state.ID + "\n" + world.getWorldSize() + "\n";

                //Populate walls
                foreach (Wall wall in world.Walls.Values)
                    s += JsonConvert.SerializeObject(wall) + "\n";

                //Convert Tank to json
                s += JsonConvert.SerializeObject(t) + "\n";

                Networking.Send(state.TheSocket, s);
                t.join = false; //reset joining indicator

                //client to the dictionary
                lock (clients)
                    clients.Add((int)state.ID, state);


                //Display newly connected player 
                Console.WriteLine("Player: " + playerName + ", ID: " + t.tank + " joined the game.");
            }
            Networking.GetData(state);
        }

        /// <summary>
        /// Receives control commands from the client once they connect
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveControls(SocketState state)
        {
            if (state.ErrorOccurred)
                return;

            string[] array = ProcessData(state);

            // For each control command, add to model world, and wait until next frame to be processed
            foreach (string s in array)
            {
                if (s != "")
                {
                    //Gets commands from client, assings ID, and adds them to world
                    try
                    {
                        lock (world)
                        {
                            ControlCommands controlcommand = JsonConvert.DeserializeObject<ControlCommands>(s);
                            controlcommand.tankID = (int)state.ID;
                            world.ControlCommands = controlcommand;
                            UpdateControls(controlcommand);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Not sending control commands");
                    }
                }
            }
            Networking.GetData(state);
        }

        /// <summary>
        /// Processes data in the buffer that end with a newline character. Saves 
        /// incomplete data until it is fully received to process. Clears buffer when done. 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private static string[] ProcessData(SocketState state)
        {
            string data = state.GetData();
            string[] array = data.Split('\n');

            if (data != "")
            {
                if (data.Last() != '\n')
                {
                    state.RemoveData(0, data.LastIndexOf('\n') + 1);
                    array[array.Length - 1] = "";
                }

                state.RemoveData(0, state.GetData().Length);
            }


            return array;
        }

        /// <summary>
        /// Send tank, powerup, projectile, and beam json information to all connected clients.
        /// </summary>
        private void SendToClients()
        {
            string s = "";

            lock (world.Tanks)
                foreach (Tank tank in world.Tanks.Values)
                    s += JsonConvert.SerializeObject(tank) + "\n";

            foreach (Powerup powerup in world.Powerups.Values)
                s += JsonConvert.SerializeObject(powerup) + "\n";

            foreach (Projectile proj in world.Projectiles.Values)
                s += JsonConvert.SerializeObject(proj) + "\n";

            foreach (Beam beam in world.Beams.Values)
                s += JsonConvert.SerializeObject(beam) + "\n";


            //Iterates through connected clients and sends information
            lock (clients)
                foreach (SocketState client in clients.Values)
                    if (!client.ErrorOccurred)
                        Networking.Send(client.TheSocket, s);
        }

        /// <summary>
        /// Upon a disconnect, removes the tank from world and removes client from client dictionary.
        /// Anything in "died" status is also updated and or removed, such as tanks, projectiles, powerups, and beams.
        /// </summary>
        private void DeathOrDisconnect()
        {
            foreach (int i in tankRespawnList)
                world.Tanks[i].Died = false;

            //Remove disconnected clients
            foreach (int i in dcClients)
            {
                lock (clients)
                {
                    clients.Remove(i);
                    Console.WriteLine("Client: " + i + " disconnected.");
                }

                lock (world.Tanks)
                    world.Tanks.Remove(i);

            }

            foreach (int i in removeProjectile)
                world.Projectiles.Remove(i);

            foreach (int i in removePowerup)
                world.Powerups.Remove(i);

            world.Beams.Clear();

            //Reset List trackers
            removeProjectile.Clear();
            removePowerup.Clear();
            dcClients.Clear();
            tankRespawnList.Clear();
        }

        /// <summary>
        /// Create a random tank location to populate tank
        /// </summary>
        /// <returns></returns>
        private Vector2D PopulateRandomTank()
        {
            Random rng = new Random();
            while (true)
            {
                Vector2D location = new Vector2D(rng.Next(-world.getWorldSize() / 2, world.getWorldSize() / 2), rng.Next(-world.getWorldSize() / 2, world.getWorldSize() / 2));
                if (!CollisionDetection.ObjectToWallCollision(new Tank(), location, world))
                    return location;
            }
        }

        /// <summary>
        /// Check if client's tank is dead, disconnected, or hp is 0.
        /// If disconnected, it will set tank to "died", health to 0, and disconnected to true.
        /// Once all those statuses are updated, the tank is removed from the world
        /// If tank is hp == 0, it will try to respawn current died tank
        /// If a connected tank loses all of it's health, it's set to "died" and respawned.
        /// </summary>
        /// <param name="client"></param>
        private void UpdateTank(SocketState client)
        {
            Tank tank = world.Tanks[(int)client.ID];

            //Check for disconnects
            if (client.ErrorOccurred)
            {
                //Update statuses if disconnect occurs 
                tank.dc = true;
                tank.hp = 0;
                tank.Died = true;

                //Add to disconnected clients list to remove
                dcClients.Add((int)client.ID);
            }

            //If tank is still in game
            if (!tank.dc)
            {
                if (tank.hp <= 0)
                {
                    //Respawn tank after wait
                    if (tank.waitToSpawn >= tankRespawn)
                    {
                        tank.hp = tankHealth;
                        tank.loc = PopulateRandomTank();
                    }
                    else
                        tank.waitToSpawn++;
                }

                if (tank.Died)
                    tankRespawnList.Add(tank.tank);
            }
        }

        /// <summary>
        /// If tank has not died or disconnected, updates tank position and firing actions.
        /// Tank Position won't move if against a wall. If tank goes out of bounds it will teleport
        /// back into the world.
        /// </summary>
        /// <param name="cmd"></param>
        private void UpdateControls(ControlCommands c)
        {
            Tank tank = world.Tanks[c.tankID];
            tank.getAim = c.Tdir;

            if (tank.hp > 0 && !tank.dc && !tank.Died)
            {
                //Get and set tank orientation based on commands
                if (c.Move != "none")
                {
                    Vector2D direction = GetDirection(c.Move, tank);
                    tank.getOrientation = direction;
                    Vector2D nextframelocation = tank.loc + direction * tankSpeed;

                    //If tank doesn't collide, update tank based on current speed and direction
                    if (!CollisionDetection.ObjectToWallCollision(tank, nextframelocation, world) && TrackLocation(nextframelocation))
                        tank.loc = nextframelocation;

                    //Teleport to the other side
                    if (!TrackLocation(nextframelocation))
                    {
                        if (Math.Abs(nextframelocation.GetX()) > (world.getWorldSize() / 2))
                            tank.loc = new Vector2D(-nextframelocation.GetX(), nextframelocation.GetY());
                        if (Math.Abs(nextframelocation.GetY()) > (world.getWorldSize() / 2))
                            tank.loc = new Vector2D(nextframelocation.GetX(), -nextframelocation.GetY());
                    }
                }

                //Creates and adds new projectile to the world
                if ((c.FireKey == "main") && (tank.waitToShoot >= projectileRate))
                {
                    Projectile p = new Projectile(projectileID, tank.loc + tank.getAim * (tankSize / 2), tank.getAim, false, tank.tank);
                    world.Projectiles.Add(projectileID, p);
                    projectileID++;
                    tank.waitToShoot = 0;
                }

                //Creates and adds new beam to the world if tank has powerup
                if ((c.FireKey == "alt") && (tank.beamFire > 0))
                {
                    Beam beam = new Beam(beamID, tank.loc, tank.getAim, tank.tank);
                    world.Beams.Add(beamID, beam);
                    beamID++;
                    tank.beamFire--;
                }
                tank.waitToShoot++;
            }
        }

        /// <summary>
        /// Create a random powerup location to populate powerup
        /// </summary>
        /// <returns></returns>
        private Vector2D PopulateRandomPowerup()
        {
            Random rng = new Random();
            while (true)
            {
                Vector2D location = new Vector2D(rng.Next(-world.getWorldSize() / 2, world.getWorldSize() / 2), rng.Next(-world.getWorldSize() / 2, world.getWorldSize() / 2));
                //Make sure it doesn't collide with other walls
                if (!CollisionDetection.ObjectToWallCollision(new Powerup(), location, world))
                    return location;
            }
        }

        /// <summary>
        /// Creates a new powerup that spawns randomly. Limits the total number of powerups that 
        /// are allowed at any given time in the world.
        /// </summary>
        private void GenerateNewPowerup()
        {
            if (newFrame == powerupRespawn)
            {
                world.Powerups.Add(powerupID, new Powerup(powerupID, false, PopulateRandomPowerup()));
                powerupID++;
                powerupWait = new Random().Next(0, powerupRespawn);
            }
            else if (newFrame > powerupRespawn)
            {
                int numpower = world.Powerups.Values.Count;
                if (numpower < powerupNum && powerupWait <= 0)
                {
                    world.Powerups.Add(powerupID, new Powerup(powerupID, false, PopulateRandomPowerup()));
                    powerupID++;
                    powerupWait = new Random().Next(0, powerupRespawn);
                }
            }
            powerupWait--;
        }

        /// <summary>
        /// Updates current projectiles. If a projectile collides with a tank, lowers that tanks health. 
        /// If projectile ends up killing another tank, the score of the tank that shot the projectile is incremented.
        /// Should the projectile hit a wall or go out of bounds it is removed from the world.
        /// </summary>
        /// <param name="proj"></param>
        private void UpdateProjectile(Projectile proj)
        {
            bool wallCollision = CollisionDetection.ObjectToWallCollision(proj, proj.loc, world);
            bool tankCollision = CollisionDetection.ObjectToTankCollision(proj, world, removePowerup);
            bool freeFlying = TrackLocation(proj.loc);

            //If no collision
            if (!wallCollision && !tankCollision && freeFlying)
                proj.loc += proj.bdir * projectileSpeed;


            if (wallCollision || tankCollision || !freeFlying)
            {
                proj.Died = true;
                removeProjectile.Add(proj.ID);
            }
        }

        /// <summary>
        /// Create vector based on movement
        /// </summary>
        /// <param name="movement"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private Vector2D GetDirection(string movement, Tank t)
        {
            if (movement == "up")
                return new Vector2D(0, -1);

            else if (movement == "down")
                return new Vector2D(0, 1);

            else if (movement == "left")
                return new Vector2D(-1, 0);

            else if (movement == "right")
                return new Vector2D(1, 0);

            return t.getOrientation;
        }

        /// <summary>
        /// Reads XML file given file path
        /// </summary>
        /// <param name="file"></param>
        public void ReadSettingXmlFile(string file)
        {
            int wallID = 0;
            try
            {
                using (XmlReader reader = XmlReader.Create(file))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            if (reader.Name == "GameSettings")
                                world = new World();
                            else if (reader.Name == "UniverseSize")
                            {
                                reader.Read();
                                world.worldSize = int.Parse(reader.Value);
                            }
                            else if (reader.Name == "MSPerFrame")
                            {
                                reader.Read();
                                frameDelay = int.Parse(reader.Value);
                            }
                            else if (reader.Name == "FramesPerShot")
                            {
                                reader.Read();
                                projectileRate = int.Parse(reader.Value);
                            }
                            else if (reader.Name == "RespawnRate")
                            {
                                reader.Read();
                                tankRespawn = int.Parse(reader.Value);
                            }
                            else if (reader.Name == "TankHP")
                            {
                                reader.Read();
                                tankHealth = int.Parse(reader.Value);
                            }
                            else if (reader.Name == "ProjectileSpeed")
                            {
                                reader.Read();
                                projectileSpeed = int.Parse(reader.Value);
                            }
                            else if (reader.Name == "TankSpeed")
                            {
                                reader.Read();
                                tankSpeed = int.Parse(reader.Value);
                            }
                            else if (reader.Name == "TankSize")
                            {
                                reader.Read();
                                tankSize = int.Parse(reader.Value);
                                world.tanksize = tankSize;
                            }
                            else if (reader.Name == "WallSize")
                            {
                                reader.Read();
                                wallSize = int.Parse(reader.Value);
                                world.wallsize = wallSize;
                            }
                            else if (reader.Name == "NumberOfPowerups")
                            {
                                reader.Read();
                                powerupNum = int.Parse(reader.Value);
                            }
                            else if (reader.Name == "PowerupRespawnRate")
                            {
                                reader.Read();
                                powerupRespawn = int.Parse(reader.Value);
                            }
                            else if (reader.Name == "Wall")
                            {
                                while (reader.Value == "" || reader.Value.Contains("\n"))
                                {
                                    reader.Read();
                                }
                                int p1_x = int.Parse(reader.Value);
                                reader.Read();
                                while (reader.Value == "" || reader.Value.Contains("\n"))
                                {
                                    reader.Read();
                                }
                                int p1_y = int.Parse(reader.Value);
                                Vector2D p1 = new Vector2D(p1_x, p1_y);
                                reader.Read();
                                while (reader.Value == "" || reader.Value.Contains("\n"))
                                {
                                    reader.Read();
                                }
                                int p2_x = int.Parse(reader.Value);
                                reader.Read();
                                while (reader.Value == "" || reader.Value.Contains("\n"))
                                {
                                    reader.Read();
                                }
                                int p2_y = int.Parse(reader.Value);
                                Vector2D p2 = new Vector2D(p2_x, p2_y);
                                //Add to the world
                                Wall wall = new Wall(wallID, p1, p2);
                                world.Walls.Add(wallID, wall);
                                wallID++; // Make sure wallID is unique
                            }
                            else
                                throw new Exception("Settings Error");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
