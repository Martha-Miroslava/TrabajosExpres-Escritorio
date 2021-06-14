
namespace TrabajosExpres.Models
{
    public class RatingReceived
    {
        public int idRating { get; set; }
        public string comment { get; set; }
        public int rating { get; set; }
        public int idRequest { get; set; }
        public string isClient { get; set; }
    }
}
