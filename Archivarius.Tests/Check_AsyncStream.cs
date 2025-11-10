using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Archivarius.AsyncBackend;
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

            bool finished = false;

            byte[] buffer1 = new byte[128];
            AsyncBufferedWriter writer = new AsyncBufferedWriter(r =>
            {
                int c;
                while ((c = r.Pop(buffer1, 0, buffer1.Length)) != 0)
                {
                    pipe.Writer.AsStream().Write(buffer1, 0, c);
                }

                return default;
            } );
            
            byte[] buffer2 = new byte[128];
            AsyncBufferedReader reader = new AsyncBufferedReader(async (w, count) =>
            {
                while (count > 0)
                {
                    int c = await pipe.Reader.AsStream().ReadAsync(buffer2, 0, buffer2.Length);
                    if (c > 0)
                    {
                        w.Put(buffer2, 0, c);
                        count -= c;
                    }

                    if (c == 0 && finished)
                    {
                        return;
                    }
                }
            });

            HierarchicalSerializer serializer = new HierarchicalSerializer(writer, new TypenameBasedTypeSerializer(), null, false);
            HierarchicalDeserializer deserializer = new HierarchicalDeserializer(reader, new TypenameBasedTypeDeserializer(), null, null, false);
            
            deserializer.OnException += ex => Assert.Fail(ex.ToString());

            var finish = async () =>
            {
                await pipe.Writer.FlushAsync();
                await pipe.Writer.CompleteAsync();
                finished = true;
            };
            
            await TestAsync(serializer, deserializer, reader, finish);
        }
        
        private async Task TestAsync(HierarchicalSerializer serializer, HierarchicalDeserializer deserializer,
            AsyncBufferedReader reader, Func<Task> finish)
        {
            int count = 0;
            int N = 2;//00000;
            var t1 = Task.Run(async () =>
            {
                await Task.Delay(100);
                for (int i = 0; i < N; ++i)
                {
                    var ca = new CA() { hello = i.ToString() };
                    serializer.AddClass(ref ca);
                    await serializer.FlushPoint();
                    Interlocked.Increment(ref count);
                }
                await finish();
            });

            await reader.Preload();
            deserializer.Prepare();
            int i = 0;
            while (true)
            {
                CA? ca = null;
                deserializer.AddClass(ref ca);
                Assert.That(ca!.hello, Is.EqualTo(i.ToString()));
                
                Interlocked.Increment(ref count);
                i += 1;
                
                if (!await deserializer.FlushPoint())
                {
                    break;
                }
            }
            
            await t1;
            
            Assert.That(count, Is.EqualTo(N * 2));
        }
    }
}