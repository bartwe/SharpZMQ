using System;

namespace SharpZMQ.lib;

public class SharpZMQException : Exception {
    public SharpZMQException(string message, string text) : base(message + ": " + text) { }
}