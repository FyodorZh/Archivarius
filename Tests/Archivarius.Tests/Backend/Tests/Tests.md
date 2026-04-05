# IWriter / IReader Contract Test Suite

This document describes every test case in `Check_IWriter_IReader` — an abstract NUnit fixture
that verifies the full `IWriter`/`IReader` contract.

## How to use

Create a concrete subclass for each writer/reader pair you want to test:

```csharp
[TestFixture]
public class Check_BinaryWriter_BinaryReader : Check_IWriter_IReader
{
    protected override Task<(IWriter, IReader, bool)> GetWriterReaderPair()
    {
        var writer = new BinaryWriter();
        var reader = new BinaryReader(Array.Empty<byte>());
        // connect them …
        return Task.FromResult<(IWriter, IReader, bool)>(
            (writer, reader, typeSafety: false));
    }
}
```

Section-support capability is read automatically from `IWriter.SectionsSupport` /
`IReader.SectionsSupport` on the returned objects — both always expose the same value.

### Capability flags

| Flag | Type | Meaning |
|---|---|---|
| `IWriter.SectionsSupport` | `IAnyBackendCapabilities.CapabilityState` | Read from the writer/reader. Controls which section test paths execute (see below). |
| `typeSafety` | `bool` | Every type mismatch throws `InvalidDataException` immediately. Type-safety tests run. |

#### `SectionsSupport` values

| Value | Meaning |
|---|---|
| `NotSupported` | Sections are never active. `TrySetSectionUsage(true)` → `false`; `TrySetSectionUsage(false)` → `true`. `BeginSection`/`EndSection` are no-ops (but LIFO still enforced). Only *disabled-sections* tests run. |
| `Supported` | Sections can be toggled. `TrySetSectionUsage(true/false)` both return `true`. Initial state is **undefined** — callers must always set the mode explicitly before I/O. Both *disabled* and *enabled* test paths run. |
| `Mandatory` | Sections are permanently on. `TrySetSectionUsage(true)` → `true`; `TrySetSectionUsage(false)` → `false`. Only *enabled-sections* tests run. |

Tests that require a particular sections state call `EnableSections(writer, reader)` or
`EnsureSectionsDisabled(writer, reader)` as appropriate. Both helpers read `writer.SectionsSupport`
internally and call `Assert.Ignore(...)` when the capability does not allow the required mode.

---

## File: `Check_IWriter_IReader.cs` — Defaults (13 tests)

| Test | Description |
|---|---|
| `Defaults_Writer_TrySetSectionUsageFalse_MatchesSectionsSupport` | `TrySetSectionUsage(false)` returns `true` for `NotSupported`/`Supported`; returns `false` for `Mandatory` (cannot disable mandatory sections). |
| `Defaults_Reader_TrySetSectionUsageFalse_MatchesSectionsSupport` | Same check on the reader side. |
| `Defaults_Writer_TrySetSectionUsageTrue_MatchesSectionsSupport` | `TrySetSectionUsage(true)` returns `false` for `NotSupported`; returns `true` for `Supported`/`Mandatory`. |
| `Defaults_Reader_TrySetSectionUsageTrue_MatchesSectionsSupport` | Same check on the reader side. |
| `Defaults_Reader_IsCompleted_FalseBeforeAnyFetch` | `IsCompleted` is `false` right after construction. |
| `Defaults_Fetch_ReturnsTrueAfterComplete` | First `Fetch()` after `Complete()` returns `true`. |
| `Defaults_Fetch_ReturnsFalseAfterDataConsumedAndComplete` | Second `Fetch()` after data is consumed and writer is complete returns `false`. |
| `Defaults_IsCompleted_TrueAfterTerminalFetch` | `IsCompleted` flips to `true` only after `Fetch()` returns `false`. |
| `Defaults_Reader_EndSection_WithoutBeginSection_ThrowsInvalidOp` | `EndSection()` without matching `BeginSection()` always throws on the reader. |
| `Defaults_Writer_EndSection_WithoutBeginSection_ThrowsInvalidOp` | Same on the writer. |
| `Defaults_SectionsDisabledByDefault_BeginEndAreNoop_BehavioralCheck` | **`NotSupported` only.** With no `TrySetSectionUsage` call, `BeginSection`/`EndSection` on the writer inject no extra bytes into the stream. Skipped for `Supported` (initial state undefined) and `Mandatory` (always on). |
| `Defaults_Writer_ExtraEndSection_AfterBalancedPair_Throws` | A second `EndSection()` after a balanced pair throws. |
| `Defaults_Reader_ExtraEndSection_AfterBalancedPair_Throws` | Same on the reader side. |

