using System;

namespace EventStreamSharp.Domain;

public record EventRecord(

    DateTime Timestamp,
    string ServiceName,
    string Action,
    int DurationMs,
    int PayloadSize,
    bool Success
);