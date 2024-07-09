using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ers_server.Models;

namespace ers_server.Data
{
    public class ErsDbContext : DbContext
    {
        public ErsDbContext (DbContextOptions<ErsDbContext> options)
            : base(options)
        {
        }

        public DbSet<ers_server.Models.Employee> Employees { get; set; } = default!;
        public DbSet<ers_server.Models.Expense> Expenses { get; set; } = default!;
        public DbSet<ers_server.Models.Item> Items { get; set; } = default!;
        public DbSet<ers_server.Models.Expenseline> Expenselines { get; set; } = default!;



    }
}
