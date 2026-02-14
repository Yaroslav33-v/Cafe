namespace PaymentApi.Dto
{
    public record PaymentDto(string CardToken, string LastFour, int ExpMonth, int ExpYear, decimal Total, string Description);
    
}
