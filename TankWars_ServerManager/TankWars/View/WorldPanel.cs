using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TankWars;

namespace TankWars
{
    public class WorldPanel : Panel
    {
        /// <summary>
        /// States current world model data that is used to draw objects.
        /// </summary>
        private World theWorld;
        private int playerID;

        /// <summary>
        /// Image storage variables.
        /// </summary>
        private Image[] tankBodies;
        private Image[] turrets;
        private Image[] projectile;
        private Image wall;
        private Image background;

        /// <summary>
        /// List that keeps track of beams invoked by right click to draw.
        /// </summary>
        private List<Beam> beams = new List<Beam> { };

        /// <summary>
        /// List that keeps track of explosions invoked when tank dies.
        /// </summary>
        private List<Explosion> explosions = new List<Explosion> { };

        /// <summary>
        /// Sets images from file to variables.
        /// </summary>
        public WorldPanel()
        {
            DoubleBuffered = true;
            string str = !File.Exists("..\\..\\..\\Resources\\Images\\BlueTank.png") ? "Images\\" : "..\\..\\..\\Resources\\Images\\";
            this.tankBodies = new Image[8]
            {
              Image.FromFile(str + "BlueTank.png"),
              Image.FromFile(str + "RedTank.png"),
              Image.FromFile(str + "GreenTank.png"),
              Image.FromFile(str + "OrangeTank.png"),
              Image.FromFile(str + "DarkTank.png"),
              Image.FromFile(str + "LightGreenTank.png"),
              Image.FromFile(str + "YellowTank.png"),
              Image.FromFile(str + "PurpleTank.png")
            };
            this.turrets = new Image[8]
            {
              Image.FromFile(str + "BlueTurret.png"),
              Image.FromFile(str + "RedTurret.png"),
              Image.FromFile(str + "GreenTurret.png"),
              Image.FromFile(str + "OrangeTurret.png"),
              Image.FromFile(str + "DarkTurret.png"),
              Image.FromFile(str + "LightGreenTurret.png"),
              Image.FromFile(str + "YellowTurret.png"),
              Image.FromFile(str + "PurpleTurret.png")
            };
            this.projectile = new Image[8]
            {
              Image.FromFile(str + "shot-blue.png"),
              Image.FromFile(str + "shot-red.png"),
              Image.FromFile(str + "shot-yellow.png"),
              Image.FromFile(str + "shot-white.png"),
              Image.FromFile(str + "shot-brown.png"),
              Image.FromFile(str + "shot-grey.png"),
              Image.FromFile(str + "shot-yellow.png"),
              Image.FromFile(str + "shot-violet.png")
            };
            this.wall = Image.FromFile(str + "WallSprite.png");
            this.background = Image.FromFile(str + "Background.png");
        }

        /// <summary>
        /// Function called in Form.cs to set a world to worldPanel
        /// </summary>
        /// <param name="world"></param>
        public void SetWorld(World world) => theWorld = world;

        /// <summary>
        /// Function called in Form.cs to set a playerID to worldPanel
        /// </summary>
        /// <param name="ID"></param>
        public void SetID(int ID) => playerID = ID;

        /// <summary>
        /// Adds beam to beam list to be drawn in worldPanel. Invoked by Controller when it receives a beam json object.
        /// </summary>
        /// <param name="b"></param>
        public void AddBeam(Beam b)
        {
            beams.Add(b);
        }

