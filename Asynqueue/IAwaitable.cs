namespace Asynqueue
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// IAwaitable defines the interface for objects that can be awaited using the await keyword.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAwaitable<T> : INotifyCompletion
    {
        /// <summary>
        /// Gets a value indicating whether or not the await has completed
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets the awaiter object instance
        /// </summary>
        /// <returns></returns>
        IAwaitable<T> GetAwaiter();

        /// <summary>
        /// Gets the result of the await operation
        /// </summary>
        /// <returns>The message which was awaited</returns>
        T GetResult();
    }
}
