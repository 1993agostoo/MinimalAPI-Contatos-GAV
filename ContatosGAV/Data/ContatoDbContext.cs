using ContatosGAV.Models;
using Microsoft.EntityFrameworkCore;

namespace ContatosGAV.Repository
{
    public class ContatoDbContext : DbContext
    {
        public ContatoDbContext(DbContextOptions<ContatoDbContext> options) : base(options) { }

        public DbSet<Contato> Contatos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("DataSource=gav.db;Cache=Shared");
    }
}