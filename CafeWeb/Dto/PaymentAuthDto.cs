namespace CafeWeb.Dto
{
    public record PaymentAuthDto(string Token, DateTime CreatedAt, DateTime ExpiresAt);
}
