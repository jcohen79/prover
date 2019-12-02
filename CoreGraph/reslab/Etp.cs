using GraphMatching;
using System;
using System.Diagnostics;
using System.Text;

namespace reslab
{
    /// <summary>
    /// Create an Eqs to handle first pair in atp. Then for each solution, advance to the pair in that atp,
    /// substituting in the solution to the first.
    /// </summary>
    public class Etp : Bid, Ent, Imp
    {
        public Eqs eqsToSolve;
        Res res;
        public Eqs eqsToNotify;
        Ent entNextSameSize;
        public ushort nOffsetLeft;
        public ushort nOffsetRight;
        public Moa mobToOffsetForChild;
        public Mva mvbMapToVblIdForChild;
        public readonly Atp atpOriginal;
        public Vbv vbvPrev;
        public Atp atpChild;   // needs to be here for debugging only

        /// <summary>
        /// Each Etp sets up Eqs to handle first child of given Atp, 
        /// then in fProcessSolution starts on subsequent children of term
        /// </summary>
        private Etp(Res res, Eqs eqsToSolve, ushort nOffsetLeft, ushort nOffsetRight, 
                   Vbv vbvPrev, Atp atpOriginal)
        {
            this.res = res;
            this.eqsToSolve = eqsToSolve;
            this.vbvPrev = vbvPrev;
            this.nOffsetLeft = nOffsetLeft;
            this.nOffsetRight = nOffsetRight;
            this.atpOriginal = atpOriginal;
        }

        /// <summary>
        /// Begin processing the child terms of the Eqs.
        /// Return true if an immediate solution was found for all children.
        /// </summary>
        public static Vbv vbvStartFirstChild(Res res, Eqs eqsToSolve, ushort nOffsetLeft, 
                                           ushort nOffsetRight, Etp etpPrev, Vbv vbvPrev)
        {
            Atp atpTermsToEquate = eqsToSolve.atpToEquate;
            Atp atpOriginal = etpPrev == null ? atpTermsToEquate : etpPrev.atpOriginal;
            Etp etpNew = new Etp(res, eqsToSolve, nOffsetLeft, nOffsetRight, vbvPrev, atpOriginal);

            // Tell the first etp in the series the eqs to notify. The actual notification
            // will be only done on the last step.
            etpNew.eqsToNotify = etpPrev == null ? eqsToSolve : etpPrev.eqsToNotify;
#if DEBUG
            if (etpPrev == null)
                etpNew.eqsToNotify.etpForChildren = etpNew;
#endif

            Spl spl = new Spl();
            spl.Init(atpTermsToEquate, nOffsetLeft,
                            atpTermsToEquate, nOffsetRight,
                            vbvPrev,
                            vbvPrev, // (vbvPrev != null) ? vbvPrev.vbvBSide() : null,
                            false);
            etpNew.atpChild = spl.atpCreateOutput(out etpNew.mobToOffsetForChild,
                                                  out etpNew.mvbMapToVblIdForChild);
            if (res.irp != null)
            {
                // solution specific
                res.irp.Report(Tcd.tcdRegisterEtpByVbv, etpPrev, vbvPrev, etpNew);
            }

            Vbv vbvImmediate = null;
            Eqs eqsChild = res.eqsObtain(etpNew.atpChild);
            if (res.irp != null)
            {
                // this does not depend on which solution is found
                res.irp.Report(Tcd.tcdRegisterEtpByOffset, eqsToSolve, Tcd.nCached(nOffsetLeft), etpNew);

            }

            // Extra call to report minimal solution immediately. (Is redundant with later regular call).
            // Is this really needed? Purpose is to avoid creating goals when immediate soln works.
            if (eqsChild.vbvMinimalSoln != null)
                vbvImmediate = etpNew.vbvCombineSolution(eqsChild.vbvMinimalSoln, true);

            // register anyway, for cases where minimal soln fails
            eqsChild.TransferLeft(etpNew);
            // ProcessSolution() will be called for each solution that eqsChild finds
            return vbvImmediate;
        }

