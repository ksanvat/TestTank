using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PrjCore.System.AsyncTask {
    public abstract class PAsyncTask<T> : PBaseAsyncTask, IEquatable<PAsyncTask<T>> {

        private readonly PInternalAsyncTaskManager _owner;
        private readonly ulong _id;
        
        private readonly Action<T> _callbackSuccess;
        private readonly Action _callbackFailure;
        
        private Coroutine _coroutine;

        private Task<T> _task;
        private CancellationTokenSource _cts;
        private CancellationToken _ct;

        private PAsyncTaskStatus _status;

        protected PAsyncTask(Action<T> callbackSuccess, Action callbackFailure) {
            _owner = PInternalAsyncTaskManager.AsOwner();
            _id = _owner.GenerateID();
            _callbackSuccess = callbackSuccess;
            _callbackFailure = callbackFailure;
            
            _coroutine = null;

            _task = null;
            _cts = null;
            _ct = CancellationToken.None;
            
            _status = PAsyncTaskStatus.NotStarted;
        }

        public override ulong ID => _id;

        protected CancellationToken CT => _ct;

        public bool Run() {
            if (_status != PAsyncTaskStatus.NotStarted) {
                Debug.LogErrorFormat("Cannot start {0} cause it is {1}", GetType(), _status);
                return false;
            }

            if (!_owner.Add(this)) {
                Debug.LogErrorFormat("Cannot add {0} to manager", GetType());
                return false;
            }

            try {
                _coroutine = _owner.StartCoroutine(InternalRunCoroutine());
                _status = PAsyncTaskStatus.Running;
            } catch (Exception e) {
                Debug.LogErrorFormat("Cannot start {0} cause of exception", GetType());
                Debug.LogException(e);
                
                _status = PAsyncTaskStatus.UnexpectedError;
                _owner.Remove(_id);
                
                return false;
            }

            return true;
        }

        public void Cancel() {
            _owner.Remove(_id);
        }

        public override bool Equals(object obj) {
            return Equals(obj as PAsyncTask<T>);
        }

        public bool Equals(PAsyncTask<T> other) {
            if (ReferenceEquals(other, null)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            if (GetType() != other.GetType()) {
                return false;
            }

            return _id == other._id;
        }

        public override int GetHashCode() {
            return _id.GetHashCode();
        }

        protected virtual Task<T> AsyncTask() {
            return Task.FromResult(default(T));
        }
        
        protected override bool TryDispose() {
            if (_status == PAsyncTaskStatus.Disposed) {
                return true;
            }
            
            InternalCancel();
            
            InternalDisposeCoroutine();

            if (_task == null || _task.IsCompleted) {
                InternalDisposeCTS();
                InternalDisposeTask();
            }

            if (!InternalIsAllDisposed()) {
                return false;
            }

            _status = PAsyncTaskStatus.Disposed;
            return true;
        }

        private IEnumerator InternalRunCoroutine() {
            try {
                _cts = new CancellationTokenSource();
                _ct = _cts.Token;
                _task = Task.Run(AsyncTask, _ct);
            } catch (Exception e) {
                Debug.LogErrorFormat("Cannot start {0} cause of exception", GetType());
                Debug.LogException(e);
                
                _status = PAsyncTaskStatus.Failed;
                _callbackFailure?.Invoke();
                _owner.Remove(_id);
                
                yield break;
            }

            // guarantee task will not complete instant
            yield return null;
            
            while (!_task.IsCompleted) {
                yield return null;
            }
            
            if (_task.IsFaulted) {
                Debug.LogErrorFormat("Task {0} failed cause of exception", GetType());
                Debug.LogException(_task.Exception);
                
                _status = PAsyncTaskStatus.Failed;
                _callbackFailure?.Invoke();
                _owner.Remove(_id);
                
                yield break;
            }

            if (_task.IsCanceled) {
                Debug.LogFormat("Task {0} cancelled", GetType());
                
                _status = PAsyncTaskStatus.Cancelled;
                _owner.Remove(_id);
                
                yield break;
            }

            _status = PAsyncTaskStatus.Success;
            _callbackSuccess?.Invoke(_task.Result);
            _owner.Remove(_id);
        }
        
        private void InternalCancel() {
            if (_cts == null || _status != PAsyncTaskStatus.Running) {
                return;
            }

            try {
                _cts.Cancel();
                _status = PAsyncTaskStatus.CancelRequested;
            } catch (Exception e) {
                Debug.LogErrorFormat("Cannot cancel {0} cause of exception", GetType());
                Debug.LogException(e);
                _status = PAsyncTaskStatus.UnexpectedError;
            }
        }
        
        private void InternalDisposeCoroutine() {
            try {
                if (_owner != null && _coroutine != null) {
                    _owner.StopCoroutine(_coroutine);
                }
            } finally {
                _coroutine = null;
            }
        }

        private void InternalDisposeTask() {
            if (_task != null) {
                try {
                    _task.Dispose();
                } finally {
                    _task = null;
                }
            }
        }

        private void InternalDisposeCTS() {
            if (_cts != null) {
                try {
                    _cts.Dispose();
                } finally {
                    _cts = null;
                }
            }
        }

        private bool InternalIsAllDisposed() {
            return _coroutine == null && _task == null && _cts == null;
        }

    }
}