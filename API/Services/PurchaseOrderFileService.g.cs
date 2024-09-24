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
    /// The purchaseorderfileService responsible for managing purchaseorderfile related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting purchaseorderfile information.
    /// </remarks>
    public interface IPurchaseOrderFileService
    {
        /// <summary>Retrieves a specific purchaseorderfile by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorderfile</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The purchaseorderfile data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of purchaseorderfiles based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of purchaseorderfiles</returns>
        Task<List<PurchaseOrderFile>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new purchaseorderfile</summary>
        /// <param name="model">The purchaseorderfile data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(PurchaseOrderFile model);

        /// <summary>Updates a specific purchaseorderfile by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorderfile</param>
        /// <param name="updatedEntity">The purchaseorderfile data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, PurchaseOrderFile updatedEntity);

        /// <summary>Updates a specific purchaseorderfile by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorderfile</param>
        /// <param name="updatedEntity">The purchaseorderfile data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<PurchaseOrderFile> updatedEntity);

        /// <summary>Deletes a specific purchaseorderfile by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorderfile</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The purchaseorderfileService responsible for managing purchaseorderfile related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting purchaseorderfile information.
    /// </remarks>
    public class PurchaseOrderFileService : IPurchaseOrderFileService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the PurchaseOrderFile class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public PurchaseOrderFileService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific purchaseorderfile by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorderfile</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The purchaseorderfile data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.PurchaseOrderFile.AsQueryable();
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

            string[] navigationProperties = ["PurchaseOrderId_PurchaseOrder"];
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

        /// <summary>Retrieves a list of purchaseorderfiles based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of purchaseorderfiles</returns>/// <exception cref="Exception"></exception>
        public async Task<List<PurchaseOrderFile>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetPurchaseOrderFile(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new purchaseorderfile</summary>
        /// <param name="model">The purchaseorderfile data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(PurchaseOrderFile model)
        {
            model.Id = await CreatePurchaseOrderFile(model);
            return model.Id;
        }

        /// <summary>Updates a specific purchaseorderfile by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorderfile</param>
        /// <param name="updatedEntity">The purchaseorderfile data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, PurchaseOrderFile updatedEntity)
        {
            await UpdatePurchaseOrderFile(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific purchaseorderfile by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorderfile</param>
        /// <param name="updatedEntity">The purchaseorderfile data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<PurchaseOrderFile> updatedEntity)
        {
            await PatchPurchaseOrderFile(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific purchaseorderfile by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorderfile</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeletePurchaseOrderFile(id);
            return true;
        }
        #region
        private async Task<List<PurchaseOrderFile>> GetPurchaseOrderFile(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.PurchaseOrderFile.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<PurchaseOrderFile>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(PurchaseOrderFile), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<PurchaseOrderFile, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreatePurchaseOrderFile(PurchaseOrderFile model)
        {
            _dbContext.PurchaseOrderFile.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdatePurchaseOrderFile(Guid id, PurchaseOrderFile updatedEntity)
        {
            _dbContext.PurchaseOrderFile.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeletePurchaseOrderFile(Guid id)
        {
            var entityData = _dbContext.PurchaseOrderFile.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.PurchaseOrderFile.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchPurchaseOrderFile(Guid id, JsonPatchDocument<PurchaseOrderFile> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.PurchaseOrderFile.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.PurchaseOrderFile.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}