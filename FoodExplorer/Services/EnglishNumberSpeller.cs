using System.Globalization;
using System.Text.RegularExpressions;

namespace FoodExplorer.Services;

/// <summary>
/// Spells digits as English words so TTS never reads numbers using Chinese pronunciation.
/// </summary>
internal static class EnglishNumberSpeller
{
    private static readonly string[] Ones =
    [
        "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
        "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
        "seventeen", "eighteen", "nineteen"
    ];

    private static readonly string[] Tens =
    [
        "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"
    ];

    public static string SpellDigitsInText(string text) =>
        Regex.Replace(text, @"\d+", match =>
        {
            if (!int.TryParse(match.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
                return match.Value;

            return value is >= 0 and <= 9999 ? Spell(value) : match.Value;
        });

    public static string Spell(int number)
    {
        if (number is < 0 or > 9999)
            return number.ToString(CultureInfo.InvariantCulture);

        if (number < 20)
            return Ones[number];

        if (number < 100)
        {
            var tens = Tens[number / 10];
            var ones = number % 10;
            return ones == 0 ? tens : $"{tens} {Ones[ones]}";
        }

        if (number < 1000)
        {
            var hundreds = number / 100;
            var remainder = number % 100;
            var head = $"{Ones[hundreds]} hundred";
            return remainder == 0 ? head : $"{head} {Spell(remainder)}";
        }

        var thousands = number / 1000;
        var rest = number % 1000;
        var prefix = $"{Spell(thousands)} thousand";
        return rest == 0 ? prefix : $"{prefix} {Spell(rest)}";
    }
}
