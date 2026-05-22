using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using CarRental.Domain;

namespace CarRental.Infrastructure
{
    public class VehicleJsonConverter : JsonConverter<Vehicle>
    {
        public override Vehicle? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var root = jsonDoc.RootElement;
                var id = root.GetProperty("Id").GetGuid();
                var brand = root.GetProperty("Brand").GetString() ?? "";
                var plate = root.GetProperty("LicensePlate").GetString() ?? "";
                var price = root.GetProperty("BasePricePerDay").GetDecimal();
                var isAvailable = root.GetProperty("IsAvailable").GetBoolean();

                Vehicle vehicle;
                if (root.TryGetProperty("SeatingCapacity", out var seatingProp))
                {
                    vehicle = new PassengerCar(id, brand, plate, price, seatingProp.GetInt32());
                }
                else if (root.TryGetProperty("MaxLoadCapacity", out var loadProp))
                {
                    vehicle = new Truck(id, brand, plate, price, loadProp.GetDouble());
                }
                else
                {
                    throw new JsonException("Невідомий тип транспортного засобу.");
                }

                if (!isAvailable) vehicle.MarkAsRented();
                return vehicle;
            }
        }

        public override void Write(Utf8JsonWriter writer, Vehicle value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Id", value.Id);
            writer.WriteString("Brand", value.Brand);
            writer.WriteString("LicensePlate", value.LicensePlate);
            writer.WriteNumber("BasePricePerDay", value.BasePricePerDay);
            writer.WriteBoolean("IsAvailable", value.IsAvailable);

            if (value is PassengerCar car)
            {
                writer.WriteNumber("SeatingCapacity", car.SeatingCapacity);
            }
            else if (value is Truck truck)
            {
                writer.WriteNumber("MaxLoadCapacity", truck.MaxLoadCapacity);
            }

            writer.WriteEndObject();
        }
    }
}