---

## File: `Check_IWriter_IReader.Primitives.cs` — Primitive round-trips (63 tests)

All tests follow the pattern: write one value → `FlushFetch` → read back → assert equality.

| Test | Description |
|---|---|
| `Primitives_Bool_True` / `…False` | Round-trip `bool`. |
| `Primitives_Byte_MinValue` / `…MidValue` / `…MaxValue` | Round-trip `byte` at 0, 128, 255. |
| `Primitives_SByte_MinValue` / `…NegativeOne` / `…Zero` / `…MaxValue` | Round-trip `sbyte` at extremes. |
| `Primitives_Char_NullChar` / `…AsciiLetter` / `…HighUnicode` / `…MaxValue` | Round-trip `char` including `'\0'`, `'A'`, CJK, `\uFFFF`. |
| `Primitives_Short_*` (4) | Round-trip `short` at min, −1, 0, max. |
| `Primitives_UShort_*` (3) | Round-trip `ushort` at 0, 32768, max. |
| `Primitives_Int_*` (5) | Round-trip `int` at min, −1, 0, 1, max. |
| `Primitives_UInt_*` (3) | Round-trip `uint` at 0, 2147483648, max. |
| `Primitives_Long_*` (4) | Round-trip `long` at min, −1, 0, max. |
| `Primitives_ULong_*` (3) | Round-trip `ulong` at 0, long.MaxValue+1, max. |
| `Primitives_Float_*` (9) | Round-trip `float` including zero, one, min, max, epsilon, **NaN**, **+∞**, **−∞**, **−0**. |
| `Primitives_Double_*` (9) | Same coverage for `double`. |
| `Primitives_Decimal_*` (7) | Round-trip `decimal` at 0, 1, −1, min, max, high precision, π. |
| `Primitives_String_*` (6) | Round-trip `string` null, empty, ASCII, Unicode (emoji + CJK + Cyrillic), embedded `'\0'`, 10 000-char string. |
| `Primitives_Array_*` (5) | Round-trip `byte[]` null, empty, single byte, multi-byte, 64 KB. |
| `Primitives_AllTypes_Sequential_RoundTrip` | Writes all 17 Write* methods in sequence (one Flush/Fetch), reads all back. |
| `Primitives_SameType_MultipleValues_Sequential` | 100 consecutive ints, all correct. |
| `Primitives_StringAndArrayInterleaved` | Mixed string/array null/non-null sequence to verify distinct encoding. |

---

## File: `Check_IWriter_IReader.VarLen.cs` — Variable-length encoding boundaries (12 tests)

Catches bugs in LEB128/ZigZag encoders at byte-count thresholds. Fixed-width implementations pass trivially.

| Test | Description |
|---|---|
| `VarLen_Int_KeyBoundaryValues` | 30 signed-int values at every 7-bit VarInt boundary (0, ±1, ±63/64, ±8191/8192, …, min/max). |
| `VarLen_UInt_KeyBoundaryValues` | 12 unsigned-int values (0, 1, 127, 128, 16383, …, max). |
| `VarLen_Long_KeyBoundaryValues` | Signed long at 7-bit boundaries including 56-bit threshold and extremes. |
| `VarLen_ULong_KeyBoundaryValues` | Unsigned long at 7-bit boundaries including `1<<56` and `ulong.MaxValue`. |
| `VarLen_Short_KeyBoundaryValues` | Signed short at every significant boundary. |
| `VarLen_UShort_KeyBoundaryValues` | Unsigned short boundaries. |
| `VarLen_Byte_KeyBoundaryValues` | Byte values 0, 63, 64, 127, 128, 254, 255. |
| `VarLen_SByte_KeyBoundaryValues` | SByte extremes and ±63/64 boundary. |
| `VarLen_Char_KeyBoundaryValues` | Char values crossing encoding boundaries. |
| `VarLen_Float_SpecialBitPatterns` | 13 float bit patterns: subnormals, normals, ±∞, NaN, −0, π. All verified bit-exactly (NaN checked via `float.IsNaN`). |
| `VarLen_Double_SpecialBitPatterns` | Same for 14 double values including `Math.E`. |
| `VarLen_Decimal_KeyValues` | Decimal at 0, ±1, min/max, max-precision, high-integer, repeated values with different scale. |
| `VarLen_String_LengthBoundaries` | Strings at lengths 1, 127, 128, 16383, 16384 (LEB128 length-prefix boundary). |
| `VarLen_String_MultibyteLengthCrossesLengthBoundary` | 43 × `'中'` = 129 UTF-8 bytes — crosses 128-byte boundary. |
| `VarLen_Array_LengthBoundaries` | Byte arrays at lengths 1, 127, 128, 16383, 16384. |
| `VarLen_WriteBytes_CountBoundaries` | `WriteBytes`/`ReadBytes` with counts at 1, 127, 128, 16383, 16384. |

