using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        [JsonProperty(PropertyName = "proj")]
        public int ID;
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location;
        [JsonProperty(PropertyName = "dir")]
        public Vector2D orientation;
        [JsonProperty(PropertyName = "died")]
        public bool Died;
        [JsonProperty(PropertyName = "owner")]
        public int Owner;

        public int projectile { get { return ID; } set { ID = value; } }
        public Vector2D loc { get { return location; } set { location = value; } }
        public Vector2D bdir { get { return orientation; } set { orientation = value; } }
        public bool died { get { return Died; } set { Died = value; } }
        public Projectile(int id, Vector2D loc, Vector2D dir, bool used, int ownerID)
        {
            ID = id;
            location = loc;
            orientation = dir;
            died = used;
            Owner = ownerID;
        }
    }
}
