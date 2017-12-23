using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SensingNet.Ws.Controllers.Api
{
    public class DbController : ApiController
    {


        public IEnumerable<string> Query()
        {
            return new string[] { "value1", "value2xxxx" };
        }

        public IEnumerable<string> QueryWithParams()
        {
            return new string[] { "value1", "value2xxxx" };
        }



        public IEnumerable<string> NonQuery()
        {
            return new string[] { "value1", "value2xxxx" };
        }

        public IEnumerable<string> NonQueryWithParams()
        {
            return new string[] { "value1", "value2xxxx" };
        }


    }
}
