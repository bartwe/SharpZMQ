using System;
using System.Diagnostics;
using SharpZMQ.lib;

namespace SharpZMQ {
    public struct Context : IDisposable {
        private IntPtr _context;

        public static Context Create() {
            var instance = LibzmqBinding.zmq_ctx_new();
            if (instance == IntPtr.Zero) {
                LibzmqBinding.RaiseError("Failed to open context.");
            }
            return new() { _context = instance };
        }

        public void Dispose() {
            if (_context != IntPtr.Zero) {
                Close();
            }
        }

        public Socket CreateRequesterSocket() {
            Debug.Assert(_context != IntPtr.Zero);
            var socket = LibzmqBinding.zmq_socket(_context, SocketType.ZMQ_REQ);
            if (socket == IntPtr.Zero) {
                LibzmqBinding.RaiseError("Failed to create requester socket.");
            }
            return new() { _socket = socket };
        }

        public Socket CreateResponderSocket() {
            Debug.Assert(_context != IntPtr.Zero);
            var socket = LibzmqBinding.zmq_socket(_context, SocketType.ZMQ_REP);
            if (socket == IntPtr.Zero) {
                LibzmqBinding.RaiseError("Failed to create responder socket.");
            }
            return new() { _socket = socket };
        }

        public void Close() {
            Debug.Assert(_context != IntPtr.Zero);
            var rc = LibzmqBinding.zmq_ctx_term(_context);
            if (rc != 0) {
                LibzmqBinding.RaiseError("Failed to close context.");
            }
            _context = IntPtr.Zero;
        }
    }
}