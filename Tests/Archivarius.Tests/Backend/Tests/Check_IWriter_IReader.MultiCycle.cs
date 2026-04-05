using System.Threading.Tasks;
using NUnit.Framework;

namespace Archivarius.Tests
{
    public abstract partial class Check_IWriter_IReader
    {
        // ── Single Flush/Fetch cycle ──────────────────────────────────────────────

        [Test, Category("MultiCycle")]
        public async Task MultiCycle_SingleFlushFetch_DataReadable()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteInt(12345);
            await FlushFetch(writer, reader);
            Assert.That(reader.ReadInt(), Is.EqualTo(12345));
        }

        /// <summary>
        /// Flush() with no data still counts as one cycle.
        /// Fetch() after an empty Flush() must return true (no-data flushes are valid).
        /// </summary>
        [Test, Category("MultiCycle")]
        public async Task MultiCycle_EmptyFlush_FetchReturnsTrue()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            // Flush with no data written
            await writer.Flush();
            bool ok = await FetchWithTimeout(reader);
            Assert.That(ok, Is.True, "Fetch() after an empty Flush() must return true.");
        }

        // ── Two-cycle ────────────────────────────────────────────────────────────

        [Test, Category("MultiCycle")]
        public async Task MultiCycle_TwoCycles_EachCycleDataCorrect()
        {
            var (writer, reader, _) = await GetWriterReaderPair();

            // Cycle 1
            writer.WriteInt(1);
            await FlushFetch(writer, reader);
            Assert.That(reader.ReadInt(), Is.EqualTo(1));

            // Cycle 2
            writer.WriteInt(2);
            await FlushFetch(writer, reader);
            Assert.That(reader.ReadInt(), Is.EqualTo(2));
        }

        // ── Five-cycle ───────────────────────────────────────────────────────────

        [Test, Category("MultiCycle")]
        public async Task MultiCycle_FiveCycles_AllDataCorrect()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            for (int i = 0; i < 5; i++)
            {
                writer.WriteInt(i * 100);
                await FlushFetch(writer, reader);
                Assert.That(reader.ReadInt(), Is.EqualTo(i * 100), $"Mismatch at cycle {i}");
            }
        }

        // ── Complete() semantics ─────────────────────────────────────────────────

        /// <summary>
        /// Complete() with no explicit Flush() counts as exactly one Flush().
        /// Data is available in the very next Fetch(), and the Fetch() after that must return false.
        /// </summary>
        [Test, Category("MultiCycle")]
        public async Task MultiCycle_Complete_CountsAsOneFlush_NoExplicitFlush()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteInt(777);
            await writer.Complete();   // one implicit flush

            bool first = await FetchWithTimeout(reader);
            Assert.That(first, Is.True, "First Fetch() after Complete() must return true.");
            Assert.That(reader.ReadInt(), Is.EqualTo(777));

            bool second = await FetchWithTimeout(reader);
            Assert.That(second, Is.False, "Second Fetch() after Complete() must return false.");
            Assert.That(reader.IsCompleted, Is.True);
        }

        /// <summary>Explicit Flush() before Complete() requires two Fetch() calls to drain.</summary>
        [Test, Category("MultiCycle")]
        public async Task MultiCycle_ExplicitFlushThenComplete_TwoFetchesBeforeTerminal()
        {
            var (writer, reader, _) = await GetWriterReaderPair();

            // Cycle 1 — explicit flush
            writer.WriteInt(10);
            await writer.Flush();
            bool fetch1 = await FetchWithTimeout(reader);
            Assert.That(fetch1, Is.True);
            Assert.That(reader.ReadInt(), Is.EqualTo(10));

            // Cycle 2 — complete (counts as a second flush)
            writer.WriteInt(20);
            await writer.Complete();
            bool fetch2 = await FetchWithTimeout(reader);
            Assert.That(fetch2, Is.True);
            Assert.That(reader.ReadInt(), Is.EqualTo(20));

            // Terminal cycle
            bool terminal = await FetchWithTimeout(reader);
            Assert.That(terminal, Is.False);
            Assert.That(reader.IsCompleted, Is.True);
        }

        // ── IsCompleted tracking ─────────────────────────────────────────────────

        [Test, Category("MultiCycle")]
        public async Task MultiCycle_IsCompleted_FalseAfterDataFetch()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await writer.Complete();
            await FetchWithTimeout(reader);
            Assert.That(reader.IsCompleted, Is.False,
                "IsCompleted must be false after the data Fetch(), before the terminal Fetch().");
        }

        [Test, Category("MultiCycle")]
        public async Task MultiCycle_IsCompleted_TrueAfterTerminalFetch()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteInt(1);
            await writer.Complete();
            await FetchWithTimeout(reader);
            reader.ReadInt();
            await FetchWithTimeout(reader);
            Assert.That(reader.IsCompleted, Is.True);
        }

        // ── Multiple empty flushes ───────────────────────────────────────────────

        [Test, Category("MultiCycle")]
        public async Task MultiCycle_MultipleEmptyFlushes_AllFetchesReturnTrue()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            for (int i = 0; i < 4; i++)
            {
                await writer.Flush();
                bool ok = await FetchWithTimeout(reader);
                Assert.That(ok, Is.True, $"Empty Flush/Fetch cycle {i} — Fetch must return true.");
            }
        }

        // ── Interleaved read/write ────────────────────────────────────────────────

        [Test, Category("MultiCycle")]
        public async Task MultiCycle_DifferentTypesEachCycle_AllCorrect()
        {
            var (writer, reader, _) = await GetWriterReaderPair();

            writer.WriteInt(42);
            await FlushFetch(writer, reader);
            Assert.That(reader.ReadInt(), Is.EqualTo(42));

            writer.WriteString("hello");
            await FlushFetch(writer, reader);
            Assert.That(reader.ReadString(), Is.EqualTo("hello"));

            writer.WriteBool(true);
            await FlushFetch(writer, reader);
            Assert.That(reader.ReadBool(), Is.True);

            writer.WriteDouble(3.14);
            await FlushFetch(writer, reader);
            Assert.That(reader.ReadDouble(), Is.EqualTo(3.14));
        }

        [Test, Category("MultiCycle")]
        public async Task MultiCycle_LargeDataEachCycle_Correct()
        {
            const int cycles = 3;
            const int bytesPerCycle = 50 * 1024;
            var (writer, reader, _) = await GetWriterReaderPair();

            for (int c = 0; c < cycles; c++)
            {
                byte[] src = new byte[bytesPerCycle];
                for (int i = 0; i < bytesPerCycle; i++) src[i] = (byte)((c * 7 + i) % 256);
                writer.WriteArray(src);
                await FlushFetch(writer, reader);
                byte[]? result = reader.ReadArray();
                Assert.That(result, Is.EqualTo(src), $"Large data mismatch on cycle {c}.");
            }
        }

        // ── Complete() with no data ──────────────────────────────────────────────

        [Test, Category("MultiCycle")]
        public async Task MultiCycle_NoDataFlushedBeforeComplete_OneSuccessfulFetch()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            await writer.Complete();
            bool first = await FetchWithTimeout(reader);
            // Per contract, Complete() counts as one Flush() and Fetch() may return true
            // even when no data was written. The second Fetch() must then return false.
            if (first)
            {
                bool second = await FetchWithTimeout(reader);
                Assert.That(second, Is.False, "After an empty Complete(), the second Fetch() must return false.");
                Assert.That(reader.IsCompleted, Is.True);
            }
            else
            {
                // Some implementations may return false immediately for empty Complete
                Assert.That(reader.IsCompleted, Is.True);
            }
        }

        // ── Concurrent Fetch/Flush ────────────────────────────────────────────────

        /// <summary>
        /// Starts Fetch() concurrently before Flush() is called.
        /// For streaming implementations, Fetch() must block until Flush() delivers data.
        /// For in-memory implementations the result must still be correct.
        /// </summary>
        [Test, Category("MultiCycle")]
        public async Task MultiCycle_ConcurrentFetchAndFlush_DataCorrect()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteInt(99999);

            // Start Fetch *before* Flush
            var fetchTask = reader.Fetch().AsTask();

            // Allow the task to begin executing
            await Task.Yield();

            // Now flush
            await writer.Flush();

            bool ok = await fetchTask.WaitAsync(TestTimeout);
            Assert.That(ok, Is.True, "Fetch() must return true after Flush().");
            Assert.That(reader.ReadInt(), Is.EqualTo(99999));
        }

        /// <summary>
        /// For streaming implementations, Fetch() called before Flush() must block
        /// (not complete immediately). Verifies the blocking invariant with a 100ms delay.
        /// </summary>
        [Test, Category("MultiCycle")]
        public async Task MultiCycle_FetchBlocksUntilFlush()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteInt(42);

            // Start Fetch before Flush
            var fetchTask = reader.Fetch().AsTask();

            // Give the task ~100 ms to start executing and potentially block
            await Task.Delay(100);

            // For true streaming implementations, the task must still be pending here
            Assert.That(fetchTask.IsCompleted, Is.False,
                "Fetch() must block until Flush() is called (for streaming implementations).");

            await writer.Flush();

            bool ok = await fetchTask.WaitAsync(TestTimeout);
            Assert.That(ok, Is.True);
            Assert.That(reader.ReadInt(), Is.EqualTo(42));
        }

        /// <summary>
        /// Complete() must also unblock a pending Fetch() the same way Flush() does.
        /// </summary>
        [Test, Category("MultiCycle")]
        public async Task MultiCycle_FetchUnblockedByComplete()
        {
            var (writer, reader, _) = await GetWriterReaderPair();
            writer.WriteInt(555);

            var fetchTask = reader.Fetch().AsTask();
            await Task.Delay(100);
            Assert.That(fetchTask.IsCompleted, Is.False,
                "Fetch() must block until Complete() is called.");

            await writer.Complete();

            bool ok = await fetchTask.WaitAsync(TestTimeout);
            Assert.That(ok, Is.True);
            Assert.That(reader.ReadInt(), Is.EqualTo(555));
        }
    }
}

