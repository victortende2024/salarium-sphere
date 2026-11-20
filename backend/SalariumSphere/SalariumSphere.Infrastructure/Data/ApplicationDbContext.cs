using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Common;
using SalariumSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SalariumSphere.Infrastructure.Data
{

    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly string _connectionString;

        public ApplicationDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        // DbSets for all entities
        public DbSet<Company> Companies { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<PayrollProfile> PayrollProfiles { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeSalaryStructure> EmployeeSalaryStructures { get; set; }
        public DbSet<EmployeeCustomBenefit> EmployeeCustomBenefits { get; set; }
        public DbSet<PayrollRule> PayrollRules { get; set; }
        public DbSet<PayrollRun> PayrollRuns { get; set; }
        public DbSet<PayrollRunDetail> PayrollRunDetails { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ProfileBenefit> ProfileBenefits { get; set; }
        public DbSet<PayrollTaxRule> PayrollTaxRules { get; set; }
        public DbSet<EmployeeDeduction> EmployeeDeductions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Company ----
            modelBuilder.Entity<Company>()
                .HasIndex(c => c.TenantId); // For faster queries

            // ---- Department ----
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Company)
                .WithMany()
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Cascade); // If company deleted, departments are deleted

            // ---- PayrollProfile ----
            modelBuilder.Entity<PayrollProfile>()
                .Property(p => p.BaseSalaryCurrency)
                .HasConversion<int>(); // Store enum as int

            // ---- Employee ----
            modelBuilder.Entity<Employee>()
                .HasIndex(e => new { e.TenantId, e.EmployeeCode }) // Enforce unique EmployeeCode per tenant
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting company with active employees

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.PrimaryProfile)
                .WithMany()
                .HasForeignKey(e => e.PrimaryProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- EmployeeSalaryStructure ----
            modelBuilder.Entity<EmployeeSalaryStructure>()
                .HasOne(ess => ess.Employee)
                .WithMany()
                .HasForeignKey(ess => ess.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade); // If employee is deleted, their salary structures are deleted

            modelBuilder.Entity<EmployeeSalaryStructure>()
                .Property(ess => ess.Currency)
                .HasConversion<int>();

            // ---- EmployeeCustomBenefit ----
            modelBuilder.Entity<EmployeeCustomBenefit>()
                .HasOne(ecb => ecb.Employee)
                .WithMany()
                .HasForeignKey(ecb => ecb.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeCustomBenefit>()
                .Property(ecb => ecb.Currency)
                .HasConversion<int>();

            modelBuilder.Entity<PayrollProfile>()
                 .Property(p => p.BaseSalaryAmount)
                 .HasPrecision(18, 4);

            modelBuilder.Entity<EmployeeSalaryStructure>()
                .Property(ess => ess.Amount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<EmployeeCustomBenefit>()
                .Property(ecb => ecb.Amount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<PayrollRule>()
                .HasIndex(r => r.WorkflowName);
            // ---- PayrollRun ----
            modelBuilder.Entity<PayrollRun>()
                .HasOne(pr => pr.Company)
                .WithMany()
                .HasForeignKey(pr => pr.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PayrollRun>()
                .HasIndex(pr => new { pr.CompanyId, pr.PayPeriodStart, pr.PayPeriodEnd });

            // ---- PayrollRunDetail ----
            modelBuilder.Entity<PayrollRunDetail>()
                .HasOne(prd => prd.PayrollRun)
                .WithMany(pr => pr.Details)
                .HasForeignKey(prd => prd.PayrollRunId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PayrollRunDetail>()
                .HasOne(prd => prd.Employee)
                .WithMany()
                .HasForeignKey(prd => prd.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PayrollRunDetail>()
                .Property(prd => prd.Currency)
                .HasConversion<int>();

            modelBuilder.Entity<PayrollRunDetail>()
                .Property(prd => prd.LineItemsJson)
                .HasColumnType("nvarchar(max)"); // For JSON storage
                                                 // ---- Decimal Precision for PayrollRun ----
            modelBuilder.Entity<PayrollRun>()
                .Property(pr => pr.TotalGrossPay)
                .HasPrecision(18, 4);

            modelBuilder.Entity<PayrollRunDetail>()
                .Property(prd => prd.TotalDeductions)
                .HasPrecision(18, 4);


            modelBuilder.Entity<PayrollRun>()
                .Property(pr => pr.TotalNetPay)
                .HasPrecision(18, 4);



            // ---- Decimal Precision for PayrollRunDetail ----
            modelBuilder.Entity<PayrollRunDetail>()
                .Property(prd => prd.GrossPay)
                .HasPrecision(18, 4);

            modelBuilder.Entity<PayrollRunDetail>()
                .Property(prd => prd.NetPay)
                .HasPrecision(18, 4);

            modelBuilder.Entity<PayrollRunDetail>()
                .Property(prd => prd.TotalDeductions)
                .HasPrecision(18, 4);

            // For EmployeeCustomBenefit
            modelBuilder.Entity<EmployeeCustomBenefit>()
                .HasOne(ecb => ecb.Employee)
                .WithMany(e => e.EmployeeCustomBenefits)   // Requires Employee to have this collection
                .HasForeignKey(ecb => ecb.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // For EmployeeSalaryStructure
            modelBuilder.Entity<EmployeeSalaryStructure>()
                .HasOne(ess => ess.Employee)
                .WithMany(e => e.EmployeeSalaryStructures) // Requires Employee to have this collection
                .HasForeignKey(ess => ess.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- Decimal Precision for Monetary Values ----
            modelBuilder.Entity<PayrollProfile>()
                .Property(p => p.BaseSalaryAmount)
                .HasPrecision(18, 4);

            // Add this for ProfileBenefit
            modelBuilder.Entity<ProfileBenefit>()
                .Property(pb => pb.Amount)
                .HasPrecision(18, 4);

            // EmployeeDeduction
            modelBuilder.Entity<EmployeeDeduction>()
                .Property(ed => ed.FixedAmount)
                .HasPrecision(18, 4)
                .IsRequired(false);
            modelBuilder.Entity<EmployeeDeduction>()
                .Property(ed => ed.CapAmount)
                .HasPrecision(18, 4)
                .IsRequired(false);
            modelBuilder.Entity<EmployeeDeduction>()
                .Property(ed => ed.ThresholdAmount)
                .HasPrecision(18, 4)
                .IsRequired(false);

            // PayrollTaxRule
            modelBuilder.Entity<PayrollTaxRule>()
                .Property(ptr => ptr.FixedAmount)
                .HasPrecision(18, 4)
                .IsRequired(false);
            modelBuilder.Entity<PayrollTaxRule>()
                .Property(ptr => ptr.CapAmount)
                .HasPrecision(18, 4)
                .IsRequired(false);
            modelBuilder.Entity<PayrollTaxRule>()
                .Property(ptr => ptr.ThresholdAmount)
                .HasPrecision(18, 4)
                .IsRequired(false);

            // ---- Decimal Precision for Percentage Fields ----
            // EmployeeDeduction
            modelBuilder.Entity<EmployeeDeduction>()
                .Property(ed => ed.PercentageValue)
                .HasPrecision(18, 4)
                .IsRequired(false);
            modelBuilder.Entity<EmployeeDeduction>()
                .Property(ed => ed.PercentageOfGross)
                .HasPrecision(18, 4)
                .IsRequired(false);

            // PayrollTaxRule
            modelBuilder.Entity<PayrollTaxRule>()
                .Property(ptr => ptr.PercentageValue)
                .HasPrecision(18, 4)
                .IsRequired(false);
            modelBuilder.Entity<PayrollTaxRule>()
                .Property(ptr => ptr.PercentageOfGross)
                .HasPrecision(18, 4)
                .IsRequired(false);

            // Company self‑reference (Parent/Child)
            modelBuilder.Entity<Company>()
                .HasOne(c => c.ParentCompany)
                .WithMany(c => c.ChildCompanies)
                .HasForeignKey(c => c.ParentCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Company → Departments
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Company)
                .WithMany(c => c.Departments)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            //User → Employee (optional)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithMany()
                .HasForeignKey(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        public new DbSet<T> Set<T>() where T : BaseEntity => base.Set<T>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Auto-update UpdatedAtUtc on modifications (coming later)
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
