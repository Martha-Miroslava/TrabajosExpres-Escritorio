using System;

namespace TrabajosExpres.Models
{
    public class MemberATE
    {
        public int idAccount { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string lastName { get; set; }
        public DateTime dateBirth { get; set; }
        public string email { get; set; }
        public int idCity { get; set; }
        public int memberATEStatus { get; set; }
        public int memberATEType { get; set; }

    }
}
