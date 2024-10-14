using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;

namespace ParserCombinatorConsole.Pidgin;

public readonly record struct TaskDate : IParsable<TaskDate>
{
    public const string DateFormat = "yyyy-MM-dd";
    public const string RoundTripFormat = "o";
    public readonly DateTimeKind Kind => Value.Kind;
    public readonly DateTime Value { get; }
    public readonly DateOnly DateOnly => DateOnly.FromDateTime(Value);
    public readonly bool IsDateOnly { get; }

    private TaskDate(DateTime dateTime, bool asDateOnly = false)
    {
        if (dateTime == DateTime.MinValue) throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime, "dateTime cannot be MinValue");
        if (dateTime == DateTime.MaxValue) throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime, "dateTime cannot be MaxValue");

        Value = dateTime.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => throw new NotImplementedException()
        };

        IsDateOnly = asDateOnly;
    }
    public TaskDate(DateTime dateTime) : this(dateTime, false) { }

    public TaskDate(DateOnly dateOnly) : this(dateOnly.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), true) { }

    public string ToString(string? format) => format is null ? ToString() : Value.ToString(format);

    public override string ToString() => IsDateOnly
        ? Value.ToString(DateFormat)
        : Value.ToString();

    public static TaskDate Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out TaskDate result)
    {
        throw new NotImplementedException();
    }

    public static implicit operator TaskDate(DateTime dateTime) => new(dateTime);
    public static implicit operator DateTime(TaskDate taskDate) => taskDate.Value;
}
