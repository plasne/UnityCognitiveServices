/*
  from https://gist.githubusercontent.com/cuppster/3612000/raw/a0be7f80244ee3eb5ea040bc35cfffd1de31874e/1.cs
  modified from original source: https://bitbucket.org/mattkotsenas/c-promises/overview
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Promises
{

    public interface Promise
    {
        Promise Done(Action callback);
        Promise Fail(Action callback);
        Promise Always(Action callback);

        bool IsRejected { get; }
        bool IsResolved { get; }
        bool IsFulfilled { get; }
    }

    public interface Promise<T> : Promise
    {
        Promise<T> Done(Action<T> callback);
        Promise<T> Done(IEnumerable<Action<T>> callbacks);

        Promise<T> Fail(Action<T> callback);
        Promise<T> Fail(IEnumerable<Action<T>> callbacks);

        Promise<T> Always(Action<T> callback);
        Promise<T> Always(IEnumerable<Action<T>> callbacks);
    }

    public class Deferred : Deferred<object>
    {
        // generic object
    }

    public class Deferred<T> : Promise<T>
    {
        private List<Callback> callbacks = new List<Callback>();
        protected bool _isResolved = false;
        protected bool _isRejected = false;
        private T _arg;

        public static Promise When(IEnumerable<Promise> promises)
        {
            var count = 0;
            var masterPromise = new Deferred();

            foreach (var p in promises)
            {
                count++;
                p.Fail(() =>
                {
                    masterPromise.Reject();
                });
                p.Done(() =>
                {
                    count--;
                    if (0 == count)
                    {
                        masterPromise.Resolve();
                    }
                });
            }

            return masterPromise;
        }

        public static Promise When(object d)
        {
            var masterPromise = new Deferred();
            masterPromise.Resolve();
            return masterPromise;
        }

        public static Promise When(Deferred d)
        {
            return d.Promise();
        }

        public static Promise<T> When(Deferred<T> d)
        {
            return d.Promise();

        }

        public Promise<T> Promise()
        {
            return this;
        }

        public Promise Always(Action callback)
        {
            if (_isResolved || _isRejected)
                callback();
            else
                callbacks.Add(new Callback(callback, Callback.Condition.Always, false));
            return this;
        }

        public Promise<T> Always(Action<T> callback)
        {
            if (_isResolved || _isRejected)
                callback(_arg);
            else
                callbacks.Add(new Callback(callback, Callback.Condition.Always, true));
            return this;
        }

        public Promise<T> Always(IEnumerable<Action<T>> callbacks)
        {
            foreach (Action<T> callback in callbacks)
                this.Always(callback);
            return this;
        }

        public Promise Done(Action callback)
        {
            if (_isResolved)
                callback();
            else
                callbacks.Add(new Callback(callback, Callback.Condition.Success, false));
            return this;
        }

        public Promise<T> Done(Action<T> callback)
        {
            if (_isResolved)
                callback(_arg);
            else
                callbacks.Add(new Callback(callback, Callback.Condition.Success, true));
            return this;
        }

        public Promise<T> Done(IEnumerable<Action<T>> callbacks)
        {
            foreach (Action<T> callback in callbacks)
                this.Done(callback);
            return this;
        }

        public Promise Fail(Action callback)
        {
            if (_isRejected)
                callback();
            else
                callbacks.Add(new Callback(callback, Callback.Condition.Fail, false));
            return this;
        }

        public Promise<T> Fail(Action<T> callback)
        {
            if (_isRejected)
                callback(_arg);
            else
                callbacks.Add(new Callback(callback, Callback.Condition.Fail, true));
            return this;
        }

        public Promise<T> Fail(IEnumerable<Action<T>> callbacks)
        {
            foreach (Action<T> callback in callbacks)
                this.Fail(callback);
            return this;
        }

        public bool IsRejected
        {
            get { return _isRejected; }
        }

        public bool IsResolved
        {
            get { return _isResolved; }
        }

        public bool IsFulfilled
        {
            get { return _isRejected || _isResolved; }
        }

        public Promise Reject()
        {
            if (_isRejected || _isResolved) // ignore if already rejected or resolved
                return this;
            _isRejected = true;
            DequeueCallbacks(Callback.Condition.Fail);
            return this;
        }

        public Deferred<T> Reject(T arg)
        {
            if (_isRejected || _isResolved) // ignore if already rejected or resolved
                return this;
            _isRejected = true;
            this._arg = arg;
            DequeueCallbacks(Callback.Condition.Fail);
            return this;
        }

        public Promise Resolve()
        {
            if (_isRejected || _isResolved) // ignore if already rejected or resolved
                return this;
            this._isResolved = true;
            DequeueCallbacks(Callback.Condition.Success);
            return this;
        }

        public Deferred<T> Resolve(T arg)
        {
            if (_isRejected || _isResolved) // ignore if already rejected or resolved
                return this;
            this._isResolved = true;
            this._arg = arg;
            DequeueCallbacks(Callback.Condition.Success);
            return this;
        }

        private void DequeueCallbacks(Callback.Condition cond)
        {
            foreach (Callback callback in callbacks)
            {
                if (callback.Cond == cond || callback.Cond == Callback.Condition.Always)
                {
                    if (callback.IsReturnValue)
                        callback.Del.DynamicInvoke(_arg);
                    else
                        callback.Del.DynamicInvoke();
                }
            }
            callbacks.Clear();
        }
    }

    class Callback
    {
        public enum Condition { Always, Success, Fail };

        public Callback(Delegate del, Condition cond, bool returnValue)
        {
            Del = del;
            Cond = cond;
            IsReturnValue = returnValue;
        }

        public bool IsReturnValue { get; private set; }
        public Delegate Del { get; private set; }
        public Condition Cond { get; private set; }

    }
}