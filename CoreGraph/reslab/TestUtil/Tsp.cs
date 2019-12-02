using System;
using System.Collections.Generic;

namespace reslab.TestUtil
{
    public class Pid
    {
        public readonly long nId1;
        public readonly long nId2;

        public Pid(long nId1, long nId2)
        {
            this.nId1 = nId1;
            this.nId2 = nId2;
        }

        public override bool Equals(Object other)
        {
            if (!(other is Pid))
                return false;
            Pid pidO = (Pid)other;
            if (nId1 != pidO.nId1)
                return false;
            if (nId2 != pidO.nId2)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = -1828525943;
            hashCode = hashCode * -1521134295 + nId1.GetHashCode();
            hashCode = hashCode * -121134295 + nId2.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Injected to prover to capture objects that are handled specially in test
    /// </summary>
    public class Adh : Irh
    {
        Dictionary<Pid, Tpp> mppid_tpp = new Dictionary<Pid, Tpp>();

        public Adh() { }

        public void AddDeferredPti(Rpi rpi)
        {
            Pid pid = new Pid(rpi.lsm.nId, rpi.bid().nId);
            Tpp tpp;
            if (mppid_tpp.TryGetValue(pid, out tpp))
            {
                tpp.rpi = rpi;
                if (tpp.fGetImmediate())
                    tpp.Perform();
            }
            else
            {
                tpp = new Tpp(rpi);
                mppid_tpp[pid] = tpp;
                Bid.AddIsp(tpp);
            }
        }

        public void Reset()
        {
        }
    }

    /// <summary>
    /// State for call to StartPair
    /// </summary>
    public abstract class Tsb : Isp
    {
        Iac iacNext;
        bool fImmediate;
        long nIdWatch;
        static long cId = 0;
        long nId = cId++;

        public Tsb()
        {
            this.nIdWatch = Bid.bidMaxId.nId;
        }
        public bool fGetImmediate()
        {
            return fImmediate;
        }
        public void SetImmediate()
        {
            this.fImmediate = true;
        }
        public Iac iacGetNext()
        {
            return iacNext;
        }

        public void SetNext(Iac iacNext)
        {
            this.iacNext = iacNext;
        }

        public abstract void Perform();
        public void Perform(Bid bid)
        {
            Perform();
        }

        public long nIdToWatch()
        {
            return nIdWatch;
        }
    }


    /// <summary>
    /// State for call to StartPair
    /// </summary>
    public class Tsp : Tsb
    {
        Tfc tfc;
        public Asc ascLeft;
        public Asc ascRight;
        Psb psbFirst;

        public Tsp(Tfc tfc, Asc ascLeft, Asc ascRight, Psb psbFirst)
        {
            this.tfc = tfc;
            this.ascLeft = ascLeft;
            this.ascRight = ascRight;
            this.psbFirst = psbFirst;
        }

        public override void Perform()
        {
            tfc.WatchPair(ascLeft, ascRight, Tcd.tcdMakeEpu,
                      "make Epu from left and right", psbFirst);
            tfc.tfm.res.gnpAscAsc.ProcessPair(ascLeft, ascRight);
        }
    }

    /// <summary>
    /// State for nonvbl Pti, Lsm
    /// Wrap the rpi that is provided by system under test
    /// </summary>
    class Tpp : Tsb
    {
        public Rpi rpi;

        public Tpp(Rpi rpi)
        {
            this.rpi = rpi;
        }
        public override void Perform()
        {
            rpi.Perform();
        }
    }

}
