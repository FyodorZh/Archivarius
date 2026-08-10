{
  "library": "Archivarius",
  "meta": {
    "target": "netstandard2.0",
    "lang": 12,
    "nullable": true
  },
  "philosophy": "Archivarius is a compact binary serialization library built around the ISerializer interface: data structures implement IDataStruct.Serialize(ISerializer) and use the same code for writing and reading, branching on serializer.IsWriter. The naming convention is Add* for the write/read-symmetric primitive and container calls, with low-level I/O separated into IWriter/IReader backends that optionally write section length headers for forward/backward compatibility and corruption detection.",
  "quickPick": [
    {
      "need": "Arbitrary dynamic types (collections, generics) not expressible as IDataStruct",
      "use": "ISerializerExtension<T>"
    },
    {
      "need": "Forward/backward compatible polymorphic classes with registered numeric ids",
      "use": "IdBasedTypeSerializer / IdBasedTypeDeserializer"
    },
    {
      "need": "In-memory byte[] round-trip",
      "use": "BinaryWriter / BinaryReader"
    },
    {
      "need": "Network or pipe transport with async framing",
      "use": "AsyncBufferedWriter / AsyncBufferedReader"
    },
    {
      "need": "Object graph with shared references",
      "use": "GraphSerializer / GraphDeserializer"
    },
    {
      "need": "Simple type identification without registration",
      "use": "TypenameBasedTypeSerializer / TypenameBasedTypeDeserializer"
    },
    {
      "need": "Stream-backed I/O without sections",
      "use": "BinaryStreamWriter / BinaryStreamReader"
    },
    {
      "need": "Testing serialization logic without buffers",
      "use": "ReaderWriterStream"
    }
  ],
  "decisionTree": [
    [
      "Which transport do you use?",
      "In-memory byte[] buffer",
      "BinaryWriter / BinaryReader"
    ],
    [
      "Which transport do you use?",
      "System.IO.Stream",
      "BinaryStreamWriter / BinaryStreamReader"
    ],
    [
      "Which transport do you use?",
      "Async pipe/network with custom framing",
      "AsyncBufferedWriter / AsyncBufferedReader"
    ],
    [
      "Which transport do you use?",
      "Typed in-process queues (tests)",
      "ReaderWriterStream"
    ],
    [
      "How are polymorphic classes identified?",
      "Single concrete type only (monomorphic)",
      "HierarchicalSerializer"
    ],
    [
      "How are polymorphic classes identified?",
      "Registered numeric ids via [Id]",
      [
        [
          "Do you need shared references preserved?",
          "Yes - object graph",
          "GraphSerializer"
        ],
        [
          "Do you need shared references preserved?",
          "No - regular hierarchical",
          "HierarchicalSerializer"
        ]
      ]
    ],
    [
      "How are polymorphic classes identified?",
      "GUID attributes",
      "GuidBasedTypeSerializer / GuidBasedTypeDeserializer"
    ],
    [
      "How are polymorphic classes identified?",
      "Assembly-qualified type names",
      "TypenameBasedTypeSerializer / TypenameBasedTypeDeserializer"
    ]
  ],
  "interfaces": [
    {
      "name": "IByteQueueReader",
      "members": [
        {
          "sig": "Pop(byte[] dst, int start, int count):int",
          "desc": "copies up to count bytes from the internal buffer into dst at start; returns the number of bytes actually copied, which may be less than count when the buffer has fewer bytes",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "dst must be non-null and have room for count bytes"
          ],
          "example": "int n = queue.Pop(dst, 0, size);"
        }
      ]
    },
    {
      "name": "IByteQueueWriter",
      "members": [
        {
          "sig": "Put(byte[] values, int start, int count)",
          "desc": "appends count bytes from values starting at start into the internal buffer",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "values must be non-null; start + count must not exceed values.Length"
          ],
          "example": "queue.Put(bytes, 0, bytes.Length);"
        }
      ]
    },
    {
      "name": "IConstructor",
      "members": [
        {
          "sig": "Construct():object?",
          "desc": "creates a new instance of the target type; returns null when IsValid is false",
          "threadSafety": "not thread-safe",
          "postconditions": [
            "caller takes ownership of the returned instance"
          ],
          "example": "object? o = ctor.Construct();"
        },
        {
          "sig": "IsValid:bool { get; }",
          "desc": "returns true if Construct can produce a usable instance",
          "threadSafety": "not thread-safe",
          "example": "if (ctor.IsValid) { var o = ctor.Construct(); }"
        }
      ]
    },
    {
      "name": "IConstructorFactory",
      "members": [
        {
          "sig": "Build(Type type):IConstructor",
          "desc": "returns an IConstructor able to create instances of type; the concrete strategy is chosen by the implementation",
          "threadSafety": "not thread-safe",
          "example": "IConstructor ctor = factory.Build(typeof(MyData));"
        }
      ]
    },
    {
      "name": "IDataStruct",
      "members": [
        {
          "sig": "Serialize(ISerializer serializer)",
          "desc": "writes or reads the struct fields; when serializer.IsWriter is true each field is written, otherwise each field is read into the same object; called by the serializer for every AddStruct/AddClass/AddStaticClass/AddDynamic member",
          "threadSafety": "not thread-safe",
          "postconditions": [
            "on a reader, field values are restored into this instance"
          ],
          "example": "public void Serialize(ISerializer s) { s.Add(ref _value); }",
          "remarks": "The same Serialize body must be used on both sides; field order is the wire format."
        }
      ]
    },
    {
      "name": "IHierarchicalDeserializer_Builder",
      "members": [
        {
          "sig": "Build():HierarchicalDeserializer",
          "desc": "creates the configured HierarchicalDeserializer; throws InvalidOperationException if neither SetMonomorphic nor SetPolymorphic was applied",
          "threadSafety": "not thread-safe",
          "example": "var des = HierarchicalDeserializer.From(reader).SetMonomorphic().Build();"
        }
      ]
    },
    {
      "name": "IHierarchicalDeserializer_MonomorphicParams",
      "extends": [
        "IHierarchicalDeserializer_Params",
        "IHierarchicalDeserializer_Builder"
      ],
      "members": [
        {
          "sig": "SetAutoPrepare(bool autoPrepare):IHierarchicalDeserializer_MonomorphicParams",
          "desc": "configures whether the header is read and the type reader prepared immediately on Build; returns this builder",
          "threadSafety": "not thread-safe",
          "example": "From(reader).SetMonomorphic().SetAutoPrepare(true).Build();"
        },
        {
          "sig": "SetCtorFactory(IConstructorFactory ctorFactory):IHierarchicalDeserializer_MonomorphicParams",
          "desc": "replaces the default constructor factory; returns this builder",
          "threadSafety": "not thread-safe",
          "example": "From(reader).SetMonomorphic().SetCtorFactory(new MyFactory()).Build();"
        }
      ]
    },
    {
      "name": "IHierarchicalDeserializer_MonomorphicParamsView",
      "extends": [
        "IHierarchicalDeserializer_ParamsView"
      ],
      "isAuxiliary": true,
      "desc": "marker view interface exposing the monomorphic configuration; normally read via AsMonomorphic() and not implemented by consumers"
    },
    {
      "name": "IHierarchicalDeserializer_Params",
      "members": [
        {
          "sig": "SetAutoPrepare(bool autoPrepare):IHierarchicalDeserializer_Params",
          "desc": "configures whether the header is read and the type reader prepared immediately on Build; returns this builder",
          "threadSafety": "not thread-safe",
          "example": "p.SetAutoPrepare(false);"
        },
        {
          "sig": "SetCtorFactory(IConstructorFactory ctorFactory):IHierarchicalDeserializer_Params",
          "desc": "replaces the default constructor factory; returns this builder",
          "threadSafety": "not thread-safe",
          "example": "p.SetCtorFactory(new DefaultCtorTypeConstructorFactory());"
        },
        {
          "sig": "SetMonomorphic():IHierarchicalDeserializer_MonomorphicParams",
          "desc": "marks the deserializer as monomorphic; throws InvalidOperationException if SetPolymorphic was already called; returns a monomorphic builder",
          "threadSafety": "not thread-safe",
          "example": "var mono = p.SetMonomorphic();"
        },
        {
          "sig": "SetPolymorphic(ITypeDeserializer typeDeserializer):IHierarchicalDeserializer_PolymorphicParams",
          "desc": "marks the deserializer as polymorphic and sets the type deserializer; throws InvalidOperationException if SetMonomorphic was already called; returns a polymorphic builder",
          "threadSafety": "not thread-safe",
          "example": "var poly = p.SetPolymorphic(new IdBasedTypeDeserializer());"
        }
      ]
    },
    {
      "name": "IHierarchicalDeserializer_ParamsView",
      "members": [
        {
          "sig": "AsMonomorphic():IHierarchicalDeserializer_MonomorphicParamsView?",
          "desc": "returns this as a monomorphic view when IsPolymorphic is false, otherwise null",
          "threadSafety": "not thread-safe",
          "example": "var m = paramsView.AsMonomorphic();"
        },
        {
          "sig": "AsPolymorphic():IHierarchicalDeserializer_PolymorphicParamsView?",
          "desc": "returns this as a polymorphic view when IsPolymorphic is true, otherwise null",
          "threadSafety": "not thread-safe",
          "example": "var p = paramsView.AsPolymorphic();"
        },
        {
          "sig": "AutoPrepare:bool { get; }",
          "desc": "returns true if the header is prepared automatically on construction",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "IsPolymorphic:bool? { get; }",
          "desc": "returns true after SetPolymorphic, false after SetMonomorphic, null before either is called",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ObjectConstructorFactory:IConstructorFactory? { get; }",
          "desc": "returns the configured constructor factory or null for the default",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Source:IReader { get; }",
          "desc": "returns the underlying reader",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "IHierarchicalDeserializer_PolymorphicParams",
      "extends": [
        "IHierarchicalDeserializer_Params",
        "IHierarchicalDeserializer_Builder"
      ],
      "members": [
        {
          "sig": "SetAutoPrepare(bool autoPrepare):IHierarchicalDeserializer_PolymorphicParams",
          "desc": "configures whether the header is read and the type reader prepared immediately on Build; returns this builder",
          "threadSafety": "not thread-safe",
          "example": "From(reader).SetPolymorphic(td).SetAutoPrepare(true).Build();"
        },
        {
          "sig": "SetCtorFactory(IConstructorFactory ctorFactory):IHierarchicalDeserializer_PolymorphicParams",
          "desc": "replaces the default constructor factory; returns this builder",
          "threadSafety": "not thread-safe",
          "example": "From(reader).SetPolymorphic(td).SetCtorFactory(f).Build();"
        },
        {
          "sig": "SetDefaultTypeSet(Func<int, IReadOnlyList<Type>> defaultTypeSetProvider):IHierarchicalDeserializer_PolymorphicParams",
          "desc": "provides a callback that resolves a default type set version (written by the writer) to the list of types registered under that version; returns this builder",
          "threadSafety": "not thread-safe",
          "example": "poly.SetDefaultTypeSet(v => v == 1 ? myTypes : Array.Empty<Type>());"
        },
        {
          "sig": "SetExtensionsFactory(ISerializerExtensionsFactory extensionsFactory):IHierarchicalDeserializer_PolymorphicParams",
          "desc": "sets the factory used by AddDynamic; returns this builder",
          "threadSafety": "not thread-safe",
          "example": "poly.SetExtensionsFactory(new MyExtensionsFactory());"
        }
      ]
    },
    {
      "name": "IHierarchicalDeserializer_PolymorphicParamsView",
      "extends": [
        "IHierarchicalDeserializer_ParamsView"
      ],
      "members": [
        {
          "sig": "DefaultTypeSetProvider:Func<int, IReadOnlyList<Type>>? { get; }",
          "desc": "returns the configured default type set provider or null",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ExtensionsFactory:ISerializerExtensionsFactory? { get; }",
          "desc": "returns the configured extensions factory or null",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "TypeDeserializer:ITypeDeserializer? { get; }",
          "desc": "returns the configured type deserializer or null",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "ILowLevelReader",
      "members": [
        {
          "sig": "ReadArray():byte[]?",
          "desc": "reads a length-prefixed byte array; returns null if a null sentinel was stored; throws EndOfStreamException if not enough data remains; throws InvalidDataException if the stored data is not a valid byte-array encoding or a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "must be paired with a previous ILowLevelWriter.WriteArray call"
          ],
          "example": "byte[]? arr = reader.ReadArray();",
          "remarks": "In this library the buffer/stream backends throw InvalidOperationException instead of EndOfStreamException on truncation."
        },
        {
          "sig": "ReadBool():bool",
          "desc": "reads a bool; throws EndOfStreamException if not enough data remains; throws InvalidDataException if the stored data is not a valid bool encoding or a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "bool b = reader.ReadBool();"
        },
        {
          "sig": "ReadByte():byte",
          "desc": "reads a byte; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "byte b = reader.ReadByte();"
        },
        {
          "sig": "ReadBytes(byte[] dst, int offset, int count)",
          "desc": "reads count raw bytes into dst at offset; throws ArgumentNullException if dst is null; throws ArgumentOutOfRangeException if offset or count is negative; throws ArgumentException if offset + count exceeds dst.Length; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "must be paired with a previous ILowLevelWriter.WriteBytes call with the same count"
          ],
          "example": "reader.ReadBytes(dst, 0, 8);"
        },
        {
          "sig": "ReadChar():char",
          "desc": "reads a char; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "char c = reader.ReadChar();"
        },
        {
          "sig": "ReadDecimal():decimal",
          "desc": "reads a decimal; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "decimal d = reader.ReadDecimal();"
        },
        {
          "sig": "ReadDouble():double",
          "desc": "reads a double; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "double d = reader.ReadDouble();"
        },
        {
          "sig": "ReadFloat():float",
          "desc": "reads a float; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "float f = reader.ReadFloat();"
        },
        {
          "sig": "ReadInt():int",
          "desc": "reads an int; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "int i = reader.ReadInt();"
        },
        {
          "sig": "ReadLong():long",
          "desc": "reads a long; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "long l = reader.ReadLong();"
        },
        {
          "sig": "ReadSByte():sbyte",
          "desc": "reads an sbyte; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "sbyte sb = reader.ReadSByte();"
        },
        {
          "sig": "ReadShort():short",
          "desc": "reads a short; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "short s = reader.ReadShort();"
        },
        {
          "sig": "ReadString():string?",
          "desc": "reads a nullable UTF-8 string; returns null if a null sentinel was stored; throws EndOfStreamException if not enough data remains; throws InvalidDataException if the stored data is not a valid string encoding or a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "must be paired with a previous ILowLevelWriter.WriteString call"
          ],
          "example": "string? s = reader.ReadString();"
        },
        {
          "sig": "ReadUInt():uint",
          "desc": "reads a uint; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "uint u = reader.ReadUInt();"
        },
        {
          "sig": "ReadULong():ulong",
          "desc": "reads a ulong; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "ulong ul = reader.ReadULong();"
        },
        {
          "sig": "ReadUShort():ushort",
          "desc": "reads a ushort; throws EndOfStreamException if not enough data remains; throws InvalidDataException if a type mismatch is detected",
          "threadSafety": "not thread-safe",
          "example": "ushort us = reader.ReadUShort();"
        }
      ]
    },
    {
      "name": "ILowLevelWriter",
      "members": [
        {
          "sig": "WriteArray(byte[]? value)",
          "desc": "writes a nullable byte array using a length prefix where 0 encodes null; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteArray(bytes);"
        },
        {
          "sig": "WriteBool(bool value)",
          "desc": "writes a bool; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteBool(true);"
        },
        {
          "sig": "WriteByte(byte value)",
          "desc": "writes a byte; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteByte(1);"
        },
        {
          "sig": "WriteBytes(byte[] value, int offset, int count)",
          "desc": "writes count raw bytes from value at offset; throws ArgumentNullException if value is null; throws ArgumentOutOfRangeException if offset or count is negative; throws ArgumentException if offset + count exceeds value.Length; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteBytes(src, 0, src.Length);"
        },
        {
          "sig": "WriteChar(char value)",
          "desc": "writes a char; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteChar('x');"
        },
        {
          "sig": "WriteDecimal(decimal value)",
          "desc": "writes a decimal; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteDecimal(3.14m);"
        },
        {
          "sig": "WriteDouble(double value)",
          "desc": "writes a double; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteDouble(1.5);"
        },
        {
          "sig": "WriteFloat(float value)",
          "desc": "writes a float; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteFloat(1.5f);"
        },
        {
          "sig": "WriteInt(int value)",
          "desc": "writes an int; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteInt(42);"
        },
        {
          "sig": "WriteLong(long value)",
          "desc": "writes a long; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteLong(42L);"
        },
        {
          "sig": "WriteSByte(sbyte value)",
          "desc": "writes an sbyte; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteSByte(-1);"
        },
        {
          "sig": "WriteShort(short value)",
          "desc": "writes a short; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteShort((short)7);"
        },
        {
          "sig": "WriteString(string? value)",
          "desc": "writes a nullable UTF-8 string using a length prefix where 0 encodes null; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteString(\"hello\");"
        },
        {
          "sig": "WriteUInt(uint value)",
          "desc": "writes a uint; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteUInt(1u);"
        },
        {
          "sig": "WriteULong(ulong value)",
          "desc": "writes a ulong; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteULong(1ul);"
        },
        {
          "sig": "WriteUShort(ushort value)",
          "desc": "writes a ushort; throws InvalidOperationException if the writer has already been completed via IWriter.Complete",
          "threadSafety": "not thread-safe",
          "example": "writer.WriteUShort(1);"
        }
      ]
    },
    {
      "name": "IPrimitiveClassSerializer<T>",
      "extends": [
        "IPrimitiveSerializer<T?>"
      ],
      "genericConstraints": "where T : class",
      "members": [
        {
          "sig": "Add(ref T? value)",
          "desc": "writes or reads a nullable class reference of type T; inherited from IPrimitiveSerializer<T?>",
          "threadSafety": "not thread-safe",
          "example": "serializer.Add(ref name);"
        }
      ]
    },
    {
      "name": "IPrimitiveSerializer",
      "isAuxiliary": true,
      "desc": "aggregates IPrimitiveSerializer<T> for bool, byte, sbyte, char, short, ushort, int, uint, long, ulong, float, double, decimal, string, byte[], Guid, DateTime and TimeSpan; this is what gives ISerializer its set of Add(ref T) overloads"
    },
    {
      "name": "IPrimitiveSerializer<T>",
      "extends": [
        "ISerializerCore"
      ],
      "members": [
        {
          "sig": "Add(ref T value)",
          "desc": "writes or reads a single primitive value of type T; on a writer the current value is stored, on a reader the field is overwritten with the stored value",
          "threadSafety": "not thread-safe",
          "example": "serializer.Add(ref count);"
        }
      ]
    },
    {
      "name": "IReader",
      "extends": [
        "ILowLevelReader"
      ],
      "members": [
        {
          "sig": "BeginSection()",
          "desc": "reads a section length header (when sections are enabled) and constrains subsequent reads to that boundary; throws InvalidOperationException if the reader has been completed; throws EndOfStreamException if not enough data remains to read the header",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "must be balanced by exactly one EndSection call; sections must nest in LIFO order"
          ],
          "example": "reader.BeginSection();",
          "remarks": "When sections are disabled this is a no-op but the nesting contract still applies."
        },
        {
          "sig": "EndSection():bool",
          "desc": "closes the current section and skips any unconsumed bytes to restore stream alignment; returns false if bytes were skipped, true if the section was fully consumed; throws InvalidOperationException if called without a matching open BeginSection",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "must be preceded by a matching BeginSection call"
          ],
          "example": "bool fullyConsumed = reader.EndSection();",
          "remarks": "The nesting contract is enforced even when sections are disabled."
        },
        {
          "sig": "Fetch():ValueTask<bool>",
          "desc": "loads the next logical block of data; returns false when no more data will ever be available; throws InvalidOperationException if a section is currently open; throws IOException if an I/O error occurs; must be called before reading any data",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "Fetch calls must match writer Flush calls 1-to-1; IWriter.Complete counts as one Flush"
          ],
          "example": "while (await reader.Fetch()) { var v = reader.ReadInt(); }",
          "remarks": "A true return does not guarantee new bytes are present; never read without a preceding Fetch."
        },
        {
          "sig": "IsCompleted:bool { get; }",
          "desc": "returns true only after Fetch has returned false; in this library every concrete reader returns false permanently",
          "threadSafety": "not thread-safe",
          "example": "if (reader.IsCompleted) { break; }"
        },
        {
          "sig": "TrySetSectionUsage(bool useSections):bool",
          "desc": "attempts to enable or disable section markers; must be called before any data is read; returns false if data was already read or the implementation does not support the requested mode; disabling is always accepted",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "must match the value used on the paired writer"
          ],
          "example": "if (!reader.TrySetSectionUsage(true)) { throw new Exception(); }",
          "remarks": "Sections are disabled by default on every implementation except BinaryReader."
        }
      ]
    },
    {
      "name": "ISerializer",
      "extends": [
        "IPrimitiveSerializer"
      ],
      "members": [
        {
          "sig": "AddClass<T>(ref T? value)",
          "desc": "serializes a class field with an optional polymorphic type id; on the writer a runtime type without a default constructor throws InvalidOperationException; on the reader an unresolvable type produces null and raises OnException",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : class, IDataStruct",
          "preconditions": [
            "T must have a public or non-public default constructor to be deserialized"
          ],
          "example": "serializer.AddClass(ref myClass);"
        },
        {
          "sig": "AddDynamic<T>(ref T value)",
          "desc": "serializes an arbitrary type through ISerializerExtension<T> from the extensions factory; throws InvalidOperationException if no factory is configured or Construct<T> returns null",
          "threadSafety": "not thread-safe",
          "example": "serializer.AddDynamic(ref myThing);"
        },
        {
          "sig": "AddStaticClass<T>(ref T? value)",
          "desc": "serializes a class field using a fixed type marker without a type id; on the writer a runtime subtype different from T throws InvalidOperationException",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : class, IDataStruct",
          "example": "serializer.AddStaticClass(ref myClass);"
        },
        {
          "sig": "AddStruct<T>(ref T value)",
          "desc": "serializes a struct by calling value.Serialize(this) with no version or type header",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : struct, IDataStruct",
          "example": "serializer.AddStruct(ref point);"
        },
        {
          "sig": "AddVersionedStruct<T>(ref T value)",
          "desc": "writes or reads value.Version as a byte and sets Version during the Serialize call; on the reader the stored version is exposed so old fields can be skipped",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : struct, IDataStruct, IVersionedData",
          "example": "serializer.AddVersionedStruct(ref data);"
        },
        {
          "sig": "FlushPoint():ValueTask<bool>",
          "desc": "synchronizes with the transport; on the writer flushes buffered data and returns true, on the reader performs a Fetch",
          "threadSafety": "not thread-safe",
          "example": "await serializer.FlushPoint();"
        },
        {
          "sig": "Version:byte { get; }",
          "desc": "returns the current version context; 0 unless a versioned struct or versioned class is being serialized",
          "threadSafety": "not thread-safe",
          "example": "if (serializer.Version == 1) { serializer.Add(ref oldField); }"
        }
      ]
    },
    {
      "name": "ISerializerCore",
      "members": [
        {
          "sig": "IsWriter:bool { get; }",
          "desc": "returns true on serializers (writing) and false on deserializers (reading); used by Serialize bodies to pick the branch",
          "threadSafety": "not thread-safe",
          "example": "if (serializer.IsWriter) { ... } else { ... }"
        },
        {
          "sig": "Reader:ILowLevelReader { get; }",
          "desc": "returns the underlying low-level reader; throws InvalidOperationException on a writer",
          "threadSafety": "not thread-safe",
          "example": "serializer.Reader.ReadInt();"
        },
        {
          "sig": "Writer:ILowLevelWriter { get; }",
          "desc": "returns the underlying low-level writer; throws InvalidOperationException on a deserializer",
          "threadSafety": "not thread-safe",
          "example": "serializer.Writer.WriteInt(1);"
        }
      ]
    },
    {
      "name": "ISerializerExtension",
      "isAuxiliary": true,
      "desc": "marker interface for serialization extensions; add members via ISerializerExtension<T>"
    },
    {
      "name": "ISerializerExtension<T>",
      "extends": [
        "ISerializerExtension"
      ],
      "members": [
        {
          "sig": "Add(ISerializer serializer, ref T value)",
          "desc": "serializes one value of type T; called by ISerializer.AddDynamic; the implementation branches on serializer.IsWriter",
          "threadSafety": "not thread-safe",
          "example": "public void Add(ISerializer s, ref T v) { if (s.IsWriter) { ... } else { ... } }"
        }
      ]
    },
    {
      "name": "ISerializerExtensionsFactory",
      "members": [
        {
          "sig": "Construct(Type type):ISerializerExtension?",
          "desc": "returns an extension for type or null if the type is not recognized",
          "threadSafety": "not thread-safe",
          "example": "ISerializerExtension? ext = factory.Construct(typeof(MyType));"
        },
        {
          "sig": "Construct<T>():ISerializerExtension<T>?",
          "desc": "returns a strongly typed extension for T or null if T is not recognized",
          "threadSafety": "not thread-safe",
          "example": "ISerializerExtension<MyType>? ext = factory.Construct<MyType>();"
        },
        {
          "sig": "event Action<Type, Exception> OnError",
          "desc": "raised when an extension fails to handle a type; subscribed by the serializers to forward failures",
          "threadSafety": "not thread-safe",
          "remarks": "SerializerExtensionsEmptyFactory is a ready-made implementation that always returns null."
        }
      ]
    },
    {
      "name": "ITypeDeserializer",
      "members": [
        {
          "sig": "Deserialize(IReader reader):Type?",
          "desc": "reads a type identifier from the reader and returns the resolved Type; returns null if the identifier is unknown",
          "threadSafety": "not thread-safe",
          "example": "Type? t = typeDeserializer.Deserialize(reader);"
        }
      ]
    },
    {
      "name": "ITypeSerializer",
      "members": [
        {
          "sig": "Serialize(IWriter writer, Type type)",
          "desc": "writes an identifier that fully describes type, including generic arguments",
          "threadSafety": "not thread-safe",
          "example": "typeSerializer.Serialize(writer, typeof(Derived));"
        }
      ]
    },
    {
      "name": "IVersionedData",
      "members": [
        {
          "sig": "Version:byte { get; }",
          "desc": "returns the format version of the data; written as a byte before the data body when serialized through AddVersionedStruct or a versioned class",
          "threadSafety": "not thread-safe",
          "example": "public byte Version => 1;"
        }
      ]
    },
    {
      "name": "IVersionedDataStruct",
      "extends": [
        "IDataStruct",
        "IVersionedData"
      ],
      "isAuxiliary": true,
      "desc": "marker interface combining IDataStruct and IVersionedData; lets consumers refer to a versioned serializable struct with one type constraint"
    },
    {
      "name": "IWriter",
      "extends": [
        "ILowLevelWriter"
      ],
      "members": [
        {
          "sig": "BeginSection()",
          "desc": "marks the beginning of a logical section and reserves space for its length header; throws InvalidOperationException if the writer has been completed",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "must be balanced by exactly one EndSection call; sections must nest in LIFO order"
          ],
          "example": "writer.BeginSection();",
          "remarks": "When sections are disabled this is a no-op but the nesting contract still applies."
        },
        {
          "sig": "Complete():ValueTask",
          "desc": "flushes all buffered data and signals that no more data will be written; throws InvalidOperationException if a section is open or if already completed; counts as one Flush for lockstep purposes",
          "threadSafety": "not thread-safe",
          "example": "await writer.Complete();"
        },
        {
          "sig": "EndSection()",
          "desc": "marks the end of the most recently opened section and writes its length header; throws InvalidOperationException if called without a matching open BeginSection or if the writer has been completed",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "must be preceded by a matching BeginSection call"
          ],
          "example": "writer.EndSection();"
        },
        {
          "sig": "Flush():ValueTask",
          "desc": "commits buffered data to the underlying store; throws InvalidOperationException if sections are enabled and a section is open, or if the writer has been completed; throws IOException on I/O error",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "Flush calls must match reader Fetch calls 1-to-1"
          ],
          "example": "await writer.Flush();"
        },
        {
          "sig": "TrySetSectionUsage(bool useSections):bool",
          "desc": "attempts to enable or disable section markers; must be called before any data is written; returns false if data was already written or the implementation does not support the requested mode; disabling is always accepted",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "must match the value used on the paired reader"
          ],
          "example": "if (!writer.TrySetSectionUsage(true)) { throw new Exception(); }",
          "remarks": "BinaryStreamWriter, AsyncBufferedWriter and ReaderWriterStream reject sections; the in-memory BinaryWriter defaults to them enabled."
        }
      ]
    }
  ],
  "types": [
    {
      "name": "AsyncBufferedReader",
      "kind": "class",
      "category": "reader",
      "implements": [
        "IReader"
      ],
      "desc": "async reader that pulls framed blocks through a Func<IByteQueueWriter, int, ValueTask> callback; sections are not supported",
      "threadSafety": "not thread-safe",
      "limitations": "sections are rejected (TrySetSectionUsage(true) returns false); IsCompleted always returns false",
      "constructors": [
        {
          "sig": "ctor(Func<IByteQueueWriter, int, ValueTask> readAsync)",
          "desc": "creates a reader that asks readAsync to fill the internal buffer when more bytes are needed; the callback should block until data arrives or the source is exhausted",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "readAsync must be non-null"
          ],
          "example": "var r = new AsyncBufferedReader(async (q, n) => { var bytes = await source.ReadAsync(n); q.Put(bytes, 0, bytes.Length); });"
        }
      ],
      "properties": [
        {
          "sig": "IsCompleted:bool { get; }",
          "desc": "always returns false in this implementation",
          "threadSafety": "not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "BeginSection()",
          "desc": "no-op; sections are unsupported",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "EndSection():bool",
          "desc": "always returns true; sections are unsupported",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Fetch():ValueTask<bool>",
          "desc": "reads the next 4-byte frame size header and the framed payload; returns false when the header size is zero or the source cannot supply the frame",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "must be called before reading and paired with writer Flush calls"
          ]
        },
        {
          "sig": "ReadArray():byte[]?",
          "desc": "reads a length-prefixed byte array from the internal buffer; throws InvalidOperationException if fewer bytes remain than declared",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadBool():bool",
          "desc": "reads a bool; throws InvalidOperationException if fewer than 1 byte remains; throws ArgumentOutOfRangeException if the stored byte is not 0 or 1",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadByte():byte",
          "desc": "reads a byte; throws InvalidOperationException if fewer than 1 byte remains",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadBytes(byte[] dst, int offset, int count)",
          "desc": "copies count raw bytes into dst; throws InvalidOperationException if fewer than count bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadChar():char",
          "desc": "reads a char; throws InvalidOperationException if fewer than 2 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadDecimal():decimal",
          "desc": "reads a decimal; throws InvalidOperationException if fewer than 16 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadDouble():double",
          "desc": "reads a double; throws InvalidOperationException if fewer than 8 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadFloat():float",
          "desc": "reads a float; throws InvalidOperationException if fewer than 4 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadInt():int",
          "desc": "reads an int; throws InvalidOperationException if fewer than 4 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadLong():long",
          "desc": "reads a long; throws InvalidOperationException if fewer than 8 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadSByte():sbyte",
          "desc": "reads an sbyte; throws InvalidOperationException if fewer than 1 byte remains",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadShort():short",
          "desc": "reads a short; throws InvalidOperationException if fewer than 2 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadString():string?",
          "desc": "reads a length-prefixed UTF-8 string; throws InvalidOperationException if fewer bytes remain than declared",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadUInt():uint",
          "desc": "reads a uint as unchecked ReadInt; throws InvalidOperationException if fewer than 4 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadULong():ulong",
          "desc": "reads a ulong as unchecked ReadLong; throws InvalidOperationException if fewer than 8 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadUShort():ushort",
          "desc": "reads a ushort as unchecked ReadShort; throws InvalidOperationException if fewer than 2 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "TrySetSectionUsage(bool useSections):bool",
          "desc": "returns true for false and false for true; sections are not supported",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "AsyncBufferedWriter",
      "kind": "class",
      "category": "writer",
      "implements": [
        "IWriter"
      ],
      "desc": "async writer that accumulates bytes and ships framed blocks through a Func<IByteQueueReader, ValueTask> callback on each Flush; sections are not supported",
      "threadSafety": "not thread-safe",
      "limitations": "sections are rejected (TrySetSectionUsage(true) returns false)",
      "constructors": [
        {
          "sig": "ctor(Func<IByteQueueReader, ValueTask> asyncWriter)",
          "desc": "creates a writer whose asyncWriter callback receives the frame (size header plus payload) on every Flush; the callback must drain the queue",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "asyncWriter must be non-null"
          ],
          "example": "var w = new AsyncBufferedWriter(async q => { byte[] h = new byte[4]; q.Pop(h, 0, 4); int n = BitConverter.ToInt32(h, 0); byte[] p = new byte[n]; q.Pop(p, 0, n); await SendAsync(p); });"
        }
      ],
      "methods": [
        {
          "sig": "BeginSection()",
          "desc": "no-op; sections are unsupported",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Clear()",
          "desc": "resets the buffer and writes a fresh 4-byte frame-size placeholder",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Complete():ValueTask",
          "desc": "flushes the pending block and returns; does not mark the writer as completed",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "EndSection()",
          "desc": "no-op; sections are unsupported",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Flush():ValueTask",
          "desc": "fills in the frame-size header, invokes asyncWriter with the whole block, then prepares the next block",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "TrySetSectionUsage(bool useSections):bool",
          "desc": "returns true for false and false for true; sections are not supported",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteArray(byte[]? value)",
          "desc": "buffers a length-prefixed byte array; a null value is encoded as the zero sentinel",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteBool(bool value)",
          "desc": "buffers a bool as one byte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteByte(byte value)",
          "desc": "buffers one byte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteBytes(byte[] value, int offset, int count)",
          "desc": "buffers count raw bytes from value at offset",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteChar(char value)",
          "desc": "buffers a char as two bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteDecimal(decimal value)",
          "desc": "buffers a decimal as sixteen bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteDouble(double value)",
          "desc": "buffers a double as eight bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteFloat(float value)",
          "desc": "buffers a float as four bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteInt(int value)",
          "desc": "buffers an int as four bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteLong(long value)",
          "desc": "buffers a long as eight bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteSByte(sbyte value)",
          "desc": "buffers an sbyte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteShort(short value)",
          "desc": "buffers a short as two bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteString(string? value)",
          "desc": "buffers a length-prefixed UTF-8 string; a null value is encoded as the zero sentinel",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteUInt(uint value)",
          "desc": "buffers a uint as unchecked WriteInt",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteULong(ulong value)",
          "desc": "buffers a ulong as unchecked WriteLong",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteUShort(ushort value)",
          "desc": "buffers a ushort as unchecked WriteShort",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "AttributeBasedTypeDeserializer<TId>",
      "kind": "class",
      "category": "type-serialization",
      "implements": [
        "ITypeDeserializer"
      ],
      "desc": "abstract base that reconstructs a Type from a sequence of attribute-derived ids read from the stream, resolving generic arguments recursively",
      "threadSafety": "not thread-safe",
      "limitations": "subclasses must be configured (Add/RegisterType/RegisterAssembly) before Deserialize can resolve registered types",
      "methods": [
        {
          "sig": "Add(params Assembly[] assemblies):AttributeBasedTypeDeserializer<TId>",
          "desc": "registers every non-abstract IDataStruct type with an identification attribute found in the given assemblies; returns this",
          "threadSafety": "not thread-safe",
          "example": "new IdBasedTypeDeserializer().Add(typeof(MyData).Assembly);"
        },
        {
          "sig": "Add(params Type[] types):AttributeBasedTypeDeserializer<TId>",
          "desc": "registers each type explicitly; throws InvalidOperationException if a type does not inherit IDataStruct, is abstract, or lacks the identification attribute",
          "threadSafety": "not thread-safe",
          "example": "new GuidBasedTypeDeserializer().Add(typeof(A), typeof(B));"
        },
        {
          "sig": "Deserialize(IReader reader):Type?",
          "desc": "reads the version byte and id list and reconstructs the Type; throws InvalidOperationException if the version is not 0; returns null if any id is unknown",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "the reader must be positioned at a type written by the matching AttributeBasedTypeSerializer"
          ]
        },
        {
          "sig": "RegisterAssembly(Assembly assembly)",
          "desc": "registers all non-abstract IDataStruct types in assembly, silently skipping those without an identification attribute",
          "threadSafety": "not thread-safe",
          "example": "deserializer.RegisterAssembly(assembly);"
        },
        {
          "sig": "RegisterType(Type type)",
          "desc": "registers a single type; throws InvalidOperationException if it does not inherit IDataStruct, is abstract, or lacks the identification attribute",
          "threadSafety": "not thread-safe",
          "example": "deserializer.RegisterType(typeof(MyData));"
        }
      ]
    },
    {
      "name": "AttributeBasedTypeSerializer<TId>",
      "kind": "class",
      "category": "type-serialization",
      "implements": [
        "ITypeSerializer"
      ],
      "desc": "abstract base that encodes a Type (including generic arguments) as a flat id list: version byte 0, id count byte, then ids in pre-order",
      "threadSafety": "not thread-safe",
      "limitations": "the count is stored in a single byte, so a single type graph with more than 255 ids throws OverflowException",
      "methods": [
        {
          "sig": "Serialize(IWriter writer, Type type)",
          "desc": "writes the id list for type; throws OverflowException if the id count exceeds 255; throws InvalidOperationException if GetTypeId cannot produce an id for any type",
          "threadSafety": "not thread-safe",
          "example": "typeSerializer.Serialize(writer, typeof(Derived));"
        }
      ]
    },
    {
      "name": "BinaryReader",
      "kind": "class",
      "category": "reader",
      "implements": [
        "IReader"
      ],
      "desc": "in-memory reader over a byte[] buffer; does not detect type mismatches and throws InvalidOperationException (not EndOfStreamException) when reads exceed the available data",
      "threadSafety": "not thread-safe",
      "limitations": "holds a reference to the supplied buffer, it is not copied; the buffer must not be mutated while reading; no type safety is performed",
      "constructors": [
        {
          "sig": "ctor(byte[] buffer, int count = -1)",
          "desc": "creates a reader over buffer; count defaults to buffer.Length when negative",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "buffer must be non-null; count, when non-negative, must not exceed buffer.Length"
          ],
          "postconditions": [
            "holds a reference to buffer; caller must keep it alive and unmodified"
          ],
          "example": "var r = new BinaryReader(bytes);"
        }
      ],
      "properties": [
        {
          "sig": "IsCompleted:bool { get; }",
          "desc": "always returns false in this implementation",
          "threadSafety": "not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "BeginSection()",
          "desc": "when sections are enabled, reads an int length header and limits reads to that boundary; otherwise no-op",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "EndSection():bool",
          "desc": "closes the current section; skips unconsumed bytes and returns false if any were skipped, true if fully consumed; when sections are disabled always returns true",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Fetch():ValueTask<bool>",
          "desc": "returns true while bytes remain in the buffer, false when the read position reaches the end",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadArray():byte[]?",
          "desc": "reads a length-prefixed byte array; returns null for the zero sentinel; throws InvalidOperationException if fewer bytes remain than declared",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadBool():bool",
          "desc": "reads a bool; throws InvalidOperationException if fewer than 1 byte remains; throws ArgumentOutOfRangeException if the stored byte is not 0 or 1",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadByte():byte",
          "desc": "reads a byte; throws InvalidOperationException if fewer than 1 byte remains",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadBytes(byte[] dst, int offset, int count)",
          "desc": "copies count raw bytes into dst at offset; throws InvalidOperationException if fewer than count bytes remain; throws ArgumentNullException if dst is null; throws ArgumentOutOfRangeException if offset or count is negative; throws ArgumentException if offset + count exceeds dst.Length",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadChar():char",
          "desc": "reads a char; throws InvalidOperationException if fewer than 2 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadDecimal():decimal",
          "desc": "reads a decimal; throws InvalidOperationException if fewer than 16 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadDouble():double",
          "desc": "reads a double; throws InvalidOperationException if fewer than 8 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadFloat():float",
          "desc": "reads a float; throws InvalidOperationException if fewer than 4 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadInt():int",
          "desc": "reads an int; throws InvalidOperationException if fewer than 4 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadLong():long",
          "desc": "reads a long; throws InvalidOperationException if fewer than 8 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadSByte():sbyte",
          "desc": "reads an sbyte as unchecked ReadByte; throws InvalidOperationException if fewer than 1 byte remains",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadShort():short",
          "desc": "reads a short; throws InvalidOperationException if fewer than 2 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadString():string?",
          "desc": "reads a length-prefixed UTF-8 string; returns null for the zero sentinel; throws InvalidOperationException if fewer bytes remain than declared",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadUInt():uint",
          "desc": "reads a uint as unchecked ReadInt; throws InvalidOperationException if fewer than 4 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadULong():ulong",
          "desc": "reads a ulong as unchecked ReadLong; throws InvalidOperationException if fewer than 8 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadUShort():ushort",
          "desc": "reads a ushort as unchecked ReadShort; throws InvalidOperationException if fewer than 2 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Reset(byte[] newBuffer, int count = -1)",
          "desc": "rebinds the reader to a new buffer and resets position and section stack; count defaults to newBuffer.Length when negative",
          "threadSafety": "not thread-safe",
          "example": "reader.Reset(nextChunk);"
        },
        {
          "sig": "TrySetSectionUsage(bool useSections):bool",
          "desc": "always returns true and stores the requested mode; note that sections are enabled by default in this implementation, so call it before use",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "BinaryStreamReader",
      "kind": "class",
      "category": "reader",
      "implements": [
        "IReader"
      ],
      "desc": "stream-backed reader that reads directly from a System.IO.Stream position; sections are supported",
      "threadSafety": "not thread-safe",
      "limitations": "requires a seekable stream because it uses Length and Position; cannot read from non-seekable sources",
      "constructors": [
        {
          "sig": "ctor(Stream stream, long count = -1)",
          "desc": "creates a reader over stream; count defaults to the remaining stream length when negative",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "stream must be non-null, readable and seekable"
          ],
          "example": "var r = new BinaryStreamReader(stream);"
        }
      ],
      "properties": [
        {
          "sig": "IsCompleted:bool { get; }",
          "desc": "always returns false in this implementation",
          "threadSafety": "not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "BeginSection()",
          "desc": "when sections are enabled, reads an int length header and limits reads to that boundary; otherwise no-op",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "EndSection():bool",
          "desc": "closes the current section; skips unconsumed bytes and returns false if any were skipped, true if fully consumed; when sections are disabled always returns true",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Fetch():ValueTask<bool>",
          "desc": "returns true while the stream position is below the configured limit, false otherwise",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadArray():byte[]?",
          "desc": "reads a length-prefixed byte array; returns null for the zero sentinel; throws InvalidOperationException if fewer bytes remain than declared; throws EndOfStreamException if the stream ends before the array is read",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadBool():bool",
          "desc": "reads a bool; throws InvalidOperationException if fewer than 1 byte remains; throws ArgumentOutOfRangeException if the stored byte is not 0 or 1",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadByte():byte",
          "desc": "reads a byte; throws InvalidOperationException if fewer than 1 byte remains",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadBytes(byte[] dst, int offset, int count)",
          "desc": "reads count raw bytes into dst; throws InvalidOperationException if fewer than count bytes remain; throws EndOfStreamException if the stream ends before count bytes are read",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadChar():char",
          "desc": "reads a char; throws InvalidOperationException if fewer than 2 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadDecimal():decimal",
          "desc": "reads a decimal; throws InvalidOperationException if fewer than 16 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadDouble():double",
          "desc": "reads a double; throws InvalidOperationException if fewer than 8 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadFloat():float",
          "desc": "reads a float; throws InvalidOperationException if fewer than 4 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadInt():int",
          "desc": "reads an int; throws InvalidOperationException if fewer than 4 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadLong():long",
          "desc": "reads a long; throws InvalidOperationException if fewer than 8 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadSByte():sbyte",
          "desc": "reads an sbyte as unchecked ReadByte; throws InvalidOperationException if fewer than 1 byte remains",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadShort():short",
          "desc": "reads a short; throws InvalidOperationException if fewer than 2 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadString():string?",
          "desc": "reads a length-prefixed UTF-8 string; returns null for the zero sentinel; throws InvalidOperationException if fewer bytes remain than declared; throws EndOfStreamException if the stream ends before the string is read",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadUInt():uint",
          "desc": "reads a uint as unchecked ReadInt; throws InvalidOperationException if fewer than 4 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadULong():ulong",
          "desc": "reads a ulong as unchecked ReadLong; throws InvalidOperationException if fewer than 8 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadUShort():ushort",
          "desc": "reads a ushort as unchecked ReadShort; throws InvalidOperationException if fewer than 2 bytes remain",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Reset()",
          "desc": "resets the section stack and rewinds the stream position to zero",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Reset(Stream stream, long count = -1)",
          "desc": "rebinds the reader to a new stream and resets the section stack; count defaults to the remaining stream length when negative",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "TrySetSectionUsage(bool useSections):bool",
          "desc": "always returns true and stores the requested mode",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "BinaryStreamWriter",
      "kind": "class",
      "category": "writer",
      "implements": [
        "IWriter"
      ],
      "desc": "stream-backed writer that writes each primitive directly to a System.IO.Stream; sections are not supported",
      "threadSafety": "not thread-safe",
      "limitations": "rejects section usage; has no buffering so it is less efficient for many small writes",
      "constructors": [
        {
          "sig": "ctor(Stream stream)",
          "desc": "creates a writer over stream; holds a reference only and does not own it",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "stream must be non-null and writable"
          ],
          "example": "var w = new BinaryStreamWriter(stream);"
        }
      ],
      "properties": [
        {
          "sig": "BaseStream:Stream { get; }",
          "desc": "returns the underlying stream",
          "threadSafety": "not thread-safe",
          "example": "Stream s = writer.BaseStream;"
        }
      ],
      "methods": [
        {
          "sig": "BeginSection()",
          "desc": "no-op; sections are unsupported",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Complete():ValueTask",
          "desc": "no-op; does not flush or mark the writer as completed",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "EndSection()",
          "desc": "no-op; sections are unsupported",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Flush():ValueTask",
          "desc": "no-op; data is written to the stream immediately by each Write method",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "TrySetSectionUsage(bool useSections):bool",
          "desc": "returns true for false and false for true; sections are not supported",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteArray(byte[]? value)",
          "desc": "writes a length-prefixed byte array to the stream; a null value is encoded as the zero sentinel",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteBool(bool value)",
          "desc": "writes a bool as one byte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteByte(byte value)",
          "desc": "writes one byte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteBytes(byte[] value, int offset, int count)",
          "desc": "writes count raw bytes from value at offset",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteChar(char value)",
          "desc": "writes a char as two bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteDecimal(decimal value)",
          "desc": "writes a decimal as sixteen bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteDouble(double value)",
          "desc": "writes a double as eight bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteFloat(float value)",
          "desc": "writes a float as four bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteInt(int value)",
          "desc": "writes an int as four bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteLong(long value)",
          "desc": "writes a long as eight bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteSByte(sbyte value)",
          "desc": "writes an sbyte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteShort(short value)",
          "desc": "writes a short as two bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteString(string? value)",
          "desc": "writes a length-prefixed UTF-8 string; a null value is encoded as the zero sentinel",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteUInt(uint value)",
          "desc": "writes a uint as unchecked WriteInt",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteULong(ulong value)",
          "desc": "writes a ulong as unchecked WriteLong",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteUShort(ushort value)",
          "desc": "writes a ushort as unchecked WriteShort",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "BinaryWriter",
      "kind": "class",
      "category": "writer",
      "implements": [
        "IWriter"
      ],
      "desc": "in-memory writer that appends bytes into a growable buffer (initial capacity 1024, doubling) and exposes the result as a byte[]",
      "threadSafety": "not thread-safe",
      "limitations": "sections are enabled by default (call TrySetSectionUsage before writing); Complete does not actually mark the writer as completed",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty writer with a 1024-byte buffer",
          "threadSafety": "not thread-safe",
          "example": "var w = new BinaryWriter();"
        }
      ],
      "methods": [
        {
          "sig": "BeginSection()",
          "desc": "when sections are enabled, reserves a 4-byte length header placeholder; otherwise no-op",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Clear()",
          "desc": "resets the write position and section stack, discarding buffered data",
          "threadSafety": "not thread-safe",
          "example": "writer.Clear();"
        },
        {
          "sig": "Complete():ValueTask",
          "desc": "no-op; returns immediately and does not prevent further writes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "EndSection()",
          "desc": "writes the current section length into its reserved header; when sections are disabled it is a no-op",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Flush():ValueTask",
          "desc": "no-op; data is always available in the internal buffer",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "GetBuffer():byte[]",
          "desc": "returns a fresh copy of the written bytes; caller takes ownership",
          "threadSafety": "not thread-safe",
          "example": "byte[] bytes = writer.GetBuffer();"
        },
        {
          "sig": "GetBufferUnsafe(out int size):byte[]",
          "desc": "returns the internal buffer and its size; the returned array is reused and must not be retained or mutated by the caller",
          "threadSafety": "not thread-safe",
          "example": "byte[] raw = writer.GetBufferUnsafe(out int n);"
        },
        {
          "sig": "TakeBuffer():byte[]",
          "desc": "returns a copy of the written bytes and resets the write position, allowing the writer to be reused",
          "threadSafety": "not thread-safe",
          "example": "byte[] chunk = writer.TakeBuffer();"
        },
        {
          "sig": "TrySetSectionUsage(bool useSections):bool",
          "desc": "returns false if data has already been written, otherwise stores the mode and returns true",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "must be called before any data is written"
          ]
        },
        {
          "sig": "WriteArray(byte[]? value)",
          "desc": "appends a length-prefixed byte array; a null value is encoded as the zero sentinel",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteBool(bool value)",
          "desc": "appends a bool as one byte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteByte(byte value)",
          "desc": "appends one byte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteBytes(byte[] value, int offset, int count)",
          "desc": "appends count raw bytes from value at offset; throws ArgumentNullException if value is null; throws ArgumentOutOfRangeException if offset or count is negative; throws ArgumentException if offset + count exceeds value.Length",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteChar(char value)",
          "desc": "appends a char as two bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteDecimal(decimal value)",
          "desc": "appends a decimal as sixteen bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteDouble(double value)",
          "desc": "appends a double as eight bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteFloat(float value)",
          "desc": "appends a float as four bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteInt(int value)",
          "desc": "appends an int as four bytes; amortised O(1)",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteLong(long value)",
          "desc": "appends a long as eight bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteSByte(sbyte value)",
          "desc": "appends an sbyte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteShort(short value)",
          "desc": "appends a short as two bytes",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteString(string? value)",
          "desc": "appends a length-prefixed UTF-8 string; a null value is encoded as the zero sentinel",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteUInt(uint value)",
          "desc": "appends a uint as unchecked WriteInt",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteULong(ulong value)",
          "desc": "appends a ulong as unchecked WriteLong",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteUShort(ushort value)",
          "desc": "appends a ushort as unchecked WriteShort",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "BitMath",
      "kind": "staticClass",
      "category": "utility",
      "isAuxiliary": true,
      "desc": "internal bit utility used by ByteStream; provides HiBit(uint) returning the index of the highest set bit and NextPow2(uint) returning the smallest power of two greater than or equal to the input",
      "threadSafety": "not thread-safe"
    },
    {
      "name": "ByteStream",
      "kind": "class",
      "category": "buffer",
      "implements": [
        "IByteQueueReader",
        "IByteQueueWriter"
      ],
      "isAuxiliary": true,
      "desc": "circular byte buffer with a power-of-two capacity used by the async backend; supports Put/Set/Pop/Skip/ToArray/Clear and exposes LogicalPosition and AvailableToProcess; Pop and Skip throw IndexOutOfRangeException when the buffer is exhausted",
      "threadSafety": "not thread-safe"
    },
    {
      "name": "DefaultCtorTypeConstructorFactory",
      "kind": "class",
      "category": "factory",
      "implements": [
        "IConstructorFactory"
      ],
      "desc": "builds IConstructors for types: a public parameterless constructor yields GenericTypeConstructor<T>, otherwise a ReflectionBasedConstructor that accepts a non-public default constructor",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates the default constructor factory",
          "threadSafety": "not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "Build(Type type):IConstructor",
          "desc": "returns GenericTypeConstructor<T> when type has a public parameterless constructor, otherwise a ReflectionBasedConstructor over the public or non-public default constructor",
          "threadSafety": "not thread-safe",
          "example": "IConstructor c = new DefaultCtorTypeConstructorFactory().Build(typeof(MyData));"
        }
      ]
    },
    {
      "name": "GenericTypeConstructor<T>",
      "kind": "class",
      "category": "constructor",
      "implements": [
        "IConstructor"
      ],
      "genericConstraints": "where T : class, new()",
      "desc": "fast reflection-free IConstructor that produces new T()",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates a constructor for T",
          "threadSafety": "not thread-safe"
        }
      ],
      "properties": [
        {
          "sig": "IsValid:bool { get; }",
          "desc": "always returns true",
          "threadSafety": "not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "Construct():object?",
          "desc": "returns new T(); caller takes ownership",
          "threadSafety": "not thread-safe",
          "example": "var o = new GenericTypeConstructor<MyData>().Construct();"
        }
      ]
    },
    {
      "name": "GraphDeserializer",
      "kind": "class",
      "category": "deserializer",
      "base": "HierarchicalDeserializer",
      "desc": "deserializer that reconstructs an object graph by instance id so shared references written by GraphSerializer are restored to the same instance",
      "threadSafety": "not thread-safe",
      "limitations": "memory grows with the number of distinct instances; instances are cached by id for the lifetime of the reader",
      "constructors": [
        {
          "sig": "ctor(IReader reader)",
          "desc": "creates a monomorphic graph deserializer",
          "threadSafety": "not thread-safe",
          "example": "var des = new GraphDeserializer(reader);"
        },
        {
          "sig": "ctor(IReader reader, ITypeDeserializer typeDeserializer, ISerializerExtensionsFactory? factory = null)",
          "desc": "creates a polymorphic graph deserializer with the given type deserializer and optional extensions factory",
          "threadSafety": "not thread-safe",
          "example": "var des = new GraphDeserializer(reader, new IdBasedTypeDeserializer());"
        }
      ]
    },
    {
      "name": "GraphSerializer",
      "kind": "class",
      "category": "serializer",
      "base": "HierarchicalSerializer",
      "desc": "serializer that preserves object identity: the first write of an instance stores the full data, subsequent references write only the instance id",
      "threadSafety": "not thread-safe",
      "limitations": "identity is compared by reference; equal-but-distinct instances are serialized separately",
      "constructors": [
        {
          "sig": "ctor(IWriter writer, ITypeSerializer typeSerializer, ISerializerExtensionsFactory? factory = null, bool useAntiCorruptionSections = true)",
          "desc": "creates a polymorphic graph serializer; throws InvalidOperationException if the writer rejects section usage",
          "threadSafety": "not thread-safe",
          "example": "var s = new GraphSerializer(writer, new IdBasedTypeSerializer());"
        },
        {
          "sig": "ctor(IWriter writer, bool useAntiCorruptionSections)",
          "desc": "creates a monomorphic graph serializer with the given section usage",
          "threadSafety": "not thread-safe",
          "example": "var s = new GraphSerializer(writer, true);"
        }
      ]
    },
    {
      "name": "GuidBasedTypeDeserializer",
      "kind": "class",
      "category": "type-serialization",
      "base": "AttributeBasedTypeDeserializer<string>",
      "desc": "resolves types by the value of the GuidAttribute on IDataStruct types; the standard system primitives are recognized by their TypeCode name (Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Char, Single, Double, Decimal, String)",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates a Guid-based type deserializer",
          "threadSafety": "not thread-safe",
          "example": "var td = new GuidBasedTypeDeserializer();"
        }
      ],
      "methods": [
        {
          "sig": "Deserialize(IReader reader):Type?",
          "desc": "reads the type id list and returns the resolved Type; returns null if an id is unknown",
          "threadSafety": "not thread-safe",
          "example": "Type? t = td.Deserialize(reader);"
        }
      ]
    },
    {
      "name": "GuidBasedTypeSerializer",
      "kind": "class",
      "category": "type-serialization",
      "base": "AttributeBasedTypeSerializer<string>",
      "desc": "writes type identities as GuidAttribute values; the standard system primitives are written by their TypeCode name",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates a Guid-based type serializer",
          "threadSafety": "not thread-safe",
          "example": "var ts = new GuidBasedTypeSerializer();"
        }
      ],
      "methods": [
        {
          "sig": "Serialize(IWriter writer, Type type)",
          "desc": "writes the id list for type; throws InvalidOperationException if type lacks a GuidAttribute",
          "threadSafety": "not thread-safe",
          "example": "ts.Serialize(writer, typeof(MyData));"
        }
      ]
    },
    {
      "name": "GuidToDecimal",
      "kind": "struct",
      "category": "adapter",
      "isAuxiliary": true,
      "desc": "explicit-layout struct overlaying a Guid and a decimal at offset 0; used internally to serialize a Guid through the decimal primitive",
      "threadSafety": "not thread-safe"
    },
    {
      "name": "HierarchicalDeserializer",
      "kind": "class",
      "category": "deserializer",
      "base": "PrimitiveDeserializer",
      "implements": [
        "ISerializer"
      ],
      "desc": "the standard deserializer: reads the protocol header, resolves class types through an ITypeReader and reconstructs instances via IConstructor; supports monomorphic and polymorphic modes and raises OnException for non-fatal issues Note: it has no public constructor; construct it via From(IReader).SetMonomorphic()/SetPolymorphic().Build().",
      "threadSafety": "not thread-safe",
      "limitations": "monomorphic mode rejects runtime subtypes; polymorphic mode silently yields null (plus OnException) for unknown types",
      "properties": [
        {
          "sig": "Version:byte { get; }",
          "desc": "returns the version of the data currently being deserialized; set from the stored version byte of a versioned struct or class",
          "threadSafety": "not thread-safe",
          "example": "if (deserializer.Version == 1) { ... }"
        }
      ],
      "events": [
        {
          "sig": "event Action<Exception> OnException",
          "desc": "raised for non-fatal problems: unconsumed bytes skipped at a section boundary, an unknown or unconstructable dynamic type, or an object whose Serialize threw; subscribing allows graceful degradation",
          "threadSafety": "not thread-safe",
          "remarks": "Subscribe before calling Add* methods to capture issues that would otherwise be silent."
        }
      ],
      "methods": [
        {
          "sig": "AddClass<T>(ref T? value)",
          "desc": "deserializes a class field; reads a type marker, constructs the instance and fills it; if the section is not fully consumed OnException is raised with InvalidOperationException and the leftover bytes are skipped",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : class, IDataStruct",
          "example": "deserializer.AddClass(ref myClass);"
        },
        {
          "sig": "AddDynamic<T>(ref T value)",
          "desc": "deserializes an arbitrary type through ISerializerExtension<T>; throws InvalidOperationException if no extensions factory is configured or Construct<T> returns null",
          "threadSafety": "not thread-safe",
          "example": "deserializer.AddDynamic(ref myThing);"
        },
        {
          "sig": "AddStaticClass<T>(ref T? value)",
          "desc": "deserializes a class field with a fixed type marker; if the section is not fully consumed OnException is raised with InvalidOperationException and the leftover bytes are skipped",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : class, IDataStruct",
          "example": "deserializer.AddStaticClass(ref myClass);"
        },
        {
          "sig": "AddStruct<T>(ref T value)",
          "desc": "deserializes a struct by calling value.Serialize(this)",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : struct, IDataStruct",
          "example": "deserializer.AddStruct(ref point);"
        },
        {
          "sig": "AddVersionedStruct<T>(ref T value)",
          "desc": "reads the stored version byte, sets Version during value.Serialize(this), then restores the previous version context",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : struct, IDataStruct, IVersionedData",
          "example": "deserializer.AddVersionedStruct(ref data);"
        },
        {
          "sig": "FlushPoint():ValueTask<bool>",
          "desc": "performs a Fetch on the underlying reader and returns its result",
          "threadSafety": "not thread-safe",
          "example": "await deserializer.FlushPoint();"
        },
        {
          "sig": "From(IReader source):IHierarchicalDeserializer_Params",
          "desc": "returns a builder to configure and construct a HierarchicalDeserializer",
          "threadSafety": "not thread-safe",
          "example": "HierarchicalDeserializer.From(reader).SetMonomorphic().Build();"
        },
        {
          "sig": "Prepare()",
          "desc": "reads the protocol header and prepares the type reader; throws InvalidOperationException if the section usage recorded in the header is not supported by the reader",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Prepare(Func<int, IReadOnlyList<Type>>? defaultTypeSetProvider)",
          "desc": "reads the protocol header, the default type set version and resolves it through defaultTypeSetProvider; throws InvalidOperationException if the header requests a default type set but no provider is given or the section usage is unsupported",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "HierarchicalDeserializer_Params",
      "kind": "class",
      "category": "builder",
      "implements": [
        "IHierarchicalDeserializer_MonomorphicParams",
        "IHierarchicalDeserializer_PolymorphicParams",
        "IHierarchicalDeserializer_MonomorphicParamsView",
        "IHierarchicalDeserializer_PolymorphicParamsView"
      ],
      "desc": "builder returned by HierarchicalDeserializer.From; chaining methods SetMonomorphic/SetPolymorphic/SetCtorFactory/SetAutoPrepare are available through the interface, and Build() produces the deserializer",
      "threadSafety": "not thread-safe",
      "properties": [
        {
          "sig": "AutoPrepare:bool { get; }",
          "desc": "returns true if the header is prepared automatically on Build",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "DefaultTypeSetProvider:Func<int, IReadOnlyList<Type>>? { get; }",
          "desc": "returns the configured default type set provider or null",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ExtensionsFactory:ISerializerExtensionsFactory? { get; }",
          "desc": "returns the configured extensions factory or null",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "IsPolymorphic:bool? { get; }",
          "desc": "returns true after SetPolymorphic, false after SetMonomorphic, null before either",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ObjectConstructorFactory:IConstructorFactory? { get; }",
          "desc": "returns the configured constructor factory or null for the default",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Source:IReader { get; }",
          "desc": "returns the underlying reader",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "TypeDeserializer:ITypeDeserializer? { get; }",
          "desc": "returns the configured type deserializer or null",
          "threadSafety": "not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "AsMonomorphic():IHierarchicalDeserializer_MonomorphicParamsView?",
          "desc": "returns this as a monomorphic view when IsPolymorphic is false, otherwise null",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "AsPolymorphic():IHierarchicalDeserializer_PolymorphicParamsView?",
          "desc": "returns this as a polymorphic view when IsPolymorphic is true, otherwise null",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Build():HierarchicalDeserializer",
          "desc": "constructs the deserializer; throws InvalidOperationException if neither SetMonomorphic nor SetPolymorphic was called",
          "threadSafety": "not thread-safe",
          "example": "var des = HierarchicalDeserializer.From(reader).SetMonomorphic().Build();"
        },
        {
          "sig": "SetExtensionsFactory(ISerializerExtensionsFactory extensionsFactory):IHierarchicalDeserializer_PolymorphicParams",
          "desc": "sets the factory used by AddDynamic; returns this builder",
          "threadSafety": "not thread-safe",
          "example": "builder.SetExtensionsFactory(new SerializerExtensionsEmptyFactory());"
        }
      ]
    },
    {
      "name": "HierarchicalSerializer",
      "kind": "class",
      "category": "serializer",
      "base": "PrimitiveSerializer",
      "implements": [
        "ISerializer"
      ],
      "desc": "the standard serializer: writes a protocol header, manages class type ids (polymorphic) or fixed type markers (monomorphic) and wraps every class body in a section when sections are enabled",
      "threadSafety": "not thread-safe",
      "limitations": "monomorphic mode throws InvalidOperationException on runtime subtypes; polymorphic mode requires every serialized type to have a default constructor",
      "constructors": [
        {
          "sig": "ctor(IWriter writer, ITypeSerializer typeSerializer, ISerializerExtensionsFactory? factory = null, bool useAntiCorruptionSections = true, int defaultTypeSetVersion = 0, IReadOnlyList<Type>? defaultTypeSet = null)",
          "desc": "creates a polymorphic serializer and writes the protocol header; throws InvalidOperationException if defaultTypeSetVersion is negative or the writer rejects section usage",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "writer must have no data written yet; every type in defaultTypeSet must have a default constructor"
          ],
          "example": "var s = new HierarchicalSerializer(writer, new IdBasedTypeSerializer());"
        },
        {
          "sig": "ctor(IWriter writer, bool useAntiCorruptionSections = true)",
          "desc": "creates a monomorphic serializer (no type ids) and writes the protocol header; throws InvalidOperationException if the writer rejects section usage",
          "threadSafety": "not thread-safe",
          "example": "var s = new HierarchicalSerializer(writer);"
        }
      ],
      "properties": [
        {
          "sig": "Version:byte { get; }",
          "desc": "returns the version of the data currently being serialized; set from IVersionedData during versioned writes",
          "threadSafety": "not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "AddClass<T>(ref T? value)",
          "desc": "serializes a class field; in polymorphic mode the concrete type gets a short id (and, on first occurrence, its type is written via ITypeSerializer); throws InvalidOperationException if the runtime type has no public or non-public default constructor",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : class, IDataStruct",
          "example": "serializer.AddClass(ref myClass);"
        },
        {
          "sig": "AddDynamic<T>(ref T value)",
          "desc": "serializes an arbitrary type through ISerializerExtension<T> from the extensions factory; throws InvalidOperationException if no factory is configured or Construct<T> returns null",
          "threadSafety": "not thread-safe",
          "example": "serializer.AddDynamic(ref myThing);"
        },
        {
          "sig": "AddStaticClass<T>(ref T? value)",
          "desc": "serializes a class field using a fixed byte marker (0 = null, 1 = present); throws InvalidOperationException if value.GetType() differs from T",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : class, IDataStruct",
          "example": "serializer.AddStaticClass(ref myClass);"
        },
        {
          "sig": "AddStruct<T>(ref T value)",
          "desc": "serializes a struct by calling value.Serialize(this)",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : struct, IDataStruct",
          "example": "serializer.AddStruct(ref point);"
        },
        {
          "sig": "AddVersionedStruct<T>(ref T value)",
          "desc": "writes value.Version as a byte, sets Version during value.Serialize(this), then restores the previous version context",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : struct, IDataStruct, IVersionedData",
          "example": "serializer.AddVersionedStruct(ref data);"
        },
        {
          "sig": "FlushPoint():ValueTask<bool>",
          "desc": "flushes the underlying writer and returns true",
          "threadSafety": "not thread-safe",
          "example": "await serializer.FlushPoint();"
        },
        {
          "sig": "Prepare(bool useAntiCorruptionSections = true)",
          "desc": "writes the protocol header (protocol id 2, section usage flag, reserved byte); throws InvalidOperationException if the writer rejects section usage; must be called before any data is written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Prepare(bool useAntiCorruptionSections, int defaultTypeSetVersion, IReadOnlyList<Type>? defaultTypeSet)",
          "desc": "writes the protocol header and the default type set version; throws InvalidOperationException if defaultTypeSetVersion is negative or the writer is not polymorphic",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "IdAttribute",
      "kind": "class",
      "category": "attribute",
      "base": "Attribute",
      "desc": "declares a stable 16-bit identity for a class or struct; Value packs GroupId and IdInGroup, group 0 is reserved",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(byte groupId, byte idInGroup)",
          "desc": "creates the attribute; throws InvalidOperationException if groupId is 0 because zero group is reserved for internal usage",
          "threadSafety": "not thread-safe",
          "preconditions": [
            "groupId must not be 0"
          ],
          "example": "[Id(1, 2)] class MyData : IDataStruct { ... }"
        }
      ],
      "properties": [
        {
          "sig": "GroupId:byte { get; }",
          "desc": "returns the group id",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "IdInGroup:byte { get; }",
          "desc": "returns the id within the group",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Value:ushort { get; }",
          "desc": "returns (ushort)((GroupId << 8) | IdInGroup), the value written to the stream",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "IdBasedTypeDeserializer",
      "kind": "class",
      "category": "type-serialization",
      "base": "AttributeBasedTypeDeserializer<ushort>",
      "desc": "resolves types by the IdAttribute value on IDataStruct types; system primitives are recognized by ids 1 through 14 (Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Char, Single, Double, Decimal, String)",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an Id-based type deserializer",
          "threadSafety": "not thread-safe",
          "example": "var td = new IdBasedTypeDeserializer();"
        }
      ],
      "methods": [
        {
          "sig": "Deserialize(IReader reader):Type?",
          "desc": "reads the id list and returns the resolved Type; returns null if an id is unknown",
          "threadSafety": "not thread-safe",
          "example": "Type? t = td.Deserialize(reader);"
        }
      ]
    },
    {
      "name": "IdBasedTypeSerializer",
      "kind": "class",
      "category": "type-serialization",
      "base": "AttributeBasedTypeSerializer<ushort>",
      "desc": "writes type identities as IdAttribute values; system primitives are written by their fixed ids 1 through 14",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an Id-based type serializer",
          "threadSafety": "not thread-safe",
          "example": "var ts = new IdBasedTypeSerializer();"
        }
      ],
      "methods": [
        {
          "sig": "Serialize(IWriter writer, Type type)",
          "desc": "writes the id list for type; throws InvalidOperationException if type lacks an IdAttribute",
          "threadSafety": "not thread-safe",
          "example": "ts.Serialize(writer, typeof(MyData));"
        }
      ]
    },
    {
      "name": "NullConstructor",
      "kind": "class",
      "category": "constructor",
      "implements": [
        "IConstructor"
      ],
      "desc": "singleton constructor that always fails: used by the deserializer when a dynamic type cannot be resolved",
      "threadSafety": "not thread-safe",
      "properties": [
        {
          "sig": "Instance:IConstructor { get; }",
          "desc": "returns the shared NullConstructor instance",
          "threadSafety": "not thread-safe",
          "example": "IConstructor c = NullConstructor.Instance;"
        },
        {
          "sig": "IsValid:bool { get; }",
          "desc": "always returns false",
          "threadSafety": "not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "Construct():object?",
          "desc": "returns null",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "PrimitiveDeserializer",
      "kind": "class",
      "category": "deserializer",
      "implements": [
        "IPrimitiveSerializer"
      ],
      "desc": "base deserializer that reads every supported primitive from an IReader; Guid is stored as a decimal, DateTime via ToBinary and TimeSpan via Ticks",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(IReader reader)",
          "desc": "creates a primitive deserializer over reader",
          "threadSafety": "not thread-safe",
          "example": "var p = new PrimitiveDeserializer(reader);"
        }
      ],
      "properties": [
        {
          "sig": "IsWriter:bool { get; }",
          "desc": "always returns false",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Reader:ILowLevelReader { get; }",
          "desc": "returns the underlying reader",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Writer:ILowLevelWriter { get; }",
          "desc": "throws InvalidOperationException; a deserializer has no writer",
          "threadSafety": "not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "Add(ref DateTime value)",
          "desc": "reads a long and restores the DateTime via DateTime.FromBinary",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref Guid value)",
          "desc": "reads a decimal and reinterprets it as a Guid",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref TimeSpan value)",
          "desc": "reads a long and restores the TimeSpan via TimeSpan.FromTicks",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref bool value)",
          "desc": "reads a bool into value",
          "threadSafety": "not thread-safe",
          "example": "deserializer.Add(ref flag);"
        },
        {
          "sig": "Add(ref byte value)",
          "desc": "reads a byte into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref byte[]? value)",
          "desc": "reads a nullable byte array into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref char value)",
          "desc": "reads a char into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref decimal value)",
          "desc": "reads a decimal into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref double value)",
          "desc": "reads a double into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref float value)",
          "desc": "reads a float into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref int value)",
          "desc": "reads an int into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref long value)",
          "desc": "reads a long into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref sbyte value)",
          "desc": "reads an sbyte into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref short value)",
          "desc": "reads a short into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref string? value)",
          "desc": "reads a nullable string into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref uint value)",
          "desc": "reads a uint into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref ulong value)",
          "desc": "reads a ulong into value",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref ushort value)",
          "desc": "reads a ushort into value",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "PrimitiveSerializer",
      "kind": "class",
      "category": "serializer",
      "implements": [
        "IPrimitiveSerializer"
      ],
      "desc": "base serializer that writes every supported primitive to an IWriter; Guid is stored as a decimal, DateTime via ToBinary and TimeSpan via Ticks",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(IWriter writer)",
          "desc": "creates a primitive serializer over writer",
          "threadSafety": "not thread-safe",
          "example": "var p = new PrimitiveSerializer(writer);"
        }
      ],
      "properties": [
        {
          "sig": "IsWriter:bool { get; }",
          "desc": "always returns true",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Reader:ILowLevelReader { get; }",
          "desc": "throws InvalidOperationException; a serializer has no reader",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Writer:ILowLevelWriter { get; }",
          "desc": "returns the underlying writer",
          "threadSafety": "not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "Add(ref DateTime value)",
          "desc": "writes value.ToBinary() as a long",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref Guid value)",
          "desc": "reinterprets the Guid as a decimal and writes it",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref TimeSpan value)",
          "desc": "writes value.Ticks as a long",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref bool value)",
          "desc": "writes a bool",
          "threadSafety": "not thread-safe",
          "example": "serializer.Add(ref flag);"
        },
        {
          "sig": "Add(ref byte value)",
          "desc": "writes a byte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref byte[]? value)",
          "desc": "writes a nullable byte array",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref char value)",
          "desc": "writes a char",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref decimal value)",
          "desc": "writes a decimal",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref double value)",
          "desc": "writes a double",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref float value)",
          "desc": "writes a float",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref int value)",
          "desc": "writes an int",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref long value)",
          "desc": "writes a long",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref sbyte value)",
          "desc": "writes an sbyte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref short value)",
          "desc": "writes a short",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref string? value)",
          "desc": "writes a nullable string",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref uint value)",
          "desc": "writes a uint",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref ulong value)",
          "desc": "writes a ulong",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Add(ref ushort value)",
          "desc": "writes a ushort",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "ReaderWriterStream",
      "kind": "class",
      "category": "adapter",
      "implements": [
        "IReader",
        "IWriter"
      ],
      "desc": "in-memory bidirectional adapter that stores each written value in a per-type queue and returns it on the matching read; useful for testing serialization logic without a byte transport",
      "threadSafety": "not thread-safe",
      "limitations": "not a wire format (values are kept typed, not encoded); sections unsupported; Fetch/Flush/BeginSection/EndSection are only reachable through the interface and are no-ops",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty reader/writer",
          "threadSafety": "not thread-safe",
          "example": "var rw = new ReaderWriterStream();"
        }
      ],
      "properties": [
        {
          "sig": "IsCompleted:bool { get; }",
          "desc": "always returns false",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "IsEmpty:bool { get; }",
          "desc": "returns true when every internal queue is empty",
          "threadSafety": "not thread-safe",
          "example": "if (rw.IsEmpty) { return; }"
        }
      ],
      "methods": [
        {
          "sig": "Clear()",
          "desc": "empties all internal queues",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "Complete():ValueTask",
          "desc": "no-op; returns immediately",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadArray():byte[]?",
          "desc": "returns the next queued byte array; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadBool():bool",
          "desc": "returns the next queued bool; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadByte():byte",
          "desc": "returns the next queued byte; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadBytes(byte[] dst, int offset, int count)",
          "desc": "copies the next queued byte sequence into dst; throws Exception if the queued length differs from count",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadChar():char",
          "desc": "returns the next queued char; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadDecimal():decimal",
          "desc": "returns the next queued decimal; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadDouble():double",
          "desc": "returns the next queued double; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadFloat():float",
          "desc": "returns the next queued float; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadInt():int",
          "desc": "returns the next queued int; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadLong():long",
          "desc": "returns the next queued long; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadSByte():sbyte",
          "desc": "returns the next queued sbyte; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadShort():short",
          "desc": "returns the next queued short; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadString():string?",
          "desc": "returns the next queued string; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadUInt():uint",
          "desc": "returns the next queued uint; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadULong():ulong",
          "desc": "returns the next queued ulong; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "ReadUShort():ushort",
          "desc": "returns the next queued ushort; throws InvalidOperationException if none was written",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "TrySetSectionUsage(bool useSections):bool",
          "desc": "returns true for false and false for true; sections are not supported",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteArray(byte[]? value)",
          "desc": "queues a byte array reference (holds a reference only)",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteBool(bool value)",
          "desc": "queues a bool",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteByte(byte value)",
          "desc": "queues a byte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteBytes(byte[] value, int offset, int count)",
          "desc": "copies count bytes into a new array and queues it",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteChar(char value)",
          "desc": "queues a char",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteDecimal(decimal value)",
          "desc": "queues a decimal",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteDouble(double value)",
          "desc": "queues a double",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteFloat(float value)",
          "desc": "queues a float",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteInt(int value)",
          "desc": "queues an int",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteLong(long value)",
          "desc": "queues a long",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteSByte(sbyte value)",
          "desc": "queues an sbyte",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteShort(short value)",
          "desc": "queues a short",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteString(string? value)",
          "desc": "queues a string reference",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteUInt(uint value)",
          "desc": "queues a uint",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteULong(ulong value)",
          "desc": "queues a ulong",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "WriteUShort(ushort value)",
          "desc": "queues a ushort",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "ReflectionBasedConstructor",
      "kind": "class",
      "category": "constructor",
      "implements": [
        "IConstructor"
      ],
      "desc": "IConstructor that finds the public or non-public default constructor of a type via reflection",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor(Type type)",
          "desc": "searches for a public or non-public default constructor of type",
          "threadSafety": "not thread-safe",
          "example": "var c = new ReflectionBasedConstructor(typeof(MyData));"
        }
      ],
      "properties": [
        {
          "sig": "IsValid:bool { get; }",
          "desc": "returns true if a default constructor was found",
          "threadSafety": "not thread-safe"
        }
      ],
      "methods": [
        {
          "sig": "Construct():object?",
          "desc": "invokes the found constructor; returns null if no default constructor exists",
          "threadSafety": "not thread-safe"
        }
      ]
    },
    {
      "name": "SerializerExtensionsEmptyFactory",
      "kind": "class",
      "category": "factory",
      "implements": [
        "ISerializerExtensionsFactory"
      ],
      "desc": "no-op extensions factory that always returns null from Construct and never raises OnError; usable wherever an ISerializerExtensionsFactory is required but no dynamic extensions are needed",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates an empty extensions factory",
          "threadSafety": "not thread-safe",
          "example": "var factory = new SerializerExtensionsEmptyFactory();"
        }
      ],
      "properties": [
        {
          "sig": "Instance:ISerializerExtensionsFactory { get; }",
          "desc": "returns the shared empty factory instance",
          "threadSafety": "not thread-safe",
          "example": "ISerializerExtensionsFactory f = SerializerExtensionsEmptyFactory.Instance;"
        }
      ]
    },
    {
      "name": "TypenameBasedTypeDeserializer",
      "kind": "class",
      "category": "type-serialization",
      "implements": [
        "ITypeDeserializer"
      ],
      "desc": "resolves types by their assembly-qualified name read from the stream via Type.GetType",
      "threadSafety": "not thread-safe",
      "limitations": "type names are brittle across assembly renames, moves and obfuscation; requires the assembly to be loaded",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates a typename-based type deserializer",
          "threadSafety": "not thread-safe",
          "example": "var td = new TypenameBasedTypeDeserializer();"
        }
      ],
      "methods": [
        {
          "sig": "Deserialize(IReader reader):Type?",
          "desc": "reads a string and resolves it via Type.GetType; returns null if the string was null or the type cannot be resolved",
          "threadSafety": "not thread-safe",
          "example": "Type? t = td.Deserialize(reader);"
        }
      ]
    },
    {
      "name": "TypenameBasedTypeSerializer",
      "kind": "class",
      "category": "type-serialization",
      "implements": [
        "ITypeSerializer"
      ],
      "desc": "writes a type as its assembly-qualified name",
      "threadSafety": "not thread-safe",
      "constructors": [
        {
          "sig": "ctor()",
          "desc": "creates a typename-based type serializer",
          "threadSafety": "not thread-safe",
          "example": "var ts = new TypenameBasedTypeSerializer();"
        }
      ],
      "methods": [
        {
          "sig": "Serialize(IWriter writer, Type type)",
          "desc": "writes type.AssemblyQualifiedName; throws InvalidOperationException if the name is null",
          "threadSafety": "not thread-safe",
          "example": "ts.Serialize(writer, typeof(MyData));"
        }
      ]
    }
  ],
  "delegates": [
    {
      "name": "ISerializer_AddMethod<T>",
      "sig": "void ISerializer_AddMethod<T>(ISerializer serializer, ref T value)",
      "desc": "callback that serializes one non-primitive element; passed to the AddArray/AddList/AddDictionary overloads that take an addValue delegate; lambda bodies cannot use ref parameters so pass an anonymous delegate or a method group"
    }
  ],
  "extensions": [
    {
      "for": "IPrimitiveClassSerializer<T>",
      "members": [
        {
          "sig": "Add<T>(ref T value, Func<T> defaultValue)",
          "desc": "serializes a non-null class reference; on the reader, if the stored value is null it is replaced with defaultValue(); if the writer is used the defaultValue is ignored",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : class",
          "example": "serializer.Add(ref name, () => \"unknown\");"
        },
        {
          "sig": "AddArray<TValue>(ref TValue[] array, Func<TValue[]> defaultValue, Func<TValue> defaultElementValue)",
          "desc": "serializes a non-null class array; on the reader a null array is replaced with defaultValue() and a null element is replaced with defaultElementValue()",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where TValue : class",
          "example": "serializer.AddArray(ref arr, () => Array.Empty<string>(), () => \"\");"
        },
        {
          "sig": "AddArray<TValue>(ref TValue[]? array, Func<TValue> defaultValue)",
          "desc": "serializes a nullable class array; writes int count (or -1 for null); on the reader a null element is replaced with defaultValue()",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where TValue : class"
        },
        {
          "sig": "AddList<TValue>(ref List<TValue> list, Func<List<TValue>> defaultValue, Func<TValue> defaultElementValue)",
          "desc": "serializes a non-null class list; on the reader a null list is replaced with defaultValue() and a null element is replaced with defaultElementValue()",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where TValue : class"
        },
        {
          "sig": "AddList<TValue>(ref List<TValue>? list, Func<TValue> defaultValue)",
          "desc": "serializes a nullable class list; writes int count (or -1 for null); on the reader a null element is replaced with defaultValue()",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where TValue : class"
        }
      ]
    },
    {
      "for": "IPrimitiveSerializer<T>",
      "members": [
        {
          "sig": "Add<T>(ref T? value)",
          "desc": "serializes a nullable value type; writes a bool null sentinel followed by the value when present; on the reader a true sentinel sets value to null",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : struct",
          "example": "serializer.Add(ref maybeCount);"
        },
        {
          "sig": "AddArray<TValue>(ref TValue[] array, Func<TValue[]> defaultValue)",
          "desc": "serializes a non-null value-type array; on the reader a null array is replaced with defaultValue()",
          "threadSafety": "not thread-safe",
          "example": "serializer.AddArray(ref arr, () => Array.Empty<int>());"
        },
        {
          "sig": "AddArray<TValue>(ref TValue[]? array)",
          "desc": "serializes a nullable value-type array; writes int count (or -1 for null); on the reader count 0 yields Array.Empty and a negative count yields null",
          "threadSafety": "not thread-safe",
          "example": "serializer.AddArray(ref arr);"
        },
        {
          "sig": "AddDictionary<TKey, TValue, TSerializer>(ref Dictionary<TKey, TValue> dictionary, Func<Dictionary<TKey, TValue>> defaultValue)",
          "desc": "serializes a non-null dictionary; on the reader a null dictionary is replaced with defaultValue()",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where TKey : notnull, TSerializer : IPrimitiveSerializer<TKey>, IPrimitiveSerializer<TValue>"
        },
        {
          "sig": "AddDictionary<TKey, TValue, TSerializer>(ref Dictionary<TKey, TValue>? dictionary)",
          "desc": "serializes a nullable dictionary; writes int count (or -1 for null); keys and values are written in enumeration order and re-added with Dictionary.Add",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where TKey : notnull, TSerializer : IPrimitiveSerializer<TKey>, IPrimitiveSerializer<TValue>"
        },
        {
          "sig": "AddList<TValue>(ref List<TValue> list, Func<List<TValue>> defaultValue)",
          "desc": "serializes a non-null value-type list; on the reader a null list is replaced with defaultValue()",
          "threadSafety": "not thread-safe",
          "example": "serializer.AddList(ref list, () => new List<int>());"
        },
        {
          "sig": "AddList<TValue>(ref List<TValue>? list)",
          "desc": "serializes a nullable value-type list; writes int count (or -1 for null)",
          "threadSafety": "not thread-safe",
          "example": "serializer.AddList(ref list);"
        }
      ]
    },
    {
      "for": "IPrimitiveSerializer<T?>",
      "members": [
        {
          "sig": "Add<T>(ref IReadOnlyList<T?>? list)",
          "desc": "serializes a nullable IReadOnlyList of nullable primitives; writes int count + 1 where 0 encodes null and 1 encodes empty; on the reader the list is materialized as a T?[] array",
          "threadSafety": "not thread-safe",
          "remarks": "This encoding differs from AddArray/AddList which use -1 for null; do not mix the two on the same field across versions."
        }
      ]
    },
    {
      "for": "ISerializer",
      "members": [
        {
          "sig": "Add<T>(ref T? value)",
          "desc": "serializes a nullable IDataStruct value type; writes a bool null sentinel followed by the struct when present",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : struct, IDataStruct"
        },
        {
          "sig": "AddArray<TValue>(ref TValue[] array, Func<TValue[]> defaultValue, ISerializer_AddMethod<TValue> addValue)",
          "desc": "serializes a non-null array of non-primitive elements using addValue for each element; on the reader a null array is replaced with defaultValue()",
          "threadSafety": "not thread-safe",
          "example": "serializer.AddArray(ref arr, () => Array.Empty<MyData>(), delegate (ISerializer s, ref MyData v) { s.AddStruct(ref v); });"
        },
        {
          "sig": "AddArray<TValue>(ref TValue[]? array, ISerializer_AddMethod<TValue> addValue)",
          "desc": "serializes a nullable array of non-primitive elements; writes int count (or -1 for null); each element is serialized with addValue",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "AddClass<T>(ref T value, Func<T> defaultValue)",
          "desc": "serializes a non-null IDataStruct class; on the reader a null stored value is replaced with defaultValue()",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : class, IDataStruct",
          "example": "serializer.AddClass(ref obj, () => new MyData());"
        },
        {
          "sig": "AddDictionary<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary, Func<Dictionary<TKey, TValue>> defaultValue, ISerializer_AddMethod<TKey> addKey, ISerializer_AddMethod<TValue> addValue)",
          "desc": "serializes a non-null dictionary with delegate-based key and value serialization; on the reader a null dictionary is replaced with defaultValue()",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where TKey : notnull"
        },
        {
          "sig": "AddDictionary<TKey, TValue>(ref Dictionary<TKey, TValue>? dictionary, ISerializer_AddMethod<TKey> addKey, ISerializer_AddMethod<TValue> addValue)",
          "desc": "serializes a nullable dictionary with delegate-based key and value serialization; writes int count (or -1 for null)",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where TKey : notnull"
        },
        {
          "sig": "AddList<TValue>(ref List<TValue> list, Func<List<TValue>> defaultValue, ISerializer_AddMethod<TValue> addValue)",
          "desc": "serializes a non-null list of non-primitive elements using addValue for each element; on the reader a null list is replaced with defaultValue()",
          "threadSafety": "not thread-safe",
          "example": "serializer.AddList(ref list, () => new List<MyData>(), delegate (ISerializer s, ref MyData v) { s.AddStruct(ref v); });"
        },
        {
          "sig": "AddList<TValue>(ref List<TValue>? list, ISerializer_AddMethod<TValue> addValue)",
          "desc": "serializes a nullable list of non-primitive elements; writes int count (or -1 for null); each element is serialized with addValue",
          "threadSafety": "not thread-safe"
        },
        {
          "sig": "AddStaticClass<T>(ref T value, Func<T> defaultValue)",
          "desc": "serializes a non-null IDataStruct class without a type id; on the reader a null stored value is replaced with defaultValue()",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : class, IDataStruct"
        }
      ]
    },
    {
      "for": "ISerializerCore",
      "members": [
        {
          "sig": "Add<T>(ref T value)",
          "desc": "serializes an enum of any integer base type; writes and reads the underlying value via the matching WriteByte/Short/Int/Long family; throws InvalidOperationException for unsupported TypeCodes (such as Boolean and Char)",
          "threadSafety": "not thread-safe",
          "genericConstraints": "where T : struct, Enum",
          "example": "serializer.Add(ref myEnum);"
        }
      ]
    }
  ],
  "gotchas": [
    "AddArray/AddList/AddDictionary use -1 as the null sentinel but ISerializer_Containers.Add encodes null as 0 and empty as 1; mixing the two corrupts data.",
    "AddDynamic requires the serializer to be configured with an ISerializerExtensionsFactory, otherwise it throws InvalidOperationException.",
    "Archivarius is not thread-safe: all readers, writers and serializers are single-use, single-threaded objects.",
    "Attribute-based type serializers encode generic arguments recursively; more than 255 ids in one type throws OverflowException.",
    "BinaryReader and BinaryWriter default _useSections to true even though the interface contract says sections are disabled by default; always call TrySetSectionUsage explicitly before writing or reading.",
    "BinaryStreamWriter, AsyncBufferedWriter and ReaderWriterStream reject TrySetSectionUsage(true); using sections with them throws InvalidOperationException from the serializer header preparation.",
    "Complete() does not enforce the documented all-later-calls-throw contract in BinaryWriter, BinaryStreamWriter and AsyncBufferedWriter; they remain usable after Complete.",
    "Endianness is host-dependent; the explicit-layout conversions write bytes in native order, so data is not portable across endianness.",
    "Fetch() must be called before reading and paired 1-to-1 with writer Flush() calls; IWriter.Complete counts as one Flush, and a Fetch returning true does not guarantee new bytes.",
    "IsCompleted is always false in BinaryReader, BinaryStreamReader and AsyncBufferedReader; it never transitions to true.",
    "ReaderWriterStream stores values in separate per-type queues; a read of a type that was never written throws InvalidOperationException from Queue.Dequeue, and ReadBytes throws a plain Exception on a length mismatch.",
    "The Monomorphic (non-polymorphic) serializer forbids runtime subtypes: AddClass with value.GetType() different from typeof(T) throws InvalidOperationException.",
    "The in-memory and stream reader implementations throw InvalidOperationException when reads exceed the available data, not the EndOfStreamException documented on the interface; only ReadString/ReadArray/ReadBytes on BinaryStreamReader throw EndOfStreamException.",
    "string is encoded as a UTF-8 byte-length prefix plus 1 (0 means null), and byte[] uses the same scheme; both differ from the raw, un-length-prefixed WriteBytes."
  ],
  "commonMistakes": [
    "Calling Fetch() multiple times for a single writer Flush(), or reading before Fetch(), produces undefined behaviour; keep Flush/Fetch counts equal and in lockstep.",
    "Forgetting to call serializer.Add(ref field) in the same order on the writer and the reader; the format is positional and has no field names.",
    "Mixing section-enabled writers with section-rejecting readers (or vice versa); the serializer throws InvalidOperationException during Prepare because TrySetSectionUsage failed.",
    "Mutating a byte[] passed to BinaryReader or retained from GetBufferUnsafe while it is still in use; the reader holds a reference, not a copy.",
    "Serializing a class in polymorphic mode without a public or non-public default constructor; the writer throws InvalidOperationException and the reader reports OnException and yields null.",
    "Using GraphSerializer with objects that override Equals/GetHashCode; graph identity is compared by reference, so override-based equality does not merge instances."
  ],
  "patterns": [
    {
      "goal": "Basic in-memory round-trip of a struct data structure",
      "code": "BinaryWriter writer = new BinaryWriter();HierarchicalSerializer serializer = new HierarchicalSerializer(writer);MyData data = new MyData { Value = 42 };serializer.AddStruct(ref data);await serializer.FlushPoint();byte[] bytes = writer.GetBuffer();BinaryReader reader = new BinaryReader(bytes);HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(reader).SetMonomorphic().Build();MyData copy = new MyData();deserializer.AddStruct(ref copy);"
    },
    {
      "goal": "Configuring polymorphic serialization with attribute-based type ids",
      "code": "IdBasedTypeSerializer ts = new IdBasedTypeSerializer();ts.Add(typeof(Derived));BinaryWriter writer = new BinaryWriter();HierarchicalSerializer serializer = new HierarchicalSerializer(writer, ts);Derived d = new Derived();serializer.AddClass(ref d);await serializer.FlushPoint();BinaryReader reader = new BinaryReader(writer.GetBuffer());IdBasedTypeDeserializer td = new IdBasedTypeDeserializer();td.Add(typeof(Derived));HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(reader).SetPolymorphic(td).Build();Derived? copy = null;deserializer.AddClass(ref copy);"
    },
    {
      "goal": "Forward compatibility using versioned data with a version switch",
      "code": "class V1Data : IDataStruct, IVersionedData { public byte Version => 1; public int X; public string? Name; public void Serialize(ISerializer s) { s.Add(ref X); if (s.IsWriter || s.Version >= 1) { s.Add(ref Name); } } }serializer.AddVersionedStruct(ref data);"
    },
    {
      "goal": "Graceful degradation when a dynamic type cannot be constructed on deserialization",
      "code": "HierarchicalDeserializer deserializer = HierarchicalDeserializer.From(reader).SetPolymorphic(new IdBasedTypeDeserializer().Add(typeof(Base))).Build();deserializer.OnException += ex => Console.WriteLine(ex.Message);Base? result = null;deserializer.AddClass(ref result);Base safe = result ?? new Base();"
    },
    {
      "goal": "Graceful handling of unexpected extra bytes in a class section",
      "code": "deserializer.OnException += ex => { if (ex is InvalidOperationException) { Console.WriteLine(\"Unknown or extra data skipped: \" + ex.Message); } };MyData? value = null;deserializer.AddClass(ref value);MyData result = value ?? new MyData();"
    },
    {
      "goal": "Preserving shared references with the graph serializer",
      "code": "GraphSerializer serializer = new GraphSerializer(writer, new IdBasedTypeSerializer());TreeNode root = new TreeNode();root.Child = root;serializer.AddClass(ref root);await serializer.FlushPoint();GraphDeserializer deserializer = new GraphDeserializer(reader, new IdBasedTypeDeserializer());TreeNode? copy = null;deserializer.AddClass(ref copy);bool sameRef = copy != null && ReferenceEquals(copy, copy.Child);"
    },
    {
      "goal": "Serializing a list of non-primitive objects with a delegate",
      "code": "List<MyData> list = new List<MyData> { new MyData() };serializer.AddList(ref list, () => new List<MyData>(), delegate (ISerializer s, ref MyData v) { s.AddStruct(ref v); });"
    },
    {
      "goal": "Streaming framed data with the async buffered backend",
      "code": "byte[] header = new byte[4];Func<IByteQueueReader, ValueTask> send = async q => { q.Pop(header, 0, 4); int size = BitConverter.ToInt32(header, 0); byte[] payload = new byte[size]; q.Pop(payload, 0, size); await transport.SendAsync(payload); };AsyncBufferedWriter writer = new AsyncBufferedWriter(send);writer.WriteInt(42);await writer.Flush();"
    }
  ],
  "relationships": [
    {
      "from": "AsyncBufferedReader",
      "to": "IByteQueueWriter",
      "rel": "takes Func<IByteQueueWriter, int, ValueTask> in constructor"
    },
    {
      "from": "AsyncBufferedReader",
      "to": "IReader",
      "rel": "implements"
    },
    {
      "from": "AsyncBufferedWriter",
      "to": "IByteQueueReader",
      "rel": "takes Func<IByteQueueReader, ValueTask> in constructor"
    },
    {
      "from": "AsyncBufferedWriter",
      "to": "IWriter",
      "rel": "implements"
    },
    {
      "from": "BinaryReader",
      "to": "IReader",
      "rel": "implements"
    },
    {
      "from": "BinaryStreamReader",
      "to": "IReader",
      "rel": "implements"
    },
    {
      "from": "BinaryStreamWriter",
      "to": "IWriter",
      "rel": "implements"
    },
    {
      "from": "BinaryWriter",
      "to": "IWriter",
      "rel": "implements"
    },
    {
      "from": "DefaultCtorTypeConstructorFactory",
      "to": "IConstructor",
      "rel": "Build returns GenericTypeConstructor<T> or ReflectionBasedConstructor"
    },
    {
      "from": "DefaultCtorTypeConstructorFactory",
      "to": "IConstructorFactory",
      "rel": "implements"
    },
    {
      "from": "GenericTypeConstructor",
      "to": "IConstructor",
      "rel": "implements"
    },
    {
      "from": "GraphDeserializer",
      "to": "HierarchicalDeserializer",
      "rel": "extends"
    },
    {
      "from": "GraphSerializer",
      "to": "HierarchicalSerializer",
      "rel": "extends"
    },
    {
      "from": "GuidBasedTypeDeserializer",
      "to": "AttributeBasedTypeDeserializer",
      "rel": "extends"
    },
    {
      "from": "GuidBasedTypeSerializer",
      "to": "AttributeBasedTypeSerializer",
      "rel": "extends"
    },
    {
      "from": "HierarchicalDeserializer",
      "to": "ISerializer",
      "rel": "implements"
    },
    {
      "from": "HierarchicalDeserializer",
      "to": "PrimitiveDeserializer",
      "rel": "extends"
    },
    {
      "from": "HierarchicalDeserializer.From",
      "to": "IHierarchicalDeserializer_Params",
      "rel": "returns builder from From(IReader)"
    },
    {
      "from": "HierarchicalDeserializer_Params",
      "to": "HierarchicalDeserializer",
      "rel": "produced by Build()"
    },
    {
      "from": "HierarchicalSerializer",
      "to": "ISerializer",
      "rel": "implements"
    },
    {
      "from": "HierarchicalSerializer",
      "to": "PrimitiveSerializer",
      "rel": "extends"
    },
    {
      "from": "IDataStruct",
      "to": "ISerializer",
      "rel": "Serialize receives ISerializer"
    },
    {
      "from": "ISerializer",
      "to": "ILowLevelReader",
      "rel": "exposes via Reader property"
    },
    {
      "from": "ISerializer",
      "to": "ILowLevelWriter",
      "rel": "exposes via Writer property"
    },
    {
      "from": "IdBasedTypeDeserializer",
      "to": "AttributeBasedTypeDeserializer",
      "rel": "extends"
    },
    {
      "from": "IdBasedTypeSerializer",
      "to": "AttributeBasedTypeSerializer",
      "rel": "extends"
    },
    {
      "from": "NullConstructor",
      "to": "IConstructor",
      "rel": "implements"
    },
    {
      "from": "PrimitiveDeserializer",
      "to": "IReader",
      "rel": "takes IReader in constructor"
    },
    {
      "from": "PrimitiveSerializer",
      "to": "IWriter",
      "rel": "takes IWriter in constructor"
    },
    {
      "from": "ReaderWriterStream",
      "to": "IReader",
      "rel": "implements"
    },
    {
      "from": "ReaderWriterStream",
      "to": "IWriter",
      "rel": "implements"
    },
    {
      "from": "ReflectionBasedConstructor",
      "to": "IConstructor",
      "rel": "implements"
    },
    {
      "from": "SerializerExtensionsEmptyFactory",
      "to": "ISerializerExtensionsFactory",
      "rel": "implements"
    },
    {
      "from": "TypenameBasedTypeDeserializer",
      "to": "ITypeDeserializer",
      "rel": "implements"
    },
    {
      "from": "TypenameBasedTypeSerializer",
      "to": "ITypeSerializer",
      "rel": "implements"
    }
  ]
}
