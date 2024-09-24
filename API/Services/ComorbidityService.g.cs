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
    /// The comorbidityService responsible for managing comorbidity related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting comorbidity information.
    /// </remarks>
    public interface IComorbidityService
    {
        /// <summary>Retrieves a specific comorbidity by its primary key</summary>
        /// <param name="id">The primary key of the comorbidity</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The comorbidity data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of comorbiditys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of comorbiditys</returns>
        Task<List<Comorbidity>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new comorbidity</summary>
        /// <param name="model">The comorbidity data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(Comorbidity model);

        /// <summary>Updates a specific comorbidity by its primary key</summary>
        /// <param name="id">The primary key of the comorbidity</param>
        /// <param name="updatedEntity">The comorbidity data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, Comorbidity updatedEntity);

        /// <summary>Updates a specific comorbidity by its primary key</summary>
        /// <param name="id">The primary key of the comorbidity</param>
        /// <param name="updatedEntity">The comorbidity data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<Comorbidity> updatedEntity);

        /// <summary>Deletes a specific comorbidity by its primary key</summary>
        /// <param name="id">The primary key of the comorbidity</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The comorbidityService responsible for managing comorbidity related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting comorbidity information.
    /// </remarks>
    public class ComorbidityService : IComorbidityService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the Comorbidity class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public ComorbidityService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific comorbidity by its primary key</summary>
        /// <param name="id">The primary key of the comorbidity</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The comorbidity data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.Comorbidity.AsQueryable();
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

        /// <summary>Retrieves a list of comorbiditys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of comorbiditys</returns>/// <exception cref="Exception"></exception>
        public async Task<List<Comorbidity>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetComorbidity(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new comorbidity</summary>
        /// <param name="model">The comorbidity data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(Comorbidity model)
        {
            model.Id = await CreateComorbidity(model);
            return model.Id;
        }

        /// <summary>Updates a specific comorbidity by its primary key</summary>
        /// <param name="id">The primary key of the comorbidity</param>
        /// <param name="updatedEntity">The comorbidity data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, Comorbidity updatedEntity)
        {
            await UpdateComorbidity(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific comorbidity by its primary key</summary>
        /// <param name="id">The primary key of the comorbidity</param>
        /// <param name="updatedEntity">The comorbidity data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<Comorbidity> updatedEntity)
        {
            await PatchComorbidity(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific comorbidity by its primary key</summary>
        /// <param name="id">The primary key of the comorbidity</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteComorbidity(id);
            return true;
        }
        #region
        private async Task<List<Comorbidity>> GetComorbidity(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.Comorbidity.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<Comorbidity>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(Comorbidity), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<Comorbidity, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateComorbidity(Comorbidity model)
        {
            _dbContext.Comorbidity.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateComorbidity(Guid id, Comorbidity updatedEntity)
        {
            _dbContext.Comorbidity.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteComorbidity(Guid id)
        {
            var entityData = _dbContext.Comorbidity.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.Comorbidity.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchComorbidity(Guid id, JsonPatchDocument<Comorbidity> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.Comorbidity.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.Comorbidity.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}