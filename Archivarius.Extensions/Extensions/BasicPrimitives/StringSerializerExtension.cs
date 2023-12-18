﻿namespace Archivarius
{
    public class StringSerializerExtension : ISerializerExtension<string?>
    {
        public void Add(ISerializer serializer, ref string? value)
        {
            serializer.Add(ref value);
        }
    }
}