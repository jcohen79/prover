using System;
using System.Collections;

namespace reslab
{
    /// <summary>
    /// Hold a pair of terms. They have come from different clauses, but they can share vbls.
    /// </summary>
    public class Atp : Asb, Rib
    {
        public const ushort nOffsetLeftSize = 0;
        public const ushort nOffsetFirstTerm = 1;
        public bool fLiteralLevel;

        public Atp(sbyte[] rgnTree, Lsm[] rglsmData) : base(rgnTree, rglsmData)
        {
        }

        public Ipr iprPriority()
        {
            return Prn.prnObtain(rgnTree.Length);
        }
        public ushort nRightSidePosn ()
        {
            return (ushort) (rgnTree[nOffsetLeftSize] + nOffsetFirstTerm);
        }

        public int nNumVblsOnLeft()
        {
            int nLeftSideSize = rgnTree[nOffsetLeftSize];
            int nMaxVblId = nVar - 1;
            int nEnd = nLeftSideSize + nOffsetFirstTerm;
            for (int i = nOffsetFirstTerm; i < nEnd; i++)
            {
                sbyte nId = rgnTree[i];
                if (nId >= nMaxVblId)
                    nMaxVblId = nId;
            }
            return nMaxVblId - (nVar - 1);
        }

        public bool fOccur(sbyte nNeedleId, ushort nOffsetE)  // pg 268
        {
            int nPending = nTermSize(nOffsetE);
            while (nPending-- > 0)
            {
                sbyte nId = rgnTree[nOffsetE++];
                if (nNeedleId == nId)
                    return true;
            }
            return false;
        }

