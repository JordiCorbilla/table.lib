using System;
using System.Globalization;
using System.IO;

namespace table.lib
{
    internal static class RenderingHelpers
    {
        private static readonly char[] CsvSpecialCharacters = [',', '"', '\r', '\n'];

        public static string RenderToString(Action<TextWriter> render)
        {
            using var writer = new StringWriter(CultureInfo.CurrentCulture);
            render(writer);
            return writer.ToString();
        }

        public static void WriteCenteredCell(TextWriter writer, string value, int width)
        {
            var padding = Math.Max(0, width - value.Length);
            var left = padding / 2;
            writer.Write(' ');
            WriteRepeated(writer, ' ', left);
            writer.Write(value);
            WriteRepeated(writer, ' ', padding - left);
            writer.Write(" |");
        }

        public static void WriteValueCell(TextWriter writer, string value, int width,
            TextJustification justification = TextJustification.Left)
        {
            var padding = Math.Max(0, width - value.Length);
            var left = justification switch
            {
                TextJustification.Centered => padding / 2,
                TextJustification.Right => padding,
                _ => 0
            };

            writer.Write(' ');
            WriteRepeated(writer, ' ', left);
            writer.Write(value);
            WriteRepeated(writer, ' ', padding - left);
            writer.Write(" |");
        }

        public static void WriteSeparatorCell(TextWriter writer, int width)
        {
            writer.Write(' ');
            WriteRepeated(writer, '-', width);
            writer.Write(" |");
        }

        public static void WriteRepeated(TextWriter writer, char value, int count)
        {
            if (count > 0)
                writer.Write(new string(value, count));
        }

        public static void WriteCsvValue(TextWriter writer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                writer.Write(value);
                return;
            }

            if (value.IndexOfAny(CsvSpecialCharacters) < 0)
            {
                writer.Write(value);
                return;
            }

            writer.Write('"');
            var start = 0;
            while (start < value.Length)
            {
                var quote = value.IndexOf('"', start);
                if (quote < 0)
                {
                    writer.Write(value.AsSpan(start));
                    break;
                }

                writer.Write(value.AsSpan(start, quote - start));
                writer.Write("\"\"");
                start = quote + 1;
            }
            writer.Write('"');
        }

        public static string ToSqlLiteral(object value)
        {
            return value switch
            {
                null => "NULL",
                string text => $"'{text.ToSql()}'",
                char character => $"'{character.ToString().ToSql()}'",
                bool boolean => boolean ? "1" : "0",
                DateTime dateTime => $"'{dateTime:yyyy-MM-dd}'",
                DateTimeOffset dateTimeOffset => $"'{dateTimeOffset:yyyy-MM-dd}'",
                byte or sbyte or short or ushort or int or uint or long or ulong =>
                    Convert.ToString(value, CultureInfo.InvariantCulture),
                decimal number => number.ToString("#0.0###", CultureInfo.InvariantCulture),
                double number => number.ToString("#0.0###", CultureInfo.InvariantCulture),
                float number => number.ToString("#0.0###", CultureInfo.InvariantCulture),
                _ => value.ToString().ToSql()
            };
        }
    }
}
