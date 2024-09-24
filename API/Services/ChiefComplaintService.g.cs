using ProjEMR.Models;
using ProjEMR.Data;
using ProjEMR.Filter;
using ProjEMR.Entities;
using ProjEMR.Logger;
using Microsoft.AspNetCore.JsonPatch;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using Task = System.Threading.Tasks.Task;

namespace ProjEMR.Services
{
    /// <summary>
    /// The chiefcomplaintService responsible for managing chiefcomplaint related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting chiefcomplaint information.
    /// </remarks>
    public interface IChiefComplaintService
    {
        /// <summary>Retrieves a specific chiefcomplaint by its primary key</summary>
        /// <param name="id">The primary key of the chiefcomplaint</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The chiefcomplaint data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of chiefcomplaints based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of chiefcomplaints</returns>
        Task<List<ChiefComplaint>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new chiefcomplaint</summary>
        /// <param name="model">The chiefcomplaint data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(ChiefComplaint model);

        /// <summary>Updates a specific chiefcomplaint by its primary key</summary>
        /// <param name="id">The primary key of the chiefcomplaint</param>
        /// <param name="updatedEntity">The chiefcomplaint data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, ChiefComplaint updatedEntity);

        /// <summary>Updates a specific chiefcomplaint by its primary key</summary>
        /// <param name="id">The primary key of the chiefcomplaint</param>
        /// <param name="updatedEntity">The chiefcomplaint data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<ChiefComplaint> updatedEntity);

        /// <summary>Deletes a specific chiefcomplaint by its primary key</summary>
        /// <param name="id">The primary key of the chiefcomplaint</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The chiefcomplaintService responsible for managing chiefcomplaint related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting chiefcomplaint information.
    /// </remarks>
    public class ChiefComplaintService : IChiefComplaintService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the ChiefComplaint class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public ChiefComplaintService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific chiefcomplaint by its primary key</summary>
        /// <param name="id">The primary key of the chiefcomplaint</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The chiefcomplaint data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.ChiefComplaint.AsQueryable();
            List<string> allfields = new List<string>();
            if (!string.IsNullOrEmpty(fields))
            {
                allfields.AddRange(fields.Split(","));
                fields = $"Id,{fields}";
            }
            else
            {
                fields = "Id";
            }

            string[] navigationProperties = [];
            foreach (var navigationProperty in navigationProperties)
            {
                if (allfields.Any(field => field.StartsWith(navigationProperty + ".", StringComparison.OrdinalIgnoreCase)))
                {
                    query = query.Include(navigationProperty);
                }
            }

            query = query.Where(entity => entity.Id == id);
            return _mapper.MapToFields(await query.FirstOrDefaultAsync(),fields);
        }

        /// <summary>Retrieves a list of chiefcomplaints based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of chiefcomplaints</returns>/// <exception cref="Exception"></exception>
        public async Task<List<ChiefComplaint>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetChiefComplaint(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new chiefcomplaint</summary>
        /// <param name="model">The chiefcomplaint data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(ChiefComplaint model)
        {
            model.Id = await CreateChiefComplaint(model);
            return model.Id;
        }

        /// <summary>Updates a specific chiefcomplaint by its primary key</summary>
        /// <param name="id">The primary key of the chiefcomplaint</param>
        /// <param name="updatedEntity">The chiefcomplaint data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, ChiefComplaint updatedEntity)
        {
            await UpdateChiefComplaint(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific chiefcomplaint by its primary key</summary>
        /// <param name="id">The primary key of the chiefcomplaint</param>
        /// <param name="updatedEntity">The chiefcomplaint data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<ChiefComplaint> updatedEntity)
        {
            await PatchChiefComplaint(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific chiefcomplaint by its primary key</summary>
        /// <param name="id">The primary key of the chiefcomplaint</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteChiefComplaint(id);
            return true;
        }
        #region
        private async Task<List<ChiefComplaint>> GetChiefComplaint(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.ChiefComplaint.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<ChiefComplaint>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(ChiefComplaint), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<ChiefComplaint, object>>(Expression.Convert(property, typeof(object)), parameter);
                if (sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.OrderBy(lambda);
                }
                else if (sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.OrderByDescending(lambda);
                }
                else
                {
                    throw new ApplicationException("Invalid sort order. Use 'asc' or 'desc'");
                }
            }

            var paginatedResult = await result.Skip(skip).Take(pageSize).ToListAsync();
            return paginatedResult;
        }

        private async Task<Guid> CreateChiefComplaint(ChiefComplaint model)
        {
            _dbContext.ChiefComplaint.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateChiefComplaint(Guid id, ChiefComplaint updatedEntity)
        {
            _dbContext.ChiefComplaint.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteChiefComplaint(Guid id)
        {
            var entityData = _dbContext.ChiefComplaint.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.ChiefComplaint.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchChiefComplaint(Guid id, JsonPatchDocument<ChiefComplaint> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.ChiefComplaint.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.ChiefComplaint.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}