        public bool fSymmetric (bool fIdentical)
        {
            ushort nLhs = nOffsetFirstTerm;
            sbyte nLeftLen = rgnTree[Atp.nOffsetLeftSize];
            ushort nRhs = (ushort)(Atp.nOffsetFirstTerm + nLeftLen);
            ushort nRightLen = (ushort) (rgnTree.Length - nRhs);
            if (nLeftLen != nRightLen)
                return false;
            bool nFirstVbl = true;
            sbyte nVblIdDiff = 0;
            for (ushort nPosn = 0; nPosn < nLeftLen; nPosn++)
            {
                sbyte nLhsVal = rgnTree[nLhs + nPosn];
                sbyte nRhsVal = rgnTree[nRhs + nPosn];
                if (nLhsVal < Asb.nVar)
                {
                    if (nLhsVal != nRhsVal)
                        return false;
                }
                else
                {
                    if (nRhsVal < Asb.nVar)
                        return false;
                    if (nFirstVbl)
                    {
                        nFirstVbl = false;
                        nVblIdDiff = (sbyte) (nRhsVal - nLhsVal);
                        if (fIdentical)
                        {
                            if (nVblIdDiff != 0)
                                throw new ArgumentException();
                        }
                        else if (nVblIdDiff <= 0)
                            throw new ArgumentException();
                    }
                    else if (nVblIdDiff != (sbyte)(nRhsVal - nLhsVal))
                        return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
                return false;
            Asb asbOther = (Asb)obj;
            if (rgnTree.Length != asbOther.rgnTree.Length)
                return false;

            return fEqualsRange(asbOther, nOffsetFirstTerm, rgnTree.Length, nOffsetFirstTerm);
        }

        public override int GetHashCode()
        {
            int nAcc = 0;
            for (int i = nOffsetFirstTerm; i < rgnTree.Length; i++)
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

        public override Lsx lsxTo(Asy asy)
        {
            Lpr lprHead = null;
            Lpr lprTail = null;
            int nPos = nOffsetFirstTerm;
            bool fRight = true;
            int nSideLen = rgnTree[Atp.nOffsetLeftSize];
            do
            {
                fRight = !fRight;
                Lpr lprHeadSide = null;
                Lpr lprTailSide = null;
                if (fRight)
                {
                    nSideLen = rgnTree.Length - nSideLen - Atp.nOffsetFirstTerm;
                }
                int nEnd = nPos + nSideLen;
                while (nPos < nEnd)
                {
                    Lsx lsxTerm = lsxToTerm(ref nPos, asy);
                    Sko.AddToResult(lsxTerm, ref lprHeadSide, ref lprTailSide);
                    if (lsxTerm == Lsm.lsmInvalidTerm)
                        break;
                }
                Lsx lsxSide = (lprHeadSide == null) ? (Lsx) Lsm.lsmNil : lprHeadSide;
                Sko.AddToResult(lsxSide, ref lprHead, ref lprTail);
            }
            while (!fRight);
            return lprHead;
        }

        public Ipr iprComplexity()
        {
            return Prn.prnObtain(rgnTree.Length);
        }
        public void NextSameSize(Atp riiNext)
        {
            throw new NotImplementedException();
        }

        public Atp riiNextSameSize()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// For testing
        /// </summary>
        public static Atp atpMake(LParse lparse, string stL, string stR)
        {
            Lsx lsxL = lparse.lsxParse(stL);
            Asc ascL = Asc.ascFromLsx(lsxL);
            Lsx lsxR = lparse.lsxParse(stR);
            Asc ascR = Asc.ascFromLsx(lsxR);
            Avc avcL = new Avc(new Aic(ascL));
            Avc avcR = new Avc(new Aic(ascR));

            Moa mobToOffsetForChild;
            Mva mvbMapToVblIdForChild;

            Spl spl = new Spl();
            spl.Init(avcL.aic.asc, Asb.nClauseLeadingSizeNumbers,
                avcR.aic.asc, Asb.nClauseLeadingSizeNumbers, null, null, true);
            return spl.atpCreateOutput(out mobToOffsetForChild, out mvbMapToVblIdForChild);
        }

    }

    /// <summary>
    /// Save state during nested vbl sbst
    /// </summary>
    public class Asp
    {
        public int nInputPos;
        public int nEndPos;
        public Asb asbSource;
        public Vbv vbvSource;
        public sbyte[] rgnNewIdSave;
        public Vbv vbvCurrent;
        public Asp aspPrev;
#if DEBUG
        public bool fReplace;  // for debug check only
        public int nPrevInPos;
        public Vbv vbvForValue;
        public int nNewPos;
#endif

        static sbyte cId = 0;

        public Asp()
        {
        }

        public static sbyte[] rgnMakeNewId()
        {
            sbyte[]  rgnNewId = new sbyte[Tde.nNoVblId - 1];
            for (int nId = 0; nId < rgnNewId.Length; nId++)
                rgnNewId[nId] = Tde.nNoVblId;

            rgnNewId[0] = cId++;
            return rgnNewId;
        }
    }

    /// <summary>
    /// Keep a list of variable mappings: 
    /// </summary>
    public class Svm
    {
        public sbyte[] rgnNewIdTable;
        public Svm svmNext;
        public Vbv vbvKey;

        public static sbyte nFindLsm(Lsm[] rglsmTemp, Lsm lsmToAdd)
        {
            for (sbyte nOldId = 0; nOldId < rglsmTemp.Length; nOldId++)
            {
                if (rglsmTemp[nOldId] == lsmToAdd)
                    return nOldId;
            }
            return Tde.nNoVblId;
        }

    }

    /// <summary>
    /// Perform hashcode and equality for using rgnTree as key in Hla to find Pie
    /// </summary>
    public class Tec : IEqualityComparer
    {
        public static Tec lecOnly = new Tec();

        public new bool Equals(object x, object y)
        {
            sbyte[] rgnX = (sbyte[])x;
            sbyte[] rgnY = (sbyte[])y;
            int nLen = rgnX.Length;
            if (nLen != rgnX.Length)
                return false;
            for (int i = 0; i < nLen; i++)
            {
                if (rgnX[i] != rgnY[i])
                    return false;
            }
            return true;
        }

        public int GetHashCode(object obj)
        {
            sbyte[] rgnX = (sbyte[])obj;
            int nLen = rgnX.Length;
            int nAcc = nLen;
            for (int i = 0; i < nLen; i++)
            {
                nAcc = (nAcc * 23) + rgnX[i];
            }
            return nAcc;
        }
    }

    /// <summary>
    /// Hold lower level hashtable: key is rgntree, to hold solvers for equating with that rgntree.
    /// Is shared accross different symbols used with that rgntree.
    /// </summary>
    public class Hla<Tval> where
        Tval : class
    {
        public Hashtable mprgn_valAtpsForSyms;
        const int nDefaultCapacity = 10;

        public Hla()
        {
            mprgn_valAtpsForSyms = new Hashtable(nDefaultCapacity, Tec.lecOnly);
        }

        public Tval valGet (sbyte[] rgnTree) 
        {
            return (Tval) mprgn_valAtpsForSyms[rgnTree];
        }

        public void Add (sbyte[] rgnTree, Tval val)
        {
            mprgn_valAtpsForSyms.Add(rgnTree, val);
        }
    }

    /// <summary>
    /// Perform hashcode and equality for using rgnTree as key in Hla to find Pie
    /// </summary>
    public class Aec : IEqualityComparer
    {
        public static Aec aecOnly = new Aec();

        public new bool Equals(object x, object y)
        {
            Atp atpX = (Atp)x;
            Atp atpY = (Atp)y;
            return atpX.Equals(atpY);
        }

        public int GetHashCode(object obj)
        {
            Atp atpX = (Atp)obj;
            return atpX.GetHashCode();
        }
    }

    /// <summary>
    /// Index value by an Atp, to hold solvers for equating with that Atp.
    /// TODO: return an object that can work in either direction of Atp. Returned object
    /// has accessors to access the appropriate side, depending on if the Atp is reversed.
    /// </summary>
    public class Hua<Tval> where
        Tval : class
    {
        public Hashtable mprgn_valAtpsForSyms;
        const int nDefaultCapacity = 10;

        public Hua()
        {
            mprgn_valAtpsForSyms = new Hashtable(nDefaultCapacity, Aec.aecOnly);
        }

        public Tval valGet(Atp atp)
        {
            return (Tval)mprgn_valAtpsForSyms[atp];
        }

        public void Add(Atp atp, Tval val)
        {
            mprgn_valAtpsForSyms.Add(atp, val);
        }
    }
}

