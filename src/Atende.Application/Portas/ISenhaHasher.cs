namespace Atende.Application.Portas
{
    public interface ISenhaHasher
    {
        string Hash(string senha);
    }
}
