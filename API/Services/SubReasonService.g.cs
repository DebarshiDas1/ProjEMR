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
    /// The subreasonService responsible for managing subreason related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting subreason information.
    /// </remarks>
    public interface ISubReasonService
    {
        /// <summary>Retrieves a specific subreason by its primary key</summary>
        /// <param name="id">The primary key of the subreason</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The subreason data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of subreasons based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of subreasons</returns>
        Task<List<SubReason>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new subreason</summary>
        /// <param name="model">The subreason data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(SubReason model);

        /// <summary>Updates a specific subreason by its primary key</summary>
        /// <param name="id">The primary key of the subreason</param>
        /// <param name="updatedEntity">The subreason data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, SubReason updatedEntity);

        /// <summary>Updates a specific subreason by its primary key</summary>
        /// <param name="id">The primary key of the subreason</param>
        /// <param name="updatedEntity">The subreason data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<SubReason> updatedEntity);

        /// <summary>Deletes a specific subreason by its primary key</summary>
        /// <param name="id">The primary key of the subreason</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The subreasonService responsible for managing subreason related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting subreason information.
    /// </remarks>
    public class SubReasonService : ISubReasonService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the SubReason class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public SubReasonService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific subreason by its primary key</summary>
        /// <param name="id">The primary key of the subreason</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The subreason data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.SubReason.AsQueryable();
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

            string[] navigationProperties = ["GoodsReturns_GoodsReturn"];
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

        /// <summary>Retrieves a list of subreasons based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of subreasons</returns>/// <exception cref="Exception"></exception>
        public async Task<List<SubReason>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetSubReason(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new subreason</summary>
        /// <param name="model">The subreason data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(SubReason model)
        {
            model.Id = await CreateSubReason(model);
            return model.Id;
        }

        /// <summary>Updates a specific subreason by its primary key</summary>
        /// <param name="id">The primary key of the subreason</param>
        /// <param name="updatedEntity">The subreason data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, SubReason updatedEntity)
        {
            await UpdateSubReason(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific subreason by its primary key</summary>
        /// <param name="id">The primary key of the subreason</param>
        /// <param name="updatedEntity">The subreason data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<SubReason> updatedEntity)
        {
            await PatchSubReason(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific subreason by its primary key</summary>
        /// <param name="id">The primary key of the subreason</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteSubReason(id);
            return true;
        }
        #region
        private async Task<List<SubReason>> GetSubReason(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.SubReason.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<SubReason>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(SubReason), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<SubReason, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateSubReason(SubReason model)
        {
            _dbContext.SubReason.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateSubReason(Guid id, SubReason updatedEntity)
        {
            _dbContext.SubReason.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteSubReason(Guid id)
        {
            var entityData = _dbContext.SubReason.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.SubReason.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchSubReason(Guid id, JsonPatchDocument<SubReason> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.SubReason.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.SubReason.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}