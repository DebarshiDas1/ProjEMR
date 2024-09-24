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
    /// The purchaseorderService responsible for managing purchaseorder related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting purchaseorder information.
    /// </remarks>
    public interface IPurchaseOrderService
    {
        /// <summary>Retrieves a specific purchaseorder by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorder</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The purchaseorder data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of purchaseorders based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of purchaseorders</returns>
        Task<List<PurchaseOrder>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new purchaseorder</summary>
        /// <param name="model">The purchaseorder data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(PurchaseOrder model);

        /// <summary>Updates a specific purchaseorder by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorder</param>
        /// <param name="updatedEntity">The purchaseorder data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, PurchaseOrder updatedEntity);

        /// <summary>Updates a specific purchaseorder by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorder</param>
        /// <param name="updatedEntity">The purchaseorder data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<PurchaseOrder> updatedEntity);

        /// <summary>Deletes a specific purchaseorder by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorder</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The purchaseorderService responsible for managing purchaseorder related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting purchaseorder information.
    /// </remarks>
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the PurchaseOrder class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public PurchaseOrderService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific purchaseorder by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorder</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The purchaseorder data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.PurchaseOrder.AsQueryable();
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

            string[] navigationProperties = ["LocationId_Location","PurchaseOrderFile_PurchaseOrderFile","Requisition_Requisition"];
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

        /// <summary>Retrieves a list of purchaseorders based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of purchaseorders</returns>/// <exception cref="Exception"></exception>
        public async Task<List<PurchaseOrder>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetPurchaseOrder(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new purchaseorder</summary>
        /// <param name="model">The purchaseorder data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(PurchaseOrder model)
        {
            model.Id = await CreatePurchaseOrder(model);
            return model.Id;
        }

        /// <summary>Updates a specific purchaseorder by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorder</param>
        /// <param name="updatedEntity">The purchaseorder data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, PurchaseOrder updatedEntity)
        {
            await UpdatePurchaseOrder(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific purchaseorder by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorder</param>
        /// <param name="updatedEntity">The purchaseorder data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<PurchaseOrder> updatedEntity)
        {
            await PatchPurchaseOrder(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific purchaseorder by its primary key</summary>
        /// <param name="id">The primary key of the purchaseorder</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeletePurchaseOrder(id);
            return true;
        }
        #region
        private async Task<List<PurchaseOrder>> GetPurchaseOrder(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.PurchaseOrder.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<PurchaseOrder>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(PurchaseOrder), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<PurchaseOrder, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreatePurchaseOrder(PurchaseOrder model)
        {
            _dbContext.PurchaseOrder.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdatePurchaseOrder(Guid id, PurchaseOrder updatedEntity)
        {
            _dbContext.PurchaseOrder.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeletePurchaseOrder(Guid id)
        {
            var entityData = _dbContext.PurchaseOrder.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.PurchaseOrder.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchPurchaseOrder(Guid id, JsonPatchDocument<PurchaseOrder> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.PurchaseOrder.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.PurchaseOrder.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}