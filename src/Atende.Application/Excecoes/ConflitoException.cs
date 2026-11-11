namespace Atende.Application.Excecoes
{
    public class ConflitoException : Exception
    {
        public ConflitoException(string mensagem) : base(mensagem) { }
    }
}
