using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models; // Employee modelinin namespace'i

namespace WebApplication4.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Assignment> Assignments { get; set; }

    }
}
