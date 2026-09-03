using System;
using System.Collections.Generic;
using System.Text;
using AstrumApp.Interfaces;

namespace AstrumApp.Security.Models
{
    internal class SecurityData : IPinData
    {
        public string PinHash { get; set; } = String.Empty;

        public byte[] Salt { get; set; } = Array.Empty<byte>();

        public DateTime PinChanged { get; set; }

        public int Iterations { get; set; } = 300_000;
    }
}
