using System;
using System.Collections.Generic;
using System.Text;

namespace AstrumApp.Interfaces
{
    public interface IPinData
    {
        string PinHash { get; set; }
        byte[] Salt { get; set; }
        int Iterations { get; set; }
        DateTime PinChanged { get; set; }
    }
}
