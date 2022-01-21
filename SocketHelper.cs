using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SharpZMQ;

public enum SendMode {
    // send large data with the 'more' flag if it uses multiple message except for the last part
    Batched,

    // send the data each without the 'more' flag
    // when the receiving side can handle incremental receiving this can help reduce peak buffer usage and latency.
    // not suitable for sending to a ResponderSocket
    Parted,

    // send the data with the 'more' flag, including the last part
    MoreToFollow,

    Flush,
}

public static class SocketHelper {
    const int _ChunkSize = 1024 * 1024;
    static readonly UTF8Encoding _Utf8Encoding = new();

    public static void SendString(this Socket socket, string text, SendMode mode = SendMode.MoreToFollow) {
        var moreToFollow = mode == SendMode.MoreToFollow;
        var byteCount = _Utf8Encoding.GetByteCount(text);
        var message = Message.AllocateSendMessage(byteCount);
        if (_Utf8Encoding.GetBytes(text, Message.AsSpan(ref message)) != byteCount)
            throw new();
        socket.Send(ref message, moreToFollow);
    }

    public static string ReceiveString(this Socket socket) {
        var message = Message.AllocateReceiveMessage();
        try {
            if (!socket.Receive(ref message))
                throw new();
            return _Utf8Encoding.GetString(Message.AsSpan(ref message));
        }
        finally {
            Message.Release(ref message);
        }
    }

    public static unsafe void SendValue<T>(this Socket socket, T value, SendMode mode = SendMode.MoreToFollow) where T : unmanaged {
        var size = sizeof(T);
        var moreToFollow = mode == SendMode.MoreToFollow;
        var message = Message.AllocateSendMessage(size);
        var span = Message.AsSpan(ref message);
        fixed (void* spanPtr = &span.GetPinnableReference()) {
            *(T*)spanPtr = value;
        }
        socket.Send(ref message, moreToFollow);
    }

    public static unsafe T ReceiveValue<T>(this Socket socket) where T : unmanaged {
        var size = sizeof(T);
        var message = Message.AllocateReceiveMessage();
        try {
            if (!socket.Receive(ref message))
                throw new();
            var span = Message.AsSpan(ref message);
            if (span.Length != size) {
                throw new InvalidDataException();
            }
            fixed (void* spanPtr = &span.GetPinnableReference()) {
                return *(T*)spanPtr;
            }
        }
        finally {
            Message.Release(ref message);
        }
    }

    public static unsafe void SendValueSpan<T>(this Socket socket, Span<T> span, SendMode mode = SendMode.MoreToFollow) where T : unmanaged {
        var size = sizeof(T) * span.Length;
        var moreToFollow = mode == SendMode.MoreToFollow;
        var message = Message.AllocateSendMessage(size);
        fixed (void* spanPtr = &Message.AsSpan(ref message).GetPinnableReference()) {
            span.CopyTo(new((T*)spanPtr, span.Length));
        }
        socket.Send(ref message, moreToFollow);
    }

    public static unsafe void ReceiveValueSpan<T>(this Socket socket, Span<T> span) where T : unmanaged {
        var size = sizeof(T) * span.Length;
        var message = Message.AllocateReceiveMessage();
        try {
            if (!socket.Receive(ref message))
                throw new();
            var messageSpan = Message.AsSpan(ref message);
            if (messageSpan.Length != size) {
                throw new InvalidDataException();
            }
            fixed (void* spanPtr = &messageSpan.GetPinnableReference()) {
                new Span<T>((T*)spanPtr, span.Length).CopyTo(span);
            }
        }
        finally {
            Message.Release(ref message);
        }
    }

    public static unsafe void SendStream(this Socket socket, Stream stream, SendMode mode = SendMode.MoreToFollow) {
        if (mode == SendMode.Flush) {
            mode = SendMode.Batched;
        }
        var moreToFollow = mode == SendMode.MoreToFollow;
        var batched = mode == SendMode.Batched;
        Debug.Assert(stream != null, nameof(stream) + " != null");
        var length = stream.Length - stream.Position;
        {
            //send length
            var message = Message.AllocateSendMessage(8);
            fixed (void* spanPtr = &Message.AsSpan(ref message).GetPinnableReference()) {
                *(long*)spanPtr = length;
            }
            socket.Send(ref message, moreToFollow || (length > 0));
        }
        while (length > 0) {
            var partSize = Math.Min(length, _ChunkSize);
            length -= partSize;
            var message = Message.AllocateSendMessage((int)partSize);
            try {
                var span = Message.AsSpan(ref message);
                var offset = stream.Read(span);
                if (offset <= 0) {
                    throw new EndOfStreamException();
                }
                while (offset < span.Length) {
                    var count = stream.Read(span[offset..]);
                    if (count <= 0) {
                        throw new EndOfStreamException();
                    }
                    offset += count;
                }
            }
            catch {
                Message.Release(ref message);
                throw;
            }
            var sendMore = moreToFollow || (batched && (length > 0));
            socket.Send(ref message, sendMore);
        }
    }

    public static unsafe void ReceiveStream(this Socket socket, Stream stream) {
        Debug.Assert(stream != null, nameof(stream) + " != null");
        var message = Message.AllocateReceiveMessage();
        try {
            long length;
            {
                if (!socket.Receive(ref message))
                    throw new();
                fixed (void* spanPtr = &Message.AsSpan(ref message).GetPinnableReference()) {
                    length = *(long*)spanPtr;
                }
                stream.SetLength(Math.Max(stream.Length, stream.Position + length));
            }
            while (length > 0) {
                var partSize = Math.Min(length, _ChunkSize);
                length -= partSize;
                if (!socket.Receive(ref message))
                    throw new();
                stream.Write(Message.AsSpan(ref message));
            }
        }
        finally {
            Message.Release(ref message);
        }
    }
}
