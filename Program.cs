using Bpi.Services.Whatsapp;
using chatBotTwilio.Hub;
using chatBotTwilio.Models;
using chatBotTwilio.Services.BPI;
using chatBotTwilio.Services.WhatsApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBiWhatsOrigin", builder =>
    {
        builder.WithOrigins("https://localhost",
	"http://localhost:4200",
	"http://82.180.132.86:5008",
	"https://smp25-beta.netlify.app",
	"https://beap-prueba.netlify.app",
	"https://www.hco-bpi.com")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Microservicio TWilio", Version = "v1 Mod. 2026-05-16 11:13 Server 82.180.132.86" });
});

// Agregar HttpClient
builder.Services.AddHttpClient();
builder.Services.AddDbContext<DbSmpContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbConnSmp")));

//WhatsApp Services        
builder.Services.AddScoped<IReceiveWhatsAppService, ReceiveWhatsAppService>();
builder.Services.AddScoped<IConversationManagerService, ConversationManagerService>();
builder.Services.AddScoped<ISendWhatsappService, SendWhatsappService>();
builder.Services.AddScoped<IConversationStateService, ConversationStateService>();
//builder.Services.AddScoped<WhatsAppImageServiceFire, WhatsAppImageServiceFire>();
builder.Services.AddScoped<WhatsAppImageServiceAzure, WhatsAppImageServiceAzure>();

//BPI Services
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ILogbookService, LogbookService>();

// Configure Azure Storage options with reload
builder.Services.Configure<AzureStorageConfig>(builder.Configuration.GetSection("AzureStorageConfig"));
builder.Services.Configure<AzureStorageConfig>("Primary", builder.Configuration.GetSection("AzureStorageConfig"));
builder.Services.Configure<AzureStorageConfig>("Legacy", builder.Configuration.GetSection("AzureStorageConfig2"));

var app = builder.Build();
// Configurar Swagger en desarrollo o producci�n
//if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
//{
  //  app.UseSwagger();
  //  app.UseSwaggerUI(c =>
  //  {
  //      c.SwaggerEndpoint("/swagger/v1/swagger.json", "Microservicio SMP v1");
  //      c.RoutePrefix = string.Empty; // Para acceder directamente desde la ra�z
  //  });
//}
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
//
//
//

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Microservicio SMP v1");
    c.RoutePrefix = "swagger"; // O usa string.Empty si quieres que cargue en 
});

app.UseHttpsRedirection();
app.UseCors("AllowBiWhatsOrigin");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapHub<StorageHub>("storageHub");
app.Run();
