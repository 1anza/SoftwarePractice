using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;

namespace TankWars
{
    public class Controller
    {
        private int uniquePlayerID;
        private string playerName;
        public World world;

        /// <summary>
        /// Delegate and event to update frame
        /// </summary>
        private event UpdateFrame OnUpdater;
        public void FrameEvent(UpdateFrame f) => OnUpdater += f;

        /// <summary>
        /// Delegate and event for NetworkErrors
        /// </summary>
        private event NetworkEventHandler NetworkError;
        public void NetworkEvent(NetworkEventHandler n) => NetworkError += n;

        /// <summary>
        /// Delegate and event for connections.
        /// </summary>
        private event ConnectionEventHandler Connected;
        public void ConnectionEvent(ConnectionEventHandler c) => Connected += c;

        /// <summary>
        /// Delegate and event to create a beam
        /// </summary>
        private event BeamEventHandler Beam;
        public void BeamEvent(BeamEventHandler b) => Beam += b;

        /// <summary>
        /// Delegate and event for death occurrences
        /// </summary>
        private event DeathEventHandler Died;
        public void DeathEvent(DeathEventHandler d) => Died += d;

        /// <summary>
        /// Control Commands to handle movement, firing, and aiming
        /// </summary>
        private ControlCommands command = new ControlCommands();
        private string Move = "none";
        private string FireKey = "none";
        private Vector2D aim = new Vector2D();

        /// <summary>
        /// Lists that keep track of mouse and key presses
        /// </summary>
        private List<string> keysPressed = new List<string>();
        private List<string> mousePressed = new List<string>();


        /// <summary>
        /// Gets the IP address and player name, then attempts to connect to server.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="server"></param>
        public void Connect(string name, string server)
        {
            playerName = name;
            Networking.ConnectToServer((ConnectPlayerFirstTime), server, 11000);
        }

        /// <summary>
        /// Callback for connecting to the server. Sends the players name and starts receiving data from InitialWorld Load
        /// </summary>
        /// <param name="state"></param>
        private void ConnectPlayerFirstTime(SocketState state)
        {
            if (state.ErrorOccurred || state.TheSocket == null || !state.TheSocket.Connected)
            {
                NetworkError?.Invoke();
                return;
            }
            else
            {
                state.OnNetworkAction = new Action<SocketState>(InitialWorldLoad);
                Networking.Send(state.TheSocket, playerName + '\n');
                Networking.GetData(state);
            }
        }

        /// <summary>
        /// Initializes the worldPanel and the world. Starts receiing and communicating with server.
        /// </summary>
        /// <param name="state"></param>
        private void InitialWorldLoad(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                NetworkError?.Invoke();
            }

            world = new World();

            //Get initial data to set ID and worldsize
            string data = state.GetData();
            string[] initialArray = Regex.Split(data, "(?<=[\\n])");
            uniquePlayerID = int.Parse(initialArray[0]);
            int size = int.Parse(initialArray[1]);
            world.WorldSize(size);

            //ReceiveData
            Connected?.Invoke(world, uniquePlayerID);
            state.OnNetworkAction = new Action<SocketState>(ReceiveData);

            //Receive loop
            lock (world)
                Networking.GetData(state);

        }

        /// <summary>
        /// Gets all movement commands to serialized state and sends them to the server
        /// </summary>
        /// <param name="state"></param>
        public void HandleMovement(SocketState state)
        {
            Vector2D vector2D = this.aim;
            vector2D.Normalize();
            ControlCommands controlCommand = new ControlCommands(uniquePlayerID, Move, FireKey, vector2D);
            string jsonData = JsonConvert.SerializeObject(controlCommand);
            Networking.Send(state.TheSocket, jsonData + "\n");
        }

