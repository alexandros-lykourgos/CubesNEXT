using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cubes.Core.DataAccess;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Cubes.Web.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly IQueryManager queryManager;
        private readonly IConnectionManager connectionManager;
        private readonly DataAccessSettings settings;

        public DataController(IConnectionManager connectionManager,
            IQueryManager queryManager,
            IOptionsSnapshot<DataAccessSettings> options)
        {
            this.queryManager      = queryManager;
            this.connectionManager = connectionManager;
            this.settings          = options.Value;
        }

        /// <summary>
        /// Get Connections
        /// </summary>
        /// <remarks>
        /// Get all <see cref="Connection"/> objects defined in the DataAccessSettings file.
        /// </remarks>
        /// <returns><see cref="IEnumerable{Connection}"/></returns>
        [HttpGet("connections")]
        public IEnumerable<Connection> GetConnections() => this.settings.Connections;

        /// <summary>
        /// Test connection
        /// </summary>
        /// <remarks>
        /// Test if <see cref="Connection"/> named <paramref name="connectionName"/> can be used by calling OpenAsync method.
        /// </remarks>
        /// <param name="connectionName">Connection name</param>
        /// <returns></returns>
        [HttpGet("connections/{connectionName}/test")]
        public async Task<IActionResult> TestConnection(string connectionName)
        {
            try
            {
                using var cnx = this.connectionManager.GetConnection(connectionName);
                await cnx.OpenAsync();
                await cnx.CloseAsync();

                return Ok(new { Message = "Connection was successful!" });
            }
            catch (ArgumentException ax)
            {
                return NotFound(new { Message = ax.Message });
            }
        }

        /// <summary>
        /// Get Queries
        /// </summary>
        /// <remarks>
        /// Get all <see cref="Query"/> objects defined in the DataAccessSettings file.
        /// </remarks>
        /// <returns><see cref="IEnumerable{Query}"/></returns>
        [HttpGet("queries")]
        public IEnumerable<Query> GetQueries() => this.settings.Queries;

        /// <summary>
        /// Execute known query
        /// </summary>
        /// <remarks>
        /// Execute query stored on DataAccessSettings file using a known database connection.
        /// Query string contains values for query parameters.
        /// </remarks>
        /// <param name="connectionName">Connection name</param>
        /// <param name="queryName">Query name</param>
        /// <returns></returns>
        [HttpGet("queries/{connectionName}/{queryName}")]
        public async Task<IActionResult> ExecuteQuery(string connectionName, string queryName)
            => await ExecuteQueryInternal(connectionName, queryName, null);

        /// <summary>
        /// Execute query defined in request
        /// </summary>
        /// <remarks>
        /// Execute query defined in requests body using a known database connection.
        /// Query string contains values for query parameters.
        /// </remarks>
        /// <param name="connectionName"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost("queries/{connectionName}")]
        public async Task<IActionResult> ExecuteQuery(string connectionName, [FromBody] Query query)
            => await ExecuteQueryInternal(connectionName, null, query);

        private async Task<IActionResult> ExecuteQueryInternal(string connectionName, string queryName, Query query)
        {
            try
            {
                using var cnx = this.connectionManager.GetConnection(connectionName);
                await cnx.OpenAsync();

                if (query == null)
                    query = this.queryManager.GetSqlQuery(queryName);
                var dynamicParameters = new DynamicParameters();
                if (Request.Query.Count > 0)
                {
                    foreach (var item in Request.Query)
                        dynamicParameters.Add(item.Key, item.Value.FirstOrDefault());
                }
                var result = cnx.Query<dynamic>(query.QueryCommand, dynamicParameters);

                return Ok(new { Results = result });
            }
            catch (ArgumentException ax)
            {
                return NotFound(new { Message = ax.Message });
            }
        }
    }
}