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
    /// Paramodulation 
    /// http://aitopics.org/sites/default/files/classic/Machine%20Intelligence%204/MI4-Ch8-Robinson&Wos.pdf
    /// Paramodulation: Given clauses A and a' = b' V B (or b' =a' V B) having no variable in common and such that 
    /// A contains a term d, with d and a' having a most general common instance a identical to a' [si/ ui] and to d[tj, wj], 
    /// form A' by replacing in A[tj/wj] some single occurrence of a (resulting from an occurrence of d) by b'[si /ui], 
    /// and infer A' V B[si / ui].

    /// </summary>


    /// <summary>
    /// Refer to term or literal to perform paramodulation or resolution on
    ///
    /// Some symbols have equations that can be viewed as definitions of a function, e.g. f(x) = ... or g = ...
    /// The parameters can be constants also.
    /// Restriction is that the parameters cannot themselves be defined, to avoid having to deal with recursive expansion (yet)
    /// Chain the definitions to the symbol being defined.
    /// These definitions are placed in Bas like other Pti. They are only expanded when encountered during equate if the definition matches.
    /// </summary>
    public class Pti : Bid, Rib, Ipd
    {
        public ushort nFromOffset;
        public ushort nToOffset;
        public Ipr iprPriorityValue = Prs.iprDefaultPriority;
        public Asc ascB;
        public uint nLitB;  // literal number where = subst is being done

        public Pti()
        {
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Pti))
                return false;
            Pti pti = (Pti)obj;
            if (nFromOffset != pti.nFromOffset)
                return false;
            if (nToOffset != pti.nToOffset)
                return false;
#if false
            if (nLitB != pti.nLitB)
                return false;
#endif
            if (!ascB.Equals(pti.ascB))
                return false;
            return true;
        }

#if false
        public void AddToSym(Res res)
        {
            Pti ptiNew = this;
            Asc ascB = ptiNew.ascB;
            sbyte nDefinedId = ascB.rgnTree[ptiNew.nFromOffset];
            if (nDefinedId >= Asb.nVar)
            {
                // add to list to handle in equate
                ptiNew.NextSameSize(res.ptiVariableEqn);
                res.ptiVariableEqn = ptiNew;
                return;
            }
            else
            {
                // attach rule to symbol to match during equate
                Lsm lsmSym = ascB.rglsmData[Asb.nLsmId - nDefinedId];
                ptiNew.NextSameSize((Pti)lsmSym.ipdData);
                lsmSym.ipdData = ptiNew;
            }
        }
#endif

        public override int GetHashCode()
        {
            return (int) (ascB.GetHashCode() + nFromOffset + nToOffset + nLitB);
        }

        public Lsm lsmTopFrom ()
        {
            return lsmAt(nFromOffset);
        }

        public Lsm lsmTopTo()
        {
            return lsmAt(nToOffset);
        }

        public Lsm lsmAt(ushort nOffset)
        {
            sbyte nId = ascB.rgnTree[nOffset];
            if (nId > Asb.nLsmId)
                return null;
            return ascB.rglsmData[Asb.nLsmId - nId];
        }

        private void PtiFields(Asc ascB, int nFromOffset, int nToOffset, Ipr iprPriorityValue, uint nLitB)
        {
            this.ascB = ascB;
            this.nFromOffset = (ushort)nFromOffset;
            this.nToOffset = (ushort)nToOffset;
            this.iprPriorityValue = iprPriorityValue;
            this.nLitB = nLitB;
        }

        public static Pti ptiMakeEq(Asc ascB, int nFromOffset, int nToOffset, Ipr iprPriorityValue, uint nLitB)
        {
            Pti ptiNew = new Pti();
            ptiNew.PtiFields(ascB, nFromOffset, nToOffset, iprPriorityValue, nLitB);
            return ptiNew;
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append(nFromOffset);
            sb.Append("->");
            sb.Append(nToOffset);
            sb.Append(" in ");
            ascB.stString(pctl, sb);
        }

        public Lsx lsxTo(Asy asy)
        {
            throw new NotImplementedException();
        }

        public Ipr iprComplexity()
        {
            return iprPriorityValue;
        }

