namespace CafeWeb.Enums
{
    public static class OrderStatus
    {
        public const string Pending = "Ожидает оплаты";
        public const string Error = "Ошибка оплаты";
        public const string InProcess = "Готовится";
        public const string Ready = "Готов";
    }
}
