using System.Collections.Generic;

namespace SatelliteSite.Data.Demos
{
    public class Child
    {
        public string FamilyName { get; set; }
        public string FirstName { get; set; }
        public string Gender { get; set; }
        public int Grade { get; set; }
        public ICollection<Pet> Pets { get; set; }
    }
}
