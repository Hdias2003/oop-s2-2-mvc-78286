namespace FoodSafety.Domain.Models


{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        
        public string Message { get; set; } = "An unexpected error occurred.";

        
        public int? StatusCode { get; set; }
    }
}