using System;
using System.Diagnostics;

namespace reslab
{


    /// <summary>
    /// Create a new Atp and data to map the vbl ids and offsets from that atp back to the original locations.
    /// This doesn't equate, it just just sbst from the indicated starting locations and current vbl values.
    /// </summary>
    public abstract class Spb
    {


        public const int nOrigin = (Tde.nNoVblId - 1) / 2;
        public const short nLhsValueStart = 0;   // assumes 0 never used as value

        protected sbyte[] rgnNewTree;
        protected ushort[] rgnMapBack;   // for each position in output, what was position in input
        protected Vie vieList;

        protected Mva mvbMapBackVblId;

        protected sbyte nNextSymId;
        public int nOutPosn;
        Asp aspTop;
        protected Asb asbCurrent;
        sbyte nMaxOldVarId;
        protected Lsm[] rglsmTemp;
        protected sbyte[] rgnNewIdTable;   // different input sides use same numbers for different vbls, also constants
        public Svm svmSaveNewIds;
        protected bool fAgain;
        public Vbv vbvSkip;
        public bool fConflictFound = false;
        public bool fPtiEnabled = true;
        protected bool fCheckPti = true;  // experiment only


        public Moa mobToOffsetForChild;
        protected Asb asbLeft;
        protected Asb asbRight;
        protected Vbv vbvLeft;
        protected Vbv vbvRight;
        protected ushort nOffsetLeft;
        protected ushort nOffsetRight;

        sbyte nNextVblId;
        protected Nvi nviList;
        //protected sbyte[] rgnNewIdLeft;
        //protected sbyte[] rgnNewIdRight;

        protected bool fRight;

        protected int nInPos;
        protected int nNumToProcess;
        protected Vbv vbvCurrent;
        protected Vbv vbvSource;
        protected Vba vbaJustExpanded;

        public sbyte nNextLocalId = 0;
        public const int nLocalIdFactor = 2;  // allow space for vbvId and vblId
        public const sbyte nNoVbvId = -1;
        public const sbyte nVbvIdA = -2;
        public const sbyte nVbvIdB = -3;
        public const sbyte nMaxTreeSize = 126;

        public static bool fTrace = false;

        protected void Init(bool fSeparateVblSet)
        {
            rgnNewTree = new sbyte[Ascb.nInitialBufferSize];
            if (Nvi.rgnIdTableForVbv(nviList, Vbv.vbvA) == null)
            {
                Nvi nviLeft = Nvi.nviAddNew(ref nviList, Vbv.vbvA, Asp.rgnMakeNewId());
                if (fSeparateVblSet)
                    Nvi.nviAddNew(ref nviList, Vbv.vbvB, Asp.rgnMakeNewId());
                else
                    Nvi.nviAddNew(ref nviList, Vbv.vbvB, nviLeft.rgnIdTable);
            }

            fRight = true;

            nNextSymId = Atp.nLsmId;
            nOutPosn = Atp.nOffsetFirstTerm;
            aspTop = null;
            nMaxOldVarId = Atp.nVar - 1;
            rglsmTemp = new Lsm[nOrigin];
            svmSaveNewIds = null;

        }

        public void GetVblIds (out sbyte nNextVblId, out Nvi nviList)
        {
            nNextVblId = this.nNextVblId;
            nviList = this.nviList;
        }

        public void SetVblIds(sbyte nNextVblId, Nvi nviList)
        {
            this.nNextVblId = nNextVblId;
            this.nviList = nviList;
        }
        
        protected Vbv vbvKeyNorm (Vbv vbvKey)
        {
            if (vbvKey == vbvLeft)
                return Vbv.vbvA;
            else if (vbvKey == vbvRight)
                return Vbv.vbvB;
            else
                return vbvKey;
        }

        public sbyte[] rgnObtainIdTable(Vbv vbvKey)
        {
            Svm svmOld = svmSaveNewIds;
            while (svmOld != null)
            {
                if (svmOld.vbvKey == vbvKey)
                    return svmOld.rgnNewIdTable;
                svmOld = svmOld.svmNext;
            }

            sbyte[] rgnTable = Nvi.rgnIdTableForVbv(ref nviList, vbvKeyNorm(vbvKey), true);

            Svm svmNew = new Svm();
            svmNew.rgnNewIdTable = rgnTable;
            svmNew.vbvKey = vbvKey;
            svmNew.svmNext = svmSaveNewIds;
            svmSaveNewIds = svmNew;
            return svmNew.rgnNewIdTable;
        }

