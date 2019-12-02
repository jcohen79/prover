using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab
{

    public class Aspb
    {
        public ushort nCurrent = 0;
        public ushort[] nBuf = new ushort[Ascb.nInitialBufferSize];

        public void AppendVal (ushort nVal)
        {
            nBuf[nCurrent++] = nVal;
        }
    }

    /// <summary>
    /// Show variable values in clause
    /// </summary>
    public class Shv
    {
        protected Res res;
        public readonly Avc avcA;
        public readonly Avc avcB;
        public readonly Cmb cmb;
        public readonly Vbv vbvLeft;
        public readonly Vbv vbvRight;
        public bool fPtiEnabled = true;

        public int nNumResNegTerms = 0;
        public int nNumResPosTerms = 0;
        public Ascb ascbLiteral;
        protected Ascb ascbK;
        public Ascb ascbRes;
        ushort[] rgnTermOffset;   // next posn being written to for terms in ascbRes
        Mva mvbMapToVblIdForChild;
        Moa mobToOffsetForChild;
        sbyte nNextVblId = Atp.nVar;
        Nvi nviList;

        public Shv(Cmb cmb, Vbv vbvLeft, Vbv vbvRight, Avc avcLeft, Avc avcRight, Mva mvbMapToVblIdForChild, Moa mobToOffsetForChild)
        {
            this.cmb = cmb;
            this.res = cmb.res;
            this.vbvLeft = vbvLeft;
            this.vbvRight = vbvRight;
            this.avcA = avcLeft;
            this.avcB = avcRight;
            this.mvbMapToVblIdForChild = mvbMapToVblIdForChild;
            this.mobToOffsetForChild = mobToOffsetForChild;
        }


        public void MakeRgnTermOffset(int nMaxNumTerms)
        {
            rgnTermOffset = new ushort[sbyte.MaxValue];  // allow 1 extra to compute size from diff from next
            rgnTermOffset[0] = Asc.nClauseLeadingSizeNumbers;
        }

        /// <summary>
        /// Dummy versions of MakeAscbResRmu for use by Cmr, which doesn't use K term 
        /// </summary>
        public void InitForCmr()
        {
            ascbRes = new Ascb(); //  ascbK.ascbCreateWithSameTde();
            nNextVblId = Atp.nVar;
            Nvi.nviAddNew(ref nviList, Vbv.vbvA, Asp.rgnMakeNewId());
            Nvi.nviAddNew(ref nviList, Vbv.vbvB, Asp.rgnMakeNewId());
        }

        public virtual void MakeAscbResRmu(ushort nOffsetShowTerm)
        {
            ascbK = new Ascb();

            Vbv vbvSource = Vbv.vbvB;

            Sps sps = new Sps();
            ShowTerm(ascbK, avcA.aic.asc, avcB.aic.asc, nOffsetShowTerm, vbvLeft, vbvRight, vbvSource,
                mvbMapToVblIdForChild, mobToOffsetForChild, sps, true);
            ascbRes = ascbK.ascbCreateWithSameTde();
            sps.GetVblIds(out nNextVblId, out nviList);
        }

    /// <summary>
    /// merge into resolution the non-cut terms from the indicated clause and side
    /// </summary>
    /// <returns>true if tautology</returns>
    public bool fMergeTerms(Asc ascCurrent, bool fPos, bool fResolveNegSide, uint nSkipField, Vbv vbvSource)
        {
            bool fRight = vbvSource == Vbv.vbvB;
            Asc ascLeft = avcA.aic.asc;
            Asc ascRight = avcB.aic.asc;

            // Asc ascCurrent = fRight ? ascRight : ascLeft;

            sbyte nNumNegLiterals = ascCurrent.rgnTree[Asc.nPosnNumNegTerms];
            sbyte nNumPosLiterals = ascCurrent.rgnTree[Asc.nPosnNumPosTerms];
            int nStartLitNum = fPos ? nNumNegLiterals : 0;
            int nBitPos = 1 << nStartLitNum;
            int nStopAtLiteral = fPos ? (nNumPosLiterals + nNumNegLiterals) : nNumNegLiterals;
            ushort nNextOffsetTerm = ascCurrent.nOffsetOfLit(nStartLitNum);
            for (int nLiteralNum = nStartLitNum; nLiteralNum < nStopAtLiteral; nLiteralNum++)
            {
                int nCurrBitPos = nBitPos;
                nBitPos <<= 1;
                ushort nOffsetTerm = nNextOffsetTerm;
                nNextOffsetTerm += ascCurrent.nTermSize(nOffsetTerm);

                if ((nSkipField & nCurrBitPos) == 0)  // skip 1 bits in mask, those are the terms that where unified into K
                {
                    ascbLiteral.Reset();
                    // note: the atp only has the terms that are being skipped. atp could be used in value, but vblIds need to be mapped.
                    // perform susbstitutions to build resulting term
                    Sps sps = new Sps();
                    sps.SetVblIds(nNextVblId, nviList);

                    ShowTerm(ascbLiteral, ascLeft, ascRight, nOffsetTerm, 
                             vbvLeft, vbvRight, vbvSource,
                             mvbMapToVblIdForChild, mobToOffsetForChild, sps, fPtiEnabled);

                    sps.GetVblIds(out nNextVblId, out nviList);

                    if (ascbK != null)
                    {
#if false
                        if ((fRight != fPos) == fResolveNegSide)
                        {
                            if (ascbLiteral.fTermMatches(0, ascbK, 0, (ushort)ascbK.nCurrentBufferSize))   // skip if it is same as unified K
                                continue;
                        }
#else
                        if (ascbLiteral.fTermMatches(0, ascbK, 0, (ushort)ascbK.nCurrentBufferSize))   // skip if it is same as unified K
                        {
                            if ((fRight != fPos) == fResolveNegSide)
                                continue;
                            else
                                return true;  // tautology
#endif
                        }
                    }

                    // compare to all terms so far on neg side: skip
                    if (fTermMatchesAny(false))
                    {
                        if (fPos)
                            return true;  // tautology
                        continue;
                    }
                    // compare to all terms so far on pos side: skip
                    if (fTermMatchesAny(true))
                    {
                        if (!fPos)
                            return true;  // tautology
                        continue;
                    }
                    ascbRes.AppendTerm(ascbLiteral);
                    if (fPos)
                        nNumResPosTerms++;
                    else
                        nNumResNegTerms++;
                    rgnTermOffset[nNumResPosTerms + nNumResNegTerms] = (ushort)ascbRes.nCurrentBufferSize;
                }
            }
            return false;
        }

        /// <summary>
        /// Compare the term in ascbTerm to all the terms already in the indicated side of ascbRes 
        /// </summary>
        bool fTermMatchesAny(bool fPos)
        {
            int numTermsToCompare = fPos ? nNumResPosTerms : nNumResNegTerms;
            if (numTermsToCompare == 0)
                return false;
            int nTermNumInRes = fPos ? nNumResNegTerms : 0;

            for (int nPos = 0; nPos < numTermsToCompare; nPos++)
            {
                ushort nOffset = rgnTermOffset[nTermNumInRes++];
                ushort nSize = (ushort)(rgnTermOffset[nTermNumInRes] - nOffset);
                if (ascbLiteral.fTermMatches(0, ascbRes, nOffset, nSize))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// replace variables in the term in this at nOffset with their value and write result to ascb.
        /// </summary>
        public static void ShowTerm(Ascb ascbOut, Asb asbLeft, Asb asbRight, ushort nTermToShowOffset,
                                    Vbv vbvLeft, Vbv vbvRight, Vbv vbvSource,
                                    Mva mvbMapForChild, Moa mobToOffsetForChild, 
                                    Sps sps, bool fPtiEnabled)
        {
            sps.Init(ascbOut, mvbMapForChild);
            sps.fPtiEnabled = fPtiEnabled;

            sps.mobToOffsetForChild = mobToOffsetForChild;
            sps.SetupSoln(asbLeft, asbRight, vbvLeft, vbvRight);
            sps.SetupSbst(nTermToShowOffset, vbvSource);
            sps.ProcessParts();
            sps.GetResult();
        }

        public Asc ascBuild(Ascb ascbRes)
        {
            return ascbRes.ascBuild(nNumResNegTerms, nNumResPosTerms);
        }

        void Msg (Abt abt, Asc ascR)
        {
            Debug.WriteLine("result" + ascR + "#" + ascR.nId + " <= " + abt.avcA.aic.asc.nId + "," + abt.avcB.aic.asc.nId);

        }

        public bool fProcessAscb(Ascb ascbRes, Abt abt, Esn esnSolution, Unb unb)
        {


            Asc ascR = ascBuild(ascbRes);
            cmb.SetOrigin(ascR, esnSolution, unb, mvbMapToVblIdForChild, mobToOffsetForChild);

#if false
            if (res.fDoReflexivePredicates)
            {
                if (ascR.fSimplifyReflexivePredicates())
                    return true;
                // check for cases like f(a,y) = f(a,b) could have been resolved with x=x
                Rmu cmiSelf = new Rmu(res);
                cmiSelf.SetAbt(new Abt(ascR, ascR, res));
                ascProof = cmiSelf.ascSymmetricClauses(true);
                if (ascProof != null)
                    return false;
                ascProof = cmiSelf.ascSymmetricClauses(false);
                if (ascProof != null)
                    return false;
            }
#endif

            if (res.irr != null)
            {
                res.irr.AscCreated(ascR, this);
                // called in setOrigin? res.irp.Report(Tcd.tcdNewAscConnect, ascR.ribLeft, ascR.ribRight, ascR);
            }

#if DEBUG
            if (res.fVerifyEachStep)
            {
#if false
                // this causes asc to be not be found, because they subsume themselves?
                GnpR.KSide kSide = res.imd.kSide(ascR);
                if (!(res.gnpAscAsc.fIsSubsumed(kSide == GnpR.KSide.kRight, ascR, false))) 
                    // add to Ckn tree so later dups are filtered out
#endif
                {
                    Pvm pvmMain = new Pvm();
                    Pex pex = new Pex();
                    pex.pvbCreateProofSteps(pvmMain, ascR);
                }
            }
#endif

            if (ascR.fEmpty())
            {
                Gfh gfhHypothesis = ascR.gfbSource.gfhIsSecondaryHyp();
                if (gfhHypothesis != null)
                {
                    Asc ascS = ascR.ascSbstfromRefutation(res, (Vbv)esnSolution);
                    if (res.irp != null)
                        res.irp.Report(Tcd.tcdAscFromNgc, ((Gfi)ascS.gfbSource).gfhFrom, esnSolution, ascS);
                    res.SaveForFilter(ascS);
                }
                else
                {
                    if (res.irp != null)
                        res.irr.AscAdded(ascR);
                    res.HaveProof(ascR);
                    return true;
                }
            }
            else
                res.SaveForFilter(ascR);

            return false;
        }
    }
}