        public Vbv vbvGetUsedAtValue(sbyte nVblId, Vbv vbvOutput, Vbv vbvPti)
        {
            return null;
            // is something needed ?
            // throw new NotImplementedException();
        }

        public Asb asbGet()
        {
            return atpOriginal;
        }


        public Vbv vbvPtiFromOutput(Vbv vbvPrev, Vbv vbvOutput)
        {
#if false
            Vbv vbvB = vbvOutput.vbvBSide();
            if (vbvB != null)
                return vbvB;
#endif
            return vbvOutput;
        }

        /// <summary>
        /// Process solution coming from Eqs created in ctor. 
        /// Each solution equates the two halves of the corresponding child eqs.atpToEquate
        /// Invoke next step or notify originator of parent terms.
        /// </summary>
        public bool fProcessSolution(Esn esnSolution)
        {
            vbvCombineSolution(esnSolution, false);
            return false;  // only remove from processing when all caught up with solutions
        }

        /// <summary>
        /// Combine soln from previous step with current soln so far, and proceed to next step
        /// Return soln if end reached with an immediate solution.
        /// </summary>
        public Vbv vbvCombineSolution(Esn esnSolution, bool fDirect)
        {
            // convert from child ids to ids used in Atp passed to parent Eqs
            Vbv vbvOutput;
            Vbv vbvPti;
            Vbv.MergeVbv(out vbvOutput, out vbvPti, (Vbv)esnSolution, vbvPrev, this);
            if (res.irp != null)
                res.irp.Report(Tcd.tcdVbvForEtp, this, esnSolution, vbvOutput);
            if (vbvOutput == null)
                return null;

            Atp atpTermsToEquate = eqsToSolve.atpToEquate;
            int nRightTermSize = atpTermsToEquate.nTermSize(nOffsetRight);
            ushort nOffsetRightNext = (ushort) (nOffsetRight + nRightTermSize);
            if (nOffsetRightNext < atpTermsToEquate.rgnTree.Length)
            {
                int nLeftTermSize = atpTermsToEquate.nTermSize(nOffsetLeft);
                ushort nOffsetLeftNext = (ushort) (nOffsetLeft + nLeftTermSize);
                return Etp.vbvStartFirstChild(res, eqsToSolve, nOffsetLeftNext, nOffsetRightNext, this, vbvOutput);
            }
            else
            {
                // end of sequence:
                eqsToNotify.TransferRight(vbvOutput);
                return fDirect ? vbvOutput : null;
            }
        }

        public void MapVblId(sbyte nVblId, out sbyte nVblIdNew, out Vbv vbvOriginal)
        {
            if (!Mva.fMapOutputToSource(mvbMapToVblIdForChild, nVblId, out nVblIdNew, out vbvOriginal))
                throw new ArgumentException();
        }

        public void MapValue(ushort nValue, out ushort nValueNew, out Vbv vbvForValue, Vbv vbvOutput, Vbv vbvPti)
        {
            Spl.MapValue(mobToOffsetForChild, nValue, out nValueNew, out vbvForValue);
        }

        public void NextSameSize(Ent riiNext)
        {
            entNextSameSize = (Ent) riiNext;
        }

        public Ent riiNextSameSize()
        {
            return entNextSameSize;
        }

        public Ipr iprComplexity()
        {
            Atp atpTermsToEquate = eqsToSolve.atpToEquate;
            return Prn.prnObtain(atpTermsToEquate.rgnTree.Length);
        }

        public Lsx lsxTo(Asy asy)
        {
            Atp atpTermsToEquate = eqsToSolve.atpToEquate;
            return atpTermsToEquate.lsxTo(asy);
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append(GetType().Name);
            sb.Append("#");
            sb.Append(nId);
            sb.Append(" ");

            sb.Append(nOffsetLeft);
            sb.Append(",");
            sb.Append(nOffsetRight);
            sb.Append(" ");
            if (eqsToSolve == null)
                sb.Append("null");
            else
            {
                Atp atpTermsToEquate = eqsToSolve.atpToEquate;
                atpTermsToEquate.stString(pctl, sb);
            }
        }
    }
    /// <summary>
    /// Handle the notifications that result from equating the pairs in a unification that is initiated by
    /// Esu. Then start the next step, which will notify a new Epu instance. At end, 
    /// </summary>
    public class Epu : Eqb, Ent, Imp
    {
        public Atp atpToEquate;
        Ent entNextSameSize;
        public Moa mobToOffsetForChild;
        public Mva mvbMapToVblIdForChild;
        public Epu epuPrev;
        public Vbv vbvPrev;
        protected ushort nOffsetA;
        protected ushort nOffsetB;
#if DEBUG
        Eqs eqsStart;
#endif

