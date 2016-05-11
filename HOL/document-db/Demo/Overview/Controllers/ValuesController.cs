using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Overview.Models;
using Overview.Repositories;

namespace Overview.Controllers
{
    
    public class ValuesController : ApiController
    {

        public ValuesController():this(new ValuesDocumentRepository(new DocumentClientFactory().GetInstance()))
        {
            
        }

        private readonly IRepository<Values> _valuesRepository;
        public ValuesController(IRepository<Values> valuesRepository)
        {
            _valuesRepository = valuesRepository;
        }

        // GET api/values
        public async Task<IEnumerable<Values>> Get()
        {
            return await _valuesRepository.GetAll();
        }

        // GET api/values/5
        public async Task<Values> Get(string id)
        {
            return await _valuesRepository.GetById(id);
        }

        // POST api/values
        public async Task<IHttpActionResult> Post(Values value)
        {
            var selfLink=await _valuesRepository.Add(value);
            return Created(new Uri(selfLink), "New Environment Created");
        }

        // PUT api/values/5
        public async Task<IHttpActionResult> Put(string id, Values value)
        {
            await _valuesRepository.Update(id, value);

            return Ok();
        }

        // DELETE api/values/5
        public async Task<IHttpActionResult> Delete(string id)
        {
            await _valuesRepository.Delete(id);
            return Ok();
        }
    }
}
