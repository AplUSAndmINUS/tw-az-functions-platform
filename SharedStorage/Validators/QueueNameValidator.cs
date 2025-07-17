using System.Text.RegularExpressions;

namespace SharedStorage.Validators;

public static class QueueNameValidator
{
    private static readonly Regex QueueNameRegex = new Regex(
        @"^[a-z0-9][a-z0-9-]*[a-z0-9]$",
        RegexOptions.Compiled
    );

    public static void ValidateQueueName(string queueName)
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));

        if (queueName.Length < 3 || queueName.Length > 63)
            throw new ArgumentException("Queue name must be between 3 and 63 characters long.", nameof(queueName));

        if (!QueueNameRegex.IsMatch(queueName))
            throw new ArgumentException("Queue name must start and end with a letter or number, and can contain only letters, numbers, and hyphens.", nameof(queueName));

        if (queueName.Contains("--"))
            throw new ArgumentException("Queue name cannot contain consecutive hyphens.", nameof(queueName));
    }
}