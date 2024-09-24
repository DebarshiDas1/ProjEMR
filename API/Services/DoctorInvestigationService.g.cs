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
    /// The doctorinvestigationService responsible for managing doctorinvestigation related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting doctorinvestigation information.
    /// </remarks>
    public interface IDoctorInvestigationService
    {
        /// <summary>Retrieves a specific doctorinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the doctorinvestigation</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The doctorinvestigation data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of doctorinvestigations based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of doctorinvestigations</returns>
        Task<List<DoctorInvestigation>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new doctorinvestigation</summary>
        /// <param name="model">The doctorinvestigation data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(DoctorInvestigation model);

        /// <summary>Updates a specific doctorinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the doctorinvestigation</param>
        /// <param name="updatedEntity">The doctorinvestigation data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, DoctorInvestigation updatedEntity);

        /// <summary>Updates a specific doctorinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the doctorinvestigation</param>
        /// <param name="updatedEntity">The doctorinvestigation data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<DoctorInvestigation> updatedEntity);

        /// <summary>Deletes a specific doctorinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the doctorinvestigation</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The doctorinvestigationService responsible for managing doctorinvestigation related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting doctorinvestigation information.
    /// </remarks>
    public class DoctorInvestigationService : IDoctorInvestigationService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the DoctorInvestigation class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public DoctorInvestigationService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific doctorinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the doctorinvestigation</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The doctorinvestigation data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.DoctorInvestigation.AsQueryable();
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

            string[] navigationProperties = ["InvestigationId_Investigation","DoctorId_Doctor"];
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

        /// <summary>Retrieves a list of doctorinvestigations based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of doctorinvestigations</returns>/// <exception cref="Exception"></exception>
        public async Task<List<DoctorInvestigation>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetDoctorInvestigation(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new doctorinvestigation</summary>
        /// <param name="model">The doctorinvestigation data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(DoctorInvestigation model)
        {
            model.Id = await CreateDoctorInvestigation(model);
            return model.Id;
        }

        /// <summary>Updates a specific doctorinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the doctorinvestigation</param>
        /// <param name="updatedEntity">The doctorinvestigation data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, DoctorInvestigation updatedEntity)
        {
            await UpdateDoctorInvestigation(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific doctorinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the doctorinvestigation</param>
        /// <param name="updatedEntity">The doctorinvestigation data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<DoctorInvestigation> updatedEntity)
        {
            await PatchDoctorInvestigation(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific doctorinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the doctorinvestigation</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteDoctorInvestigation(id);
            return true;
        }
        #region
        private async Task<List<DoctorInvestigation>> GetDoctorInvestigation(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.DoctorInvestigation.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<DoctorInvestigation>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(DoctorInvestigation), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<DoctorInvestigation, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateDoctorInvestigation(DoctorInvestigation model)
        {
            _dbContext.DoctorInvestigation.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateDoctorInvestigation(Guid id, DoctorInvestigation updatedEntity)
        {
            _dbContext.DoctorInvestigation.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteDoctorInvestigation(Guid id)
        {
            var entityData = _dbContext.DoctorInvestigation.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.DoctorInvestigation.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchDoctorInvestigation(Guid id, JsonPatchDocument<DoctorInvestigation> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.DoctorInvestigation.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.DoctorInvestigation.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}