using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetKonf.Registration.API.Models
{

    public class ApiResponse<T>
    {
        public bool IsSucceeded { get; set; }
        public string Message { get; set; }
        public T Value { get; set; }

        public ApiResponse<T> AsSuccess(T value, string message = null)
        {
            IsSucceeded = true;
            Value = value;
            Message = message;
            return this;
        }

        public ApiResponse<T> AsError(string message)
        {
            IsSucceeded = false;
            Message = message;
            return this;
        }
    }
}
