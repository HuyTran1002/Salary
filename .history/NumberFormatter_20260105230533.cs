using System;
using System.Windows.Forms;
using System.Globalization;

namespace SalaryCalculator
{
    /// <summary>
    /// Helper class for formatting number inputs with thousand separators in real-time
    /// Ví dụ: 1000000 → 1,000,000 (lúc đang gõ)
    /// </summary>
    public static class NumberFormatter
    {
        /// <summary>
        /// Format number input in real-time as user types
        /// </summary>
        public static void FormatNumberInput(TextBox textBox)
        {
            textBox.TextChanged += (s, e) =>
            {
                string text = textBox.Text;
                int oldSelStart = textBox.SelectionStart;
                int oldLength = textBox.Text.Length;

                // Remove all non-digit characters
                string cleaned = "";
                foreach (char c in text)
                {
                    if (char.IsDigit(c))
                        cleaned += c;
                }

                // Allow empty (user can clear all)
                if (string.IsNullOrEmpty(cleaned))
                {
                    textBox.TextChanged -= null; // prevent recursion
                    textBox.Text = "";
                    textBox.SelectionStart = 0;
                    return;
                }

                // Remove leading zeros (but allow single zero)
                cleaned = cleaned.TrimStart('0');
                if (cleaned == "") cleaned = "0";

                if (decimal.TryParse(cleaned, out decimal value))
                {
                    string formatted = value.ToString("N0", new CultureInfo("en-US"));
                    // Calculate new caret position
                    int diff = formatted.Length - oldLength;
                    textBox.TextChanged -= null; // prevent recursion
                    textBox.Text = formatted;
                    textBox.SelectionStart = Math.Max(0, Math.Min(formatted.Length, oldSelStart + diff));
                }
            };
        }

        /// <summary>
        /// Format a number string with thousand separators for display
        /// </summary>
        public static string FormatNumberDisplay(string numberStr)
        {
            if (string.IsNullOrWhiteSpace(numberStr))
                return "0";

            // Remove existing commas
            string cleaned = numberStr.Replace(",", "").Replace(".", "");

            if (decimal.TryParse(cleaned, out decimal value))
            {
                return value.ToString("N0", new CultureInfo("en-US"));
            }

            return numberStr;
        }

        /// <summary>
        /// Format a decimal number for display
        /// </summary>
        public static string FormatNumberDisplay(decimal value)
        {
            return value.ToString("N0", new CultureInfo("en-US"));
        }

        /// <summary>
        /// Get pure number value from formatted textbox (remove commas)
        /// </summary>
        public static string GetPureNumber(TextBox textBox)
        {
            return textBox.Text.Replace(",", "").Replace(".", "");
        }
    }
}
