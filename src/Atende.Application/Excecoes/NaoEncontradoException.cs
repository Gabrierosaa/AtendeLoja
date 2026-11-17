namespace Atende.Application.Excecoes
{
    public class NaoEncontradoException : Exception
    {
        public NaoEncontradoException(string message) : base(message)
        {
        }
    }
}
