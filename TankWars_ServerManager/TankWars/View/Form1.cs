using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TankWars;
using static TankWars.Controller;

namespace TankWars
{
    public partial class Form1 : Form
    {
        private Controller theController;
        private WorldPanel worldPanel;
        private World world;
        private int playerID;

        /// <summary>
        /// Initializes Form and Controller.
        /// Sets event handler methods for Network Errors and when controller connects successfully.
        /// Initializes key handler.
        /// Sets default worldPanel view and client size.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            theController = new Controller();
            theController.NetworkEvent(new NetworkEventHandler(NetworkErrorHandler));
            theController.ConnectionEvent(new ConnectionEventHandler(ConnectWorld));

            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;

            // Place and add the drawing panel
            worldPanel = new WorldPanel();
            worldPanel.Location = new Point(0, 40);
            worldPanel.Size = new Size(900, 900);
            worldPanel.BackColor = Color.Black;
            ClientSize = new Size(900, 940);
        }

        /// <summary>
        /// Input checker that makes sure server name and player name are correct inputs.
        /// Connects client to server.
        /// Disables connect button and input textboxes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectBtn_Click(object sender, EventArgs e)
        {
            if (this.serverText.Text == "")
            {
                int num1 = (int)MessageBox.Show("Please enter a server address");
            }
            else if (this.playerNameText.Text == "")
            {
                int num2 = (int)MessageBox.Show("Please enter a name");
            }
            else if (this.playerNameText.Text.Length > 16)
            {
                int num3 = (int)MessageBox.Show("Name must be less than 16 characters");
            }
            else
            {
                this.connectBtn.Enabled = false;
                this.serverText.Enabled = false;
                this.playerNameText.Enabled = false;
                theController.Connect(playerNameText.Text, serverText.Text);             
            }
        }

        private void NetworkErrorHandler()
        {
            int num = (int)MessageBox.Show("Disconnected from server");
        }

        /// <summary>
        /// Is invoked by the controller once the controller connects successfully. 
        /// Connects the world panel and sets up event handlers.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="id"></param>
        private void ConnectWorld(World w, int id) => this.Invoke(new Action(() =>
        {
            this.playerID = id;
            this.world = w;
            this.worldPanel.SetWorld(this.world);
            this.worldPanel.SetID(this.playerID);
            this.worldPanel.MousePressed += new MouseEventHandler(this.OnPanelMouseDown);
            this.worldPanel.MouseReleased += new MouseEventHandler(this.OnPanelMouseUp);
            this.worldPanel.MouseMoved += new MouseEventHandler(this.OnMouseMoved);


            this.StartPosition = FormStartPosition.CenterScreen;

            this.Invoke(new MethodInvoker(() => {
                theController.FrameEvent(new UpdateFrame(FrameUpdateHandler));
                theController.BeamEvent(new BeamEventHandler(worldPanel.AddBeam));
                theController.DeathEvent(new DeathEventHandler(worldPanel.ExplodingDeath));

                this.Controls.Add(worldPanel);
                this.KeyPreview = true;
            }
            ));
        }));

        /// <summary>
        /// Invokes a new frame when updater is called
        /// </summary>
        private void FrameUpdateHandler()
        {
            try
            {
                this.Invoke(new Action(() => this.Invalidate(true)));
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Registers and send mouse presses to the controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPanelMouseDown(object sender, MouseEventArgs e) 
        {
            if (e.Button.Equals(MouseButtons.Left))
                theController.MousePressed("main");
            if (e.Button.Equals(MouseButtons.Right))
                theController.MousePressed("alt");
        }

        /// <summary>
        /// Registers and sends mouse releases to the controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPanelMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Left))
                theController.MouseReleased("main");
            if (e.Button.Equals(MouseButtons.Right))
                theController.MouseReleased("alt");
        }

        /// <summary>
        /// Registers and sends mouse movements to the controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseMoved(object sender, MouseEventArgs e) => this.theController.MouseMove(e.Location);

        /// <summary>
        /// OnPaint override
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        /// <summary>
        /// Registers key presses and sends them to the controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                theController.KeyPressed("up");
            if (e.KeyCode == Keys.S)
                theController.KeyPressed("down");
            if (e.KeyCode == Keys.A)
                theController.KeyPressed("left");
            if (e.KeyCode == Keys.D)
                theController.KeyPressed("right");
            if (e.KeyCode == Keys.Space)
                theController.KeyPressed("main");
        }

        /// <summary>
        /// Registers key releases and sends them to the controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                theController.KeyRelease("up");
            if (e.KeyCode == Keys.S)
                theController.KeyRelease("down");
            if (e.KeyCode == Keys.A)
                theController.KeyRelease("left");
            if (e.KeyCode == Keys.D)
                theController.KeyRelease("right");
            if (e.KeyCode == Keys.Space)
                theController.KeyRelease("main");
        }

    }
}


