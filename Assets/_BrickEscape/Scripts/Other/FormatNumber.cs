using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FormatNumber
{
    public static string FormatNumberInt(long number)
    {
        double value = number;
        string[] suffixes = { "", "K", "M", "B", "T", "Qa", "Qi" };
        int suffixIndex = 0;

        while (value >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1000;
            suffixIndex++;
        }

        string formatted = value % 1 == 0 ? value.ToString("0") : value.ToString("0.##");

        return formatted + suffixes[suffixIndex];
    }
}
