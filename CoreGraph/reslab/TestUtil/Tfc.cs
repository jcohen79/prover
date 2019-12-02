using System;
using System.Collections.Generic;

namespace reslab.TestUtil
{
    /// <summary>
    /// Handle a tracing step for a Tfc controller.
    /// Handle the notifications for tracing by passing to controller
    /// </summary>
    public class Tfs : Tpf
    {
        public readonly Tfc tfcController;
        Psb psbNext;
        static Dictionary<Pid, Tfs> mppid_Tfs;

        private Tfs(Tlr tlr, string stLabel, Tfc tfcController, Psb psbNext) : base(tlr, stLabel)
        {
            this.tfcController = tfcController;
            this.psbNext = psbNext;
        }

        static Tfs()
        {
            Reset();
        }

        public static void Reset()
        {
            mppid_Tfs = new Dictionary<Pid, Tfs>();
        }

        public static Tfs tfsMake(Tlr tlr, string stLabel, Tfc tfcController, Psb psbNext)
        {
            Tfs tfs = new Tfs(tlr, stLabel, tfcController, psbNext);
     //       mppid_Tfs[pid] = tfs;
            return tfs;
        }


        public override bool fPerform(object objLeft, object objRight, object objData, Tcd tcdEvent)
        {
            Tif tifSave = null;
            if (tfcController.tfm.tscOutput != null)
                tifSave = tfcController.tfm.tscOutput.tifCurrent;
            bool res = tfcController.fNextStep(tfcController, psbNext,
                                           objLeft, objRight, objData, tcdEvent);
            if (tfcController.tfm.tscOutput != null)
                tfcController.tfm.tscOutput.tifCurrent = tifSave;
            return res;
        }
    }

    /// <summary>
    /// Type indicator
    /// </summary>
    public class Tin
    {
        public readonly string stName;

        public static readonly Tin tinAsc = new Tin("Asc");
        public static readonly Tin tinPti = new Tin("Pti");
        public static readonly Tin tinNgc = new Tin("Ngc");

        // public Dictionary<string, Ivb> mpst_ivbForTin = new Dictionary<string, Ivb>();


        public Tin(string stName)
        {
            this.stName = stName;
        }
        /*
        public void set(string stEntryName, Ivb ivb)
        {
            mpst_ivbForTin.Add(stEntryName, ivb);
        }

        public Ivb get (string stEntryName)
        {
            return mpst_ivbForTin[stEntryName];
        } */
    }

    /// <summary>
    /// Base class for controller to trace through steps of proof.
    /// Implementations provide a place to store objects for later use, and to
    /// make the progress easier to follow using a switch stmt.
    /// </summary>
    public abstract class Tfc : Iba
    {
        public readonly Tfm tfm;
        protected int nTfcId;
        public readonly Tfc tfcParent;

        /// <summary>
        /// return true to remove
        /// </summary>
        public virtual bool fNextStep(Tfc tfcPhase, Psb psbNext,
                             object objLeft, object objRight, object objData, Tcd tcdEvent)
        {
            return tfcParent.fNextStep(tfcPhase, psbNext, objLeft, objRight, objData, tcdEvent);
        }

        public abstract void Init();

        public Tfc(Tfm tfm, int nTfcId, Tfc tfcParent)
        {
            this.tfm = tfm;
            this.nTfcId = nTfcId;
            this.tfcParent = tfcParent;
        }
        
        public int nId()
        {
            return nTfcId;
        }

        public void Restart()
        {
            Tpf.Restart();
        }

        public abstract TfcTop tfcGetTop();

        public void FoundBid(Tbi tbi, Bid bidFound)
        {
            // resume based on save stated
            Tif tifSave = null;
            if (tfm.tscOutput != null)
                tifSave = tfm.tscOutput.tifCurrent;
            tbi.tfc.fNextStep(tbi.tfc, tbi.psbDest, null, null, bidFound, Tcd.tcdNewBid);
            if (tfm.tscOutput != null)
                tfm.tscOutput.tifCurrent = tifSave;
        }

        /// <summary>
        /// Find entry in hashSet that is equal to give term
        /// </summary>
        public static Vbv vbvFindDup(HashSet<Vbv> rgvbvHaystack, Vbv vbvNeedle)
        {
            foreach (Vbv vbvIn in rgvbvHaystack)
            {
                if (vbvIn.Equals(vbvNeedle))
                    return vbvIn;
            }
            return vbvNeedle;
            // throw new ArgumentException();
        }

        public void WatchPair(Bid objTarget, Bid objInput, Tcd tcdEvent, string stLabel, Psb psb)
        {
            tfm.WatchPair(this, objTarget, objInput, tcdEvent, stLabel, psb);
        }

        public void WatchTarget(Bid objTarget, Tcd tcdEvent, string stLabel, Psb psb)
        {
            tfm.WatchTarget(this, objTarget, tcdEvent, stLabel, psb);
        }

        // Convenience function for manual code to add action to invoke when bidToWatch is created again
        public bool fWatch(Bid bidToWatch, Psb psbDest, string stLabel)
        {
            return tfm.fWatch(this, bidToWatch, psbDest, stLabel);
        }

        public void SetPtiAllowEprNonVbl(Bid bidPti)
        {
            Res.nPtiAllowEprNonVbl = bidPti.nId;  // hack until on-demand nonvbl epr. See kHaveEqsFor3rdNgc
                                                  // this fn is only called during AddAxioms during init, so is not needed in generated code
            //if (tfm.tscOutput != null)
            //    tfm.tscOutput.SetPtiAllowEprNonVbl(bidPti);
        }

