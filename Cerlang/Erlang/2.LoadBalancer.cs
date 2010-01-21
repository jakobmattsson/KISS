using System.Collections.Generic;
using System.Linq;
using Jad.Extensions;

namespace Erlang
{
    public abstract class BalancerData : IMessage
    {
        public string pid;
        public object body;
        public override string ToString() { return body.ToString(); }
        public abstract string Signature { get; }
    }
    public sealed class BalancerRequest : BalancerData
    {
        public sealed override string Signature { get { return "request"; } }
    }
    public sealed class BalancerResponse : BalancerData
    {
        public sealed override string Signature { get { return "response"; } }
    }
    public sealed class UpdateList : IMessage
    {
        public string pid;
        public string Signature { get; set; }
    }

    public class Balancer : BaseHandler
    {
        private class BusyData
        {
            public string client;
            public string server;
        }

        private IList<string> free;
        private IList<BusyData> busy = new BusyData[0];
        private IList<string> toRemove = new string[0];

        public override bool Handler(IMessage data)
        {
            if (data.Signature == "create")
            {
                return true;
            }
            else if (data.Signature == "request")
            {
                var d = data as BalancerRequest;
                var first = free.FirstOrDefault();
                if (first != null)
                {
                    busy = busy.Concat(new BusyData[] { new BusyData { server = first, client = d.pid } }).ToList();
                    free = free.Skip(1).ToList();
                    Send(first, new BalancerRequest { body = d.body, pid = this.PID });
                    return true;
                }
            }
            else if (data.Signature == "response")
            {
                var d = data as BalancerResponse;
                var match = busy.Where(s => s.server == d.pid);

                free = free.Concat(match.Select(s => s.server).Where(s => !toRemove.Contains(s))).ToArray();
                toRemove = toRemove.Where(s => !match.Select(p => p.server).Contains(s)).ToArray();
                busy = busy.Where(s => s.server != d.pid).ToArray();

                Send(match.Single().client, new BalancerResponse { body = d.body, pid = this.PID });
                return true;
            }
            else if (data.Signature == "add")
            {
                var d = data as UpdateList;
                free = free.Concat(d.pid).ToArray();
                return true;
            }
            else if (data.Signature == "remove")
            {
                var d = data as UpdateList;
                if (free.Contains(data.ToString()))
                    free = free.Where(s => s != d.pid).ToArray();
                else
                    toRemove = toRemove.Concat(d.pid).ToArray();
                return true;
            }

            return false;
        }
        public Balancer(params string[] pids)
        {
            free = pids;
        }
    }
}
