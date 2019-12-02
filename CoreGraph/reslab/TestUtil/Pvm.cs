
using System;
using System.Collections.Generic;
using System.Text;

namespace reslab
{
    /// <summary>
    /// Base class for proof verifier step
    /// </summary>
    public abstract class Pvb : Bid
    {
        public Asc ascResult;
        public Mof mofOutput;
        public Mvf mvfOutput;
        public Pvb pvbPrevSameVbv;
        public Vpo vpoOrigin;

        public abstract bool fPerform(Mrs mrsToUpdate, Mvf mvf, Vbv vbvToUpdate);

        public abstract void ApplyRevisedIds(Mrs mrs);

        protected Pvb(Vpo vpoOrigin, Pvb pvbPrevSameVbv)
        {
            this.vpoOrigin = vpoOrigin;
            this.pvbPrevSameVbv = pvbPrevSameVbv;

#if DEBUG
            if (nId == -682931)
                Console.WriteLine("trap pvb");
#endif
        }

        public Pvb Add(Pvm pvm)
        {
            pvm.Add(this);
            return this;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(GetType().Name);
            sb.Append(" ");
            sb.Append(nId);
            if (vpoOrigin != null)
            {
                sb.Append(" ");
                sb.Append(vpoOrigin.stId());
            }
            if (ascResult != null)
            {
                sb.Append(" ");
                sb.Append(ascResult);
                sb.Append(" -| ");
            }
            sb.Append(" ");
            Format(sb);
            sb.Append("]");
            return sb.ToString();
        }

        protected abstract void Format(StringBuilder sb);

