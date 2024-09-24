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
    /// The pricelistversionService responsible for managing pricelistversion related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting pricelistversion information.
    /// </remarks>
    public interface IPriceListVersionService
    {
        /// <summary>Retrieves a specific pricelistversion by its primary key</summary>
        /// <param name="id">The primary key of the pricelistversion</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The pricelistversion data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of pricelistversions based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of pricelistversions</returns>
        Task<List<PriceListVersion>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new pricelistversion</summary>
        /// <param name="model">The pricelistversion data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(PriceListVersion model);

        /// <summary>Updates a specific pricelistversion by its primary key</summary>
        /// <param name="id">The primary key of the pricelistversion</param>
        /// <param name="updatedEntity">The pricelistversion data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, PriceListVersion updatedEntity);

        /// <summary>Updates a specific pricelistversion by its primary key</summary>
        /// <param name="id">The primary key of the pricelistversion</param>
        /// <param name="updatedEntity">The pricelistversion data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<PriceListVersion> updatedEntity);

        /// <summary>Deletes a specific pricelistversion by its primary key</summary>
        /// <param name="id">The primary key of the pricelistversion</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The pricelistversionService responsible for managing pricelistversion related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting pricelistversion information.
    /// </remarks>
    public class PriceListVersionService : IPriceListVersionService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the PriceListVersion class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public PriceListVersionService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific pricelistversion by its primary key</summary>
        /// <param name="id">The primary key of the pricelistversion</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The pricelistversion data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.PriceListVersion.AsQueryable();
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

            string[] navigationProperties = ["PriceListId_PriceList"];
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

        /// <summary>Retrieves a list of pricelistversions based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of pricelistversions</returns>/// <exception cref="Exception"></exception>
        public async Task<List<PriceListVersion>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetPriceListVersion(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new pricelistversion</summary>
        /// <param name="model">The pricelistversion data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(PriceListVersion model)
        {
            model.Id = await CreatePriceListVersion(model);
            return model.Id;
        }

        /// <summary>Updates a specific pricelistversion by its primary key</summary>
        /// <param name="id">The primary key of the pricelistversion</param>
        /// <param name="updatedEntity">The pricelistversion data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, PriceListVersion updatedEntity)
        {
            await UpdatePriceListVersion(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific pricelistversion by its primary key</summary>
        /// <param name="id">The primary key of the pricelistversion</param>
        /// <param name="updatedEntity">The pricelistversion data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<PriceListVersion> updatedEntity)
        {
            await PatchPriceListVersion(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific pricelistversion by its primary key</summary>
        /// <param name="id">The primary key of the pricelistversion</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeletePriceListVersion(id);
            return true;
        }
        #region
        private async Task<List<PriceListVersion>> GetPriceListVersion(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.PriceListVersion.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<PriceListVersion>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(PriceListVersion), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<PriceListVersion, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreatePriceListVersion(PriceListVersion model)
        {
            _dbContext.PriceListVersion.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdatePriceListVersion(Guid id, PriceListVersion updatedEntity)
        {
            _dbContext.PriceListVersion.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeletePriceListVersion(Guid id)
        {
            var entityData = _dbContext.PriceListVersion.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.PriceListVersion.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchPriceListVersion(Guid id, JsonPatchDocument<PriceListVersion> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.PriceListVersion.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.PriceListVersion.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}