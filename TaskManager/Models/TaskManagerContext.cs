using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace TaskManager.Models
{
    public partial class TaskManagerContext : DbContext, ITaskManagerContext
    {
        public TaskManagerContext()
        {
        }

        public TaskManagerContext(DbContextOptions<TaskManagerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Task> Task { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //Credits for: https://stackoverflow.com/questions/45796776/get-connectionstring-from-appsettings-json-instead-of-being-hardcoded-in-net-co

                //Get settings from App Settings File
                IConfigurationRoot configuration = new ConfigurationBuilder()
                                                   .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                                   .AddJsonFile("appsettings.json")
                                                   .Build();
                //Get Default connection string 'TaskManagerDatabase' from App Settings File and use it
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("TaskManagerDatabase"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Task>(entity =>
            {
                entity.HasComment("Tabela que armazena as Tasks dos Usuários");

                entity.Property(e => e.Id).HasComment("Identificação da Task");

                entity.Property(e => e.Concluded).HasComment("Flag de Estado da Task");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(900)
                    .HasComment("Descrição da Task");

                entity.Property(e => e.UserId)
                    .HasColumnName("User_Id")
                    .HasComment("Id do Usuário");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Task)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_User_Task");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasComment("Tabela que armazena dados dos usuários");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasComment("Email");

                entity.Property(e => e.Firstname)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasComment("Nome do usuário");

                entity.Property(e => e.LastUpdateDate)
                    .HasColumnName("Last_Update_Date")
                    .HasComment("Data da Última atualização");

                entity.Property(e => e.Lastname)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasComment("Sobrenome do usuário");

                

                entity.Property(e => e.PasswordSalt)
                    .IsRequired()
                    .HasColumnName("Password_Salt")
                    .HasMaxLength(512)
                    .HasComment("Entropia para calcular Hash");

                entity.Property(e => e.PassworhHash)
                    .IsRequired()
                    .HasColumnName("Passworh_Hash")
                    .HasMaxLength(512)
                    .HasComment("Hash de senha");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnName("Registration_Date")
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Data do Registro do usuário");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
