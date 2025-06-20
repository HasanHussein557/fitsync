using ApiWrapper.Services;
using System.Net.Http.Headers;
using Core.Interfaces;
using Core.Services;
using DAL;
using DAL.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register HttpClient
builder.Services.AddHttpClient();

// Register API wrapper services
builder.Services.AddScoped<IWorkoutSchemaGenerator, GenerateWorkoutSchema>(sp => {
    var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = clientFactory.CreateClient();
    var apiUrl = builder.Configuration.GetValue<string>("ApiUrl");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    return new GenerateWorkoutSchema(client, apiUrl);
});

// Register the NutritionSchemaGenerator
builder.Services.AddScoped<INutritionSchemaGenerator, GenerateNutritionSchema>(sp => {
    var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = clientFactory.CreateClient();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    var apiUrl = builder.Configuration.GetValue<string>("ApiUrl");
    return new GenerateNutritionSchema(client, apiUrl);
});

// Register the repositories
builder.Services.AddScoped<IAthleteRepository, AthleteRepository>(sp => {
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new AthleteRepository(connectionString);
});

// Register the WorkoutSchemaRepository
builder.Services.AddScoped<IWorkoutSchemaRepository, WorkoutSchemaRepository>(sp => {
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var logger = sp.GetRequiredService<ILogger<WorkoutSchemaRepository>>();
    return new WorkoutSchemaRepository(connectionString, logger);
});

// Register the NutritionSchemaRepository
builder.Services.AddScoped<INutritionSchemaRepository, NutritionSchemaRepository>(sp => {
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var logger = sp.GetRequiredService<ILogger<NutritionSchemaRepository>>();
    return new NutritionSchemaRepository(connectionString, logger);
});

// Register the UserRepository
builder.Services.AddScoped<IUserRepository, UserRepository>(sp => {
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new UserRepository(connectionString);
});

// Register database utility
builder.Services.AddScoped<DatabaseUtility>(sp => {
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var logger = sp.GetRequiredService<ILogger<DatabaseUtility>>();
    return new DatabaseUtility(connectionString, logger);
});

// Register the AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// Register the AthleteService
builder.Services.AddScoped<IAthleteService, AthleteService>();

// Register the WorkoutSchemaService
builder.Services.AddScoped<WorkoutSchemaService>(sp => {
    var workoutSchemaRepo = sp.GetRequiredService<IWorkoutSchemaRepository>();
    var athleteRepo = sp.GetRequiredService<IAthleteRepository>();
    var generateWorkoutSchema = sp.GetRequiredService<Core.Interfaces.IWorkoutSchemaGenerator>();
    var logger = sp.GetRequiredService<ILogger<WorkoutSchemaService>>();
    return new WorkoutSchemaService(workoutSchemaRepo, athleteRepo, generateWorkoutSchema, logger);
});

// Add authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})

// Add cookie authentication
.AddCookie(options => {
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/AccessDenied";
    options.Cookie.Name = "FitSync.Auth";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// Add basic authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Order is important - Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();