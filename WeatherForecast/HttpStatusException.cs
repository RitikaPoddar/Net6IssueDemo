using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WeatherForecast
{
    public class HttpStatusException : Exception
    {
        /// <summary>
        /// Represents the httpStatusCode for the exception raised
        /// </summary>
        public HttpStatusCode Status { get; private set; }
        /// <summary>
        /// Initializes instance of ApplicationException class.
        /// </summary>
        /// <param name="status">HttpStatusCode</param>
        /// <param name="msg">Exception message</param>
        public HttpStatusException(HttpStatusCode status, string msg) : base(msg)
        {
            Status = status;
        }
    }
}
