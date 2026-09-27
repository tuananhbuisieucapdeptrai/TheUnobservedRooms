using System.Collections.Generic;
using System.Linq;

namespace UnobservedRooms.Data
{
    public sealed class ValidationResult
    {
        public readonly List<string> Errors = new();
        public readonly List<string> Warnings = new();
        public bool IsValid => Errors.Count == 0;
        public string Summary => IsValid ? "Valid" : string.Join("; ", Errors.Take(5));

        public void Error(string message) => Errors.Add(message);
        public void Warn(string message) => Warnings.Add(message);
    }
}