        protected abstract void StartSide();

        protected sbyte nOutLsm(Lsm lsmToAdd)
        {
            sbyte nOldIdx = Svm.nFindLsm(rglsmTemp, lsmToAdd);
            sbyte nNewId;
            if (nOldIdx == Tde.nNoVblId)
            {
                nNewId = nNextSymId--;
                rglsmTemp[Atp.nLsmId - nNewId] = lsmToAdd;
            }
            else
                nNewId = (sbyte)(Atp.nLsmId - nOldIdx);
            return nNewId;
        }

        void Write(string st)
        {
            if (fTrace)
                Debug.Write(st);
        }

        static int nLineNum = 0;

        void WriteLine()
        {
            if (fTrace)
            {
                Debug.WriteLine("");
                Debug.Write(nLineNum++ + ": ");
            }
        }

        void WriteFrame(Asp aspCheck)
        {
                Write("nInputPos: " + aspCheck.nInputPos);
                Write(", fReplace: " + (aspCheck.fReplace ? "t" : "f"));
                Write(", vbvCurrent: " + aspCheck.vbvCurrent);
                Write(", asbSource: " + aspCheck.asbSource);
                WriteLine();
        }

        void WriteStack()
        {
            Write("  stack:");
            WriteLine();
            for (Asp aspCheck = aspTop; aspCheck != null; aspCheck = aspCheck.aspPrev)
            {
                Write("    ");
                WriteFrame(aspCheck);
            }
        }

        /// <summary>
        /// Return true if there is a loop
        /// </summary>
        KBool kCheckStack (int nNextInPos, bool fReplace, Vbv vbvForValue, int nNewPos)
        {
            for (Asp aspCheck = aspTop; aspCheck != null; aspCheck = aspCheck.aspPrev)
            {
#if false
                if (aspCheck.fReplace)
                    return KBool.kFalse;  // to allow one cycle of an replacement to occur, 
                             // because the check for replace in progress will stop the loop.
#endif

                if (aspCheck.nInputPos == nNextInPos
                     && aspCheck.fReplace == fReplace    // distinguishes skip over Pti term vs inside that term
                     && aspCheck.vbvCurrent == vbvCurrent    // not necessarily unique  (e.g. vbvLeft/vbvRight)
                     && aspCheck.asbSource == asbCurrent    // could be shared between pti, but pti gives new vbvCurrent
                     && aspCheck.nPrevInPos == nInPos
                     && aspCheck.nNewPos == nNewPos
                     && aspCheck.vbvForValue == vbvForValue)    
                        // the new context is included, because that can mean it is not a cycle
                {
                    FoundLoop();
                    return KBool.kFailed;
                }
            }
            return KBool.kFalse;
        }

        public virtual void FoundLoop()
        {
             fConflictFound = true;

            if (fTrace)
                WriteStack();
        }

        /// <summary>
        /// Push current state onto stack. Return true if an error is found.
        /// </summary>
        KBool kPushState(int nTermSize, int nEndPos, bool fReplace, Vbv vbvForValue, int nNewPos)
        {
            int nNextInPos = nInPos + nTermSize;

            if (kCheckStack(nNextInPos, fReplace, vbvForValue, nNewPos) == KBool.kFailed)
                return KBool.kFailed;

            Asp aspNew = new Asp();
            aspNew.nInputPos = nNextInPos;
            aspNew.nEndPos = nEndPos;
            aspNew.aspPrev = aspTop;
            aspNew.asbSource = asbCurrent;
            aspNew.rgnNewIdSave = rgnNewIdTable;
            aspNew.vbvCurrent = vbvCurrent;
            aspNew.vbvSource = vbvSource;

            aspNew.fReplace = fReplace;
            aspNew.nPrevInPos = nInPos;
            aspNew.vbvForValue = vbvForValue;
            aspNew.nNewPos = nNewPos;

            if (fTrace)
            {
                Write(" push ");
                WriteFrame(aspNew);
            }

            aspTop = aspNew;
            return KBool.kFalse;
        }

