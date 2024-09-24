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
    /// The dayvisitService responsible for managing dayvisit related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting dayvisit information.
    /// </remarks>
    public interface IDayVisitService
    {
        /// <summary>Retrieves a specific dayvisit by its primary key</summary>
        /// <param name="id">The primary key of the dayvisit</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The dayvisit data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of dayvisits based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of dayvisits</returns>
        Task<List<DayVisit>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new dayvisit</summary>
        /// <param name="model">The dayvisit data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(DayVisit model);

        /// <summary>Updates a specific dayvisit by its primary key</summary>
        /// <param name="id">The primary key of the dayvisit</param>
        /// <param name="updatedEntity">The dayvisit data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, DayVisit updatedEntity);

        /// <summary>Updates a specific dayvisit by its primary key</summary>
        /// <param name="id">The primary key of the dayvisit</param>
        /// <param name="updatedEntity">The dayvisit data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<DayVisit> updatedEntity);

        /// <summary>Deletes a specific dayvisit by its primary key</summary>
        /// <param name="id">The primary key of the dayvisit</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The dayvisitService responsible for managing dayvisit related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting dayvisit information.
    /// </remarks>
    public class DayVisitService : IDayVisitService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the DayVisit class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public DayVisitService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific dayvisit by its primary key</summary>
        /// <param name="id">The primary key of the dayvisit</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The dayvisit data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.DayVisit.AsQueryable();
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

            string[] navigationProperties = ["PatientId_Patient","DoctorId_Doctor","VisitId_Visit","AppointmentId_Appointment","InvoiceId_Invoice","LocationId_Location"];
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

        /// <summary>Retrieves a list of dayvisits based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of dayvisits</returns>/// <exception cref="Exception"></exception>
        public async Task<List<DayVisit>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetDayVisit(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new dayvisit</summary>
        /// <param name="model">The dayvisit data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(DayVisit model)
        {
            model.Id = await CreateDayVisit(model);
            return model.Id;
        }

        /// <summary>Updates a specific dayvisit by its primary key</summary>
        /// <param name="id">The primary key of the dayvisit</param>
        /// <param name="updatedEntity">The dayvisit data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, DayVisit updatedEntity)
        {
            await UpdateDayVisit(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific dayvisit by its primary key</summary>
        /// <param name="id">The primary key of the dayvisit</param>
        /// <param name="updatedEntity">The dayvisit data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<DayVisit> updatedEntity)
        {
            await PatchDayVisit(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific dayvisit by its primary key</summary>
        /// <param name="id">The primary key of the dayvisit</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteDayVisit(id);
            return true;
        }
        #region
        private async Task<List<DayVisit>> GetDayVisit(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.DayVisit.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<DayVisit>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(DayVisit), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<DayVisit, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateDayVisit(DayVisit model)
        {
            _dbContext.DayVisit.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateDayVisit(Guid id, DayVisit updatedEntity)
        {
            _dbContext.DayVisit.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteDayVisit(Guid id)
        {
            var entityData = _dbContext.DayVisit.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.DayVisit.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchDayVisit(Guid id, JsonPatchDocument<DayVisit> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.DayVisit.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.DayVisit.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}