using ExamSys.Application.BackgroundServices;
using ExamSys.Application.Interfaces;
using ExamSys.Application.Services;
using ExamSys.Core.Interfaces;
using ExamSys.Core.Interfaces.Repositories;
using ExamSys.Infrastructure.Data;
using ExamSys.Infrastructure.Data.Repositories;
using ExamSys.Infrastructure.Identity;
using ExamSys.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add INFRASTRUCTURE services
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// In-memory Caching
builder.Services.AddMemoryCache();

// Background Services
builder.Services.Configure<HostOptions>(x =>
{
    x.ServicesStartConcurrently = true;
    x.ServicesStopConcurrently = true;
});

builder.Services.AddHostedService<AutoSyncExamsStateService>();

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IParticipantExamRepository, ParticipantExamRepository>();
builder.Services.AddScoped<IParticipantAnswerRepository, ParticipantAnswerRepository>();


// Application Services
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IExamStateManager, ExamStateManager>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IExamTakingService, ExamTakingService>();
builder.Services.AddScoped<IExamCacheService, ExamCacheService>();


builder.Services.ConfigureApplicationCookie(configure =>
{
    configure.AccessDeniedPath = "/Authentication/AccessDenied";
    configure.LoginPath = "/Authentication/Login";
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
       .AddCookie();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DbInitializer.SeedRolesAsync(roleManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
