using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
namespace controller.Extensions
{
    public static class MigrationHelper
    {
        public static IApplicationBuilder MigrationDB(this IApplicationBuilder app)
        {
            try
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ConvosDbContext>();
                    context.Database.Migrate();
                }
                return app;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MigrationDB failed: {ex.Message}");
                throw;
            }
        }
    }

}