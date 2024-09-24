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
    /// The patientallergyService responsible for managing patientallergy related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting patientallergy information.
    /// </remarks>
    public interface IPatientAllergyService
    {
        /// <summary>Retrieves a specific patientallergy by its primary key</summary>
        /// <param name="id">The primary key of the patientallergy</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The patientallergy data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of patientallergys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of patientallergys</returns>
        Task<List<PatientAllergy>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new patientallergy</summary>
        /// <param name="model">The patientallergy data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(PatientAllergy model);

        /// <summary>Updates a specific patientallergy by its primary key</summary>
        /// <param name="id">The primary key of the patientallergy</param>
        /// <param name="updatedEntity">The patientallergy data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, PatientAllergy updatedEntity);

        /// <summary>Updates a specific patientallergy by its primary key</summary>
        /// <param name="id">The primary key of the patientallergy</param>
        /// <param name="updatedEntity">The patientallergy data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<PatientAllergy> updatedEntity);

        /// <summary>Deletes a specific patientallergy by its primary key</summary>
        /// <param name="id">The primary key of the patientallergy</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The patientallergyService responsible for managing patientallergy related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting patientallergy information.
    /// </remarks>
    public class PatientAllergyService : IPatientAllergyService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the PatientAllergy class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public PatientAllergyService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific patientallergy by its primary key</summary>
        /// <param name="id">The primary key of the patientallergy</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The patientallergy data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.PatientAllergy.AsQueryable();
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

        /// <summary>Retrieves a list of patientallergys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of patientallergys</returns>/// <exception cref="Exception"></exception>
        public async Task<List<PatientAllergy>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetPatientAllergy(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new patientallergy</summary>
        /// <param name="model">The patientallergy data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(PatientAllergy model)
        {
            model.Id = await CreatePatientAllergy(model);
            return model.Id;
        }

        /// <summary>Updates a specific patientallergy by its primary key</summary>
        /// <param name="id">The primary key of the patientallergy</param>
        /// <param name="updatedEntity">The patientallergy data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, PatientAllergy updatedEntity)
        {
            await UpdatePatientAllergy(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific patientallergy by its primary key</summary>
        /// <param name="id">The primary key of the patientallergy</param>
        /// <param name="updatedEntity">The patientallergy data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<PatientAllergy> updatedEntity)
        {
            await PatchPatientAllergy(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific patientallergy by its primary key</summary>
        /// <param name="id">The primary key of the patientallergy</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeletePatientAllergy(id);
            return true;
        }
        #region
        private async Task<List<PatientAllergy>> GetPatientAllergy(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.PatientAllergy.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<PatientAllergy>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(PatientAllergy), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<PatientAllergy, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreatePatientAllergy(PatientAllergy model)
        {
            _dbContext.PatientAllergy.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdatePatientAllergy(Guid id, PatientAllergy updatedEntity)
        {
            _dbContext.PatientAllergy.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeletePatientAllergy(Guid id)
        {
            var entityData = _dbContext.PatientAllergy.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.PatientAllergy.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchPatientAllergy(Guid id, JsonPatchDocument<PatientAllergy> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.PatientAllergy.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.PatientAllergy.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}