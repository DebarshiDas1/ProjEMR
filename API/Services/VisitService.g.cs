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
    /// The visitService responsible for managing visit related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting visit information.
    /// </remarks>
    public interface IVisitService
    {
        /// <summary>Retrieves a specific visit by its primary key</summary>
        /// <param name="id">The primary key of the visit</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The visit data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of visits based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of visits</returns>
        Task<List<Visit>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new visit</summary>
        /// <param name="model">The visit data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(Visit model);

        /// <summary>Updates a specific visit by its primary key</summary>
        /// <param name="id">The primary key of the visit</param>
        /// <param name="updatedEntity">The visit data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, Visit updatedEntity);

        /// <summary>Updates a specific visit by its primary key</summary>
        /// <param name="id">The primary key of the visit</param>
        /// <param name="updatedEntity">The visit data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<Visit> updatedEntity);

        /// <summary>Deletes a specific visit by its primary key</summary>
        /// <param name="id">The primary key of the visit</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The visitService responsible for managing visit related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting visit information.
    /// </remarks>
    public class VisitService : IVisitService
    {
        private readonly ProjEMRContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the Visit class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public VisitService(ProjEMRContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific visit by its primary key</summary>
        /// <param name="id">The primary key of the visit</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The visit data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.Visit.AsQueryable();
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

            string[] navigationProperties = ["PatientId_Patient","LocationId_Location","VisitType_VisitType","VisitMode_VisitMode","Doctor_Doctor","Contact_Address","Appointment_Appointment","DayVisit_DayVisit","VisitChiefComplaint_VisitChiefComplaint","VisitDiagnosis_VisitDiagnosis","VisitGuideline_VisitGuideline","VisitVitalTemplateParameter_VisitVitalTemplateParameter","VisitInvestigation_VisitInvestigation","Invoice_Invoice","Dispenses_Dispense","VisitMedicalCertificates_VisitMedicalCertificate"];
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

        /// <summary>Retrieves a list of visits based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of visits</returns>/// <exception cref="Exception"></exception>
        public async Task<List<Visit>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetVisit(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new visit</summary>
        /// <param name="model">The visit data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(Visit model)
        {
            model.Id = await CreateVisit(model);
            return model.Id;
        }

        /// <summary>Updates a specific visit by its primary key</summary>
        /// <param name="id">The primary key of the visit</param>
        /// <param name="updatedEntity">The visit data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, Visit updatedEntity)
        {
            await UpdateVisit(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific visit by its primary key</summary>
        /// <param name="id">The primary key of the visit</param>
        /// <param name="updatedEntity">The visit data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<Visit> updatedEntity)
        {
            await PatchVisit(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific visit by its primary key</summary>
        /// <param name="id">The primary key of the visit</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteVisit(id);
            return true;
        }
        #region
        private async Task<List<Visit>> GetVisit(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.Visit.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<Visit>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(Visit), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<Visit, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateVisit(Visit model)
        {
            _dbContext.Visit.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateVisit(Guid id, Visit updatedEntity)
        {
            _dbContext.Visit.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteVisit(Guid id)
        {
            var entityData = _dbContext.Visit.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.Visit.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchVisit(Guid id, JsonPatchDocument<Visit> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.Visit.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.Visit.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}