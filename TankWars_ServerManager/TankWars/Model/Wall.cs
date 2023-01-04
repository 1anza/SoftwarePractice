using Newtonsoft.Json;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        private static int nextID;
        [JsonProperty(PropertyName = "wall")]
        private int ID;

        [JsonProperty(PropertyName = "p1")]
        private Vector2D endpoint1;

        [JsonProperty(PropertyName = "p2")]
        private Vector2D endpoint2;

        public int wall { get { return ID; } set { ID = value; } }
        public Vector2D EP1 { get { return endpoint1; } }
        public Vector2D EP2 { get { return endpoint2; } }


        public Wall()
        {
            this.ID = -1;
            this.endpoint1 = this.endpoint2 = (Vector2D)null;
        }

        public Wall(int id, Vector2D _p1, Vector2D _p2)
        {
            this.ID = id;
            this.endpoint1 = new Vector2D(_p1);
            this.endpoint2 = new Vector2D(_p2);
        }
    }
}