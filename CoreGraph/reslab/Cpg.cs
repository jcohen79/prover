
using GraphMatching;
using System;
using System.Diagnostics;

namespace reslab
{
    public class Prl
    {
        public readonly Lsm lsmLeft;
        public readonly Lsm lsmRight;

        public Prl(Lsm lsmLeft, Lsm lsmRight)
        {
            this.lsmLeft = lsmLeft;
            this.lsmRight = lsmRight;
        }

        public override string ToString()
        {
            return "{" + GetType().Name + " " + lsmLeft + " " + lsmRight + "}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Prl))
                return false;
            Prl prl = (Prl)obj;
            return lsmLeft == prl.lsmLeft && lsmRight == prl.lsmRight;
        }

        public override int GetHashCode()
        {
            return lsmLeft.GetHashCode() * 23 + lsmRight.GetHashCode();
        }
    }

    /*    connect two pti_s
          +-----+
          | Cpg |--------+
          +-----+        v
            ^  ^        atp to solve to connect
         lsm|  |lsm      |
            |  |         |                          handle
            |  |         |       +------+          solution to connect pti-ctp for new ctp
      pti---+  |         +------>|  Eqs |----------> cmr -----+
               |                 +------+                     |
               |                                              V
               +-------------------------------------------  pti     apply pti two solve an Eqs
                                                              |           +------+       connect sides using pti
                                                              +---------> |  Eub |---> Euh(left) ---> Euh(right)
                                                                   prl    +------+                        |
                                                                                                          |
                                                                                 +-------+                |
                                                                                 |  Eqs  | <--------------+
                                                                                 +-------+
                                                                                equate terms or subterms

        		□ Cpg takes each (pti/pti) pair and creates a new cmr
					® Obtain Eqs via atp to connect the pti/pti pair
					® Cpg registers so Eqs will send solution to cmr
				□ Cmr.fProcessSolution take solution from and creates ctp, 
					® Obtains cpg (key?), sends cpt to cpg on right
					® obtains eub by prl, and send cpt to the eub
				□ Eub passes each cpt to all the Eqs that have registered for the lsm pair


 asciidraw.com                                                                                
     */

    /// <summary>
    /// Just used to merge literals in Pti_s to make a new Ctp. 
    /// </summary>
    public class Eqc : Eqb
    {
        public Eqc(Abt abtToEquate, Unb unb, Urc urc, Res res, ushort nOffsetShowTerm) : base(abtToEquate, unb, urc, res, nOffsetShowTerm)
        {
        }
    }

    /// <summary>
    /// Combine two pti with same Lsm in the middle, form a new Pti
    /// </summary>
    public class Cpg : Pcu2<Pti, Pti>
    {
        GnpCpg gnp;
        bool fInactive = false;
#if DEBUG
        Lsm lsmId;
#endif

        public Cpg(Res res, Lsm lsmId) : base(res)
        {
#if DEBUG
            this.lsmId = lsmId;
#endif
        }

        public override int nSize()
        {
            if (gnp == null)
                return 1;
            return gnp.nSize();
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append(GetType().Name);
            sb.Append("#");
            sb.Append(gnp.nId);
        }

        public override bool fStep()
        {
            Pti ptiLeft;
            Pti ptiRight;
            if (gnp.fGetNextPair(out ptiLeft, out ptiRight))
            {
                if (ptiLeft.ascB != ptiRight.ascB    // don't do reverse
                    || ptiLeft.nFromOffset == ptiRight.nFromOffset)   // allow two in same direction
                    gnp.ProcessPair(ptiLeft, ptiRight);
                return false;
            }
            else
            {
                fInactive = true;
                return true;  // all results distributed
            }
        }


        /// <summary>
        /// Add 
        /// </summary>
        public override void TransferLeft(Pti inlLeft)
        {
            if (res.irp != null)
                res.irp.Report(Tcd.tcdTransferLeftCpg, this, inlLeft, null);
            if (gnp == null)
                gnp = new GnpCpg(res, this);
            gnp.AddToOneSide(gnp.basLefts, false, inlLeft);
            Reactivate();
        }

