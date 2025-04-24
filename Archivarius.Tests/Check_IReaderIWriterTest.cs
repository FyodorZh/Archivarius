using System;
using System.Collections.Generic;
using System.IO;
using Archivarius.JsonBackend;
using Archivarius.StructuredBinaryBackend;
using NUnit.Framework;

namespace Archivarius.Tests
{
    [TestFixture]
    public class Check_IReaderIWriterTest
    {
        [Test]
        public void Test_Binary()
        {
            Check(GetList(), () => new BinaryBackend.BinaryWriter(), w =>
            {
                BinaryBackend.BinaryWriter bw = (BinaryBackend.BinaryWriter)w;
                var buffer = bw.GetBuffer();
                return new BinaryBackend.BinaryReader(buffer);
            });
        }
        
        [Test]
        public void Test_BinaryStream()
        {
            Check(GetList(), () => new BinaryBackend.BinaryWriter(), w =>
            {
                BinaryBackend.BinaryWriter bw = (BinaryBackend.BinaryWriter)w;
                var buffer = bw.GetBuffer();
                return new BinaryBackend.BinaryStreamReader(new MemoryStream(buffer));
            });
        }

        [Test]
        public void Test_StructuredBinary()
        {
            Check(GetList(), () => new StructuredBinaryWriter(), w =>
            {
                StructuredBinaryWriter bw = (StructuredBinaryWriter)w;
                return new StructuredBinaryReader(bw.ExtractData());
            });
        }
        
        [Test]
        public void Test_Json()
        {
            Check(GetList(), () => new JsonWriter(), w =>
            {
                JsonWriter writer = (JsonWriter)w;
                return new JsonReader(writer.ToJsonString());
            });
        }

        private IReadOnlyCollection<object?> GetList()
        {
            return new object?[]
            {
                true,
                false,
                byte.MinValue,
                byte.MaxValue,
                sbyte.MinValue,
                sbyte.MaxValue,
                'x',
                'z',
                '\0',
                short.MinValue,
                short.MaxValue,
                ushort.MinValue,
                ushort.MaxValue,
                int.MinValue,
                int.MaxValue,
                uint.MinValue,
                uint.MaxValue,
                long.MinValue,
                long.MaxValue,
                ulong.MinValue,
                ulong.MaxValue,
                0.25f,
                0.5,
                decimal.MinValue,
                decimal.MaxValue,
                "hello",
                "world",
                null,
                "",
                null,
                new byte[]{},
                new byte[]{1},
                new byte[]{200,201,202}
            };
        }

        private void Check(IReadOnlyCollection<object?> objects, Func<IWriter> getWriter, Func<IWriter, IReader> getReader)
        {
            IWriter writer = getWriter();

            foreach (var obj in objects)
            {
                switch (obj)
                {
                    case bool boolValue:
                        writer.WriteBool(boolValue);
                        break;
                    case byte byteValue:
                        writer.WriteByte(byteValue);
                        break;
                    case sbyte sbyteValue:
                        writer.WriteSByte(sbyteValue);
                        break;
                    case char charValue:
                        writer.WriteChar(charValue);
                        break;
                    case short shortValue:
                        writer.WriteShort(shortValue);
                        break;
                    case ushort ushortValue:
                        writer.WriteUShort(ushortValue);
                        break;
                    case int intValue:
                        writer.WriteInt(intValue);
                        break;
                    case uint uintValue:
                        writer.WriteUInt(uintValue);
                        break;
                    case long longValue:
                        writer.WriteLong(longValue);
                        break;
                    case ulong ulongValue:
                        writer.WriteULong(ulongValue);
                        break;
                    case float floatValue:
                        writer.WriteFloat(floatValue);
                        break;
                    case double doubleValue:
                        writer.WriteDouble(doubleValue);
                        break;
                    case decimal decimalValue:
                        writer.WriteDecimal(decimalValue);
                        break;
                    case string stringValue:
                        writer.WriteString(stringValue);
                        break;
                    case byte[] arrayValue:
                        writer.WriteBytes(arrayValue);
                        break;
                    case null:
                        writer.WriteString(null);
                        break;
                    default:
                        Assert.Fail("Unknown type");
                        break;
                }
            }

            IReader reader = getReader(writer);

            foreach (var obj in objects)
            {
                switch (obj)
                {
                    case bool boolValue:
                        Assert.That(reader.ReadBool(), Is.EqualTo(boolValue));
                        break;
                    case byte byteValue:
                        Assert.That(reader.ReadByte(), Is.EqualTo(byteValue));
                        break;
                    case sbyte sbyteValue:
                        Assert.That(reader.ReadSByte(), Is.EqualTo(sbyteValue));
                        break;
                    case char charValue:
                        Assert.That(reader.ReadChar(), Is.EqualTo(charValue));
                        break;
                    case short shortValue:
                        Assert.That(reader.ReadShort(), Is.EqualTo(shortValue));
                        break;
                    case ushort ushortValue:
                        Assert.That(reader.ReadUShort(), Is.EqualTo(ushortValue));
                        break;
                    case int intValue:
                        Assert.That(reader.ReadInt(), Is.EqualTo(intValue));
                        break;
                    case uint uintValue:
                        Assert.That(reader.ReadUInt(), Is.EqualTo(uintValue));
                        break;
                    case long longValue:
                        Assert.That(reader.ReadLong(), Is.EqualTo(longValue));
                        break;
                    case ulong ulongValue:
                        Assert.That(reader.ReadULong(), Is.EqualTo(ulongValue));
                        break;
                    case float floatValue:
                        Assert.That(reader.ReadFloat(), Is.EqualTo(floatValue));
                        break;
                    case double doubleValue:
                        Assert.That(reader.ReadDouble(), Is.EqualTo(doubleValue));
                        break;
                    case decimal decimalValue:
                        Assert.That(reader.ReadDecimal(), Is.EqualTo(decimalValue));
                        break;
                    case string stringValue:
                        Assert.That(reader.ReadString(), Is.EqualTo(stringValue));
                        break;
                    case byte[] arrayValue:
                        Assert.That(reader.ReadBytes(), Is.EqualTo(arrayValue));
                        break;
                    case null:
                        Assert.That(reader.ReadString(), Is.EqualTo(null));
                        break;
                    default:
                        Assert.Fail("Unknown type");
                        break;
                }
            }
        }
    }
}