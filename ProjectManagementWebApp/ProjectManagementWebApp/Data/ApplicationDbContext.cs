using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagementWebApp.Models;

namespace ProjectManagementWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public virtual DbSet<Student> Students { get; set; }

        public virtual DbSet<Lecturer> Lecturers { get; set; }

        public virtual DbSet<Audit> Audits { get; set; }

        public virtual DbSet<ProjectType> ProjectTypes { get; set; }

        public virtual DbSet<Project> Projects { get; set; }

        public virtual DbSet<ProjectMember> ProjectMembers { get; set; }

        public virtual DbSet<ProjectLecturer> ProjectLecturers { get; set; }

        public virtual DbSet<ProjectSchedule> ProjectSchedules { get; set; }

        public virtual DbSet<ProjectScheduleReport> ProjectScheduleReports { get; set; }

        public virtual DbSet<ProjectScheduleReportFile> ProjectScheduleReportFiles { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ProjectMember>()
                .HasKey(pm => new { pm.ProjectId, pm.StudentId });
            builder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.ProjectMembers)
                .HasForeignKey(pm => pm.ProjectId);
            builder.Entity<ProjectMember>()
             .HasOne(pm => pm.Student)
             .WithMany(s => s.ProjectMembers)
             .HasForeignKey(pm => pm.StudentId);

            builder.Entity<ProjectLecturer>()
                .HasKey(pl => new { pl.ProjectId, pl.LecturerId });
            builder.Entity<ProjectLecturer>()
                .HasOne(pl => pl.Project)
                .WithMany(p => p.ProjectLecturers)
                .HasForeignKey(pl => pl.ProjectId);
            builder.Entity<ProjectLecturer>()
             .HasOne(pl => pl.Lecturer)
             .WithMany(l => l.ProjectLecturers)
             .HasForeignKey(pl => pl.LecturerId);

            builder.Entity<ProjectType>()
                .HasData(
                new ProjectType
                {
                    Id = 1,
                    Name = "Đồ án cơ sở"
                },
                new ProjectType
                {
                    Id = 2,
                    Name = "Đồ án chuyên ngành"
                },
                new ProjectType
                {
                    Id = 3,
                    Name = "Đồ án tổng hợp"
                });
            base.OnModelCreating(builder);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaveChangesSetTrackable();
            var auditEntries = OnBeforeSaveChanges();
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            OnAfterSaveChanges(auditEntries);
            return result;
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            OnBeforeSaveChangesSetTrackable();
            var auditEntries = OnBeforeSaveChanges();
            var result = base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            OnAfterSaveChanges(auditEntries);
            return result;
        }

        private void OnBeforeSaveChangesSetTrackable()
        {
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                if (entry.Entity is ITrackable trackable)
                {
                    var dateTimeNow = DateTime.UtcNow;

                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedDate = dateTimeNow;
                            break;
                        case EntityState.Added:
                            trackable.CreatedDate = dateTimeNow;
                            trackable.UpdatedDate = dateTimeNow;
                            break;
                    }
                }
            }
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            var auditEntries = new List<AuditEntry>();
            ChangeTracker.DetectChanges();
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {

                if (entry.Entity is Audit || !(entry.Entity is ITrackable) || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                {
                    continue;
                }
                var auditEntry = new AuditEntry(entry);

                auditEntry.TableName = entry.Metadata.GetTableName();
                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }

            foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
            {
                Audits.Add(auditEntry.ToAudit());
            }

            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
            {
                return Task.CompletedTask;
            }

            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                Audits.Add(auditEntry.ToAudit());
            }

            return SaveChangesAsync();
        }
    }
}
