using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajosExpres.Models
{
    public class Resource
    {
        public int idResource { get; set; }
        public string isMainResource { get; set; }
        public string name { get; set; }
        public string idService { get; set; }
        public string idMemberATE { get; set; }
    }
}
