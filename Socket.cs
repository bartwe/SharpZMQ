using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpZMQ.lib;

namespace SharpZMQ {
    public struct Socket {
        internal IntPtr _socket;

        public void Connect(string address) {
            Debug.Assert(_socket != IntPtr.Zero);
            var addressUtf8 = Marshal.StringToCoTaskMemUTF8(address);
            var rc = LibzmqBinding.zmq_connect(_socket, addressUtf8);
            Marshal.FreeCoTaskMem(addressUtf8);
            if (rc != 0)
                LibzmqBinding.RaiseError("Failed to connect socket to: '" + address + "'.");
        }

        public void Bind(string address) {
            Debug.Assert(_socket != IntPtr.Zero);
            var addressUtf8 = Marshal.StringToCoTaskMemUTF8(address);
            var rc = LibzmqBinding.zmq_bind(_socket, addressUtf8);
            Marshal.FreeCoTaskMem(addressUtf8);
            if (rc != 0)
                LibzmqBinding.RaiseError("Failed to connect socket to: '" + address + "'.");
        }

        public void Close() {
            Debug.Assert(_socket != IntPtr.Zero);
            var rc = LibzmqBinding.zmq_close(_socket);
            if (rc != 0)
                LibzmqBinding.RaiseError("Failed to close socket.");
            _socket = IntPtr.Zero; ;
        }

        // on successful send the message is consumed
        public unsafe bool Send(ref Message message, bool dontWait, bool sendMore) {
            Debug.Assert(_socket != IntPtr.Zero);
            Message localMessage = message;
            Message* msgPtr = &localMessage;
            var options = (dontWait ? SendRecvOptions.ZMQ_DONTWAIT : SendRecvOptions.ZMQ_NONE) | (sendMore ? SendRecvOptions.ZMQ_SNDMORE : SendRecvOptions.ZMQ_NONE);
            var rc = LibzmqBinding.zmq_msg_send((IntPtr)msgPtr, _socket, options);
            if (rc < 0) {
                if (dontWait) {
                    var errno = LibzmqBinding.zmq_errno();
                    if (errno == (int)ErrorCodes.EAGAIN)
                        return false;
                }
                LibzmqBinding.RaiseError("Failed to send message from socket");
            }
            message = localMessage;
            return true;
        }

        // 'more' is explicitly not exposed as this should be obvious to the protocol above this layer
        public unsafe bool Receive(ref Message message, bool dontWait) {
            Debug.Assert(_socket != IntPtr.Zero);
            Message localMessage = message;
            Message* msgPtr = &localMessage;
            var rc = LibzmqBinding.zmq_msg_recv((IntPtr)msgPtr, _socket, dontWait ? SendRecvOptions.ZMQ_DONTWAIT : SendRecvOptions.ZMQ_NONE);
            if (rc < 0) {
                if (dontWait) {
                    var errno = LibzmqBinding.zmq_errno();
                    if (errno == (int)ErrorCodes.EAGAIN) {
                        return false;
                    }
                }
                LibzmqBinding.RaiseError("Failed to receive message from socket");
            }
            message = localMessage;
            return true;
        }
    }
}
