using GraphMatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace reslab
{
    /// <summary>
    /// Gets solutions. This can be the top level requestor or a lower level.
    /// Even top level can in principle be added to list, but it has the best priority.
    /// </summary>
    public interface Ent : Rib
    {
        /// <summary>
        /// Notification of a new solutin.
        /// return true to terminate
        /// </summary>
        bool fProcessSolution(Esn esnSolution);   // TODO: add vbl/values, etc
    }

    /// <summary>
    /// A saved partial solution.
    /// </summary>
    public interface Esn : Rib
    {
    }

    /// <summary>
    /// Holds lists of saved requests to be notified when an Eqs finds a solution
    /// </summary>
    public class BasEntEsn : Bas
    {
        public BasEntEsn(Gnb gnp, bool fRight, Bab basPrev, Ipr iprSize) : base(gnp, fRight, basPrev, iprSize)
        {
        }
    }

    public class BasEsnEnt : Bas
    {
        public BasEsnEnt(Gnb gnp, bool fRight, Bab basPrev, Ipr iprSize) : base(gnp, fRight, basPrev, iprSize)
        {
        }
    }

    /// <summary>
    /// Equate a single term or literal. 
    /// </summary>
    public class Eqs : Pcu2<Ent, Esn>, Rib, Icmp
    {
        public Gnp<Ent, Esn> gnp;
        public Atp atpToEquate;
        bool fInactive = false;
        public Vbv vbvMinimalSoln;
        private HashSet<Vbv> rgvbvSolutions;
#if DEBUG
        public Etp etpForChildren;

        public static long nEqsAllowEprNonVbl;  // hack until on-demand non-vbl epr
#endif

        /// <summary>
        /// Requester should call TransferLeft to be notified of solution, and when each solution is found,
        /// requestor will be called at fProcessSolution.
        /// Solutions to this come in through a call to TransferRight
        /// </summary>
        public Eqs(Res res, Atp atpToEquate) : base(res)
        {
            this.atpToEquate = atpToEquate;
        }

        // this can be dynamic
        public override Ipr iprPriority()
        {
            return (gnp == null) ? Npr.nprOnly : gnp.iprPriority();
        }

        // this is constant because it is used to order contengs of gnp
        public Ipr iprComplexity()
        {
            return atpToEquate.iprComplexity();
        }

        public override int nSize()
        {
            if (gnp == null)
                return 1;
            return gnp.nSize();
        }
        public void FirstStep()
        {
            ushort nLeftTermSize = (ushort)atpToEquate.rgnTree[Atp.nOffsetLeftSize];
            sbyte nLeftId = atpToEquate.rgnTree[Atp.nOffsetFirstTerm];
            ushort nRightPosn = (ushort)(Atp.nOffsetFirstTerm + nLeftTermSize);
            sbyte nRightId = atpToEquate.rgnTree[nRightPosn];
            if (nLeftId >= Asb.nVar)
            {
                if (nLeftId == nRightId)
                {
                    vbvMinimalSoln = new Vbv(atpToEquate);
                    TransferRight(vbvMinimalSoln);
                    return;
                }
                if (!atpToEquate.fOccur(nLeftId, nRightPosn))
                {
                    // add rhs term as value for lhs
                    vbvMinimalSoln = new Vbv(atpToEquate);
                    vbvMinimalSoln.vbaAdd(nLeftId, nRightPosn, Vbv.vbvB);
                    TransferRight(vbvMinimalSoln);
                }
                // tests fail if als else is used. Why is that?
                if (nRightId < Asb.nVar)
                {
                    Lsm lsmRight = atpToEquate.rglsmData[Asb.nLsmId - nRightId];
                    Epr eprRightVbl = res.eprObtainForVblOrNon(lsmRight, false, true);
                    eprRightVbl.TransferLeft(this);
                    // Left is a vbl, and right is not. So a pti with anything on left matches.
                    if (this.nId == nEqsAllowEprNonVbl)  // hack for development, until is on-demand
                    {
                        if (res.irhTestHook != null)
                            res.irhTestHook.AddDeferredPti(new Rpi(res, lsmRight, false, this, null));
                    }
                }
            }
            else if (nRightId >= Asb.nVar)
            {
                if (!atpToEquate.fOccur(nRightId, Atp.nOffsetFirstTerm))
                {
                    // add lhs term as value for rhs
                    vbvMinimalSoln = new Vbv(atpToEquate);
                    vbvMinimalSoln.vbaAdd(nRightId, Atp.nOffsetFirstTerm, Vbv.vbvA);
                    TransferRight(vbvMinimalSoln);
                }
                Lsm lsmLeft = atpToEquate.rglsmData[Asb.nLsmId - nLeftId];
                Epr eprLeftVbl = res.eprObtainForVblOrNon(lsmLeft, true, true);
                eprLeftVbl.TransferLeft(this);
                if (this.nId == nEqsAllowEprNonVbl)  // hack for development, until is on-demand
                {
                    if (res.irhTestHook != null)
                        res.irhTestHook.AddDeferredPti(new Rpi(res, lsmLeft, true, this, null));
                }
            }
            else
            {
                Lsm lsmLeft = atpToEquate.rglsmData[Asb.nLsmId - nLeftId];
                if (nLeftId == nRightId && lsmLeft.nArity == Lsm.nArityConst)
                {
                    vbvMinimalSoln = new Vbv(atpToEquate);
                    TransferRight(vbvMinimalSoln);
                    return;
                }
                if (res.fDoParamodulation
                    && !atpToEquate.fLiteralLevel)   // do not apply fns to predicates
                {
                    Lsm lsmRight = atpToEquate.rglsmData[Asb.nLsmId - nRightId];
                    // TODO- check for compatible type
                    {


                        Prl prl = new Prl(lsmLeft, lsmRight);
                        Epr epr = res.eprObtain(prl);
                        epr.TransferLeft(this);

                        Epr eprLeft = res.eprObtainForVblOrNon(lsmLeft, true, true);
                        eprLeft.TransferLeft(this);

                        Epr eprRight = res.eprObtainForVblOrNon(lsmRight, false, true);
                        eprRight.TransferLeft(this);
                    }
                }

                if (gnp == null)
                    gnp = new GnpE(null, this);


                // create Etp for first term. The Etp will process subsequent terms.
                // Etp creates child Atp for each child term to match. When solution is
                // found to last child, the combined solution is passed to the parent eqs.
                if (nLeftId == nRightId)
                { 
                    vbvMinimalSoln =
                          Etp.vbvStartFirstChild(res, this, Atp.nOffsetFirstTerm + 1,
                                        (ushort)(nRightPosn + 1), null, null);
                    if (vbvMinimalSoln != null && res.irp != null)
                        res.irp.Report(Tcd.tcdImmediateVbv, this, this, vbvMinimalSoln);
                }
                    // The goals created by Epr do not drive a search, so it is not complete.
#if false
                    if (!atpToEquate.fLiteralLevel)     // literal level is complete, nothing extra needed
                    {
                        if (res.fBuildClauseFromHyp)
                        {
                            // not implemented: it would create too many clauses based on likely junk
                            // Add this eqs to a Vhy and attach that to a Vbv as part of solution.
                            // Etp will see that and form an implication from hypotheses to conclusion.
                            Vhy vhyNew = new Vhy(this, null);
                            Vbv vbvHyp = new Vbv(atpToEquate);
                            vbvHyp.vhyFirst = vhyNew;
                            TransferRight(vbvHyp);
                        }
                        else
                    }
#endif
            //        MakeNgc();
            }
            Asc ascNgc = ascMakeNgc();
            if (ascNgc != null && res.isrRegister != null)
                res.isrRegister.Register(KCapture.kResult, ascNgc, null); // todo: use tcdEqsToNgc 
        }

        Asc ascMakeNgc ()
        {
            if (!res.fUseNegatedGoals)
                return null;
            if (atpToEquate.fLiteralLevel)     // literal level is complete, nothing extra needed
                return null;

            // Create a hypothesis so that resolution, which is complete,
            // will attempt to refute it.
            Ngc ngc = new Ngc(this);
            Asc ascS = ngc.ascNegatedHypothesis();
            // res.AddIfNotSubsumed(ascNegHypothesis);
            ascS.nResolveTerm = res.imd.nResTerm(ascS);
            GnpR.KSide kSide = res.imd.kSide(ascS);
            bool fRight = kSide == GnpR.KSide.kRight;

            // do not check for subsumes, it may be that the more specific is more easily refuted
            if (res.irp != null)
                res.irp.Report(Tcd.tcdEqsToNgc, this, ngc, ascS);
            // add to Ckn tree so later dups are filtered out
            res.AddToFiltered(fRight, ascS);

            return ascS;
#if false
            // TODO check/add to hash table for that asc
            Asc ascSubsumes = res.gnpAscAsc.ascIsSubsumed(fRight, ascS, true);
            if (ascSubsumes == null)
            {
            }
            else
            {
                Gfh gfhPrior = ascSubsumes.gfbSource.gfhIsSecondaryHyp();
                if (gfhPrior != null)
                {
                    Gfl gflNew = new Gfl(ngc, gfhPrior);
                    // is simplification possible?
                    ascSubsumes.gfbSource = gflNew;
                }
            }
            // if that Asc is subsumed by an asc with no gfh,
            // then this Eqs will never succeed.
            // TODO: discard this eqs
#endif
        }

        public override bool fStep()
        {
            // Get a pair of a solution from below and a requestor, and provide solution to requestor
            Ent entToNotify;
            Esn esnSolution;
            if (gnp != null && gnp.fGetNextPair(out entToNotify, out esnSolution))
            {
                if (res.irr != null && res.irr.fLogging())
                {
                    res.irr.EqsLog("\neqs ProcessPair");
                    res.irr.EqsLog("   " + this);
                    res.irr.EqsLog("   " + entToNotify);
                    res.irr.EqsLog("   " + esnSolution);
                }
                if (res.irp != null)
                {
                    res.irp.Report(Tcd.tcdSolnToEqs, entToNotify, esnSolution, this);
                    res.irp.Report(Tcd.tcdEqsToEnt, entToNotify, this, esnSolution);
                }
                gnp.ProcessPair(entToNotify, esnSolution);
                return false;
            }
            else
            {
                fInactive = true;
                return true;  // all results distributed
            }
        }

        /// <summary>
        /// Store requests for solutions matching the two terms
        /// </summary>
        /// <param name="inlLeft"></param>
        public override void TransferLeft(Ent inlLeft)
        {
            if (gnp == null)
                gnp = new GnpE(null, this);
            gnp.AddToOneSide(gnp.basLefts, false, inlLeft);
            Reactivate();
            if (res.irp != null)
                res.irp.Report(Tcd.tcdTransferLeftEqs, this, inlLeft, null);
        }

        void Reactivate()
        {
            if (fInactive)
            {
                res.prs.Add(this);
                fInactive = false;
            }
        }

        public HashSet<Vbv> rgvbvGetSolutions()
        {
            if (rgvbvSolutions == null)
                rgvbvSolutions = new HashSet<Vbv>();
            return rgvbvSolutions;
        }

        /// <summary>
        /// Store solution
        /// </summary>
        public override void TransferRight(Esn inlRight)
        {
            if (gnp == null)
                gnp = new GnpE(null, this);
            if (res.irp != null)
                res.irp.Report(Tcd.tcdTransferRightEqs, this, inlRight, gnp);
            Vbv vbvRight = (Vbv)inlRight;
            bool fAdd;
            if (vbvRight == vbvMinimalSoln)
                fAdd = true;
            else
            {
                if (!rgvbvGetSolutions().Contains(vbvRight))
                {
                    fAdd = true;
                    rgvbvSolutions.Add(vbvRight);
                }
                else
                    fAdd = false;
            }
            if (fAdd)
            {
                gnp.AddToOneSide(gnp.basRights, true, inlRight);
                Reactivate();
            }
        }

        public Lsx lsxTo(Asy asy)
        {
            throw new NotImplementedException();
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append("eqs" + Pctl.stIdTypePrefix);
            if (!pctl.fIdentifier)
            {
                sb.Append(nId);
                sb.Append(" ");
            }
            if (atpToEquate == null)
                sb.Append("null");
            else
                atpToEquate.stString(pctl, sb);
            sb.Append(Pctl.stIdTypeSuffix);
        }

        public override Ipp<Ent, Esn> ippGet()
        {
            return gnp;
        }
    }

    public class GnpE : Gnp<Ent, Esn>
    {

        public GnpE(Res res, Sprc sprcOwner) : base(res, sprcOwner)
        {
        }

        public override void AddToFiltered(bool fRight, Asc ascFiltered)
        {
            throw new NotImplementedException();
        }

        public override Bas babNew(bool fRight, Bas babPrev, Ipr iprSize)
        {
            if (fRight)
                return new BasEsnEnt(this, true, babPrev, iprSize);
            else
                return new BasEntEsn(this, false, babPrev, iprSize);
        }

        public override Asc ascIsSubsumed(bool fRight, Rib ascNew, bool fAdd)
        {
            return null;
        }

        public override void ProcessPair(Ent entLeft, Esn esnRight)
        {
            entLeft.fProcessSolution(esnRight);
        }
    }


}