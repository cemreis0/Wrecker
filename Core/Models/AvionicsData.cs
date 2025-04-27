namespace Core.Models
{
    public class AvionicsData : AvionicsBase
    {
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float Altitude { get; set; }
        public float Speed { get; set; }

        public float Latitude { get; set; }
        public float Longitude { get; set; }


        public override string ToString()
        {
            return $"Roll: {Roll}, Pitch: {Pitch}, Yaw: {Yaw}, İrtifa: {Altitude}, Hız: {Speed}, Enlem: {Latitude}, Boylam: {Longitude}";
        }
    }
}
