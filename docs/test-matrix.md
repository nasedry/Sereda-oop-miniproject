# Матриця трасування (Test Matrix) 

Цей документ демонструє відповідність між розробленими Use Cases та інженерними автоматизованими тестами у файлі `UnitTest.cs`. 

| Ідентифікатор Use Case | Назва бізнес-сценарію | Назва інженерного тесту (У файлі UnitTest1.cs) |
| :--- | :--- | :--- |
| **UC-1: Розрахунок тарифів** | Знижка за тривалу оренду (від 5 днів) | `LongTermStrategy_ShouldApply15PercentDiscount` |
| **UC-1: Розрахунок тарифів** | Знижка вихідного дня (короткий термін) | `WeekendStrategy_ShouldApply5PercentDiscount` |
| **UC-2: Керування замовленням**| Запобігання повторної оренди одного авто | `Service_CreateOrder_WhenVehicleAlreadyRented_ShouldReturnFailure` |
| **UC-2: Керування замовленням**| Нарахування штрафу за запізнення повернення | `CompleteOrder_WithOverdue_ShouldApplyHeavyPenalty` |
| **UC-3: Збереження даних** | Стійкість до збоїв при читанні JSON (Fault) | `IT_6_LoadCorruptedJson_ShouldThrowInvalidOperationException` |
| **UC-3: Збереження даних** | Обробка відсутності файлу конфігурацій | `IT_7_MissingFile_ShouldReturnEmptyCollectionSafely` |