        protected bool fMatchVbvForValue(Vbv vbvForValue, out Asb asbNew, out Vbv vbvNew, ref int nInPos)
        {
            // vbvSource = null;  // turn off map back from source
            rgnNewIdTable = Nvi.rgnIdTableForVbv(ref nviList, vbvKeyNorm(vbvForValue), true);
            if (vbvForValue == Vbv.vbvA)
            {
                asbNew = asbLeft;
                vbvNew = vbvLeft;
                vbvSource = Vbv.vbvA;
                return true;
            }
            else if (vbvForValue == Vbv.vbvB)
            {
                asbNew = asbRight;
                vbvNew = vbvRight;
                vbvSource = Vbv.vbvB;
                return true;
            }
            else
            {
                asbNew = null;
                vbvNew = null;
                vbvSource = null;
                return false;
            }
        }


        KBool kPushValue(Vbv vbvForValue, int nTermSize, int nNewPos, ref int nEndPos, bool fReplace)
        {
            if (kPushState(nTermSize, nEndPos, fReplace, vbvForValue, nNewPos) == KBool.kFailed)
                return KBool.kFailed;
            Asb asbNew;
            Vbv vbvNew;
            if (!fMatchVbvForValue(vbvForValue, out asbNew, out vbvNew, ref nInPos))
            {
                asbNew = vbvForValue.asb;
                // rgnNewIdTable = rgnObtainIdTable(vbvForValue);  // done in fMatchVbvForValue
                vbvNew = vbvForValue;
                vbvSource = vbvKeyNorm(vbvForValue);
            }
            nInPos = nNewPos;
            nEndPos = nInPos + asbNew.nTermSize(nInPos);
            vbvCurrent = vbvNew;
            asbCurrent = asbNew;

#if DEBUG
            if (fTrace)
            {
                Write("now at nInPos: " + nInPos);
                Write(", vbvCurrent: " + vbvCurrent);
                Write(", asbSource: " + asbCurrent);
                WriteLine();
            }
#endif
            return KBool.kFalse;
        }
        protected virtual Vbv vbvIdCurrent()
        {
            if (vbvCurrent == null)
                return fRight ? Vbv.vbvB : Vbv.vbvA;
            else if (vbvCurrent == vbvLeft)
                return Vbv.vbvA;
            else if (vbvCurrent == vbvRight)
                return Vbv.vbvB;
            else
                return vbvCurrent;
        }

        /// <summary>
        /// Allocate a new nVblId
        /// </summary>
        protected sbyte nGetNextVblId(sbyte nIdFromInput)
        {
            sbyte nNewId = nNextVblId++;
            Vbv vbvIdCurrentL = vbvIdCurrent();
#if DEBUG
            Mva mvbPrev = mvbMapBackVblId;
            if (!(this is Sps))   // still true?
                        // was: because Mtp.MakeAtp false alarm. it thinks multiple terms are all vbvA
            {
                sbyte nVblIdOutput;
                // check if a nVblId is already assigned to nIdFromInput
                if (Mva.fMapSourceToOutput(mvbPrev, nIdFromInput, vbvIdCurrentL, out nVblIdOutput))
                    throw new ArgumentException();
            }

#endif
            Mva mvbNew = new Mva();
            mvbNew.tOutput = nNewId;
            mvbNew.tSource = nIdFromInput;
            mvbNew.vbvSource = vbvIdCurrentL;
            mvbNew.mvbNext = mvbMapBackVblId;
            mvbMapBackVblId = mvbNew;

            mvbNew.Validate();

            if (nNewId > nMaxOldVarId)
                nMaxOldVarId = nNewId;
            return nNewId;
        }

        public static void MapValue(Mob mobOffset, ushort nValue, out ushort nValueNew, out Vbv vbvSource)
        {
            if (nValue == Pmu.nNoReplace || mobOffset == null)
            {
                vbvSource = null; //  Vbv.vbvA;
                nValueNew = nValue;
            }
            else
            {
                mobOffset.Lookup(nValue, out nValueNew, out vbvSource);
            }
        }