        /// <summary>
        /// Receives world information
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveData(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                NetworkError?.Invoke();
                return;
            }
            try
            {
                ParseData(state);

                HandleMovement(state);

                lock (world)
                    Networking.GetData(state);

            }
            catch
            {
            }
        }

        /// <summary>
        /// Parses data and processes each element in the data to a Json. 
        /// Once data is deserialized, it is sent to the server and updated. 
        /// </summary>
        /// <param name="state"></param>
        private void ParseData(SocketState state)
        {
            try
            {
                // Split up data sent from server for parsing.
                string data = state.GetData();
                string[] wholeArray = Regex.Split(data, "(?<=[\\n])");

                lock (world)
                {
                    foreach (string p in wholeArray)
                    {
                        if (p.Contains("wall"))
                        {
                            lock (world.Walls)
                            {
                                Wall x = (Wall)JsonConvert.DeserializeObject<Wall>(p);
                                world.Walls.Add(x.wall, x);
                            }
                        }
                        else if (p.Contains("tank"))
                        {
                            Tank tank;
                            try
                            {
                                tank = JsonConvert.DeserializeObject<Tank>(p);
                            }
                            catch
                            {
                                string newTank = p + "\n";
                                tank = JsonConvert.DeserializeObject<Tank>(newTank);
                            }

                            lock (world.Tanks)
                            {
                                if (tank.dc) // Remove the tank from the world and Start Explosion animation
                                {
                                    if (world.Tanks.ContainsKey(tank.tank))
                                    {
                                        world.Tanks.Remove(tank.tank);
                                    }

                                    Died?.Invoke(tank);
                                }
                                else if (tank.Died)
                                {
                                    Died?.Invoke(tank);
                                }
                                else // add to the world
                                {
                                    if (world.Tanks.ContainsKey(tank.tank))
                                    {
                                        world.Tanks[tank.tank] = tank;
                                    }
                                    else
                                    {
                                        world.Tanks.Add(tank.tank, tank);
                                    }
                                }
                            }

                        }

                        // If p is a powerup, adds a powerup.
                        else if (p.Contains("power"))
                        {
                            Powerup powerup = (Powerup)JsonConvert.DeserializeObject<Powerup>(p);
                            // Remove collected powerups
                            lock (world.Powerups)
                            {
                                if (powerup.died) //Remove power up from the world
                                {
                                    if (world.Powerups.ContainsKey(powerup.ID))
                                    {
                                        world.Powerups.Remove(powerup.ID);
                                    }

                                }
                                else // Add to the World model as usual
                                {
                                    if (world.Powerups.ContainsKey(powerup.ID))
                                    {
                                        world.Powerups[powerup.ID] = powerup;
                                    }
                                    else
                                    {
                                        world.Powerups.Add(powerup.ID, powerup);
                                    }
                                }
                            }
                        }
                        // If p is a projectile, add a projectile.
                        else if (p.Contains("proj"))
                        {
                            Projectile projectile = (Projectile)JsonConvert.DeserializeObject<Projectile>(p);
                            lock (world.Projectiles)
                            {
                                if (projectile.Died) // Remove projectile in the world
                                {
                                    if (world.Projectiles.ContainsKey(projectile.ID))
                                    {
                                        world.Projectiles.Remove(projectile.ID);
                                    }

                                }
                                else // Add to the World model as usual
                                {
                                    if (world.Projectiles.ContainsKey(projectile.ID))
                                    {
                                        world.Projectiles[projectile.ID] = projectile;
                                    }
                                    else
                                    {
                                        world.Projectiles.Add(projectile.ID, projectile);
                                    }
                                }

                            }

                        }
                        // If p is a beam, add it.
                        else if (p.Contains("beam"))
                        {
                            lock (world.Beams)
                            {
                                Beam x = (Beam)JsonConvert.DeserializeObject<Beam>(p);
                                world.Beams.Add(x.beam, x);
                                Beam?.Invoke(x);
                            }

                        }
                        //clears the buffer
                        state.RemoveData(0, p.Length);
                    }
                }
                //Updates
                OnUpdater?.Invoke();
            }
            catch (JsonReaderException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Registers key presses and sets them to move or fire when appropriate. Handles 
        /// multiple inputs to allow for better movement handling.
        /// </summary>
        /// <param name="key"></param>
        public void KeyPressed(string key)
        {
            if (!keysPressed.Contains(key))
                keysPressed.Add(key);

            if (mousePressed.Contains("main"))
                FireKey = "main";

            if (keysPressed.Count < 4)
            {
                if ((key.Contains("up")) || (key.Contains("down")) || (key.Contains("left")) || (key.Contains("right")))
                    Move = key;
                if (key.Contains("main"))
                    FireKey = key;
            }
        }

        /// <summary>
        /// Registers key releases and updates movement and firing parameters
        /// </summary>
        /// <param name="key"></param>
        public void KeyRelease(string key)
        {
            if (keysPressed.Contains("main"))
                FireKey = "none";

            keysPressed.Remove(key);

            if (keysPressed.Count == 0)
                Move = "none";
            else
                Move = keysPressed[keysPressed.Count - 1];
        }

        /// <summary>
        /// Registers mouse presses to fire projectiles or beams
        /// </summary>
        /// <param name="click"></param>
        public void MousePressed(string click)
        {

            if (!mousePressed.Contains(click))
                mousePressed.Add(click);

            FireKey = click;
        }

        /// <summary>
        /// Handles mouse release to halt firing
        /// </summary>
        /// <param name="click"></param>
        public void MouseReleased(string click)
        {
            mousePressed.Remove(click);
            if (mousePressed.Count == 0)
                FireKey = "none";
        }

        /// <summary>
        /// Handles the cursor movement to direct the tank's turret
        /// </summary>
        /// <param name="p"></param>
        public void MouseMove(Point p)
        {
            aim = new Vector2D(p.X - 450, p.Y - 450);
            aim.Normalize();
            command.Tdir = aim;
        }
    }
}