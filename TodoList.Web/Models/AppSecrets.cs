using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TodoList.Web.Models
{
    public class AppSecrets
    {
        public string SendGridApiKey { get; set; }
        public Wunderlist Wunderlist { get; set; } 
    }

    public class Wunderlist
    {
        public string ClientId { get; set; }
        public string AccessToken { get; set; }
        public string ClientSecret { get; set; }

    }
}
