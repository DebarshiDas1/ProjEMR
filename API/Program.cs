using ProjEMR.Data;
using Microsoft.OpenApi.Models;
using System.Reflection;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProjEMR.Models;
using ProjEMR.Services;
using ProjEMR.Logger;
using Newtonsoft.Json;
var builder = WebApplication.CreateBuilder(args);

// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Host.UseNLog();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProjEMR", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();
    c.CustomSchemaIds(type => type.ToString());
    c.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter into field the word 'Bearer' following by space and JWT",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                    Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string> ()
                    }
                });
});
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
// Build the configuration object from appsettings.json
var config = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json", optional: false)
  .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
  .Build();
//Set value to appsetting
AppSetting.JwtIssuer = config.GetValue<string>("Jwt:Issuer");
AppSetting.JwtKey = config.GetValue<string>("Jwt:Key");
AppSetting.TokenExpirationtime = config.GetValue<int>("TokenExpirationtime");
// Add NLog as the logging service
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders(); // Remove other logging providers
    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
});
// Add JWT authentication services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = AppSetting.JwtIssuer,
        ValidAudience = AppSetting.JwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSetting.JwtKey ?? ""))
    };
});
//Service inject
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ITitleService, TitleService>();
builder.Services.AddScoped<IGenderService, GenderService>();
builder.Services.AddScoped<IContactMemberService, ContactMemberService>();
builder.Services.AddScoped<IComorbidityService, ComorbidityService>();
builder.Services.AddScoped<IQualificationService, QualificationService>();
builder.Services.AddScoped<ISpecialisationService, SpecialisationService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IStateService, StateService>();
builder.Services.AddScoped<IStockAdjustmentFileService, StockAdjustmentFileService>();
builder.Services.AddScoped<IStockAdjustmentItemService, StockAdjustmentItemService>();
builder.Services.AddScoped<IStockAdjustmentService, StockAdjustmentService>();
builder.Services.AddScoped<IRequisitionFileService, RequisitionFileService>();
builder.Services.AddScoped<IRequisitionLineService, RequisitionLineService>();
builder.Services.AddScoped<IRequisitionService, RequisitionService>();
builder.Services.AddScoped<IPurchaseOrderFileService, PurchaseOrderFileService>();
builder.Services.AddScoped<IPurchaseOrderLineService, PurchaseOrderLineService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IPriceListComponentService, PriceListComponentService>();
builder.Services.AddScoped<IPriceListItemService, PriceListItemService>();
builder.Services.AddScoped<IPriceListVersionService, PriceListVersionService>();
builder.Services.AddScoped<IPriceListService, PriceListService>();
builder.Services.AddScoped<IGoodsReturnFileService, GoodsReturnFileService>();
builder.Services.AddScoped<IGoodsReturnItemService, GoodsReturnItemService>();
builder.Services.AddScoped<ISubReasonService, SubReasonService>();
builder.Services.AddScoped<IGoodsReturnService, GoodsReturnService>();
builder.Services.AddScoped<IGoodsReceiptItemService, GoodsReceiptItemService>();
builder.Services.AddScoped<IGoodsReceiptActivityHistoryService, GoodsReceiptActivityHistoryService>();
builder.Services.AddScoped<IGoodsReceiptFileService, GoodsReceiptFileService>();
builder.Services.AddScoped<IGoodsReceiptPurchaseOrderRelationService, GoodsReceiptPurchaseOrderRelationService>();
builder.Services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
builder.Services.AddScoped<ITokenManagementService, TokenManagementService>();
builder.Services.AddScoped<IUomService, UomService>();
builder.Services.AddScoped<IClinicalParameterValueService, ClinicalParameterValueService>();
builder.Services.AddScoped<IClinicalParameterService, ClinicalParameterService>();
builder.Services.AddScoped<IVisitChiefComplaintParameterService, VisitChiefComplaintParameterService>();
builder.Services.AddScoped<IChiefComplaintService, ChiefComplaintService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IProcedureService, ProcedureService>();
builder.Services.AddScoped<IRouteInfoService, RouteInfoService>();
builder.Services.AddScoped<IGenericService, GenericService>();
builder.Services.AddScoped<IFormulationService, FormulationService>();
builder.Services.AddScoped<IProductManufactureService, ProductManufactureService>();
builder.Services.AddScoped<IProductClassificationService, ProductClassificationService>();
builder.Services.AddScoped<IInvoiceFileService, InvoiceFileService>();
builder.Services.AddScoped<IInvoiceLineService, InvoiceLineService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IGstSettingsService, GstSettingsService>();
builder.Services.AddScoped<IProductUomService, ProductUomService>();
builder.Services.AddScoped<IFinanceSettingService, FinanceSettingService>();
builder.Services.AddScoped<IProductBatchService, ProductBatchService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IVisitInvestigationService, VisitInvestigationService>();
builder.Services.AddScoped<IPaymentModeService, PaymentModeService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAppointmentServiceService, AppointmentServiceService>();
builder.Services.AddScoped<IAccountSettlementService, AccountSettlementService>();
builder.Services.AddScoped<IAppointmentReminderLogService, AppointmentReminderLogService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
builder.Services.AddScoped<IVisitChiefComplaintService, VisitChiefComplaintService>();
builder.Services.AddScoped<IVisitModeService, VisitModeService>();
builder.Services.AddScoped<IVisitTypeService, VisitTypeService>();
builder.Services.AddScoped<IVisitDiagnosisParameterService, VisitDiagnosisParameterService>();
builder.Services.AddScoped<IVisitDiagnosisService, VisitDiagnosisService>();
builder.Services.AddScoped<IVisitGuidelineService, VisitGuidelineService>();
builder.Services.AddScoped<IVisitVitalTemplateParameterService, VisitVitalTemplateParameterService>();
builder.Services.AddScoped<IInvestigationService, InvestigationService>();
builder.Services.AddScoped<IDoctorInvestigationService, DoctorInvestigationService>();
builder.Services.AddScoped<IMedicationDosageService, MedicationDosageService>();
builder.Services.AddScoped<IDispenseItemDosageService, DispenseItemDosageService>();
builder.Services.AddScoped<IMedicationCompositionService, MedicationCompositionService>();
builder.Services.AddScoped<IDoctorFavouriteMedicationService, DoctorFavouriteMedicationService>();
builder.Services.AddScoped<IDrugListItemsService, DrugListItemsService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IPatientPharmacyQueueService, PatientPharmacyQueueService>();
builder.Services.AddScoped<IDispenseItemService, DispenseItemService>();
builder.Services.AddScoped<IDispenseActivityHistoryService, DispenseActivityHistoryService>();
builder.Services.AddScoped<IVisitMedicalCertificateService, VisitMedicalCertificateService>();
builder.Services.AddScoped<IPatientPayorService, PatientPayorService>();
builder.Services.AddScoped<IPatientComorbidityService, PatientComorbidityService>();
builder.Services.AddScoped<IPatientCategoryService, PatientCategoryService>();
builder.Services.AddScoped<IPatientStatisticsService, PatientStatisticsService>();
builder.Services.AddScoped<IPatientPregnancyService, PatientPregnancyService>();
builder.Services.AddScoped<IPatientMedicalHistoryNoteService, PatientMedicalHistoryNoteService>();
builder.Services.AddScoped<IPatientHospitalisationHistoryService, PatientHospitalisationHistoryService>();
builder.Services.AddScoped<IPatientEnrollmentLinkService, PatientEnrollmentLinkService>();
builder.Services.AddScoped<IPatientLifeStyleService, PatientLifeStyleService>();
builder.Services.AddScoped<IPatientAllergyService, PatientAllergyService>();
builder.Services.AddScoped<IPatientNotesService, PatientNotesService>();
builder.Services.AddScoped<IPregnancyHistoryService, PregnancyHistoryService>();
builder.Services.AddScoped<IBloodGroupService, BloodGroupService>();
builder.Services.AddScoped<IDispenseService, DispenseService>();
builder.Services.AddScoped<IDiagnosisService, DiagnosisService>();
builder.Services.AddScoped<IDayVisitService, DayVisitService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IEntityService, EntityService>();
builder.Services.AddScoped<IRoleEntitlementService, RoleEntitlementService>();
builder.Services.AddScoped<IUserInRoleService, UserInRoleService>();
builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
builder.Services.AddScoped<IFieldMapperService, FieldMapperService>();
builder.Services.AddScoped<IJsonMessageService, JsonMessageService>();
builder.Services.AddTransient<ILoggerService, LoggerService>();
//Json handler
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    // Configure Newtonsoft.Json settings here
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
});
//Inject context
builder.Services.AddTransient<ProjEMRContext>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.SetIsOriginAllowed(_ => true)
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjEMR API v1");
        c.RoutePrefix = string.Empty;
    });
    app.MapSwagger().RequireAuthorization();
}
app.UseAuthorization();
app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();