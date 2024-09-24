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
    /// The pricelistService responsible for managing pricelist related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting pricelist information.
    /// </remarks>
    public interface IPriceListService
    {
        /// <summary>Retrieves a specific pricelist by its primary key</summary>
        /// <param name="id">The primary key of the pricelist</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The pricelist data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of pricelists based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of pricelists</returns>
        Task<List<PriceList>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new pricelist</summary>
        /// <param name="model">The pricelist data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(PriceList model);

        /// <summary>Updates a specific pricelist by its primary key</summary>
        /// <param name="id">The primary key of the pricelist</param>
        /// <param name="updatedEntity">The pricelist data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, PriceList updatedEntity);

        /// <summary>Updates a specific pricelist by its primary key</summary>
        /// <param name="id">The primary key of the pricelist</param>
        /// <param name="updatedEntity">The pricelist data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<PriceList> updatedEntity);

        /// <summary>Deletes a specific pricelist by its primary key</summary>
        /// <param name="id">The primary key of the pricelist</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The pricelistService responsible for managing pricelist related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting pricelist information.
    /// </remarks>
    public class PriceListService : IPriceListService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the PriceList class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public PriceListService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific pricelist by its primary key</summary>
        /// <param name="id">The primary key of the pricelist</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The pricelist data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.PriceList.AsQueryable();
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

            string[] navigationProperties = ["PriceListComponents_PriceListComponent","PriceListItems_PriceListItem","PriceListVersions_PriceListVersion"];
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

        /// <summary>Retrieves a list of pricelists based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of pricelists</returns>/// <exception cref="Exception"></exception>
        public async Task<List<PriceList>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetPriceList(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new pricelist</summary>
        /// <param name="model">The pricelist data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(PriceList model)
        {
            model.Id = await CreatePriceList(model);
            return model.Id;
        }

        /// <summary>Updates a specific pricelist by its primary key</summary>
        /// <param name="id">The primary key of the pricelist</param>
        /// <param name="updatedEntity">The pricelist data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, PriceList updatedEntity)
        {
            await UpdatePriceList(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific pricelist by its primary key</summary>
        /// <param name="id">The primary key of the pricelist</param>
        /// <param name="updatedEntity">The pricelist data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<PriceList> updatedEntity)
        {
            await PatchPriceList(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific pricelist by its primary key</summary>
        /// <param name="id">The primary key of the pricelist</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeletePriceList(id);
            return true;
        }
        #region
        private async Task<List<PriceList>> GetPriceList(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.PriceList.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<PriceList>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(PriceList), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<PriceList, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreatePriceList(PriceList model)
        {
            _dbContext.PriceList.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdatePriceList(Guid id, PriceList updatedEntity)
        {
            _dbContext.PriceList.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeletePriceList(Guid id)
        {
            var entityData = _dbContext.PriceList.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.PriceList.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchPriceList(Guid id, JsonPatchDocument<PriceList> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.PriceList.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.PriceList.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}