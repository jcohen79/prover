using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab
{
    /// <summary>
    /// Simplify connecting together producers, consumers. There can be multiple consumers watching producer,
    /// and the consumer continues to wait for multiple inputs. Perform one operation on each call to Stepper.
    /// </summary>
    public abstract class Pcu1<TinL, Tout> : Pcc<TinL>
        where TinL: class
        where Tout : class
    {
        Tout outConsumer;
        readonly Pri priScheduler;

        public Pcu1(Pri priScheduler)
        {
            this.priScheduler = priScheduler;
            priScheduler.Add(this);
        }

        public void SetConsumer(Tout outConsumer)
        {
            this.outConsumer = outConsumer;
        }

        public abstract void TransferLeft(TinL inlLeft);

        /// <summary>
        /// Process one unit of work.
        /// Return false to terminate calls to this.
        /// </summary>
        public abstract bool fStep();

        public abstract Ipr iprComplexity();
        public abstract Ipr iprPriority();

        public abstract int nSize();
    }

    public interface Ipp<TinL, TinR>
        where TinL : class
        where TinR : class
    {
        void ProcessPair(TinL ascLeft, TinR ascRight);
    }

    public abstract class Pcu2<TinL, TinR> : Bid, Pcc<TinL>
    where TinL : class
    where TinR : class
    {
        protected readonly Res res;

        public Pcu2(Res res)
        {
            this.res = res;
        }

        public abstract Ipr iprPriority();
        public abstract bool fStep();

        public abstract void TransferLeft(TinL inrLeft);

        /// <summary>
        /// Implementation takes input and adds to Bas. Converts to interal class if needed.
        /// </summary>
        public abstract void TransferRight(TinR inrRight);

        public void Log(string stText)
        {
            if (res.irp != null)
                res.irr.EqsLog(stText);
        }

        public abstract int nSize();

        public abstract Ipp<TinL, TinR> ippGet();
    }

    /// <summary>
    /// Consumer: is called by a provider
    /// </summary>
    public interface Pcc<Tin> : Sprc
    where Tin : class
    {
        void TransferLeft(Tin inInput);
    }
}
