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
    /// The clinicalparametervalueService responsible for managing clinicalparametervalue related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting clinicalparametervalue information.
    /// </remarks>
    public interface IClinicalParameterValueService
    {
        /// <summary>Retrieves a specific clinicalparametervalue by its primary key</summary>
        /// <param name="id">The primary key of the clinicalparametervalue</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The clinicalparametervalue data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of clinicalparametervalues based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of clinicalparametervalues</returns>
        Task<List<ClinicalParameterValue>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new clinicalparametervalue</summary>
        /// <param name="model">The clinicalparametervalue data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(ClinicalParameterValue model);

        /// <summary>Updates a specific clinicalparametervalue by its primary key</summary>
        /// <param name="id">The primary key of the clinicalparametervalue</param>
        /// <param name="updatedEntity">The clinicalparametervalue data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, ClinicalParameterValue updatedEntity);

        /// <summary>Updates a specific clinicalparametervalue by its primary key</summary>
        /// <param name="id">The primary key of the clinicalparametervalue</param>
        /// <param name="updatedEntity">The clinicalparametervalue data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<ClinicalParameterValue> updatedEntity);

        /// <summary>Deletes a specific clinicalparametervalue by its primary key</summary>
        /// <param name="id">The primary key of the clinicalparametervalue</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The clinicalparametervalueService responsible for managing clinicalparametervalue related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting clinicalparametervalue information.
    /// </remarks>
    public class ClinicalParameterValueService : IClinicalParameterValueService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the ClinicalParameterValue class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public ClinicalParameterValueService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific clinicalparametervalue by its primary key</summary>
        /// <param name="id">The primary key of the clinicalparametervalue</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The clinicalparametervalue data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.ClinicalParameterValue.AsQueryable();
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

        /// <summary>Retrieves a list of clinicalparametervalues based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of clinicalparametervalues</returns>/// <exception cref="Exception"></exception>
        public async Task<List<ClinicalParameterValue>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetClinicalParameterValue(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new clinicalparametervalue</summary>
        /// <param name="model">The clinicalparametervalue data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(ClinicalParameterValue model)
        {
            model.Id = await CreateClinicalParameterValue(model);
            return model.Id;
        }

        /// <summary>Updates a specific clinicalparametervalue by its primary key</summary>
        /// <param name="id">The primary key of the clinicalparametervalue</param>
        /// <param name="updatedEntity">The clinicalparametervalue data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, ClinicalParameterValue updatedEntity)
        {
            await UpdateClinicalParameterValue(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific clinicalparametervalue by its primary key</summary>
        /// <param name="id">The primary key of the clinicalparametervalue</param>
        /// <param name="updatedEntity">The clinicalparametervalue data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<ClinicalParameterValue> updatedEntity)
        {
            await PatchClinicalParameterValue(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific clinicalparametervalue by its primary key</summary>
        /// <param name="id">The primary key of the clinicalparametervalue</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteClinicalParameterValue(id);
            return true;
        }
        #region
        private async Task<List<ClinicalParameterValue>> GetClinicalParameterValue(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.ClinicalParameterValue.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<ClinicalParameterValue>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(ClinicalParameterValue), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<ClinicalParameterValue, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateClinicalParameterValue(ClinicalParameterValue model)
        {
            _dbContext.ClinicalParameterValue.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateClinicalParameterValue(Guid id, ClinicalParameterValue updatedEntity)
        {
            _dbContext.ClinicalParameterValue.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteClinicalParameterValue(Guid id)
        {
            var entityData = _dbContext.ClinicalParameterValue.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.ClinicalParameterValue.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchClinicalParameterValue(Guid id, JsonPatchDocument<ClinicalParameterValue> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.ClinicalParameterValue.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.ClinicalParameterValue.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}