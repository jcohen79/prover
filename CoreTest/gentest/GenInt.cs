using GrammarDLL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WBS;

namespace reslab.test
{
    public class Gid
    {
        public const int nFExprList1 = 0;
        public const int nFExprList2 = 1;
        public const int nFExprList3 = 2;
        public const int nFConstant = 3;
        public const int nFFnName1 = 4;
        public const int nFFnName2 = 5;
        public const int nFFnName3 = 6;
        public const int nFPredName1 = 7;
        public const int nFPredName2 = 8;
        public const int nFPredName3 = 9;
        public const int nFFnCall1 = 10;
        public const int nFFnCall2 = 11;
        public const int nFFnCall3 = 12;
        public const int nFPredCall1 = 13;
        public const int nFPredCall2 = 14;
        public const int nFPredCall3 = 15;
        public const int nFQuantifier = 16;
        public const int nFBinExpr = 17;
        public const int nFUnaryExpr = 18;
        public const int nFNaryExpr1 = 19;
        public const int nFNaryExpr2 = 20;
        public const int nFNaryExpr3 = 21;

        public const int nNumFlags = 22;
        public const int nMaxLevels = 1;   // 3
        public readonly int nMaxBits;
        public const int nValues = nNumFlags * nMaxLevels;
        public const int nSecondFlags = 1 << nNumFlags;
        public const int nThirdFlags = 1 << (nNumFlags * 2);

        public long nState = 0;
        public long nAlways;
        private int nBits = 0;
        private bool fSkipSameName;
        private int[] rgnPosn;
        bool fPrevLevel = true;

        public bool fFlag (int nName)
        {
            return (nState & (1 << nName)) != 0;
        }

        public static long nVal(params int[] rgnPos)
        {
            long nv = 0;
            foreach (int nPos in rgnPos)
            {
                nv |= 1L << nPos;
            }
            return nv;
        }

        public static long nTypical()
        {
            long nVal = Gid.nVal(Gid.nFExprList1, Gid.nFPredCall1, Gid.nFConstant,
                Gid.nFFnName1, Gid.nFFnName2, Gid.nFPredName1, Gid.nFQuantifier, Gid.nFBinExpr);
            return nVal;
        }

        public Gid(int nMaxBits, bool fSkipSameName, long nAlways = -1)
        {
            this.nMaxBits = nMaxBits;
            this.fSkipSameName = fSkipSameName;
            rgnPosn = new int[nMaxBits];
            if (nAlways == -1)
                nAlways = nVal(Gid.nFExprList1, Gid.nFPredCall1, Gid.nFConstant);
            this.nAlways = nAlways;
            nState = nAlways;
        }

        public void TestMore (long nState, int nBits, int[] rgnPosn)
        {
            this.nState = nState;
            this.nBits = nBits;
            this.rgnPosn = rgnPosn;
        }

        public bool fMore()
        {
            long nSaveState = nState;
            int nSaveBits = nBits;
            int[] rgnSavePosn = (int[]) rgnPosn.Clone();
            
            int nLevel = nBits - 1;
            while (true)
            {
                if (fPrevLevel)
                {
                    if (nLevel <= 0)
                    {
                        // wrap around to the top level, with one more bit
                        nBits++;
                        if (nBits > nMaxBits)
                        {
                            nBits = 0;
                            fPrevLevel = true;
                            return false;
                        }
                        for (int iB = 0; iB < nBits; iB++)
                            rgnPosn[iB] = iB;
                        nLevel = nBits - 1;
                        fPrevLevel = false;
                        break;
                    }
                    fPrevLevel = false;
                    nLevel--;
                }
                else
                {
                    int nPos = rgnPosn[nLevel];
                    while ((nAlways & (1L << nPos)) != 0)
                        nPos++;
                    nPos++;
                    if (nPos + (nBits - nLevel) > nValues)
                    {
                        fPrevLevel = true;
                        continue;
                    }
                    bool fSkip = false;
                    while (nLevel < nBits)
                    {
                        rgnPosn[nLevel] = nPos;
                        if (fSkipSameName)
                        {
                            if (fNeedSkip(nLevel, nPos))
                            {
                                fSkip = true;
                                break;
                            }
                        }
                        while ((nAlways & (1L << nPos)) != 0)
                            nPos++;
                        nPos++;
                        nLevel++;
                    }
                    if (!fSkip)
                        break;
                }
            }
            nState = nAlways;
            for (int iL = 0; iL < nBits; iL++)
                nState |= 1L << rgnPosn[iL];
            return true;
        }

        private bool fNeedSkip (int iB, int nPos)
        {
            int iC = nPos;
            while (iC >= nNumFlags)
            {
                iC = iC - nNumFlags;
                for (int iL = 0; iL < iB; iL++)
                {
                    if (rgnPosn[iL] == iC)
                        return true;
                }
            }
            return false;
        }
    }

    public class TqsGid : Iqs
    {
        Gid gid;
        Tqs tqsMain;
        Ivd dvdData = null;
        public LParse res;
        int nDepth = 2;
        GL gl;

        public TqsGid (Gid gid, LParse res)
        {
            this.gid = gid;
            this.res = res;
        }

        public KInitializeResult fInitialize(Ivd ivdData)
        {
            Sko.AddSyms(res, new ExpressionEvaluatorGrammar());
            gl = new GL(res, gid, nDepth);

            Tqd tqdRoot = gl.tqdlSequent;

            tqsMain = tqdRoot.TqsMake(null);
            return tqsMain.fInitialize(dvdData);
        }

        public KVisitResult fMoveNext(Ivd ivdData)
        {

            KVisitResult kRes = tqsMain.fMoveNext(dvdData);
            if (kRes == KVisitResult.Continue)
            {
                if (!gid.fMore())
                {
                    nDepth++;
                }
                gl.BuildTqs(gid);
                gl.SetDepth(nDepth);
                if (tqsMain.fInitialize(dvdData) == KInitializeResult.Succeeded)
                    return KVisitResult.Stop;
            }
            return kRes;
        }

        public object objCurrent(Imc imc)
        {
            return tqsMain.objCurrent(imc);
        }
    }

}
