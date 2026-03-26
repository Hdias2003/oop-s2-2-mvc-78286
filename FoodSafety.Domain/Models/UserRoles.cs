namespace FoodSafety.Domain.Models
{
    // Standardized role names to be used in Authorize attributes
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Inspector = "Inspector";
        public const string Viewer = "Viewer";
    }
}