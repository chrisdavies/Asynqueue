# Asynqueue

This is a simple mechanism for passing messages between tasks in .NET. It is a naive, but fast actor model implementation.

Get it via Nuget:

    Install-Package Asynqueue

## Performance
This performs quite nicely on my laptop (an i7 2.4Ghz). It is capable of processing 1 million messages (and responses) in 575ms.

As a comparison, the same implementation using DataFlow is 100k messages in 1400ms. Or using Stact 10k in 4000ms.

To be fair to DataFlow and Stact, these are much more robust solutions.

Anyway, performance degrades significantly (orders of magnitude) while in debug mode in VS. I'm not sure why, but I don't really care, as performance only matters when I'm not running in the IDE, anyway.

## Asynqueue
The Asynqueue class is used to send messages to an actor. Any number of processes can send messages to an asynqueue, but only one process can ever own (receive on) any given asynqueue.

So, let's define an asynqueue that receives a WelcomeEmail message type.

    var emailQueue = new Asynqueue<WelcomeEmail>(async emailMessage => {
        await EmailSys.Send(email);
    });

This receives email messages and then sends them asynchronously using the fictional EmailSys class. To send welcome emails to this queue, we would simply do this:

    emailQueue.Send(new WelcomeEmail { Subject = "Welcome!" });

OK, I didn't specify any email addresses, etc, but that's not the point. The anonymous function (async emailMessage => { ... }) is really an actor, spawned into a .NET Task which is efficiently managed by the queue.

## QueriableAsynqueue
OK, that's great. But what if we want to get the results of the actor's work? Well, the actor could publish results to another instance of Asynqueue, but that would be cumbersome, plus only one actor could read from the result queue.

This is where QueriableAsynqueue comes in. It allows you to send messages to an actor, and then process the response. Here's an example.

    var userQuery = new QueriableAsynqueue<string, User>(async name => {
        var result = await Users.GetByNameAsync(name);
        return result;
    });

This queue will send strings (usernames, in our example) to an actor, and will receive User objects in return.

We might send and receive like this:

    var user = await userQuery.Query("cdavies");

## Error handling
When using QueriableAsynqueue, exceptions that occur in the actor will be thrown in the process that handles the actor's response. When using the normal Asynqueue, unhandled exceptions in the actor are unhandled by the system.

In future versions, we may implement supervisor functionality to attempt to recover from unhandled errors in actors.
