namespace IndexTankDotNet
{
   using System;
   using System.ComponentModel;
   using System.Threading;

   internal static class TimeOutRunner
   {
      internal static T Invoke<T>(Func<CancelEventArgs, T> function, TimeSpan timeout)
      {
         if (timeout.TotalMilliseconds <= 0)
         {
            throw new ArgumentOutOfRangeException("timeout", timeout, "Timeout is less than or equal to zero.");
         }

         CancelEventArgs args = new CancelEventArgs(false);
         IAsyncResult functionResult = function.BeginInvoke(args, null, null);
         WaitHandle waitHandle = functionResult.AsyncWaitHandle;

         if (!waitHandle.WaitOne(timeout))
         {
            args.Cancel = true;
            
            ThreadPool.UnsafeRegisterWaitForSingleObject(waitHandle,
                (state, timedOut) => function.EndInvoke(functionResult),
                null, -1, true);

            throw new TimeoutException("The specified timeout was exceeded.");
         }

         return function.EndInvoke(functionResult);
      }

      internal static T Invoke<T>(Func<T> function, TimeSpan timeout)
      {
         return Invoke(args => function(), timeout); // ignore CancelEventArgs
      }

      internal static void Invoke(Action<CancelEventArgs> action, TimeSpan timeout)
      {
         // pass a function that returns 0 & ignore result
         Invoke(args => {action(args); return 0;}, timeout);
      }

      internal static void TryInvoke(Action action, TimeSpan timeout)
      {
         Invoke(args => action(), timeout); // ignore CancelEventArgs
      }
   }
}