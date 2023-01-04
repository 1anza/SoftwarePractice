using Newtonsoft.Json;

namespace TankWars
{

    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        [JsonProperty(PropertyName = "power")]
        public int ID;
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location;
        [JsonProperty(PropertyName = "died")]
        public bool died;
        public int powerup { get { return ID; } set { ID = value; } }
        public Vector2D loc { get { return location; } }


        public Powerup()
        {
        }


        public Powerup(int power, bool died, Vector2D loc)
        {
            ID = power;
            location = loc;
            this.died = died;
        }
    }

}