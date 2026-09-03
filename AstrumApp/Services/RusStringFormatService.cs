using System;
using System.Collections.Generic;
using System.Text;

namespace AstrumApp.Services
{
    public class RusStringFormatService
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
    }
}
