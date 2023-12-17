namespace Archivarius.StructuredBinaryBackend
{
    public class StructuredData
    {
        private readonly Record _root;

        public Record Data => _root;

        public StructuredData(Record root)
        {
            _root = root;
        }
    }
}