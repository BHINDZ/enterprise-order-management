using Microsoft.EntityFrameworkCore;
using OrderPortal.Api.Models;

namespace OrderPortal.Api.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}
