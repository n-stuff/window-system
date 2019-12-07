using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace NStuff.WindowSystem
{
    /// <summary>
    /// Provides a base class to implement a run loop.
    /// </summary>
    public abstract class RunLoopBase
    {
        private struct ScheduledAction : IComparable<ScheduledAction>
        {
            internal long time;
            internal Action<long> action;

            int IComparable<ScheduledAction>.CompareTo(ScheduledAction other) => (int)(time - other.time);
        }

        private readonly PriorityQueue<ScheduledAction> scheduledActions = new PriorityQueue<ScheduledAction>();
        private readonly Stopwatch stopwatch = Stopwatch.StartNew();
        private bool interrupted;
        private Thread? runThread;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Run()"/> method is currently being invoked.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// Gets a list of actions that should be invoked each time the <see cref="Wait(int)"/> method returns.
        /// </summary>
        public List<Action<long>> RecurringActions { get; } = new List<Action<long>>();

        /// <summary>
        /// Notifies the run loop that the <see cref="Run()"/> should terminate.
        /// </summary>
        public void Interrupt()
        {
            interrupted = true;
            if (Running && runThread != Thread.CurrentThread)
            {
                InterruptWait();
            }
        }

        /// <summary>
        /// Schedules an action for invocation after the specified delay expires.
        /// </summary>
        /// <param name="delay">A delay in milliseconds.</param>
        /// <param name="action">An action taking as argument the number of milliseconds elapsed since the delay expired.</param>
        public void InvokeLater(int delay, Action<long> action)
        {
            lock (scheduledActions)
            {
                var invocationTime = stopwatch.ElapsedMilliseconds + delay;
                scheduledActions.Push(new ScheduledAction { time = invocationTime, action = action });
                if (Running && runThread != Thread.CurrentThread)
                {
                    InterruptWait();
                }
            }
        }

        /// <summary>
        /// The actual run loop. It only returns after <see cref="Interrupt()"/> has been called.
        /// </summary>
        public void Run()
        {
            Running = true;
            runThread = Thread.CurrentThread;
            interrupted = false;
            var initialTime = stopwatch.ElapsedMilliseconds;
            while (!interrupted)
            {
                var startTime = stopwatch.ElapsedMilliseconds;
                for (int i = 0; i < RecurringActions.Count; i++)
                {
                    RecurringActions[i](stopwatch.ElapsedMilliseconds - initialTime);
                }
                var time = stopwatch.ElapsedMilliseconds;
                long waitTimeout;
                if (scheduledActions.Count > 0)
                {
                    var a = scheduledActions.Peek();
                    waitTimeout = Math.Max(0, a.time - time);
                    InvokeScheduledActions(Math.Max(0, waitTimeout - (time - startTime)));
                }
                else
                {
                    waitTimeout = -1;
                    InvokeScheduledActions(long.MaxValue);
                }
                Wait((waitTimeout < 0) ? -1 : (int)Math.Max(0, waitTimeout - (stopwatch.ElapsedMilliseconds - startTime)));
            }
            Running = false;
        }

        /// <summary>
        /// Called to wait for the next scheduled action.
        /// </summary>
        /// <param name="timeout">The maximum time in milliseconds the method should wait.</param>
        protected abstract void Wait(int timeout);

        /// <summary>
        /// Called to unblock the <see cref="Wait(int)"/> method.
        /// </summary>
        protected abstract void InterruptWait();

        private void InvokeScheduledActions(long allocatedTime)
        {
            var initialTime = stopwatch.ElapsedMilliseconds;
            var time = initialTime;

            while (scheduledActions.Count > 0)
            {
                ScheduledAction a;
                lock (scheduledActions)
                {
                    a = scheduledActions.Peek();
                    if (a.time > time)
                    {
                        break;
                    }
                    scheduledActions.Pop();
                }
                time = stopwatch.ElapsedMilliseconds;
                a.action(time - a.time);
                time = stopwatch.ElapsedMilliseconds;
                if (time - initialTime > allocatedTime)
                {
                    break;
                }
            }
        }
    }
}
