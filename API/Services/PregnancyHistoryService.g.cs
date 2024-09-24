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
    /// The pregnancyhistoryService responsible for managing pregnancyhistory related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting pregnancyhistory information.
    /// </remarks>
    public interface IPregnancyHistoryService
    {
        /// <summary>Retrieves a specific pregnancyhistory by its primary key</summary>
        /// <param name="id">The primary key of the pregnancyhistory</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The pregnancyhistory data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of pregnancyhistorys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of pregnancyhistorys</returns>
        Task<List<PregnancyHistory>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new pregnancyhistory</summary>
        /// <param name="model">The pregnancyhistory data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(PregnancyHistory model);

        /// <summary>Updates a specific pregnancyhistory by its primary key</summary>
        /// <param name="id">The primary key of the pregnancyhistory</param>
        /// <param name="updatedEntity">The pregnancyhistory data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, PregnancyHistory updatedEntity);

        /// <summary>Updates a specific pregnancyhistory by its primary key</summary>
        /// <param name="id">The primary key of the pregnancyhistory</param>
        /// <param name="updatedEntity">The pregnancyhistory data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<PregnancyHistory> updatedEntity);

        /// <summary>Deletes a specific pregnancyhistory by its primary key</summary>
        /// <param name="id">The primary key of the pregnancyhistory</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The pregnancyhistoryService responsible for managing pregnancyhistory related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting pregnancyhistory information.
    /// </remarks>
    public class PregnancyHistoryService : IPregnancyHistoryService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the PregnancyHistory class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public PregnancyHistoryService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific pregnancyhistory by its primary key</summary>
        /// <param name="id">The primary key of the pregnancyhistory</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The pregnancyhistory data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.PregnancyHistory.AsQueryable();
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

            string[] navigationProperties = ["PatientId_Patient"];
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

        /// <summary>Retrieves a list of pregnancyhistorys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of pregnancyhistorys</returns>/// <exception cref="Exception"></exception>
        public async Task<List<PregnancyHistory>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetPregnancyHistory(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new pregnancyhistory</summary>
        /// <param name="model">The pregnancyhistory data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(PregnancyHistory model)
        {
            model.Id = await CreatePregnancyHistory(model);
            return model.Id;
        }

        /// <summary>Updates a specific pregnancyhistory by its primary key</summary>
        /// <param name="id">The primary key of the pregnancyhistory</param>
        /// <param name="updatedEntity">The pregnancyhistory data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, PregnancyHistory updatedEntity)
        {
            await UpdatePregnancyHistory(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific pregnancyhistory by its primary key</summary>
        /// <param name="id">The primary key of the pregnancyhistory</param>
        /// <param name="updatedEntity">The pregnancyhistory data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<PregnancyHistory> updatedEntity)
        {
            await PatchPregnancyHistory(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific pregnancyhistory by its primary key</summary>
        /// <param name="id">The primary key of the pregnancyhistory</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeletePregnancyHistory(id);
            return true;
        }
        #region
        private async Task<List<PregnancyHistory>> GetPregnancyHistory(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.PregnancyHistory.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<PregnancyHistory>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(PregnancyHistory), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<PregnancyHistory, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreatePregnancyHistory(PregnancyHistory model)
        {
            _dbContext.PregnancyHistory.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdatePregnancyHistory(Guid id, PregnancyHistory updatedEntity)
        {
            _dbContext.PregnancyHistory.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeletePregnancyHistory(Guid id)
        {
            var entityData = _dbContext.PregnancyHistory.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.PregnancyHistory.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchPregnancyHistory(Guid id, JsonPatchDocument<PregnancyHistory> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.PregnancyHistory.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.PregnancyHistory.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}