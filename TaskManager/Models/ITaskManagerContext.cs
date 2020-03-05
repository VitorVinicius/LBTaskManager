using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TaskManager.Models
{
    public interface ITaskManagerContext
    {
       // DbSet<Task> Task { get; set; }
       // DbSet<User> User { get; set; }

       // EntityEntry Add([NotNullAttribute] object entity);
       // EntityEntry<TEntity> Add<TEntity>([NotNullAttribute] TEntity entity) where TEntity : class;

       
       //  EntityEntry Update([NotNullAttribute] object entity);
        
       //  EntityEntry<TEntity> Update<TEntity>([NotNullAttribute] TEntity entity) where TEntity : class;


       //int SaveChanges(bool acceptAllChangesOnSuccess);
     
       // int SaveChanges();
       
       //Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
       
       //  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}