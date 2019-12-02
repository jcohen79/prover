#define DEBUG
using GraphMatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab
{

    /// <summary>
    /// Common functionality between resolution and paramodulation
    /// </summary>
    public abstract class Cmb : Bid, Spr, Icmp
    {
        public readonly Res res;
        public Abt abt;


        protected Cmb (Res res)
        {
            this.res = res;
        }
        public void SetAbt(Abt abt)
        {
            this.abt = abt;
        }
        public virtual void SetOrigin(Asc ascR, Esn esnSolution, Unb unb, Mva mvbMapToVblIdForChild, Moa mobToOffsetForChild)
        {
            Asc ascA = abt.avcA.aic.asc;
            Asc ascB = abt.avcB.aic.asc;
            ascR.ribLeft = ascA;
            ascR.ribRight = ascB;
            ascR.esnSolution = esnSolution;
            //ascR.mvbMapToVblIdForChild = mvbMapToVblIdForChild;
            //ascR.mobToOffsetForChild = mobToOffsetForChild;

            Gfh gfhA = ascA.gfbSource.gfhIsSecondaryHyp();
            Gfh gfhB = ascB.gfbSource.gfhIsSecondaryHyp();
            if (gfhA != null)
            {
                if (gfhB != null)
                    ascR.gfbSource = new Gfp(gfhA, gfhB);
                else
                    ascR.gfbSource = gfhA;
            }
            else if (gfhB != null)
                ascR.gfbSource = gfhB;

            else if (ascA.gfbSource.fIsFromNegation()
                || ascB.gfbSource.fIsFromNegation())
                ascR.gfbSource = Gfc.gfcFromNegHyp;
            else
                ascR.gfbSource = Gfc.gfcFromPosHyp;

            if (res.irp != null)
                res.irp.Report(Tcd.tcdNewAscResolve, ascA, ascB, ascR);

        }

        protected int nTermSize ()
        {
            return abt.avcA.aic.asc.rgnTree.Length + abt.avcB.aic.asc.rgnTree.Length;
        }

        /// <summary>
        /// Perform a step. 
        /// </summary>
        /// <returns>True if this should be removed from future calls, at least for now</returns>
        public abstract bool fStep();

        public abstract int nSize();

        /// <summary>
        /// Terms so far unified, so try launch new iterator to expand the set
        /// </summary>
        public abstract void GrowUnification(Unb unb);

        public abstract Ipr iprComplexity();
        public Ipr iprPriority()
        {
            throw new NotImplementedException();
        }

        public virtual void ExtraString(Fwt sb)
        {
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append("{");
            sb.Append(GetType().Name);
            sb.Append(" A:");
            abt.avcA.aic.asc.stString(Pctl.pctlPlain, sb);
            sb.Append(" B:");
            abt.avcA.aic.asc.stString(Pctl.pctlPlain, sb);
            ExtraString(sb);
            sb.Append("}");
        }
    }


    /// <summary>
    /// Iterate through fields on a pair of clauses
    /// </summary>
    public abstract class Rmub : Cmb
    {
        public Rmub(Res res) : base(res)
        {
        }

        protected static int nNumBits(uint nLeft)
        {
            int nC = 0;
            while (nLeft != 0)
            {
                if ((nLeft & 1) != 0)
                    nC++;
                nLeft >>= 1;
            }
            return nC;
        }

        public abstract bool fInit();

        public const int nEndPowerSet = 0;
        public const int nInitialPowerSet = 1;

        /// <summary>
        /// Return the next power set up to max number bits.
        /// Either add a new bit (fGrow) or get next value with same number of bits by moving highest bit only.
        /// </summary>
        public static uint nNextPowerSet(uint nBitField, uint nNumBits, bool fGrow)
        {
            int nHighBit = 0;
            uint nTemp = nBitField;
            while (nTemp > 1)
            {
                nHighBit++;
                nTemp >>= 1;
            }
            if (nHighBit + 1 >= nNumBits)
                return nEndPowerSet;

            if (fGrow)
                return (nBitField | (uint)(2 << nHighBit));
            else
                return (nBitField ^ (uint) (3 << nHighBit));
        }
    }

    public class Rmup : Rmub
    {
        sbyte nResolveTerm;
        uint nMaxLeftBits;
        bool fResolveNegSide;
        uint nLeftFieldBits = nInitialPowerSet;
        int nNumSymmetricPredicates;
        uint nFlipState;   // 1 indicates that term is reversed
        uint nMaxFlipState;

        public Rmup(Res res) : base(res)
        {
        }
        public override void ExtraString(Fwt sb)
        {
            sb.Append(" ");
            sb.Append(nResolveTerm);
            sb.Append(" ");
            sb.Append(nMaxLeftBits);
            sb.Append(" ");
            sb.Append(fResolveNegSide);
            sb.Append(" ");
            sb.Append(nLeftFieldBits);
        }

        public override Ipr iprComplexity()
        {
            Asc ascA = abt.avcA.aic.asc;
            Asc ascB = abt.avcB.aic.asc;
            Ipr iprA = ascA.iprComplexity();
            Ipr iprB = ascB.iprComplexity();
            return iprA.iprMax(iprB);
        }

        public override bool fInit()
        {
            // caller must call SetAbt first
            nResolveTerm = abt.avcB.aic.asc.nResolveTerm;
            fResolveNegSide = (nResolveTerm < abt.avcB.aic.nNegLiterals);
            // true: usual side for positive resolution

            nMaxLeftBits = (uint) abt.avcA.aic.nPosLiterals;
            // CountSymmetricPredicates();
            return false;
        }

        public override bool fStep()
        {
            if (nLeftFieldBits == nEndPowerSet)
                return true;

            Prh prh = new Prh();
            prh.rmu = this;
            prh.fResolveNegSide = fResolveNegSide;
            prh.nLeftFieldBits = nLeftFieldBits;
            prh.nRightFieldBits = (uint) 1 << nResolveTerm;
            if (fResolveNegSide)
                prh.nLeftFieldBits <<= abt.avcA.aic.nNegLiterals;
            else
                prh.nRightFieldBits <<= abt.avcA.aic.nNegLiterals;

            Up1 up1 = new Up1(abt, prh, prh.nLeftFieldBits, nFlipState, fResolveNegSide);
            res.iehEquateHandler(abt, up1, res);

#if false
    Use Lsm.fCommutative
		Cmi.fSetp
			Test: AsqTest.cs
			Like CheckUsyBit, but for Rmup
				
		Resolve: Epu 
			Use setting provided by Cmu/Up1 to flip terms as indicated
			Use Spv ?
		Merge
			Check if each other term can match  new one, in either order
		Subsumes
            replace current with:
            DAG of all terms seen across clauses
            algo is to find a clause that is lower or same in DAG for each term, with consistent vbls
            iterate over terms, depth first search backwards through DAG, find clause that can match each term
            order list of clauses on each Node of DAG?
                in simple id order
                do inverted list scan forward, tracking each node that is lower in the DAG for that term
                answer is any clause that is on list for each term
		Track
            Check flags that epu is the one desired
            remove creation of symmetric axiom, Epu
    Reflexive
        check each literal when doing Merge
            positive: tautology
            negative: skip


            // add back in as first step for symmetric predicates
            if (nFlipState < nMaxFlipState)
            {
                // next selection of current terms to flip for symmetric (commutative) predicate
                nFlipState++;
            }
            else
#endif
            {
                // next set of terms
                nLeftFieldBits = nNextPowerSet(nLeftFieldBits, nMaxLeftBits, false);
                CountSymmetricPredicates(); // reset lower iterator
            }
            return false;
        }

        void CountSymmetricPredicates()
        {
            int nCount = 0;
            Aic aicRight = abt.avcB.aic;
            for (int nTerm = 0; nTerm < aicRight.nNegLiterals; nTerm++)
            {
                ushort nTermPos = aicRight.rgnTermOffset[nTerm];
                sbyte nPredId = aicRight.asc.rgnTree[nTermPos];
                Lsm lsmPred = aicRight.asc.rglsmData[Asb.nLsmId - nPredId];
                if (lsmPred.fCommutative)
                    nCount++;
            }
            nNumSymmetricPredicates = nCount;
            nFlipState = 0;
            nMaxFlipState = (uint) ( (1 << nCount) - 1); 
        }


        /// <summary>
        /// Extend the previous field by one term. This is done when the previous field was unifiable.
        /// It does done even if the previous field resulted in a tautology, since this may remove the tautology and allow progress.
        /// </summary>
        public override void GrowUnification(Unb unb)
        {
            uint nLeftFieldBitsNew = nNextPowerSet(unb.nGetLeftFieldBits(), nMaxLeftBits, true);
            if (nLeftFieldBitsNew == nEndPowerSet)
                return;

            // launch new iterator to handle more terms
            Rmup rmupSub = new Rmup(res);
            rmupSub.SetAbt(abt);
            rmupSub.fInit();
            rmupSub.nLeftFieldBits = nLeftFieldBitsNew;
            res.prs.Add(rmupSub);
        }

        public override void SetOrigin(Asc ascR, Esn esnSolution, Unb unb, Mva mvbMapToVblIdForChild, Moa mobToOffsetForChild)
        {
            base.SetOrigin(ascR, esnSolution, unb, mvbMapToVblIdForChild, mobToOffsetForChild);
            ascR.nLeftMask = unb.nGetLeftFieldBits();
            ascR.nRightMask = (uint) 1 << nResolveTerm;
        }

        public override int nSize()
        {
            return (int) nMaxLeftBits;
        }
    }

    /// <summary>
    /// Step through different fields on both left and right sides (not p1)
    /// </summary>
    public class Rmus : Rmub
    {
        uint nSizeLeftField;
        uint nSizeRightField;
        bool fResolveNegSide = true;
        uint nLeftFieldBits;
        uint nRightFieldBits;

        public Rmus(Res res) : base(res)
        {
        }


        public override Ipr iprComplexity()
        {
            Asc ascA = abt.avcA.aic.asc;
            Asc ascB = abt.avcB.aic.asc;
            Ipr iprA = ascA.iprComplexity();
            Ipr iprB = ascB.iprComplexity();
            return iprA.nCompare(iprB) < 0 ? iprB : iprA;
            //return abt.avcA.aic.asc.iprPriority() +
            //        abt.avcB.aic.asc.iprPriority();
#if false
            // this depends on current state
            int nLeft = abt.avcB.aic.asc.nSizeMaskedTerms(nLeftFieldBits);
            int nRight = abt.avcB.aic.asc.nSizeMaskedTerms(nRightFieldBits);
            return nLeft + nRight;
#endif

        }

        public override void SetOrigin(Asc ascR, Esn esnSolution, Unb unb, Mva mvbMapToVblIdForChild, Moa mobToOffsetForChild)
        {
            base.SetOrigin(ascR, esnSolution, unb, mvbMapToVblIdForChild, mobToOffsetForChild);
            ascR.nLeftMask = nLeftFieldBits;
            ascR.nRightMask = nRightFieldBits;
        }

        public override bool fInit()
        {
            // caller must call SetAbt first
            // fResolveNegSide true: usual side for positive resolution
            do
            {
                if (fResolveNegSide)
                {
                    nSizeLeftField = abt.avcA.aic.nPosLiterals;
                    nSizeRightField = abt.avcB.aic.nNegLiterals;
                }
                else
                {
                    nSizeLeftField = abt.avcA.aic.nNegLiterals;
                    nSizeRightField = abt.avcB.aic.nPosLiterals;
                }
                nLeftFieldBits = nInitialPowerSet;
                nRightFieldBits = nInitialPowerSet;
                if (nSizeLeftField != 0 && nSizeRightField != 0)
                    return false;
                fResolveNegSide = !fResolveNegSide;
            }
            while (!fResolveNegSide);
            return true;
        }

        public override bool fStep()
        {

            Prh prh = new Prh();
            prh.rmu = this;

            prh.fResolveNegSide = fResolveNegSide;
            prh.nLeftFieldBits = nLeftFieldBits;
            prh.nRightFieldBits = nRightFieldBits;
            if (fResolveNegSide)
                prh.nLeftFieldBits <<= abt.avcA.aic.nNegLiterals;
            else
                prh.nRightFieldBits <<= abt.avcB.aic.nNegLiterals;

            Usy usy = new Usy(abt, prh, nLeftFieldBits, nRightFieldBits, fResolveNegSide);
            res.iehEquateHandler(abt, usy, res);

            nLeftFieldBits = nNextPowerSet(nLeftFieldBits, nSizeLeftField, false);
            if (nLeftFieldBits == nEndPowerSet)
            {
                nLeftFieldBits = nInitialPowerSet;
                nRightFieldBits = nNextPowerSet(nRightFieldBits, nSizeRightField, false);
                if (nRightFieldBits == nEndPowerSet)
                {
                    fResolveNegSide = !fResolveNegSide;
                    if (fResolveNegSide)
                        return true;

                    if (fInit())
                        return true;
                }
            }
            return false;
        }

        public override void GrowUnification(Unb unb)
        {
            Usy usy = (Usy)unb;
            GrowUnificationSide(usy, false);
            GrowUnificationSide(usy, true);
        }

        void GrowUnificationSide(Usy usy, bool fRight)
        {
            uint nFieldBitsNew = nNextPowerSet(fRight ? usy.nRightFieldBits : usy.nLeftFieldBits,
                                              fRight ? nSizeRightField : nSizeLeftField,
                                              true);
            if (nFieldBitsNew == nEndPowerSet)
                return;

            // launch new iterator to handle more terms
            Rmus rmusSub = new Rmus(res);
            rmusSub.SetAbt(abt);
            rmusSub.fResolveNegSide = fResolveNegSide;
            rmusSub.nLeftFieldBits = fRight ? nInitialPowerSet : nFieldBitsNew;
            rmusSub.nRightFieldBits = fRight ? nFieldBitsNew : nInitialPowerSet;
            rmusSub.nSizeLeftField = nSizeLeftField;
            rmusSub.nSizeRightField = nSizeRightField;
            res.prs.Add(rmusSub);
        }

        public override int nSize()
        {
            return (int) (nSizeLeftField + nSizeRightField);
        }
    }

    /// <summary>
    /// Common functions for inferences
    /// </summary>
    public interface Urc
    {

        Cmb cmbInferencer();

        void GetMergeParms(out bool fResolveNegSide, out uint nSkipFieldFF, out uint nSkipFieldTF, out uint nSkipFieldFT, out uint nSkipFieldTT);

        void GrowUnification(Unb unb);

        void MakeResolvantTerm(Shv shv, ushort nOffsetShowTerm);

    }

    /// <summary>
    /// Perform resolution (instead of paramodulation)
    /// </summary>
    public class Prh : Urc
    {
        public bool fResolveNegSide;       // true for p1 resolution, is false if resolving on right side of right clause
        public uint nLeftFieldBits;
        public uint nRightFieldBits;
        public Cmb rmu;
        // public bool fResolved = false;

        public const ushort nUndefinedOffset = 127;  // testing only

        public Prh() { }

        public void GrowUnification(Unb unb)
        {
            rmu.GrowUnification(unb);
        }

        public Cmb cmbInferencer()
        {
            return rmu;
        }

        public void MakeResolvantTerm(Shv shv, ushort nOffsetShowTerm)
        {
            shv.MakeAscbResRmu(nOffsetShowTerm);  // fills in shv.ascbK using ShowTerm
        }


        public virtual void GetMergeParms(out bool fResolveNegSide, out uint nSkipFieldFF, out uint nSkipFieldTF, out uint nSkipFieldFT, out uint nSkipFieldTT)
        {
            fResolveNegSide = this.fResolveNegSide;
            nSkipFieldFF = fResolveNegSide ? 0 : nLeftFieldBits;
            nSkipFieldTF = fResolveNegSide ? nRightFieldBits : 0;
            nSkipFieldFT = fResolveNegSide ? nLeftFieldBits : 0;
            nSkipFieldTT = fResolveNegSide ? 0 : nRightFieldBits;
        }

    }
}
