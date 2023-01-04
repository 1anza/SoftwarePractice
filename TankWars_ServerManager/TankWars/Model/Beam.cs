using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam
    {
        [JsonProperty(PropertyName = "beam")]
        public int ID;
        [JsonProperty(PropertyName = "org")]
        public Vector2D origin;
        [JsonProperty(PropertyName = "dir")]
        public Vector2D direction;
        [JsonProperty(PropertyName = "owner")]
        public int owner;

        public bool spent;
        public int frameNumber { get; set; }

        public int beam { get { return ID; } set { ID = value; } }
        public bool Spent { get { return spent; } set { spent = value; } }

        public Beam() { }
        public Beam(int id, Vector2D org, Vector2D dir, int ownerID)
        {
            ID = id;
            origin = org;
            direction = dir;
            owner = ownerID;
        }
    }
}
