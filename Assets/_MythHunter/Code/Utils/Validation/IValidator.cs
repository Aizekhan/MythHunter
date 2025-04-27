namespace MythHunter.Utils.Validation
{
    /// <summary>
    /// Інтерфейс для валідації об'єктів
    /// </summary>
    public interface IValidator<T>
    {
        ValidationResult Validate(T obj);
    }
}