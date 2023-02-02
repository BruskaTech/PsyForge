using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;

#if !UNITY_WEBGL || UNITY_EDITOR // System.Threading
public class EventLoop4 {
    protected SingleThreadTaskScheduler scheduler;
    protected CancellationTokenSource cts = new CancellationTokenSource();

    public EventLoop4() {
        scheduler = new SingleThreadTaskScheduler(cts.Token);
    }

    ~EventLoop4() {
        Stop();
    }

    public void Stop() {
        cts.Cancel();
        Task.Factory.StartNew(() => { }, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
    }

    // Do

    // TODO: JPB: (refactor) The Do functions could be improved with C# Source Generators
    //            Ex: any number of variadic arguments
    //            Ex: attribute on original method to generate the call to Do automatically
    //            https://itnext.io/this-is-how-variadic-arguments-could-work-in-c-e2034a9c241
    //            https://github.com/WhiteBlackGoose/InductiveVariadics
    //            This above link needs to be changed to include support for generic constraints
    //            Get it working in unity: https://medium.com/@EnescanBektas/using-source-generators-in-the-unity-game-engine-140ff0cd0dc
    //            This may also currently requires Roslyn https://forum.unity.com/threads/released-roslyn-c-runtime-c-compiler.651505/
    //            Intro to Source Generators: https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/

    protected void Do(Func<Task> func) {
        Task.Factory.StartNew(func, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
    }
    protected void Do<T>(Func<T, Task> func, T t)
            where T : struct {
        AssertBlittable(t);
        T tCopy = t;
        Do(async () => { await func(tCopy); });
    }
    protected void Do<T, U>(Func<T, U, Task> func, T t, U u)
            where T : struct
            where U : struct {
        AssertBlittable(t, u);
        T tCopy = t;
        U uCopy = u;
        Do(async () => { await func(tCopy, uCopy); });
    }
    protected void Do<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        T tCopy = t;
        U uCopy = u;
        V vCopy = v;
        Do(async () => { await func(tCopy, uCopy, vCopy); });
    }
    protected void Do<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        T tCopy = t;
        U uCopy = u;
        V vCopy = v;
        W wCopy = w;
        Do(async () => { await func(tCopy, uCopy, vCopy, wCopy); });
    }

    // DoIn

    // TODO: JPB: (feature) Add DoIn (and in the webgl version too)

    // DoRepeating

    // TODO: JPB: (feature) Add DoRepeating (and in the webgl version too)

    // DoWaitFor

