using System;

namespace SharpZMQ;

public class SharpZMQException : Exception {
    public SharpZMQException(string message, string text) : base(message + ": " + text) { }
}