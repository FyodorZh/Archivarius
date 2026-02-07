using System;
using System.Collections.Generic;
using System.Threading;

namespace Archivarius.Storage.Test.ChainStorage
{
    public class CommandsGenerator
    {
        public IEnumerable<ITestCommand<IChainStorage<Payload>>> Generate(int count, int? rndSeed = null)
        {
            Random rnd = rndSeed.HasValue ? new Random(rndSeed.Value) : new Random();
            for (int i = 0; i < count; ++i)
            {
                if (rnd.NextDouble() < 0.001)
                {
                    if (rnd.NextDouble() < 0.5)
                    {
                        yield return new ClearCache_Command<Payload>();
                    }
                    else
                    {
                        yield return new RewriteData_Command<Payload>(rnd.Next() % 2 == 1, rnd.Next() % 2 == 1, rnd.Next() % 2 == 1);
                    }
                }
                else
                {
                    switch (rnd.Next(0, 5))
                    {
                        case 0:
                            yield return new Append_Command<Payload>(Payload.New());
                            break;
                        case 1:
                            yield return new GetAll_Command<Payload>();
                            break;
                        case 2:
                            yield return new GetMany_Command<Payload>(rnd.NextDouble(), rnd.NextDouble());
                            break;
                        case 3:
                            yield return new GetAt_Command<Payload>(rnd.NextDouble());
                            break;
                        case 4:
                            yield return new GetCount_Command<Payload>();
                            break;
                    }
                }
            }
        }
        
        public class Payload : IEquatable<Payload>, IDataStruct
        {
            private static int _counter = 0;

            private int _payload;

            public Payload()
            {
                _payload = -1;
            }

            public static Payload New()
            {
                return new Payload() { _payload = Interlocked.Increment(ref _counter) };
            }

            public override bool Equals(object? obj)
            {
                return obj is Payload other && Equals(other);
            }

            public bool Equals(Payload? other)
            {
                if (other == null)
                {
                    return false;
                }
                return _payload == other._payload;
            }

            public void Serialize(ISerializer serializer)
            {
                serializer.Add(ref _payload);
            }

            public override string ToString()
            {
                return _payload.ToString();
            }
        }
    }
}