
namespace TrabajosExpres.Models
{
    public class Service
    {
        public int idService { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string slogan { get; set; }
        public string typeService { get; set; }
        public string workingHours { get; set; }
        public float minimalCost { get; set; }
        public float maximumCost { get; set; }
        public int idCity { get; set; }
        public int idMemberATE { get; set; }
        public int serviceStatus { get; set; }
    }
}
