using System.Text.Json.Nodes;

namespace Archivarius.JsonBackend
{
    public class JsonReader : IReader
    {
        private readonly JsonArray _root;
        private JsonArray _currentSection;
        
        private readonly Stack<int> _cursorStack = new Stack<int>();
        private readonly Stack<JsonArray> _sectionStack = new Stack<JsonArray>();
        
        private int _cursor;

        public JsonReader(string json)
            : this((JsonArray)JsonNode.Parse(json)!)
        {
        }

        public JsonReader(JsonArray json)
        {
            _root = json;
            _currentSection = json;
            _cursor = 0;
        }

        public void Reset()
        {
            _cursorStack.Clear();
            _sectionStack.Clear();
            _currentSection = _root;
            _cursor = 0;
        }

        private void CheckGrow()
        {
            if (_cursor >= _currentSection.Count)
            {
                throw new InvalidOperationException();
            }
        }
        
        public bool ReadBool()
        {
            CheckGrow();
            var res = _currentSection[_cursor]!.GetValue<bool>();
            _cursor += 1;
            return res;
        }

        public byte ReadByte()
        {
            CheckGrow();
            var res = _currentSection[_cursor]!.GetValue<byte>();
            _cursor += 1;
            return res;
        }

        public char ReadChar()
        {
            CheckGrow();
            var res = _currentSection[_cursor]!.GetValue<char>();
            _cursor += 1;
            return res;
        }

        public short ReadShort()
        {
            CheckGrow();
            var res = _currentSection[_cursor]!.GetValue<short>();
            _cursor += 1;
            return res;
        }

        public int ReadInt()
        {
            CheckGrow();
            var res = _currentSection[_cursor]!.GetValue<int>();
            _cursor += 1;
            return res;
        }

        public long ReadLong()
        {
            CheckGrow();
            var res = _currentSection[_cursor]!.GetValue<long>();
            _cursor += 1;
            return res;
        }

        public float ReadFloat()
        {
            CheckGrow();
            var res = _currentSection[_cursor]!.GetValue<float>();
            _cursor += 1;
            return res;
        }

        public double ReadDouble()
        {
            CheckGrow();
            var res = _currentSection[_cursor]!.GetValue<double>();
            _cursor += 1;
            return res;
        }

        public string? ReadString()
        {
            CheckGrow();
            var element = _currentSection[_cursor];
            if (element == null)
            {
                _cursor += 1;
                return null;
            }
            var res = element.GetValue<string>();
            _cursor += 1;
            return res;
        }
        
        public byte[]? ReadBytes()
        {
            CheckGrow();
            var element = _currentSection[_cursor];
            if (element == null)
            {
                _cursor += 1;
                return null;
            }
            var base64 = element.GetValue<string>();
            var res = Convert.FromBase64String(base64);
            _cursor += 1;
            return res;
        }

        public void BeginSection()
        {
            CheckGrow();
            _cursorStack.Push(_cursor);
            _sectionStack.Push(_currentSection);
            _currentSection = _currentSection[_cursor]?.AsArray() ?? throw new InvalidOperationException();
            _cursor = 0;
        }

        public bool EndSection()
        {
            bool result = _cursor == _currentSection.Count;
            
            _cursor = _cursorStack.Pop() + 1;
            _currentSection = _sectionStack.Pop();
            
            return result;
        }
    }
}