---

## File: `Check_IWriter_IReader.Bytes.cs` — WriteBytes / ReadBytes (19 tests)

| Test | Description |
|---|---|
| `Bytes_SingleByte_RoundTrip` | 1 byte, offset 0. |
| `Bytes_MultipleBytes_RoundTrip` | 5 bytes, offset 0. |
| `Bytes_WithNonZeroWriteOffset_CorrectSliceTransmitted` | `WriteBytes(src, 2, 3)` — only bytes [2..4] are transmitted. |
| `Bytes_WithNonZeroReadOffset_BytesPlacedAtCorrectPosition` | `ReadBytes(dst, 2, 3)` — bytes land in the correct slice. |
| `Bytes_ZeroCount_Write_IsNoop` | `WriteBytes(buf, 0, 0)` writes nothing; subsequent `ReadInt` works. |
| `Bytes_ZeroCount_Read_IsNoop` | `ReadBytes(dst, 0, 0)` consumes nothing. |
| `Bytes_LargeBuffer_RoundTrip` | 100 KB buffer. |
| `Bytes_TwoConsecutivePairs_BothCorrect` | Two sequential write/read pairs of different sizes. |
| `Bytes_AllZeroContent_RoundTrip` | 32 zero bytes. |
| `Bytes_AllMaxByteContent_RoundTrip` | 32 × `0xFF`. |
| `Bytes_AlternatingPattern_RoundTrip` | Alternating `0xAA`/`0x55`. |
| `Bytes_DoesNotOverwriteBeyondOffsetPlusCount` | Guard bytes outside `[offset, offset+count)` must remain untouched. |
| `Bytes_Write_NullBuffer_ThrowsArgumentNullException` | Null buffer → `ArgumentNullException`. |
| `Bytes_Write_NegativeOffset_ThrowsArgumentOutOfRangeException` | Negative offset → `ArgumentOutOfRangeException`. |
| `Bytes_Write_NegativeCount_ThrowsArgumentOutOfRangeException` | Negative count → `ArgumentOutOfRangeException`. |
| `Bytes_Write_OffsetPlusCountExceedsBuffer_ThrowsArgumentException` | `offset + count > buffer.Length` → `ArgumentException`. |
| `Bytes_Write_ZeroCount_NullBuffer_StillThrowsArgumentNullException` | Null check precedes the count=0 no-op shortcut. |
| `Bytes_Read_NullBuffer_*` / `Read_NegativeOffset_*` / `Read_NegativeCount_*` / `Read_Overflow_*` | Mirror set of argument-validation tests for `ReadBytes`. |
| `Bytes_Read_BeyondEnd_ThrowsEndOfStreamException` | Writing 2 bytes and reading 4 must throw `EndOfStreamException`. |

---

## File: `Check_IWriter_IReader.Sections.cs` — Section behavior (28 tests)

### Sections-disabled tests (7 tests)
Run for `NotSupported` (always disabled) and `Supported` (explicitly disabled via `TrySetSectionUsage(false)`).
Skipped for `Mandatory` (sections cannot be disabled).

