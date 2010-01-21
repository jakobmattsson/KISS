using System;
using System.Linq;
using System.Threading;

namespace Erlang
{
    public static class Methods
    {
        public static void Double(this BaseHandler handler, int v)
        {
            handler.Send("doubler1", new BalancerRequest { pid = handler.PID, body = new BinOpServer.Data { input = v } });
        }
        public static void Inc(this BaseHandler handler, int v)
        {
            handler.Send("incer", new BalancerRequest { pid = handler.PID, body = new BinOpServer.Data { input = v } });
        }
        public static void Square(this BaseHandler handler, int v)
        {
            handler.Send("squarer", new BalancerRequest { pid = handler.PID, body = new BinOpServer.Data { input = v } });
        }

        public static void Some(this BaseHandler handler, int v)
        {
            handler.Send("balancer", new BalancerRequest { pid = handler.PID, body = new BinOpServer.Data { input = v } });
        }

        public static Random rand = new Random();

        public static double Rand()
        {
            return rand.NextDouble();
        }
    }

    public class BinOpServer : BaseHandler
    {
        public class Data
        {
            public int input;
            public int output;

            public override string ToString() { return string.Format("in: {0}, out: {1}", input, output); }
        }

        public int sleep;
        public Func<int, int> f;

        public override bool Handler(IMessage data)
        {
            switch (data.Signature)
            {
                case "request":
                    var d = data as BalancerRequest;
                    var bod = d.body as Data;
             
                    var doub = sleep * (1 + 0 * 2 * (Methods.Rand() - 0.5));
                    Thread.Sleep((int)doub);
                    
                    Send(d.pid, new BalancerResponse { pid = PID, body = new Data { input = bod.input, output = f(bod.input) } });
                    return true;
                case "create":
                    Console.WriteLine("Created binopserver " + this.PID);
                    return true;
            }
            return false;
        }
    }
    public class MainProc : BaseHandler
    {
        public override bool Handler(IMessage data)
        {
            if (data.Signature == "create")
            {
                foreach (var v in Enumerable.Range(1, 1000))
                    this.Some(v);
                return true;
            }
            if (data.Signature == "response")
            {
                Console.WriteLine(DateTime.Now + " Response: " + data.ToString());

                if (Methods.Rand() > 0.9)
                {
                    var v = Spawn(new BinOpServer { f = a => a + a, sleep = 200 });
                    Send("balancer", new UpdateList { Signature = "add", pid = v });
                }

                return true;
            }

            return false;
        }
    }
    public class Samling : BaseHandler
    {
        public override bool Handler(IMessage data)
        {
            if (data.Signature == "getSite1Level1")
            {
                // "data" innehåller information om vilken sida som ska hämtas
//                var html = "...";
                // parsa htmln
                Send("instanceBalancer", null);
                return true;
            }
            if (data.Signature == "getSite1Level2")
            {
                // "data" innehåller viss information om filen, bland annat vilken html som nu ska hämtas
//                var html = "...";
                Send("store", null); // här ska den fullständiga datan in
                return true;
            }

            return false;
        }
    }

    public static class MainClass
    {
        static void Main(string[] args)
        {
            int s = 20;

            // en startsida. denna genererar x antal jobb:
            // - samlingssida 1
            // - samlingssida 2
            // - ...
            // - samlingssida n

            // varje samlingssida ger upphov till (eventuellt) en ny samlingssida
            // och till ett antal instanssidor:
            // - instans 1
            // - instans 2
            // - ...
            // - instans m





            var dis = new Dispatcher();
            dis.Spawn("d1", new BinOpServer { f = a => a + a, sleep = s });
            //dis.Spawn("d2", new BinOpServer { f = a => a + a, sleep = s });
            //dis.Spawn("d3", new BinOpServer { f = a => a + a, sleep = s });
            //dis.Spawn("d4", new BinOpServer { f = a => a + a, sleep = s });
            //dis.Spawn("d5", new BinOpServer { f = a => a + a, sleep = s });
            //dis.Spawn("d6", new BinOpServer { f = a => a + a, sleep = s });
            //dis.Spawn("d7", new BinOpServer { f = a => a + a, sleep = s });
            //dis.Spawn("d8", new BinOpServer { f = a => a + a, sleep = s });
            //dis.Spawn("d9", new BinOpServer { f = a => a + a, sleep = s });
            //dis.Spawn("d10", new BinOpServer { f = a => a + a, sleep = s });
            //dis.Spawn("balancer", new Balancer("d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8", "d9", "d10"));
            dis.Spawn("balancer", new Balancer("d1"));
            dis.Spawn("main", new MainProc());
            dis.Block();
        }
    }
}

// request, pid, msg - pid är den som skickat meddeladnet (så att servern vet vem som ska svaras till)
// response, pid, msg - pid är serverns pid, så att klienten vet vad det är för tjänst som svarar
// använd logiken för site X
// följ spår som den visar, branscha ut nya trådar för varje spår
