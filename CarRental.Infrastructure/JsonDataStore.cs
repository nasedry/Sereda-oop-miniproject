using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CarRental.Domain;

namespace CarRental.Infrastructure
{
    public class JsonDataStore<T> : IDataStore<T>
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;

        public JsonDataStore(string fileName)
        {
            string appDataPath = AppDomain.CurrentDomain.BaseDirectory;
            _filePath = Path.Combine(appDataPath, fileName);
            
            _options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            _options.Converters.Add(new VehicleJsonConverter());
        }

        public async Task<IReadOnlyCollection<T>> LoadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new List<T>();

                using (var stream = File.OpenRead(_filePath))
                {
                    var data = await JsonSerializer.DeserializeAsync<List<T>>(stream, _options, cancellationToken);
                    return data ?? new List<T>();
                }
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Файл даних пошкоджений або має некоректний формат JSON.");
            }
            catch (IOException)
            {
                throw new InvalidOperationException("Помилка доступу до файлу даних.");
            }
        }

        public async Task SaveAsync(IReadOnlyCollection<T> items, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var stream = File.Create(_filePath))
                {
                    await JsonSerializer.SerializeAsync(stream, items, _options, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Не вдалося зберегти дані у файл: {ex.Message}");
            }
        }
    }
}