namespace Archivarius
{
    public class GraphSerializer : HierarchicalSerializer
    {
        private readonly Dictionary<object, int> _instanceMap = new Dictionary<object, int>(new ReferenceComparer());

        public GraphSerializer(IWriter writer, ITypeSerializer typeSerializer, ISerializerExtensionsFactory? factory = null)
            : base(writer, typeSerializer, factory)
        {
        }

        protected override void SerializeClass(IDataStruct value)
        {
            bool needToSerialize = false;
            if (!_instanceMap.TryGetValue(value, out int instanceId))
            {
                instanceId = _instanceMap.Count;
                _instanceMap.Add(value, instanceId);
                needToSerialize = true;
            }

            _writer.WriteInt(instanceId);

            if (needToSerialize)
            {
                base.SerializeClass(value);
            }
        }

        private class ReferenceComparer : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals(object? x, object? y)
            {
                return ReferenceEquals(x, y);
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                int hash = obj.GetHashCode();
                return hash;
            }
        }
    }
}