| Test | Description |
|---|---|
| `Sections_Disabled_DataIntegrityPreserved_BeginEndAreNoop` | Writer's `BeginSection`/`EndSection` inject no bytes; reader reads without section calls. |
| `Sections_Disabled_EndSection_ReturnsTrue` | `EndSection()` after `BeginSection()` returns `true` when disabled. |
| `Sections_Disabled_Flush_WithOpenSection_DoesNotThrow` | `Flush()` with open section does NOT throw when disabled. |
| `Sections_Disabled_Complete_WithOpenSection_DoesThrow` | `Complete()` with open section throws `InvalidOperationException` (LIFO nesting always enforced). |
| `Sections_Disabled_NestedTwoLevels_Works` | Two nested sections (disabled) round-trip data correctly. |
| `Sections_Disabled_Writer_EndSection_WithoutBeginSection_Throws` | LIFO guard always active on writer regardless of sections mode. |
| `Sections_Disabled_Reader_EndSection_WithoutBeginSection_Throws` | LIFO guard always active on reader regardless of sections mode. |

### Sections-enabled tests (18 tests)
Run for `Mandatory` (always enabled) and `Supported` (explicitly enabled via `TrySetSectionUsage(true)`).
Skipped for `NotSupported` (sections unavailable).

| Test | Description |
|---|---|
| `Sections_Enabled_EmptySection_ReturnsTrue` | Empty section → `EndSection()` returns `true`. |
| `Sections_Enabled_SingleValue_FullConsumption_ReturnsTrue` | One int written and read → returns `true`. |
| `Sections_Enabled_MultipleValues_FullConsumption_ReturnsTrue` | Three ints, all read → returns `true`. |
| `Sections_Enabled_PartialConsumption_EndSection_ReturnsFalse` | Reading fewer ints than written → returns `false`. |
| `Sections_Enabled_PartialConsumption_DataAfterSection_StillReadable` | After `EndSection()` skips bytes, data outside the section is still readable (stream alignment check). |
| `Sections_Enabled_OverConsumption_ThrowsEndOfStreamException` | Reading beyond section boundary throws `EndOfStreamException`. |
| `Sections_Enabled_OverConsumption_ExceptionImmediateNotDeferred` | The exception comes from `Read*()` itself, not from `EndSection()`. |
| `Sections_Enabled_NestedSections_BothFullyConsumed_BothReturnTrue` | Two nested levels, both fully consumed. |
| `Sections_Enabled_NestedSections_InnerPartial_OuterFullyConsumed` | Inner section skips; outer is fully consumed. |
| `Sections_Enabled_NestedSections_ThreeLevels_AllFull` | Three nested levels, all fully consumed. |
| `Sections_Enabled_MultipleSequentialSections_AllCorrect` | Five sequential sections, each fully consumed. |
| `Sections_Enabled_Flush_WithOpenSection_ThrowsInvalidOp` | `Flush()` inside open section (enabled) throws `InvalidOperationException`. |
| `Sections_Enabled_Complete_WithOpenSection_ThrowsInvalidOp` | Same for `Complete()`. |
| `Sections_Enabled_Writer_EndSection_WithoutBeginSection_Throws` | LIFO guard active even when sections are enabled. |
| `Sections_Enabled_Reader_EndSection_WithoutBeginSection_Throws` | Same on reader. |
| `Sections_Enabled_LargeSection_FullConsumption` | 1000 ints in one section, all read. |
| `Sections_Enabled_ReadBytes_BeyondSectionBoundary_ThrowsEndOfStream` | `ReadBytes()` that exceeds the section's byte boundary throws `EndOfStreamException`. |
| `Sections_Enabled_Fetch_WithOpenSection_ThrowsInvalidOp` | `Fetch()` while a section is open on the reader throws `InvalidOperationException`. |

### Mode-independent tests (3 tests)

| Test | Description |
|---|---|
| `Sections_TrySetSectionUsage_AfterDataWritten_ReturnsFalse` | Cannot change section mode after writing has started (applies to all implementations). |
| `Sections_TrySetSectionUsage_AfterDataRead_ReturnsFalse` | Cannot change section mode after reading has started. |
| `Sections_Reader_BeginSection_WhenIsCompleted_Throws` | `BeginSection()` on a completed reader throws. |

---

## File: `Check_IWriter_IReader.TypeSafety.cs` — Type safety & weak safety (35 tests)

