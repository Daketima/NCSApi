using DataLayer.Data;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Options;

namespace DataLayer.DataContext
{
    public class CustomContext : DbContext
    {


        public CustomContext()
        {

        }

        public CustomContext(DbContextOptions<CustomContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Assessment> Assessment { get; set; }
        public virtual DbSet<Tax> Tax { get; set; }
        public virtual DbSet<XMLArchive> XMLArchive { get; set; }
        public virtual DbSet<PaymentLog> Payment { get; set; }
        public virtual DbSet<LOV> LOV { get; set; }

        public virtual DbSet<PaymentStatus> PaymentStatus { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("server=TJ-DC-SRV-NCS\\SQLEXPRESS;database=CustomDuty;integrated security = true");
            //optionsBuilder.UseSqlServer("server=172.19.2.33,1433;database=CustomeDutyProduction;user id=appdev;password=Abcd1234");
            optionsBuilder.UseSqlServer("server=172.19.2.33,1433;database=CustomeDuty;user id=appdev;password=Abcd1234");
            // optionsBuilder.UseSqlServer(_setting.Value.DefaultConnectionString);           
            //optionsBuilder.UseSqlServer(AppSetting.GetConnectionString());
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
            .Entity<Assessment>()
            .Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder
            .Entity<Tax>()
            .Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder
            .Entity<XMLArchive>()
            .Property(e => e.Id).ValueGeneratedOnAdd();

            // modelBuilder.Entity<AssessmentType>().HasData( new AssessmentType{Id = 1, Name = "Excise"}, new AssessmentType{Id = 2, Name= "SD"}, new AssessmentType{Id = 3, Name = "SGD"});
            // modelBuilder.Entity<AssesmentPaymentLog>().Property(e => e.PaymentTypeId).HasDefaultValue(1);                      
        }
    }
}