        bool fSkipReplacement(Vbv vbv)
        {
            // Special case for when a vbl in a pti refers to location that triggers the rte.
            // TODO: Should consider more general cases also.
            if (aspTop != null
                && aspTop.vbvCurrent == vbv   // most recent push was from pti,
                                              // and an rte would not have been visible at that point
                && !aspTop.fReplace)
                return true;     // If changing this, also change Mrs.fSkipReplacement


            Asp aspPrev = null;
            for (Asp aspPlace = aspTop; aspPlace != null; aspPlace = aspPlace.aspPrev)
            {
                if (aspPlace.vbvCurrent == vbvCurrent    // not necessarily unique  (e.g. vbvLeft/vbvRight)
                     && aspPlace.asbSource == asbCurrent    // could be shared between pti, but pti gives new vbvCurrent
                     && aspPrev != null && aspPrev.vbvCurrent == vbv
                     && aspPlace.fReplace)
                    return true;
                aspPrev = aspPlace;
            }
            return false;
        }


        KBool kReplaceUsingPti(ref int nEndPos)
        {
            if (vbvCurrent == null)
                return KBool.kFalse;
            for (Vbv rto = vbvCurrent.vbvFirst; rto != null; rto = rto.vbvNext)
            {
                Vbv vbv = (Vbv)rto;

                if (vbv.nReplaceAtPosn == Pmu.nNoReplace)
                    continue;

                if (vbv == vbvSkip)
                    continue;

                ushort nReplaceAtSource;
                nReplaceAtSource = vbv.nReplaceAtPosn;

                if (nInPos != nReplaceAtSource)
                    continue;

#if false
                if (vbaJustExpanded != null)
                {
                    // Do not apply an pti if the vba that referred to the current position
                    // did not have that pti applied when it was created. This avoids a fixed
                    // point pti being applied just because a lower level vba referred to the
                    // start location.
                    if (rto.fVbaIsInterior(vbaJustExpanded))
                        continue;
                }
#endif

                if (fSkipReplacement(vbv))
                    continue;

                int nTermSize = asbCurrent.nTermSize(nInPos);
                int nNextInPos = nInPos + nTermSize;

#if false
                // does this preserve completeness?

                // check pti rule has already been applied at this location
                // TODO: how to know they are applied in the right order as in solution?(does it matter?)
                for (Asp aspCheck = aspTop; aspCheck != null; aspCheck = aspCheck.aspPrev)
                {
                    if (aspCheck.nInputPos == nNextInPos
                         && aspCheck.vbvCurrent == vbvCurrent)
                    {
                        fAlready = true;
                        break;
                    }
                }
                if (fAlready)
                    continue;
#endif

#if true
                // experiment to see how vbl pts to pti that needs to be applied
                if (!fCheckPti)
                {
                    // Allow special case where the pti from is a vbl that is being replaced.
                    // (Same as Vbv.fCheckConflictingReplaceAtPosn)
                    // Allow if vbl is the entire 'from' of the pti
                    if (vbaJustExpanded.vbvForValue != vbvCurrent)
                            // that means the vba is address the scope where vbv is, and the offset
                    {

                        fConflictFound = true;
                        return KBool.kFailed;
                        Write("would skip");
                        WriteLine();
                    }
                }
#endif

                if (kPushValue(vbv, nTermSize, vbv.nReplaceWithPosn, ref nEndPos, true) == KBool.kFailed)
                    return KBool.kFailed;
                return KBool.kTrue;
            }
            return KBool.kFalse;
        }

        protected KBool kPushFromInput (sbyte nIdFromInput, ref int nEndPos)
        {
            if (vbvCurrent != null)
            {
                // find current value of vblId in the Vbv that applies to asbCurrent 
                Vba vbaForVbl = vbvCurrent.vbaFind(nIdFromInput);   // Vba has already been converted in Etp.fProcessSolution
                if (vbaForVbl != null)
                {
                    // push to vbl sbst value stack
                    ushort nMappedOffset;
                    Vbv vbvValue;
                    if (vbaForVbl.vbvForValue.fNeedsMapping())
                        MapValue(mobToOffsetForChild, vbaForVbl.nValue, out nMappedOffset, out vbvValue);
                    else
                        nMappedOffset = vbaForVbl.nValue;
                    if (kPushValue(vbaForVbl.vbvForValue, 1, nMappedOffset, ref nEndPos, false) == KBool.kFailed)
                        return KBool.kFailed;
                    vbaJustExpanded = vbaForVbl;
                    return KBool.kTrue;
                }
            }
            return KBool.kFalse;
        }

