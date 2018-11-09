﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information. 

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableEx
    {
        public static IAsyncEnumerable<TValue> Never<TValue>()
        {
            return NeverAsyncEnumerable<TValue>.Instance;
        }

        private sealed class NeverAsyncEnumerable<TValue> : IAsyncEnumerable<TValue>
        {
            internal static readonly NeverAsyncEnumerable<TValue> Instance = new NeverAsyncEnumerable<TValue>();

            public IAsyncEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken)
            {
                return new NeverAsyncEnumerator(cancellationToken);
            }

            private sealed class NeverAsyncEnumerator : IAsyncEnumerator<TValue>
            {
                public TValue Current => throw new InvalidOperationException();

                private readonly CancellationToken _token;

                private CancellationTokenRegistration _registration;

                private bool _once;

                private TaskCompletionSource<bool> _task;

                public NeverAsyncEnumerator(CancellationToken token)
                {
                    _token = token;
                }

                public ValueTask DisposeAsync()
                {
                    _registration.Dispose();
                    _task = null;
                    return TaskExt.CompletedTask;
                }

                public ValueTask<bool> MoveNextAsync()
                {
                    if (_once)
                    {
                        return TaskExt.False;
                    }
                    _once = true;
                    _task = new TaskCompletionSource<bool>();
                    _registration = _token.Register(state => ((NeverAsyncEnumerator)state)._task.SetCanceled(), this);
                    return new ValueTask<bool>(_task.Task);
                }
            }
        }
    }
}
