using System;
using Archivarius.JsonBackend;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public class Check_JsonBackend
    {
        [Test]
        public void Writer()
        {
            JsonWriter writer = new JsonWriter();
            Assert.That(writer.ToJsonString(), Is.EqualTo("[]"));
            
            writer.WriteBool(true);
            writer.WriteBool(false);
            Assert.That(writer.ToJsonString(), Is.EqualTo("[true,false]"));
            
            writer.WriteByte(123);
            Assert.That(writer.ToJsonString(), Is.EqualTo("[true,false,123]"));
            
            writer.WriteChar('a');
            writer.WriteChar('\n');
            Assert.That(writer.ToJsonString(), Is.EqualTo("[true,false,123,\"a\",\"\\n\"]"));
            
            writer.Clear();
            Assert.That(writer.ToJsonString(), Is.EqualTo("[]"));
            
            writer.WriteShort(short.MaxValue);
            Assert.That(writer.ToJsonString(), Is.EqualTo("[32767]"));
            
            writer.WriteInt(int.MinValue);
            Assert.That(writer.ToJsonString(), Is.EqualTo("[32767,-2147483648]"));
            
            writer.WriteLong(long.MaxValue);
            Assert.That(writer.ToJsonString(), Is.EqualTo("[32767,-2147483648,9223372036854775807]"));
            
            writer.Clear();
            writer.WriteFloat(0.1f);
            writer.WriteDouble(0.2);
            Assert.That(writer.ToJsonString(), Is.EqualTo("[0.1,0.2]"));
            
            writer.Clear();
            writer.WriteString(null);
            writer.WriteString("hello");
            writer.WriteString("");
            Assert.That(writer.ToJsonString(), Is.EqualTo("[null,\"hello\",\"\"]"));
            
            writer.Clear();
            writer.WriteInt(7);
            {
                writer.BeginSection();
                writer.WriteBool(false);
                {
                    writer.BeginSection();
                    {
                        writer.BeginSection();
                        writer.EndSection();
                    }
                    writer.WriteFloat(0.5f);
                    writer.EndSection();
                }
                writer.EndSection();
            }
            Assert.That(writer.ToJsonString(), Is.EqualTo("[7,[false,[[],0.5]]]"));
        }
        
        [Test]
        public void Reader()
        {
            {
                JsonReader reader = new JsonReader("[]");
                Assert.Throws<InvalidOperationException>(() => reader.BeginSection());
            }
            {
                JsonReader reader = new JsonReader("[]");
                Assert.Throws<InvalidOperationException>(() => reader.ReadByte());
            }
            
            {
                JsonReader reader = new JsonReader("[1]");
                Assert.That(reader.ReadByte(), Is.EqualTo(1));
            }
            {
                JsonReader reader = new JsonReader("[true]");
                Assert.Throws<InvalidOperationException>(() => reader.ReadByte());
                Assert.That(reader.ReadBool(), Is.EqualTo(true));
                Assert.Throws<InvalidOperationException>(() => reader.ReadBool());
            }
            {
                JsonReader reader = new JsonReader("[[]]");
                Assert.Throws<InvalidOperationException>(() => reader.ReadByte());
                Assert.DoesNotThrow(() => reader.BeginSection());
                Assert.That(reader.EndSection(), Is.EqualTo(true));
            }
            {
                JsonReader reader = new JsonReader("[[1],2]");
                Assert.DoesNotThrow(() => reader.BeginSection());
                Assert.That(reader.ReadInt(), Is.EqualTo(1));
                Assert.That(reader.EndSection(), Is.EqualTo(true));
                Assert.That(reader.ReadInt(), Is.EqualTo(2));
            }
        }
    }
}