        /// <summary>
        /// Find if vblId already assigned for the given in the current vbv, or assign new one
        /// </summary>
        protected sbyte nObtainVblId (sbyte nIdFromInput)
        {
            // check cache for this input
            sbyte nIdIdx = (sbyte)(nIdFromInput + nOrigin);
            sbyte nNewId = rgnNewIdTable[nIdIdx];
            if (nNewId == Tde.nNoVblId)
            {
                nNewId = nGetNextVblId(nIdFromInput);
                rgnNewIdTable[nIdIdx] = nNewId;
            }
            return nNewId;

        }

        protected virtual sbyte nMapVblIdToOutput(sbyte nIdFromInput, ref int nEndPos)
        {
            switch (kPushFromInput(nIdFromInput, ref nEndPos))
            {
                case KBool.kTrue:
                    return Tde.nNoVblId;   // tell caller to continue to use pushed valued
                case KBool.kFailed:
                    return Tde.nFailed;
                case KBool.kFalse:
                    break;
            }
            return nObtainVblId(nIdFromInput);
        }

        /// <summary>
        /// Perform the request copy to the temp output locations.
        /// </summary>
        public void ProcessParts()
        {
            // extract left then right part of this Atp, convert to new ids and move to rgnNew
            do
            {
                StartSide();
                vbaJustExpanded = null;

                int nEndPos = nInPos + nNumToProcess;
                while (true)
                {
                    if (nInPos >= nEndPos)
                    {
                        if (aspTop == null)
                            break;
#if DEBUG
                        if (fTrace)
                        {
                            Write(" pop  ");
                            WriteFrame(aspTop);
                        }
#endif
                        // pop from vbl sbst value stack
                        nInPos = aspTop.nInputPos;
                        nEndPos = aspTop.nEndPos;
                        asbCurrent = aspTop.asbSource;
                        rgnNewIdTable = aspTop.rgnNewIdSave;
                        vbvCurrent = aspTop.vbvCurrent;
                        vbvSource = aspTop.vbvSource;
                        aspTop = aspTop.aspPrev;

                        continue;
                    }

                    if (fPtiEnabled)
                    {
#if true
                        fCheckPti = true;
                        if (vbaJustExpanded != null)
                        {
                            if (vbaJustExpanded.nValue == nInPos
                                    && vbaJustExpanded.vbvForValue == vbvCurrent)
                                fCheckPti = false;
                        }
                        // if (fCheckPti)
#endif
                        switch (kReplaceUsingPti(ref nEndPos))
                        {
                            case KBool.kTrue:
                                continue;
                            case KBool.kFalse:
                                break;
                            case KBool.kFailed:
                                return;
                        }
                    }

                    sbyte nIdFromInput = asbCurrent.rgnTree[nInPos];
                    vbaJustExpanded = null;

#if DEBUG
                    if (fTrace)
                    {
                        Write(" nInPos=");
                        Write(nInPos.ToString());
                        Write(" asbCurrent=");
                        Write(asbCurrent.ToString());
                        Write(" is ");
                        Write(nIdFromInput.ToString());
                        WriteLine();
                    }
#endif

                    sbyte nNewId;
                    if (nIdFromInput >= Atp.nVar)
                    {
                        nNewId = nMapVblIdToOutput(nIdFromInput, ref nEndPos);
                        if (nNewId == Tde.nNoVblId)
                            continue;
                        if (nNewId == Tde.nFailed)
                            return;
                    }
                    else // not a vbl
                    {
                        nNewId = nGetConstSymbolId(nIdFromInput);
                    } // if (nIdFromInput >= nVar)

                    if (rgnMapBack != null)
                    {
                        byte nIfxVbvIn = Vie.nIdxForVbv(vbvSource, ref vieList);
                        rgnMapBack[nOutPosn] = (ushort) ((nIfxVbvIn << Vie.nBitsForOffset) + nInPos);
                    }
#if DEBUG
                    if (fTrace)
                    {
                        Write("out ");
                        Write(nNewId.ToString());
                        Write(" at ");
                        Write(nOutPosn.ToString());
                        WriteLine();
                    }

                    // WriteStack();

                    if (nOutPosn > nMaxTreeSize - 10)
                        fTrace = true;
#endif
                        rgnNewTree[nOutPosn++] = nNewId;
                    if (nOutPosn > nMaxTreeSize)
                        throw new ArgumentException();
                    nInPos++;
                }

            }
            while (fAgain);

        }

