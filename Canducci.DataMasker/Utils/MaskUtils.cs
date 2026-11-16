
using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace Canducci.DataMasker.Utils
{
    internal static class MaskUtils
    {
        private static bool IsNotValue(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        internal static string MaskValue(string value, MaskAttribute attribute)
        {
            if (IsNotValue(value) || attribute == null)
            {
                return value;
            }

            switch (attribute.Strategy)
            {
                case MaskStrategy.Full:
                    return Full(value);

                case MaskStrategy.Partial:
                    return MaskPartial(value, attribute.Type);

                case MaskStrategy.FirstCharacters:
                    return FirstCharacters(value);

                case MaskStrategy.LastCharacters:
                    return LastCharacters(value);

                case MaskStrategy.Custom:
                    return Custom(value, attribute);

                default:
                    return value;
            }
        }

        private static string Custom(string value, MaskAttribute attribute)
        {
            if (attribute.CustomType == null || string.IsNullOrWhiteSpace(attribute.CustomMethod))
            {
                throw new InvalidOperationException("Custom mask configuration is invalid.");
            }

            var method = attribute.CustomType.GetMethod(
                attribute.CustomMethod,
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic,
                null,
                new[] { typeof(string) },
                null);

            if (method == null ||
                method.ReturnType != typeof(string) ||
                method.ContainsGenericParameters)
            {
                throw new InvalidOperationException(
                    $"Custom method '{attribute.CustomMethod}' was not found or has an invalid signature.");
            }

            return (string)method.Invoke(null, new object[] { value });
        }

        private static string MaskPartial(string value, MaskType type)
        {
            switch (type)
            {
                case MaskType.None:
                case MaskType.Custom:
                    return value;

                case MaskType.Name:
                    return Name(value);

                case MaskType.Cpf:
                    return Cpf(value);

                case MaskType.Cnpj:
                    return Cnpj(value);

                case MaskType.Rg:
                    return Rg(value);

                case MaskType.Email:
                    return Email(value);

                case MaskType.Phone:
                    return Phone(value);

                case MaskType.Mobile:
                    return Mobile(value);

                case MaskType.Cep:
                    return Cep(value);

                case MaskType.Address:
                    return Address(value);

                case MaskType.BirthDate:
                    return BirthDate(value);

                case MaskType.IpAddress:
                    return IpAddress(value);

                case MaskType.CreditCard:
                    return CreditCard(value);

                case MaskType.BankAccount:
                    return BankAccount(value);

                case MaskType.Pix:
                    return Pix(value);

                case MaskType.Salary:
                case MaskType.Health:
                case MaskType.Biometric:
                case MaskType.Password:
                case MaskType.Token:
                    return Full(value);

                default:
                    return value;
            }
        }

        internal static string Full(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            return new string('*', value.Length);
        }

        internal static string FirstCharacters(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            return new string('*', value.Length - 1) + value.Substring(value.Length - 1);
        }

        internal static string LastCharacters(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            return value.Length <= 1
                ? "*"
                : value.Substring(0, 1) + new string('*', value.Length - 1);
        }

        internal static string Name(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            return string.Join(" ", value.Split(' ').Select(word =>
            {
                if (string.IsNullOrEmpty(word))
                {
                    return word;
                }

                return word.Substring(0, 1) + new string('*', word.Length - 1);
            }));
        }

        internal static string Cpf(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            var digits = GetDigits(value);

            if (digits.Length != 11)
            {
                return value;
            }

            return string.Format(
                "{0}.***.***-{1}",
                digits.Substring(0, 3),
                digits.Substring(9, 2));
        }

        internal static string Cnpj(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            var digits = GetDigits(value);

            if (digits.Length != 14)
            {
                return value;
            }

            return string.Format(
                "{0}.***/****-{1}",
                digits.Substring(0, 2),
                digits.Substring(12, 2));
        }

        internal static string Rg(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            if (value.Length <= 3)
            {
                return value;
            }

            return new string('*', value.Length - 3)
                + value.Substring(value.Length - 3);
        }

        internal static string Email(string value)
        {
            if (!IsValidEmail(value))
            {
                return value;
            }

            var index = value.IndexOf('@');

            return value.Substring(0, 1)
                + "***"
                + value.Substring(index);
        }

        internal static string Phone(string value)
        {
            return MaskFormattedDigits(value, 4, 10);
        }

        internal static string Mobile(string value)
        {
            return MaskFormattedDigits(value, 4, 11);
        }

        internal static string Cep(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            var digits = GetDigits(value);

            if (digits.Length != 8)
            {
                return value;
            }

            return string.Format("{0}-***", digits.Substring(0, 5));
        }

        internal static string Address(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            return string.Join(" ", value.Split(' ').Select(word =>
            {
                if (string.IsNullOrEmpty(word))
                {
                    return word;
                }

                var result = word.Select(character =>
                {
                    if (char.IsDigit(character) || char.IsLetter(character))
                    {
                        return '*';
                    }

                    return character;
                }).ToArray();

                var firstLetter = Array.FindIndex(word.ToCharArray(), char.IsLetter);

                if (firstLetter >= 0)
                {
                    result[firstLetter] = word[firstLetter];
                }

                return new string(result);
            }));
        }

        internal static string BirthDate(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            if (!DateTime.TryParseExact(
                value,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
            {
                return value;
            }

            return string.Format("**/**/{0:yyyy}", date);
        }

        internal static string IpAddress(string value)
        {
            if (IsNotValue(value) || !IPAddress.TryParse(value, out var address))
            {
                return value;
            }

            var bytes = address.GetAddressBytes();

            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.***.***.***",
                    bytes[0]);
            }

            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var firstGroup = ((bytes[0] << 8) | bytes[1]).ToString("x", CultureInfo.InvariantCulture);
                return firstGroup + string.Concat(Enumerable.Repeat(":****", 7));
            }

            return value;
        }

        internal static string CreditCard(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            var digits = GetDigits(value);

            if (digits.Length < 15 || digits.Length > 19)
            {
                return value;
            }

            var digitsToMask = digits.Length - 4;
            var maskedDigits = 0;
            var result = new StringBuilder(value.Length);

            foreach (var character in value)
            {
                if (!char.IsDigit(character))
                {
                    result.Append(character);
                    continue;
                }

                if (maskedDigits < digitsToMask)
                {
                    result.Append('*');
                    maskedDigits++;
                    continue;
                }

                result.Append(character);
            }

            return result.ToString();
        }

        internal static string BankAccount(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            return new string(value.Select(character =>
                char.IsDigit(character) ? '*' : character).ToArray());
        }

        internal static string Pix(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            if (value.Contains("@"))
            {
                return PixEmail(value);
            }

            return LastCharacters(value);
        }

        private static string PixEmail(string value)
        {
            if (!IsValidEmail(value))
            {
                return value;
            }

            var parts = value.Split('@');
            var domain = parts[1].Split('.');

            return parts[0].Substring(0, 1)
                + new string('*', parts[0].Length - 1)
                + "@"
                + domain[0].Substring(0, 1)
                + new string('*', domain[0].Length - 1)
                + "."
                + string.Join(".", domain.Skip(1));
        }


        private static string MaskFormattedDigits(
            string value,
            int visible,
            int expectedDigits)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            var digits = GetDigits(value);
            var hasBrazilianCountryCode = digits.Length == expectedDigits + 2 &&
                digits.StartsWith("55", StringComparison.Ordinal);

            if (digits.Length != expectedDigits && !hasBrazilianCountryCode)
            {
                return value;
            }

            var nationalDigits = hasBrazilianCountryCode
                ? digits.Substring(2)
                : digits;

            if (nationalDigits.Length != expectedDigits)
            {
                return value;
            }

            var areaCodeDigits = 2;
            var countryCodeDigits = hasBrazilianCountryCode ? 2 : 0;
            var firstMaskedDigit = countryCodeDigits + areaCodeDigits;
            var lastVisibleDigit = digits.Length - visible;
            var result = new StringBuilder(value.Length);
            var digitIndex = 0;

            foreach (var character in value)
            {
                if (!char.IsDigit(character))
                {
                    result.Append(character);
                    continue;
                }

                var isCountryCodeOrAreaCode = digitIndex < firstMaskedDigit;
                var isLastVisibleDigit = digitIndex >= lastVisibleDigit;

                if (!isCountryCodeOrAreaCode && !isLastVisibleDigit)
                {
                    result.Append('*');
                }
                else
                {
                    result.Append(character);
                }

                digitIndex++;
            }

            return result.ToString();
        }


        private static bool IsValidEmail(string value)
        {
            if (IsNotValue(value) || value.Any(char.IsWhiteSpace))
            {
                return false;
            }

            try
            {
                var address = new MailAddress(value);

                return string.Equals(
                    address.Address,
                    value,
                    StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(address.User)
                    && address.Host.Contains(".");
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string GetDigits(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            return new string(value.Where(char.IsDigit).ToArray());
        }
    }
}
