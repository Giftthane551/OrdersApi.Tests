using Microsoft.EntityFrameworkCore;
using OrdersApi.Data;

namespace OrdersApi.Tests
{
    public static class TestHelpers
    {
        public static OrdersDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new OrdersDbContext(options);
            db.Database.EnsureCreated();
            return db;
        }
    }
}
