using System.Collections.Generic;
using DSharpPlus.Entities;

// Extracted from a version of HSNXT.DSharpPlus.Extended.Lite, originally at uwx/HSNXT.DSharpPlus at March 10th, 2019.

namespace HSNXT.DSharpPlus.Extended.ExtensionMethods
{
    public readonly struct SnowflakeEqualityComparer : IEqualityComparer<SnowflakeObject>
    {
        public bool Equals(SnowflakeObject x, SnowflakeObject y) => x.Id == y.Id;

        public int GetHashCode(SnowflakeObject obj) => obj.Id.GetHashCode();
    }
}
