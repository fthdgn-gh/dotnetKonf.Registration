using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetKonf.Registration.API.Models
{
    public class EntryVerifyModel
    {
        public string Action { get; set; }
        public string VerifyCode { get; set; }
    }
}