        /// <summary>
        /// Receive solutions to matching these terms
        /// </summary>
        /// <param name="inrRight"></param>
        public override void TransferRight(Pti inrRight)
        {
            if (res.irp != null)
                res.irp.Report(Tcd.tcdTransferRightCpg, this, inrRight, null);
            if (gnp == null)
                gnp = new GnpCpg(res, this);
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

        public override Ipr iprPriority()
        {
            return (gnp == null) ? Npr.nprOnly : gnp.iprPriority();
        }

        public override Ipp<Pti, Pti> ippGet()
        {
            return gnp;
        }
    }

    public class BasPtiPti : Bas
    {
        public BasPtiPti(Gnb gnp, bool fRight, Bab basPrev, Ipr iprSize) : base(gnp, fRight, basPrev, iprSize)
        {
        }
    }

    /// <summary>
    /// Form new Cpt from pti,pti pairs that connect with the same lsm on right of 1st pti and left of 2nd
    /// </summary>
    public class GnpCpg : Gnp<Pti, Pti>
    {

        public GnpCpg(Res res, Sprc sprcOwner) : base(res, sprcOwner)
        {
        }

        public override void AddToFiltered(bool fRight, Asc ascFiltered)
        {
            throw new NotImplementedException();
        }

        public override Bas babNew(bool fRight, Bas babPrev, Ipr iprSize)
        {
            return new BasPtiPti(this, fRight, babPrev, iprSize);
        }

        public override Asc ascIsSubsumed(bool fRight, Rib ascNew, bool fAdd)
        {
            return null;
        }

        public override void ProcessPair(Pti ptiLeft, Pti ptiRight)
        {
            // obtain task to equate the pti and cpt, and the cmr that will use solution to the equate to make new cpt
            Cmr cmr = new Cmr(res, ptiLeft, ptiRight);

            cmr.StartSubproblem(null, null);
        }
    }

    /// <summary>
    /// Constructed by Cpg to be notified by Eqs when the two ptis have been connected.
    /// This will construct new Cpt from Pti and old Cpt. 
    /// Then dispatch it to all Cpg that can consume it.
    /// </summary>
    public class Cmr : Euh, Ent, Urc
    {
        Pti ptiLeft;
        Pti ptiRight;
#if DEBUG
        public Eqs eqs;
#endif

        Atp atpSubproblem;

        //        Mtp mtpMake;  // to equate vbls from solution back to inputs
        //        int nId = Etp.cId++;

        public Cmr(Res res, Pti ptiLeft, Pti ptiRight) : base(res, null)
        {
            this.res = res;
            this.ptiLeft = ptiLeft;
            this.ptiRight = ptiRight;
        }
        public override Vbv vbvGetUsedAtValue(sbyte nVblId, Vbv vbvOutput, Vbv vbvPti)
        {
            return null;
            // is something needed ?
            // throw new NotImplementedException();
        }

        public override Asb asbGet()
        {
            return atpSubproblem;
        }

        public void MakeResolvantTerm(Shv shv, ushort nOffsetShowTerm)
        {
            shv.InitForCmr();
        }

        public override Ipr iprComplexity()
        {
            Asc ascA = ptiLeft.ascB;
            Asc ascB = ptiRight.ascB;
            Ipr iprA = ascA.iprComplexity();
            Ipr iprB = ascB.iprComplexity();
            return iprA.iprMax(iprB);
           // return ptiLeft.ascB.iprPriority()
           //         + ptiRight.ascB.iprPriority();
        }

        public Ipr iprPriority()
        {
            throw new NotImplementedException();
        }

