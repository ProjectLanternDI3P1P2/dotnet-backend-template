namespace Combat.Domain.ValueObjects;

/// <summary>
/// The 64-bit value from which a whole dungeon is derived. It is shared with players as
/// 13 Crockford base32 characters (for example <c>0KX4M2T9QZ7PA</c>), which avoids
/// ambiguous letters (I, L, O, U) and fits in a URL without escaping.
/// </summary>
public readonly record struct Seed
{
    public const int TextLength = 13;

    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
    private const int BitsPerCharacter = 5;

    // 13 characters carry 65 bits: the first one only holds the 4 highest bits.
    private const int MaximumFirstCharacterValue = 15;

    public Seed(ulong value)
    {
        Value = value;
    }

    public ulong Value { get; }

    public override string ToString()
    {
        Span<char> characters = stackalloc char[TextLength];
        ulong remaining = Value;

        for (int index = TextLength - 1; index >= 0; index--)
        {
            characters[index] = Alphabet[(int)(remaining & 31)];
            remaining >>= BitsPerCharacter;
        }

        return new string(characters);
    }

    public static Seed Parse(string text)
    {
        return TryParse(text, out Seed seed)
            ? seed
            : throw new FormatException(
                $"'{text}' is not a valid dungeon seed: {TextLength} base32 characters are expected."
            );
    }

    public static bool TryParse(string? text, out Seed seed)
    {
        seed = default;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        ulong value = 0;
        int count = 0;

        foreach (char raw in text.Trim())
        {
            // Dashes are accepted so that players can share "0KX4-M2T9-QZ7PA".
            if (raw == '-')
            {
                continue;
            }

            int digit = DecodeCharacter(raw);
            if (digit < 0 || count >= TextLength)
            {
                return false;
            }

            if (count == 0 && digit > MaximumFirstCharacterValue)
            {
                return false;
            }

            value = (value << BitsPerCharacter) | (uint)digit;
            count++;
        }

        if (count != TextLength)
        {
            return false;
        }

        seed = new Seed(value);
        return true;
    }

    private static int DecodeCharacter(char character)
    {
        char upper = char.ToUpperInvariant(character);

        // Crockford decoding: the letters a human confuses map to the digit they look like.
        return upper switch
        {
            'O' => 0,
            'I' or 'L' => 1,
            _ => Alphabet.IndexOf(upper, StringComparison.Ordinal),
        };
    }
}
