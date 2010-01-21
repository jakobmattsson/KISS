using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Jad.Extensions;

namespace Erlang
{
    public interface IMessage { string Signature { get; } }

    public class SigOnly : IMessage
    {
        public string Signature { get; set; }
        public SigOnly(string s) { this.Signature = s; }
    }

    public abstract class BaseHandler
    {
        public Dispatcher dispatcher;

        public abstract bool Handler(IMessage data);
        public string PID { get; set; }

        public void KillMe() { dispatcher.Kill(PID); }
        public void Kill(string pid) { dispatcher.Kill(pid); }
        public string Spawn(string pid, BaseHandler handler) {dispatcher.Spawn(pid, handler); return pid; }
        public string Spawn(BaseHandler handler) { return dispatcher.Spawn(handler); }
        public void Send(string pid, IMessage data) { dispatcher.Send(pid, data); }
    }
    public class Dispatcher
    {
        private class Msg
        {
            public IMessage data;
            public bool handled;
            public override string ToString() { return data.ToString(); }
        }
        private class TData
        {
            public List<Msg> msgs = new List<Msg>();
            public List<Msg> newMsgs = new List<Msg>();
            public Thread thread;
            public BaseHandler handler;
            public bool stop;
            public bool stopped;

            public void RunMsg()
            {
                while (!stop)
                {
                    foreach (var m in msgs)
                        m.handled = handler.Handler(m.data);

                    lock (newMsgs)
                    {
                        msgs = msgs.Where(s => !s.handled).Concat(newMsgs).ToList();
                        newMsgs.Clear();
                    }

                    Thread.Sleep(10); // vad är lagom här?
                }
                stopped = true;
            }

            public override string ToString() { return handler.PID; }
        }

        private int ctr = 0;
        private List<TData> handlers = new List<TData>();

        private string InternalSpawn(string pid, BaseHandler handler)
        {
            handler.PID = pid;
            handler.dispatcher = this;

            var td = new TData { handler = handler };
            td.thread = new Thread(td.RunMsg);

            //var f1 = DateTime.Now;
            lock (handlers)
            {
                handlers.Add(td);
            }
            //var f2 = DateTime.Now.Subtract(f1);
            //Console.WriteLine(f2.TotalMilliseconds);

            td.thread.Start();

            Send(handler.PID, new SigOnly("create"));
            return pid;
        }

        public string Spawn(BaseHandler handler)
        {
            return InternalSpawn("___" + (++ctr), handler);
        }
        public string Spawn(string pid, BaseHandler handler)
        {
            if (pid.StartsWith("_"))
                throw new Exception("Invalid pid name");
            return InternalSpawn(pid, handler);
        }

        public void Kill(string pid)
        {
            foreach (var v in handlers.Where(s => s.handler.PID == pid))
                v.stop = true;
        }
        public void Send(string pid, IMessage data)
        {
            IEnumerable<TData> tt = null;

            var f1 = DateTime.Now;
            lock (handlers)
            {
                tt = handlers.Where(s => s.handler.PID == pid).ToArray();
            }

            foreach (var v in tt)
            {
                lock (v.newMsgs)
                {
                    v.newMsgs.Add(new Msg { data = data });
                }
            }
            var f2 = DateTime.Now.Subtract(f1);
//            Console.WriteLine(f2.TotalMilliseconds);
        }

        public void Block()
        {
            while (true)
            {
                lock (handlers)
                {
                    if (!handlers.Any())
                        return;
                    handlers = handlers.Where(s => !s.stopped).ToList();
                    Console.WriteLine("sleeping: " + handlers.AggregateToString(", ", s => s.handler.PID + ": " + s.msgs.Count));
                }

                Thread.Sleep(10); // vad är lagom här?
            }
        }

        public static void Run(Action<string[]> f, params string[] args)
        {
            var v = new Dispatcher();
            f(args);
            v.Block();
        }
    }
}
