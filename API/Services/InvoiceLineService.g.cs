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
    /// The invoicelineService responsible for managing invoiceline related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting invoiceline information.
    /// </remarks>
    public interface IInvoiceLineService
    {
        /// <summary>Retrieves a specific invoiceline by its primary key</summary>
        /// <param name="id">The primary key of the invoiceline</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The invoiceline data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of invoicelines based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of invoicelines</returns>
        Task<List<InvoiceLine>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new invoiceline</summary>
        /// <param name="model">The invoiceline data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(InvoiceLine model);

        /// <summary>Updates a specific invoiceline by its primary key</summary>
        /// <param name="id">The primary key of the invoiceline</param>
        /// <param name="updatedEntity">The invoiceline data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, InvoiceLine updatedEntity);

        /// <summary>Updates a specific invoiceline by its primary key</summary>
        /// <param name="id">The primary key of the invoiceline</param>
        /// <param name="updatedEntity">The invoiceline data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<InvoiceLine> updatedEntity);

        /// <summary>Deletes a specific invoiceline by its primary key</summary>
        /// <param name="id">The primary key of the invoiceline</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The invoicelineService responsible for managing invoiceline related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting invoiceline information.
    /// </remarks>
    public class InvoiceLineService : IInvoiceLineService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the InvoiceLine class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public InvoiceLineService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific invoiceline by its primary key</summary>
        /// <param name="id">The primary key of the invoiceline</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The invoiceline data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.InvoiceLine.AsQueryable();
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

            string[] navigationProperties = ["InvoiceId_Invoice","ProductId_Product","ProductBatchId_ProductBatch","ProductUomId_ProductUom","GstSettingsId_GstSettings"];
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

        /// <summary>Retrieves a list of invoicelines based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of invoicelines</returns>/// <exception cref="Exception"></exception>
        public async Task<List<InvoiceLine>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetInvoiceLine(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new invoiceline</summary>
        /// <param name="model">The invoiceline data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(InvoiceLine model)
        {
            model.Id = await CreateInvoiceLine(model);
            return model.Id;
        }

        /// <summary>Updates a specific invoiceline by its primary key</summary>
        /// <param name="id">The primary key of the invoiceline</param>
        /// <param name="updatedEntity">The invoiceline data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, InvoiceLine updatedEntity)
        {
            await UpdateInvoiceLine(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific invoiceline by its primary key</summary>
        /// <param name="id">The primary key of the invoiceline</param>
        /// <param name="updatedEntity">The invoiceline data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<InvoiceLine> updatedEntity)
        {
            await PatchInvoiceLine(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific invoiceline by its primary key</summary>
        /// <param name="id">The primary key of the invoiceline</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteInvoiceLine(id);
            return true;
        }
        #region
        private async Task<List<InvoiceLine>> GetInvoiceLine(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.InvoiceLine.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<InvoiceLine>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(InvoiceLine), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<InvoiceLine, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateInvoiceLine(InvoiceLine model)
        {
            _dbContext.InvoiceLine.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateInvoiceLine(Guid id, InvoiceLine updatedEntity)
        {
            _dbContext.InvoiceLine.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteInvoiceLine(Guid id)
        {
            var entityData = _dbContext.InvoiceLine.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.InvoiceLine.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchInvoiceLine(Guid id, JsonPatchDocument<InvoiceLine> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.InvoiceLine.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.InvoiceLine.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}