using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace UDPNetworking {
    public static class CancellationTokenExtensions {
        /// <summary>
        /// Allows a cancellation token to be awaited.
        /// </summary>
        public static CancellationTokenAwaiter GetAwaiter(this CancellationToken token) {
            return new CancellationTokenAwaiter {
                token = token
            };
        }

        /// <summary>
        /// The custom awaiter for cancellation tokens.
        /// </summary>
        public struct CancellationTokenAwaiter : INotifyCompletion, ICriticalNotifyCompletion {
            internal CancellationToken token;

            public CancellationTokenAwaiter(CancellationToken token) {
                this.token = token;
            }

            //NOTE: This is called by compiler-generated/.NET internals to check if the Task has completed.
            public bool IsCompleted => token.IsCancellationRequested;

            //NOTE: This is called by compiler-generated methods, when the Task has completed.
            //Instead of returning a result, we throw an exception (to match .NET behavior).
            public object GetResult() {
                if (IsCompleted)
                    throw new OperationCanceledException();
                else
                    throw new InvalidOperationException("The cancellation token has not yet been cancelled.");
            }

            //NOTE: The compiler will generate stuff that hooks in here.
            //We hook those methods back into the CancellationToken.
            public void OnCompleted(Action continuation) => token.Register(continuation);
            public void UnsafeOnCompleted(Action continuation) => token.Register(continuation);
        }
    }
}