        public override void StartSubproblem(Vbv vbvPrev, Vbv vbvPti)
        {
            Spl splSubproblem = splMake(vbvPrev, vbvPti);
            atpSubproblem = splSubproblem.atpCreateOutput(
                                       out mobToOffsetForChild, out mvbMapToVblIdForChild);
            MeasureForSplit(atpSubproblem);
            Eqs eqsSub = res.eqsObtain(atpSubproblem);

#if DEBUG
            eqs = eqsSub;
#endif

            if (res.irp != null)
                res.irp.Report(Tcd.tcdNewCmr, ptiLeft, ptiRight, this);

            eqsSub.TransferLeft(this);
        }

        protected override Spl splMake(Vbv vbvPrev, Vbv vbvPti)
        {
            Spl spl = new Spl();
            // match lhs of eqsToNotify to lhs of Pti
            spl.Init(ptiLeft.ascB, ptiLeft.nToOffset,
                     ptiRight.ascB, ptiRight.nFromOffset,
                        null, null, true);
            return spl;
        }

        protected override bool fAcceptSolution(Vbv vbvInput)
        {
#if false
            // the following seems wrong. Shouldn't be used?
            for (Vbv vbvChild = vbvInput.vbvFirst; vbvChild != null; vbvChild = vbvChild.vbvNext)
            {
                Vbv vbvValue;
                ushort nReplaceAtMapped;
                Spl.MapValue(mobToOffsetForChild, vbvChild.nReplaceAtPosn, out nReplaceAtMapped, out vbvValue);

                if (nReplaceAtMapped >= ptiLeft.nToOffset)
                {
                    ushort nTermSize = ptiLeft.ascB.nTermSize(ptiLeft.nToOffset);
                    if (nReplaceAtMapped < ptiLeft.nToOffset + nTermSize)
                        return false;
                }

               // The following check is not done because might use another part of asc
               // if (vbvChild.asb == ptiRight.ascB)
               //     return false;
            }
#endif
            return base.fAcceptSolution(vbvInput);
        }

        protected override void MakeSolnVbv(Vbv vbvInput, out Vbv vbvOutput, out Vbv vbvPti)
        {
            Avc avcLeft = new Avc(new Aic(ptiLeft.ascB));
            Avc avcRight = new Avc(new Aic(ptiRight.ascB));
            vbvOutput = new Vbv(ptiLeft.ascB);
            vbvPti = new Vbv(ptiRight.ascB);
            vbvPti.pti = ptiRight; // needed for fMergeOtherTerms
            vbvOutput.avcOfValue = avcLeft;
            vbvPti.avcOfValue = avcRight;

            SplitVbv(vbvInput, vbvOutput, vbvPti);
        }

        protected override void CreateNextStep(Vbv vbvOutput, Vbv vbvPti)
        {
            if (Vbv.fFindOccurs(ptiLeft.ascB, ptiRight.ascB, vbvOutput, vbvPti))
                return;
            if (vbvOutput.fCheckConflictingReplaceAtPosn(vbvOutput, vbvPti))
                return;

            ValidateSoln(vbvOutput, vbvPti);

            Asc ascR = ascMakeCombined(vbvOutput, vbvPti);
            if (ascR == null)
                return;

            res.SaveForFilter(ascR);

            // TODO: create task to filter using embed. Problem is that several lists are added to in SavePti 
            //GnpR.KSide kSide = res.imd.kSide(ascR);
            // res.AddToFiltered(kSide != GnpR.KSide.kLeft, ascR);
        }

