namespace SatelliteSite.Entities
{
    public class PlagiarismCompilation
    {
        public int Id { get; set; }

        public string Error { get; set; }

        public byte[] Tokens { get; set; }
    }
}
