using System.Collections.Generic;

namespace Archivarius
{
    public static class PrimitiveCollectionsSerializer
    {
        /// <summary>
        /// Позволяет сериализовать
        /// List<T>,
        /// T[],
        /// Queue<T>
        /// Stack<T>
        /// HashSet<T>
        /// IReadOnlyCollection<T>
        /// В случае десериализации, если контейнер != null и допускает свою очистку и перезаполнение, то он переиспользуется
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TSerializerExtension"></typeparam>
        /// <typeparam name="TList"></typeparam>
        public static void AddCollection<T, TList>(this ISerializer serializer, ref TList list)
            where TList : class, IReadOnlyCollection<T>
        {
            serializer.AddDynamic(ref list);
        }

        public static void AddStructCollection<T, TList>(this ISerializer serializer, ref TList list)
            where T : struct, IDataStruct
            where TList : class, IReadOnlyCollection<T>
        {
            serializer.AddDynamic(ref list);
        }

        public static void AddClassCollection<T, TList>(this ISerializer serializer, ref TList list)
            where T : class, IDataStruct
            where TList : class, IReadOnlyCollection<T>
        {
            serializer.AddDynamic(ref list);
        }
    }
}