        protected abstract sbyte nGetConstSymbolId(sbyte nIdFromInput);

        /// <summary>
        /// Construct an Atp for the pair of children of each side at the position indicated by the Etp.
        /// Similar to Asc.ascSbst
        /// </summary>
        public Atp atpCreateOutput(out Moa mobToOffsetForChild, out Mva mvbMapBackVblIdForChild)
        {
            ProcessParts();
            if (fConflictFound)
            {
                mobToOffsetForChild = null;
                mvbMapBackVblIdForChild = null;
                return null;
            }

            int nNumSyms = Atp.nLsmId - nNextSymId;
            Lsm[] rglsmDataNew = new Lsm[nNumSyms];
            for (int nNewId = Atp.nLsmId; nNewId > nNextSymId; nNewId--)
                rglsmDataNew[Atp.nLsmId - nNewId] = rglsmTemp[Atp.nLsmId - nNewId];

            // for each position in result, what was input position. 
            // negative value means left side
            // TODO: make this alternating vbvLocalId and position in that source
            sbyte[] rgnTreeOut = new sbyte[nOutPosn];
            ushort[] rgnMapToOffsetForChild = new ushort[nOutPosn];
            for (int i = 0; i < nOutPosn; i++)
            {
                rgnTreeOut[i] = rgnNewTree[i];
                if (rgnMapBack != null)
                    rgnMapToOffsetForChild[i] = rgnMapBack[i];
            }
            mobToOffsetForChild = new Moa();
            mobToOffsetForChild.rgnSourceOffsetMap = rgnMapToOffsetForChild;
            mobToOffsetForChild.vieList = vieList;
            rgnMapBack = null;
            vieList = null;

            mvbMapBackVblIdForChild = mvbMapBackVblId;

            Atp atp = new Atp(rgnTreeOut, rglsmDataNew);
            // Debug.WriteLine(atp.ToString());
            return atp;

        }
    }

    /// <summary>
    /// Create atp from two terms using given Asb with vbv applied to each.
    /// </summary>
    public class Spl : Spb
    {
        protected int nState;


        public void Init(Asb asbLeft, ushort nOffsetLeft,
            Asb asbRight, ushort nOffsetRight,
            Vbv vbvLeft, Vbv vbvRight, bool fSeparateVblSet)
        {
            this.asbLeft = asbLeft;
            this.nOffsetLeft = nOffsetLeft;
            this.asbRight = asbRight;
            this.nOffsetRight = nOffsetRight;
            this.vbvLeft = vbvLeft;
            this.vbvRight = vbvRight;
            nState = 0;
            fAgain = true;  // two terms
            Init(fSeparateVblSet);
            rgnMapBack = new ushort[Ascb.nInitialBufferSize];
        }
        protected override void StartSide()
        {
            if (nState == 0)
            {
                asbCurrent = asbLeft;
                rgnNewIdTable = Nvi.rgnIdTableForVbv(nviList, Vbv.vbvA);
                nInPos = nOffsetLeft;
                nNumToProcess = asbCurrent.nTermSize(nOffsetLeft);
                vbvCurrent = vbvLeft;
                vbvSource = Vbv.vbvA;
                fRight = false;
                nState = 1;
            }
            else
            {
                rgnNewTree[Atp.nOffsetLeftSize] = (sbyte)(nOutPosn - Atp.nOffsetFirstTerm);
                asbCurrent = asbRight;
                rgnNewIdTable = Nvi.rgnIdTableForVbv(nviList, Vbv.vbvB);
                nInPos = nOffsetRight;
                nNumToProcess = asbCurrent.nTermSize(nOffsetRight);
                vbvCurrent = vbvRight;
                vbvSource = Vbv.vbvB;
                fRight = true;
                fAgain = false;
                nState++;
            }
        }


