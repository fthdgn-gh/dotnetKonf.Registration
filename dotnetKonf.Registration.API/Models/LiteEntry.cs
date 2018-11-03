using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetKonf.Registration.API.Models
{
    public class LiteEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public string MailAddress { get; set; }
    }
}
