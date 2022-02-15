﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hydra
{
    /// <summary>
    /// A wrapper around a stream that limits the amount of data that can be read from it
    /// </summary>
    public class SizedStream : ReadOnlyStream
    {
        private readonly Stream stream;
        private readonly int length;
        private int n = 0;

        private int Count(int count) => Math.Min(count, length - n);

        /// <summary>
        /// Wraps the given stream for length limitation
        /// </summary>
        /// <param name="stream">Stream to wrap</param>
        /// <param name="length">Length to limit to</param>
        public SizedStream(Stream stream, int length)
        {
            this.stream = stream;
            this.length = length;
        }

        public override bool CanRead => stream.CanRead;

        public override long Length => length;
        public override long Position { get => n; set => throw new NotSupportedException(); }

        public override int Read(Span<byte> buffer)
        {
            int length = Count(buffer.Length);
            if (length == 0) return 0;

            int read = stream.Read(buffer[..length]);
            n += read;
            return read;
        }
        public override int Read(byte[] buffer, int offset, int count) =>
            Read(buffer.AsSpan()[offset..(offset + count)]);

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int length = Count(buffer.Length);
            if (length == 0) return 0;

            int read = await stream.ReadAsync(buffer[..length], cancellationToken);
            n += read;
            return read;
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) =>
            ReadAsync(buffer.AsMemory()[offset..(offset + count)], cancellationToken).AsTask();
    }
}
