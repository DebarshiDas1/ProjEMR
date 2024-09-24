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
    /// The productbatchService responsible for managing productbatch related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting productbatch information.
    /// </remarks>
    public interface IProductBatchService
    {
        /// <summary>Retrieves a specific productbatch by its primary key</summary>
        /// <param name="id">The primary key of the productbatch</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The productbatch data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of productbatchs based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of productbatchs</returns>
        Task<List<ProductBatch>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new productbatch</summary>
        /// <param name="model">The productbatch data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(ProductBatch model);

        /// <summary>Updates a specific productbatch by its primary key</summary>
        /// <param name="id">The primary key of the productbatch</param>
        /// <param name="updatedEntity">The productbatch data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, ProductBatch updatedEntity);

        /// <summary>Updates a specific productbatch by its primary key</summary>
        /// <param name="id">The primary key of the productbatch</param>
        /// <param name="updatedEntity">The productbatch data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<ProductBatch> updatedEntity);

        /// <summary>Deletes a specific productbatch by its primary key</summary>
        /// <param name="id">The primary key of the productbatch</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The productbatchService responsible for managing productbatch related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting productbatch information.
    /// </remarks>
    public class ProductBatchService : IProductBatchService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the ProductBatch class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public ProductBatchService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific productbatch by its primary key</summary>
        /// <param name="id">The primary key of the productbatch</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The productbatch data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.ProductBatch.AsQueryable();
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

            string[] navigationProperties = ["ProductUomId_ProductUom","Location_Location","Product_Product","InvoiceLineId_InvoiceLine"];
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

        /// <summary>Retrieves a list of productbatchs based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of productbatchs</returns>/// <exception cref="Exception"></exception>
        public async Task<List<ProductBatch>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetProductBatch(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new productbatch</summary>
        /// <param name="model">The productbatch data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(ProductBatch model)
        {
            model.Id = await CreateProductBatch(model);
            return model.Id;
        }

        /// <summary>Updates a specific productbatch by its primary key</summary>
        /// <param name="id">The primary key of the productbatch</param>
        /// <param name="updatedEntity">The productbatch data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, ProductBatch updatedEntity)
        {
            await UpdateProductBatch(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific productbatch by its primary key</summary>
        /// <param name="id">The primary key of the productbatch</param>
        /// <param name="updatedEntity">The productbatch data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<ProductBatch> updatedEntity)
        {
            await PatchProductBatch(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific productbatch by its primary key</summary>
        /// <param name="id">The primary key of the productbatch</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteProductBatch(id);
            return true;
        }
        #region
        private async Task<List<ProductBatch>> GetProductBatch(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.ProductBatch.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<ProductBatch>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(ProductBatch), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<ProductBatch, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateProductBatch(ProductBatch model)
        {
            _dbContext.ProductBatch.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateProductBatch(Guid id, ProductBatch updatedEntity)
        {
            _dbContext.ProductBatch.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteProductBatch(Guid id)
        {
            var entityData = _dbContext.ProductBatch.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.ProductBatch.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchProductBatch(Guid id, JsonPatchDocument<ProductBatch> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.ProductBatch.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.ProductBatch.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}