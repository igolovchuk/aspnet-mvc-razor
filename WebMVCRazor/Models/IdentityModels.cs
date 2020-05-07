using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebMVCRazor.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public bool IsActive { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Visit> Visits { get; set; }


        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }


    [Table("Providers")]
    public class Provider
    {
        [Key]
        public string ProviderId { get; set; }

        public DateTime DateOfBirth { get; set; }

        [ForeignKey("ProviderId")]
        public virtual ApplicationUser User { get; set; }

    }

    [Table("Facilities")]
    public class Facility
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public bool IsActive { get; set; }

        public string PhoneNumber { get; set; }

    }

    [Table("Patients")]
    public class Patient
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int PatientId { get; set; }
        
        public int LocationId { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public bool IsActive { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime? AdmissionDate { get; set; }

        public DateTime? DischargeDate { get; set; }

        public DateTime? EligibileDate { get; set; }

        public DateTime? DeadlineDate { get; set; }

        [ForeignKey("LocationId")]
        public virtual Facility Facility { get; set; }

    }

    [Table("Visits")]
    public class Visit
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int VisitId { get; set; }

        public DateTime VisitDate { get; set; }

        public VisitType VisitType { get; set; }

        public string ProviderId { get; set; }

        public int PatientId { get; set; }

        public bool IsNoteComplete { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }

    }
}