Tests 1–30 require `typeSafety = true`. Tests 31–35 require `typeSafety = false`.

### Type-safe mismatch tests (each writes type A, flushes/fetches, reads type B → `InvalidDataException`)

| Test | Write type → Read type |
|---|---|
| `TypeSafety_WriteBool_ReadByte_Throws` | `bool` → `byte` |
| `TypeSafety_WriteBool_ReadSByte_Throws` | `bool` → `sbyte` |
| `TypeSafety_WriteBool_ReadInt_Throws` | `bool` → `int` |
| `TypeSafety_WriteBool_ReadString_Throws` | `bool` → `string` |
| `TypeSafety_WriteByte_ReadSByte_Throws` | `byte` → `sbyte` |
| `TypeSafety_WriteByte_ReadBool_Throws` | `byte` → `bool` |
| `TypeSafety_WriteSByte_ReadByte_Throws` | `sbyte` → `byte` |
| `TypeSafety_WriteChar_ReadUShort_Throws` | `char` → `ushort` (same width, different type) |
| `TypeSafety_WriteChar_ReadShort_Throws` | `char` → `short` |
| `TypeSafety_WriteShort_ReadUShort_Throws` | `short` → `ushort` |
| `TypeSafety_WriteShort_ReadInt_Throws` | `short` → `int` (different size) |
| `TypeSafety_WriteUShort_ReadShort_Throws` | `ushort` → `short` |
| `TypeSafety_WriteUShort_ReadInt_Throws` | `ushort` → `int` |
| `TypeSafety_WriteInt_ReadUInt_Throws` | `int` → `uint` |
| `TypeSafety_WriteInt_ReadFloat_Throws` | `int` → `float` (same bit width!) |
| `TypeSafety_WriteInt_ReadLong_Throws` | `int` → `long` |
| `TypeSafety_WriteUInt_ReadInt_Throws` | `uint` → `int` |
| `TypeSafety_WriteFloat_ReadInt_Throws` | `float` → `int` |
| `TypeSafety_WriteFloat_ReadDouble_Throws` | `float` → `double` |
| `TypeSafety_WriteLong_ReadULong_Throws` | `long` → `ulong` |
| `TypeSafety_WriteLong_ReadDouble_Throws` | `long` → `double` (same width!) |
| `TypeSafety_WriteULong_ReadLong_Throws` | `ulong` → `long` |
| `TypeSafety_WriteDouble_ReadFloat_Throws` | `double` → `float` |
| `TypeSafety_WriteDouble_ReadLong_Throws` | `double` → `long` |
| `TypeSafety_WriteDecimal_ReadString_Throws` | `decimal` → `string` |
| `TypeSafety_WriteString_ReadArray_Throws` | non-null `string` → `byte[]` |
| `TypeSafety_WriteArray_ReadString_Throws` | non-null `byte[]` → `string` |
| `TypeSafety_WriteString_ReadInt_Throws` | `string` → `int` |
| `TypeSafety_WriteInt_ReadString_Throws` | `int` → `string` |
| `TypeSafety_WriteNullString_ReadArray_Throws` | `null` string → `ReadArray` (null variants are typed) |
| `TypeSafety_WriteNullArray_ReadString_Throws` | `null` array → `ReadString` |
| `TypeSafety_WriteBytes_ReadInt_ThrowsImmediately` | `WriteBytes` → `ReadInt` |
| `TypeSafety_WriteBytes_ReadFloat_ThrowsImmediately` | `WriteBytes` → `ReadFloat` |
| `TypeSafety_WriteInt_ReadBytes_ThrowsImmediately` | `WriteInt` → `ReadBytes` |
| `TypeSafety_WriteFloat_ReadBytes_ThrowsImmediately` | `WriteFloat` → `ReadBytes` |
| `TypeSafety_WriteBytes5_ReadBytes3_ThrowsImmediately` | count mismatch (5 written, 3 read) → throws at `ReadBytes` |
| `TypeSafety_WriteBytes3_ReadBytes7_ThrowsImmediately` | count mismatch (3 written, 7 read) |
| `TypeSafety_WriteBytes0_ReadBytes1_ThrowsImmediately` | 0 bytes written, 1 expected |
| `TypeSafety_WriteBytes1_ReadBytes0_ThrowsImmediately` | 1 byte written, 0 expected |
| `TypeSafety_WriteBytes_ReadBytes_SameCount_NoThrow` | matching counts → no exception (sanity) |
| `TypeSafety_MismatchIsImmediate_NotDeferredToEndSection` | Confirms the exception fires on `Read*()`, not on `EndSection()`. Requires sections (`Supported` or `Mandatory`). |

