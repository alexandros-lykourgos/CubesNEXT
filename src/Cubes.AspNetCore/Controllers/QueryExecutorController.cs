using System;
using System.Linq;
using Cubes.Core.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace Cubes.AspNetCore.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class QueryExecutorController : ControllerBase
    {
        private readonly IQueryExecutor queryExecutor;

        public QueryExecutorController(IQueryExecutor queryExecutor)
            => this.queryExecutor = queryExecutor;

        [HttpGet("{connection}/{query}")]
        public ActionResult ExecuteQuery(string connection, string query)
        {
            var parameters = Request
                .Query
                .ToDictionary(q => q.Key, q => (Object)q.Value.ToString());

            var results = queryExecutor.Query(connection, query, parameters);
            return Ok(results);
        }
    }
}