        public static void EquateUnify(Abt abtToEquate, Unb unb, Res res) // pg 271
        {
            if (!unb.fMore())  // first one is here, the rest are in fProcessSolution
                return;
            Epu epu = new Epu(res, abtToEquate, unb, null, null, null);
            if (res.irp != null)
                res.irp.Report(Tcd.tcdMakeEpu, abtToEquate.avcA.aic.asc, abtToEquate.avcB.aic.asc, epu);
            epu.Start();
        }

        /// <summary>
        /// Each Epu sets up Eqs to handle first child of given Atp, 
        /// then in fProcessSolution starts on subsequent pairs to unify
        /// </summary>
        public Epu(Res res, Abt abtToEquate, Unb unb, Epu epuPrev, Vbv vbvPrev, Vbv vbvPrevPti)
            : base(abtToEquate, unb, unb.urc, res, unb.nOffsetB)
        {
            this.res = res;
            this.epuPrev = epuPrev;
            this.vbvPrev = vbvPrev;
            nOffsetA = unb.nOffsetA;
            nOffsetB = unb.nOffsetB;

            Spl spl = new Spl();
            spl.Init(abtToEquate.avcA.aic.asc, nOffsetA,
                abtToEquate.avcB.aic.asc, nOffsetB, vbvPrev, vbvPrevPti, true);

            atpToEquate = spl.atpCreateOutput(out mobToOffsetForChild, out mvbMapToVblIdForChild);
        }

        public void Start()
        { 
#if DEBUG
#else
            Eqs eqsStart;
#endif
            if (atpToEquate != null)
            {
                atpToEquate.fLiteralLevel = true;   // first order logic does not allow sbst of predicate
                eqsStart = res.eqsObtain(atpToEquate);
                if (res.irp != null)
                    res.irp.Report(Tcd.tcdEqsForEpu, this, vbvPrev, atpToEquate);
                eqsStart.TransferLeft(this);  // ProcessSolution() will be called for each solution that eqsChild finds
            }
        }

        public Vbv vbvGetUsedAtValue(sbyte nVblId, Vbv vbvOutput, Vbv vbvPti)
        {
            return null;
            // is something needed ?
            // throw new NotImplementedException();
        }

        public Asb asbGet()
        {
            return atpToEquate;
        }

        public Vbv vbvPtiFromOutput(Vbv vbvPrev, Vbv vbvOutput)
        {
            Vbv vbvPti = vbvOutput.vbvBSide();
            if (vbvPti == null)
            {
                vbvPti = new Vbv(abtToEquate.avcB.aic.asc);
                vbvPti.nReplaceAtPosn = Pmu.nNoReplace;
                vbvOutput.vbvFirst = vbvPti;
            }
            return vbvPti;
        }

        protected virtual void ReportResult(Esn esnSolution)
        {
            // no more terms to equate for this unification
            Vbv.StartGlobalHack(atpToEquate);

            // SaveAsc(esnSolution, mvbMapToVblIdForChild, mobToOffsetForChild);
            SaveAsc(esnSolution, null, null);

            Vbv.EndGlobalHack();

        }

        protected virtual void MakeNextEpu(Unb unbLocal, Vbv vbvOutput, Vbv vbvPti)
        {
            Epu epuNext = new Epu(res, abtToEquate, unbLocal, this, vbvOutput, vbvPti);
        }

