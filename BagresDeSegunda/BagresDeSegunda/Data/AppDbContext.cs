using Microsoft.EntityFrameworkCore;
using BagresDeSegunda.Models;

namespace BagresDeSegunda.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Jogador> Jogadores { get; set; }
        public DbSet<Partida> Partidas { get; set; }
        public DbSet<Atuacao> Atuacoes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Substitua pelos seus dados de acesso ao MySQL
            string connectionString = "Server=localhost;Database=bagres_db;User=root;Password=tunasadmin;";

            // O Pomelo precisa saber a versão do seu MySQL (ex: 8.0.31 ou 5.7)
            var serverVersion = ServerVersion.AutoDetect(connectionString);

            optionsBuilder
                .UseMySql(connectionString, serverVersion)
                .UseLazyLoadingProxies();
        }
    }
}