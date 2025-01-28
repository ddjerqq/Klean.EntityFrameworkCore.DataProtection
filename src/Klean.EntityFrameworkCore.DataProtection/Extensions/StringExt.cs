using System.Security.Cryptography;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Provides extensions for the <see cref="string"/> type.
/// </summary>
public static class StringExt
{
    /// <summary>
    /// The environment variable key for the hashing salt. `EFCORE_DATA_PROTECTION__HASHING_SALT`
    /// </summary>
    public const string HashingSaltKey = "EFCORE_DATA_PROTECTION__HASHING_SALT";

    /// <summary>
    /// Hashes the string using HMACSHA256
    /// </summary>
    /// <param name="value">The value to hash</param>
    /// <returns>Hashed value as hex lowercase</returns>
    /// <exception cref="InvalidOperationException">If `EFCORE_DATA_PROTECTION__HASHING_SALT` is not present in the environment</exception>
    public static string HmacSha256Hash(this string value)
    {
        var salt =
            Environment.GetEnvironmentVariable(HashingSaltKey)
            ?? throw new InvalidOperationException($"{HashingSaltKey} is not present in the environment, please set it to a strong value and keep it safe, otherwise querying will not work.");

        var saltBytes = Encoding.UTF8.GetBytes(salt);
        var payloadBytes = Encoding.UTF8.GetBytes(value);
        var hash = HMACSHA256.HashData(saltBytes, payloadBytes);
        var hexDigest = Convert.ToHexString(hash);
        return hexDigest.ToLower();
    }
}