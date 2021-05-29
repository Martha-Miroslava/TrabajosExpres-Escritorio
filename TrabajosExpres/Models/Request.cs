using System;

namespace TrabajosExpres.Models
{
    public class Request
    {
        public int idRequest { get; set; }
        public string address { get; set; }
        public DateTime date { get; set; }
        public DateTime time { get; set; }
        public string trouble { get; set; }
        public int idMemberATE { get; set; }
        public int idService { get; set; }
    }
}
