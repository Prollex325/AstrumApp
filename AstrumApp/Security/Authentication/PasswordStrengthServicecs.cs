using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Zxcvbn;

namespace AstrumApp.Security.Authentication
{
    internal class PasswordStrengthServicecs
    {
        public void CheckPasswordStrength(string password)
        {
            var result = Core.EvaluatePassword(password);
            if (result.Score < 3)
            {
                
            }
        }
    }
}
