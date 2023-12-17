namespace Archivarius
{
    public enum SerializerExtensionsFactoryMode
    {
        /// <summary>
        /// Отсутсвует поддержка AOT компиляции. Режим подходит для серверов
        /// </summary>
        JITMode,

        /// <summary>
        /// Режим 100% совместимый с AOT компиляцией. Любой небезопасный функционал отсутсвует
        /// </summary>
        AOTSafeMode,

        /// <summary>
        /// AOT поддерживается, весь функционал работает, но необходимы дополнительные действия,
        /// при невыполнении которых возможны креши в рантайме.
        /// </summary>
        AOTUnsafeMode
    }
}