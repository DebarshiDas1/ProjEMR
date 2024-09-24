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
    /// The currencyService responsible for managing currency related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting currency information.
    /// </remarks>
    public interface ICurrencyService
    {
        /// <summary>Retrieves a specific currency by its primary key</summary>
        /// <param name="id">The primary key of the currency</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The currency data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of currencys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of currencys</returns>
        Task<List<Currency>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new currency</summary>
        /// <param name="model">The currency data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(Currency model);

        /// <summary>Updates a specific currency by its primary key</summary>
        /// <param name="id">The primary key of the currency</param>
        /// <param name="updatedEntity">The currency data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, Currency updatedEntity);

        /// <summary>Updates a specific currency by its primary key</summary>
        /// <param name="id">The primary key of the currency</param>
        /// <param name="updatedEntity">The currency data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<Currency> updatedEntity);

        /// <summary>Deletes a specific currency by its primary key</summary>
        /// <param name="id">The primary key of the currency</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The currencyService responsible for managing currency related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting currency information.
    /// </remarks>
    public class CurrencyService : ICurrencyService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the Currency class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public CurrencyService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific currency by its primary key</summary>
        /// <param name="id">The primary key of the currency</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The currency data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.Currency.AsQueryable();
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

        /// <summary>Retrieves a list of currencys based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of currencys</returns>/// <exception cref="Exception"></exception>
        public async Task<List<Currency>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetCurrency(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new currency</summary>
        /// <param name="model">The currency data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(Currency model)
        {
            model.Id = await CreateCurrency(model);
            return model.Id;
        }

        /// <summary>Updates a specific currency by its primary key</summary>
        /// <param name="id">The primary key of the currency</param>
        /// <param name="updatedEntity">The currency data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, Currency updatedEntity)
        {
            await UpdateCurrency(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific currency by its primary key</summary>
        /// <param name="id">The primary key of the currency</param>
        /// <param name="updatedEntity">The currency data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<Currency> updatedEntity)
        {
            await PatchCurrency(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific currency by its primary key</summary>
        /// <param name="id">The primary key of the currency</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteCurrency(id);
            return true;
        }
        #region
        private async Task<List<Currency>> GetCurrency(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.Currency.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<Currency>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(Currency), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<Currency, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateCurrency(Currency model)
        {
            _dbContext.Currency.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateCurrency(Guid id, Currency updatedEntity)
        {
            _dbContext.Currency.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteCurrency(Guid id)
        {
            var entityData = _dbContext.Currency.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.Currency.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchCurrency(Guid id, JsonPatchDocument<Currency> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.Currency.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.Currency.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}