        /// <summary>
        /// Adds explosion to explosion list to be drawn in worldPanel. Invoked by Controller when tank dies.
        /// </summary>
        /// <param name="t"></param>
        public void ExplodingDeath(Tank t)
        {
            Explosion explosion = new Explosion(t.loc, 50);
            new MethodInvoker(() => this.explosions.Add(explosion))();
        }


        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        /// <summary>
        /// Draws tank and scorepanel with colored healthbar.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;
            Image tankBody = this.tankBodies[tank.tank % this.tankBodies.Length];
            e.Graphics.DrawImage(tankBody, -30, -30, 60, 60);
            e.Graphics.RotateTransform(-tank.getOrientation.ToAngle());
            int width = (int)(40.0 * ((double)tank.hp / 3.0));
            Color color = Color.Green;
            if (tank.hp == 2)
                color = Color.Yellow;
            if (tank.hp == 1)
                color = Color.Red;
            using (SolidBrush solidBrush1 = new SolidBrush(color))
            {
                using (SolidBrush solidBrush2 = new SolidBrush(Color.White))
                {
                    e.Graphics.FillRectangle((Brush)solidBrush1, new Rectangle(-20, -40, width, 5));
                    using (Font font = new Font("Arial", 12f))
                    {
                        SizeF sizeF = e.Graphics.MeasureString(tank.Name + ": " + tank.Score.ToString(), font);
                        e.Graphics.DrawString(tank.Name + ": " + tank.Score.ToString(), font, (Brush)solidBrush2, (float)(-(double)sizeF.Width / 2.0), 30f);
                    }
                }
            }
        }

