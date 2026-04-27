using System;
using Microsoft.AspNetCore.Identity;

namespace HashGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var hasher = new PasswordHasher<object>();
            string hash = hasher.HashPassword(null, "Password123!");
            Console.WriteLine(hash);
        }
    }
}