### Weak-safety tests (`typeSafety = false`)

| Test | Sections required | Description |
|---|---|---|
| `WeakSafety_Sections_UnderConsumption_EndSectionReturnsFalse` | `Supported` or `Mandatory` | Unconsumed bytes → `EndSection()` returns `false`. |
| `WeakSafety_Sections_OverConsumption_ThrowsEndOfStream` | `Supported` or `Mandatory` | Over-reading inside section → `EndOfStreamException`. |
| `WeakSafety_Sections_ExactConsumption_EndSectionReturnsTrue` | `Supported` or `Mandatory` | Exact read → `EndSection()` returns `true`. |
| `WeakSafety_NoSections_ReadBeyondEnd_Throws` | `NotSupported` only | Reading past stream end without sections → `EndOfStreamException`. |

---

## File: `Check_IWriter_IReader.MultiCycle.cs` — Flush/Fetch lifecycle (15 tests)

| Test | Description |
|---|---|
| `MultiCycle_SingleFlushFetch_DataReadable` | Basic one-cycle write/read. |
| `MultiCycle_EmptyFlush_FetchReturnsTrue` | `Flush()` with no data → `Fetch()` still returns `true`. |
| `MultiCycle_TwoCycles_EachCycleDataCorrect` | Two cycles, each with one int. |
| `MultiCycle_FiveCycles_AllDataCorrect` | Five cycles, all values correct. |
| `MultiCycle_Complete_CountsAsOneFlush_NoExplicitFlush` | `Complete()` makes data available on exactly one `Fetch()`. |
| `MultiCycle_ExplicitFlushThenComplete_TwoFetchesBeforeTerminal` | One `Flush()` + one `Complete()` = two data fetches before terminal. |
| `MultiCycle_IsCompleted_FalseAfterDataFetch` | `IsCompleted` is still `false` after data fetch but before terminal. |
| `MultiCycle_IsCompleted_TrueAfterTerminalFetch` | `IsCompleted` is `true` after the terminal `Fetch()`. |
| `MultiCycle_MultipleEmptyFlushes_AllFetchesReturnTrue` | Four empty flush/fetch cycles, all return `true`. |
| `MultiCycle_DifferentTypesEachCycle_AllCorrect` | Each cycle writes a different primitive (int, string, bool, double). |
| `MultiCycle_LargeDataEachCycle_Correct` | Three cycles × 50 KB, all data matches. |
| `MultiCycle_NoDataFlushedBeforeComplete_OneSuccessfulFetch` | Empty `Complete()` terminates stream correctly. |
| `MultiCycle_ConcurrentFetchAndFlush_DataCorrect` | `Fetch()` started just before `Flush()` — data must arrive correctly. |
| `MultiCycle_FetchBlocksUntilFlush` | **Streaming**: `Fetch()` must be pending after 100 ms before `Flush()` is called. *(May not apply to in-memory implementations.)* |
| `MultiCycle_FetchUnblockedByComplete` | **Streaming**: `Complete()` must also unblock a pending `Fetch()`. |

---

## File: `Check_IWriter_IReader.Errors.cs` — Error / invalid usage (25 tests)

### Post-complete errors (all 16 write methods + control operations)

All `Errors_PostComplete_Write*_Throws` tests follow the same pattern: complete the writer, then
attempt the operation → `InvalidOperationException`.

