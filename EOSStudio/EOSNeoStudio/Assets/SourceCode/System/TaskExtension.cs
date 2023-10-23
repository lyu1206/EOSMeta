using System;
using System.Threading;
using System.Threading.Tasks;
public static class TaskExtension
{
    public static async Task WaitUntil<T>(T elem, Func<T, bool> predicate, int seconds = 10)
    {
        var tcs = new TaskCompletionSource<int>();
        using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(seconds)))
        {
            cancellationTokenSource.Token.Register(() =>
            {
                tcs.SetException(
                    new TimeoutException($"Waiting predicate {predicate} for {elem.GetType()} timed out!"));
                tcs.TrySetCanceled();
            });

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (!predicate(elem))
                    {
                        await Task.Yield();
                        continue;
                    }
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }

                tcs.SetResult(0);
                break;
            }
        }
    }
    public static async Task WaitUntil(Func<bool> predicate, int seconds = 10)
    {
        var tcs = new TaskCompletionSource<int>();
        using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(seconds)))
        {
            cancellationTokenSource.Token.Register(() =>
            {
                tcs.SetException(
                    new TimeoutException($"Waiting predicate {predicate} for timed out!"));
                tcs.TrySetCanceled();
            });

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (!predicate())
                    {
                        await Task.Yield();
                        continue;
                    }
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }

                tcs.SetResult(0);
                break;
            }
        }
    }
}
