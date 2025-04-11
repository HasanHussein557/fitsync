using Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register HttpClient
builder.Services.AddHttpClient<WorkoutSchema>(client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:8000/");
});

// Register WorkoutSchema as a scoped service
builder.Services.AddScoped<WorkoutSchema>();

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();