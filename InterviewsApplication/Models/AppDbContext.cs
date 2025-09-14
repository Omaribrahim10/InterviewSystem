using Microsoft.EntityFrameworkCore;
using InterviewsApplication.Models;
using InterviewsApplication.Models.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace InterviewsApplication.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TestStudent> TestStudents { get; set; }
        public DbSet<StudentsData> StudentsData { get; set; }
        public DbSet<StudentStatus> StudentStatuses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<InterviewSchedule> InterviewSchedules { get; set; }
        public DbSet<StudentBooking> StudentBookings { get; set; }
        public DbSet<InterviewHistory> InterviewHistories { get; set; }
        public DbSet<InterviewResult> InterviewResults { get; set; }
        public DbSet<MailingContent> MailingContents { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentID = 1, Name = "Personal Interview" },
                new Department { DepartmentID = 2, Name = "Medical" },
                new Department { DepartmentID = 3, Name = "English" }
            );

            modelBuilder.Entity<InterviewResult>(entity =>
            {
                entity.HasKey(ir => ir.ResultID);

                entity.HasOne(ir => ir.Department)
                      .WithMany()
                      .HasForeignKey(ir => ir.DepartmentID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ir => ir.Student)
                      .WithMany()
                      .HasForeignKey(ir => ir.UniversityID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ir => ir.Agent)
                      .WithMany()
                      .HasForeignKey(ir => ir.ReviewedBy)
                      .HasPrincipalKey(u => u.Id)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TestStudent>()
                .Property(ts => ts.Percentage)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<StudentsData>(entity =>
            {
                entity.HasKey(sd => sd.UniversityID);

                entity.HasOne(sd => sd.TestStudent)
                      .WithOne(ts => ts.StudentsData)
                      .HasForeignKey<StudentsData>(sd => sd.UniversityID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sd => sd.MailingContent)
                      .WithMany(mc => mc.TargetedStudents)
                      .HasForeignKey(sd => sd.MailID);
            });

            modelBuilder.Entity<StudentBooking>()
                .HasOne(ts => ts.Student)
                .WithOne(sb => sb.StudentBooking)
                .HasForeignKey<StudentBooking>(sb => sb.UniversityID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentBooking>()
                .HasOne(sb => sb.InterviewSchedule)
                .WithMany(s => s.StudentBookings)
                .HasForeignKey(sb => sb.ScheduleID);

            modelBuilder.Entity<InterviewHistory>()
                .HasOne(h => h.Student)
                .WithMany(s => s.InterviewHistories)
                .HasForeignKey(h => h.UniversityID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InterviewHistory>()
                .HasOne(h => h.Department)
                .WithMany(d => d.InterviewHistories)
                .HasForeignKey(h => h.DepartmentID);

            modelBuilder.Entity<InterviewHistory>()
                .HasOne(h => h.Agent)
                .WithMany(u => u.InterviewHistories)
                .HasForeignKey(h => h.ReviewedBy)
                .HasPrincipalKey(u => u.Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentStatus>()
                .HasOne(ss => ss.Student)
                .WithMany(ts => ts.StudentStatuses)
                .HasForeignKey(ss => ss.UniversityID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentStatus>()
                .HasOne(s => s.Agent)
                .WithMany(u => u.StudentStatus)
                .HasForeignKey(s => s.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MailingContent>()
                .HasOne(m => m.Agent)
                .WithMany(a => a.MailingContent)
                .HasForeignKey(m => m.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InterviewSchedule>()
                .HasOne(i => i.MailingContent)
                .WithMany(m => m.InterviewSchedules)
                .HasForeignKey(i => i.MailID);

            modelBuilder.Entity<InterviewSchedule>()
                .HasOne(i => i.Agent)
                .WithMany(a => a.InterviewSchedule)
                .HasForeignKey(i => i.CreatedBy)
                .HasPrincipalKey(u => u.Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentID);

            modelBuilder.Entity<StudentsView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vwInterviewApplicants");
            });

            modelBuilder.Entity<StudentStatus>()
                .Property(e => e.Status)
                .HasConversion<string>();

            modelBuilder.Entity<StudentsData>()
                .Property(e => e.MailResult)
                .HasConversion<string>();

            modelBuilder.Entity<InterviewHistory>()
                .Property(e => e.InterviewStatus)
                .HasConversion<string>();

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType.IsEnum ||
                               (Nullable.GetUnderlyingType(p.PropertyType)?.IsEnum ?? false));

                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion<string>();
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
