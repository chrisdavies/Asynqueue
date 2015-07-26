# Perf

I kept hearing how fast Go is and how easy it makes concurrency, so I put this project together to see how C# stacked up.

Turns out C# stacks up pretty well, at least on Windows, given a relatively simple channel implementation that I wrote.

## Hardware

- 2008 DELL Precision M6400, 4GB RAM, 64 bit dual-core 2.x GHz
- Windows 10

## Simple queues/channels

This test sends a million messages to a channel and waits for the channel to acknowledge the receipt of all messages.

1 million messages

- C#: 648ms average
- Go: 811ms average

## Bi-directional queues/channels

This test sends a message to a channel and waits for acknowledgement before sending the next message.

1 million messages

- C#: 776ms average
- Go: 1381ms average

## Running tests

Build Asynqueue.Console.csproj in Release mode, then run from the terminal like so:

    Asynqueue.Console plain
    Asynqueue.Console bidirectional

Run the Go tests like so:
 
    go run ./chanperf.go plain
    go run ./chanperf.go bidirectional