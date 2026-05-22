namespace CarRental.Domain
{
    public interface IDiscountStrategy
    {
        string Name { get; }
        decimal CalculateDiscount(decimal baseCost, int days);
    }

    public class RegularCustomerStrategy : IDiscountStrategy
    {
        public string Name => "Звичайна (без знижки)";
        public decimal CalculateDiscount(decimal baseCost, int days) => 0;
    }

    public class LongTermRentStrategy : IDiscountStrategy
    {
        public string Name => "Довгострокова оренди (від 5 днів)";
        public decimal CalculateDiscount(decimal baseCost, int days) 
            => days >= 5 ? baseCost * 0.15m : 0; // 15% знижки
    }

    public class WeekendStrategy : IDiscountStrategy
    {
        public string Name => "Вікенд-знижка (фіксована)";
        public decimal CalculateDiscount(decimal baseCost, int days) 
            => days <= 2 ? baseCost * 0.05m : 0; // 5% знижки на короткі поїздки
    }
}