        private void TurretDrawer(object o, PaintEventArgs e)
        {
            Image turret = this.turrets[(o as Tank).tank % this.turrets.Length];
            e.Graphics.DrawImage(turret, -25, -25, 50, 50);
        }

        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile projectile = o as Projectile;
            Image image = this.projectile[projectile.ID % this.projectile.Length];
            e.Graphics.DrawImage(image, -15, -15, 30, 30);
        }

        /// <summary>
        /// Draws orage and green circle to represent powerup.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.InterpolationMode = InterpolationMode.Low;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            Color orange = Color.Orange;
            Color green = Color.Green;
            using (SolidBrush solidBrush1 = new SolidBrush(orange))
            {
                using (SolidBrush solidBrush2 = new SolidBrush(green))
                {
                    e.Graphics.FillEllipse((Brush)solidBrush1, -(8), -(8), 16, 16);
                    e.Graphics.FillEllipse((Brush)solidBrush2, -(5), -(5), 10, 10);
                }
            }
        }

        /// <summary>
        /// Draws white line to represent beam object.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            Beam beam = (Beam)o;

            using (Pen pen = new Pen(Color.FromArgb(255, 255, 255, 255)))
            {
                e.Graphics.DrawLine(pen, 0, 0, (float)beam.direction.GetX() * 2000, (float)beam.direction.GetY() * 2000);
            }
        }

        private void WallDrawer(object o, PaintEventArgs e) => e.Graphics.DrawImage(this.wall, new Rectangle(-25, -25, 50, 50));

        /// <summary>
        /// Centers and zooms client view to worldPanel.
        /// </summary>
        /// <param name="worldSize"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int worldSize, double w) => (int)(w + (double)(worldSize / 2));

        /// <summary>
        /// Helps center objects in world panel and rotates images when called.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="o"></param>
        /// <param name="worldSize"></param>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="angle"></param>
        /// <param name="drawer"></param>
        private void DrawObjectWithTransform(
        PaintEventArgs e,
        object o,
        int worldSize,
        double worldX,
        double worldY,
        double angle,
        ObjectDrawer drawer)
        {
            Matrix matrix = e.Graphics.Transform.Clone();
            int imageSpace1 = WorldSpaceToImageSpace(worldSize, worldX);
            int imageSpace2 = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform((float)imageSpace1, (float)imageSpace2);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);
            e.Graphics.Transform = matrix;
        }

        /// <summary>
        /// Paints world using data from world model.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.theWorld != null)
            {
                lock (theWorld)
                {
                    //Centers client view and worldPanel around tank location.
                    if (theWorld.getTanks().ContainsKey(this.playerID))
                    {
                        double x = this.theWorld.getTanks()[this.playerID].loc.GetX();
                        double y = this.theWorld.getTanks()[this.playerID].loc.GetY();
                        double num1 = 900.0 / (double)this.theWorld.worldSize;
                        int num2 = (int)((double)this.theWorld.worldSize / 2.0 * num1 / 1);
                        double num3 = (double)(-WorldSpaceToImageSpace(this.theWorld.worldSize, x) + num2);
                        double num4 = (double)(-WorldSpaceToImageSpace(this.theWorld.worldSize, y) + num2);
                        e.Graphics.ScaleTransform(1, 1);
                        e.Graphics.TranslateTransform((float)num3, (float)num4);
                        e.Graphics.SmoothingMode = SmoothingMode.None;
                        e.Graphics.InterpolationMode = InterpolationMode.Low;
                        e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                        int num5 = this.theWorld.worldSize / 2;
                    }

                    //Draws background image.
                    e.Graphics.DrawImage(this.background, 0, 0, this.theWorld.worldSize, this.theWorld.worldSize);

                    //Draws walls in correct place and rotates wall image when needed.
                    foreach (Wall wall in theWorld.Walls.Values)
                    {
                        int distToNextSegment = 50;
                        Vector2D diff = wall.EP1 - wall.EP2;
                        int segmentsNumber = (int)(diff.Length() / 50 + 1);
                        if (wall.EP1.GetX() > wall.EP2.GetX())
                        {
                            for (int i = 0; i < segmentsNumber; i++)
                                DrawObjectWithTransform(e, wall, theWorld.worldSize, wall.EP2.GetX() + distToNextSegment * i, wall.EP2.GetY(), 0, WallDrawer);

                        }
                        else if (wall.EP1.GetX() < wall.EP2.GetX())
                        {
                            for (int i = 0; i < segmentsNumber; i++)
                                DrawObjectWithTransform(e, wall, theWorld.worldSize, wall.EP1.GetX() + distToNextSegment * i, wall.EP1.GetY(), 0, WallDrawer);

                        }
                        else if (wall.EP1.GetY() > wall.EP2.GetY()) // from EP2 y to EP1 y 
                        {
                            for (int i = 0; i < segmentsNumber; i++)
                                DrawObjectWithTransform(e, wall, theWorld.worldSize, wall.EP2.GetX(), wall.EP2.GetY() + distToNextSegment * i, 0, WallDrawer);
                        }
                        else if (wall.EP1.GetY() < wall.EP2.GetY()) // from EP1 y to EP2 y 
                        {
                            for (int i = 0; i < segmentsNumber; i++)
                                DrawObjectWithTransform(e, wall, theWorld.worldSize, wall.EP1.GetX(), wall.EP1.GetY() + distToNextSegment * i, 0, WallDrawer);
                        }
                    }

                    //Draws tank when hitpoints are more than 0.
                    foreach (Tank tank in this.theWorld.getTanks().Values)
                    {
                        if (tank.hp != 0)
                        {
                            double x = tank.loc.GetX();
                            double y = tank.loc.GetY();
                            this.DrawObjectWithTransform(e, (object)tank, this.theWorld.worldSize, x, y, (double)tank.getOrientation.ToAngle(), new ObjectDrawer(this.TankDrawer));
                            this.DrawObjectWithTransform(e, (object)tank, this.theWorld.worldSize, x, y, (double)tank.getAim.ToAngle(), new ObjectDrawer(this.TurretDrawer));
                        }
                    }

                    //Draws powerup in location from server.
                    foreach (Projectile proj in theWorld.Projectiles.Values)
                    {
                        DrawObjectWithTransform(e, proj, theWorld.worldSize, proj.loc.GetX(), proj.loc.GetY(), proj.bdir.ToAngle(), ProjectileDrawer);
                    }

                    //Draws powerup in location from server.
                    foreach (Powerup powerup in theWorld.Powerups.Values)
                    {
                        DrawObjectWithTransform(e, powerup, theWorld.worldSize, powerup.loc.GetX(), powerup.loc.GetY(), 0, PowerupDrawer);
                    }

                    //Displays explosions when invoked.
                    List<Explosion> explosions = new List<Explosion>();
                    foreach (Explosion explosion in this.explosions)
                    {
                        double x3 = explosion.GetOrigin().GetX();
                        double y3 = explosion.GetOrigin().GetY();
                        Explosion explosion1 = explosion;
                        this.DrawObjectWithTransform(e, explosion, this.theWorld.getWorldSize(), x3, y3, (double)explosion.GetOrientation().ToAngle(), new WorldPanel.ObjectDrawer(explosion1.ExplosionDrawer));
                        if (!explosion.Step())
                        {
                            continue;
                        }
                        explosions.Add(explosion);
                    }

                    foreach (Explosion explosion2 in explosions)
                    {
                        this.explosions.Remove(explosion2);
                    }

                    //Draws beams that were invoked.
                    for (int i = 0; i < beams.Count; i++)
                        DrawBeam(e, beams[i]);


                    base.OnPaint(e);
                }
            }
            else
            {
                Console.WriteLine("World null.");
            }
        }

        //Stops beam from drawing more than 17 frames.
        private void DrawBeam(PaintEventArgs e, Beam beam)
        {
            if (!beam.Spent)
            {
                DrawObjectWithTransform(e, beam, theWorld.worldSize, beam.origin.GetX(), beam.origin.GetY(), 0, BeamDrawer);

                if (beam.frameNumber == 17)
                    beam.Spent = true;
                else
                    beam.frameNumber++;
            }
            else
                beams.Remove(beam);
        }

        //Mouse event handlers.
        public event MouseEventHandler MousePressed;
        public event MouseEventHandler MouseReleased;
        public event MouseEventHandler MouseMoved;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            MouseEventHandler mouseMovedEvent = this.MouseMoved;
            if (mouseMovedEvent == null)
                return;
            mouseMovedEvent((object)this, e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            MouseEventHandler panelMouseDownEvent = this.MousePressed;
            if (panelMouseDownEvent == null)
                return;
            panelMouseDownEvent((object)this, e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            MouseEventHandler panelMouseUpEvent = this.MouseReleased;
            if (panelMouseUpEvent == null)
                return;
            panelMouseUpEvent((object)this, e);
        }

    }
}