        public void SetEqsAllowEprNonVbl(Bid bidEqs)
        {
            Eqs.nEqsAllowEprNonVbl = bidEqs.nId;  // apply hack until on-demand nonvbl epr works
            if (tfm.tscOutput != null)
                tfm.tscOutput.SetEqsAllowEprNonVbl(bidEqs);
        }
    }

    public interface Itt
    {
        void SetPtiAllowEprNonVbl(Bid bidEqs);
        void SetEqsAllowEprNonVbl(Bid bidEqs);
    }


    /// <summary>
    /// Base class for top level controller for the proof outline for a specific proof
    /// </summary>
    public abstract class TfcTop : Tfc, Irt, Inr, Isr, Itt
    {

        public TfcTop(Tfm tfm, Pou pou) : base(tfm, -1, null)
        {
        }

        public override TfcTop tfcGetTop()
        {
            return this;
        }

        public abstract void ReadyToRun(Res res);

        public override bool fNextStep(Tfc tfcPhase, Psb psbNext, object objLeft, object objRight, object objData, Tcd tcdEvent)
        {
            throw new InvalidProgramException();
        }


        public abstract void SetRes(Res res);
        public abstract void RegisterPreset(Bid bid, string stLabel);
        public abstract void Register(KCapture kSource, Bid bid, Isb psbStmt);
    }

    /// <summary>
    /// Hold several objects used in a proof
    /// </summary>
    public class Tfm
    {
        public Irs[] rgirsTestHook;
        public Pmb pmbResolution;
        public Tsc tscOutput;
        public Drt drt;
        public Res res;
        public Cbd cbd;

        public void SetTestHook(params Irs[] rgirhTestHook)
        {
            this.rgirsTestHook = rgirhTestHook;
        }

        public void SetVhd<T>(Tin tin, Vhd<T> vhd, T tVal) where T: Ibd
        {
            if (tscOutput != null)
                tscOutput.SetVhd(tin, vhd.stName, tVal);
            vhd.Set(tVal);
        }

        public void WatchPair(Tfc tfc, Ibd objTarget, Ibd objInput, Tcd tcdEvent, string stLabel, Psb psb)
        {
            if (tscOutput != null)
                tscOutput.WatchPair(objTarget, objInput, tcdEvent, stLabel, tfc, psb);
            drt.WatchPair(objTarget, objInput, tcdEvent, Tfs.tfsMake(drt, stLabel, tfc, psb));
        }

        public void WatchTarget(Tfc tfc, Ibd objTarget, Tcd tcdEvent, string stLabel, Psb psb)
        {
            if (tscOutput != null)
                tscOutput.WatchTarget(objTarget, tcdEvent, stLabel, tfc, psb);
            drt.WatchTarget(objTarget, tcdEvent, Tfs.tfsMake(drt, stLabel, tfc, psb));
        }

        public void WatchInput(Tfc tfc, Ibd objInput, Tcd tcdEvent, string stLabel, Psb psb)
        {
            if (tscOutput != null)
                tscOutput.WatchInput(objInput, tcdEvent, stLabel, tfc, psb);
            drt.WatchInput(objInput, tcdEvent, Tfs.tfsMake(drt, stLabel, tfc, psb));
        }

        // Set up an action to be invoked when bidToWatch is created again
        public bool fWatch(Tfc tfc, Ibd bidToWatch, Psb psbDest, string stLabel)
        {
            bool fAddedToBid = Bid.fAddWatch(bidToWatch.nGetId(), tfc, psbDest, tfc, drt, stLabel);
            if (fAddedToBid)
            {
                if (tscOutput != null)
                {
                    tscOutput.fWatch(bidToWatch, tfc, psbDest, stLabel);
                }
            }
            else
            {
                if (tscOutput != null)
                {
                    Tif tifCurrent = tscOutput.tifCurrent;
                    tifCurrent.tiwSave.IncrementAfter(tifCurrent);
                }
            }
            return fAddedToBid;
        }

        public void InstrumentedProof(TfcTop tfc, string stProblem)
        {
            cbd = new Cbd();
            Lsx lsxProblem = cbd.lparse.lsxParse(stProblem);
            while (true)
            {
                Bid.Reset();
                if (rgirsTestHook != null)
                {
                    foreach (Irs irs in rgirsTestHook)
                        irs.Reset();
                }

                drt = new Drt();
                res = new Res(imd: pmbResolution, irr: drt);
                res.irp = drt;
                res.isrRegister = tfc;
                if (rgirsTestHook.Length > 0)
                    res.irhTestHook = (Irh) rgirsTestHook[0];
                res.Init();
                tfc.SetRes(res);

                res.fVerifyEachStep = false;
                try
                {
                    tfc.Init();
                    // fNextStep(null, nFirstStep, null, null, null, null);
                    // ResTest.DoProof(stProblem, res, cbd.lparse);    don't expect proof to be found
                    // run until Etr says res is done.

                    Asc ascProof = res.ascProve(lsxProblem, 1);

                    break;
                }
                catch (RestartException)
                { }
            }
        }

    }

    public interface Irt
    {
        void ReadyToRun(Res res);
    }

    /// <summVary>
    /// Does not run map res.gnpAscAsc spr
    /// </summary>
    public class Prtfc : Prm
    {
        Irt tfc;

        public Prtfc (Irt tfc) : base (false)
        {
            this.tfc = tfc;
        }

        public override void ReadyToRun(Res res)
        {
            tfc.ReadyToRun(res);
        }
    }

}
