using System.Collections.Generic;

namespace Archivarius
{
    public class GraphDeserializer : HierarchicalDeserializer
    {
        private struct InstanceInfo
        {
            public object? Instance;
            public bool IsInitialized;
        }

        private readonly List<InstanceInfo> _instances = new List<InstanceInfo>();

        public GraphDeserializer(IReader reader, ITypeDeserializer typeDeserializer, ISerializerExtensionsFactory? factory = null)
            : base(reader, typeDeserializer, factory)
        {
        }

        protected override T? DeserializeClass<T>(IConstructor ctor)
            where T : class
        {
            int instanceId = _reader.ReadInt();
            while (instanceId >= _instances.Count)
            {
                _instances.Add(new InstanceInfo());
            }

            InstanceInfo instanceInfo = _instances[instanceId];
            if (instanceInfo.IsInitialized)
            {
                return instanceInfo.Instance as T;
            }
            else
            {
                var value = ctor.Construct() as T;

                instanceInfo.IsInitialized = true;
                instanceInfo.Instance = value;
                _instances[instanceId] = instanceInfo;

                if (value != null)
                {
                    DeserializeClass(value);
                }

                return value;
            }
        }
    }
}