
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnlineBookstore.Service;    
using OnlineBookstore.Services;
using OnlineBookstore.Utilities;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Data.Repository;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<BookstoreContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("OnlineBookStore_Data")
    )
);

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<AuthorRepository>();
builder.Services.AddScoped<GenreRepository>();
builder.Services.AddScoped<PublisherRepository>();
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<UserRepository>();

builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<AuthorService>();
builder.Services.AddScoped<GenreService>();
builder.Services.AddScoped<PublisherService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.Cookie.Name = ".OnlineBookstore.Session";
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.IdleTimeout = TimeSpan.FromHours(2);
});


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts =>
    {
     
        opts.LoginPath = "/Account/Login";
        opts.LogoutPath = "/Account/Logout";
        opts.AccessDeniedPath = "/Account/AccessDenied";
        opts.SlidingExpiration = true;
        opts.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("IsAdmin", "true"));
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

app.Run();
