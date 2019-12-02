using GraphMatching;
using reslab.TestUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab
{

    public class RestartException : Exception
    {
        public RestartException()
        {

        }
    }

    /// <summary>
    /// List of actions attached
    /// </summary>
    public interface Iac
    {
        long nIdToWatch();
        Iac iacGetNext();
        void SetNext(Iac iacNext);
        void Perform(Bid bidFound);
    }

    /// <summary>
    /// Test implements Iba to be called when a Bid that was been registered is re-created.
    /// </summary>
    public interface Iba
    {
        void FoundBid(Tbi tbi, Bid bidFound);
    }

    /// <summary>
    /// Action to be performed in the correct sequence as objects are re-created so that existing
    /// nId_s are reassigned before new Bid get created.
    /// </summary>
    public interface Isp : Iac
    {
        bool fGetImmediate();
        void SetImmediate();
    }

    /// <summary>
    /// Record info when adding watchpoint on creation of Bid, based on its nId
    /// </summary>
    public class Tbi : Tpf, Iac
    {
        public Iac iacNext;
        public readonly long nIdWatch;
        public readonly Iba ibaToDo;
        public readonly Psb psbDest;
        public readonly Tfc tfc;

        public Tbi(long nIdToWatch, Iba ibaToDo, Tfc tfc, Psb psbDest, Iac iacPrev, Tlr tlr, string stLabel) 
            : base(tlr, stLabel)
        {
            this.nIdWatch = nIdToWatch;
            this.ibaToDo = ibaToDo;
            this.psbDest = psbDest;
            this.iacNext = iacPrev;
            this.tfc= tfc;
        }

        public Iac iacGetNext()
        {
            return iacNext;
        }

        public long nIdToWatch()
        {
            return nIdWatch;
        }

        public void Perform(Bid bidFound)
        {
            ibaToDo.FoundBid(this, bidFound);
        }

        public void SetNext(Iac iacNext)
        {
            this.iacNext = iacNext;
        }
    }

    /// <summary>
    /// Used where an interface is required, but it is really a Bid.
    /// </summary>
    public interface Ibd
    {
        long nGetId();

        Type typeOf();

        string stDesc(Tsc tsc);
    }

    /// <summary>
    /// Object that has an id and can be used to set traps to track when an object with specific nId is created.
    /// Base class for most classes in prover.
    /// </summary>
    public abstract class Bid : Ibd
    {
        private static long cId = 0;
        public readonly long nId = cId++;
#if DEBUG
        // non-reset
        private static Iac iacSWatchHead = null;
        private static Iac iacSWatchTail = null;
        public static Bid bidMaxId = null;
#endif

        public long nGetId()
        {
            return nId;
        }

        public Type typeOf()
        {
            return GetType();
        }

        public string stDesc(Tsc tsc)
        {
            return tsc.stIdentifier(this);
        }

        public static void AddIsp(Isp isp)
        {
            if (isp.fGetImmediate())
                isp.Perform(null);
            else if (bidMaxId == null || cId > bidMaxId.nId)
            {
                isp.SetImmediate();
                isp.Perform(null);
            }
            else
            {
                AddIac(isp);
            }
        }

        public override string ToString()
        {
            Fwt sb = new Fwt();
            stString(Pctl.pctlPlain, sb);
            return sb.stString();
        }
        public string stPretty()
        {
            Fwt sb = new Fwt();
            stString(Pctl.pctlVerbose, sb);
            return sb.stString();
        }
        public string stIdentifier()
        {
            Fwt sb = new Fwt();
            stString(Pctl.pctlIdentifier, sb);
            return sb.stString();
        }


        public virtual void stString(Pctl pctl, Fwt sb)
        {
            // should override and use flags in Pctl
            sb.Append("<" + GetType().Name + "#" + nId + ">");
        }

        public static bool fAddWatch(long nIdToTrigger, Tfc tfc, Psb psbDest, Iba ibaToDo, Tlr tlr, string stLabel)
        {
#if DEBUG
            for (Iac iacCheck = iacSWatchHead;
                iacCheck != null; iacCheck = iacCheck.iacGetNext())
            {
                if (iacCheck is Tbi)
                {
                    Tbi tbiCheck = (Tbi)iacCheck;
                    if (nIdToTrigger == tbiCheck.nIdToWatch()
                        && tbiCheck.tfc == tfc
                        && tbiCheck.psbDest == psbDest
                        && tbiCheck.ibaToDo == ibaToDo)
                    {
                        return false;
                    }
                }
            }
            Tbi tbiNew = new Tbi(nIdToTrigger, ibaToDo, tfc, psbDest, null, tlr, stLabel);
            AddIac(tbiNew);
#endif
            return true;
        }

        public static void AddIac(Iac iacNew)
        {
            if (iacSWatchTail == null)
                iacSWatchHead = iacNew;
            else
                iacSWatchTail.SetNext(iacNew);
            iacSWatchTail = iacNew;
        }

        public static void Reset()
        {
            cId = 0;
            Vbv.Reset();
            Tcd.Reset();
            Gfc.Reset();
            Tfs.Reset();
        }

        static Bid()
        {
            // do all static inits of Bids here so that they are done in a consistent order.
            Reset();
        }

        static Dictionary<long, Type> mpn_type = new Dictionary<long, Type>();

        public Bid()
        {
#if true
            if (mpn_type == null)
                mpn_type = new Dictionary<long, Type>();
            Type typePrev;
            if (mpn_type.TryGetValue(nId, out typePrev))
            {
                Type typeThis = GetType();
                if (typePrev != typeThis)
                    Debug.WriteLine("wrong type for " + nId + ", was " + typePrev.Name
                        + ", now " + typeThis.Name);
            }
            else
                mpn_type[nId] = GetType();

           // if (nId == 2343) //  && nId > 165)
           //    Debug.WriteLine("bid " + nId + " is " + GetType().ToString());

            if (bidMaxId == null || bidMaxId.nId < nId)
                bidMaxId = this;
#endif
            for (Iac iacCheck = iacSWatchHead; 
                iacCheck != null; iacCheck = iacCheck.iacGetNext())
            {
                if (nId == iacCheck.nIdToWatch())
                {
                    // don't disable: need to repeat the same steps each time
                    iacCheck.Perform(this);
                }
            }
        }

        public void Verbose()
        {
            Debug.WriteLine(stPretty());
        }

    }
}
