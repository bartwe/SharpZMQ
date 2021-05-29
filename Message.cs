﻿using System;
using System.Runtime.InteropServices;
using SharpZMQ.lib;

namespace SharpZMQ {
    [StructLayout(LayoutKind.Sequential)]
    public struct Message {
        unsafe fixed byte ZmqMsgT[64];

        public unsafe Span<byte> AsSpan() {
            Message localMessage = this;
            Message* msgPtr = &localMessage;
            var length = LibzmqBinding.zmq_msg_size((IntPtr)(&msgPtr));
            var data = LibzmqBinding.zmq_msg_data((IntPtr)(&msgPtr));
            return new((void*)data, length);
        }

        public static unsafe Message AllocateReceiveMessage() {
            Message message;
            Message* msgPtr = &message;
            var rc = LibzmqBinding.zmq_msg_init((IntPtr)msgPtr);
            if (rc != 0)
                LibzmqBinding.RaiseError("Failed to initialize receive message.");
            return message;
        }

        public static unsafe Message AllocateSendMessage(int size) {
            Message message;
            Message* msgPtr = &message;
            var rc = LibzmqBinding.zmq_msg_init_size((IntPtr)msgPtr, size);
            if (rc != 0)
                LibzmqBinding.RaiseError("Failed to initialize send message.");
            return message;
        }

        public static unsafe void Release(ref Message message) {
            Message localMessage = message;
            Message* msgPtr = &localMessage;
            var rc = LibzmqBinding.zmq_msg_close((IntPtr)msgPtr);
            if (rc != 0)
                LibzmqBinding.RaiseError("Failed to initialize message.");
            message = localMessage;
        }
    }
}
