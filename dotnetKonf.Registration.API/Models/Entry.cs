using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetKonf.Registration.API.Models
{
    public class Entry : LiteEntry
    {
        public string PhoneNumber { get; set; }
        public string VerifyCode { get; set; }
        public bool IsVerified { get; set; }
    }
}
