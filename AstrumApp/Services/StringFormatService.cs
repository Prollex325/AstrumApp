using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace AstrumApp.Services
{
    public class StringFormatService
    {
        public string Format(int value, string form1, string form2, string form3)
        {
            int lastTwoDigits = value % 100;
            int start = lastTwoDigits / 10;
            int ending = lastTwoDigits % 10;

            if (ending == 1 && start != 1)
            {
                return form1;
            }
            else if (ending >= 2 && ending <= 4 && start != 1)
            {
                return form2;
            }
            else
            {
                return form3;
            }
        }

        public static string FormatTimestamp(long timestamp, string format = "dd.MM.yyyy HH:mm")
        {
            return DateTimeOffset
                .FromUnixTimeMilliseconds(timestamp)
                .ToLocalTime()
                .ToString(format);
        }

        public static bool IsAllowedEmailChar(char c)
        {
            return char.IsAsciiLetterOrDigit(c) ||
                   c is '@' or '.' or '_' or '-' or '+' or '%';
        }

        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > 254)
                return false;

            bool firstCheck = false;
            try
            {
                var address = new MailAddress(email);
                firstCheck = address.Address == email;
            }
            catch
            {
                firstCheck = false;
            }

            return Regex.IsMatch(
                email,
                @"^[A-Za-z0-9](?:[A-Za-z0-9._%+-]*[A-Za-z0-9])?@[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?(?:\.[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?)*\.[A-Za-z]{2,}$"
            ) && !email.Split('@')[0].Contains("..") && firstCheck;
        }

        public static bool IsValidUsername(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length > 16)
                return false;

            return !username.Any(char.IsControl);
        }

        public static bool IsValidBio(string? bio)
        {
            if (bio == null || bio.Length > 160)
                return false;

            return !bio.Any(c =>
                char.IsControl(c) && c != '\n' && c != '\r');
        }
    }
}
