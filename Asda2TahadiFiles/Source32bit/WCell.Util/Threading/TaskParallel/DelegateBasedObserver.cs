﻿using System;

namespace WCell.Util.Threading.TaskParallel
{
  internal class DelegateBasedObserver<T> : IObserver<T>
  {
    private Action<T> _onNext;
    private Action<Exception> _onError;
    private Action _onCompleted;

    internal DelegateBasedObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
    {
      if(onNext == null)
        throw new ArgumentNullException(nameof(onNext));
      if(onError == null)
        throw new ArgumentNullException(nameof(onError));
      if(onCompleted == null)
        throw new ArgumentNullException(nameof(onCompleted));
      _onNext = onNext;
      _onError = onError;
      _onCompleted = onCompleted;
    }

    public void OnCompleted()
    {
      _onCompleted();
    }

    public void OnError(Exception error)
    {
      _onError(error);
    }

    public void OnNext(T value)
    {
      _onNext(value);
    }
  }
}