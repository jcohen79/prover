using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace reslab
{

    /// <summary>
    /// Called to perform a single unit of computation
    /// </summary>
    public interface Spr
    {
        /// <summary>
        /// Do one unit of work. Return true to terminate this item.
        /// </summary>
        bool fStep();

        /// <summary>
        /// Number of items included
        /// </summary>
        int nSize();
    }

    public interface Sprc : Icmp, Spr
    {

    }

    /// <summary>
    /// Call notify scheduler of the priority of the next item of work for the Stp
    /// </summary>
    public interface Pri
    {
        void Add(Spr spr);
    }

    public class Tli
    {
        public Tli tliPrev;
        public Tli tliNext;
        public Spr sprItem;
    }


    /// <summary>
    /// Execute queued tasks
    /// </summary>
    public class Prs : Pri
    {
        public HeapLoop<Sprc> rgsprHeap = new HeapLoop<Sprc>();
        public Loop<Spr> rgsprLoop = new Loop<Spr>();
        Queue<Spr> rgsprManual;
        bool fHeapNext = false;
        Res res;
        bool fTimeLimit;
        bool fTimedOut = false;

        public static Ipr iprDefaultPriority = Prn.prnZero;

        public Prs(Res res, bool fTimeLimit)
        {
            this.res = res;
            this.fTimeLimit = fTimeLimit;
        }

        public void AddManual(Spr spr)
        {
            if (rgsprManual == null)
                rgsprManual = new Queue<Spr>();
            rgsprManual.Enqueue(spr);
        }

        public void Add(Spr spr)
        {
            if (spr is Sprc)
                rgsprHeap.Add((Sprc)spr, true);
            else
                rgsprLoop.Add(spr);
        }

        public bool fEmpty()
        {
            return rgsprHeap.fEmpty();
        }

        public void Done()
        {
            rgsprHeap = new HeapLoop<Sprc>();
            rgsprLoop.Clear();
            fHeapNext = false;
        }

        void OutOfTime(object sender, ElapsedEventArgs arg)
        {
            fTimedOut = true;
        }

        public void Run(double msTimeLimit = 60000)
        {
            Timer timer = null;
            int nLoops = 0;
            try
            {
                if (msTimeLimit > 0 && fTimeLimit)
                {
                    timer = new Timer(msTimeLimit);
                    timer.Elapsed += OutOfTime;
                    timer.Start();
                }

                while (!fTimedOut)
                {
                    if (rgsprManual != null)
                    {
                        try
                        {
                            Spr sprManual = rgsprManual.Dequeue();
                            sprManual.fStep();
                            continue;
                        }
                        catch (InvalidOperationException )
                        {
                            rgsprManual = null;
                        }
                    }

                    if (fHeapNext && !rgsprHeap.fEmpty())
                    {
                        fHeapNext = false;
                        Sprc sprc = rgsprHeap.tGet();
                        if (!sprc.fStep())
                            rgsprHeap.Add(sprc, false);
                    }
                    else
                    {
                        fHeapNext = true;
                        Spr spr = rgsprLoop.tPeek();
                        if (spr == null)
                        {
                            if (rgsprHeap.fEmpty())
                                break;
                        }
                        else
                        {
                            if (spr.fStep())
                            {
                                if (spr == res)
                                {
                                    if (rgsprHeap.fEmpty() && rgsprLoop.fOnlyActive(res))
                                        break;
                                }
                                else
                                {
                                    if (rgsprLoop.tPeek() == null)  // Done
                                        continue;
                                    rgsprLoop.Remove();
                                }
                            }
                            rgsprLoop.MoveToNext();

                        }
                    }
                    if (nLoops++ > 1000000)
                    {
                        Debug.WriteLine("\nnLoops:" + nLoops);
                        Show();
                        nLoops = 0;
                    }
                }
            }
            finally
            {
                if (timer != null)
                    timer.Stop();
            }
            if (res.ascProof == null)
            {
                Done();
                if (res.irr != null)
                    res.irr.NoProof();
            }
        }

        public void Show()
        {
            new Cpr(this).Show();

        }
    }

    public class Cnt
    {
        public int nCount = 0;
        public int nTotal = 0;
    }

    /// <summary>
    /// Perform counts on a Prs
    /// </summary>
    class Cpr
    {
        Dictionary<Type, Cnt> mpt_cntCounts = new Dictionary<Type, Cnt>();

        void Add(Spr spr)
        {
                Type type = spr.GetType();
                Cnt cntPrev;
                if (!mpt_cntCounts.TryGetValue(type, out cntPrev))
                {
                    cntPrev = new Cnt();
                    mpt_cntCounts.Add(type, cntPrev);
                }
                cntPrev.nCount++;
                cntPrev.nTotal += spr.nSize();

        }

        public Cpr(Prs prs)
        {
            foreach(Spr spr in prs.rgsprLoop)
            {
                Add(spr);
            }
            foreach(Sprc sprc in prs.rgsprHeap)
            {
                Add(sprc);
            }

        }

        public void Show()
        {
            foreach (KeyValuePair<Type,Cnt> pair in mpt_cntCounts)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(pair.Key.Name);
                sb.Append(": ");
                sb.Append(pair.Value.nCount);
                sb.Append(", ");
                sb.Append(pair.Value.nTotal);
                Debug.WriteLine(sb.ToString());
            }
        }
    }
}
