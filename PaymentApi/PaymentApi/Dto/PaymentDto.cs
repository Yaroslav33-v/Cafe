namespace PaymentApi.Dto
{
    public record PaymentDto(string CardToken, string LastFour, decimal Total, string Description);
    
}
