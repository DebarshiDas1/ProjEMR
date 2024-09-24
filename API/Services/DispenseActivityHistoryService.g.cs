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
    /// The dispenseactivityhistoryService responsible for managing dispenseactivityhistory related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting dispenseactivityhistory information.
    /// </remarks>
    public interface IDispenseActivityHistoryService
    {
        /// <summary>Retrieves a specific dispenseactivityhistory by its primary key</summary>
        /// <param name="id">The primary key of the dispenseactivityhistory</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The dispenseactivityhistory data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of dispenseactivityhistorys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of dispenseactivityhistorys</returns>
        Task<List<DispenseActivityHistory>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new dispenseactivityhistory</summary>
        /// <param name="model">The dispenseactivityhistory data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(DispenseActivityHistory model);

        /// <summary>Updates a specific dispenseactivityhistory by its primary key</summary>
        /// <param name="id">The primary key of the dispenseactivityhistory</param>
        /// <param name="updatedEntity">The dispenseactivityhistory data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, DispenseActivityHistory updatedEntity);

        /// <summary>Updates a specific dispenseactivityhistory by its primary key</summary>
        /// <param name="id">The primary key of the dispenseactivityhistory</param>
        /// <param name="updatedEntity">The dispenseactivityhistory data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<DispenseActivityHistory> updatedEntity);

        /// <summary>Deletes a specific dispenseactivityhistory by its primary key</summary>
        /// <param name="id">The primary key of the dispenseactivityhistory</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The dispenseactivityhistoryService responsible for managing dispenseactivityhistory related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting dispenseactivityhistory information.
    /// </remarks>
    public class DispenseActivityHistoryService : IDispenseActivityHistoryService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the DispenseActivityHistory class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public DispenseActivityHistoryService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific dispenseactivityhistory by its primary key</summary>
        /// <param name="id">The primary key of the dispenseactivityhistory</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The dispenseactivityhistory data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.DispenseActivityHistory.AsQueryable();
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

            string[] navigationProperties = ["DispenseId_Dispense"];
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

        /// <summary>Retrieves a list of dispenseactivityhistorys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of dispenseactivityhistorys</returns>/// <exception cref="Exception"></exception>
        public async Task<List<DispenseActivityHistory>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetDispenseActivityHistory(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new dispenseactivityhistory</summary>
        /// <param name="model">The dispenseactivityhistory data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(DispenseActivityHistory model)
        {
            model.Id = await CreateDispenseActivityHistory(model);
            return model.Id;
        }

        /// <summary>Updates a specific dispenseactivityhistory by its primary key</summary>
        /// <param name="id">The primary key of the dispenseactivityhistory</param>
        /// <param name="updatedEntity">The dispenseactivityhistory data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, DispenseActivityHistory updatedEntity)
        {
            await UpdateDispenseActivityHistory(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific dispenseactivityhistory by its primary key</summary>
        /// <param name="id">The primary key of the dispenseactivityhistory</param>
        /// <param name="updatedEntity">The dispenseactivityhistory data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<DispenseActivityHistory> updatedEntity)
        {
            await PatchDispenseActivityHistory(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific dispenseactivityhistory by its primary key</summary>
        /// <param name="id">The primary key of the dispenseactivityhistory</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteDispenseActivityHistory(id);
            return true;
        }
        #region
        private async Task<List<DispenseActivityHistory>> GetDispenseActivityHistory(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.DispenseActivityHistory.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<DispenseActivityHistory>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(DispenseActivityHistory), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<DispenseActivityHistory, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateDispenseActivityHistory(DispenseActivityHistory model)
        {
            _dbContext.DispenseActivityHistory.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateDispenseActivityHistory(Guid id, DispenseActivityHistory updatedEntity)
        {
            _dbContext.DispenseActivityHistory.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteDispenseActivityHistory(Guid id)
        {
            var entityData = _dbContext.DispenseActivityHistory.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.DispenseActivityHistory.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchDispenseActivityHistory(Guid id, JsonPatchDocument<DispenseActivityHistory> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.DispenseActivityHistory.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.DispenseActivityHistory.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}