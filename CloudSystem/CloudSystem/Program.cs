using CloudSystem.Model;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue; // Set maximum request size limit
});

builder.WebHost.UseKestrel((context, options) =>
{
    if (context.HostingEnvironment.IsDevelopment())
    {
        // For development, use a self-signed certificate
        options.ListenAnyIP(5000); // HTTP
        options.ListenAnyIP(5001, listenOptions =>
        {
            listenOptions.UseHttps(AppDomain.CurrentDomain.BaseDirectory
                + "..\\..\\..\\Certificates\\mycert.pfx", "");
        }); // HTTPS
    }
    else
    {
        // For production, use a CA-issued certificate
        options.ListenAnyIP(443, listenOptions =>
        {
            listenOptions.UseHttps("/certificates/mycert.pfx", "");
        }); // HTTPS
    }
});


builder.Services.AddRazorPages(options =>
{
    options.Conventions
        .AddPageApplicationModelConvention("/StreamedSingleFileUploadDb",
            model =>
            {
                model.Filters.Add(
                    new DisableFormValueModelBindingAttribute());
            });
    options.Conventions
        .AddPageApplicationModelConvention("/StreamedSingleFileUploadPhysical",
            model =>
            {
                model.Filters.Add(
                    new DisableFormValueModelBindingAttribute());
            });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseAuthentication();

app.MapControllers();

app.Run();