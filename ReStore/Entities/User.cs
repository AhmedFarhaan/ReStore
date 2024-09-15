using Microsoft.AspNetCore.Identity;

namespace ReStore.Entities
{
    public class User : IdentityUser<int>
    {
        public UserAddress Address { get; set; }
    }
}
