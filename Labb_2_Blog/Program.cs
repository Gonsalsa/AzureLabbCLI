using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Labb_2_Blog.Context;
using Labb_2_Blog.Core.Interface;
using Labb_2_Blog.Core.Services;
using Labb_2_Blog.Data.Interfaces;
using Labb_2_Blog.Data.Repos;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace Labb_2_Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            var keyVaultURL = builder.Configuration.GetSection("KeyVault:KeyVaultURL");

            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                    {
                        Delay= TimeSpan.FromSeconds(2),
                        MaxDelay = TimeSpan.FromSeconds(16),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential
                    }
            };

            var client = new SecretClient(new Uri(keyVaultURL.Value!.ToString()), new DefaultAzureCredential(), options);

            KeyVaultSecret connectionString = client.GetSecret("connString").Value;
            string connString = connectionString.Value;

            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<AppDbContext>(options => 
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            //Dependancy Injections Repos
            builder.Services.AddScoped<IUserRepo, UserRepo>();
            builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
            builder.Services.AddScoped<IPostRepo, PostRepo>();
            builder.Services.AddScoped<ICommentRepo, CommentRepo>();


            //Dependancy Injections Services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IBlogPostService, BlogpostService>();
            builder.Services.AddScoped<ICommentService, CommentService>();


            var app = builder.Build();

            //Konfigurerar HTTP pipelinen
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();


        }
    }
}
