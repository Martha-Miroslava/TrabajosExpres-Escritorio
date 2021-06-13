
namespace TrabajosExpres.Models
{
    public class RequestSent
    {
        public int idRequest { get; set; }
        public string address { get; set; }
        public int requestStatus { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string trouble { get; set; }
        public int idMemberATE { get; set; }
        public string idService { get; set; }
    }
}
