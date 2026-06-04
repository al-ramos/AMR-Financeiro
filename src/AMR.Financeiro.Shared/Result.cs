namespace AMR.Financeiro.Shared;

public sealed class Result<T>
{
    public bool Sucesso { get; private init; }
    public T? Valor { get; private init; }
    public string Erro { get; private init; } = string.Empty;

    private Result() { }

    public static Result<T> Ok(T valor) => new() { Sucesso = true, Valor = valor };
    public static Result<T> Falha(string erro) => new() { Sucesso = false, Erro = erro };
}
