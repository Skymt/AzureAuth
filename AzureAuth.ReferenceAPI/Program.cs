using AzureAuth.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Enable JWT Authentication
builder.Services.AddJWTAuthentication(builder.Configuration);

// Enable JWT generation
builder.Services.AddJWTManager();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

// Allow index.html to be served
app.UseStaticFiles();

// Order is important here:
// First, authenticate the JWT
app.UseAuthentication();
// Then, set the user rights in the HttpContext
app.UseAuthorization();

app.MapControllers();

app.Run();

