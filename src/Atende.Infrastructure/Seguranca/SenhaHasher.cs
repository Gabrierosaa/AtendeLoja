using Atende.Application.Portas;
using Microsoft.AspNetCore.Identity;

namespace Atende.Infrastructure.Seguranca
{
    public class SenhaHasher : ISenhaHasher
    {
        private readonly PasswordHasher<object> _hasher = new();

        public string Hash(string senha) => _hasher.HashPassword(new object(), senha);
    }
}
