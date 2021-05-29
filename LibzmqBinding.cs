using System;
using System.Runtime.InteropServices;

namespace SharpZMQ.lib {
    public enum SocketType {
        ZMQ_PAIR = 0,
        ZMQ_PUB = 1,
        ZMQ_SUB = 2,
        ZMQ_REQ = 3,
        ZMQ_REP = 4,
        ZMQ_DEALER = 5,
        ZMQ_ROUTER = 6,
        ZMQ_PULL = 7,
        ZMQ_PUSH = 8,
        ZMQ_XPUB = 9,
        ZMQ_XSUB = 10,
        ZMQ_STREAM = 11,
    }

    public enum SendRecvOptions {
        ZMQ_NONE = 0,
        ZMQ_DONTWAIT = 1,
        ZMQ_SNDMORE = 2,
    }

    public enum ErrorCodes {
        EAGAIN = 11,
    }

    public static unsafe class LibzmqBinding {
        internal const int MessageSize = 64;

        // The static constructor prepares static readonly fields
        static LibzmqBinding() {
            int major, minor, patch;
            zmq_version(out major, out minor, out patch);
            if (new Version(major, minor, patch) < new Version(4, 1))
                throw VersionNotSupported(null, ">= v4.1");
        }

        static NotSupportedException VersionNotSupported(string methodName, string requiredVersion) {
            return new(string.Format("{0}libzmq version not supported. Required version {1}", methodName == null ? string.Empty : methodName + ": ", requiredVersion));
        }

        [DllImport("libzmq", EntryPoint = "zmq_version", CallingConvention = CallingConvention.Cdecl)]
        public static extern void zmq_version(out int major, out int minor, out int patch);

        [DllImport("libzmq", EntryPoint = "zmq_ctx_new", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_ctx_new();

        [DllImport("libzmq", EntryPoint = "zmq_ctx_get", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_ctx_get(IntPtr context, int option);

        [DllImport("libzmq", EntryPoint = "zmq_ctx_set", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_ctx_set(IntPtr context, int option, int optval);

        [DllImport("libzmq", EntryPoint = "zmq_ctx_shutdown", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_ctx_shutdown(IntPtr context);

        [DllImport("libzmq", EntryPoint = "zmq_ctx_term", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_ctx_term(IntPtr context);

        [DllImport("libzmq", EntryPoint = "zmq_msg_init", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_init(IntPtr msg);

        [DllImport("libzmq", EntryPoint = "zmq_msg_init_size", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_init_size(IntPtr msg, int size);

        [DllImport("libzmq", EntryPoint = "zmq_msg_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_send(IntPtr msg, IntPtr socket, SendRecvOptions flags);

        [DllImport("libzmq", EntryPoint = "zmq_msg_recv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_recv(IntPtr msg, IntPtr socket, SendRecvOptions flags);

        [DllImport("libzmq", EntryPoint = "zmq_msg_close", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_close(IntPtr msg);

        [DllImport("libzmq", EntryPoint = "zmq_msg_data", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_msg_data(IntPtr msg);

        [DllImport("libzmq", EntryPoint = "zmq_msg_size", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_size(IntPtr msg);

        [DllImport("libzmq", EntryPoint = "zmq_msg_more", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_more(IntPtr msg);

        [DllImport("libzmq", EntryPoint = "zmq_msg_gets", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_msg_gets(IntPtr msg, IntPtr property);

        [DllImport("libzmq", EntryPoint = "zmq_msg_get", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_get(IntPtr msg, int property);

        [DllImport("libzmq", EntryPoint = "zmq_msg_set", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_set(IntPtr msg, int property, int value);

        [DllImport("libzmq", EntryPoint = "zmq_msg_copy", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_copy(IntPtr dest, IntPtr src);

        [DllImport("libzmq", EntryPoint = "zmq_msg_move", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_msg_move(IntPtr dest, IntPtr src);

        [DllImport("libzmq", EntryPoint = "zmq_socket", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_socket(IntPtr context, SocketType type);

        [DllImport("libzmq", EntryPoint = "zmq_close", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_close(IntPtr socket);

        [DllImport("libzmq", EntryPoint = "zmq_getsockopt", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_getsockopt(IntPtr socket, int option_name, IntPtr option_value, IntPtr option_len);

        [DllImport("libzmq", EntryPoint = "zmq_setsockopt", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_setsockopt(IntPtr socket, int option_name, IntPtr option_value, int option_len);

        [DllImport("libzmq", EntryPoint = "zmq_bind", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_bind(IntPtr socket, IntPtr endpoint);

        [DllImport("libzmq", EntryPoint = "zmq_unbind", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_unbind(IntPtr socket, IntPtr endpoint);

        [DllImport("libzmq", EntryPoint = "zmq_connect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_connect(IntPtr socket, IntPtr endpoint);

        [DllImport("libzmq", EntryPoint = "zmq_disconnect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_disconnect(IntPtr socket, IntPtr endpoint);

        [DllImport("libzmq", EntryPoint = "zmq_poll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_poll(void* items, int numItems, long timeout);

        [DllImport("libzmq", EntryPoint = "zmq_has", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_has(IntPtr capability);

        [DllImport("libzmq", EntryPoint = "zmq_socket_monitor", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_socket_monitor(IntPtr socket, IntPtr endpoint, int events);

        [DllImport("libzmq", EntryPoint = "zmq_proxy", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_proxy(IntPtr frontend, IntPtr backend, IntPtr capture);

        [DllImport("libzmq", EntryPoint = "zmq_proxy_steerable", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_proxy_steerable(IntPtr frontend, IntPtr backend, IntPtr capture, IntPtr control);

        [DllImport("libzmq", EntryPoint = "zmq_curve_keypair", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_curve_keypair(IntPtr z85_public_key, IntPtr z85_secret_key);

        [DllImport("libzmq", EntryPoint = "zmq_z85_encode", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_z85_encode(IntPtr dest, IntPtr data, int size);

        [DllImport("libzmq", EntryPoint = "zmq_z85_decode", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_z85_decode(IntPtr dest, IntPtr data);

        [DllImport("libzmq", EntryPoint = "zmq_errno", CallingConvention = CallingConvention.Cdecl)]
        public static extern int zmq_errno();

        [DllImport("libzmq", EntryPoint = "zmq_strerror", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zmq_strerror(int errnum);

        public static void RaiseError(string message) {
            var errno = zmq_errno();
            var text = Marshal.PtrToStringUTF8(zmq_strerror(errno));
            throw new SharpZMQException(message, text);
        }
    }

    public class SharpZMQException : Exception {
        public SharpZMQException(string message, string text) : base(message + ": " + text) { }
    }
}
