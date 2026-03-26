using FoodSafety.Domain.Models;
using Xunit;

namespace FoodSafety.Tests.Unit
{
    public class RoleAuthorizationTests
    {
        [Fact]
        public void UserRoles_ContainsRequiredConstants()
        {
            // Ensures the roles used in [Authorize] attributes exist in the system
            Assert.Equal("Admin", UserRoles.Admin);
            Assert.Equal("Inspector", UserRoles.Inspector);
            Assert.Equal("Viewer", UserRoles.Viewer);
        }
    }
}