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
    /// The patientpregnancyService responsible for managing patientpregnancy related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting patientpregnancy information.
    /// </remarks>
    public interface IPatientPregnancyService
    {
        /// <summary>Retrieves a specific patientpregnancy by its primary key</summary>
        /// <param name="id">The primary key of the patientpregnancy</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The patientpregnancy data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of patientpregnancys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of patientpregnancys</returns>
        Task<List<PatientPregnancy>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new patientpregnancy</summary>
        /// <param name="model">The patientpregnancy data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(PatientPregnancy model);

        /// <summary>Updates a specific patientpregnancy by its primary key</summary>
        /// <param name="id">The primary key of the patientpregnancy</param>
        /// <param name="updatedEntity">The patientpregnancy data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, PatientPregnancy updatedEntity);

        /// <summary>Updates a specific patientpregnancy by its primary key</summary>
        /// <param name="id">The primary key of the patientpregnancy</param>
        /// <param name="updatedEntity">The patientpregnancy data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<PatientPregnancy> updatedEntity);

        /// <summary>Deletes a specific patientpregnancy by its primary key</summary>
        /// <param name="id">The primary key of the patientpregnancy</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The patientpregnancyService responsible for managing patientpregnancy related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting patientpregnancy information.
    /// </remarks>
    public class PatientPregnancyService : IPatientPregnancyService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the PatientPregnancy class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public PatientPregnancyService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific patientpregnancy by its primary key</summary>
        /// <param name="id">The primary key of the patientpregnancy</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The patientpregnancy data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.PatientPregnancy.AsQueryable();
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

        /// <summary>Retrieves a list of patientpregnancys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of patientpregnancys</returns>/// <exception cref="Exception"></exception>
        public async Task<List<PatientPregnancy>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetPatientPregnancy(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new patientpregnancy</summary>
        /// <param name="model">The patientpregnancy data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(PatientPregnancy model)
        {
            model.Id = await CreatePatientPregnancy(model);
            return model.Id;
        }

        /// <summary>Updates a specific patientpregnancy by its primary key</summary>
        /// <param name="id">The primary key of the patientpregnancy</param>
        /// <param name="updatedEntity">The patientpregnancy data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, PatientPregnancy updatedEntity)
        {
            await UpdatePatientPregnancy(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific patientpregnancy by its primary key</summary>
        /// <param name="id">The primary key of the patientpregnancy</param>
        /// <param name="updatedEntity">The patientpregnancy data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<PatientPregnancy> updatedEntity)
        {
            await PatchPatientPregnancy(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific patientpregnancy by its primary key</summary>
        /// <param name="id">The primary key of the patientpregnancy</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeletePatientPregnancy(id);
            return true;
        }
        #region
        private async Task<List<PatientPregnancy>> GetPatientPregnancy(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.PatientPregnancy.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<PatientPregnancy>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(PatientPregnancy), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<PatientPregnancy, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreatePatientPregnancy(PatientPregnancy model)
        {
            _dbContext.PatientPregnancy.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdatePatientPregnancy(Guid id, PatientPregnancy updatedEntity)
        {
            _dbContext.PatientPregnancy.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeletePatientPregnancy(Guid id)
        {
            var entityData = _dbContext.PatientPregnancy.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.PatientPregnancy.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchPatientPregnancy(Guid id, JsonPatchDocument<PatientPregnancy> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.PatientPregnancy.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.PatientPregnancy.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}