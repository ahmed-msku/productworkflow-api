using ProductWorkflow.Data;
using Microsoft.EntityFrameworkCore;
using ProductWorkflow.API.Services;
using ProductWorkflow.API.Middleware;
using ProductWorkflow.API.Repositories;
using ProductWorkflow.API.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseInMemoryDatabase("ProductDb"));
builder.Services.AddDbContextFactory<AppDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IJobRepository, JobRepository>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddHostedService<FileProcessingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(options =>
    options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
);

app.UseMiddleware<GlocalErrorHandler>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

