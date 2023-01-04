using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommands
    {
        [JsonProperty(PropertyName = "moving")]
        private string moving = "none";

        [JsonProperty(PropertyName = "fire")]
        private string fireSpace;

        [JsonProperty(PropertyName = "tdir")]
        private Vector2D tdir = new Vector2D(1, 0);

        //Server
        public int tankID;

        public string Move { get { return moving; } set { moving = value; } }
        public string FireKey { get { return fireSpace; } set { fireSpace = value; } }
        public Vector2D Tdir { get { return tdir; } set { tdir = value; } }

        public ControlCommands(int tID, string move, string shootSpace, Vector2D aim)
        {
            tankID = tID;
            moving = move;
            fireSpace = shootSpace;
            tdir = aim;
        }

        public ControlCommands() { }

    }
}