| Test | Operation |
|---|---|
| `Errors_PostComplete_WriteBool_Throws` | `WriteBool` |
| `Errors_PostComplete_WriteByte_Throws` | `WriteByte` |
| `Errors_PostComplete_WriteSByte_Throws` | `WriteSByte` |
| `Errors_PostComplete_WriteChar_Throws` | `WriteChar` |
| `Errors_PostComplete_WriteShort_Throws` | `WriteShort` |
| `Errors_PostComplete_WriteUShort_Throws` | `WriteUShort` |
| `Errors_PostComplete_WriteInt_Throws` | `WriteInt` |
| `Errors_PostComplete_WriteUInt_Throws` | `WriteUInt` |
| `Errors_PostComplete_WriteLong_Throws` | `WriteLong` |
| `Errors_PostComplete_WriteULong_Throws` | `WriteULong` |
| `Errors_PostComplete_WriteFloat_Throws` | `WriteFloat` |
| `Errors_PostComplete_WriteDouble_Throws` | `WriteDouble` |
| `Errors_PostComplete_WriteDecimal_Throws` | `WriteDecimal` |
| `Errors_PostComplete_WriteString_Throws` | `WriteString` |
| `Errors_PostComplete_WriteArray_Throws` | `WriteArray` |
| `Errors_PostComplete_WriteBytes_Throws` | `WriteBytes` |
| `Errors_PostComplete_BeginSection_Throws` | `BeginSection` |
| `Errors_PostComplete_EndSection_Throws` | `EndSection` |
| `Errors_PostComplete_TrySetSectionUsage_Throws` | `TrySetSectionUsage` |
| `Errors_PostComplete_Flush_Throws` | `Flush` |
| `Errors_PostComplete_Complete_SecondCallThrows` | Second `Complete()` call |
| `Errors_PostComplete_TrySetSectionUsageTrue_Throws` | `TrySetSectionUsage(true)` after complete (complement to the false variant below) |
| `Errors_PostComplete_TrySetSectionUsageFalse_Throws` | `TrySetSectionUsage(false)` after complete — even "disabling" throws once completed |

### TrySetSectionUsage after activity

| Test | Description |
|---|---|
| `Errors_TrySetSectionUsage_AfterDataWritten_ReturnsFalse` | Returns `false` (not throw) once data has been written. |
| `Errors_TrySetSectionUsage_AfterDataRead_ReturnsFalse` | Returns `false` once data has been read. |
| `Errors_TrySetSectionUsage_AfterBeginSection_ReturnsFalse` | Returns `false` after `BeginSection()` wrote infrastructure bytes. Skipped for `NotSupported` (no bytes written). Runs for `Supported` (after explicit enable) and `Mandatory` (always active). |

### Section nesting errors

| Test | Description |
|---|---|
| `Errors_Writer_ExtraEndSection_AfterAllClosed_Throws` | Third `EndSection()` on a writer with only two matching pairs. |
| `Errors_Reader_ExtraEndSection_AfterAllClosed_Throws` | Same on reader. |
| `Errors_Sections_Enabled_Flush_WithOpenSection_Throws` | `Flush()` inside open section (enabled) throws. |
| `Errors_Sections_Enabled_Complete_WithOpenSection_Throws` | `Complete()` inside open section (enabled) throws. |
| `Errors_Sections_Disabled_NestedEndSection_WrongOrder_Throws` | Three `EndSection()` calls with only two matching opens. |

### End-of-stream reads

| Test | Description |
|---|---|
| `Errors_Read_BeyondStreamEnd_ReadInt_Throws` | `ReadInt()` with no buffered data → `EndOfStreamException`. |
| `Errors_Read_BeyondStreamEnd_ReadString_Throws` | Same for `ReadString`. |
| `Errors_Read_BeyondStreamEnd_ReadBool_Throws` | Same for `ReadBool`. |
| `Errors_Read_BeyondStreamEnd_ReadArray_Throws` | Same for `ReadArray`. |
| `Errors_Reader_BeginSection_WhenIsCompleted_Throws` | `BeginSection()` after stream is permanently exhausted → `InvalidOperationException`. |

---

## Cross-references

- `Sections_Enabled_Flush_WithOpenSection_ThrowsInvalidOp` and `Errors_Sections_Enabled_Flush_WithOpenSection_Throws` test the same invariant from different angles (sections file vs errors file).
- `WeakSafety_*` tests mirror the `Sections_Enabled_*` tests but target the `typeSafety=false` path.
- `MultiCycle_FetchBlocksUntilFlush` and `MultiCycle_FetchUnblockedByComplete` are only meaningful for streaming (non-in-memory) implementations.
- For `SectionsSupport = Supported`, both the disabled and enabled test groups execute, providing full coverage of both modes via `TrySetSectionUsage`.
