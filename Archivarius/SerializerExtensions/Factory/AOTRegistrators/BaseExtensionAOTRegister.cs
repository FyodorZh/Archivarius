namespace Archivarius
{
    public abstract class BaseExtensionAOTRegister : IExtensionsAOTRegister
    {
        public abstract void RegisterTypes();

        protected void RegisterKeyValuePair<TKey, TValue>()
            where TKey : notnull
        {
            AOTGuard.Instance.RegisterType(new DictionarySerializerExtension<TKey, TValue>(null!, null!));
        }

        protected void RegisterDataStruct<T>()
            where T : struct, IDataStruct
        {
            AOTGuard.Instance.RegisterType(new DataStructSerializerExtension<T>());
            RegisterStruct<T>();
        }

        protected void RegisterStruct<T>()
            where T: struct
        {
            AOTGuard.Instance.RegisterType(new ArraySerializerExtension<T>(null!));
            AOTGuard.Instance.RegisterType(new ListSerializerExtension<T>(null!));
            AOTGuard.Instance.RegisterType(new ListSerializerExtension<T>(null!));
            AOTGuard.Instance.RegisterType(new QueueSerializerExtension<T>(null!));
            AOTGuard.Instance.RegisterType(new StackSerializerExtension<T>(null!));
            AOTGuard.Instance.RegisterType(new HashSetSerializerExtension<T>(null!));
        }
    }
}