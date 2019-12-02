using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab
{

    /// <summary>
    /// Base class for goal for clause. E.g. axiom, negated hypothesis or intemediate hypothesis.
    /// </summary>
    public abstract class Gfb : Bid
    {
        public Gfb()
        {
        }

        /// <summary>
        /// output prove takes this as axiom
        /// </summary>
        public abstract bool fIsGiven();

        public abstract bool fIsFromNegation();

        public abstract Gfh gfhIsSecondaryHyp();

        /// <summary>
        /// Utility to compute complexity of an Asc, depending on the context in which it was created.
        /// </summary>
        public abstract Ipr iprComplexityOf(Asc ascOf);

        /// <summary>
        /// Return true if other hypothesis is a special case of this.
        /// </summary>
        public virtual bool fSubsumes(Gfb gfbOther)
        {
            return gfbOther == this;
        }
    }

    /// <summary>
    /// Goal for clause. For static cases
    /// </summary>
    public class Gfc : Gfb
    {
        string stName;
        bool fGiven;
        bool fFromNegation;

        public static Gfc gfcAxiom;
        public static Gfc gfcNegHypothesis;
        public static Gfc gfcFromNegHyp;
        public static Gfc gfcFromPosHyp;

        public Gfc(string stName, bool fGiven, bool fFromNegation)
        {
            this.stName = stName;
            this.fGiven = fGiven;
            this.fFromNegation = fFromNegation;
        }

        public new static void Reset()
        {
            gfcAxiom = new Gfc("axiom", true, false);
            gfcNegHypothesis = new Gfc("NegHypothesis", true, true);
            gfcFromNegHyp = new Gfc("FromNegHyp", false, true);
            gfcFromPosHyp = new Gfc("FromPosHyp", false, false);
        }

        public override bool fIsGiven()
        {
            return fGiven;
        }

        public override bool fIsFromNegation()
        {
            return fFromNegation;
        }
        public override Gfh gfhIsSecondaryHyp()
        {
            return null;
        }

        public override Ipr iprComplexityOf(Asc ascOf)
        {
            return ascOf.iprComplexityRaw();
        }

    }

    /// <summary>
    /// A conclusion formed by refuting a Gfh. 
    /// </summary>
    public class Gfi : Gfb
    {
        public Gfh gfhFrom;

        public Gfi(Gfh gfhFrom)
        {
            this.gfhFrom = gfhFrom;
        }

        public override Ipr iprComplexityOf(Asc ascOf)
        {
            return Prp.prpBehind(gfhFrom.iprComplexityOf(ascOf));
        }

        public override bool fIsFromNegation()
        {
            throw new NotImplementedException();
        }

        public override bool fIsGiven()
        {
            return false;
        }

        public override Gfh gfhIsSecondaryHyp()
        {
            return null; // the Asc that refers to this was created from a hypothesis
        }
    }

    /// <summary>
    /// Base class for goals formed from hypothesis rather than axioms and resolution steps
    /// </summary>
    public abstract class Gfh : Gfb
    {
        public override bool fIsFromNegation()
        {
            return true;
        }

        public override bool fIsGiven()
        {
            return false;
        }
        public override Gfh gfhIsSecondaryHyp()
        {
            return this;
        }

        /// <summary>
        /// make Asc that can be concluded from a proof that refutes a Gfh.
        /// </summary>
        public abstract Asc ascImpliedByRefutation(Res res, Vbv vbvSolution);

        /// <summary>
        /// Move Gfl(AND) outside of Gfp(OR) by distributing
        /// </summary>
        public abstract Gfh gfhToNormalForm();

        /// <summary>
        /// Combine a group of Gfh to a single Asc
        /// </summary>
        public abstract void AddToAsc(Mgb msb);

        public override bool fSubsumes(Gfb gfbOther)
        {
            // gfhToNormalForm(), check ands and ors
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Hold information for building up a merged Asc from a set of hypotheses that led to refutation
    /// </summary>
    public class Mgb
    {
        public sbyte[] rgnTree;
        public Asy asy;
        public int nOutPos;
        public int nNumNegTerms = 0;
        public int nNumPosTerms = 0;
        public int nNextVblNum = Asb.nVar;
    }

    /// <summary>
    /// Negated goal clause: 
    /// </summary>
    public class Ngc : Gfh
    {
        public Eqs eqsGoal;

        public Ngc(Eqs eqsGoal)
        {
            this.eqsGoal = eqsGoal;
        }

        public override Ipr iprComplexityOf(Asc ascOf)
        {
            return Prp.prpBehind(ascOf.iprComplexityRaw());
        }

#if false
        public override Gsb gsbMake()
        {
            return new Gss(this);
        }
#endif
        public override Gfh gfhToNormalForm()
        {
            return this;
        }

        /// <summary>
        /// Create negative Asc that is introduced as a hypothesis to be refuted.
        /// </summary>
        public Asc ascNegatedHypothesis()
        {
            // Don't fReplaceWithSkolem yet, so that vbls can equate to some other symbol.
            // Need to check later and replace the remaining vbls with skolem at that time
            Spl spl = new Spl();
            Atp atpForEqs = eqsGoal.atpToEquate;

            // for now do it slow way, atp first, then convert to Asc
            Moa mobToOffsetForChildValidate;
            Mva mvbMapToVblIdForChildValidate;
            spl.Init(atpForEqs, Atp.nOffsetFirstTerm,
                     atpForEqs, atpForEqs.nRightSidePosn(),
                     null, null, true);

            Atp atpResult = spl.atpCreateOutput(
                            out mobToOffsetForChildValidate, out mvbMapToVblIdForChildValidate);
            return ascEqFromAtp(atpResult, false, this);
        }

        /// <summary>
        /// Create the Asc that results from refuting an Ngc, 
        /// with substitutions required because of what may be a special case that was proved
        /// </summary>
        public override Asc ascImpliedByRefutation(Res res, Vbv vbvSolution)
        {
            Asc ascR = ascEqFromAtp(eqsGoal.atpToEquate, true, new Gfi(this));
            return ascR;
#if false
            // Multiple Eqs are grouped into a single Asc only when a Gfp groups them.
            // Replace any remaining vbls in the Ngc.
            Spl spl = new Spl();
            Atp atpForEqs = eqsGoal.atpToEquate;

            // for now do it slow way, atp first, then convert to Asc
            Moa mobToOffsetForChildValidate;
            Mva mvbMapToVblIdForChildValidate;
            spl.Init(atpForEqs, Atp.nOffsetFirstTerm,
                     atpForEqs, atpForEqs.nRightSidePosn(),
                     vbvSolution,
                     (vbvSolution == null) ? null : vbvSolution.vbvBSide(),
                     true);

            Atp atpResult = spl.atpCreateOutput(
                            out mobToOffsetForChildValidate, out mvbMapToVblIdForChildValidate);
            Asc ascR = ascEqFromAtp(atpResult, true, new Gfi(this));

            if (res.irr != null)
                res.irr.AscCreated(ascR, null);
            if (res.irp != null)
                res.irp.Report(Tcd.tcdAscFromNgc, this, vbvSolution, ascR);
            res.SaveForFilter(ascR);
            return ascR;
#endif
        }

        public Asc ascEqFromAtp(Atp atpResult, bool fPos, Gfb gfbSource)
        {
            Lsm[] rglsmIn = atpResult.rglsmData;
            int nTerms = atpResult.rgnTree.Length - Atp.nOffsetFirstTerm;
            sbyte[] rgnOutTree = new sbyte[nTerms + Asc.nClauseLeadingSizeNumbers + 1];
            Asy asy = new Asy();
            rgnOutTree[Asc.nClauseLeadingSizeNumbers] = (sbyte)asy.nIdForLsm(Lsm.lsmEquals);
            for (int nCopy = 0; nCopy < nTerms; nCopy++)
            {
                sbyte nId = atpResult.rgnTree[nCopy + Atp.nOffsetFirstTerm];
                if (nId <= Asb.nLsmId)
                    nId = (sbyte)asy.nIdForLsm(rglsmIn[Asb.nLsmId - nId]);
                rgnOutTree[nCopy + Asc.nClauseLeadingSizeNumbers + 1] = nId;
            }
            Asc ascR = new Asc(rgnOutTree, asy.rglsmData());
            ascR.gfbSource = gfbSource;
            if (fPos)
            {
                ascR.rgnTree[Asc.nPosnNumNegTerms] = 0;
                ascR.rgnTree[Asc.nPosnNumPosTerms] = 1;
            }
            else
            {
                ascR.rgnTree[Asc.nPosnNumNegTerms] = 1;
                ascR.rgnTree[Asc.nPosnNumPosTerms] = 0;
            }
            return ascR;
        }

        /// <summary>
        ///  Old version, doesn't work or handle replacement
        /// </summary>
        static readonly sbyte nUnassignedVblId = 127;
        Asc ascForEqs_Old(bool fReplaceWithSkolem, Gfb gfbSource, bool fPos, Vbv vbvSolution)
        {
            Asy asy = new Asy();
            Atp atpToEquate = eqsGoal.atpToEquate;
            int nNextLevel = atpToEquate.nMaxLsmLevel() + 1;
            sbyte[] rgnIdForVbl = null;
            if (fReplaceWithSkolem)
            {
                int nMaxVblId = atpToEquate.nMaxVarId(Atp.nOffsetFirstTerm);
                rgnIdForVbl = new sbyte[nMaxVblId + 1];
                for (int nVblId = Asb.nVar; nVblId <= nMaxVblId; nVblId++)
                    rgnIdForVbl[nVblId] = nUnassignedVblId;
            }
            int nLenAtp = atpToEquate.rgnTree.Length - Atp.nOffsetFirstTerm;
            sbyte[] rgnTOutputBuffer = new sbyte[Ascb.nInitialBufferSize];
            int nOutPos = Asc.nClauseLeadingSizeNumbers;
            int nInPos = Atp.nOffsetFirstTerm;
            rgnTOutputBuffer[nOutPos++] = (sbyte)asy.nIdForLsm(Lsm.lsmEquals);
            for (int nPos = 0; nPos < nLenAtp; nPos++)
            {
                sbyte nInValue = atpToEquate.rgnTree[nInPos++];
                sbyte nOutValue;
                if (nInValue >= Asb.nVar)
                {
                    nOutValue = rgnIdForVbl[nInValue];
                    if (nOutValue == nUnassignedVblId)
                    {
                        Vba vbaForVbl = null;
                        if (vbvSolution != null
                            && (vbaForVbl = vbvSolution.vbaFind(nInValue)) != null)
                        {
                            // moving to top would require moving special handling of vblIds for this use case
                            Sps sps = new Sps();
                            Ascb ascbTerm = new Ascb();
                            Mva mvbMapForChild = null;
                            sps.Init(ascbTerm, mvbMapForChild);
                            sps.fPtiEnabled = true;
                            Moa mobToOffsetForChild = null;

                            sps.mobToOffsetForChild = mobToOffsetForChild;
                            Vbv vbvBside = vbvSolution.vbvBSide();
                            sps.SetupSoln(vbvSolution.asb, vbvBside != null ? vbvBside.asb : vbvSolution.asb,
                                          vbvSolution, vbvBside);
                            sps.SetupSbst(vbaForVbl.nValue, vbaForVbl.vbvForValue);
                            sps.ProcessParts();
                            sps.GetResult();
                            for (int nCopy = Atp.nOffsetFirstTerm; nCopy < sps.nOutPosn; nCopy++)
                            {
                                rgnTOutputBuffer[nOutPos++] = ascbTerm.rgnBuffer[nCopy];
                            }
                            continue;
                        }
                        else if (fReplaceWithSkolem)
                        {
                            //todo: checked if value is Assignment, if so then write value (use Ascb)

                            Lsm lsmForVbl = new Lsm("Q_" + eqsGoal.nId + "." + nInValue);
                            lsmForVbl.nArity = Lsm.nArityConst;
                            lsmForVbl.nLevel = nNextLevel;

                            nOutValue = (sbyte)asy.nIdForLsm(lsmForVbl);
                            rgnIdForVbl[nInValue] = nOutValue;
                        }
                        else
                            nOutValue = nInValue;
                    }
                    else
                        nOutValue = nInValue;
                }
                else
                {
                    Lsm lsmSym = atpToEquate.rglsmData[Asb.nLsmId - nInValue];
                    nOutValue = (sbyte)asy.nIdForLsm(lsmSym);
                }
                rgnTOutputBuffer[nOutPos++] = nOutValue;
            }
            sbyte[] rgnOutTree = new sbyte[nOutPos];
            for (int nCopy = Asc.nClauseLeadingSizeNumbers; nCopy < nOutPos; nCopy++)
                rgnOutTree[nCopy] = rgnTOutputBuffer[nCopy];

            Asc ascR = new Asc(rgnOutTree, asy.rglsmData());
            ascR.gfbSource = gfbSource;
            if (fPos)
            {
                ascR.rgnTree[Asc.nPosnNumNegTerms] = 0;
                ascR.rgnTree[Asc.nPosnNumPosTerms] = 1;
            }
            else
            {
                ascR.rgnTree[Asc.nPosnNumNegTerms] = 1;
                ascR.rgnTree[Asc.nPosnNumPosTerms] = 0;
            }
            return ascR;
        }

        public override void AddToAsc(Mgb mgb)
        {
            Atp atpToEquate = eqsGoal.atpToEquate;
            sbyte nIdEquals = (sbyte)mgb.asy.nIdForLsm(Lsm.lsmEquals);
            int nLenAtp = atpToEquate.rgnTree.Length - Atp.nOffsetFirstTerm;
            int nInPos = Atp.nOffsetFirstTerm;
            mgb.rgnTree[mgb.nOutPos++] = (sbyte)mgb.asy.nIdForLsm(Lsm.lsmEquals);
            int nVblBase = mgb.nNextVblNum; // assume all vbls are different
            int nVblMax = Asb.nVar - 1;
            for (int nPos = 0; nPos < nLenAtp; nPos++)
            {
                sbyte nInValue = atpToEquate.rgnTree[nInPos++];
                sbyte nOutValue;
                if (nInValue >= Asb.nVar)
                {
                    nOutValue = (sbyte)(nInValue + nVblBase);
                    if (nInValue > nVblMax)
                        nVblMax = nInValue;
                }
                else
                {
                    Lsm lsmSym = atpToEquate.rglsmData[Asb.nLsmId - nInValue];
                    nOutValue = (sbyte)mgb.asy.nIdForLsm(lsmSym);
                }
                mgb.rgnTree[mgb.nOutPos++] = nOutValue;
            }
            mgb.nNextVblNum += nVblMax + 1;
            mgb.nNumPosTerms++;
        }
    }

    /// <summary>
    /// List of Gfb. Both first and second are valid, separately.
    /// This means there are two hypotheses or more that resulted in the same resolvant.
    /// </summary>
    public class Gfl : Gfh
    {
        public Gfh gfhFirst;
        public Gfh gfhSecond;

        public Gfl(Gfh gfhFirst, Gfh gfhSecond)
        {
            this.gfhFirst = gfhFirst;
            this.gfhSecond = gfhSecond;
        }

        public override Ipr iprComplexityOf(Asc ascOf)
        {
            Ipr iprLeft = gfhFirst.iprComplexityOf(ascOf);
            Ipr iprRight = gfhSecond.iprComplexityOf(ascOf);
            return iprLeft.iprMin(iprRight);
        }


#if false
        public override Gsb gsbMake()
        {
            return new Gsl(this);
        }
#endif

        public override Gfh gfhToNormalForm()
        {
            Gfh gfhNormalFirst = gfhFirst.gfhToNormalForm();
            Gfh gfhNormalSecond = gfhSecond.gfhToNormalForm();
            if (gfhNormalFirst == gfhFirst
                && gfhNormalSecond == gfhSecond)
                return this;
            Gfl gflNew = new Gfl(gfhNormalFirst, gfhNormalSecond);
            return gflNew;
        }

        public override Asc ascImpliedByRefutation(Res res, Vbv vbvSolution)
        {
            gfhFirst.ascImpliedByRefutation(res, vbvSolution);
            gfhSecond.ascImpliedByRefutation(res, vbvSolution);
            return null;  // ?
        }

        public override void AddToAsc(Mgb mgb)
        {
            throw new ArgumentException();
        }
    }

    /// <summary>
    /// Product of Gfb. Form the resulting set by concat each from left with each from right.
    /// Concat means disjunction of hypotheses: can't tell which one is the correct result, but 
    /// one or more of them is.
    /// </summary>
    public class Gfp : Gfh
    {
        public Gfh gfhLeft;
        public Gfh gfhRight;

        public Gfp(Gfh gfhLeft, Gfh gfhRight)
        {
            this.gfhLeft = gfhLeft;
            this.gfhRight = gfhRight;
        }

        public override Ipr iprComplexityOf(Asc ascOf)
        {
            Ipr iprLeft = gfhLeft.iprComplexityOf(ascOf);
            Ipr iprRight = gfhRight.iprComplexityOf(ascOf);
            return Prp.prpBehind(iprLeft.iprMax(iprRight));
        }


#if false
        public override Gsb gsbMake()
        {
            return new Gsp(this);
        }
#endif

        public override Gfh gfhToNormalForm()
        {
            Gfh gfhNormalLeft = gfhLeft.gfhToNormalForm();
            Gfh gfhNormalRight = gfhRight.gfhToNormalForm();
            if (gfhNormalLeft == gfhLeft
                && gfhNormalRight == gfhRight)
                return this;
            if (gfhNormalLeft is Gfl)
            {
                Gfl gflLeft = (Gfl)gfhNormalLeft;
                Gfp gfpFirst = new Gfp(gflLeft.gfhFirst, gfhNormalRight);
                Gfp gfpSecond = new Gfp(gflLeft.gfhSecond, gfhNormalRight);
                Gfl gflNew = new Gfl(gfpFirst, gfpSecond);
                return gflNew.gfhToNormalForm();
            }
            else if (gfhNormalRight is Gfl)
            {
                Gfl gflRight = (Gfl)gfhNormalRight;
                Gfp gfpFirst = new Gfp(gfhNormalLeft, gflRight.gfhFirst);
                Gfp gfpSecond = new Gfp(gfhNormalLeft, gflRight.gfhSecond);
                Gfl gflNew = new Gfl(gfpFirst, gfpSecond);
                return gflNew;
            }
            else
            {
                Gfp gfpNew = new Gfp(gfhNormalLeft, gfhNormalRight);
                return gfpNew;
            }
        }

        public override Asc ascImpliedByRefutation(Res res, Vbv vbvSolution)
        {
            Mgb mgb = new Mgb();
            mgb.rgnTree = new sbyte[Ascb.nInitialBufferSize];
            mgb.nOutPos = Asc.nClauseLeadingSizeNumbers;
            mgb.asy = new Asy();

            AddToAsc(mgb);

            mgb.rgnTree[Asc.nPosnNumNegTerms] = (sbyte)mgb.nNumNegTerms;
            mgb.rgnTree[Asc.nPosnNumPosTerms] = (sbyte)mgb.nNumPosTerms;

            sbyte[] rgnTreeR = new sbyte[mgb.nOutPos];
            for (int nPos = Asc.nClauseLeadingSizeNumbers; nPos < mgb.nOutPos; nPos++)
                rgnTreeR[nPos] = mgb.rgnTree[nPos];
            Asc ascR = new Asc(rgnTreeR, mgb.asy.rglsmData());
            ascR.gfbSource = new Gfi(this);

            if (res.irp != null)
            {
                res.irr.AscCreated(ascR, null);
                res.irp.Report(Tcd.tcdNewAscProduct, ascR.ribLeft, ascR.ribRight, ascR);
            }
            res.SaveForFilter(ascR);
            return ascR;
        }

        public override void AddToAsc(Mgb mgb)
        {
            gfhLeft.AddToAsc(mgb);
            gfhRight.AddToAsc(mgb);
        }
    }

    /// <summary>
    /// Base class for iterating through a tree of Gfl/Gfp to generate series of Asc
    /// </summary>
    public abstract class Gsb
    {
        /// <summary>
        /// Create Asc to represent the negation of hypotheses and add it
        /// </summary>
        public abstract void MakeAsc(Res res);

        /// <summary>
        /// Return a Gsb representing the next step of iteration, or null at end.
        /// </summary>
        public abstract Gsb gsbNext();
    }
#if false
    /// <summary>
    /// Iteration has only single value
    /// </summary>
    public class Gss : Gsb
    {
        Gfh gfhPlace;

        public Gss(Gfh gfhPlace)
        {
            this.gfhPlace = gfhPlace;
        }

        public override Gsb gsbNext()
        {
            return null;
        }
    }

    /// <summary>
    /// State of iteration through Gfl
    /// </summary>
    public class Gsl : Gsb
    {
        bool fFirst = true;
        bool fSecond = true;   // ready to return gfbSecond
        Gfl gflPlace;

        public Gsl(Gfl gflPlace)
        {
            this.gflPlace = gflPlace;
        }
        public override void MakeAsc(Res res)
        {
            ?
        }

        public override Gsb gsbNext()
        {
            if (fFirst)
            {
                fFirst = false;
                return this;
            }
            if (fSecond)
            {
                fSecond = false;
                return this;
            }
            return null;
        }
    }

    /// <summary>
    /// State of iteration through Gfp
    /// </summary>
    public class Gsp : Gsb
    {
        Gsb gsbLeft;
        Gsb gsbRight;
        Gfp gfpPlace;

        public Gsp(Gfp gfpPlace)
        {
            this.gfpPlace = gfpPlace;
            gsbLeft = gfpPlace.gfhLeft.gsbMake();
            gsbRight = gfpPlace.gfhRight.gsbMake();
        }

        public override Gsb gsbNext()
        {
            gsbRight = gsbRight.gsbNext();
            if (gsbRight == null)
            {
                gsbLeft = gsbLeft.gsbNext();
                if (gsbLeft == null)
                    return null;
                gsbRight = gfpPlace.gfhRight.gsbMake();
            }
            return this;
        }
    }
#endif

    /// <summary>
    /// Step through generation of Asc with Gpb when a refutation is found
    /// </summary>
    public class Spg : Spr
    {
        Res res;
        Gsb gsbTop;  // current state in tree of composite Gfl/Gfp

        public Spg(Res res, Gsb gsbStart)
        {
            this.res = res;
            this.gsbTop = gsbStart;
        }

        public bool fStep()
        {
            // Create the next Asc, return true is there are no more.
            gsbTop.MakeAsc(res);
            gsbTop = gsbTop.gsbNext();
            return gsbTop == null;
        }

        public int nSize()
        {
            return 1;
        }
    }

}