        /// <summary>
        /// Process solution coming from Eqs created in ctor. 
        /// Each solution equates the two halves of the corresponding child eqs.atpToEquate
        /// Invoke next step or notify originator of parent terms.
        /// </summary>
        public bool fProcessSolution(Esn esnSolution)
        {
            Vbv vbvOutput;
            Vbv vbvPti;
            Vbv.MergeVbv(out vbvOutput, out vbvPti, (Vbv)esnSolution, vbvPrev, this);
            if (vbvOutput == null)
                return false;
            vbvOutput.asb = abtToEquate.avcA.aic.asc;
            if (res.irp != null)
                res.irp.Report(Tcd.tcdVbvForEpu, this, esnSolution, vbvOutput);

            if (res.fValidateEpu)
            {
                ValidateSoln(vbvOutput, vbvPti);
            }

            Unb unbLocal = unb.unbCopy();  // each solution goes in parallel (could also save a sequence to keep traversing)
            if (unbLocal.fMore())
            {

                // set up to process next pair to equate
                MakeNextEpu(unbLocal, vbvOutput, vbvPti);
            }
            else
            {
                ReportResult(vbvOutput);
            }
            return false;
        }

        void ValidateInputSoln (Esn esnSolution, Vbv vbvSource, ushort nTermToShowOffset)
        {
            Sps sps = new Sps();
            Ascb ascbR = new Ascb();
            ascbR.DummySizes();
            Mva mvbMapForChild = null;
            sps.Init(ascbR, mvbMapForChild);
            Vbv vbvLeft = (Vbv)esnSolution;
            // sps.mobToOffsetForChild = mobToOffsetForChild;
            Asb ascLeft = eqsStart.atpToEquate;
            Asb ascRight = ascLeft;
            Vbv vbvRight = vbvLeft;
            sps.SetupSoln(ascLeft, ascRight, vbvLeft, vbvRight);
            sps.SetupSbst(nTermToShowOffset, vbvSource);
            sps.ProcessParts();
            sps.GetResult();

            Asc ascR = ascbR.ascBuild(0, 1);
            Debug.WriteLine(nTermToShowOffset + " " + ascR.ToString());

        }

        public void ValidateSoln(Vbv vbvOutput, Vbv vbvPti)
        {

            Moa mobToOffsetForChildValidate;
            Mva mvbMapToVblIdForChildValidate;
            Spl spl = new Spl();
            spl.Init(abtToEquate.avcA.aic.asc, nOffsetA,
                abtToEquate.avcB.aic.asc, nOffsetB,
                vbvOutput, vbvPti, true);

            Atp atpResult = spl.atpCreateOutput(
                out mobToOffsetForChildValidate, out mvbMapToVblIdForChildValidate);
            if (!atpResult.fSymmetric(true))
                throw new ArgumentException();
        }

        public void MapVblId(sbyte nVblId, out sbyte nVblIdNew, out Vbv vbvOriginal)
        {
            if (!Mva.fMapOutputToSource(mvbMapToVblIdForChild, nVblId, out nVblIdNew, out vbvOriginal))
                throw new ArgumentException();
        }

        public void MapValue(ushort nValue, out ushort nValueNew, out Vbv vbvForValue, Vbv vbvOutput, Vbv vbvPti)
        {
            Spl.MapValue(mobToOffsetForChild, nValue, out nValueNew, out vbvForValue);
        }

        public void NextSameSize(Ent riiNext)
        {
            entNextSameSize = (Ent)riiNext;
        }

        public Ent riiNextSameSize()
        {
            return entNextSameSize;
        }

        public Ipr iprComplexity()
        {
            return Prn.prnObtain(atpToEquate.rgnTree.Length);
        }

        public Lsx lsxTo(Asy asy)
        {
            return atpToEquate.lsxTo(asy);
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append(GetType().Name);
            sb.Append(Pctl.stIdTypePrefix);
            if (!pctl.fIdentifier)
            {
                sb.Append(nId);
                sb.Append(" ");
            }

            sb.Append(nOffsetA);
            sb.Append(Pctl.stIdCircle);
            sb.Append(nOffsetB);
            sb.Append(Pctl.stIdCircle);
            atpToEquate.stString(pctl, sb);
            sb.Append(Pctl.stIdTypeSuffix);
        }
    }
}
