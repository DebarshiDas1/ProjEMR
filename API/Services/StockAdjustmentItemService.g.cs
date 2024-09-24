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
    /// The stockadjustmentitemService responsible for managing stockadjustmentitem related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting stockadjustmentitem information.
    /// </remarks>
    public interface IStockAdjustmentItemService
    {
        /// <summary>Retrieves a specific stockadjustmentitem by its primary key</summary>
        /// <param name="id">The primary key of the stockadjustmentitem</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The stockadjustmentitem data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of stockadjustmentitems based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of stockadjustmentitems</returns>
        Task<List<StockAdjustmentItem>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new stockadjustmentitem</summary>
        /// <param name="model">The stockadjustmentitem data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(StockAdjustmentItem model);

        /// <summary>Updates a specific stockadjustmentitem by its primary key</summary>
        /// <param name="id">The primary key of the stockadjustmentitem</param>
        /// <param name="updatedEntity">The stockadjustmentitem data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, StockAdjustmentItem updatedEntity);

        /// <summary>Updates a specific stockadjustmentitem by its primary key</summary>
        /// <param name="id">The primary key of the stockadjustmentitem</param>
        /// <param name="updatedEntity">The stockadjustmentitem data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<StockAdjustmentItem> updatedEntity);

        /// <summary>Deletes a specific stockadjustmentitem by its primary key</summary>
        /// <param name="id">The primary key of the stockadjustmentitem</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The stockadjustmentitemService responsible for managing stockadjustmentitem related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting stockadjustmentitem information.
    /// </remarks>
    public class StockAdjustmentItemService : IStockAdjustmentItemService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the StockAdjustmentItem class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public StockAdjustmentItemService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific stockadjustmentitem by its primary key</summary>
        /// <param name="id">The primary key of the stockadjustmentitem</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The stockadjustmentitem data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.StockAdjustmentItem.AsQueryable();
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

            string[] navigationProperties = ["StockAdjustmentId_StockAdjustment","ProductId_Product","ProductBatchId_ProductBatch","ProductUomId_ProductUom"];
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

        /// <summary>Retrieves a list of stockadjustmentitems based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of stockadjustmentitems</returns>/// <exception cref="Exception"></exception>
        public async Task<List<StockAdjustmentItem>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetStockAdjustmentItem(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new stockadjustmentitem</summary>
        /// <param name="model">The stockadjustmentitem data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(StockAdjustmentItem model)
        {
            model.Id = await CreateStockAdjustmentItem(model);
            return model.Id;
        }

        /// <summary>Updates a specific stockadjustmentitem by its primary key</summary>
        /// <param name="id">The primary key of the stockadjustmentitem</param>
        /// <param name="updatedEntity">The stockadjustmentitem data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, StockAdjustmentItem updatedEntity)
        {
            await UpdateStockAdjustmentItem(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific stockadjustmentitem by its primary key</summary>
        /// <param name="id">The primary key of the stockadjustmentitem</param>
        /// <param name="updatedEntity">The stockadjustmentitem data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<StockAdjustmentItem> updatedEntity)
        {
            await PatchStockAdjustmentItem(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific stockadjustmentitem by its primary key</summary>
        /// <param name="id">The primary key of the stockadjustmentitem</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteStockAdjustmentItem(id);
            return true;
        }
        #region
        private async Task<List<StockAdjustmentItem>> GetStockAdjustmentItem(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.StockAdjustmentItem.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<StockAdjustmentItem>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(StockAdjustmentItem), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<StockAdjustmentItem, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateStockAdjustmentItem(StockAdjustmentItem model)
        {
            _dbContext.StockAdjustmentItem.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateStockAdjustmentItem(Guid id, StockAdjustmentItem updatedEntity)
        {
            _dbContext.StockAdjustmentItem.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteStockAdjustmentItem(Guid id)
        {
            var entityData = _dbContext.StockAdjustmentItem.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.StockAdjustmentItem.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchStockAdjustmentItem(Guid id, JsonPatchDocument<StockAdjustmentItem> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.StockAdjustmentItem.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.StockAdjustmentItem.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}