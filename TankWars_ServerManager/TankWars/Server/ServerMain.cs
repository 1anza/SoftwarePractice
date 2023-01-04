using System;
using System.IO;

namespace TankWars
{
    class ServerMain
    {
        private static int port = 11000;

        public static void Main(string[] args)
        {
            //Intialize controller and Read given setting file
            ServerController controller = new ServerController(@"..\\..\\..\\..\\Resources\\Settings\\settings.xml");

            //Set up network connection and start running server
            controller.InitializeServer(port);

            //Print to console to indicate server is ready to receive clients connections 
            Console.WriteLine("Server has started...");        }
    }
}
