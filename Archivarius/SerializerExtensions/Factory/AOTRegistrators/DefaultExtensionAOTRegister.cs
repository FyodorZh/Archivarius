namespace Archivarius
{
    internal class DefaultExtensionAOTRegister // : IExtensionsAOTRegister
    {
        public void RegisterTypes()
        {
            RegisterTypeInContainers<object>();
            RegisterTypeInContainers<bool>();
            RegisterTypeInContainers<byte>();
            RegisterTypeInContainers<sbyte>();
            RegisterTypeInContainers<char>();
            RegisterTypeInContainers<short>();
            RegisterTypeInContainers<ushort>();
            RegisterTypeInContainers<int>();
            RegisterTypeInContainers<uint>();
            RegisterTypeInContainers<long>();
            RegisterTypeInContainers<ulong>();
            RegisterTypeInContainers<float>();
            RegisterTypeInContainers<double>();
            RegisterTypeInContainers<Guid>();
            RegisterTypeInContainers<DateTime>();

            RegisterTypesInDictionary<Object, bool, byte, sbyte, char, short, ushort, int, uint, long, ulong, float, double, Guid, DateTime>();
        }

        private void RegisterTypeInContainers<T>()
        {
            AOTGuard.Instance.RegisterType(new ArraySerializerExtension<T>(null!));
            AOTGuard.Instance.RegisterType(new ListSerializerExtension<T>(null!));
            AOTGuard.Instance.RegisterType(new ListSerializerExtension<T>(null!));
            AOTGuard.Instance.RegisterType(new QueueSerializerExtension<T>(null!));
            AOTGuard.Instance.RegisterType(new StackSerializerExtension<T>(null!));
            AOTGuard.Instance.RegisterType(new HashSetSerializerExtension<T>(null!));
        }

        private void F<T1, T2>()
            where T1 : notnull
            where T2 : notnull
        {
            AOTGuard.Instance.RegisterType(new DictionarySerializerExtension<T1, T2>(null!, null!));
            AOTGuard.Instance.RegisterType(new DictionarySerializerExtension<T2, T1>(null!, null!));
        }

        private void F<TKey, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>()
            where TKey : notnull
            where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull
            where T5 : notnull
            where T6 : notnull
            where T7 : notnull
            where T8 : notnull
            where T9 : notnull
            where T10 : notnull
            where T11 : notnull
            where T12 : notnull
            where T13 : notnull
            where T14 : notnull
        {
            F<TKey, T1>();
            F<TKey, T2>();
            F<TKey, T3>();
            F<TKey, T4>();
            F<TKey, T5>();
            F<TKey, T6>();
            F<TKey, T7>();
            F<TKey, T8>();
            F<TKey, T9>();
            F<TKey, T10>();
            F<TKey, T11>();
            F<TKey, T12>();
            F<TKey, T13>();
            F<TKey, T14>();
        }

        private void RegisterTypesInDictionary<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>()
            where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull
            where T5 : notnull
            where T6 : notnull
            where T7 : notnull
            where T8 : notnull
            where T9 : notnull
            where T10 : notnull
            where T11 : notnull
            where T12 : notnull
            where T13 : notnull
            where T14 : notnull
            where T15 : notnull
        {
            F<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
            F<T2, T1, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
            F<T3, T2, T1, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
            F<T4, T2, T3, T1, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
            F<T5, T2, T3, T4, T1, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
            F<T6, T2, T3, T4, T5, T1, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
            F<T7, T2, T3, T4, T5, T6, T1, T8, T9, T10, T11, T12, T13, T14, T15>();
            F<T8, T2, T3, T4, T5, T6, T7, T1, T9, T10, T11, T12, T13, T14, T15>();
            F<T9, T2, T3, T4, T5, T6, T7, T8, T1, T10, T11, T12, T13, T14, T15>();
            F<T10, T2, T3, T4, T5, T6, T7, T8, T9, T1, T11, T12, T13, T14, T15>();
            F<T11, T2, T3, T4, T5, T6, T7, T8, T9, T10, T1, T12, T13, T14, T15>();
            F<T12, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T1, T13, T14, T15>();
            F<T13, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T1, T14, T15>();
            F<T14, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T1, T15>();
            F<T15, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T1>();
        }
    }
}