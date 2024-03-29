// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace SharpCompress.Compressors.Deflate64;

internal sealed class DeflateInput
{
    public DeflateInput(byte[] buffer) => Buffer = buffer;

    public byte[] Buffer { get; }
    public int Count { get; set; }
    public int StartIndex { get; set; }

    internal void ConsumeBytes(int n)
    {
        Debug.Assert(n <= Count, "Should use more bytes than what we have in the buffer");
        StartIndex += n;
        Count -= n;
        Debug.Assert(StartIndex + Count <= Buffer.Length, "Input buffer is in invalid state!");
    }

    internal InputState DumpState() => new InputState(Count, StartIndex);

    internal void RestoreState(InputState state)
    {
        Count = state._count;
        StartIndex = state._startIndex;
    }

    internal /*readonly */
    readonly struct InputState
    {
        internal readonly int _count;
        internal readonly int _startIndex;

        internal InputState(int count, int startIndex)
        {
            _count = count;
            _startIndex = startIndex;
        }
    }
}
