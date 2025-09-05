using CarInsurance.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Tests
{
    public static class TestContext
    {

        private static readonly string DbFile = "TestCarInsurance.db";

        public static AppDbContext Create()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={DbFile}")
            .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            SeedData.EnsureSeeded(context);
            return context;
        }
    }
}