        static bool fMatchTerm(int nTermSize, sbyte[] rgnInTree, ushort nInPos,
                                sbyte[] rgnOldTree, ushort nPrevInPos)
        {
            for (int nPos = 0; nPos < nTermSize; nPos++)
            {
                if (rgnInTree[nInPos + nPos] != rgnOldTree[nPrevInPos + nPos])
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Return a the rgnTree for an Asc but without duplicate literals.
        /// rgnOldTree might not be trimmed, so don't use Length.
        /// </summary>
        protected static sbyte[] rgnRemoveDups(Lsm[] rglsmData, sbyte[] rgnOldTree, ushort nOldSize,
                    Mrs mrsToUpdate, Vbv vbvUpdated, ulong nNegLiteralsAdded)
        {
            sbyte nNumNegLiterals = rgnOldTree[Asc.nPosnNumNegTerms];
            sbyte nNumPosLiterals = rgnOldTree[Asc.nPosnNumPosTerms];
            ushort nOutPos = Asc.nClauseLeadingSizeNumbers;
            ushort nStartSide = Asc.nClauseLeadingSizeNumbers;
            sbyte[] nOutBuf = null;
            int nOutTerms = 0;

            bool fASide = (mrsToUpdate != null && vbvUpdated == mrsToUpdate.vbvLeft);
            bool fBSide = (mrsToUpdate != null && vbvUpdated == mrsToUpdate.vbvRight);
            ulong nMaskIn = (mrsToUpdate == null) ? 0 : 
                                (fASide ? mrsToUpdate.nLeftMask : mrsToUpdate.nRightMask);
            ulong nMaskOut = 0;
            ulong nMaskPosnIn = 1;
            ulong nMaskPosnOut = 1;

            bool fRight = true;
            do
            {
                fRight = !fRight;
                int nNumLiterals = fRight ? nNumPosLiterals : nNumNegLiterals;
                ushort nInPos = nStartSide;
                for (int nLitNum = 0; nLitNum < nNumLiterals; nLitNum++)
                {
                    int nCurTermSize = Asb.nTermSize(rgnOldTree, rglsmData, nInPos);
                    ushort nPrevInPos = Asc.nClauseLeadingSizeNumbers;
                    bool fMatchPrev = false;
                    // comparing to only the de-duped terms in nOutBuf
                    for (int nPrevLitNum = 0; nPrevLitNum < nOutTerms; nPrevLitNum++)
                    {
                        int nPrevTermSize = Asb.nTermSize(nOutBuf, rglsmData, nPrevInPos);
                        if (nCurTermSize == nPrevTermSize
                            && fMatchTerm (nCurTermSize, rgnOldTree, nInPos, nOutBuf, nPrevInPos))
                        {
                            if (fRight && nPrevLitNum < nNumNegLiterals)
                            {
#if false
                                throw new ArgumentException();  // tautology should have been caught
#else
                                // allow tautology for now. It happened for an intermediate step.
                                continue;
#endif
                            }
                            ulong nPrevLitBitPosn  = (ulong)(1 << nPrevLitNum);
                            if ((nMaskIn & nMaskPosnIn) != 0)
                                nMaskOut |= nPrevLitBitPosn;
                            fMatchPrev = true;
                            ulong nCurrLitBitPosn = (ulong)(1 << nLitNum);
                            if (!fRight && ((nNegLiteralsAdded & nCurrLitBitPosn) != 0))
                                mrsToUpdate.nAddedNegLiteralsLeft--;
                            break;
                        }
                        nPrevInPos += (ushort)nPrevTermSize;
                    }

                    if (!fMatchPrev)
                    {

                        if (nOutBuf == null)
                            nOutBuf = new sbyte[nOldSize];

                        // copy term to output buffer
                        for (int nPos = 0; nPos < nCurTermSize; nPos++)
                        {
                            int nCopyFromPos = nInPos + nPos;
                            nOutBuf[nOutPos] = rgnOldTree[nCopyFromPos];
#if false
                            if (nPrevInPos != nOutPos
                                && mrsToUpdate != null
                                // && mrsToUpdate.mobOutput != null
                                )
                                mrsToUpdate.mofOutput.UpdateChangedEntry(mrsToUpdate.mofOutput, nOutPos, (ushort)nCopyFromPos, vbvToUpdate, fVbvIsSoln);
#endif
                            nOutPos++;
                        }
                        if ((nMaskIn & nMaskPosnIn) != 0)
                            nMaskOut |= nMaskPosnOut;
                        nMaskPosnOut <<= 1;
                        nOutTerms++;

                    } // otherwise there is a match, so discard the term
                    else if (fRight)
                        nNumPosLiterals--;
                    else
                        nNumNegLiterals--;
                    nMaskPosnIn <<= 1;
                    nInPos += (ushort)nCurTermSize;
                }
                nStartSide = nInPos;
            }
            while (!fRight);

            if (fASide)
                mrsToUpdate.nLeftMask = (uint) nMaskOut;
            else if (fBSide)
                mrsToUpdate.nRightMask = (uint)nMaskOut;

            sbyte[] nTrimOut = new sbyte[nOutPos];
            nTrimOut[Asc.nPosnNumNegTerms] = nNumNegLiterals;
            nTrimOut[Asc.nPosnNumPosTerms] = nNumPosLiterals;
            for (int nPos = Asc.nClauseLeadingSizeNumbers; nPos < nOutPos; nPos++)
                nTrimOut[nPos] = nOutBuf[nPos];
            return nTrimOut;
        }

        public void NormalizeVblIds()
        {
            sbyte[] rgnNewIds = new sbyte[sbyte.MaxValue];
            sbyte nNextVblId = Atp.nVar;
            sbyte[] rgnTree = ascResult.rgnTree;
            for (ushort nPos = 0; nPos < rgnNewIds.Length; nPos++)
                rgnNewIds[nPos] = Tde.nNoVblId;

            for (ushort nPos = Asc.nClauseLeadingSizeNumbers; nPos < rgnTree.Length; nPos++)
            {
                sbyte nVal = rgnTree[nPos];
                if (nVal >= Atp.nVar)
                {
                    sbyte nNew = rgnNewIds[nVal];
                    if (nNew == Tde.nNoVblId)
                    {
                        nNew = nNextVblId++;
                        rgnNewIds[nVal] = nNew;
                    }
                    rgnTree[nPos] = nNew;
                }
            }
        }

    }

    /// <summary>
    /// Add axiom
    /// </summary>
    public class Pva : Pvb
    {
        public Pva (Asc asc, Vpo vpoOrigin) : base(vpoOrigin, null)
        {
            ascResult = asc;
        }

        public override void ApplyRevisedIds(Mrs mrs)
        {
        }

        public override bool fPerform(Mrs mrsToUpdate, Mvf mvf, Vbv vbvToUpdate)
        {
            return true;
        }

        protected override  void Format(StringBuilder sb)
        {
        }
    }

    public class Mvfi : Mtb<sbyte, Mvfi>
    {
    }

    public class Mvf
    {
        public Mvfi mvfiList;
        public sbyte nNextVblId = Atp.nVar;
        public HashSet<Pdd> rgpddVisited = new HashSet<Pdd>();

        public Mvf() { }

        public void Add(sbyte nLocalId, Vbv vbvForVbls, sbyte nNewId)
        {
            Mvfi mvfiNew = new Mvfi();
            mvfiNew.tSource = nLocalId;
            mvfiNew.vbvSource = vbvForVbls;
            mvfiNew.tOutput = nNewId;
            mvfiNew.mvbNext = mvfiList;
            mvfiList = mvfiNew;
        }


        public static void ReplaceVblIds(Mvf mvfOutput, sbyte[] rgnReplacementId, Vbv vbvSource)
        {
            Mvfi mvfPlace = mvfOutput.mvfiList;
            while (mvfPlace != null)
            {
                if (mvfPlace.vbvSource == vbvSource)
                {
                    sbyte nOldId = mvfPlace.tSource;
                    sbyte nNewId = rgnReplacementId[nOldId];
                    mvfPlace.tSource = nNewId;
                    mvfPlace.Validate();
                }
                mvfPlace = mvfPlace.mvbNext;
            }
        }

    }

    /// <summary>
    /// A placeholder step that holds the array to map the variable ids in the previous step to a set for the
    /// next step so they that do not collide with variable ids from other inputs to the step.
    /// e.g.   (   ( () (A2 @0 @1))  ( ( (A2 @0 (B2 @1 @0)) )  ) )
    /// 
    /// First phase is assign unique ids. More vbls will be added from Pvp, so they need to be resequenced
    /// later: ApplyRevisedIds
    /// </summary>
    public class Pvi : Pvb
    {
        public sbyte[] rgnSharedId;  // indexed by nVblId in this asc, value is unique nVblId across Pvc
        public readonly Pvb pvbPrevStep;

        public Pvi(Pvb pvbPrevStep, Mvf mvf, Vbv vbvForVbls, Vpo ipoOrigin)
                     : base(ipoOrigin, null)
        {
            this.pvbPrevStep = pvbPrevStep;

            // Assign next block of ids that are unique across the mvf
            Asc ascPrev = pvbPrevStep.ascResult;
            rgnSharedId = new sbyte[ascPrev.nMaxVarId(Asc.nClauseLeadingSizeNumbers) + 1];
            for (sbyte nLocalId = 0; nLocalId < rgnSharedId.Length; nLocalId++)
            {
                sbyte nNewId = mvf.nNextVblId++;
                rgnSharedId[nLocalId] = nNewId;
                mvf.Add(nLocalId, vbvForVbls, nNewId);
            }
            mofOutput = new Mof(ascPrev, Vie.nIdxA);
            mvfOutput = mvf;
        }

        public override void ApplyRevisedIds(Mrs mrs)
        {
            // assign new shared ids. mrs.nReplacementId will have the vblIds that appear in the result already
            // so they can be sequential.
            for (int nId = 0; nId < rgnSharedId.Length; nId++)
            {
                sbyte nLocalReplId = mrs.nReplacementId(rgnSharedId[nId]);
                rgnSharedId[nId] = nLocalReplId;
            }
        }

        public override bool fPerform(Mrs mrsToUpdate, Mvf mvf, Vbv vbvToUpdate)
        {
            Asc ascPrev = pvbPrevStep.ascResult;
            sbyte[] rgnPrev = ascPrev.rgnTree;
            sbyte[] rgnResult = new sbyte[rgnPrev.Length];
            rgnResult[Asc.nPosnNumNegTerms] = rgnPrev[Asc.nPosnNumNegTerms];
            rgnResult[Asc.nPosnNumPosTerms] = rgnPrev[Asc.nPosnNumPosTerms];
            for (int nPos = Asc.nClauseLeadingSizeNumbers; nPos < rgnResult.Length; nPos++)
            {
                sbyte nId = rgnPrev[nPos];
                if (nId >= Atp.nVar)
                    rgnResult[nPos] = rgnSharedId[rgnPrev[nPos]];
                else
                    rgnResult[nPos] = rgnPrev[nPos];
            }
            ascResult = new Asc(rgnResult, ascPrev.rglsmData);
            return true;
        }

        protected override void Format(StringBuilder sb)
        {
            bool fFirst = true;
            if (pvbPrevStep != null)
            {
                sb.Append(" pvbPrev:");
                sb.Append(pvbPrevStep.nId);
            }
            if (rgnSharedId != null)
            {
                sb.Append(" [");
                foreach (sbyte nNewId in rgnSharedId)
                {
                    if (fFirst)
                        fFirst = false;
                    else
                        sb.Append(", ");
                    sb.Append(nNewId);
                }
                sb.Append(" ]");
            }
        }
    }

    /// <summary>
    /// Perform substitution - does not pick up side terms
    /// </summary>
    public class Pvs : Pvb
    {
        public Pvb pvbInto;
        public Pvb pvbSource;
        public sbyte nReplaceVbl;
        public ushort nReplaceFrom;

        public Pvs (Pvb pvbInto, Pvb pvbSource, sbyte nReplaceVbl, ushort nReplaceFrom, 
                    Vpo ascInferred, Pvb pvbPrevSameVbv)
            : base (ascInferred, pvbPrevSameVbv)
        {
            this.pvbInto = pvbInto;
            this.pvbSource = pvbSource;
            this.nReplaceVbl = nReplaceVbl;
            this.nReplaceFrom = nReplaceFrom;
        }

        public override void ApplyRevisedIds(Mrs mrs)
        {
            nReplaceVbl = mrs.nReplacementId(nReplaceVbl);
        }

        protected override void Format(StringBuilder sb)
        {
            if (pvbInto != null)
            {
                sb.Append(" pvbInto:");
                sb.Append(pvbInto.nId);
            }
            if (pvbSource != null)
            {
                sb.Append(" pvbSource:");
                sb.Append(pvbSource.nId);
            }
            sb.Append(" ");
            sb.Append(nReplaceVbl);
            sb.Append(" ");
            sb.Append(nReplaceFrom);
        }

        public override bool fPerform(Mrs mrsToUpdate, Mvf mvf, Vbv vbvToUpdate)
        {
            sbyte[] rgnBuffer = new sbyte[Ascb.nInitialBufferSize];

            Asc ascInto = pvbInto.ascResult;
            Asc ascReplace = pvbSource.ascResult;
            sbyte[] rgnInto = ascInto.rgnTree;
            sbyte[] rgnReplace = ascReplace.rgnTree;
            rgnBuffer[Asc.nPosnNumNegTerms] = rgnInto[Asc.nPosnNumNegTerms];
            rgnBuffer[Asc.nPosnNumPosTerms] = rgnInto[Asc.nPosnNumPosTerms];
            ushort nInPos = Asc.nClauseLeadingSizeNumbers;
            ushort nOutPosn = Asc.nClauseLeadingSizeNumbers;
            bool fReplacing = false;
            ushort nRepEnd = 0;
            ushort nRepPos = 0;
            Asy asyNew = new Asy();

            Mof mofInto = null;
            ushort[] rgnMofUpdated = null;
            if (mrsToUpdate != null)
            {
                mofInto = pvbInto.mofOutput;
                rgnMofUpdated = new ushort [mofInto.rgnSourceOffsetMap.Length];
            }

            while (true)
            {
                sbyte nNewId;
                if (fReplacing)
                {
                    if (nRepPos >= nRepEnd)
                    {
                        fReplacing = false;
                        continue;
                    }
                    sbyte nReplaceId = rgnReplace[nRepPos++];
                    if (nReplaceId >= Atp.nVar)
                    {
                        nNewId = nReplaceId; //  nMapToLocalId(nReplaceId, Vbv.vbvB, rgnReplaceToNewVblId, rgnMapAtpToLocalId, mrsToUpdate.mvbOutput, ref nNextVblId);
                    }
                    else
                    {
                        Lsm lsmReplace = ascReplace.rglsmData[Asb.nLsmId - nReplaceId];
                        nNewId = (sbyte)asyNew.nIdForLsm(lsmReplace);
                    }
                }
                else
                {
                    if (nInPos >= rgnInto.Length)
                        break;
                    ushort nInPosPrev = nInPos;
                    ushort nOriginalPos = 0;

                    sbyte nIntoId = rgnInto[nInPos++];
                    if (mrsToUpdate != null)
                        mofInto.MapBack(nInPosPrev, Vbv.vbvA, out nOriginalPos);

                    if (nIntoId == nReplaceVbl)
                    {
                        nRepPos = nReplaceFrom;
                        nRepEnd = (ushort)(nRepPos + ascReplace.nTermSize(nReplaceFrom));
                        fReplacing = true;
                        nNewId = 0; // stop compiler msg
                    }
                    else
                    {
                        if (nIntoId >= Atp.nVar)
                        {
                            nNewId = nIntoId; // nMapToLocalId(nIntoId, Vbv.vbvA, rgnIntoToNewVblId, rgnMapAtpToLocalId, mrsToUpdate.mvbOutput, ref nNextVblId);
                        }
                        else
                        {
                            Lsm lsmInto = ascInto.rglsmData[Asb.nLsmId - nIntoId];
                            nNewId = (sbyte)asyNew.nIdForLsm(lsmInto);
                        }
                    }
                    if (mrsToUpdate != null)
                    {
                        // Soln references locations in original Asc, map back to that to find which entry to
                        // update with new location.
                        if (nOriginalPos != Mob.nNoOffset)
                            rgnMofUpdated[nOriginalPos] = nOutPosn;
                    }
                    if (fReplacing)
                        continue;
                }
                rgnBuffer[nOutPosn++] = nNewId;
            }

            Lsm[] rglsmDataNew = asyNew.rglsmData();
            ascResult = new Asc( rgnRemoveDups(rglsmDataNew, rgnBuffer, nOutPosn,
                                                  mrsToUpdate, vbvToUpdate, 0),
                                 rglsmDataNew);
            mofOutput = new Mof(rgnMofUpdated,
                                pvbSource.mofOutput.vieList);  // for now
            mvfOutput = pvbInto.mvfOutput;  // is same for all in same Pvc
            return true;
        }
    }

    /// <summary>
    /// Cut: Combine two clauses, removing the matching terms from opposite sides.
    /// </summary>
    public class Pvc : Pvb
    {
        public Pvb pvbA;
        public Pvb pvbB;
        public uint nMaskA;  // bit==1 if literal should match/cancel K. Low order bit for leftmost literal
        public uint nMaskB; // bit==1 if literal should match K

        public Pvc (Mrs mrs, Asc ascInferred, Vpo ipoOrigin)
            : base(ipoOrigin, null)
        {
            this.pvbA = mrs.mesLeft.pvbLatest;
            this.pvbB = mrs.mesRight.pvbLatest;
            // shift whichever side has the unified literals that are positive 
            this.nMaskA = mrs.nLeftMask << mrs.nAddedNegLiteralsLeft;
            this.nMaskB = mrs.nRightMask; // << mrs.nAddedNegLiteralsRight;
        }

        public override void ApplyRevisedIds(Mrs mrs)
        {
        }

        protected override void Format(StringBuilder sb)
        {
            if (pvbA != null)
            {
                sb.Append(" pvbA:");
                sb.Append(pvbA.nId);
            }
            if (pvbB != null)
            {
                sb.Append(" pvbB:");
                sb.Append(pvbB.nId);
            }
            sb.Append(" ");
            sb.Append(nMaskA);
            sb.Append(" ");
            sb.Append(nMaskB);
        }

        /// <summary>
        /// Sequence of states: do K compares first so that variables that are equated are found
        /// </summary>
        enum KState
        {
            kSkipLeftNeg,
            kFindFirstLeftK,  // skip to first K in pos side of A
            kGetLeftK,          // get a K from A
            kFindNextLeftK,   // skip to next K in A
            kCompareLeftK,   // compare a subsequent K in A to the first one
            kFindRightK,         // skip to first L in neg side of B
            kCompareRightK,       // compare K on B
            kCopyANeg,
            kCopyBNeg,
            kCopyAPos,
            kCopyBPos,
            kLast
        }

        static bool[] rgfDoingB;
        static bool[] rgfDoingPos;

        static Pvc()
        {
            rgfDoingB = new bool[(int)KState.kLast];
            rgfDoingPos = new bool[(int)KState.kLast];
            for (int nState = 0; nState < (int)KState.kLast; nState++)
            {
                switch ((KState)nState)
                {
                    case KState.kSkipLeftNeg:
                        rgfDoingB[nState] = false;
                        rgfDoingPos[nState] = false;
                        break;
                    case KState.kFindFirstLeftK:
                        rgfDoingB[nState] = false;
                        rgfDoingPos[nState] = true;
                        break;
                    case KState.kGetLeftK:
                        rgfDoingB[nState] = false;
                        rgfDoingPos[nState] = true;
                        break;
                    case KState.kFindNextLeftK:
                        rgfDoingB[nState] = false;
                        rgfDoingPos[nState] = true;
                        break;
                    case KState.kCompareLeftK:
                        rgfDoingB[nState] = false;
                        rgfDoingPos[nState] = true;
                        break;
                    case KState.kFindRightK:
                        rgfDoingB[nState] = true;
                        rgfDoingPos[nState] = false;
                        break;
                    case KState.kCompareRightK:
                        rgfDoingB[nState] = true;
                        rgfDoingPos[nState] = false;
                        break;
                    case KState.kCopyANeg:
                        rgfDoingB[nState] = false;
                        rgfDoingPos[nState] = false;
                        break;
                    case KState.kCopyBNeg:
                        rgfDoingB[nState] = true;
                        rgfDoingPos[nState] = false;
                        break;
                    case KState.kCopyAPos:
                        rgfDoingB[nState] = false;
                        rgfDoingPos[nState] = true;
                        break;
                    case KState.kCopyBPos:
                        rgfDoingB[nState] = true;
                        rgfDoingPos[nState] = true;
                        break;
                }
            }
        }

        enum KDisp
        {
            kSave,
            kCompare,
            kOutput,
            kSkip
        }


        public override bool fPerform(Mrs mrsToUpdate, Mvf mvf, Vbv vbvToUpdate)
        {
            sbyte[] rgnBuffer = new sbyte[Ascb.nInitialBufferSize];
            sbyte[] rgnK = new sbyte[Ascb.nInitialBufferSize];
            sbyte[] rgnNewVblIdA = new sbyte[sbyte.MaxValue];
            sbyte[] rgnNewVblIdB = new sbyte[sbyte.MaxValue];
            sbyte[] rgnNewVblIdBK = new sbyte[sbyte.MaxValue];
            sbyte[] rgnK2A = new sbyte[sbyte.MaxValue];
            sbyte[] rgnK2B = new sbyte[sbyte.MaxValue];
            sbyte[] rgnB2A = new sbyte[sbyte.MaxValue];
            sbyte[] rgnA2B = new sbyte[sbyte.MaxValue];
            sbyte[] rgnNewVblIdCurrent = null;

            Asc ascA = pvbA.ascResult;
            Asc ascB = pvbB.ascResult;
            sbyte[] rgnCurrent = null;
            ushort nInPosA = Asc.nClauseLeadingSizeNumbers;
            ushort nInPosB = Asc.nClauseLeadingSizeNumbers;
            ushort nKPosn = 0;
            ushort nOutPosn = Asc.nClauseLeadingSizeNumbers;
            ushort nInPosCurrent = Asc.nClauseLeadingSizeNumbers;
            ushort nIdsLeftInLiteral = 0;
            sbyte nLiteralsThisSide = 0;
            uint nMaskCurrent = 0;
            uint nBitPosn = 0;
            Asc ascCurrent = null;
  
            KState kState = KState.kSkipLeftNeg;
            KDisp kDisp = KDisp.kSkip;

            sbyte nNumPosLiterals = 0;
            sbyte nNumNegLiterals = 0;


            bool fDoingB = false;
            bool fWasDoingB = true;
            bool fWasDoingPos = true;
            sbyte nNextVblIdOut = Atp.nVar;
            sbyte nNextVblIdBK = Atp.nVar;
            Asy asyNew = new Asy();

            for (int nInit = 0; nInit < rgnNewVblIdB.Length; nInit++)
            {
                rgnNewVblIdA[nInit] = Tde.nNoVblId;
                rgnNewVblIdB[nInit] = Tde.nNoVblId;
                rgnNewVblIdBK[nInit] = Tde.nNoVblId;
                rgnK2A[nInit] = Tde.nNoVblId;
                rgnK2B[nInit] = Tde.nNoVblId;
                rgnB2A[nInit] = Tde.nNoVblId;
                rgnA2B[nInit] = Tde.nNoVblId;
            }

            while (true)
            {
                if (nIdsLeftInLiteral == 0)
                {
                    fDoingB = rgfDoingB[(int)kState];
                    bool fDoingPos = rgfDoingPos[(int)kState];
                    bool fSwitchedB = fWasDoingB != fDoingB;
                    if (fSwitchedB)
                    {
                        fWasDoingB = fDoingB;

                        // switching from one side to other
                        if (fDoingB)
                        {
                            nInPosA = nInPosCurrent;
                            rgnNewVblIdCurrent = rgnNewVblIdB;
                            ascCurrent = ascB;
                            nInPosCurrent = nInPosB;
                            nMaskCurrent = nMaskB;
                        }
                        else
                        {
                            nInPosB = nInPosCurrent;
                            rgnNewVblIdCurrent = rgnNewVblIdA;
                            ascCurrent = ascA;
                            nInPosCurrent = nInPosA;
                            nMaskCurrent = nMaskA;
                        }
                        rgnCurrent = ascCurrent.rgnTree;
                    }
                    if (fSwitchedB || fWasDoingPos != fDoingPos)
                    {
                        if (fDoingPos)
                        {
                            nLiteralsThisSide = rgnCurrent[Asc.nPosnNumPosTerms];
                            nBitPosn = (uint) (1 << rgnCurrent[Asc.nPosnNumNegTerms]);
                        }
                        else
                        {
                            nLiteralsThisSide = rgnCurrent[Asc.nPosnNumNegTerms];
                            nBitPosn = 1;
                        }
                        fWasDoingPos = fDoingPos;
                        if (kState == KState.kCopyANeg)
                        {
                            for (int nInit = 0; nInit < nNextVblIdBK; nInit++)
                            {
                                rgnB2A[rgnK2B[nInit]] = rgnK2A[nInit];
                                rgnA2B[rgnK2A[nInit]] = rgnK2B[nInit];
                            }
                        }
                    }

                    // first time, or end of previous term, advance to next one

                    if (nLiteralsThisSide > 0)
                    {
                        bool fMatchMask = (nMaskCurrent & nBitPosn) != 0; // next term
                        if (fMatchMask && nLiteralsThisSide == 0)
                            throw new ArgumentException();

                        nLiteralsThisSide--;
                        nBitPosn <<= 1;
                        nIdsLeftInLiteral = ascCurrent.nTermSize(nInPosCurrent);
                        // switch state
                        switch (kState)
                        {
                            case KState.kSkipLeftNeg:
                                break;
                            case KState.kFindFirstLeftK:
                                if (fMatchMask)
                                    kState = KState.kGetLeftK;
                                break;
                            case KState.kFindNextLeftK:
                                if (fMatchMask)
                                    kState = KState.kCompareLeftK;
                                break;
                            case KState.kGetLeftK:
                                if (!fMatchMask)
                                {
                                    if (nMaskCurrent < nBitPosn)
                                    {
                                        kState = KState.kFindRightK;
                                        continue;
                                    }
                                    else
                                        kState = KState.kFindNextLeftK;
                                }
                                break;
                            case KState.kCompareLeftK:
                                if (nMaskCurrent < nBitPosn)   // done with left
                                {
                                    kState = KState.kCompareRightK;   // assume P1
                                    continue;
                                }
                                else if (!fMatchMask)
                                    kState = KState.kFindNextLeftK;
                                break;
                            case KState.kFindRightK:
                            case KState.kCompareRightK:
                                if (fMatchMask)
                                    kState = KState.kCompareRightK;
                                else if (nMaskCurrent < nBitPosn)   // done with right
                                {
                                    kState = KState.kCopyANeg;
                                    nInPosA = Asc.nClauseLeadingSizeNumbers;
                                    nIdsLeftInLiteral = 0;
                                    if (nLiteralsThisSide == 0)
                                        continue; // otherwise fall through to set kDisp
                                }
                                else
                                    kState = KState.kFindRightK;
                                break;
                            case KState.kCopyANeg:
                            case KState.kCopyBNeg:
                            case KState.kCopyAPos:
                            case KState.kCopyBPos:
                                break;
                            default:
                                throw new ArgumentException();
                        }

                        // prepare for new state
                        switch (kState)
                        {
                            case KState.kSkipLeftNeg:
                            case KState.kFindFirstLeftK:
                            case KState.kFindNextLeftK:
                            case KState.kFindRightK:
                                kDisp = KDisp.kSkip;
                                break;
                            case KState.kGetLeftK:
                            case KState.kCompareLeftK:
                            case KState.kCompareRightK:
                                rgnNewVblIdCurrent = rgnNewVblIdBK;
                                nKPosn = 0;
                                if (kState == KState.kCompareLeftK || kState == KState.kCompareRightK)
                                {
                                    kDisp = KDisp.kCompare;
                                    nNextVblIdBK = Atp.nVar;
                                    for (int nInit = 0; nInit < rgnNewVblIdB.Length; nInit++)
                                        rgnNewVblIdBK[nInit] = Tde.nNoVblId;
                                }
                                else if (kState == KState.kGetLeftK)
                                    kDisp = KDisp.kSave;
                                else
                                    kDisp = KDisp.kSkip;
                                break;
                            case KState.kCopyANeg:
                            case KState.kCopyBNeg:
                                if (fMatchMask)
                                    kDisp = KDisp.kSkip;
                                else
                                {
                                    if (nIdsLeftInLiteral > 0)
                                        nNumNegLiterals++;
                                    kDisp = KDisp.kOutput;
                                }
                                break;
                            case KState.kCopyAPos:
                            case KState.kCopyBPos:
                                if (fMatchMask)
                                    kDisp = KDisp.kSkip;
                                else
                                {
                                    nNumPosLiterals++;
                                    kDisp = KDisp.kOutput;
                                }
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        if (kDisp == KDisp.kSkip)
                        {
                            nInPosCurrent += nIdsLeftInLiteral;
                            nIdsLeftInLiteral = 0;
                            continue;
                        }
                    }
                    else
                    {
                        switch (kState)
                        {
                            case KState.kSkipLeftNeg:
                                kState = KState.kFindFirstLeftK;
                                break;
                            case KState.kFindFirstLeftK:
                                throw new ArgumentException();
                            case KState.kFindNextLeftK:
                                break;
                            case KState.kFindRightK:
                                kState = KState.kCompareRightK;   // assume P1
                                break;
                            case KState.kGetLeftK:
                                kState = KState.kFindRightK;
                                break;
                            case KState.kCompareLeftK:
                                kState = KState.kCompareRightK;   // assume P1
                                break;
                            case KState.kCompareRightK:
                                kState = KState.kCopyANeg;
                                nInPosA = Asc.nClauseLeadingSizeNumbers;
                                break;
                            case KState.kCopyANeg:
                                kState = KState.kCopyBNeg;
                                nInPosB = Asc.nClauseLeadingSizeNumbers;
                                break;
                            case KState.kCopyBNeg:
                                kState = KState.kCopyAPos;
                                break;
                            case KState.kCopyAPos:
                                kState = KState.kCopyBPos;
                                break;
                            case KState.kCopyBPos:
                                rgnBuffer[Asc.nPosnNumNegTerms] = nNumNegLiterals;
                                rgnBuffer[Asc.nPosnNumPosTerms] = nNumPosLiterals;
                                Lsm[] rglsmDataNew = asyNew.rglsmData();
                                ascResult = new Asc( rgnRemoveDups(rglsmDataNew, rgnBuffer, nOutPosn,
                                                                    mrsToUpdate, vbvToUpdate, 0),
                                                      rglsmDataNew);

                                return true;
                            default:
                                throw new ArgumentException();
                        }
                    }
                    continue;
                }

                // Handle one symbol from input
                nIdsLeftInLiteral--;
                sbyte nNewId;
                sbyte nInId = rgnCurrent[nInPosCurrent++];
                if (nInId >= Atp.nVar)
                {
                    nNewId = rgnNewVblIdCurrent[nInId];
                    if (nNewId == Tde.nNoVblId)
                    {
                        if (kDisp == KDisp.kSave || kDisp == KDisp.kCompare)
                        {
                            nNewId = nNextVblIdBK++;
                            if (kDisp == KDisp.kSave)
                                rgnK2A[nNewId] = nInId;
                            else
                                rgnK2B[nNewId] = nInId;
                        }
                        else
                        {
                            if (fDoingB)
                            {
                                if (rgnB2A[nInId] != Tde.nNoVblId)
                                    nNewId = rgnNewVblIdA[rgnB2A[nInId]];
                            }
                            else
                            {
                                if (rgnA2B[nInId] != Tde.nNoVblId)
                                    nNewId = rgnNewVblIdB[rgnA2B[nInId]];
                            }
                            if (nNewId == Tde.nNoVblId)
                                nNewId = nNextVblIdOut++;
                        }
                        rgnNewVblIdCurrent[nInId] = nNewId;
                    }
                }
                else
                {
                    Lsm lsmInto = ascCurrent.rglsmData[Asb.nLsmId - nInId];
                    nNewId = (sbyte)asyNew.nIdForLsm(lsmInto);
                }

                switch (kDisp)
                {
                    case KDisp.kSave:
                        rgnK[nKPosn++] = nNewId;
                        break;
                    case KDisp.kCompare:
                        if (rgnK[nKPosn++] != nNewId)
                            throw new ArgumentException();
                        break;
                    case KDisp.kOutput:
                        rgnBuffer[nOutPosn++] = nNewId;
                        break;
                    case KDisp.kSkip:
                        break;
                }

            }

        }
    }

    /// <summary>
    /// Base class for Pvs and Pvp, to share code to perform replacement
    /// </summary>
    public abstract class Pvr : Pvb
    {
        public Pvb pvbInto;
        public Pvb pvbSource;

        // The following should be moved to a separate class if the instances are reused:
        protected ushort nInPosCurrent;
        protected KState kState;
        protected KState kStateSave;
        protected ushort nIdsLeftInLiteral = 0;
        protected Mof mofInto = null;
        protected Asc ascCurrent;
        protected int nIdsLeftInTerm;


        protected enum KState
        {
            kGetting,
            kComparing,
            kReplacing,
            kCopyStart,
            kCopyANeg,
            kCopyBNeg,
            kCopyAPos,
            kCopyBPos
        }

        public Pvr(Pvb pvbInto, Pvb pvbSource, Vpo ipoOrigin, Pvb pvbPrevSameVbv)
            : base (ipoOrigin, pvbPrevSameVbv)
        {
            this.pvbInto = pvbInto;
            this.pvbSource = pvbSource;
        }

        protected override void Format(StringBuilder sb)
        {
            if (pvbInto != null)
            {
                sb.Append(" into:");
                sb.Append(pvbInto.nId);
            }
            if (pvbSource != null)
            {
                sb.Append(" source:");
                sb.Append(pvbSource.nId);
            }
        }

        protected abstract void InitPerform();

        protected abstract bool fStartReplace(out ushort nNewPosn);


        protected virtual void StartComparing()
        {
            throw new ArgumentException();
        }

        protected virtual bool fSkipTerm()
        {
            return false;
        }
        protected abstract void UpdateIfReplaced(ushort nInPosCurrent, out ushort nOriginalPos);


        public override bool fPerform(Mrs mrsToUpdate, Mvf mvf, Vbv vbvToUpdate)
        {
            sbyte[] rgnBuffer = new sbyte[Ascb.nInitialBufferSize];
            sbyte[] rgnK = new sbyte[Ascb.nInitialBufferSize];
            sbyte[] rgnNewVblIdA = null; // was new sbyte[pvbInto.ascResult.nNumVars()];
            sbyte[] rgnNewVblIdB = new sbyte[pvbSource.ascResult.nNumVars()];

            Asc ascA = pvbInto.ascResult;
            Asc ascB = pvbSource.ascResult;
            ushort nInPosA = Asc.nClauseLeadingSizeNumbers;
            ushort nInPosB = Asc.nClauseLeadingSizeNumbers;
            ushort nOutPosn = Asc.nClauseLeadingSizeNumbers;
            sbyte nLiteralsThisSide = 0;
            sbyte nNumPosLiterals = 0;
            sbyte nNumNegLiterals = 0;
            ulong nNegLiteralsAdded = 0;  // low bit is leftmost term, set if incrementing nAddedNegLiteralsLeft


            Asy asyNew = new Asy();

            ushort[] rgnMofUpdated = null;
            if (mrsToUpdate != null)
            {
                mofInto = pvbInto.mofOutput;
                rgnMofUpdated = new ushort[mofInto.rgnSourceOffsetMap.Length];
            }

            for (int nInit = 0; nInit < rgnNewVblIdB.Length; nInit++)
                rgnNewVblIdB[nInit] = Tde.nNoVblId;

            // find vbl ids used in replacement, and make sure they stay the same
            ushort nReplacePosn;
            fStartReplace(out nReplacePosn);
            ushort nReplacementSize = ascB.nTermSize(nReplacePosn);
            for (int nInit = nReplacePosn; nInit < nReplacePosn + nReplacementSize; nInit++)
            {
                sbyte nBId = ascB.rgnTree[nInit];
                if (nBId >= Atp.nVar)
                    rgnNewVblIdB[nBId] = nBId;
            }

            ushort nKPosn = 0;
            ascCurrent = ascA;
            InitPerform();
            sbyte[] rgnNewVblIdCurrent = rgnNewVblIdA;
            sbyte[] rgnCurrent = ascCurrent.rgnTree;
            bool fMain = true;

            while (fMain)
            {
                if (nIdsLeftInTerm > 0)
                {
                    nIdsLeftInTerm--;
                    if (nIdsLeftInTerm == 0)
                    {
                        switch (kState)
                        {
                            case KState.kGetting:
                                kState = KState.kComparing;
                                nInPosA = nInPosCurrent;
                                ascCurrent = ascB;
                                StartComparing();
                                rgnNewVblIdCurrent = rgnNewVblIdB;
                                nIdsLeftInTerm = 1 + ascCurrent.nTermSize(nInPosCurrent);
                                if (nIdsLeftInTerm != nKPosn + 1)
                                    throw new ArgumentException();
                                nKPosn = 0;
                                break;
                            case KState.kComparing:
                                kState = KState.kCopyStart;
                                break;
                            case KState.kReplacing:
                                kState = kStateSave;
                                if (kState == KState.kCopyANeg || kState == KState.kCopyAPos)
                                {
                                    ascCurrent = ascA;
                                    rgnNewVblIdCurrent = rgnNewVblIdA;
                                    nInPosCurrent = nInPosA;
                                }
                                else
                                {
                                    ascCurrent = ascB;
                                    rgnNewVblIdCurrent = rgnNewVblIdB;
                                    nInPosCurrent = nInPosB;
                                }
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        rgnCurrent = ascCurrent.rgnTree;
                        continue;
                    }
                }
                else if (nIdsLeftInLiteral == 0)
                {
                    if (nLiteralsThisSide > 0)
                    {
                        nLiteralsThisSide--;
                        nIdsLeftInLiteral = ascCurrent.nTermSize(nInPosCurrent);
                        if (kState == KState.kCopyBNeg || kState == KState.kCopyBPos)
                        {
                            if (fSkipTerm())
                                continue;
                            if (mrsToUpdate != null && kState == KState.kCopyBNeg)
                            {
                                if (pvbInto == mrsToUpdate.mesLeft.pvbLatest)
                                {
                                    mrsToUpdate.nAddedNegLiteralsLeft++;
                                    nNegLiteralsAdded |= (ulong) (1L << nNumNegLiterals);
                                }
                                else if (pvbInto == mrsToUpdate.mesRight.pvbLatest)
                                    mrsToUpdate.nAddedNegLiteralsRight++;
                                else
                                { } //    throw new ArgumentException();
                            }
                        }
                        if (kState == KState.kCopyAPos || kState == KState.kCopyBPos)
                            nNumPosLiterals++;
                        else
                            nNumNegLiterals++;
                        rgnNewVblIdCurrent = (kState == KState.kCopyBNeg || kState == KState.kCopyBPos)
                                            ? rgnNewVblIdB : rgnNewVblIdA;
                        continue;
                    }

                    switch (kState)
                    {
                        case KState.kCopyStart:
                            kState = KState.kCopyANeg;
                            ascCurrent = ascA;
                            rgnNewVblIdCurrent = rgnNewVblIdA;
                            nInPosCurrent = Asc.nClauseLeadingSizeNumbers;
                            break;
                        case KState.kCopyANeg:
                            kState = KState.kCopyBNeg;
                            nInPosA = nInPosCurrent;
                            ascCurrent = ascB;
                            rgnNewVblIdCurrent = rgnNewVblIdB;
                            nInPosCurrent = Asc.nClauseLeadingSizeNumbers;
                            break;
                        case KState.kCopyBNeg:
                            kState = KState.kCopyAPos;
                            nInPosB = nInPosCurrent;
                            ascCurrent = ascA;
                            nInPosCurrent = nInPosA;
                            rgnNewVblIdCurrent = rgnNewVblIdA;
                            break;
                        case KState.kCopyAPos:
                            kState = KState.kCopyBPos;
                            ascCurrent = ascB;
                            nInPosCurrent = nInPosB;
                            rgnNewVblIdCurrent = rgnNewVblIdB;
                            break;
                        case KState.kCopyBPos:
                            fMain = false;
                            continue;
                        default:
                            throw new ArgumentException();
                    }

                    rgnCurrent = ascCurrent.rgnTree;
                    nLiteralsThisSide = (kState == KState.kCopyAPos || kState == KState.kCopyBPos)
                                    ? rgnCurrent[Asc.nPosnNumPosTerms] : rgnCurrent[Asc.nPosnNumNegTerms];
                    continue;
                }
                else
                    nIdsLeftInLiteral--;

                if (kState == KState.kCopyANeg || kState == KState.kCopyAPos)
                {
                    ushort nNewPosn;
                    if (fStartReplace(out nNewPosn))
                    {
                        kStateSave = kState;
                        kState = KState.kReplacing;
                        ushort nSize = ascCurrent.nTermSize(nInPosCurrent);
                        nIdsLeftInLiteral = (ushort)(1 + nIdsLeftInLiteral - nSize);
                        nInPosA = (ushort)(nInPosCurrent + nSize);
                        ascCurrent = ascB;
                        rgnCurrent = ascCurrent.rgnTree;
                        rgnNewVblIdCurrent = rgnNewVblIdB;
                        nInPosCurrent = nNewPosn;
                        nIdsLeftInTerm = 1 + ascB.nTermSize(nNewPosn);
                        continue;
                    }
                }

                ushort nInPosPrev = nInPosCurrent;
                ushort nOriginalPos = 0;

                sbyte nNewId;
                sbyte nInId = rgnCurrent[nInPosCurrent++];
                if (mrsToUpdate != null)
                {
                    if (kState == KState.kCopyANeg || kState == KState.kCopyAPos)
                        mofInto.MapBack(nInPosPrev, Vbv.vbvA, out nOriginalPos);
                    else
                        UpdateIfReplaced(nInPosPrev, out nOriginalPos);
                }

                if (nInId >= Atp.nVar)
                {
                    nNewId = (rgnNewVblIdCurrent == null) ? nInId : rgnNewVblIdCurrent[nInId];
                    if (nNewId == Tde.nNoVblId)
                    {
                        switch (kState)
                        {
                            case KState.kGetting:
                                nNewId = mvf.nNextVblId++;
                                break;
                            case KState.kComparing:
                                nNewId = rgnK[nKPosn];
                                break;
                            case KState.kReplacing:
                                nNewId = mvf.nNextVblId++;
                                break;
                            case KState.kCopyANeg:
                            case KState.kCopyBNeg:
                            case KState.kCopyAPos:
                            case KState.kCopyBPos:
                                nNewId = mvf.nNextVblId++;
                                break;
                            default:
                                throw new ArgumentException();
                        }

                        rgnNewVblIdCurrent[nInId] = nNewId;
                    }
                }
                else
                {
                    Lsm lsmInto = ascCurrent.rglsmData[Asb.nLsmId - nInId];
                    nNewId = (sbyte)asyNew.nIdForLsm(lsmInto);
                }

                switch (kState)
                {
                    case KState.kGetting:
                        rgnK[nKPosn++] = nNewId;
                        break;
                    case KState.kComparing:
                        if (rgnK[nKPosn++] != nNewId)
                            throw new ArgumentException();
                        break;
                    case KState.kReplacing:
                    case KState.kCopyANeg:
                    case KState.kCopyBNeg:
                    case KState.kCopyAPos:
                    case KState.kCopyBPos:
                        if (mrsToUpdate != null)
                        {
                            // Soln references locations in original Asc, map back to that to find which entry to
                            // update with new location.
                            if (nOriginalPos != Mob.nNoOffset)
                                rgnMofUpdated[nOriginalPos] = nOutPosn;
                        }

                        rgnBuffer[nOutPosn++] = nNewId;
                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            rgnBuffer[Asc.nPosnNumNegTerms] = nNumNegLiterals;
            rgnBuffer[Asc.nPosnNumPosTerms] = nNumPosLiterals;

            mofOutput = new Mof(rgnMofUpdated,
                                pvbSource.mofOutput.vieList);  // for now

            Lsm[] rglsmDataNew = asyNew.rglsmData();
            ascResult = new Asc( rgnRemoveDups(rglsmDataNew, rgnBuffer, nOutPosn,
                                                  mrsToUpdate, vbvToUpdate, nNegLiteralsAdded),
                                 rglsmDataNew);


            return true;
        }

        public override void ApplyRevisedIds(Mrs mrs)
        {
        }
    }

    /// <summary>
    /// This does a substitution of term for vbl, and add side terms.
    /// </summary>
    public class PvsWrong : Pvr
    {
        public sbyte nReplaceVbl;
        public ushort nReplaceFrom;

        // local state, move to separate object if these are reused between threads
        ushort nReplaceAt;

        public PvsWrong(Pvb pvbInto, Pvb pvbSource, sbyte nReplaceVbl, ushort nReplaceFrom,
                    Vpo ascInferred, Pvb pvbPrevSameVbv)
            : base(pvbInto, pvbSource, ascInferred, pvbPrevSameVbv)
        {
            this.nReplaceVbl = nReplaceVbl;
            this.nReplaceFrom = nReplaceFrom;
        }

        public override void ApplyRevisedIds(Mrs mrs)
        {
            nReplaceVbl = mrs.nReplacementId(nReplaceVbl);
        }

        protected override void Format(StringBuilder sb)
        {
            if (pvbInto != null)
            {
                sb.Append(" pvbInto:");
                sb.Append(pvbInto.nId);
            }
            if (pvbSource != null)
            {
                sb.Append(" pvbSource:");
                sb.Append(pvbSource.nId);
            }
            sb.Append(" ");
            sb.Append(nReplaceVbl);
            sb.Append(" ");
            sb.Append(nReplaceFrom);
        }

        protected override void InitPerform()
        {
            // nInPosCurrent = nReplaceAt;   set later?
            kState = KState.kCopyStart;
            kStateSave = KState.kReplacing;  // dummy value for init
            nIdsLeftInTerm = 0;

        }

        protected override bool fStartReplace(out ushort nNewPosn)
        {
            sbyte nCurrentId = ascCurrent.rgnTree[nInPosCurrent];

            nNewPosn = nReplaceFrom;
            if (nCurrentId == nReplaceVbl)
            {
                nReplaceAt = nInPosCurrent;
                return true;
            }
            return false;
        }

        protected override void UpdateIfReplaced(ushort nInPosPrev, out ushort nOriginalPos)
        {
            if (nReplaceFrom == nInPosPrev)
                mofInto.MapBack(nReplaceAt, Vbv.vbvA, out nOriginalPos);
            else
                nOriginalPos = 0;   // ?   Mob.nNoOffset
        }
    }

    /// <summary>
    /// Paramodulate: Perform compare, replace and merge of terms
    /// </summary>
    public class Pvp : Pvr
    {

        public ushort nReplaceAt;   // position in A of term to be replaced
        public ushort nReplaceFrom; // position in B of term that matches A
        public ushort nReplaceWith; // position in B of term to replace with

        public Pvp (Pvb pvbInto, Pvb pvbSource, 
                    ushort nReplaceAt, ushort nReplaceFrom, ushort nReplaceWith,
                    Vbv ipoOrigin)
            : base (pvbInto, pvbSource, ipoOrigin, pvbInto)
        {
            this.nReplaceAt = nReplaceAt;
            this.nReplaceFrom = nReplaceFrom;
            this.nReplaceWith = nReplaceWith;
        }

        /// <summary>
        /// Construct step for building new equality clause from two others
        /// </summary>
        public static Pvp pvpForJoin(Mrs mrs, Vbv vbvSoln)
        {
            Vbv vbvForJoin = vbvSoln.vbvForJoin();
            Mof mofOutputLeft = mrs.mesLeft.pvbLatest.mofOutput;
            Mof mofOutputRight = mrs.mesRight.pvbLatest.mofOutput;

            ushort nMappedAt;
            Vbv vbvAt;
            mofOutputLeft.Lookup(vbvForJoin.nReplaceAtPosn, out nMappedAt, out vbvAt);

            ushort nMappedFrom;
            Vbv vbvFrom;
            mofOutputRight.Lookup(vbvForJoin.nReplaceFromPosn, out nMappedFrom, out vbvFrom);

            ushort nMappedWith;
            Vbv vbvWith;
            mofOutputRight.Lookup(vbvForJoin.nReplaceWithPosn, out nMappedWith, out vbvWith);


            Pvp pvpNew = new Pvp(mrs.mesLeft.pvbLatest, mrs.mesRight.pvbLatest,
                                 nMappedAt, nMappedFrom, nMappedWith, vbvForJoin);
            return pvpNew;
        }

        protected override void Format(StringBuilder sb)
        {
            base.Format(sb);
            sb.Append(" ");
            sb.Append(nReplaceAt);
            sb.Append(" ");
            sb.Append(nReplaceFrom);
            sb.Append(" ");
            sb.Append(nReplaceWith);
        }

        protected override void InitPerform()
        {
            nInPosCurrent = nReplaceAt;
            nIdsLeftInTerm = 1 + ascCurrent.nTermSize(nInPosCurrent);
            kState = KState.kGetting;
            kStateSave = KState.kReplacing;  // dummy value for init
        }

        protected override void StartComparing()
        {
            nInPosCurrent = nReplaceFrom;
        }
        protected override bool fSkipTerm()
        {
            if (nInPosCurrent < nReplaceFrom)
            {
                ushort nEndLiteral = (ushort)(nInPosCurrent + nIdsLeftInLiteral);
                if (nReplaceFrom < nEndLiteral)
                {
                    // skip the literal that has the equality
                    nIdsLeftInLiteral = 0;
                    nInPosCurrent = nEndLiteral;
                    return true;
                }
            }
            return false;
        }

        protected override bool fStartReplace(out ushort nNewPosn)
        {
            nNewPosn = nReplaceWith;
            return (nReplaceAt == nInPosCurrent);
        }

        protected override void UpdateIfReplaced(ushort nInPosPrev, out ushort nOriginalPos)
        {
            if (nReplaceWith == nInPosPrev)
                mofInto.MapBack(nReplaceAt, Vbv.vbvA, out nOriginalPos);
            else
                nOriginalPos = 0;   // ?   Mob.nNoOffset
        }

    }

    public class Pvm
    {
        // TODO: use pvbFirst/Next/Last on Pex instead
        List<Pvb> rgpvbSteps = new List<Pvb>();
        public Asc ascResult;

        Dictionary<Asc, Pvb> mpasc_pvbSteps = new Dictionary<Asc, Pvb>();

        public Pvm() { }

        public Pvb Add(Pvb pvb, Asc asc = null)
        {
            rgpvbSteps.Add(pvb);
            if (asc != null)
                mpasc_pvbSteps.Add(asc, pvb);
            return pvb;
        }

        public void Move (Pvm pvmFrom)
        {
            foreach (Pvb pvb in pvmFrom.rgpvbSteps)
                Add(pvb);
        }

        public Pvb pvbFind(Asc asc)
        {
            Pvb pvbResult;
            if (mpasc_pvbSteps.TryGetValue(asc, out pvbResult))
                return pvbResult;
            return null;
        }

        public bool fPerform()
        {
            Mvf mvf = new Mvf();  // need to init nNextVblId?
            foreach (Pvb pvb in rgpvbSteps)
            {
                if (!pvb.fPerform(null, mvf, null))
                    return false;
                ascResult = pvb.ascResult;
            }
            return true;
        }

        public void ApplyRevisedIds(Mrs mrs)
        {
            foreach (Pvb pvb in rgpvbSteps)
                pvb.ApplyRevisedIds(mrs);
        }

    }
}