        protected override sbyte nGetConstSymbolId(sbyte nIdFromInput)
        {
            // check cache for this source
            sbyte nIdIdx = (sbyte)(nIdFromInput + nOrigin);
            sbyte nNewId = rgnNewIdTable[nIdIdx];
            if (nNewId == Tde.nNoVblId)
            {
                // assign id for output
                Lsm lsmToAdd = asbCurrent.rglsmData[Atp.nLsmId - nIdFromInput];
                nNewId = nOutLsm(lsmToAdd);
                rgnNewIdTable[nIdIdx] = nNewId;
            }
            return nNewId;
        }
    }

    /// <summary>
    /// Apply substitutions to a single term.
    /// </summary>
    public class Sps : Spb
    {
        Ascb ascbOut;

        Mva mvbMapForChild;       // map provided by caller.

        protected override void StartSide()
        {
            // rgnNewIdTable = Asp.rgnMakeNewId();   done in PerformSbst
        }

        protected override sbyte nGetConstSymbolId(sbyte nIdFromInput)
        {
            Lsm lsmSym = asbCurrent.rglsmData[Asb.nLsmId - nIdFromInput];
            int nIdOut = ascbOut.asy.nIdForLsm(lsmSym);
            return (sbyte)nIdOut;
        }

        public void Init(Ascb ascbOut, Mva mvbMapForChild)
        {
            fAgain = false;  // just one term
            base.Init(true);
            this.ascbOut = ascbOut;
            this.mvbMapForChild = mvbMapForChild;
        }

        public void SetupSoln(Asb asbLeft, Asb asbRight, Vbv vbvLeft, Vbv vbvRight)
        {
            this.asbLeft = asbLeft;
            this.asbRight = asbRight;
            this.vbvLeft = vbvLeft;
            this.vbvRight = vbvRight;
        }

        public void SetupSbst(ushort nOffsetTerm, Vbv vbvSource)
        {
            this.vbvSource = vbvKeyNorm(vbvSource);
            rgnNewIdTable = Nvi.rgnIdTableForVbv(ref nviList, this.vbvSource, true);
            if (vbvSource == Vbv.vbvB || vbvSource == vbvRight)
            {
                asbCurrent = asbRight;
                vbvCurrent = vbvRight;
            }
            else if (vbvSource == Vbv.vbvA || vbvSource == vbvLeft)
            {
                asbCurrent = asbLeft;
                vbvCurrent = vbvLeft;
            }
            else
            {
                asbCurrent = vbvSource.asb;
                vbvCurrent = vbvSource;
            }

            nInPos = nOffsetTerm;
            nNumToProcess = asbCurrent.nTermSize(nOffsetTerm);
        }

        /// <summary>
        /// Obtain or assign a output nVblIn to correspond to nIdFromInput in current context 
        /// </summary>
        protected override sbyte nMapVblIdToOutput(sbyte nIdFromInput, ref int nEndPos)
        {
            // Spl is used to find soln, so it does not have an mvb. Sps is used when this is a soln, and
            // so we need to map the vbls in the current asb to the vblIds used in the soln.

            sbyte nMappedVblId = nIdFromInput;
            if (vbvSource == Vbv.vbvA || vbvSource == Vbv.vbvB
                 || vbvSource == vbvLeft || vbvSource == vbvRight)
            {
                if (mvbMapForChild == null)
                {
                    nMappedVblId = nIdFromInput;
                }
                else if (!Mva.fMapSourceToOutput(mvbMapForChild, nIdFromInput, vbvSource, out nMappedVblId))
                {
                    // this vbl was not mapped originally
                    return nObtainVblId((sbyte)(nIdFromInput + Mva.nUnusedVblId(mvbMapForChild, vbvSource)));
                }

                switch (kPushFromInput(nIdFromInput, ref nEndPos))
                {
                    case KBool.kTrue:
                        return Tde.nNoVblId;   // tell caller to continue to use pushed valued
                    case KBool.kFailed:
                        return Tde.nFailed;
                    case KBool.kFalse:
                        return nObtainVblId(nIdFromInput);
                }

            }
            return base.nMapVblIdToOutput(nMappedVblId, ref nEndPos);
        }


