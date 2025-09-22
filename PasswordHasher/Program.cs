using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        // Generate hashes for test passwords
        var passwords = new[]
        {
            ("admin@company.com", "Admin123!"),
            ("john.manager@company.com", "Manager123!"),
            ("alice.analyst@company.com", "Analyst123!"),
            ("bob.developer@company.com", "Developer123!")
        };

        Console.WriteLine("-- Update Users with properly hashed passwords");
        Console.WriteLine("USE WorkIntakeSystem;");
        Console.WriteLine("GO");
        Console.WriteLine();

        foreach (var (email, password) in passwords)
        {
            var (hash, salt) = HashPassword(password);
            Console.WriteLine($"UPDATE Users SET PasswordHash = '{hash}', PasswordSalt = '{salt}' WHERE Email = '{email}';");
        }

        Console.WriteLine();
        Console.WriteLine("PRINT 'User passwords updated with proper HMAC-SHA512 hashing';");
        Console.WriteLine("PRINT 'Test passwords:';");
        Console.WriteLine("PRINT '  admin@company.com -> Admin123!';");
        Console.WriteLine("PRINT '  john.manager@company.com -> Manager123!';");
        Console.WriteLine("PRINT '  alice.analyst@company.com -> Analyst123!';");
        Console.WriteLine("PRINT '  bob.developer@company.com -> Developer123!';");
        Console.WriteLine("GO");
    }

    public static (string hash, string salt) HashPassword(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = Convert.ToBase64String(hmac.Key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        return (hash, salt);
    }
}