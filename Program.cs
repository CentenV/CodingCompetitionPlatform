using CodingCompetitionPlatform.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DATABASE CONNECTION
builder.Services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("myConnString")));

// AUTHENTICATION / AUTHORIZATION
builder.Services.AddAuthentication(Credential.COOKIE_NAME).AddCookie(Credential.COOKIE_NAME, options =>
{
    options.Cookie.Name = Credential.COOKIE_NAME;
    options.LoginPath = "/login";
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("competitor_policy", policy => policy.RequireClaim("competitor", "true"));
});

// Add services to the container.
builder.Services.AddRazorPages();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
