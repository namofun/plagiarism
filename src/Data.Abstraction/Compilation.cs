namespace SatelliteSite.Data
{
    public class Compilation
    {
        public int Id { get; set; }

        public string Error { get; set; }

        public byte[] Tokens { get; set; }
    }
}
