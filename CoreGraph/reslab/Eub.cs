using GraphMatching;
using System;
using System.Diagnostics;
using System.Text;

namespace reslab
{

    public class BasEqsPti : Bas
    {
        public BasEqsPti(Gnb gnp, bool fRight, Bab basPrev, Ipr iprSize) : base(gnp, fRight, basPrev, iprSize)
        {
        }
    }

    public class BasPtiEqs : Bas
    {
        public BasPtiEqs(Gnb gnp, bool fRight, Bab basPrev, Ipr iprSize) : base(gnp, fRight, basPrev, iprSize)
        {
        }
    }

    /// <summary>
    /// Feed Pti_s as candidate solutions into the Eqs_s that to equate two halves of a particular atp.
    /// </summary>
    public class Epr : Pcu2<Eqs, Pti>
    {
        GnpEpr gnp;
        bool fInactive = false;
#if DEBUG
        Prl prl;
#endif

        public Epr(Res res, Prl prl) : base(res)
        {
#if DEBUG
            this.prl = prl;
#endif
        }

        public override Ipr iprPriority()
        {
             return (gnp == null) ? Npr.nprOnly : gnp.iprPriority(); 
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append("{" + GetType().Name + "#" + nId + " " + prl + "}");
        }

#if false
        public override bool Equals(Object other)
        {
            if (!(other is Epr))
                return false;
            Epr eprObject = (Epr)other;
            if (eprObject)
        }
#endif

