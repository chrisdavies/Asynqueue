# Asynqueue Perf

I kept hearing how fast Go is and how easy it makes concurrency, so I put this project together to see how C# stacked up.

Not so hot... 

## Hardware

- 2008 DELL Precision M6400, 4GB RAM, 64 bit dual-core 2.x GHz
- Windows 10

## Simple queues/channels

This test sends a million messages to a channel and waits for the channel to acknowledge the receipt of all messages.

1 million messages

- C#: 2341ms average
- Go: 811ms average

C# is 2.8x slower

## Bi-directional queues/channels

This test sends a message to a channel and waits for acknowledgement before sending the next message.

1 million messages

- C#: 776ms average
- Go: 1381ms average

C# is 1.7x faster. This makes me wonder if my first benchmark could be improved by using this approach

## Buffered queues/channels

1 million messages, 1 million buffer size

- C#: 648ms average
- Go: 434ms average

Go is 1.4x faster

## Running tests

Build Asynqueue.Console.csproj in Release mode, then run from the terminal like so:

    Asynqueue.Console plain
    Asynqueue.Console bidirectional

Run the Go tests like so:
 
    go run ./chanperf.go plain
    go run ./chanperf.go bidirectional
    go run ./chanperf.go buff