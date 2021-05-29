using System;
using System.Diagnostics;
using System.IO;

namespace SharpZMQ {
    public static class StreamSocketHelper {
        const int ChunkSize = 1024 * 1024;

        public static unsafe void SendStream(this Socket socket, Stream stream) {
            Debug.Assert(stream != null, nameof(stream) + " != null");
            var length = stream.Length - stream.Position;
            {
                //send length
                var message = Message.AllocateSendMessage(8);
                fixed (void* spanPtr = &message.AsSpan().GetPinnableReference())
                    *(long*)spanPtr = length;
                socket.Send(ref message, length > 0);
            }
            while (length > 0) {
                var partSize = Math.Min(length, ChunkSize);
                length -= partSize;
                var message = Message.AllocateSendMessage((int)partSize);
                try {
                    var span = message.AsSpan();
                    var offset = stream.Read(span);
                    if (offset <= 0)
                        throw new EndOfStreamException();
                    while (offset < span.Length) {
                        var count = stream.Read(span.Slice(offset));
                        if (count <= 0)
                            throw new EndOfStreamException();
                        offset += count;
                    }
                }
                catch {
                    Message.Release(ref message);
                    throw;
                }
                socket.Send(ref message);
            }
        }

        public static unsafe void ReceiveStream(this Socket socket, Stream stream) {
            Debug.Assert(stream != null, nameof(stream) + " != null");
            var message = Message.AllocateReceiveMessage();
            try {
                long length;
                {
                    socket.Receive(ref message);
                    fixed (void* spanPtr = &message.AsSpan().GetPinnableReference())
                        length = *(long*)spanPtr;
                    stream.SetLength(Math.Max(stream.Length, stream.Position + length));
                }
                while (length > 0) {
                    var partSize = Math.Min(length, ChunkSize);
                    length -= partSize;
                    socket.Receive(ref message);
                    stream.Write(message.AsSpan());
                }
            }
            finally {
                Message.Release(ref message);
            }
        }
    }
}
