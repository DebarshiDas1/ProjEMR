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
    /// The procedureService responsible for managing procedure related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting procedure information.
    /// </remarks>
    public interface IProcedureService
    {
        /// <summary>Retrieves a specific procedure by its primary key</summary>
        /// <param name="id">The primary key of the procedure</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The procedure data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of procedures based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of procedures</returns>
        Task<List<Procedure>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new procedure</summary>
        /// <param name="model">The procedure data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(Procedure model);

        /// <summary>Updates a specific procedure by its primary key</summary>
        /// <param name="id">The primary key of the procedure</param>
        /// <param name="updatedEntity">The procedure data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, Procedure updatedEntity);

        /// <summary>Updates a specific procedure by its primary key</summary>
        /// <param name="id">The primary key of the procedure</param>
        /// <param name="updatedEntity">The procedure data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<Procedure> updatedEntity);

        /// <summary>Deletes a specific procedure by its primary key</summary>
        /// <param name="id">The primary key of the procedure</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The procedureService responsible for managing procedure related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting procedure information.
    /// </remarks>
    public class ProcedureService : IProcedureService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the Procedure class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public ProcedureService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific procedure by its primary key</summary>
        /// <param name="id">The primary key of the procedure</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The procedure data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.Procedure.AsQueryable();
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

            string[] navigationProperties = ["ProductId_Product"];
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

        /// <summary>Retrieves a list of procedures based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of procedures</returns>/// <exception cref="Exception"></exception>
        public async Task<List<Procedure>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetProcedure(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new procedure</summary>
        /// <param name="model">The procedure data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(Procedure model)
        {
            model.Id = await CreateProcedure(model);
            return model.Id;
        }

        /// <summary>Updates a specific procedure by its primary key</summary>
        /// <param name="id">The primary key of the procedure</param>
        /// <param name="updatedEntity">The procedure data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, Procedure updatedEntity)
        {
            await UpdateProcedure(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific procedure by its primary key</summary>
        /// <param name="id">The primary key of the procedure</param>
        /// <param name="updatedEntity">The procedure data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<Procedure> updatedEntity)
        {
            await PatchProcedure(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific procedure by its primary key</summary>
        /// <param name="id">The primary key of the procedure</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteProcedure(id);
            return true;
        }
        #region
        private async Task<List<Procedure>> GetProcedure(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.Procedure.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<Procedure>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(Procedure), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<Procedure, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateProcedure(Procedure model)
        {
            _dbContext.Procedure.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateProcedure(Guid id, Procedure updatedEntity)
        {
            _dbContext.Procedure.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteProcedure(Guid id)
        {
            var entityData = _dbContext.Procedure.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.Procedure.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchProcedure(Guid id, JsonPatchDocument<Procedure> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.Procedure.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.Procedure.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}