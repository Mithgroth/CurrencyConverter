namespace Domain;

public sealed record Currency
{
    public string Code { get; }

    public Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Currency code is required.", nameof(code));
        }

        code = code.Trim().ToUpperInvariant();
        if (code.Length != 3 || !code.All(char.IsLetter))
        {
            throw new ArgumentException($"Invalid currency code: '{code}'", nameof(code));
        }

        Code = code;
    }

    public override string ToString() => Code;
}
