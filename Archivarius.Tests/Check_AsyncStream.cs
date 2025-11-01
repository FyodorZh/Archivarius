using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Archivarius.BinaryBackend;
using Archivarius.TypeSerializers;
using NUnit.Framework;
using BinaryWriter = Archivarius.BinaryBackend.BinaryWriter;

namespace Archivarius.Tests
{
    public class Check_AsyncStream
    {
        [Test]
        public async Task TestStream()
        {
            Pipe pipe = new Pipe();
            var pipeWriterStream = pipe.Writer.AsStream();

            BinaryWriter writer = new BinaryWriter();
            AsyncBufferedStreamReader reader = new AsyncBufferedStreamReader(pipe.Reader.AsStream(), 16);

            HierarchicalSerializer serializer = new HierarchicalSerializer(writer, new TypenameBasedTypeSerializer());
            HierarchicalDeserializer deserializer = new HierarchicalDeserializer(reader, new TypenameBasedTypeDeserializer(), null, null, false);
            
            deserializer.OnException += ex => Assert.Fail(ex.ToString());
            await TestSync(serializer, deserializer, () =>
            {
                pipeWriterStream.Write(writer.TakeBuffer());
            });
            await TestAsync(serializer, deserializer, () =>
            {
                pipeWriterStream.Write(writer.TakeBuffer());
            });
        }

        private async Task TestSync(HierarchicalSerializer serializer, HierarchicalDeserializer deserializer, Action flush)
        {
            var t1 = Task.Run(async () =>
            {
                await Task.Delay(100);
                for (int i = 0; i < 100000; ++i)
                {
                    var ca = new CA() { hello = i.ToString() };
                    serializer.AddClass(ref ca);
                    flush();
                }
            });

            var t2 = Task.Run(() =>
            {
                deserializer.Prepare();
                for (int i = 0; i < 100000; ++i)
                {
                    CA? ca = null;
                    deserializer.AddClass(ref ca);
                    Assert.That(ca!.hello, Is.EqualTo(i.ToString()));
                }
            });
            
            await Task.WhenAll(t1,t2);
        }
        
        private async Task TestAsync(HierarchicalSerializer serializer, HierarchicalDeserializer deserializer, Action flush)
        {
            var t1 = Task.Run(async () =>
            {
                await Task.Delay(100);
                for (int i = 0; i < 100000; ++i)
                {
                    var ca = new CA() { hello = i.ToString() };
                    serializer.AddClass(ref ca);
                    flush();
                }
            });
            
            for (int i = 0; i < 100000; ++i)
            {
                CA? ca = null;
                ca = await deserializer.AddClassAsync(ca);
                Assert.That(ca!.hello, Is.EqualTo(i.ToString()));
            }
            
            await t1;
        }
    }
}