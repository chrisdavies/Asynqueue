# Asynqueue

This is simply a prototype/demo of a mechanism for passing messages between tasks in .NET. It is a naive, but fast actor model implementation.

## Performance
This performs quite nicely on my laptop (an i7 2.4Ghz). It is capable of processing 1 million messages (and responses) in 675ms.

As a comparison, the same implementation using DataFlow is 100k messages in 1400ms. Or using Stact 10k in 4000ms.

To be fair to DataFlow and Stact, these are much more robust solutions.

Anyway, performance degrades significantly (orders of magnitude) while in debug mode in VS. I'm not sure why, but I don't really care, as performance only matters when I'm not running in the IDE, anyway.

## Messenger
The messenger class is used to send messages to an actor. Any number of processes can send messages to a messenger, but only one process should ever own (receive on) any given messenger.

So, let's define a messenger that receives a WelcomeEmail message type.

    var emailQueue = new Messenger<WelcomeEmail>();

To send welcome emails to this queue, we would simply do this:

    emailQueue.Send(new WelcomeEmail { Subject = "Welcome!" });

OK, I didn't specify any email addresses, etc, but that's not the point.

Now, here's an actor that will receive welcome email messages from our queue, and do something with them.

    var actor = new Task(async () =>
    {
        while (true)
        {
            var email = await emailQueue.Receive();
            await EmailSys.Send(email);
        }
    });

    actor.Start();

## QueriableMessenger
OK, that's great. But what if we want to get the results of the actor's work? Well, the actor could publish results to another instance of Messenger, but that would be cumbersome, plus only one process could read from the result messenger.

This is where QueriableMessenger comes in. It allows you to send messages to an actor, and then process the response. Here's an example.

    var userQuery = new QueriableMessenger<string, User>();

This queue will send strings (usernames, in our example) to an actor, and will receive User objects in return.

Our actor might look like this:

    var actor = new Task(async () =>
    {
        while (true)
        {
            var request = await userQuery.Receive();
            var username = request.Input;
            var response = await GetUser(username);
            request.Respond(response);
        }
    });

    actor.Start();

OK, here we call a fictional function GetUser, which makes an async call to the database, and returns a User object, or more likely Task<User>.

We might call this actor like this:

    var user = await userQuery.Query("cdavies");

## Error handling
When using QueriableMessengers, exceptions that occur in the actor will be thrown in the process that handles the actor's response. When using the normal Messenger, unhandled exceptions in the actor are unhandled by the system.

In future versions, we may implement supervisor functionality to attempt to recover from unhandled errors in actors.

# To do
- Nuget
