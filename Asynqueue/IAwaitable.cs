namespace Asynqueue
{
    using System.Runtime.CompilerServices;

    public interface IAwaitable<T> : INotifyCompletion
    {
        bool IsCompleted { get; }

        IAwaitable<T> GetAwaiter();

        T GetResult();
    }
}