#if false
        public void ParamodulateOnce (Res res, Asc ascA)
        {
            Avc avcA = new Avc(new Aic(ascA));
            Avc avcB = new Avc(new Aic(ascB));
            Abt abt = new Abt(avcA, avcB);
            PrepareAndParamodulateTerm(res, abt, this, nFromOffset, nToOffset);
        }

        static void PrepareAndParamodulateTerm(Res res, Abt abt, Pti pti, int nAlphaPos, ushort nBetaPos)
        {
            Avc avcA = abt.avcA;
            Avc avcB = abt.avcB;

            // for each literal in A
            int nNumATerms = avcA.aic.nLiterals;
            for (int nLitA = 0; nLitA < nNumATerms; nLitA++)
            {
                int nLiteralAPosn = avcA.aic.rgnTermOffset[nLitA];
                int nTermsInLiteralA = avcA.aic.rgnTermSize[nLiteralAPosn];
                // for each subterm in A literal (skip the predicate symbol, it will not match a function term)
                for (ushort nOffsetA = (ushort) (nLiteralAPosn + 1); nOffsetA < nLiteralAPosn + nTermsInLiteralA; nOffsetA++)
                {
                    if (avcA.aic.asc.rgnTree[nOffsetA] >= Asb.nVar)
                        continue;  // don't bother to equate to variable, it is already general

                    Pmu pmu = new Pmu(res, pti);
                    pmu.SetAbt(abt);
                    pmu.nReplaceAtPosn = nOffsetA;
                    Unt unt = new Unt(abt, pmu);
                    pmu.nReplaceWithPosn = nBetaPos;

                    // equate term in A with From side of equals
                    unt.nOffsetA = (ushort)nOffsetA;
                    unt.nOffsetB = (ushort)nAlphaPos;
                    unt.Reset();
                    res.iehEquateHandler(abt, unt, res);

                }
            }
        }
#endif
    }

    /// <summary>
    /// Refer to a Pti and where it is being applied to: position to apply pti and the clauses being paramodulated
    /// </summary>
    public class Pmu : Cmb, Urc
    {
        Pti pti;
        public const ushort nNoReplace = System.UInt16.MaxValue;
        public ushort nReplaceAtPosn = nNoReplace;    // do paramodulation replacement here

        public Pmu(Res res, Pti pti) : base(res)
        {
            this.pti = pti;
        }

        public void MakeResolvantTerm(Shv shv, ushort nOffsetShowTerm)
        {
            throw new NotImplementedException();
        }

        public static void SetMergeParms(bool fFull, Pti pti, out bool fResolveNegSide, out uint nSkipFieldFF, out uint nSkipFieldTF, out uint nSkipFieldFT, out uint nSkipFieldTT)
        {
            fResolveNegSide = false;
            nSkipFieldFF = 0;
            nSkipFieldFT = 0;
            uint nFieldB = 0;
            if (fFull)   // abt.avcB is null for partial in Eqm.SaveAcs
                nFieldB = (uint) 1 << (int) pti.nLitB;
            nSkipFieldTF = nFieldB;
            nSkipFieldTT = nFieldB;
        }

        public void GetMergeParms(out bool fResolveNegSide, out uint nSkipFieldFF, out uint nSkipFieldTF, out uint nSkipFieldFT, out uint nSkipFieldTT)
        {
            SetMergeParms(abt.avcB != null, pti, out fResolveNegSide, out nSkipFieldFF, out nSkipFieldTF, out nSkipFieldFT, out nSkipFieldTT);
        }

        public Cmb cmbInferencer()
        {
            return this;
        }

        public override void SetOrigin(Asc ascR, Esn esnSolution, Unb unb, Mva mvbMapToVblIdForChild, Moa mobToOffsetForChild)
        {
            throw new ArgumentException();
#if false
            base.SetOrigin(ascR, esnSolution, unb, mvbMapToVblIdForChild, mobToOffsetForChild);

#endif
        }

        public void SetOffsetResolveTerm(int nTermPos)
        {
            // offsets are determined by Paramodulate
        }

        public override bool fStep()
        {
            throw new NotImplementedException();
        }

        public override void GrowUnification(Unb unb)
        {
            throw new NotImplementedException();
        }

        public override Ipr iprComplexity()
        {
            return pti.iprComplexity();
        }

        public override int nSize()
        {
            return 1;
        }
    }

}
