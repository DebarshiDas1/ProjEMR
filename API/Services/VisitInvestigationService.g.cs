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
    /// The visitinvestigationService responsible for managing visitinvestigation related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting visitinvestigation information.
    /// </remarks>
    public interface IVisitInvestigationService
    {
        /// <summary>Retrieves a specific visitinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the visitinvestigation</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The visitinvestigation data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of visitinvestigations based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of visitinvestigations</returns>
        Task<List<VisitInvestigation>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new visitinvestigation</summary>
        /// <param name="model">The visitinvestigation data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(VisitInvestigation model);

        /// <summary>Updates a specific visitinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the visitinvestigation</param>
        /// <param name="updatedEntity">The visitinvestigation data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, VisitInvestigation updatedEntity);

        /// <summary>Updates a specific visitinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the visitinvestigation</param>
        /// <param name="updatedEntity">The visitinvestigation data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<VisitInvestigation> updatedEntity);

        /// <summary>Deletes a specific visitinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the visitinvestigation</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The visitinvestigationService responsible for managing visitinvestigation related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting visitinvestigation information.
    /// </remarks>
    public class VisitInvestigationService : IVisitInvestigationService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the VisitInvestigation class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public VisitInvestigationService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific visitinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the visitinvestigation</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The visitinvestigation data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.VisitInvestigation.AsQueryable();
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

            string[] navigationProperties = ["DoctorInvestigationId_DoctorInvestigation","PatientId_Patient","InvoiceLineId_InvoiceLine"];
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

        /// <summary>Retrieves a list of visitinvestigations based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of visitinvestigations</returns>/// <exception cref="Exception"></exception>
        public async Task<List<VisitInvestigation>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetVisitInvestigation(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new visitinvestigation</summary>
        /// <param name="model">The visitinvestigation data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(VisitInvestigation model)
        {
            model.Id = await CreateVisitInvestigation(model);
            return model.Id;
        }

        /// <summary>Updates a specific visitinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the visitinvestigation</param>
        /// <param name="updatedEntity">The visitinvestigation data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, VisitInvestigation updatedEntity)
        {
            await UpdateVisitInvestigation(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific visitinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the visitinvestigation</param>
        /// <param name="updatedEntity">The visitinvestigation data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<VisitInvestigation> updatedEntity)
        {
            await PatchVisitInvestigation(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific visitinvestigation by its primary key</summary>
        /// <param name="id">The primary key of the visitinvestigation</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteVisitInvestigation(id);
            return true;
        }
        #region
        private async Task<List<VisitInvestigation>> GetVisitInvestigation(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.VisitInvestigation.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<VisitInvestigation>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(VisitInvestigation), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<VisitInvestigation, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateVisitInvestigation(VisitInvestigation model)
        {
            _dbContext.VisitInvestigation.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateVisitInvestigation(Guid id, VisitInvestigation updatedEntity)
        {
            _dbContext.VisitInvestigation.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteVisitInvestigation(Guid id)
        {
            var entityData = _dbContext.VisitInvestigation.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.VisitInvestigation.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchVisitInvestigation(Guid id, JsonPatchDocument<VisitInvestigation> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.VisitInvestigation.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.VisitInvestigation.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}