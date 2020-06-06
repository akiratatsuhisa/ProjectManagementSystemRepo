using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjectManagementWebApp.Models;
using ProjectManagementWebApp.ViewModels;

namespace ProjectManagementWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public virtual DbSet<Student> Students { get; set; }

        public virtual DbSet<Lecturer> Lecturers { get; set; }

        public virtual DbSet<Audit> Audits { get; set; }

        public virtual DbSet<Faculty> Faculties { get; set; }

        public virtual DbSet<Semester> Semesters { get; set; }

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
            #region Config Entity Project 
            builder.Entity<Project>()
             .HasIndex(p => p.UniqueId)
             .IsUnique();
            builder.Entity<Project>()
                .Property(p => p.Status)
                .HasConversion(new EnumToStringConverter<ProjectStatus>());
            #endregion

            #region Config Entity Project Member
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
            builder.Entity<ProjectMember>()
              .Property(pm => pm.Type)
              .HasConversion(new EnumToStringConverter<ProjectMemberType>());
            #endregion

            #region Config Entity Project Lecturer
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
            builder.Entity<ProjectLecturer>()
                .Property(pl => pl.Type)
                .HasConversion(new EnumToStringConverter<ProjectLecturerType>());
            #endregion

            #region Config Entity User
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Student)
                .WithOne(s => s.User)
                .HasForeignKey<Student>(s => s.Id);
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Lecturer)
                .WithOne(s => s.User)
                .HasForeignKey<Lecturer>(l => l.Id);
            #endregion

            #region Config Another Entity
            builder.Entity<Semester>()
              .HasIndex(s => s.StartedDate);
            #endregion

            InitData(builder);
            base.OnModelCreating(builder);
        }

        private void InitData(ModelBuilder builder)
        {
            builder.Entity<ProjectType>()
                .HasData(
                new ProjectType { Id = 1, Name = "Đồ án cơ sở" },
                new ProjectType { Id = 2, Name = "Đồ án chuyên ngành" },
                new ProjectType { Id = 3, Name = "Đồ án tổng hợp" }
                );
            builder.Entity<Faculty>()
                .HasData(
                new Faculty { Id = 1, Name = "Công nghệ thông tin" }
                );
            builder.Entity<Semester>()
               .HasData(
               new Semester { Id = 1, Name = "2019-1", StartedDate = new DateTime(2019, 9, 1), EndedDate = new DateTime(2020, 2, 1) },
               new Semester { Id = 2, Name = "2019-2", StartedDate = new DateTime(2020, 2, 1), EndedDate = new DateTime(2020, 7, 1) }
               );
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaveChangesSetTrackable();
            var auditEntries = OnBeforeSaveChanges();
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            OnAfterSaveChanges(auditEntries).Wait();
            return result;
        }

        public async override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            OnBeforeSaveChangesSetTrackable();
            var auditEntries = OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await OnAfterSaveChanges(auditEntries);
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
