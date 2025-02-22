using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EShopWeb
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() : base() { }

        [StringLength(200)]
        public string FirstName { get; set; }
        [StringLength(200)]
        public string LastName { get; set; }
        [StringLength(300)]
        public string Street { get; set; }
        [StringLength(100)]
        public string City { get; set; }
        [StringLength(100)]
        public string Province { get; set; }
        [StringLength(20)]
        public string PostalCode { get; set; }
        [StringLength(100)]
        public string Country { get; set; }

        public virtual ICollection<ApplicationUserClaim> Claims { get; set; }
        public virtual ICollection<ApplicationUserLogin> Logins { get; set; }
        public virtual ICollection<ApplicationUserToken> Tokens { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
        public ApplicationRole(string roleName, string description, DateTime createdDate) : base(roleName)
        {
            this.Description = description;
            this.CreatedDate = createdDate;
        }

        [StringLength(500)]
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
        public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; }
    }

    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }

    public class ApplicationUserClaim : IdentityUserClaim<string>
    {
        public virtual ApplicationUser User { get; set; }
    }

    public class ApplicationUserLogin : IdentityUserLogin<string>
    {
        public virtual ApplicationUser User { get; set; }
    }

    public class ApplicationRoleClaim : IdentityRoleClaim<string>
    {
        public virtual ApplicationRole Role { get; set; }
    }

    public class ApplicationUserToken : IdentityUserToken<string>
    {
        public virtual ApplicationUser User { get; set; }
    }
}
