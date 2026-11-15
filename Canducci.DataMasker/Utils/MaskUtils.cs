
using Canducci.DataMasker.Attributes;
using Canducci.DataMasker.Enums;
using System.Linq;

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
                    return value;

                default:
                    return value;
            }
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
                string.IsNullOrEmpty(word)
                    ? word
                    : word.Substring(0, 1) + new string('*', word.Length - 1)));
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

            return value.Substring(0, value.Length - 3).Length > 0
                ? new string('*', value.Length - 3) + value.Substring(value.Length - 3)
                : value;
        }

        internal static string Email(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            var index = value.IndexOf('@');

            if (index <= 0 || index == value.Length - 1)
            {
                return value;
            }

            return value.Substring(0, 1) + "***" + value.Substring(index);
        }

        internal static string Phone(string value)
        {
            return MaskFormattedDigits(value, 4);
        }

        internal static string Mobile(string value)
        {
            return MaskFormattedDigits(value, 4);
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
                    if (char.IsDigit(character))
                    {
                        return '*';
                    }

                    return char.IsLetter(character)
                        ? '*'
                        : character;
                }).ToArray();

                var firstLetter = word.IndexOfAny(
                    word.Where(char.IsLetter).ToArray());

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

            var parts = value.Split('/');

            if (parts.Length != 3 || parts[2].Length != 4)
            {
                return value;
            }

            return "**/**/" + parts[2];
        }

        internal static string IpAddress(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            var parts = value.Split('.');

            if (parts.Length != 4)
            {
                return value;
            }

            foreach (var part in parts)
            {
                int octet;

                if (!int.TryParse(part, out octet) || octet < 0 || octet > 255)
                {
                    return value;
                }
            }

            return string.Format("{0}.***.***.***", parts[0]);
        }

        internal static string CreditCard(string value)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            var digits = GetDigits(value);

            if (digits.Length != 16)
            {
                return value;
            }

            return string.Format(
                "{0} **** **** {1}",
                digits.Substring(0, 4),
                digits.Substring(12, 4));
        }

        internal static string BankAccount(string value)
        {
            return LastCharacters(value);
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
            var parts = value.Split('@');

            if (parts.Length != 2 || parts[0].Length == 0)
            {
                return value;
            }

            var domain = parts[1].Split('.');

            if (domain.Length < 2 || domain[0].Length == 0)
            {
                return value;
            }

            return parts[0].Substring(0, 1)
                + new string('*', parts[0].Length - 1)
                + "@"
                + domain[0].Substring(0, 1)
                + new string('*', domain[0].Length - 1)
                + "."
                + string.Join(".", domain.Skip(1));
        }

        private static string MaskFormattedDigits(string value, int visible)
        {
            if (IsNotValue(value))
            {
                return value;
            }

            var digits = GetDigits(value);

            if (digits.Length <= visible)
            {
                return value;
            }

            var remaining = digits.Length - visible;
            var result = new System.Text.StringBuilder();
            var masked = 0;

            foreach (var character in value)
            {
                if (!char.IsDigit(character))
                {
                    result.Append(character);
                    continue;
                }

                if (masked < remaining)
                {
                    result.Append('*');
                    masked++;
                }
                else
                {
                    result.Append(character);
                }
            }

            return result.ToString();
        }

        private static string GetDigits(string value)
        {
            return new string(value.Where(char.IsDigit).ToArray());
        }
    }
}
