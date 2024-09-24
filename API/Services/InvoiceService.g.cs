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
    /// The invoiceService responsible for managing invoice related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting invoice information.
    /// </remarks>
    public interface IInvoiceService
    {
        /// <summary>Retrieves a specific invoice by its primary key</summary>
        /// <param name="id">The primary key of the invoice</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The invoice data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of invoices based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of invoices</returns>
        Task<List<Invoice>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new invoice</summary>
        /// <param name="model">The invoice data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(Invoice model);

        /// <summary>Updates a specific invoice by its primary key</summary>
        /// <param name="id">The primary key of the invoice</param>
        /// <param name="updatedEntity">The invoice data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, Invoice updatedEntity);

        /// <summary>Updates a specific invoice by its primary key</summary>
        /// <param name="id">The primary key of the invoice</param>
        /// <param name="updatedEntity">The invoice data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<Invoice> updatedEntity);

        /// <summary>Deletes a specific invoice by its primary key</summary>
        /// <param name="id">The primary key of the invoice</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The invoiceService responsible for managing invoice related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting invoice information.
    /// </remarks>
    public class InvoiceService : IInvoiceService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the Invoice class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public InvoiceService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific invoice by its primary key</summary>
        /// <param name="id">The primary key of the invoice</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The invoice data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.Invoice.AsQueryable();
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

            string[] navigationProperties = ["VisitId_Visit","PatientId_Patient","DoctorId_Doctor","LocationId_Location","Payment_Payment","Appointment_Appointment","DayVisit_DayVisit","ReferredById_Address","InvoiceFiles_InvoiceFile","PayorId_Address","InvoiceLineId_InvoiceLine","Dispense_Dispense"];
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

        /// <summary>Retrieves a list of invoices based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of invoices</returns>/// <exception cref="Exception"></exception>
        public async Task<List<Invoice>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetInvoice(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new invoice</summary>
        /// <param name="model">The invoice data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(Invoice model)
        {
            model.Id = await CreateInvoice(model);
            return model.Id;
        }

        /// <summary>Updates a specific invoice by its primary key</summary>
        /// <param name="id">The primary key of the invoice</param>
        /// <param name="updatedEntity">The invoice data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, Invoice updatedEntity)
        {
            await UpdateInvoice(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific invoice by its primary key</summary>
        /// <param name="id">The primary key of the invoice</param>
        /// <param name="updatedEntity">The invoice data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<Invoice> updatedEntity)
        {
            await PatchInvoice(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific invoice by its primary key</summary>
        /// <param name="id">The primary key of the invoice</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteInvoice(id);
            return true;
        }
        #region
        private async Task<List<Invoice>> GetInvoice(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.Invoice.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<Invoice>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(Invoice), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<Invoice, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateInvoice(Invoice model)
        {
            _dbContext.Invoice.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateInvoice(Guid id, Invoice updatedEntity)
        {
            _dbContext.Invoice.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteInvoice(Guid id)
        {
            var entityData = _dbContext.Invoice.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.Invoice.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchInvoice(Guid id, JsonPatchDocument<Invoice> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.Invoice.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.Invoice.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}