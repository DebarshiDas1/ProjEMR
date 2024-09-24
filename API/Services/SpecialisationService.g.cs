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
    /// The specialisationService responsible for managing specialisation related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting specialisation information.
    /// </remarks>
    public interface ISpecialisationService
    {
        /// <summary>Retrieves a specific specialisation by its primary key</summary>
        /// <param name="id">The primary key of the specialisation</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The specialisation data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of specialisations based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of specialisations</returns>
        Task<List<Specialisation>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new specialisation</summary>
        /// <param name="model">The specialisation data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(Specialisation model);

        /// <summary>Updates a specific specialisation by its primary key</summary>
        /// <param name="id">The primary key of the specialisation</param>
        /// <param name="updatedEntity">The specialisation data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, Specialisation updatedEntity);

        /// <summary>Updates a specific specialisation by its primary key</summary>
        /// <param name="id">The primary key of the specialisation</param>
        /// <param name="updatedEntity">The specialisation data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<Specialisation> updatedEntity);

        /// <summary>Deletes a specific specialisation by its primary key</summary>
        /// <param name="id">The primary key of the specialisation</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The specialisationService responsible for managing specialisation related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting specialisation information.
    /// </remarks>
    public class SpecialisationService : ISpecialisationService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the Specialisation class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public SpecialisationService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific specialisation by its primary key</summary>
        /// <param name="id">The primary key of the specialisation</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The specialisation data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.Specialisation.AsQueryable();
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

        /// <summary>Retrieves a list of specialisations based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of specialisations</returns>/// <exception cref="Exception"></exception>
        public async Task<List<Specialisation>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetSpecialisation(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new specialisation</summary>
        /// <param name="model">The specialisation data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(Specialisation model)
        {
            model.Id = await CreateSpecialisation(model);
            return model.Id;
        }

        /// <summary>Updates a specific specialisation by its primary key</summary>
        /// <param name="id">The primary key of the specialisation</param>
        /// <param name="updatedEntity">The specialisation data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, Specialisation updatedEntity)
        {
            await UpdateSpecialisation(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific specialisation by its primary key</summary>
        /// <param name="id">The primary key of the specialisation</param>
        /// <param name="updatedEntity">The specialisation data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<Specialisation> updatedEntity)
        {
            await PatchSpecialisation(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific specialisation by its primary key</summary>
        /// <param name="id">The primary key of the specialisation</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteSpecialisation(id);
            return true;
        }
        #region
        private async Task<List<Specialisation>> GetSpecialisation(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.Specialisation.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<Specialisation>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(Specialisation), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<Specialisation, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateSpecialisation(Specialisation model)
        {
            _dbContext.Specialisation.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateSpecialisation(Guid id, Specialisation updatedEntity)
        {
            _dbContext.Specialisation.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteSpecialisation(Guid id)
        {
            var entityData = _dbContext.Specialisation.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.Specialisation.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchSpecialisation(Guid id, JsonPatchDocument<Specialisation> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.Specialisation.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.Specialisation.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}