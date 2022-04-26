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
