using Newtonsoft.Json;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        [JsonProperty(PropertyName = "tank")]
        private int ID;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "bdir")]
        private Vector2D orientation;

        [JsonProperty(PropertyName = "tdir")]
        private Vector2D aiming = new Vector2D(0, -1);

        [JsonProperty(PropertyName = "name")]
        private string name;

        [JsonProperty(PropertyName = "hp")]
        private int hitPoints = 3;

        [JsonProperty(PropertyName = "score")]
        private int score = 0;

        [JsonProperty(PropertyName = "died")]
        private bool died = false;

        [JsonProperty(PropertyName = "dc")]
        private bool disconnected = false;

        [JsonProperty(PropertyName = "join")]
        private bool joined = false;

        private static int IDCounter = 0;

        public int waitToShoot { get; set; }
        public int waitToSpawn { get; set; }
        public int beamFire { get; set; }

        public int tank { get { return ID; } set { ID = value; } }
        public Vector2D loc { get { return location; } set { location = value; } }
        public Vector2D getOrientation { get { return orientation; } set { orientation = value; } }
        public Vector2D getAim { get { return aiming; } set { aiming = value; } }
        public string Name { get { return name; } set { name = value; } }
        public int hp { get { return hitPoints; } set { hitPoints = value; } }
        public int Score { get { return score; } set { score = value; } }
        public bool Died { get { return died; } set { died = value; } }
        public bool dc { get { return disconnected; } set { disconnected = value; } }
        public bool join { get { return joined; } set { joined = value; } }



        public Tank()
        {

        }

        public Tank(int ID, Vector2D location, Vector2D orientation, Vector2D aiming, string name,
                    int hitPoints, int score, bool died, bool disconnected, bool joined)
        {
            this.ID = ID;
            this.location = location;
            this.orientation = orientation;
            this.aiming = aiming;
            this.name = name;
            this.hitPoints = hitPoints;
            this.score = score;
            this.died = died;
            this.disconnected = disconnected;
            this.joined = joined;
        }

        public Tank(int ID, string name, int health, int projectileRate, Vector2D loc)
        {
            this.ID = ID;
            this.location = loc;
            this.orientation = new Vector2D(0, 0);
            this.name = name;
            this.hitPoints = health;
            this.waitToShoot = projectileRate;
        }
    }
}