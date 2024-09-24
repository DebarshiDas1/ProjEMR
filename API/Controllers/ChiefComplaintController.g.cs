using Microsoft.AspNetCore.Mvc;
using ProjEMR.Models;
using ProjEMR.Services;
using ProjEMR.Entities;
using ProjEMR.Filter;
using ProjEMR.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Task = System.Threading.Tasks.Task;
using ProjEMR.Authorization;

namespace ProjEMR.Controllers
{
    /// <summary>
    /// Controller responsible for managing chiefcomplaint related operations.
    /// </summary>
    /// <remarks>
    /// This Controller provides endpoints for adding, retrieving, updating, and deleting chiefcomplaint information.
    /// </remarks>
    [Route("api/chiefcomplaint")]
    [Authorize]
    public class ChiefComplaintController : BaseApiController
    {
        private readonly IChiefComplaintService _chiefComplaintService;

        /// <summary>
        /// Initializes a new instance of the ChiefComplaintController class with the specified context.
        /// </summary>
        /// <param name="ichiefcomplaintservice">The ichiefcomplaintservice to be used by the controller.</param>
        public ChiefComplaintController(IChiefComplaintService ichiefcomplaintservice)
        {
            _chiefComplaintService = ichiefcomplaintservice;
        }

        /// <summary>Adds a new chiefcomplaint</summary>
        /// <param name="model">The chiefcomplaint data to be added</param>
        /// <returns>The result of the operation</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [UserAuthorize("ChiefComplaint", Entitlements.Create)]
        public async Task<IActionResult> Post([FromBody] ChiefComplaint model)
        {
            model.TenantId = TenantId;
            var id = await _chiefComplaintService.Create(model);
            return Ok(new { id });
        }

        /// <summary>Retrieves a list of chiefcomplaints based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of chiefcomplaints</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [UserAuthorize("ChiefComplaint", Entitlements.Read)]
        public async Task<IActionResult> Get([FromQuery] string filters, string searchTerm, int pageNumber = 1, int pageSize = 10, string sortField = null, string sortOrder = "asc")
        {
            List<FilterCriteria> filterCriteria = null;
            if (pageSize < 1)
            {
                return BadRequest("Page size invalid.");
            }

            if (pageNumber < 1)
            {
                return BadRequest("Page mumber invalid.");
            }

            if (!string.IsNullOrEmpty(filters))
            {
                filterCriteria = JsonHelper.Deserialize<List<FilterCriteria>>(filters);
            }

            var result = await _chiefComplaintService.Get(filterCriteria, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return Ok(result);
        }

        /// <summary>Retrieves a specific chiefcomplaint by its primary key</summary>
        /// <param name="id">The primary key of the chiefcomplaint</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The chiefcomplaint data</returns>
        [HttpGet]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [UserAuthorize("ChiefComplaint", Entitlements.Read)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, string fields = null)
        {
            var result = await _chiefComplaintService.GetById( id, fields);
            return Ok(result);
        }

        /// <summary>Deletes a specific chiefcomplaint by its primary key</summary>
        /// <param name="id">The primary key of the chiefcomplaint</param>
        /// <returns>The result of the operation</returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        [Route("{id:Guid}")]
        [UserAuthorize("ChiefComplaint", Entitlements.Delete)]
        public async Task<IActionResult> DeleteById([FromRoute] Guid id)
        {
            var status = await _chiefComplaintService.Delete(id);
            return Ok(new { status });
        }

        /// <summary>Updates a specific chiefcomplaint by its primary key</summary>
        /// <param name="id">The primary key of the chiefcomplaint</param>
        /// <param name="updatedEntity">The chiefcomplaint data to be updated</param>
        /// <returns>The result of the operation</returns>
        [HttpPut]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [UserAuthorize("ChiefComplaint", Entitlements.Update)]
        public async Task<IActionResult> UpdateById(Guid id, [FromBody] ChiefComplaint updatedEntity)
        {
            if (id != updatedEntity.Id)
            {
                return BadRequest("Mismatched Id");
            }

            updatedEntity.TenantId = TenantId;
            var status = await _chiefComplaintService.Update(id, updatedEntity);
            return Ok(new { status });
        }

        /// <summary>Updates a specific chiefcomplaint by its primary key</summary>
        /// <param name="id">The primary key of the chiefcomplaint</param>
        /// <param name="updatedEntity">The chiefcomplaint data to be updated</param>
        /// <returns>The result of the operation</returns>
        [HttpPatch]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [UserAuthorize("ChiefComplaint", Entitlements.Update)]
        public async Task<IActionResult> UpdateById(Guid id, [FromBody] JsonPatchDocument<ChiefComplaint> updatedEntity)
        {
            if (updatedEntity == null)
                return BadRequest("Patch document is missing.");
            var status = await _chiefComplaintService.Patch(id, updatedEntity);
            return Ok(new { status });
        }
    }
}