        public override bool fStep()
        {
            Eqs eqsLeft;
            Pti ptiRight;
            if (gnp.fGetNextPair(out eqsLeft, out ptiRight))
            {
                gnp.ProcessPair(eqsLeft, ptiRight);
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
        public override void TransferLeft(Eqs inlLeft)
        {
            if (res.irp != null)
                res.irp.Report(Tcd.tcdTransferLeftEpr, this, inlLeft, null);
            if (gnp == null)
                gnp = new GnpEpr(res, this);
            gnp.AddToOneSide(gnp.basLefts, false, inlLeft);
            Reactivate();
        }

        /// <summary>
        /// Pti used to solve the eqs that is passed in as left
        /// </summary>
        /// <param name="inrRight"></param>
        public override void TransferRight(Pti inrRight)
        {
            if (res.irp != null)
                res.irp.Report(Tcd.tcdTransferRightEpr, this, inrRight, null);
            if (gnp == null)
                gnp = new GnpEpr(res, this);
            gnp.AddToOneSide(gnp.basRights, true, inrRight);
            Reactivate();
        }

        void Reactivate()
        {
            if (fInactive)
            {
                res.prs.Add(this);
                fInactive = false;
            }
        }

        public override int nSize()
        {
            if (gnp == null)
                return 1;
            return gnp.nSize();
        }

        public override Ipp<Eqs, Pti> ippGet()
        {
            return gnp;
        }
    }

    /// <summary>
    /// Apply pti to eqs requests for solving an atp.
    /// Create two subproblems to check if left side of pti matches left side of
    /// atp, and then if the right sides match also.
    /// </summary>
    public class GnpEpr : Gnp<Eqs, Pti>
    {

        public GnpEpr(Res res, Sprc sprcOwner) : base(res, sprcOwner)
        {
        }

        public override void AddToFiltered(bool fRight, Asc ascFiltered)
        {
            throw new NotImplementedException();
        }

        public override Bas babNew(bool fRight, Bas babPrev, Ipr iprSize)
        {
            if (fRight)
                return new BasPtiEqs(this, true, babPrev, iprSize);
            else
                return new BasEqsPti(this, false, babPrev, iprSize);
        }

        public override Asc ascIsSubsumed(bool fRight, Rib ascNew, bool fAdd)
        {
            return null;
        }

        public override void ProcessPair(Eqs eqsLeft, Pti ptiRight)
        {
            /*
             *  Try to use the pti to equate the terms indicated in the atpToEquate of the eqs.
                Create two subproblems: each has one term from Eqs atp and one side of pti
								– Wire them together, then output to parent
								– Vbls: gather (vbl id, avc) with vbl id mapping - more than just a, b- has all in chain
								– Offsets - values in avc
                Start expanding rightmost pti,  then expand each vbl value
*/
            Avc avcOuter = new Avc(null);   // want to get rid of AvcForValue anyway, see if this works
            Avc avcPti = new Avc(new Aic(ptiRight.ascB));
            Eul eulLeft = new Eul(res, eqsLeft, ptiRight, avcOuter, avcPti);
            if (res.irp != null)
                res.irp.Report(Tcd.tcdApplyPtiToEqs, eqsLeft, ptiRight, eulLeft);
            eulLeft.StartSubproblem(null, null);


        }
    }

    /// <summary>
    /// Atp := L|R     Pti := A=B     check L matches A, R matches B
    /// Result is solution that combines those as vbvOutput with child vbvPti
    /// Handle notification from an Eqs obtained for GnpEub.Subproblem. 
    /// Solution matches up one side of parent Eqs.atp to one side of pti..
    /// Two Euh are needed. Each gets notifield for equate of one side of the pti.
    /// 
    ///  Eul creates subproblem with null,null
    ///  Eul splits soln into vbvOutput, vbvPti
    ///  □ Create Eur with vbvPrev as that vbvOutput
    ///  □ Create Eur and the right atpSubproblem
    ///  Eur merges the vbvPrev with the vnvInput
    /// </summary>
    public abstract class Euh : Bid, Ent, Imp
    {
        Ent entNext;
        protected Res res;
        protected Vbv vbvPrev;
        protected Moa mobToOffsetForChild;   // map positions in subproblem to the source that resulted in that part of subproblem
        protected Mva mvbMapToVblIdForChild;
        sbyte nNumVblsOnLeft;
        ushort nSplitAt;

        public Euh(Res res, Vbv vbvPrev)
        {
            this.res = res;
            this.vbvPrev = vbvPrev;
        }

        public abstract Vbv vbvGetUsedAtValue(sbyte nVblId, Vbv vbvOutput, Vbv vbvPti);

        public Vbv vbvPtiFromOutput(Vbv vbvPrev, Vbv vbvOutput)
        {
            return (vbvPrev != null) ? vbvOutput.vbvFirst : null;
        }

        public abstract Ipr iprComplexity();

        public abstract void StartSubproblem(Vbv vbvPrev, Vbv vbvPti);

        public abstract Asb asbGet();

        /// <summary>
        /// Check if the solution is usable.
        /// </summary>
        protected virtual bool fAcceptSolution(Vbv vbvInput)
        {
            return true;
        }

        protected abstract Spl splMake(Vbv vbvPrev, Vbv vbvPti);

        protected abstract void MakeSolnVbv(Vbv vbvInput, out Vbv vbvOutput, out Vbv vbvPti);

        protected abstract void CreateNextStep(Vbv vbvOutput, Vbv vbvPti);

        public abstract void ValidateSoln(Vbv vbvOutput, Vbv vbvPti);

        public void MapVblId(sbyte nVblId, out sbyte nVblIdNew, out Vbv vbvOriginal)
        {
            if (!Mva.fMapOutputToSource(mvbMapToVblIdForChild, nVblId, out nVblIdNew, out vbvOriginal))
                throw new ArgumentException();
        }
        public abstract void MapValue(ushort nValue, out ushort nValueNew, out Vbv vbvForValue, Vbv vbvOutput, Vbv vbvPti);

        const ushort nGrandchild = ushort.MaxValue;

        /// <summary>
        /// Assign the child vbv of vbvInput to the vbvOutput or the vbvPti, depending on
        /// which side of the atp the replacement occurs on.
        /// </summary>
        public void SplitAndMapChildren(Vbv vbvParent, Vbv vbvInput, Vpm vpm, Vbv vbvOutput, Vbv vbvPti,
              bool fRight, ushort nSplitAt)
        {
            for (Vbv vbvInputChild = vbvInput.vbvFirst; vbvInputChild != null; vbvInputChild = vbvInputChild.vbvNext)
            {
                bool fSkip = nSplitAt != nGrandchild 
                                && (vbvInputChild.nReplaceAtPosn < nSplitAt) == fRight;

                if (!fSkip)
                {
                    // TODO: handle grandchildren
                    Vbv vbvCopy = vbvInputChild.vbvMakeCopy(vpm);
                    SplitAndMapChildren(vbvCopy, vbvInputChild, vpm, vbvOutput, vbvPti,
                                           true, nGrandchild);
                    Vbv vbvForValue;
                    if (nSplitAt != nGrandchild)
                        MapValue(vbvInputChild.nReplaceAtPosn,
                                 out vbvCopy.nReplaceAtPosn, out vbvForValue,
                                 vbvOutput, vbvPti);
                    vbvParent.AddChild(vbvCopy);
                }
            }
        }

        protected void MeasureForSplit (Atp atpSubproblem)
        {
            nNumVblsOnLeft = (sbyte)atpSubproblem.nNumVblsOnLeft();
            nSplitAt = (ushort)(atpSubproblem.rgnTree[Atp.nOffsetLeftSize] + Atp.nOffsetFirstTerm);
        }

        /// <summary>
        /// Take the items in the contents of the vbvInput separate them into the output/pti vbv_s
        /// </summary>
        protected void SplitVbv(Vbv vbvInput, Vbv vbvOutput, Vbv vbvPti)
        {
            Vpm vpm = new Vpm(vbvInput);
            vpm.Add(vbvInput, vbvOutput);

            SplitAndMapChildren(vbvOutput, vbvInput, vpm, vbvOutput, vbvPti, false, nSplitAt);
            SplitAndMapChildren(vbvPti, vbvInput, vpm, vbvOutput, vbvPti, true, nSplitAt);

            vbvInput.MapVbaTree(vpm, this, vbvOutput, vbvPti);
            vbvInput.MapVbaList(null, true, vpm, this, true, vbvOutput, vbvPti);

            Vbv.UpdateVba(vpm, vbvOutput);
            Vbv.UpdateVba(vpm, vbvPti);   // (they are not connected until later)

            vbvPti.nReplaceAtPosn = Pmu.nNoReplace;
            vbvOutput.nReplaceAtPosn = Pmu.nNoReplace;
        }


        /// <summary>
        /// Get solution to subproblem and transfer to other side, or to the Eqs that requested the Eub/Pti
        /// </summary>
        /// <param name="esnSolution"></param>
        /// <returns></returns>
        public bool fProcessSolution(Esn esnSolution)
        {
            // convert from child ids to ids used in Atp built in StartSubproblem
            // - similar to Etp.fProcessSolution

            Vbv vbvInput = (Vbv)esnSolution;

            if (!fAcceptSolution(vbvInput))
                return false;

            Vbv vbvPti;
            Vbv vbvOutput;

            MakeSolnVbv(vbvInput, out vbvOutput, out vbvPti);
            if (vbvOutput == null)
                return false;
#if DEBUG
            if (res.irp != null)
                res.irp.Report(Tcd.tcdMapEuhSoln, this, vbvInput, vbvOutput);
#endif

            // need to pass up vbl values (map ids) to be used for side literals.
            // The values contain vbl and const id that need to be mapped.
            // That is done where they are used (where is that?)

            CreateNextStep(vbvOutput, vbvPti);
            return false;
        }


        public Lsx lsxTo(Asy asy)
        {
            throw new NotImplementedException();
        }

        public void NextSameSize(Ent riiNext)
        {
            entNext = riiNext;
        }

        public Ent riiNextSameSize()
        {
            return entNext;
        }

        public override void stString(Pctl pctl, GraphMatching.Fwt sb)
        {
            sb.Append(GetType().Name);
            sb.Append("#");
            sb.Append(nId);
        }
    }

    /// <summary>
    /// Base class shared between Eul and Eur.
    /// </summary>
    public abstract class Eub : Euh
    {
        public Eqs eqsToNotify;
        protected Pti pti;
        protected Avc avcPti;
        protected Avc avcOuter;
        protected Atp atpToEquate;
        protected ushort nLeftOffset;
        protected ushort nRightOffset;
#if DEBUG
        public Eqs eqsSub;
#endif


        public Eub(Res res, Vbv vbvPrev, Eqs eqsToNotify, Pti pti, Avc avcOuter, Avc avcPti)
            : base(res, vbvPrev)
        {
            this.eqsToNotify = eqsToNotify;
            this.atpToEquate = eqsToNotify.atpToEquate;
            this.pti = pti;
            this.avcPti = avcPti;
        }

        public override Asb asbGet()
        {
            return atpToEquate;
        }

        public override Ipr iprComplexity()
        {
            return Prp.prpBehind(eqsToNotify.iprComplexity().iprMax(pti.iprComplexity()));
        }

        /// <summary>
        /// request solution for equate terms for left or right side of atp/pti 
        /// </summary>
        public override void StartSubproblem(Vbv vbvPrev, Vbv vbvPti)
        {
//            if (vbvPrev != null && vbvPrev.nId == 8)
//                Spb.fTrace = true;

            Spl spl = splMake(vbvPrev, vbvPti);
            Atp atpSubproblem = spl.atpCreateOutput(
                            out mobToOffsetForChild, out mvbMapToVblIdForChild);
            if (atpSubproblem == null)
                return;   // loop found
            MeasureForSplit(atpSubproblem);
#if !DEBUG
            Eqs eqsSub;
#endif
            eqsSub = res.eqsObtain(atpSubproblem);

            if (eqsSub == eqsToNotify)
                return;   // pti is not helping, because the subproblem is the same problem as the eub was asked to solve.

            if (res.irp != null)
                res.irp.Report(Tcd.tcdStartSubproblem, this, vbvPrev, eqsSub);

            // notify this with solns to eqsSub
            eqsSub.TransferLeft(this);

            return;
        }

        public override void MapValue(ushort nValue, out ushort nValueNew, out Vbv vbvForValue, Vbv vbvOutput, Vbv vbvPti)
        {
            Vbv vbvValue;
            Spl.MapValue(mobToOffsetForChild, nValue, out nValueNew, out vbvValue);

            if (vbvValue == Vbv.vbvB)
                vbvForValue = vbvPti;
            else if (vbvValue == Vbv.vbvA)
                vbvForValue = vbvOutput;
            else
                vbvForValue = vbvValue;
        }

        public override void ValidateSoln(Vbv vbvOutput, Vbv vbvPti)
        {
            if (res.fValidateEub)
            {

                Moa mobToOffsetForChildValidate;
                Mva mvbMapToVblIdForChildValidate;
                Spl spl = new Spl();
                spl.Init(eqsToNotify.atpToEquate, nLeftOffset,
                    pti.ascB, nRightOffset,
                    vbvOutput, vbvPti, true);
                spl.vbvSkip = vbvPti;

                Atp atpResult = spl.atpCreateOutput(
                    out mobToOffsetForChildValidate, out mvbMapToVblIdForChildValidate);
                if (!atpResult.fSymmetric(true))
                    throw new ArgumentException();
            }
        }
        public override void stString(Pctl pctl, GraphMatching.Fwt sb)
        {
            base.stString(pctl, sb);
            sb.Append(" ");
            if (pti == null)
                sb.Append("null");
            else
                pti.stString(pctl, sb);
        }

    }

    public class Eul : Eub
    {
        public Eul(Res res, Eqs eqsToNotify, Pti pti, Avc avcOuter, Avc avcPti)
            : base(res, null, eqsToNotify, pti, avcOuter, avcPti)
        {
            nLeftOffset = Atp.nOffsetFirstTerm;
            nRightOffset = pti.nFromOffset;
        }

        protected override bool fAcceptSolution(Vbv vbvInput)
        {
            for (Vbv vbvChild = vbvInput.vbvFirst; vbvChild != null; vbvChild = vbvChild.vbvNext)
            {
#if true
                if (vbvChild.nReplaceAtPosn == Atp.nOffsetFirstTerm)
                    return false;
#else
        // this rejected a valid solution:
    	<#1127 
    	    <#1128 2/6 0:4$1128>     (nil  (=  (F  E @0) @0))
	        1:5$A, 0:4$1128>     (((F  (F  @0 @0) B)) ((F  E @1)))

                ushort nOffset = Atp.nOffsetFirstTerm;
                ushort nTermSize = atpToEquate.nTermSize(nOffset);
                if (vbvChild.nReplaceAtPosn < nOffset + nTermSize)
                    return false;
#endif
            }
            return base.fAcceptSolution(vbvInput);
        }

        protected override void MakeSolnVbv(Vbv vbvInput, out Vbv vbvOutput, out Vbv vbvPti)
        {
            vbvOutput = new Vbv(eqsToNotify.atpToEquate);
            vbvPti = new Vbv(pti.ascB);

            vbvPti.nReplaceAtPosn = Pmu.nNoReplace; // Atp.nOffsetFirstTerm;
            vbvPti.nReplaceFromPosn = pti.nFromOffset;
            vbvPti.nReplaceWithPosn = pti.nToOffset;
            vbvPti.avcOfValue = avcPti;
            vbvOutput.avcOfValue = avcOuter;   // for use with vbv$P
            vbvPti.pti = pti;

            SplitVbv(vbvInput, vbvOutput, vbvPti);

            vbvPti.nReplaceAtPosn = Atp.nOffsetFirstTerm; //  pti.nToOffset; 
        }

        protected override void CreateNextStep(Vbv vbvOutput, Vbv vbvPti)
        {
/* catch loops during regular expansion
            if (fFindOccurs(eqsToNotify.atpToEquate, pti.ascB, vbvOutput, vbvPti))
                return;
                */
            ValidateSoln(vbvOutput, vbvPti);

            Eur eurRight = new Eur(res, vbvOutput, eqsToNotify, pti, avcOuter, avcPti);

            // Attach the vbvPti to vbvOutput. This comes after  eurRight.StartSubproblem so the pti is not applied.
            // The attachment needs to be done before processing begins on the new eurRight.
            vbvOutput.AddChild(vbvPti);

            if (vbvOutput.fCheckConflictingReplaceAtPosn(vbvOutput, vbvPti))
                return;
#if DEBUG
            if (res.irp != null)
                res.irp.Report(Tcd.tcdLaunchEur, this, vbvOutput, eurRight);
#endif
            eurRight.StartSubproblem(vbvOutput, vbvPti);
        }

        protected override Spl splMake(Vbv vbvPrev, Vbv vbvPti)
        {
            Spl spl = new Spl();
            // match lhs of eqsToNotify to lhs of Pti
            spl.Init(atpToEquate, nLeftOffset,
                        pti.ascB, nRightOffset,
                        null, null, true);
            return spl;
        }

        public override Vbv vbvGetUsedAtValue(sbyte nVblId, Vbv vbvOutput, Vbv vbvPti)
        {
            // if pti 'from' term is a single vbl that matches the given nVblId,
            // the result is the vbvPti
            sbyte nFirstInTerm = pti.ascB.rgnTree[pti.nFromOffset];
            if (nFirstInTerm == nVblId)
                return vbvPti;
            return null;
        }

    }

    public class Eur : Eub
    {
        public Eur(Res res, Vbv vbvPrev, Eqs eqsToNotify, Pti pti, Avc avcOuter, Avc avcPti)
            : base(res, vbvPrev, eqsToNotify, pti, avcOuter, avcPti)
        {
            nLeftOffset = (ushort)(Atp.nOffsetFirstTerm + atpToEquate.rgnTree[Atp.nOffsetLeftSize]);
            nRightOffset = pti.nToOffset;
        }

        protected override void MakeSolnVbv(Vbv vbvInput, out Vbv vbvOutput, out Vbv vbvPti)
        {
            Vbv.MergeVbv(out vbvOutput, out vbvPti, vbvInput, vbvPrev, this);
        }

        protected override Spl splMake(Vbv vbvOutput, Vbv vbvPti)
        {
            // match rhs of Pti to rhs of eqsToNotify, with sbst from values matched on lhs
            Spl spl = new Spl();
            spl.Init(atpToEquate, nLeftOffset,
                     pti.ascB, nRightOffset,
                     vbvOutput, vbvPti, true);
            spl.vbvSkip = vbvPti;
            return spl;
        }

        protected override void CreateNextStep(Vbv vbvOutput, Vbv vbvPti)
        {
            //if (fFindOccurs(eqsToNotify.atpToEquate, pti.ascB, vbvOutput, vbvPti))
            //    return;
            ValidateSoln(vbvOutput, vbvPti);

            vbvPti.nReplaceAtPosn = Atp.nOffsetFirstTerm;
            vbvPti.nReplaceFromPosn = pti.nFromOffset;
            vbvPti.nReplaceWithPosn = nRightOffset;

            if (res.irp != null)
            {
                res.irp.Report(Tcd.tcdEurToNotifyEqs, eqsToNotify, this, vbvOutput);
            }

            eqsToNotify.TransferRight(vbvOutput);

        }


        public override Vbv vbvGetUsedAtValue(sbyte nVblId, Vbv vbvOutput, Vbv vbvPti)
        {
            return null;
            // is something needed ?
            // throw new NotImplementedException();
        }

        public override void ValidateSoln(Vbv vbvOutput, Vbv vbvPti)
        {
            /* Argument for why the base should be valid:
             *  - merging the vbv backs the child soln back to the vblIds and offsets of the caller
             *  - this should be part of the Spv that is done below.
             */
            base.ValidateSoln(vbvOutput, vbvPti);

            if (res.fValidateEub)
            {
                if (vbvOutput.nId == -5755)
                    Spb.fTrace = true;

                Moa mobToOffsetForChildValidate;
                Mva mvbMapToVblIdForChildValidate;
                Spv spv = new Spv(pti);
#if true
                // Spv is not the same as spl, so args are different
                spv.Init(eqsToNotify.atpToEquate, Atp.nOffsetFirstTerm,
                    pti.ascB, pti.nFromOffset,
                    vbvOutput, vbvPti, true);
#else
                spv.Init(eqsToNotify.atpToEquate, eqsToNotify.atpToEquate.nRightSidePosn(),
                    pti.ascB, pti.nToOffset,
                    vbvOutput, vbvPti, true);
#endif
                spv.vbvSkip = vbvPti;   // turn off the replacement

                Atp atpResult = spv.atpCreateOutput(
                    out mobToOffsetForChildValidate, out mvbMapToVblIdForChildValidate);
                if (!atpResult.fSymmetric(true))
                    throw new ArgumentException();
            }
        }

    }
}
