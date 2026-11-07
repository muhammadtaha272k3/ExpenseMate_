namespace Expense_Tracker_02.Services
{
    public class CurrencyHelper
    {
        public static string Symbol { get; set; } = "$";

        public static string Format(decimal amount) => $"{Symbol}{amount:N2}";
    }
}