/// <summary>
/// Explosion class that simulates explosion animation.
/// </summary>
public class Explosion
{
    protected int numParticles = 12;

    protected double speed = 1.5;

    protected Color color = Color.White;

    protected Vector2D[] particles;

    protected Vector2D[] velocities;

    protected Vector2D origin;

    protected Vector2D orientation;

    protected int step;

    protected int totalSteps;

    public Explosion(Vector2D dir, int _numParticles, int steps)
    {
        this.orientation = new Vector2D(dir);
        this.numParticles = _numParticles;
        this.totalSteps = steps;
        this.particles = new Vector2D[this.numParticles];
        this.velocities = new Vector2D[this.numParticles];
    }

    public Explosion(Vector2D position, int _totalSteps) : this(new Vector2D(0, -1), 12, _totalSteps)
    {
        Random random = new Random();
        this.origin = new Vector2D(position);
        double num = 0;
        for (int i = 0; i < this.numParticles; i++)
        {
            this.particles[i] = new Vector2D(position);
            this.velocities[i] = new Vector2D(1, 0);
            this.velocities[i].Rotate(num + random.NextDouble() * 90);
            num += 90;
            if (num >= 360)
            {
                num = 0;
            }
        }
    }

    public virtual void ExplosionDrawer(object o, PaintEventArgs e)
    {
        using (SolidBrush solidBrush = new SolidBrush(this.color))
        {
            Vector2D[] vector2DArray = this.particles;
            for (int i = 0; i < (int)vector2DArray.Length; i++)
            {
                Vector2D vector2D = vector2DArray[i] - this.origin;
                e.Graphics.FillEllipse(solidBrush, (float)vector2D.GetX() - 2f, (float)vector2D.GetY() - 2f, 4f, 4f);
            }
        }
    }

    public Vector2D GetOrientation()
    {
        return this.orientation;
    }

    public Vector2D GetOrigin()
    {
        return this.origin;
    }

    public virtual bool Step()
    {
        for (int i = 0; i < this.numParticles; i++)
        {
            this.particles[i] = this.particles[i] + (this.velocities[i] * this.speed);
        }
        this.step++;
        return this.step == this.totalSteps;
    }
}