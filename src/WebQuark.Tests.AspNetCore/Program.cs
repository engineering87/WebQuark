using WebQuark.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add required services
builder.Services.AddRazorPages();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor(); // Ensure IHttpContextAccessor is registered
builder.Services.AddWebQuark(); // Registers WebQuark services

var app = builder.Build();

// Configure WebQuark after DI is fully built
var accessor = app.Services.GetRequiredService<IHttpContextAccessor>();
WebQuark.QueryString.QueryStringHandler.ConfigureHttpContextAccessor(accessor);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();