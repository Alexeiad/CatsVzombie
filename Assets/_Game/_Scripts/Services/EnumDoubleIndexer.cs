using System;
using System.Collections.Generic;
using System.Linq;

public class EnumDoubleIndexer<TEnum1, TEnum2, TValue>
    where TEnum1 : Enum
    where TEnum2 : Enum
{
    private readonly Dictionary<TEnum1, int> _firstEnumOffsets;
    private readonly Dictionary<TEnum2, int> _secondEnumOffsets;
    private readonly List<TValue> _values;

    public EnumDoubleIndexer(List<TValue> values)
    {
        _values = values ?? throw new ArgumentNullException(nameof(values));
        _firstEnumOffsets = new Dictionary<TEnum1, int>();
        _secondEnumOffsets = new Dictionary<TEnum2, int>();

        InitializeOffsets();
    }

    private void InitializeOffsets()
    {
        // Инициализация смещений для первого enum
        var firstEnumValues = Enum.GetValues(typeof(TEnum1)).Cast<TEnum1>().ToArray();
        for (int i = 0; i < firstEnumValues.Length; i++)
        {
            _firstEnumOffsets[firstEnumValues[i]] = i;
        }

        // Инициализация смещений для второго enum
        var secondEnumValues = Enum.GetValues(typeof(TEnum2)).Cast<TEnum2>().ToArray();
        for (int i = 0; i < secondEnumValues.Length; i++)
        {
            _secondEnumOffsets[secondEnumValues[i]] = i * firstEnumValues.Length;
        }
    }

    public TValue GetValue(TEnum1 firstEnum, TEnum2 secondEnum)
    {
        if (!_firstEnumOffsets.ContainsKey(firstEnum))
            throw new ArgumentException($"Unknown enum value: {firstEnum}", nameof(firstEnum));

        if (!_secondEnumOffsets.ContainsKey(secondEnum))
            throw new ArgumentException($"Unknown enum value: {secondEnum}", nameof(secondEnum));

        int index = _firstEnumOffsets[firstEnum] + _secondEnumOffsets[secondEnum];

        if (index >= _values.Count)
            return _values[0]; // Fallback to default as in original code

        return _values[index];
    }

    public bool TryGetValue(TEnum1 firstEnum, TEnum2 secondEnum, out TValue value)
    {
        value = default(TValue);

        if (!_firstEnumOffsets.ContainsKey(firstEnum) || !_secondEnumOffsets.ContainsKey(secondEnum))
            return false;

        int index = _firstEnumOffsets[firstEnum] + _secondEnumOffsets[secondEnum];

        if (index >= _values.Count)
        {
            value = _values[0];
            return true;
        }

        value = _values[index];
        return true;
    }
}