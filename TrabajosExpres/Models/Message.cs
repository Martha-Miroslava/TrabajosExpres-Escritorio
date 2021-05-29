using System;

namespace TrabajosExpres.Models
{
    public class Message
    {
        public int idMessage { get; set; }
        public string message { get; set; }
        public int idChat { get; set; }
        public DateTime dateTime { get; set; }
        public int memberATEType { get; set; }
    }
}
