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
    /// The productService responsible for managing product related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting product information.
    /// </remarks>
    public interface IProductService
    {
        /// <summary>Retrieves a specific product by its primary key</summary>
        /// <param name="id">The primary key of the product</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The product data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of products based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of products</returns>
        Task<List<Product>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new product</summary>
        /// <param name="model">The product data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(Product model);

        /// <summary>Updates a specific product by its primary key</summary>
        /// <param name="id">The primary key of the product</param>
        /// <param name="updatedEntity">The product data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, Product updatedEntity);

        /// <summary>Updates a specific product by its primary key</summary>
        /// <param name="id">The primary key of the product</param>
        /// <param name="updatedEntity">The product data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<Product> updatedEntity);

        /// <summary>Deletes a specific product by its primary key</summary>
        /// <param name="id">The primary key of the product</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The productService responsible for managing product related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting product information.
    /// </remarks>
    public class ProductService : IProductService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the Product class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public ProductService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific product by its primary key</summary>
        /// <param name="id">The primary key of the product</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The product data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.Product.AsQueryable();
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

            string[] navigationProperties = ["ProductCategoryId_ProductCategory","MedicationId_Medication","Investigation_Investigation","Procedure_Procedure","Contact_Address","Formulation_Formulation","ProductManufacture_ProductManufacture","ProductClassification_ProductClassification","Uom_Uom","GstSettings_GstSettings","ProductUoms_ProductUom","FinanceSettings_FinanceSetting","ProductBatch_ProductBatch"];
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

        /// <summary>Retrieves a list of products based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of products</returns>/// <exception cref="Exception"></exception>
        public async Task<List<Product>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetProduct(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new product</summary>
        /// <param name="model">The product data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(Product model)
        {
            model.Id = await CreateProduct(model);
            return model.Id;
        }

        /// <summary>Updates a specific product by its primary key</summary>
        /// <param name="id">The primary key of the product</param>
        /// <param name="updatedEntity">The product data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, Product updatedEntity)
        {
            await UpdateProduct(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific product by its primary key</summary>
        /// <param name="id">The primary key of the product</param>
        /// <param name="updatedEntity">The product data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<Product> updatedEntity)
        {
            await PatchProduct(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific product by its primary key</summary>
        /// <param name="id">The primary key of the product</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteProduct(id);
            return true;
        }
        #region
        private async Task<List<Product>> GetProduct(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.Product.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<Product>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(Product), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<Product, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateProduct(Product model)
        {
            _dbContext.Product.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateProduct(Guid id, Product updatedEntity)
        {
            _dbContext.Product.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteProduct(Guid id)
        {
            var entityData = _dbContext.Product.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.Product.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchProduct(Guid id, JsonPatchDocument<Product> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.Product.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.Product.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}