using System;

namespace CarRental.Domain.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string ErrorMessage { get; }
        public bool IsFailure => !IsSuccess;

        // Захищений конструктор
        protected Result(bool isSuccess, T? value, string errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
        }

        // Фабричний метод для успішного результату
        public static Result<T> Success(T value) => new Result<T>(true, value, string.Empty);

        // Фабричний метод для помилки
        public static Result<T> Failure(string message) => new Result<T>(false, default, message);
    }
}