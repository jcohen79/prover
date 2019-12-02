
using GraphMatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace reslab
{
    public class Tde
    {
        public Avc avcValue;  // which side nValue is from
        public ushort nValue;  // >nRightSide: index into lhs arg to embed/equate. 0:no value
                               // during construction from Lsx, is used to store the Tde index num
        public const ushort nNoValue = 0;

        public Tde tdePrevSbst;
        public const sbyte nNoVblId = 127;
        public const sbyte nFailed = 126;

        public Tde()
        {
        }

        public bool fIsBound()
        {
            // assumes that nValue is cleared when Embded layer is popped
            return nValue != nNoValue;
        }
    }

    /// <summary>
    /// Variable assignment map
    /// </summary>
    public class Vam
    {
        sbyte[] rgnIdMap;
        public sbyte nNextId = Asb.nVar;

        public Vam (Asc asb)
        {
            int nMaxId = asb.nMaxVarId(Asc.nClauseLeadingSizeNumbers) + 1;
            rgnIdMap = new sbyte[nMaxId];
            for (int i = 0; i < nMaxId; i++)
                rgnIdMap[i] = Tde.nNoVblId;
        }

        public sbyte nMapId(sbyte nId)
        { 
            sbyte nIdMapped = rgnIdMap[nId];
            if (nIdMapped == Tde.nNoVblId)
            {
                nIdMapped = nNextId++;
                rgnIdMap[nId] = nIdMapped;
            }
            return nIdMapped;
        }

        public void ClearVbls(sbyte nStartingWith)
        {
            for (int i = 0; i < rgnIdMap.Length; i++)
            {
                sbyte nId = rgnIdMap[i];
                if (nId >= nStartingWith)
                    rgnIdMap[i] = Tde.nNoVblId;
            }
            nNextId = nStartingWith;
        }
    }


    /// <summary>
    /// Base for array form of sequents and clauses
    /// </summary>
    public abstract class Asb : Bid, Isi
    {
        public readonly sbyte[] rgnTree;
        public readonly Lsm[] rglsmData;   // for negative ids starting at nLsmId
        public const int nLsmId = -1;
        public const int nVar = 0;

        public const int nClauseLeadingSizeNumbers = 2;
        public const int nMaxTerms = 126;


        protected Asb(sbyte[] rgnTree, Lsm[] rglsmData)
        {
            this.rgnTree = rgnTree;
            this.rglsmData = rglsmData;
        }

        public int nMaxLsmLevel ()
        {
            int nMax = Lsm.nInitLevel;
            for (int nSym = 0; nSym < rglsmData.Length; nSym++)
            {
                Lsm lsm = rglsmData[nSym];
                int nLevel = lsm.nLevel;
                if (nMax < nLevel)
                    nMax = nLevel;
            }
            return nMax;
        }

        protected bool fEqualsRange(Asb asbOther, ushort nStartOffset,
                                    int nLimit, ushort nOtherOffset, 
                                    Vam vamL = null, Vam vamR = null)
        {
            ushort nOtherPos = nOtherOffset;
            for (int i = nStartOffset; i < nLimit; i++)
            {
                sbyte nId = rgnTree[i];
                sbyte nIdOther = asbOther.rgnTree[nOtherPos++];
                if (nId >= nVar)
                {
                    if (nIdOther < nVar)
                        return false;
                    if (vamL != null)
                        nId = vamL.nMapId(nId);
                    if (vamR != null)
                        nIdOther = vamR.nMapId(nIdOther);
                    if (nId != nIdOther)
                        return false;
                }
                else
                {
                    if (nIdOther >= nVar)
                        return false;
                    if (rglsmData[nLsmId - nId] != asbOther.rglsmData[nLsmId - nIdOther])
                        return false;
                }
            }

            return true;
        }

        protected void IdRange(int nFrom, int nSize, ref sbyte nVarId, ref sbyte nSymId)
        {
            int nTo = nFrom + nSize;
            for (int nPos = nFrom; nPos < nTo; nPos++)
            {
                sbyte nId = rgnTree[nPos];
                if (nId > nVarId)
                    nVarId = nId;
                else if (nId < nSymId)
                    nSymId = nId;
            }
        }

        public int nMaxVarId(int nStartAt)
        {
            int nMaxVblId = Atp.nVar - 1;
            for (int i = nStartAt; i < rgnTree.Length; i++)
            {
                if (rgnTree[i] > nMaxVblId)
                    nMaxVblId = rgnTree[i];
            }
            return nMaxVblId;
        }

        protected static void AppendDisjunctSize(Ascb ascb, Lsx lsxDisj)
        {
            if (lsxDisj == Lsm.lsmNil)
            {
                ascb.WriteByte(0);
                return;
            }
            int nLen = lsxDisj.nLength();
            Debug.Assert(nLen <= 255);
            ascb.WriteByte((sbyte)nLen);
        }
        protected static void AppendDisjunctTerms(Ascb ascb, Lsx lsxDisj)
        {
            while (lsxDisj != Lsm.lsmNil)
            {
                Lpr lprDisj = (Lpr)lsxDisj;
                AppendTerm(ascb, lprDisj.lsxCar, false);
                lsxDisj = lprDisj.lsxCdr;
            }

        }

        protected static void AppendDisjunct(Ascb ascb, Lsx lsxDisj)
        {
            Lpr lprDisj = (Lpr)lsxDisj;
            AppendDisjunctSize(ascb, lprDisj.lsxCar);
            AppendDisjunctSize(ascb, lprDisj.lsxCdr);
            AppendDisjunctTerms(ascb, lprDisj.lsxCar);
            AppendDisjunctTerms(ascb, lprDisj.lsxCdr);
        }

        static void AppendTerm(Ascb ascb, Lsx lsxTerm, bool fLeadTerm)
        {
            if (lsxTerm is Lsm)
            {
                Lsm lsmTerm = (Lsm)lsxTerm;
                lsmTerm.ipdData = null;
                ascb.AppendLsm(lsmTerm);
                if (!fLeadTerm && (lsmTerm.nArity == Lsm.nArgsUndefined))
                {
                    lsmTerm.nArity = Lsm.nArityConst;
                }
            }
            else
            {
                bool fFirst = true;
                Lsm lsmChild = null;
                byte nChildren = Lsm.nArityConst - 1;
                while (lsxTerm != Lsm.lsmNil)
                {
                    Lpr lprTerm = (Lpr)lsxTerm;
                    Lsx lsxChild = lprTerm.lsxCar;
                    AppendTerm(ascb, lsxChild, fFirst);
                    if (fFirst)
                    {
                        fFirst = false;
                        Debug.Assert(lsxChild is Lsm);
                        lsmChild = (Lsm)lsxChild;
                    }
                    nChildren++;
                    lsxTerm = lprTerm.lsxCdr;
                }

                // store arity of symbol at start of term
                if (lsmChild.nArity == Lsm.nArgsUndefined)
                {
                    lsmChild.nArity = nChildren;
                }
                else if (lsmChild.nArity != nChildren)
                    throw new ArgumentException();
            }
        }

        public Lsx lsxToDisj(ref int nOffset, Asy asy)
        {
            if (rgnTree == null)
                return Lsm.lsmInvalidTerm;

            sbyte nNegTerms = rgnTree[nOffset++];
            sbyte nPosTerms = rgnTree[nOffset++];
            Lsx lsxNeg = lsxToDisjTerms(nNegTerms, ref nOffset, asy);
            Lsx lsxPos = lsxToDisjTerms(nPosTerms, ref nOffset, asy);
            return new Lpr(lsxNeg, lsxPos);
        }

        public Lsx lsxToDisjTerms(sbyte nTerms, ref int nOffset, Asy asy)
        {
            if (nTerms == 0)
                return Lsm.lsmNil;
            Lpr lprHead = null;
            Lpr lprTail = null;
            for (int nTerm = 0; nTerm < nTerms; nTerm++)
            {
                Lsx lsxTerm = lsxToTerm(ref nOffset, asy);
                Sko.AddToResult(lsxTerm, ref lprHead, ref lprTail);

            }
            return lprHead;
        }

        public Lsx lsxToTerm(ref int nOffset, Asy asy)
        {
            if (rgnTree == null || nOffset >= rgnTree.Length)
            {
                return Lsm.lsmInvalidTerm;
            }
            try
            {
                sbyte nSym = rgnTree[nOffset++];
                int nISym = nSym;   // until switch to sbyte
                if (nISym <= nLsmId)
                {
                    Lsm lsmSym = rglsmData[nLsmId - nISym];
                    if (lsmSym.nArity == Lsm.nArgsUndefined)
                        lsmSym.nArity = Lsm.nArityConst;
                    int nArity = lsmSym.nArity - Lsm.nArityConst;
                    if (nArity == 0)
                        return lsmSym;
                    Lpr lprHead = new Lpr(lsmSym, Lsm.lsmNil);
                    Lpr lprTail = lprHead;
                    for (int nArg = 0; nArg < nArity; nArg++)
                    {
                        Lsx lsxTerm = lsxToTerm(ref nOffset, asy);
                        Sko.AddToResult(lsxTerm, ref lprHead, ref lprTail);
                    }
                    return lprHead;
                }
                else
                {
                    Lsm lsm;
                    if (!asy.mpn_lsmSymbols.TryGetValue(nISym, out lsm))
                    {
                        lsm = new Lsm(Lsm.stVarPrefix + nISym);
                        asy.mpn_lsmSymbols.Add(nISym, lsm);
                    }
                    return lsm;
                }
            }
            catch (Exception )
            {
                return Lsm.lsmInvalidTerm;
            }
        }

        /// <summary>
        /// Fill in values for aic.rgnTermSize and rgnTermOffset for nTerms number of terms starting at nOffset
        /// </summary>
        public void GetTermOffsets(Aic aic, ref byte nTermNum, byte nTerms, ref ushort nOffset)
        {
            while (nTerms-- > 0)
            {
                aic.rgnTermOffset[nTermNum++] = nOffset;
                ushort nPosn = nOffset;
                ushort nStart = nOffset;
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
                nOffset = nPosn;
                while (nPosn > nStart)
                {
                    nPosn--;
                    sbyte nId = rgnTree[nPosn];
                    if (nId <= Asb.nLsmId)
                    {
                        Lsm lsm = rglsmData[Asb.nLsmId - nId];
                        if (lsm.nArity <= Lsm.nArityConst)
                            aic.rgnTermSize[nPosn] = 1;
                        else
                        {
                            ushort nChild = (ushort)(nPosn + 1);
                            for (int i = lsm.nArity - Lsm.nArityConst; i > 0; i--)
                            {
                                nChild += aic.rgnTermSize[nChild];
                            }
                            aic.rgnTermSize[nPosn] = (byte)(nChild - nPosn);
                        }
                    }
                    else
                        aic.rgnTermSize[nPosn] = 1;
                }
            }
        }

        /// <summary>
        /// Return size of term at offset
        /// </summary>
        public ushort nTermSize(int nOffset)
        {
            return nTermSize(rgnTree, rglsmData, nOffset);
        }

        public static ushort nTermSize(sbyte[] rgnTree, Lsm[] rglsmData, int nOffset)
        {
            int nPosn = nOffset;
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
            return (ushort) (nPosn - nOffset);
        }

        public ushort nOffsetOfLit(int nLitNum)
        {
            ushort nOffset = nClauseLeadingSizeNumbers;
            while (nLitNum > 0)
            {
                nOffset += nTermSize(nOffset);
                nLitNum--;
            }
            return nOffset;
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            Res.stMakeString(this, pctl, sb);
        }

        public abstract Lsx lsxTo(Asy asy);
    }

    public class Asy
    {
        public readonly Dictionary<int, Lsm> mpn_lsmSymbols;  // map ids assigned to Lsm local to an Asb
        public readonly Dictionary<Lsm, int> mplsm_nSymbols;  // map Lsm back to ids assigned to Lsm local to an Asb
        public sbyte nNextVblId = 0;
        private int nNextConstId = Asb.nLsmId;

        public readonly Dictionary<int, int> mpn_nSymbols;  // map from vbl ids defined elsewhere to vbl ids here

        public Asy()
        {
            mpn_lsmSymbols = new Dictionary<int, Lsm>();
            mplsm_nSymbols = new Dictionary<Lsm, int>();
            mpn_nSymbols = new Dictionary<int, int>();
        }

        public int nIdForLsm(Lsm lsm)
        {
            int nId;
            if (!mplsm_nSymbols.TryGetValue(lsm, out nId))
            {
                if (lsm.fVariable())
                {
                    nId = nNextVblId++;
                }
                else
                {
                    nId = nNextConstId--;
                }
                mplsm_nSymbols.Add(lsm, nId);
                mpn_lsmSymbols.Add(nId, lsm);
            }
            return nId;
        }

        public Lsm[] rglsmData()
        {
            Lsm[] rmlsm = new Lsm[Asb.nLsmId - nNextConstId];
            foreach (var entry in mplsm_nSymbols)
            {
                if (entry.Value <= Asb.nLsmId)
                    rmlsm[Asb.nLsmId - entry.Value] = entry.Key;
            }
            return rmlsm;
        }

    }

    public class Ascb
    {
        public readonly Asy asy;

        public const int nInitialBufferSize = 255;
        public sbyte[] rgnBuffer;
        private int nBufferMax;
        public int nCurrentBufferSize;
        int nNegTerms;
        int nPosTerms;

        public Ascb()
        {
            nBufferMax = nInitialBufferSize;
            rgnBuffer = new sbyte[nBufferMax];
            asy = new Asy();
            Reset();
        }

        private Ascb(Ascb ascbBase)
        {
            nBufferMax = nInitialBufferSize;
            rgnBuffer = new sbyte[nBufferMax];
            this.asy = ascbBase.asy;
        }

        public void SetSizes(int nNegTerms, int nPosTerms)
        {
            this.nNegTerms = nNegTerms;
            this.nPosTerms = nPosTerms;
        }

        public void Reset()
        {
            nCurrentBufferSize = 0; //  Asc.nClauseLeadingSizeNumbers;
        }

        public Asc ascBuild()
        {
            return ascBuild(nNegTerms, nPosTerms);
        }

        public Asc ascBuild(int nNumResNegTerms, int nNumResPosTerms)
        {
            Asc asc = (Asc)Build(true);
            asc.rgnTree[Asc.nPosnNumNegTerms] = (sbyte)nNumResNegTerms;
            asc.rgnTree[Asc.nPosnNumPosTerms] = (sbyte)nNumResPosTerms;
            return asc;
        }

        public Ascb DummySizes()
        {
            rgnBuffer[Asc.nPosnNumNegTerms] = 0;
            rgnBuffer[Asc.nPosnNumPosTerms] = 0;
            nCurrentBufferSize = Asc.nClauseLeadingSizeNumbers;
            return this;
        }

        public void WriteByte(sbyte nByte)
        {
            if (nCurrentBufferSize >= nBufferMax)
            {
                nBufferMax *= 2;
                sbyte[] rgnNewBuffer = new sbyte[nBufferMax];
                for (int i = 0; i < nCurrentBufferSize; i++)
                    rgnNewBuffer[i] = rgnBuffer[i];
                rgnBuffer = rgnNewBuffer;
            }
            //  CheckForSkippedVblId(nByte);    // Ids can be skipped because K is constructed first, and other terms are compared to that

            rgnBuffer[nCurrentBufferSize++] = nByte;
        }

        void CheckForSkippedVblId (sbyte nByte)
        {
            if (nByte <= Asc.nVar)
                return;
            if (nCurrentBufferSize < Asc.nClauseLeadingSizeNumbers)
                return;
            sbyte nLower = (sbyte) (nByte - 1);
            for (int i = Asc.nClauseLeadingSizeNumbers; i < nCurrentBufferSize; i++)
            {
                sbyte nId = rgnBuffer[i];
                if (nLower >= nId)
                    return;
            }
            Debug.Assert(false);
        }

        int nMaxVblId(int nPrefixSize)
        {
            sbyte nMaxVbl = Asb.nVar - 1;
            for (int i = nPrefixSize; i < nCurrentBufferSize; i++)
            {
                sbyte nId = rgnBuffer[i];
                if (nId > nMaxVbl)
                    nMaxVbl = nId;
            }
            return nMaxVbl - Asb.nVar + 1;
        }

        sbyte[] rgnBuild()
        {
            return null;
        }

        /// <summary>
        /// Transfer content of Ascb to the asb.
        /// Vbls are made consecutive so that Atp can easily map back.
        /// </summary>
        public Asb Build(bool fMakeAsc)
        {
            int nPrefixSize = fMakeAsc ? Asc.nClauseLeadingSizeNumbers : Atp.nOffsetFirstTerm;
            sbyte[] rgnVal = new sbyte[nCurrentBufferSize];
            for (int i = 0; i < nPrefixSize; i++)
                rgnVal[i] = rgnBuffer[i];

            int nNumVbls = nMaxVblId(nPrefixSize);
            sbyte[] rgnVblIdMap = new sbyte[nNumVbls];
            for (int nId = Asb.nVar; nId < nNumVbls; nId++)
                rgnVblIdMap[nId] = Tde.nNoVblId;

            sbyte nNextId = Asb.nVar;
            for (int i = nPrefixSize; i < nCurrentBufferSize; i++)
            {
                sbyte nId = rgnBuffer[i];
                if (nId >= Asb.nVar)
                {
                    sbyte nMappedId = rgnVblIdMap[nId];
                    if (nMappedId == Tde.nNoVblId)
                    {
                        nMappedId = nNextId++;
                        rgnVblIdMap[nId] = nMappedId;
                    }
                    rgnVal[i] = nMappedId;
                }
                else
                    rgnVal[i] = nId;
            }
            Lsm[] rglsmDataNew = asy.rglsmData();
            Asb asbNew = fMakeAsc ? (Asb) new Asc(rgnVal, rglsmDataNew) : new Atp(rgnVal, rglsmDataNew); 
            return asbNew;
        }

        public void AppendLsm(Lsm lsmTerm)
        {
            int nId = asy.nIdForLsm(lsmTerm);
            WriteByte((sbyte)nId);
        }

        public void AppendId(sbyte nId)
        {
            WriteByte(nId);
        }

        public Ascb ascbCreateWithSameTde()
        {
            Ascb ascbR = new Ascb(this);
            return ascbR;
        }

        public bool fTermMatches(int nOffset, Ascb ascbOther, int nKOffset, ushort nSize)
        {
            if (nCurrentBufferSize != nSize)
                return false;
            for (int nPos = 0; nPos < nCurrentBufferSize; nPos++)
            {
                if (rgnBuffer[nPos + nOffset] != ascbOther.rgnBuffer[nPos + nKOffset])
                    return false;
            }
            return true;
        }

        public void AppendTerm(Ascb ascbTerm)
        {
            int nLen = ascbTerm.nCurrentBufferSize;
            for (int nPos = 0; nPos < nLen; nPos++)
                WriteByte(ascbTerm.rgnBuffer[nPos]);
        }
    }

}