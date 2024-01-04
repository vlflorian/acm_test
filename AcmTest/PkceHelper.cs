namespace AcmTest;

using System;
using System.Security.Cryptography;
using System.Text;

public class PkceHelper
{
    // Generate a secure random string for the code verifier
    public static string GenerateCodeVerifier(int length = 64)
    {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
        var random = new Random();

        var verifier = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            verifier.Append(validChars[random.Next(validChars.Length)]);
        }

        return verifier.ToString();
    }

    // Transform the code verifier into a code challenge using SHA-256
    public static string TransformCodeVerifier(string codeVerifier)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Base64UrlEncode(hashedBytes);
        }
    }

    // Base64Url encode without padding
    private static string Base64UrlEncode(byte[] bytes)
    {
        string base64 = Convert.ToBase64String(bytes);
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
