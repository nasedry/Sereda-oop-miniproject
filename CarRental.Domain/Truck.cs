namespace CarRental.Domain
{
    public class Truck : Vehicle
    {
        public double MaxLoadCapacity { get; private set; } // в тоннах

        public Truck(Guid id, string brand, string licensePlate, decimal basePricePerDay, double maxLoadCapacity)
            : base(id, brand, licensePlate, basePricePerDay)
        {
            if (maxLoadCapacity <= 0) throw new ArgumentException("Вантажопідйомність має бути більшою за 0.");
            MaxLoadCapacity = maxLoadCapacity;
        }

        // Вантажівки мають додатковий збір за тоннаж при оренді
        public override decimal CalculateActualRentCost(int days)
        {
            decimal tonnageSurcharge = (decimal)(MaxLoadCapacity * 150); 
            return (BasePricePerDay + tonnageSurcharge) * days;
        }
    }
}