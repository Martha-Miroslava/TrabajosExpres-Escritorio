
namespace TrabajosExpres.Models
{
    public class Rating
    {
        public int idRating { get; set; }
        public string comment { get; set; }
        public int rating { get; set; }
        public int idRequest { get; set; }
        public int isClient { get; set; }
    }
}
