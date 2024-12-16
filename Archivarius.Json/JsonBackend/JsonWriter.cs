using System.Text.Json;
using System.Text.Json.Nodes;

namespace Archivarius.JsonBackend
{
    public class JsonWriter : IWriter
    {
        private JsonArray _root;
        private JsonArray _currentSection;

        public JsonWriter()
        {
            _currentSection = new JsonArray();
            _root = _currentSection;
        }

        public override string ToString()
        {
            return ToJsonString();
        }

        public string ToJsonString()
        {
            return _root.ToJsonString(JsonSerializerOptions.Default);
        }

        public JsonArray UnsafeShowJson()
        {
            return _root;
        }

        public void Clear()
        {
            _currentSection = new JsonArray();
            _root = _currentSection;
        }
        
        public void WriteBool(bool value)
        {
            _currentSection.Add(value);
        }

        public void WriteByte(byte value)
        {
            _currentSection.Add(value);
        }

        public void WriteChar(char value)
        {
            _currentSection.Add(value);
        }

        public void WriteShort(short value)
        {
            _currentSection.Add(value);
        }

        public void WriteInt(int value)
        {
            _currentSection.Add(value);
        }

        public void WriteLong(long value)
        {
            _currentSection.Add(value);
        }

        public void WriteFloat(float value)
        {
            _currentSection.Add(value);
        }

        public void WriteDouble(double value)
        {
            _currentSection.Add(value);
        }

        public void WriteDecimal(decimal value)
        {
            _currentSection.Add(value);
        }

        public void WriteString(string? value)
        {
            _currentSection.Add(value);
        }

        public void WriteBytes(byte[]? value)
        {
            if (value == null)
            {
                _currentSection.Add(null);
                return;
            }

            var base64 = Convert.ToBase64String(value);
            _currentSection.Add(base64);
        }

        public void BeginSection()
        {
            var section = new JsonArray();
            _currentSection.Add(section);
            _currentSection = section;
        }

        public void EndSection()
        {
            _currentSection = (JsonArray)_currentSection.Parent!;
        }
    }
}