// Файл: CarRental.Domain/RentalPeriod.cs (Value Object для обробки дат)
namespace CarRental.Domain
{
    public class RentalPeriod
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public int TotalDays => (EndDate - StartDate).Days;

        public RentalPeriod(DateTime startDate, DateTime endDate)
        {
            if (startDate < DateTime.Today) throw new ArgumentException("Дата початку не може бути в минулому.");
            if (endDate <= startDate) throw new ArgumentException("Дата завершення повинна бути пізнішою за дату початку.");

            StartDate = startDate;
            EndDate = endDate;
        }
    }
}