        public void GetResult()
        { 


            for (int i = Atp.nOffsetFirstTerm; i < nOutPosn; i++)
            {
                ascbOut.AppendId(rgnNewTree[i]);
            }

        }

    }

    /// <summary>
    /// Specialize for doing an occurs check
    /// </summary>
    public class Spo : Sps
    {
        public sbyte nIdToWatchFor;
        public Vbv vbvToWatchFor;

        /// <summary>
        /// Check if the vbl we are looking for shows up
        /// </summary>
        protected override sbyte nMapVblIdToOutput(sbyte nIdFromInput, ref int nEndPos)
        {
            if (vbvCurrent == vbvToWatchFor
                && nIdFromInput == nIdToWatchFor)
            {
                fConflictFound = true;
                return Tde.nFailed;
            }
            sbyte nResult = base.nMapVblIdToOutput(nIdFromInput, ref nEndPos);
            return nResult;
        }

        /// <summary>
        /// Traverse entire solution looking for loops.
        /// Needs better performance, but see if it works for now.
        /// </summary>
        /// <returns></returns>
        public bool fCheckOccurs(Vbv vbvInput, Vba vba)
        {
            vbvToWatchFor = vbvInput;
            nIdToWatchFor = vba.nVblId;

            Ascb ascbOut = new Ascb();
            ascbOut.DummySizes();

            Mva mvbMapForChild = null;
            Init(ascbOut, mvbMapForChild);

            SetupSbst(vba.nValue, vba.vbvForValue);
            ProcessParts();

            return fConflictFound;
        }

        public bool fFindOccursTree(Vbv vbvInput)
        {
            for (Vba vba = vbvInput.rgvbaList; vba != null; vba = vba.vbaPrev)
            {
                if (fCheckOccurs(vbvInput, vba))
                    return true;
            }
            for (Vbv vbvChild = vbvInput.vbvFirst; vbvChild != null; vbvChild = vbvChild.vbvNext)
            {
                if (fFindOccursTree(vbvChild))
                    return true;
            }
            return false;
        }

    }

    /// <summary>
    /// Specialize Spl for validating atpToEquate on left with Pti(from/to) on right
    /// output is
    ///     =   atp-lhs atp-rhs  =  pti-from   pti-to
    /// </summary>
    public class Spv : Spl
    {
        Pti pti;
        sbyte nIdEquals;

        public Spv(Pti pti)
        {
            this.pti = pti;
        }

        protected override void StartSide()
        {
            if (nState < 2)
            {
                if (nState == 0)
                {
                    rgnNewIdTable = Nvi.rgnIdTableForVbv(nviList, Vbv.vbvA);
                    nInPos = nOffsetLeft;
                    nIdEquals = nOutLsm(Lsm.lsmEquals);
                    rgnNewTree[nOutPosn++] = nIdEquals;
                    asbCurrent = asbLeft;
                    vbvCurrent = vbvLeft;
                    fRight = false;
                }

                nNumToProcess = asbCurrent.nTermSize(nInPos);
                nState++;
            }
            else
            {
                // Process pti From term and then To term
                if (nState == 2)
                {
                    rgnNewTree[Atp.nOffsetLeftSize] = (sbyte)(nOutPosn - Atp.nOffsetFirstTerm);
                    rgnNewTree[nOutPosn++] = nIdEquals;
                    rgnNewIdTable = Nvi.rgnIdTableForVbv(nviList, Vbv.vbvB);
                    nInPos = pti.nFromOffset;
                    vbvCurrent = vbvRight;
                    asbCurrent = asbRight;
                    nState++;
                }
                else
                {
                    nInPos = pti.nToOffset;
                    fAgain = false;
                }
                nNumToProcess = asbCurrent.nTermSize(nInPos);
            }
        }
        protected override Vbv vbvIdCurrent()
        {
            if (nState < 2)
            {
                if (vbvCurrent == null)
                    return Vbv.vbvA;
            }
            if (vbvCurrent == null)
                return Vbv.vbvB;
            else if (vbvCurrent == vbvLeft)
                return Vbv.vbvA;
            else if (vbvCurrent == vbvRight)
                return Vbv.vbvB;
            return vbvCurrent;
        }
    }
}