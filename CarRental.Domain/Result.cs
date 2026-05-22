// Файл: CarRental.Domain/Result.cs
namespace CarRental.Domain
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; } // Знак ? указывает, что значение может быть null в случае ошибки
        public string? ErrorMessage { get; } // Строка ошибки тоже может быть null при успехе

        private Result(bool isSuccess, T? value, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, null);
        public static Result<T> Failure(string error) => new Result<T>(false, default, error);
    }
}