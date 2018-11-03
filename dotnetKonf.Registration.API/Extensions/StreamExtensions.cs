using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetKonf.Registration.API.Extensions
{
    public static class StreamExtensions
    {
        public static T Deserialize<T>(this Stream self)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                self.CopyTo(ms);
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(ms.ToArray()));
            }
        }
    }
}
