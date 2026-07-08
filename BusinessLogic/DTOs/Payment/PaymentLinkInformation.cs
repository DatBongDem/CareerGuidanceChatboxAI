namespace BusinessLogic.DTOs.Payment
{
    public class PaymentLinkInformation
    {
        public string Bin { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public long OrderCode { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string PaymentLinkId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CheckoutUrl { get; set; } = string.Empty;
        public string QrCode { get; set; } = string.Empty;
    }
}
