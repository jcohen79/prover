using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab
{

    /// <summary>
    /// Array form of clauses
    /// </summary>
    public class Asc : Asb, Rib
    {
        // for proof construction
        public Rib ribLeft;
        public Rib ribRight;
        public Esn esnSolution;
        public uint nLeftMask;
        public uint nRightMask;
        //public Mvb mvbMapToVblIdForChild; // obsolete?  (always null, because of Epu)
        //public Mob mobToOffsetForChild; // obsolete?


        public const int nPosnNumNegTerms = 0;
        public const int nPosnNumPosTerms = 1;
        public sbyte nResolveTerm;  // When using semantic resolution, which term to resolve on.
                                    // It the first term that causes this clause to be true in the semantic model
                                    // -1 means is left side of resolution
        public const sbyte nNoResolveTerm = -1;
        public const sbyte nHasResolveTerm = 0;  // when not using semantic resolution, is first neg term

        public Gfb gfbSource;
        public const int nNotCounted = -1;

        /// <summary>
        /// Don't resolve two negated goals, it won't tell which hypothesis is wrong
        /// </summary>
        public static bool fAllowedPair(Asc ascLeft, Asc ascRight)
        {
            Gfb gfbLeft = ascLeft.gfbSource;
            Gfb gfbRight = ascRight.gfbSource;
            if (gfbLeft.gfhIsSecondaryHyp() != null
                && gfbRight.gfhIsSecondaryHyp() != null)
            {
                if (gfbLeft.fSubsumes(gfbRight)
                    || gfbRight.fSubsumes(gfbLeft))
                    return true;
                return false;
            }
            return true;
        }

        public Ipr iprComplexity()
        {
            if (gfbSource != null)
                return gfbSource.iprComplexityOf(this);
            else
                return iprComplexityRaw();
        }

        public Ipr iprComplexityRaw()
        {
            return Prn.prnObtain(rgnTree.Length);
        }

        public Ipr iprPriority()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add up the size of the terms selected by the nMask. Low order bit is leftmost term.
        /// </summary>
        public int nSizeMaskedTerms(uint nMask)
        {
            ushort nPos = nClauseLeadingSizeNumbers;
            int nTotalSize = 0;
            while (nMask != 0)
            {
                int nSizeTerm = nTermSize(nPos);
                if ((nMask & 1) != 0)
                {
                    nTotalSize += nSizeTerm;
                }
                nPos = (ushort)(nPos + nSizeTerm);
                nMask >>= 1;
            }
            return nTotalSize;
        }

        public bool fRight()
        {
            return nResolveTerm != nNoResolveTerm;
        }

        public static Asc ascFromLsx(Lsx lsxClause)
        {
            if (lsxClause == Lsm.lsmNil)
            {
                throw new ArgumentException();
                // return;
            }
            Ascb ascb = new Ascb();

            AppendDisjunct(ascb, lsxClause);

            return (Asc)ascb.Build(true);
        }

        public Asc(sbyte[] rgnTree, Lsm[] rglsmData) : base(rgnTree, rglsmData)
        {
        }

        /// <summary>
        /// Traverse proof tree and apply substitutions to negated hypothesis
        /// </summary>
        public Asc ascSbstfromRefutation(Res res, Esn esnSolution)
        {
            Vbv vbvSolution = (Vbv)esnSolution;
            if (ribLeft == null)
            {
                if (ribRight != null)
                    throw new ArgumentException();
                Gfh gfhHypothesis = gfbSource.gfhIsSecondaryHyp();
                if (gfhHypothesis == null)
                    return null;
                return gfhHypothesis.ascImpliedByRefutation(res, vbvSolution);  // Ngc has been refuted, assert its negation
            }
            else
            {
                Asc ascLeft = (Asc)ribLeft;
                Asc ascRight = (Asc)ribRight;
                Asc ascLeftImplied = ascLeft.ascSbstfromRefutation(res, ascLeft.esnSolution);
                Asc ascRightImplied = ascRight.ascSbstfromRefutation(res, ascRight.esnSolution);
                Gfb gfbSource = null;
                Vbv vbvRight = vbvSolution.vbvBSide();

                Ascb ascbR = null;
                Nvi nviList = null;
                sbyte nNextVblId = Atp.nVar;
                bool fRight = false;
                do
                {
                    // base case (above) converts the negative literal into positve, after that they stay positive
                    Asc ascCurrent = fRight ? ascRightImplied : ascLeftImplied;
                    if (ascCurrent != null)
                    {
                        if (ascbR == null)
                        {
                            ascbR = new Ascb();
                            ascbR.DummySizes();
                        }
                        int nNumTerms = ascCurrent.rgnTree[Asc.nPosnNumPosTerms];
                        ushort nTermToShowOffset = Asc.nClauseLeadingSizeNumbers;
                        for (int nLit = 0; nLit < nNumTerms; nLit++)
                        {

                            Sps sps = new Sps();
                            sps.SetVblIds(nNextVblId, nviList);
                            Moa mob = null;   // no longer used?
                            Mva mvb = null;    // no longer used?

                            sps.Init(ascbR, mvb);
                            sps.fPtiEnabled = true;

                            sps.mobToOffsetForChild = mob;
                            sps.SetupSoln((Asc)ribLeft, (Asc)ribRight, vbvSolution, vbvRight);
                            sps.SetupSbst(nTermToShowOffset, fRight ? vbvRight : vbvSolution);
                            sps.ProcessParts();
                            sps.GetVblIds(out nNextVblId, out nviList);
                            sps.GetResult();
                            nTermToShowOffset += ascCurrent.nTermSize(nTermToShowOffset);
                            if (gfbSource == null)
                                gfbSource = ascCurrent.gfbSource;
                        }
                    }
                    fRight = !fRight;
                }
                while (fRight);
                if (ascbR == null)
                    return null;

                Asc ascR = ascbR.ascBuild(0, 1);
                ascR.gfbSource = gfbSource;
                return ascR;
            }
        }


        public void MakePti(Ipt ipt)
        {
            Asc ascB = this;
            Aic aicB = new Aic(ascB);
            int nNumTerms = aicB.nLiterals;
            uint nNumNegTerms = aicB.nNegLiterals;
            // For each positive '=' literal on B asc(add size of term to get to next one
            for (uint nLitB = nNumNegTerms; nLitB < nNumTerms; nLitB++)
            {
                int nLiteralBPosn = aicB.rgnTermOffset[nLitB];
                sbyte nId = ascB.rgnTree[nLiteralBPosn];
                int nISym = nId;
                if (nISym > Asb.nLsmId)
                    continue;  // shouldn't happen
                Lsm lsmSym = ascB.rglsmData[Asb.nLsmId - nISym];
                if (lsmSym != Lsm.lsmEquals)
                    continue;

                int nLeftEqPos = nLiteralBPosn + 1;
                int nLeftSize = aicB.rgnTermSize[nLiteralBPosn + 1];
                int nRightEqPos = nLeftEqPos + nLeftSize;
                int nRightSize = aicB.rgnTermSize[nRightEqPos];

                Ipr iprLeftPriority = nComputePriority(aicB, nLeftEqPos, nRightEqPos);
                Pti ptiLeft = Pti.ptiMakeEq(ascB, nLeftEqPos, nRightEqPos, iprLeftPriority, nLitB);
                ipt.SavePti(ptiLeft);

                Ipr iprRightPriority = nComputePriority(aicB, nRightEqPos, nLeftEqPos);
                Pti ptiRight = Pti.ptiMakeEq(ascB, nRightEqPos, nLeftEqPos, iprRightPriority, nLitB);
                ipt.SavePti(ptiRight);
            }
        }

        public static Ipr nComputePriority(Aic aicB, int nFromTermPos, int nToTermPos)
        {
            return aicB.asc.iprComplexity();
#if false
            Asc ascB = aicB.asc;
            int nFromLen = aicB.rgnTermSize[nFromTermPos];
            int nToLen = aicB.rgnTermSize[nToTermPos];

            if (nFromLen == 1)
            {
                int nId = ascB.rgnTree[nFromTermPos];
                if (nId >= Asb.nVar)
                    return 3 * nToLen;
                else
                    return 2 * nToLen;
            }

            int nSum = nFromLen;    // variables in ascA can replicate this to elsewhere in the asc being substituted into
            bool fEliminated = false;
            for (int nPos = nFromTermPos; nPos < nFromTermPos + nFromLen; nPos++)
            {
                int nId = ascB.rgnTree[nPos];
                bool fAlready = false;
                for (int nPosPrev = nFromTermPos; nPosPrev < nPos; nPosPrev++)
                {
                    if (ascB.rgnTree[nPosPrev] == nId)
                    {
                        fAlready = true;
                        break;
                    }
                }
                if (!fAlready)
                {
                    if (nId >= Asb.nVar)
                    {
                        // count each unique variable
                        nSum += 1; // idea is that variable will be substituted with some value and used elsewhere in acsB
                    }
                    else
                    {
                        // look for symbols that are eliminated
                        bool fAlso = false;
                        for (int nPosTo = nToTermPos; nPosTo < nToTermPos + nToLen; nPosTo++)
                        {
                            if (ascB.rgnTree[nPosTo] == nId)
                            {
                                fAlso = true;
                                break;
                            }
                        }
                        if (!fAlso)
                            fEliminated = true;
                    }
                }
            }
            if (fEliminated)
                nSum += nToLen / 2;
            else
                nSum += nToLen;
            return nSum;
#endif
        }

        /// <summary>
        /// Find if symbol is used, Asc might not be in canonical form
        /// </summary>
        public bool fAppears(sbyte nId)
        {
            for (int nPos = Asc.nClauseLeadingSizeNumbers; nPos < rgnTree.Length; nPos++)
            {
                if (rgnTree[nPos] == nId)
                    return true;
            }
            return false;
        }

        public bool fEmpty()
        {
            return rgnTree[nPosnNumNegTerms] == 0 && rgnTree[nPosnNumPosTerms] == 0;
        }

        public bool fMatches(Asc ascOther, bool fExact)
        {
            if (rgnTree.Length != ascOther.rgnTree.Length)
                return false;

            sbyte nNegTerms = rgnTree[Asc.nPosnNumNegTerms];
            sbyte nPosTerms = rgnTree[Asc.nPosnNumPosTerms];
            if (nNegTerms != ascOther.rgnTree[Asc.nPosnNumNegTerms])
                return false;
            if (nPosTerms != ascOther.rgnTree[Asc.nPosnNumPosTerms])
                return false;

            if (fExact)
                return fEqualsRange(ascOther, nClauseLeadingSizeNumbers, rgnTree.Length, nClauseLeadingSizeNumbers);
            else
            {
                if (!fEqualLiterals(ascOther, nNegTerms, nPosTerms))
                    return false;
                return true;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
                return false;
            Asc ascOther = (Asc)obj;
            return fMatches(ascOther, false);
        }

        /// <summary>
        /// State for fEqualLiterals
        /// </summary>
        class Els
        {
            public ushort nCurrentTerm;
            public ushort nNextTerm;
            public ushort nOtherCurrentTerm;
            public int nOtherLit;
            public sbyte nNextVblId;
        }

        /// <summary>
        /// Check if a sequence of literals in possibly different orders are equal
        /// </summary>
        bool fEqualLiterals(Asc ascOther, int nNumNegLiterals, int nNumPosLiterals)
        {

            int nNumLiterals = nNumNegLiterals + nNumPosLiterals;
            if (nNumLiterals == 0)
                return true;
            Vam vamR = new Vam(ascOther);
            Els[] rgelsStack = new Els[nNumLiterals];
            ushort nNextTerm = nClauseLeadingSizeNumbers;
            ushort nOtherNextTerm = nClauseLeadingSizeNumbers;
            for (int nS = 0; nS < nNumLiterals; nS++)
            {
                Els els = new Els();
                rgelsStack[nS] = els;
                els.nOtherLit = nS < nNumNegLiterals ? 0 : nNumNegLiterals;
                els.nNextVblId = Atp.nVar;
                els.nCurrentTerm = nNextTerm;
                els.nOtherCurrentTerm = nOtherNextTerm;
                nNextTerm = (ushort)(nNextTerm + nTermSize(nNextTerm));
                nOtherNextTerm = (ushort)(nOtherNextTerm + ascOther.nTermSize(nOtherNextTerm));
                els.nNextTerm = nNextTerm;
            }
            int nLitNum = 0;
            while (true)
            {
                Els els = rgelsStack[nLitNum];
                int nOtherLitNum = els.nOtherLit;
                int nStartOther = nLitNum < nNumNegLiterals ? 0 : nNumNegLiterals;
                int nStopOther = nLitNum < nNumNegLiterals ? nNumNegLiterals : nNumLiterals;
                bool fFound = false;
                while (nOtherLitNum < nNumLiterals)
                {
                    bool fAlready = false;
                    for (int nPrevLit = nStartOther; nPrevLit < nLitNum; nPrevLit++)
                    {
                        if (rgelsStack[nPrevLit].nOtherLit == nOtherLitNum)
                        {
                            fAlready = true;
                            break;
                        }
                    }
                    if (!fAlready)
                    {
                        vamR.ClearVbls(els.nNextVblId);
                        if (fEqualsRange(ascOther,
                                         rgelsStack[nLitNum].nCurrentTerm,
                                         rgelsStack[nLitNum].nNextTerm,
                                         rgelsStack[nOtherLitNum].nOtherCurrentTerm,
                                         null, vamR))
                        {
                            fFound = true;
                            break;
                        }
                    }
                    nOtherLitNum++;
                }
                if (fFound)
                {
                    rgelsStack[nLitNum].nOtherLit = nOtherLitNum;
                    nLitNum++;
                    if (nLitNum < nNumLiterals)
                    {
                        rgelsStack[nLitNum].nNextVblId = vamR.nNextId;
                        continue; // to next nLitNum
                    }
                    else
                        return true;
                }
                // return to prev and try again
                if (nLitNum == 0)
                    return false;
                rgelsStack[nLitNum].nOtherLit = nStartOther;   // try again later
                nLitNum--;
                els = rgelsStack[nLitNum];
                els.nOtherLit = els.nOtherLit + 1;
            }
        }

        public override int GetHashCode()
        {
            int nAcc = rgnTree[Asc.nPosnNumNegTerms] << 8 + rgnTree[Asc.nPosnNumPosTerms];
            for (int i = nClauseLeadingSizeNumbers; i < rgnTree.Length; i++)
            {
                sbyte nId = rgnTree[i];
                if (nId >= nVar)
                {
                    nAcc = nAcc * 23 + nId;
                }
                else
                {
                    nAcc = nAcc * 23 + rglsmData[nLsmId - nId].nId;
                }
            }
            return nAcc;
        }
        public static LinkedList<Asc> rgascConvert(Lsx rglsxClause, int nNumNotNegated = Asc.nNotCounted)
        {
            LinkedList<Asc> rgasc = new LinkedList<Asc>();
            if (rglsxClause == Lsm.lsmNil)
            {
                return rgasc;
            }
            Lpr rglprClause = (Lpr)rglsxClause;

            while (true)
            {
                Asc ascNew = Asc.ascFromLsx(rglprClause.lsxCar);
                rgasc.AddLast(ascNew);
                if (nNumNotNegated-- > 0)
                    ascNew.gfbSource = Gfc.gfcAxiom;
                else
                    ascNew.gfbSource = Gfc.gfcNegHypothesis;
                if (rglprClause.lsxCdr == Lsm.lsmNil)
                    break;
                rglprClause = (Lpr)rglprClause.lsxCdr;
            }
            return rgasc;
        }

        public static Lsx lsxFromChain(Res res, LinkedList<Asc> rgasc)
        {
            Lpr lprHead = null;
            Lpr lprTail = null;
            foreach (Asc ascPlace in rgasc)
            {
                Lsx lsxTerm = ascPlace.lsxTo(res.asyDefault);
                Sko.AddToResult(lsxTerm, ref lprHead, ref lprTail);
            }
            return lprHead;
        }

        public override Lsx lsxTo(Asy asy)
        {
            int nOffset = 0;
            return lsxToDisj(ref nOffset, asy);
        }

        public static bool fMatchesSeq(LinkedList<Asc> rgascA, LinkedList<Asc> rgascB)
        {
            var enascA = rgascA.GetEnumerator();
            var enascB = rgascB.GetEnumerator();

            while (enascA.MoveNext())
            {
                Asc ascA = enascA.Current;
                if (!enascB.MoveNext())
                    return false;
                Asc ascB = enascB.Current;
                if (!ascA.Equals(ascB))
                    return false;
            }
            if (enascB.MoveNext())
                return false;
            return true;
        }

        public int nNumVars()
        {
            // note: assumes vblIds are consecutive
            int nMaxVar = Atp.nVar - 1;
            for (int i = nClauseLeadingSizeNumbers; i < rgnTree.Length; i++)
            {
                if (rgnTree[i] > nMaxVar)
                    nMaxVar = rgnTree[i];
            }
            return nMaxVar + 1;
        }

        /// <summary>
        /// Return posn where neg terms are before and pos terms are after
        /// </summary>
        /// <returns></returns>
        public int nPosnFirstPos()
        {
            int nTerms = rgnTree[nPosnNumNegTerms];
            int nPosn = nClauseLeadingSizeNumbers;
            while (nTerms-- > 0)
            {
                ushort nPending = 1;
                while (nPending-- > 0)
                {
                    sbyte nId = rgnTree[nPosn++];
                    if (nId <= Asb.nLsmId)
                    {
                        Lsm lsm = rglsmData[Asb.nLsmId - nId];
                        if (lsm.nArity > Lsm.nArityConst)
                            nPending += (ushort)(lsm.nArity - Lsm.nArityConst);
                    }
                }
            }
            return nPosn;
        }

        Pso fEvalSubterm(ref int nPos, List<Pso> rgpsoValues, Psm psm)
        {
            sbyte nSym = rgnTree[nPos++];
            if (nSym >= 0)
            {
                return rgpsoValues[nSym];
            }
            Lsm lsmFn = rglsmData[Asb.nLsmId - nSym];
            int nArity;
            if (Lsm.nArgsUndefined == lsmFn.nArity)
                nArity = 0;
            else
                nArity = lsmFn.nArity - Lsm.nArityConst;
            Psm.PsoFunction fpso = psm.psofunctionGet(lsmFn.stName, nArity);
            List<Pso> rgpsoArgs = new List<Pso>();
            for (int nArg = 0; nArg < nArity; nArg++)
            {
                rgpsoArgs.Add(fEvalSubterm(ref nPos, rgpsoValues, psm));
            }
            return fpso.Invoke(rgpsoArgs);
        }

        bool fEvalTerm(ref int nPos, List<Pso> rgpsoValues, Psm psm)
        {
            sbyte nSym = rgnTree[nPos++];
            Lsm lsmPred = rglsmData[Asb.nLsmId - nSym];
            Psm.FPred fpred = psm.fpredGet(lsmPred.stName);
            List<Pso> rgpsoArgs = new List<Pso>();
            int nArity = lsmPred.nArity - Lsm.nArityConst;
            for (int nArg = 0; nArg < nArity; nArg++)
            {
                rgpsoArgs.Add(fEvalSubterm(ref nPos, rgpsoValues, psm));
            }
            return fpred.Invoke(rgpsoArgs);
        }

        /// <summary>
        /// return the termNum that causes clause to hold in current valuation
        /// </summary>
        public sbyte nEval(List<Pso> rgpsoValues, Psm psm)
        {
            int nPos = Asb.nClauseLeadingSizeNumbers;
            sbyte nTermNum = 0;
            for (int iTerm = rgnTree[nPosnNumNegTerms]; iTerm > 0; iTerm--)
            {
                if (!fEvalTerm(ref nPos, rgpsoValues, psm))
                    return nTermNum;
                nTermNum++;
            }
            for (int iTerm = rgnTree[nPosnNumPosTerms]; iTerm > 0; iTerm--)
            {
                if (fEvalTerm(ref nPos, rgpsoValues, psm))
                    return nTermNum++;
                nTermNum++;
            }
            return Asc.nNoResolveTerm;
        }

        /// <summary>
        /// Check for reflexive terms: tautology or redundant.
        /// </summary>
        /// <returns>true of tautology</returns>
        public bool fSimplifyReflexivePredicates()
        {
            sbyte[] rgnTreeLocal = rgnTree;
            Lsm[] rglsmDataLocal = rglsmData;
            while (true)
            {
                Asc ascLocal = new Asc(rgnTreeLocal, rglsmDataLocal);
                Aic aic = new Aic(ascLocal);
                sbyte nNumNegTerms = rgnTreeLocal[nPosnNumNegTerms];
                sbyte nNumPosTerms = rgnTreeLocal[nPosnNumPosTerms];
                int nNumTerms = nNumNegTerms + nNumPosTerms;
                for (int nTermNum = 0; nTermNum < nNumTerms; nTermNum++)
                {
                    Lsm lsmPred = rglsmDataLocal[aic.rgnTermOffset[nTermNum]];
                    if (lsmPred.fPredicateAntireflexive || lsmPred.fPredicateReflexive)
                    {
                        ushort nTermOff = aic.rgnTermOffset[nTermNum];
                        int nLeftOff = nTermOff + 1;
                        byte nLeftSize = aic.rgnTermSize[nLeftOff];
                        int nRightOff = nLeftOff + nLeftSize;
                        byte nRightSize = aic.rgnTermSize[nRightOff];
                        if (nLeftSize != nRightSize)
                            continue;
                        bool fMatch = true;
                        for (int nPos = 0; nPos < nLeftSize; nPos++)
                        {
                            if (rgnTreeLocal[nLeftOff + nPos] != rgnTreeLocal[nRightOff + nPos])
                            {
                                fMatch = false;
                                break;
                            }
                        }
                        if (fMatch)
                        {
                            if (nTermNum < nNumNegTerms)
                            {
                                // remove this term, it is a negated tautology (could resume from here, but not worth it)
                                rgnTreeLocal = rgnRemoveTerm(rgnTreeLocal, false, nTermOff, aic.rgnTermSize[nTermOff]);
                                continue; // look for more
                            }
                            else
                                return true; // this term makes the clause a tautology
                        }


                    }
                }
                return false;
            }
        }

        static sbyte[] rgnRemoveTerm(sbyte[] rgnTree, bool fPos, ushort nTermOff, byte nTermSize)
        {
            sbyte[] rgnNewTree = new sbyte[rgnTree.Length - nTermSize];
            for (int nBefore = 0; nBefore < nTermOff; nBefore++)
                rgnNewTree[nBefore] = rgnTree[nBefore];
            int nBytesAfter = rgnTree.Length - nTermOff - nTermSize;
            for (int nAfter = nTermOff; nAfter < nTermOff; nAfter++)
                rgnNewTree[nAfter + nTermSize] = rgnTree[nAfter];

            if (fPos)
                rgnNewTree[nPosnNumPosTerms] = (sbyte)(rgnNewTree[nPosnNumPosTerms] - 1);
            else
                rgnNewTree[nPosnNumNegTerms] = (sbyte)(rgnNewTree[nPosnNumNegTerms] - 1);
            return rgnNewTree;
        }
    }
}
