using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SmartCampus.Core.Mappings;
using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Data.Seeders;
using SmartCampus.Services.Implementations;
using SmartCampus.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SmartCampusDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<SmartCampusDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(SmartCampus.Web.Authorization.AuthorizationPolicies.AdminOnly,
        policy => policy.RequireRole(SmartCampus.Web.Authorization.AppRoles.Admin));

    options.AddPolicy(SmartCampus.Web.Authorization.AuthorizationPolicies.TeacherOnly,
        policy => policy.RequireRole(SmartCampus.Web.Authorization.AppRoles.Teacher));

    options.AddPolicy(SmartCampus.Web.Authorization.AuthorizationPolicies.StudentOnly,
        policy => policy.RequireRole(SmartCampus.Web.Authorization.AppRoles.Student));

    options.AddPolicy(SmartCampus.Web.Authorization.AuthorizationPolicies.TeacherOrAdmin,
        policy => policy.RequireRole(SmartCampus.Web.Authorization.AppRoles.Teacher,
                                     SmartCampus.Web.Authorization.AppRoles.Admin));

    options.AddPolicy(SmartCampus.Web.Authorization.AuthorizationPolicies.NotStudent,
        policy => policy.RequireRole(SmartCampus.Web.Authorization.AppRoles.Teacher,
                                     SmartCampus.Web.Authorization.AppRoles.Admin));
});

builder.Services.AddRazorPages();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IHomeworkService, HomeworkService>();
builder.Services.AddScoped<IHomeworkSubmissionService, HomeworkSubmissionService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IGroupService, GroupService>();

builder.Services.AddScoped<SmartCampus.Web.Utilities.UserContextHelper>();

builder.Services.AddScoped<DataSeeder>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

_ = app.Services.CreateScope().ServiceProvider.GetRequiredService<SmartCampusDbContext>()
    .Database.MigrateAsync()
    .ContinueWith(async _ =>
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            await dataSeeder.SeedAllDataAsync();
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error during database initialization");
        }
    });

app.Run();