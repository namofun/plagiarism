namespace Plag.Backend.Entities
{
    public class Compilation
    {
        public string Id { get; set; }

        public string Error { get; set; }

        public byte[] Tokens { get; set; }
    }
}
