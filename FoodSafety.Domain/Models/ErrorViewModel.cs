namespace FoodSafety.Domain.Models

{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Add this line
        public string? Message { get; set; }
    }

}