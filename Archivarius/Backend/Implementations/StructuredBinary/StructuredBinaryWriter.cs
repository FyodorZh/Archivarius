using System;
using System.Collections.Generic;

namespace Archivarius.StructuredBinaryBackend
{
    public class StructuredBinaryWriter : IWriter
    {
        private List<Record> _section = new List<Record>();
        private readonly Stack<List<Record>> _stack = new Stack<List<Record>>();

        public void WriteTo(IWriter writer)
        {
            if (_stack.Count != 0)
            {
                throw new InvalidOperationException();
            }

            writer.WriteInt(_section.Count);
            foreach (var record in _section)
            {
                record.WriteTo(writer);
            }
        }

        public StructuredData ExtractData()
        {
            var data = ShowData();
            _section = new List<Record>();
            return data;
        }

        public StructuredData ShowData()
        {
            if (_stack.Count != 0)
            {
                throw new InvalidOperationException();
            }

            StructuredData sd = new StructuredData(new Record(_section));
            return sd;
        }

        public void Clear()
        {
            _section.Clear();
            _stack.Clear();
        }

        public void BeginSection()
        {
            _stack.Push(_section);
            _section = new List<Record>();
        }

        public void EndSection()
        {
            Record r = new Record(_section);
            _section = _stack.Pop();
            _section.Add(r);
        }

        public void WriteBool(bool value)
        {
            WriteByte(value ? (byte)1 : (byte)0);
        }
        
        public void WriteByte(byte value)
        {
            _section.Add(new Record(value));
        }

        public void WriteChar(char value)
        {
            _section.Add(new Record(value));
        }

        public void WriteShort(short value)
        {
            _section.Add(new Record(value));
        }

        public void WriteInt(int value)
        {
            _section.Add(new Record(value));
        }

        public void WriteLong(long value)
        {
            _section.Add(new Record(value));
        }

        public void WriteFloat(float value)
        {
            _section.Add(new Record(value));
        }

        public void WriteDouble(double value)
        {
            _section.Add(new Record(value));
        }
        
        public void WriteDecimal(decimal value)
        {
            _section.Add(new Record(value));
        }

        public void WriteString(string? value)
        {
            _section.Add(new Record(value));
        }

        public void WriteBytes(byte[]? value)
        {
            _section.Add(new Record(value));
        }
    }
}