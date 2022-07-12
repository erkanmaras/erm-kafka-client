using System.Text;

namespace Erm.KafkaClient;

/// <summary>
///     Provides extension methods over <see cref="IKafkaMessageHeaders" />
/// </summary>
public static class KafkaMessageHeaderExtensions
{
    /// <summary>
    ///     Gets a header value as string
    /// </summary>
    /// <param name="headers">The <see cref="IKafkaMessageHeaders" /> object that this method was called on.</param>
    /// <param name="key">The header key</param>
    /// <param name="encoding">The encoding used to get the string</param>
    /// <returns>The retrieved string header value</returns>
    public static string GetString(this IKafkaMessageHeaders headers, string key, Encoding encoding)
    {
        var value = headers[key];
        return value != null ? encoding.GetString(value) : null;
    }

    /// <summary>
    ///     Gets a header value as an UTF8 string
    /// </summary>
    /// <param name="headers">The <see cref="IKafkaMessageHeaders" /> object that this method was called on.</param>
    /// <param name="key">The header key</param>
    /// <returns>The retrieved string header value</returns>
    public static string GetString(this IKafkaMessageHeaders headers, string key)
    {
        return headers.GetString(key, Encoding.UTF8);
    }

    /// <summary>
    ///     Sets a header value as string
    /// </summary>
    /// <param name="headers">The <see cref="IKafkaMessageHeaders" /> object that this method was called on.</param>
    /// <param name="key">The header key</param>
    /// <param name="value">The header value</param>
    /// <param name="encoding">The encoding used to store the string</param>
    public static void AddString(this IKafkaMessageHeaders headers, string key, string value, Encoding encoding)
    {
        if (key is not null)
        {
            headers[key] = value != null ? encoding.GetBytes(value) : null;
        }
    }

    /// <summary>
    ///     Sets a header value as UTF8 string
    /// </summary>
    /// <param name="headers">The <see cref="IKafkaMessageHeaders" /> object that this method was called on.</param>
    /// <param name="key">The header key</param>
    /// <param name="value">The header value</param>
    public static void AddString(this IKafkaMessageHeaders headers, string key, string value)
    {
        headers.AddString(key, value, Encoding.UTF8);
    }
}