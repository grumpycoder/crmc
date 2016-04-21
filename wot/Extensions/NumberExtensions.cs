using System;

namespace wot.Extensions
{
    public static class NumberExtensions
    {
        public static int Half(this int x)
        {
            return x / 2;
        }

        public static int Quarter(this int x)
        {
            return x / 4;
        }

        public static int ToInt(this string x)
        {
            return Convert.ToInt32(x);
        }

        public static int ToInt(this double x)
        {
            return Convert.ToInt32(x);
        }

        public static double ToDouble(this string x)
        {
            return Convert.ToDouble(x);
        }

        public static double ToDouble(this int x)
        {
            return Convert.ToDouble(x);
        }

        public static double PercentOf(this double value, double total)
        {
            return (double)((value / (double)total) * 100);
        }

        public static double AmountFromPercent(this double value, double total)
        {
            return (double)((value / 100) * (double)total);
        }
    }
}