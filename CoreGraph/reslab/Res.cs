#define DEBUG
//#define CHECK_SUBSUMES

using GraphMatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace reslab
{
    public interface Opc
    {
        bool fUseOnDemand(Pti ptiNew);
    }

    public interface Isi : Ibd
    {
        Lsx lsxTo(Asy asy);

        void stString(Pctl pctl, Fwt sb);
    }

    /// <summary>
    /// Different implementations of paramodulation
    /// </summary>
    public interface Ipt
    {
        void SavePti(Pti ptiNew);
    }

    public interface Ipa
    {
        void PtiAdded(Pti ptiNew);
    };

    // where an object was seen
    public enum KCapture
    {
        kPreset,
        kUsage,
        kLeft,
        kRight,
        kData,
        kResult
    }

    /// <summary>
    /// Refers to a Psb/Tst, used to save a value obtained during proof
    /// </summary>
    public interface Isb
    {

    }

    public interface Isr
    {
        void RegisterPreset(Bid bid, string stLabel);
        void Register(KCapture kSource, Bid bid, Isb psbStm);
    }
   
    /// <summary>
    /// A placeholder that can hold a Bid
    /// </summary>
    public interface Vrf
    {
        Bid bidGetValue();
    }

    public interface Iss : Isr
    {
        Bid bidGet(Vrf ibdInput);
    }
    
    /// <summary>
    /// Main class for resolution theorem prover
    /// </summary>
    public class Res : Bid, Sprc, Ipt
    {
        public static Res resLatest;
        public Asy asyDefault;
        public  Irp irp;
        public readonly Irr irr;
        public GnpR gnpAscAsc;
        public Prs prs;
        public Asc ascProof;
        private Dictionary<Prl, Epr> mpprl_eubCache;
        private Dictionary<Lsm, Cpg> mplsm_cpgCache;
        private Dictionary<Lsm, Epr> mplsm_eubCacheLhsVbl;
        private Dictionary<Lsm, Epr> mplsm_eubCacheRhsVbl;
        private Dictionary<Lsm, Epr> mplsm_eubCacheLhsNonVbl;
        private Dictionary<Lsm, Epr> mplsm_eubCacheRhsNonVbl;
        public Hua<Eqs> huaeqsCache = new Hua<Eqs>();
        public Asc[] rgascAxioms;

        public Pmb imd;
        public bool fDoReflexivePredicates = false;
        public bool fDoSymmetricPredicates = false;
        public bool fDoParamodulation = false;
        public bool fUseP1Resolution;
        public Opc opcOnDemandControl = null;
        public bool fUseNegatedGoals = true;
        public bool fBuildClauseFromHyp = false;  // not implemented
        public bool fValidateEub =
#if DEBUG
            false;
#else
            false;
#endif
        public bool fValidateEpu =
#if DEBUG
           false; // is ok: true;
#else
            false;
#endif

        // public bool fValidateProof = false;
        public bool fValidateProof = true;
        public bool fVerifyEachStep = true;
        public Irh irhTestHook;

        public double msTimeLimit = 60000;  // timeout after 1 minute

        public Pti ptiVariableEqn; // list of eqns of the form x=expr

        // ARes ares = new ARes();

        /// <summary>
        /// Perform equate of two terms as indicated by the unb
        /// </summary>
        public delegate void Ieh(Abt abtToEquate, Unb unb, Res res);
        public Ieh iehEquateHandler;
        public Ipa ipaPtiAdded;
        public Isr isrRegister;

        public Res(Pmb imd = null, Irr irr = null, bool fAlwaysTimeout = false)
        {
            this.imd = imd ?? Prm.pmbPositive();
            resLatest = this;
            asyDefault = new Asy();
            this.irr = irr;
            iehEquateHandler = Epu.EquateUnify;
            fDoParamodulation = true;
            prs = new Prs(this, fAlwaysTimeout || !Debugger.IsAttached);
        }

        public static Asy asyAnyDefault()
        {
            if (resLatest != null)
                return resLatest.asyDefault;
            else
                return new Asy();

        }

        public static void stMakeString(Isi isi, Pctl pctl, Fwt sb)
        {
            Asy asyDefault = asyAnyDefault();
            try
            {
                sb.Append(isi.lsxTo(asyDefault).stPPrint(pctl));
                if (pctl.fVerbose)
                {
                    sb.Append("#");
                    sb.Append(isi.nGetId());
                }
            }
            catch (Exception e)
            {
                sb.Append( "caught " + e);
            }
        }

        public void HandleEquality()
        {
            fDoReflexivePredicates = true;
            fDoSymmetricPredicates = true;
            fDoParamodulation = true;
        }

        public void MakeVariables(Lsx lsxList)
        {
            while (lsxList != Lsm.lsmNil)
            {
                Lpr lpr = (Lpr)lsxList;
                ((Lsm)lpr.lsxCar).MakeVariable();
                lsxList = lpr.lsxCdr;
            }
        }

        public void Init()
        {
            if (gnpAscAsc != null)
                return;

            gnpAscAsc = new GnpR(this, this);
            mpprl_eubCache = new Dictionary<Prl, Epr>();
            mplsm_eubCacheLhsVbl = new Dictionary<Lsm, Epr>();
            mplsm_eubCacheRhsVbl = new Dictionary<Lsm, Epr>();
            mplsm_eubCacheLhsNonVbl = new Dictionary<Lsm, Epr>();
            mplsm_eubCacheRhsNonVbl = new Dictionary<Lsm, Epr>();
            if (fDoParamodulation)
            {
                mplsm_cpgCache = new Dictionary<Lsm, Cpg>();
            }
        }

        public Epr eprObtain(Prl prl)
        {
            Epr epr;
            if (!mpprl_eubCache.TryGetValue(prl, out epr))
            {
                epr = new Epr(this, prl);
                mpprl_eubCache.Add(prl, epr);
                prs.Add(epr);
            }
            return epr;
        }

        /// <summary>
        /// Find/Create Eub where one side (right or left) is a vbl instead of a term with an lsm
        /// </summary>
        public Epr eprObtainForVblOrNon(Lsm lsmNonVbl, bool fVblOnRight, bool fVbl)
        {
            Epr epr;
            Dictionary<Lsm, Epr> mplsm_eub;
            if (fVbl)
                mplsm_eub = fVblOnRight ? mplsm_eubCacheRhsVbl : mplsm_eubCacheLhsVbl;
            else
                mplsm_eub = fVblOnRight ? mplsm_eubCacheRhsNonVbl : mplsm_eubCacheLhsNonVbl;
            if (!mplsm_eub.TryGetValue(lsmNonVbl, out epr))
            {
                epr = new Epr(this, null);
                mplsm_eub.Add(lsmNonVbl, epr);
                prs.Add(epr);
            }
            return epr;
        }

        public Cpg cpgObtain(Lsm lsm)
        {
            Cpg cpg;
            if (!mplsm_cpgCache.TryGetValue(lsm, out cpg))
            {
                cpg = new Cpg(this, lsm);
                mplsm_cpgCache.Add(lsm, cpg);
                prs.Add(cpg);
            }
            return cpg;
        }

        public Eqs eqsObtain(Atp atpToEquate)
        {
            Eqs eqsPrev = huaeqsCache.valGet(atpToEquate);
            if (eqsPrev == null)
            {
                eqsPrev = new Eqs(this, atpToEquate);
                if (irp != null)
                    irp.Report(Tcd.tcdNewEqs, atpToEquate, eqsPrev, null);
                huaeqsCache.Add(atpToEquate, eqsPrev);
                prs.Add(eqsPrev);
                eqsPrev.FirstStep();
            }
            // Debug.WriteLine("eqsObtain#" + eqsPrev.nId + " " + eqsPrev);
            return eqsPrev;
        }

        // lsxS: sequent
        public Asc ascProve(Lsx lsxS, int nNumNotNegated)
        {
            Init();

            AddAxioms(lsxS, nNumNotNegated);
            fUseP1Resolution = imd.fUseP1Resolution();
            if (imd.fDoResolution)
                prs.Add(this);
            imd.ReadyToRun(this);
            prs.Run(msTimeLimit);
            return ascProof;
        }

        public bool fStep()
        {
            bool fProgress = gnpAscAsc.fPerformStep();
            return !fProgress;
        }

        public int nSize()
        {
            return gnpAscAsc.nSize();
        }

        public void AddAxioms(Lsx lsxS, int nNumNotNegated)
        {
            LinkedList<Asc> rgascAxiomsList = Asc.rgascConvert(lsxS, nNumNotNegated);
            rgascAxioms = new Asc[rgascAxiomsList.Count + 1];
            int nAxiomNum = 1;
            foreach (Asc ascS in rgascAxiomsList)
            {
                rgascAxioms[nAxiomNum++] = ascS;
                if (isrRegister != null)
                    isrRegister.RegisterPreset(ascS, null);
                if (irp != null)
                    irp.Report(Tcd.tcdSaveForFilter, this, ascS, gnpAscAsc); // to add to tracking
                AddIfNotSubsumed(ascS);
            }
        }

        public void AddIfNotSubsumed(Asc ascS)
        {
            GnpR.KSide kSide = imd.kSide(ascS);
            if (gnpAscAsc.ascIsSubsumed(kSide == GnpR.KSide.kRight, ascS, true) == null)
                            // add to Ckn tree so later dups are filtered out
                AddToFiltered(kSide == GnpR.KSide.kRight, ascS);
        }

        public void SaveForFilter(Asc asc)
        {
            if (irp != null)
                irp.Report(Tcd.tcdSaveForFilter, this, asc, gnpAscAsc);
            sbyte nResTerm = imd.nResTerm(asc);
            GnpR.KSide kSide = imd.kSide(asc);
            gnpAscAsc.SaveForFilter(asc, nResTerm, kSide);
        }

        public void AddToFiltered(bool fRight, Asc ascFiltered)
        {
            gnpAscAsc.AddToFiltered(fRight, ascFiltered);
            if (fDoParamodulation)
                ascFiltered.MakePti(this);
        }

        public static long nPtiAllowEprNonVbl;  // hack until Epr non vbl is on demand
        public void SavePti(Pti ptiNew)
        {
            if (fDoParamodulation)
            {
                // ptiNew.AddToSym(this);
                Lsm lsmTo = ptiNew.lsmTopTo();
                Lsm lsmFrom = ptiNew.lsmTopFrom();

                if (lsmTo == null)
                {
                    if (lsmFrom == null)
                        return;   // handle x=x specially: where?
                    eprObtainForVblOrNon(lsmFrom, true, true).TransferRight(ptiNew);
                }
                else
                {
                    cpgObtain(lsmTo).TransferLeft(ptiNew);
                    if (lsmFrom != null)
                        if (nPtiAllowEprNonVbl == ptiNew.nId)
                            if (irhTestHook != null)
                                irhTestHook.AddDeferredPti(new Rpi(this, lsmTo, false, null, ptiNew));
                }

                if (lsmFrom == null)
                    eprObtainForVblOrNon(lsmTo, false, true).TransferRight(ptiNew);
                else
                {
                    if (lsmTo != null)
                        if (nPtiAllowEprNonVbl == ptiNew.nId)
                            if (irhTestHook != null)
                                irhTestHook.AddDeferredPti(new Rpi(this, lsmFrom, true, null, ptiNew));
                    cpgObtain(lsmFrom).TransferRight(ptiNew);
                }

                if (lsmTo != null && lsmFrom != null)
                {
                    Prl prl = new Prl(lsmFrom, lsmTo);
                    Epr epr = eprObtain(prl);
                    epr.TransferRight(ptiNew);
                }

                if (ipaPtiAdded != null)
                    ipaPtiAdded.PtiAdded(ptiNew);
            }
        }

        public void HaveProof(Asc ascProof)
        {
            this.ascProof = ascProof;
            prs.Done();
            if (irr != null)
                irr.Proved(ascProof);
            if (fValidateProof)
            {
                Pvm pvmMain = new Pvm();
                Pex pex = new Pex();
                pex.pvbCreateProofSteps(pvmMain, ascProof);
                pex.Verify(pvmMain);
            }
        }

        /// <summary>
        /// for testing
        /// </summary>
        public bool fFind(Asc ascTerm)
        {
            return gnpAscAsc.fFind(ascTerm);
        }

        /// <summary>
        /// For testing
        /// </summary>
        public int nNumClauses ()
        {
            return gnpAscAsc.nNumClauses();
        }

        public Ipr iprPriority()
        {
            return gnpAscAsc.iprPriority();
        }
    }

    public interface Irs
    {
        void Reset();
    }

    public interface Irh : Irs
    {
        void AddDeferredPti(Rpi rpi);
    }

    /// <summary>
    /// Register non-vbl Pti so it can be applied later during test
    /// </summary>
    public class Rpi
    {
        Res res;
        public readonly Lsm lsm;
        bool fVblOnRight;
        public readonly Eqs eqs;
        public readonly Pti ptiNew;

        public Rpi(Res res, Lsm lsm, bool fVblOnRight, Eqs eqs, Pti ptiNew)
        {
            this.res = res;
            this.lsm = lsm;
            this.fVblOnRight = fVblOnRight;
            this.eqs = eqs;
            this.ptiNew = ptiNew;
        }

        public Bid bid()
        {
            return (eqs != null) ? (Bid) eqs : ptiNew;
        }

        public void Perform ()
        {
            Epr epr = res.eprObtainForVblOrNon(lsm, fVblOnRight, false);
            if (eqs != null)
                epr.TransferLeft(eqs);
            else
                epr.TransferRight(ptiNew);
        }
    }

    public class BasR : Bas
    {
        public BasR(Gnb gnp, bool fRight, Bab basPrev, Ipr iprSize) : base(gnp, fRight, basPrev, iprSize)
        {
        }
    }

    /// <summary>
    /// Organize clauses for performing resolution in a prioritized manner
    /// </summary>
    public class GnpR : Gnp<Asc, Asc>
    {
        protected Ckn cknFilterIndexLeft;
        protected Ckn cknFilterIndexRight;

        public GnpR(Res res, Sprc sprcOwner) : base(res, sprcOwner)
        {
        }

        public override Bas babNew(bool fRight, Bas babPrev, Ipr iprSize)
        {
            return new BasR(this, fRight, babPrev, iprSize);
        }

        public override void AddToFiltered(bool fRight, Asc ascFiltered)
        {
            if (fRight)
                AddToOneSide(basRights, fRight, ascFiltered);
            else
                AddToOneSide(basLefts, fRight, ascFiltered);
            if (res.irr != null)
                res.irr.AscAdded(ascFiltered);
        }

        public override void ProcessPair(Asc ascLeft, Asc ascRight)
        {
            if (!Asc.fAllowedPair(ascLeft, ascRight))
                return;
            if (res.irp != null)
                res.irp.Report(Tcd.tcdStartResolve, ascLeft, ascRight, this);
            Rmub cmi = res.fUseP1Resolution ? (Rmub) new Rmup(res) : new Rmus(res);
            cmi.SetAbt(new Abt((Asc)(Rib)ascLeft, (Asc)(Rib)ascRight, res));
            if (cmi.fInit())
                return;
            res.prs.Add(cmi);
        }

        public enum KSide
        {
            kLeft,
            kRight
        }

        public void SaveForFilter(Asc asc, sbyte nResolveTerm, KSide kSide)
        {
            asc.nResolveTerm = nResolveTerm;
            switch (kSide)
            {
                // fRight was nResolveTerm != Asc.nNoResolveTerm
                case KSide.kLeft:
                    AddToUnfiltered(false, asc);
                    break;
                case KSide.kRight:
                    AddToUnfiltered(true, asc);
                    break;
            }
        }


        void AddToUnfilteredSide(Bas basList, bool fRight, Asc ascNew)
        {
            Bas basSize = basList;
            Bas basPrev;
            Bas.FindBasForSize(ascNew.iprComplexity(), ref basSize, out basPrev);
            Bas.AddToFiltered(fRight, this, ascNew, basSize, basPrev, false);
        }

        void AddToUnfiltered(bool fRight, Asc ascNew)
        {
            if (fRight)
                AddToUnfilteredSide(basRights, fRight, ascNew);
            else
                AddToUnfilteredSide(basLefts, fRight, ascNew);
        }

        public override Asc ascIsSubsumed(bool fRight, Rib ribNew, bool fAdd)
        {
            Asc ascNew = (Asc)ribNew;
            Ckn cknRoot = fRight ? cknFilterIndexRight : cknFilterIndexLeft;
            if (cknRoot == null)
            {
                cknRoot = Ckn.cknRoot(ascNew);
                if (fRight)
                    cknFilterIndexRight = cknRoot;
                else
                    cknFilterIndexLeft = cknRoot;
            }
            else
            {
                Asc ascRootSubsumes = cknRoot.ascSubsumes(ascNew, true, res);
                if (ascRootSubsumes != null)
                {

#if CHECK_SUBSUMES
                    if (ascSubsumed(fRight, ascNew, out basSize, out basPrev) == null)
                        throw new ArgumentException();
#endif

                    return ascRootSubsumes;
                }
#if CHECK_SUBSUMES
                Asc ascOld = ascSubsumed(fRight, ascNew, out basSize, out basPrev);
                if (ascOld != null)
                    throw new ArgumentException();
                // Debug.WriteLine("term should have been subsumed: " + ascNew);
#endif
               // Debug.WriteLine("was not subsumed: " + ascNew);

                if (fAdd)
                    cknRoot.AddAsc(ascNew);
            }
            return null;
        }
    }

}
