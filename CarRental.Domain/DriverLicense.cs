
namespace CarRental.Domain
{
    public class DriverLicense
    {
        public string Number { get; }
        public string Category { get; }

        public DriverLicense(string number, string category)
        {
            if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Номер ліцензії обов'язковий.");
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Категорія обов'язкова.");
            
            Number = number;
            Category = category.ToUpper();
        }
    }
}