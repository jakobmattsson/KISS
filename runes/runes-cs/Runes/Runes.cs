using System;
using System.Collections.Generic;
using System.Linq;

class Input
{
    // Data types
    class RuneData
    {
        public int next;
        public int count;
        public long value;
        public double mare;
        public double hell;
        public string name;
    }
    class Solution
    {
        public int ctr;
        public int ctrMemo;
        public int ctrCalc;
        public double prob;
        public IDictionary<string, double> memos = new Dictionary<string, double>();
    }

    Solution Solve(string runeName, int mareS, int hellS, RuneData[] runes) 
    {
        var sol = new Solution();
        var rune = runes.Select((s, i) => s.name == runeName ? i : -1).Single(s => s != -1);

        runes[0].value = 1;
        for (var i = 1; i < runes.Count(); i++)
            runes[i].value = runes[i - 1].value * runes[i - 1].next;
        
        Func<long, int, int, double> Divide = null;
        Divide = (totalValue, hell, mare) =>
        {
            sol.ctr++;
            if (hell + mare == 0)
                return totalValue >= runes[rune].value ? 1.0 : 0.0;

            var dtr = totalValue + " " + hell + " " + mare;
            if (!sol.memos.ContainsKey(dtr))
            {
                if (++sol.ctrCalc % 10000 == 0)
                    Console.WriteLine(sol.ctrCalc + " cases calculated so far");
                sol.memos.Add(dtr, runes.Select((s, i) => new { s.mare, i, prob = hell > 0 ? s.hell : s.mare, val = (i <= rune ? s.value : 0) }).Where(s => s.prob > 0).Sum(v => v.prob * Divide(totalValue + v.val, hell - (hell > 0 ? 1 : 0), mare - (hell > 0 ? 0 : 1))));
            }
            else
                sol.ctrMemo++;

            return sol.memos[dtr];
        };

        sol.prob = Divide(runes.Where((s, i) => i <= rune).Sum(s => s.value * s.count), hellS, mareS);
        return sol;
    }

    // Main
    static void Main(string[] args)
    {
        var rune = "mal";
        var nightmareRuns = 1;
        var hellRuns = 3;
        var runes = new RuneData[]{
            new RuneData{ next = 3, name = "sol",   mare = 1.0/11, hell = 0, count = 0},
            new RuneData{ next = 3, name = "shael", mare = 1.0/11, hell = 0},
            new RuneData{ next = 3, name = "dol",   mare = 1.0/11, hell = 0},
            new RuneData{ next = 3, name = "hel",   mare = 1.0/11, hell = 1.0/11},
            new RuneData{ next = 3, name = "io",    mare = 1.0/11, hell = 1.0/11},
            new RuneData{ next = 3, name = "lum",   mare = 1.0/11, hell = 1.0/11},
            new RuneData{ next = 3, name = "ko",    mare = 1.0/11, hell = 1.0/11},
            new RuneData{ next = 3, name = "fal",   mare = 1.0/11, hell = 1.0/11},
            new RuneData{ next = 3, name = "lem",   mare = 1.0/11, hell = 1.0/11},
            new RuneData{ next = 3, name = "pul",   mare = 1.0/11, hell = 1.0/11},
            new RuneData{ next = 2, name = "um",    mare = 1.0/11, hell = 1.0/11, count = 1},
            new RuneData{ next = 2, name = "mal",   mare = 0,      hell = 1.0/11},
            new RuneData{ next = 2, name = "ist",   mare = 0,      hell = 1.0/11},
            new RuneData{ next = 2, name = "gul",   mare = 0,      hell = 1.0/11},
            new RuneData{ next = 2, name = "vex",   mare = 0,      hell = 0},
            new RuneData{ next = 2, name = "ohm",   mare = 0,      hell = 0},
            new RuneData{ next = 2, name = "lo",    mare = 0,      hell = 0},
            new RuneData{ next = 2, name = "sur",   mare = 0,      hell = 0},
            new RuneData{ next = 2, name = "ber",   mare = 0,      hell = 0},
            new RuneData{ next = 2, name = "jah",   mare = 0,      hell = 0},
            new RuneData{ next = 2, name = "cham",  mare = 0,      hell = 0},
            new RuneData{ next = 0, name = "zod",   mare = 0,      hell = 0},
        };

        if (args.Length > 0 && args[0] == "-s")
            Console.WriteLine("{0:0.0}", new Input().Solve(rune, nightmareRuns, hellRuns, runes).prob * 100);
        else
        {
            Console.WriteLine("Trying to find the rune '{0}' using hellforge runs; {1} in nightmare and {2} in hell...", rune, nightmareRuns, hellRuns);
            var time = DateTime.Now;
            var result = new Input().Solve(rune, nightmareRuns, hellRuns, runes);
            Console.WriteLine("Done! The search took {0} milliseconds.", (int)DateTime.Now.Subtract(time).TotalMilliseconds);
            Console.WriteLine("Total recursive calls:   {0}", result.ctr);
            Console.WriteLine("Calls using memoization: {0}", result.ctrMemo);
            Console.WriteLine("Calls using calculation: {0}", result.ctrCalc);
            Console.WriteLine();
            Console.WriteLine("And the probability of success is: {0:0.0}%", result.prob * 100);
            Console.WriteLine("Press any key to quit...");
            Console.ReadKey();
        }
    }
}
