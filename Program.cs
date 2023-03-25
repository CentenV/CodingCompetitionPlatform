using CodingCompetitionPlatform.Services;

var builder = WebApplication.CreateBuilder(args);

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

//builder.Services.AddSession();      // Added for user session!!!!!!!!!!!!!!!!

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

//app.UseSession();       // Added for user session

app.MapRazorPages();

app.Run();