    protected Task DoWaitFor(Func<Task> func) {
        return Task.Factory.StartNew(func, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
    }
    protected Task DoWaitFor<T>(Func<T, Task> func, T t)
            where T : struct {
        AssertBlittable(t);
        T tCopy = t;
        return DoWaitFor(async () => { await func(tCopy); });
    }
    protected Task DoWaitFor<T, U>(Func<T, U, Task> func, T t, U u)
            where T : struct
            where U : struct {
        AssertBlittable(t, u);
        T tCopy = t;
        U uCopy = u;
        return DoWaitFor(async () => { await func(tCopy, uCopy); });
    }
    protected Task DoWaitFor<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        T tCopy = t;
        U uCopy = u;
        V vCopy = v;
        return DoWaitFor(async () => { await func(tCopy, uCopy, vCopy); });
    }
    protected Task DoWaitFor<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        T tCopy = t;
        U uCopy = u;
        V vCopy = v;
        W wCopy = w;
        return DoWaitFor(async () => { await func(tCopy, uCopy, vCopy, wCopy); });
    }

    // DoGet

    protected Task<Z> DoGet<Z>(Func<Task<Z>> func) {
        return Task.Factory.StartNew(func, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
    }
    protected Task<Z> DoGet<T, Z>(Func<T, Task<Z>> func, T t)
            where T : struct {
        AssertBlittable(t);
        T tCopy = t;
        return DoGet(async () => { return await func(tCopy); });
    }
    protected Task<Z> DoGet<T, U, Z>(Func<T, U, Task<Z>> func, T t, U u)
            where T : struct
            where U : struct {
        AssertBlittable(t, u);
        T tCopy = t;
        U uCopy = u;
        return DoGet(async () => { return await func(tCopy, uCopy); });
    }
    protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, Task<Z>> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        T tCopy = t;
        U uCopy = u;
        V vCopy = v;
        return DoGet(async () => { return await func(tCopy, uCopy, vCopy); });
    }
    protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, Task<Z>> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        T tCopy = t;
        U uCopy = u;
        V vCopy = v;
        W wCopy = w;
        return DoGet(async () => { return await func(tCopy, uCopy, vCopy, wCopy); });
    }

    // AssertBlittable

    protected static bool IsPassable(Type t) {
        return UnsafeUtility.IsBlittable(t)
            || t == typeof(bool)
            || t == typeof(char);
    }

    // TODO: JPB: (feature) Maybe use IComponentData from com.unity.entities when it releases
    //            This will also allow for bool and char to be included in the structs
    //            https://docs.unity3d.com/Packages/com.unity.entities@0.17/api/Unity.Entities.IComponentData.html
    protected static void AssertBlittable<T>(T t)
            where T : struct {
        if (!IsPassable(typeof(T))) {
            throw new ArgumentException("The first argument is not a blittable type.");
        }
    }
    protected static void AssertBlittable<T, U>(T t, U u)
            where T : struct
            where U : struct {
        if (!IsPassable(typeof(T))) {
            throw new ArgumentException("The first argument is not a blittable type.");
        } else if (!IsPassable(typeof(U))) {
            throw new ArgumentException("The second argument is not a blittable type.");
        }
    }
    protected static void AssertBlittable<T, U, V>(T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        if (!IsPassable(typeof(T))) {
            throw new ArgumentException("The first argument is not a blittable type.");
        } else if (!IsPassable(typeof(U))) {
            throw new ArgumentException("The second argument is not a blittable type.");
        } else if (!IsPassable(typeof(V))) {
            throw new ArgumentException("The third argument is not a blittable type.");
        }
    }
    protected static void AssertBlittable<T, U, V, W>(T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        if (!IsPassable(typeof(T))) {
            throw new ArgumentException("The first argument is not a blittable type.");
        } else if (!IsPassable(typeof(U))) {
            throw new ArgumentException("The second argument is not a blittable type.");
        } else if (!IsPassable(typeof(V))) {
            throw new ArgumentException("The third argument is not a blittable type.");
        } else if (!IsPassable(typeof(W))) {
            throw new ArgumentException("The fourth argument is not a blittable type.");
        }
    }
}


// TODO: JPB: (refactor) This may be able to cancel current running tasks
//            This would require replacing the standard task scheduler with a SingleThreadTaskScheduler
//            Not sure how this would affect unity, because it would be on its thread...
#else
public class EventLoop4 {
    protected bool isStopped = false;

    public EventLoop4() {}

    ~EventLoop4() {
        Stop();
    }

    // Do

    protected void Do(Func<Task> func) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        func();
    }
    protected void Do<T>(Func<T, Task> func, T t) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        func(t);
    }
    protected void Do<T, U>(Func<T, U, Task> func, T t, U u) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        func(t, u);
    }
    protected void Do<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        func(t, u, v);
    }
    protected void Do<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        func(t, u, v, w);
    }

    // DoWaitFor

    protected Task DoWaitFor(Func<Task> func) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func();
    }
    protected Task DoWaitFor<T>(Func<T, Task> func, T t) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t);
    }
    protected Task DoWaitFor<T, U>(Func<T, U, Task> func, T t, U u) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t, u);
    }
    protected Task DoWaitFor<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t, u, v);
    }
    protected Task DoWaitFor<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t, u, v, w);
    }

    // DoGet

    protected Task<Z> DoGet<Z>(Func<Task<Z>> func) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func();
    }
    protected Task<Z> DoGet<T, Z>(Func<T, Task<Z>> func, T t) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t);
    }
    protected Task<Z> DoGet<T, U, Z>(Func<T, U, Task<Z>> func, T t, U u) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t, u);
    }
    protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, Task<Z>> func, T t, U u, V v) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t, u, v);
    }
    protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, Task<Z>> func, T t, U u, V v, W w) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t, u, v, w);
    }

    public void Stop() {
        isStopped = true;
    }
}
#endif