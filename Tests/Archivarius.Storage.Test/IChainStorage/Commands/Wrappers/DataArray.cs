using System;
using System.Collections.Generic;
using System.Linq;

namespace Archivarius.Storage.Test.ChainStorage
{
    public struct DataArray<TData> : IEquatable<DataArray<TData>>
    {
        public readonly List<TData> Data = new();

        public DataArray()
        {
        }

        public bool Equals(DataArray<TData> other)
        {
            return Data.SequenceEqual(other.Data);
        }

        public override bool Equals(object? obj)
        {
            return obj is DataArray<TData> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        public static bool operator ==(DataArray<TData> left, DataArray<TData> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DataArray<TData> left, DataArray<TData> right)
        {
            return !left.Equals(right);
        }
    }
}