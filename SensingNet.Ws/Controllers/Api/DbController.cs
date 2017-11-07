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
        // GET: api/Db
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Db/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Db
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Db/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Db/5
        public void Delete(int id)
        {
        }



        [HttpGet]
        public IEnumerable<string> Details()
        {
            return new string[] { "value1", "value2xxxx" };
        }

    }
}
