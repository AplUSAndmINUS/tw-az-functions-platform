using Azure.Data.Tables;
using System.Text.Json;

namespace SharedStorage.Extensions;

public static class TableEntityExtensions
{
    public static T ConvertTo<T>(this TableEntity entity) where T : class, new()
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var result = new T();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            if (entity.ContainsKey(property.Name) && property.CanWrite)
            {
                var value = entity[property.Name];
                if (value != null)
                {
                    if (property.PropertyType == value.GetType())
                    {
                        property.SetValue(result, value);
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(result, value.ToString());
                    }
                    else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                    {
                        if (DateTime.TryParse(value.ToString(), out var dateValue))
                            property.SetValue(result, dateValue);
                    }
                    else if (property.PropertyType == typeof(long) || property.PropertyType == typeof(long?))
                    {
                        if (long.TryParse(value.ToString(), out var longValue))
                            property.SetValue(result, longValue);
                    }
                    else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                    {
                        if (int.TryParse(value.ToString(), out var intValue))
                            property.SetValue(result, intValue);
                    }
                    else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                    {
                        if (bool.TryParse(value.ToString(), out var boolValue))
                            property.SetValue(result, boolValue);
                    }
                }
            }
        }

        // Handle special properties for ITableEntity
        if (result is ITableEntity tableEntity)
        {
            tableEntity.PartitionKey = entity.PartitionKey;
            tableEntity.RowKey = entity.RowKey;
            tableEntity.Timestamp = entity.Timestamp;
            tableEntity.ETag = entity.ETag;
        }

        return result;
    }
}