        Asc ascMakeCombined(Vbv vbvInput, Vbv vbvPti)
        {
            // TODO: this probably has lots of problems

            // check for conflict in nReplaceAt among children
            if (vbvInput.fHasChildReplaceAt(ptiLeft.nToOffset))
                return null;

            Avc avcA = vbvInput.avcOfValue;
            Avc avcB = vbvPti.avcOfValue;
            Abt abt = new Abt(avcA, avcB);

            Unb unb = null;
            Urc urc = this;
            ushort nOffsetShowTerm = 0;  // not used by Pmu
            Eqc eqc = new Eqc(abt, unb, urc, res, nOffsetShowTerm);
            Pmu pmu = new Pmu(res, ptiRight);
            Shv shv = new Shv(pmu, vbvInput, vbvPti, avcA, avcB, mvbMapToVblIdForChild, mobToOffsetForChild);

            vbvPti.fJoinPti = true;
            vbvPti.nReplaceAtPosn = ptiLeft.nToOffset;   // not in MakeSolnVbv, because validation uses vbv_s separately
            vbvPti.nReplaceFromPosn = ptiRight.nFromOffset;
            vbvPti.nReplaceWithPosn = ptiRight.nToOffset;   // not in MakeSolnVbv, because validation uses vbv_s separately
            vbvInput.AddChild(vbvPti);

            shv.MakeRgnTermOffset(Asb.nMaxTerms);

            Vbv.StartGlobalHack(atpSubproblem);
            bool fTautology = eqc.fMergeClauses(shv, urc, false);       // combine the literals into one Asc
            Vbv.EndGlobalHack();
            if (fTautology)
                return null;

            Asc ascR = shv.ascBuild(shv.ascbRes);
            // shv.cmb.SetOrigin(ascR, vbvInput, unb, mvbMapToVblIdForChild, mobToOffsetForChild);

            ascR.ribLeft = abt.avcA.aic.asc;
            ascR.ribRight = abt.avcB.aic.asc;
            ascR.esnSolution = vbvInput;
            ascR.gfbSource = Gfc.gfcFromPosHyp;

            if (res.irr != null)
                res.irr.AscCreated(ascR, shv);
            if (res.irp != null)
                res.irp.Report(Tcd.tcdNewAscConnect, ascR.ribLeft, ascR.ribRight, ascR);
            return ascR;
        }

        Cmb Urc.cmbInferencer()
        {
            throw new NotImplementedException();
        }

        void Urc.GetMergeParms(out bool fResolveNegSide, out uint nSkipFieldFF, out uint nSkipFieldTF, out uint nSkipFieldFT, out uint nSkipFieldTT)
        {
            Pmu.SetMergeParms(true, ptiRight, out fResolveNegSide, out nSkipFieldFF, out nSkipFieldTF, out nSkipFieldFT, out nSkipFieldTT);
        }

        void Urc.GrowUnification(Unb unb)
        {
            throw new NotImplementedException();
        }

        public override void MapValue(ushort nValue, out ushort nValueNew, out Vbv vbvForValue, Vbv vbvOutput, Vbv vbvPti)
        {
            Vbv vbvValue;
            Spl.MapValue(mobToOffsetForChild, nValue, out nValueNew, out vbvValue);
#if false
            vbvForValue = vbvValue;
#else
            // switch to refer directly, so that Spl will not convert, as a result of
            //            vbaForVbl.vbvForValue.fNeedsMapping())
            if (vbvValue == Vbv.vbvB)
                vbvForValue = vbvPti;
            else
                vbvForValue = vbvOutput;
#endif
        }

        public override void ValidateSoln(Vbv vbvOutput, Vbv vbvPti)
        {
            if (res.fValidateEub)
            {
                // turned off, because pti_s mean the two sides are not necessarily same, just equal.
                // How to validate the SplitVbv, and/or ascMakeCombined ?

                Moa mobToOffsetForChildValidate;
                Mva mvbMapToVblIdForChildValidate;
                Spl spl = new Spl();
                spl.Init(ptiLeft.ascB, ptiLeft.nToOffset,
                    ptiRight.ascB, ptiRight.nFromOffset,
                    vbvOutput, vbvPti, true);

                // spl.rgnMapToOffsetForChild = rgnMapToOffsetForChild;
                Atp atpResult = spl.atpCreateOutput(
                    out mobToOffsetForChildValidate, out mvbMapToVblIdForChildValidate);
                if (!atpResult.fSymmetric(true))
                    throw new ArgumentException();
            }
        }
    }
}
