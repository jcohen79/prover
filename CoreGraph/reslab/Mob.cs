using System;
using System.Text;

namespace reslab
{
    public interface Imp
    {
        void MapVblId(sbyte nVblId, out sbyte nVblIdNew, out Vbv vbvOriginal);

        void MapValue(ushort nValue, out ushort nValueNew, out Vbv vbvForValue, Vbv vbvOutput, Vbv vbvPti);

        Vbv vbvPtiFromOutput(Vbv vbvPrev, Vbv vbvOutput);

        Asb asbGet();

        /// <summary>
        /// Determine if the nVblId vba needs a vbvUsedAtValue.
        /// This is used to prevent fixed point equations from being applied an extra time. 
        /// That occurs when a vba.nValue refers to a location where a vbv says to apply a pti, 
        /// but that may have been part of a different suproblem solution, and so pti should 
        /// not be applied.
        /// </summary>
        Vbv vbvGetUsedAtValue(sbyte nVblId, Vbv vbvOutput, Vbv vbvPti);
    }

    public enum KBool
    {
        kFalse,
        kTrue,
        kFailed
    }

    public class Mtb<Tv, Ts> where Tv : struct where Ts : Mtb<Tv, Ts>, new()
    {
        public Ts mvbNext;
        public Tv tSource;
        public Vbv vbvSource;
        public Tv tOutput;

        public virtual void Validate() { }

        public static bool fMapOutputToSource(Mtb<Tv, Ts> mvbMapBackChild, Tv tOutput, out Tv tSource, out Vbv vbvSource)
        {
            Mtb<Tv, Ts> mvbPrev = mvbMapBackChild;
            while (mvbPrev != null)
            {
                if (mvbPrev.tOutput.Equals(tOutput))
                {
                    tSource = mvbPrev.tSource;
                    vbvSource = mvbPrev.vbvSource;
                    return true;
                }
                mvbPrev = mvbPrev.mvbNext;
            }
            tSource = default(Tv); //  Tde.nNoVblId;
            vbvSource = null;
            return false;
        }

        public static bool fMapSourceToOutput(Mtb<Tv, Ts> mvbMapBackChild, Tv tSource, Vbv vbvSource, out Tv tOutput)
        {
            Mtb<Tv, Ts> mvbPrev = mvbMapBackChild;
            while (mvbPrev != null)
            {
                if (mvbPrev.tSource.Equals(tSource) && mvbPrev.vbvSource == vbvSource)
                {
                    tOutput = mvbPrev.tOutput;
                    return true;
                }
                mvbPrev = mvbPrev.mvbNext;
            }
            tOutput = default(Tv); //  Tde.nNoVblId;
            return false;
        }

        public static Ts mtbCopyList(Ts mvbMapBackChild)
        {
            if (mvbMapBackChild == null)
                return null;
            Ts mvbPrev = null;
            Ts mvbFirst = null;
            Ts mvbIn = mvbMapBackChild;
            while (mvbIn != null)
            {
                Ts mvbNew = new Ts();
                mvbNew.tSource = mvbIn.tSource;
                mvbNew.vbvSource = mvbIn.vbvSource;
                mvbNew.tOutput = mvbIn.tOutput;

                if (mvbPrev != null)
                    mvbPrev.mvbNext = mvbNew;
                else
                    mvbFirst = mvbNew;
                mvbPrev = mvbNew;
                mvbIn = mvbIn.mvbNext;

                mvbNew.Validate();
            }
            return mvbFirst;
        }

        public static void UpdateSource(Mtb<Tv, Ts> mvbToUpdate, Tv nOldVblId, Tv nNewVblId, bool fVbvIsSoln, Vbv vbvSource)
        {
            Mtb<Tv, Ts> mvbPrev = mvbToUpdate;
            while (mvbPrev != null)
            {
                if (mvbPrev.tSource.Equals(nOldVblId))
                {
                    if (mvbPrev.vbvSource == vbvSource
                     || (fVbvIsSoln && (mvbPrev.vbvSource.fNeedsMapping())))
                    {
                        mvbPrev.tSource = nNewVblId;
                        mvbPrev.Validate();
                        return;
                    }
                }
                mvbPrev = mvbPrev.mvbNext;
            }
            // vblId did not appear (was not in atp)
        }


        void AddToText(StringBuilder sb)
        {
            sb.Append(tOutput);
            sb.Append(":");
            sb.Append(tSource);
            // sb.Append("$");
            sb.Append(vbvSource);

            if (mvbNext != null)
            {
                sb.Append("+");
                mvbNext.AddToText(sb);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            AddToText(sb);
            return sb.ToString();
        }

    }


    /// <summary>
    /// Map nVblIds used in Atp.rgnTree to the Asc nVblId and vbv where solution are used
    /// </summary>
    public class Mva : Mtb<sbyte, Mva>
    {
        /// <summary>
        /// Return the lowest vblId that is not mapped to
        /// </summary>
        public static sbyte nUnusedVblId(Mva mvbPlace, Vbv vbvSource)
        {
            sbyte nVal = Atp.nVar;
            while (mvbPlace != null)
            {
                if (mvbPlace.vbvSource == vbvSource)
                {
                    if (mvbPlace.tOutput >= nVal)
                        nVal = (sbyte)(mvbPlace.tOutput + 1);
                }
                mvbPlace = mvbPlace.mvbNext;
            }
            return nVal;
        }

        public override void Validate()
        {
#if DEBUG
            //   if (vbvSource == Vbv.vbvB && tSource == 4 && tOutput == 4)
            //       Debug.WriteLine(this);
#endif
        }

    }

    /// <summary>
    /// Base class for Moa and Mof, to keep the usages clear.
    /// Each has a map to show correspondence between offsets mentioned in solution with contents of an Asb
    /// as it is transformed step by step. 
    /// Moa is updated and Vbv are merged into larger solution.
    /// Mof is updated as each substitution is performed during verification.
    /// Each entry in rgnSourceOffset maps an entry in rgnTree back to the nOffset and vbv where they originated
    /// </summary>
    public class Mob
    {
        public const ushort nNoOffset = ushort.MaxValue;

        // each entry has index into vieList to identify a vbv, and an offset within that clause
        //             ushort nNewEntry = (ushort)((nIfxVbvIn << Vie.nBitsForOffset) + nOffsetInOutput);
        public ushort[] rgnSourceOffsetMap;
        public Vie vieList;    // maps vbv <-> id

        public Mob() { }

        public void Lookup(ushort nOffset, out ushort nSourceOffset, out Vbv vbvSource)
        {
            ushort nEntry = rgnSourceOffsetMap[nOffset];
            nSourceOffset = (ushort)(nEntry & Vie.nMaskForOffset);
            vbvSource = Vie.vbvForIdx((byte)(nEntry >> Vie.nBitsForOffset), vieList);
        }

        public void MapBack(ushort nValue, Vbv vbvValue, out ushort nPosition)
        {
            byte nIfxVbvIn = Vie.nIdxForVbv(vbvValue, ref vieList);
            ushort nPrefix = (ushort)(nIfxVbvIn << Vie.nBitsForOffset);
            ushort nEntry = (ushort)(nPrefix + nValue);
            for (ushort nPos = Asc.nClauseLeadingSizeNumbers; nPos < rgnSourceOffsetMap.Length; nPos++)
            {
                if (rgnSourceOffsetMap[nPos] == nEntry)
                {
                    nPosition = nPos;
                    return;
                }
            }
            nPosition = nNoOffset;
        }

        public Mob mobCopy<T>() where T : Mob, new()
        {
            T mobNew = new T();
            mobNew.rgnSourceOffsetMap = new ushort[rgnSourceOffsetMap.Length];
            for (int n = 0; n < rgnSourceOffsetMap.Length; n++)
                mobNew.rgnSourceOffsetMap[n] = rgnSourceOffsetMap[n];
            mobNew.vieList = Vie.vieCopyList(vieList);

            return mobNew;
        }

        /// <summary>
        /// Scan the rgnSourceOffset part of this Mob (comes from gnMapBack in spl) to find the entry 
        /// that matches the given nOffsetInSource and vbvToUpdate. Then update that entry to the nOffsetInOutput
        /// </summary>
        public void UpdateChangedEntry(Mob mobToUpdate, ushort nOffsetInOutput, ushort nOffsetInSource, Vbv vbvToUpdate, bool fVbvIsSoln)
        {
            for (int nOffsetInPrev = Atp.nOffsetFirstTerm; nOffsetInPrev < rgnSourceOffsetMap.Length; nOffsetInPrev++)
            {
                ushort nEntry = rgnSourceOffsetMap[nOffsetInPrev];
                ushort nPrevOffset = (ushort)(nEntry & Vie.nMaskForOffset);
                Vbv vbvSource = Vie.vbvForIdx((byte)(nEntry >> Vie.nBitsForOffset), vieList);
                if (nOffsetInSource == nPrevOffset)
                {
                    if (vbvSource == vbvToUpdate
                        || (fVbvIsSoln && (vbvSource.fNeedsMapping())))
                    {
                        byte nIfxVbvIn = Vie.nIdxForVbv(vbvSource, ref vieList);
                        ushort nNewEntry = (ushort)((nIfxVbvIn << Vie.nBitsForOffset) + nOffsetInOutput);
                        mobToUpdate.rgnSourceOffsetMap[nOffsetInPrev] = nNewEntry;
                        return;
                    }
                }
            }
            // nothing to update
        }

    }

    /// <summary>
    /// Map nOffsets used in solution obtained using the atpToEquate.
    /// Each entry in rgnSourceOffset maps an entry in rgnTree back to the nOffset and vbv where they originated
    /// </summary>
    public class Moa : Mob
    {

    }

    /// <summary>
    /// Map offsets in a forward direction: Start with an identity mapping, and as each substitution is performed,
    /// save the new offset for each original offset in the starting Asc. This is used instead of Mob (used when
    /// equating an Atp) because Epu converts the solution to be relative to the input clauses.
    /// </summary>
    public class Mof : Mob
    {
        public Mof(Asc ascStart, byte nVbvId)
        {
            int nLen = ascStart.rgnTree.Length;
            rgnSourceOffsetMap = new ushort[nLen];
            for (int nPos = Asc.nClauseLeadingSizeNumbers; nPos < nLen; nPos++)
            {
                ushort nNewEntry = (ushort)((nVbvId << Vie.nBitsForOffset) + nPos);

                rgnSourceOffsetMap[nPos] = nNewEntry;
            }
        }
        public Mof(ushort[] rgnSourceOffsetMap, Vie vieList)
        {
            this.rgnSourceOffsetMap = rgnSourceOffsetMap;
            this.vieList = vieList;
        }
    }

    /// <summary>
    /// Assign ids to vbvs
    /// </summary>
    public class Vie
    {
        public const byte nIdxA = 0;
        public const byte nIdxB = 1;
        public const byte nIdxOther = 2;
        public const int nBitsForOffset = 12;
        public const ushort nMaskForOffset = (1 << nBitsForOffset) - 1;

        public byte nIdx;
        public Vbv vbvValue;
        public Vie vieNext;

        public static byte nIdxForVbv(Vbv vbvToEncode, ref Vie vieList)
        {
            if (vbvToEncode == Vbv.vbvA)
                return nIdxA;
            if (vbvToEncode == Vbv.vbvB)
                return nIdxB;

            Vie viePrev = vieList;
            while (viePrev != null)
            {
                if (viePrev.vbvValue == vbvToEncode)
                    return viePrev.nIdx;
                viePrev = viePrev.vieNext;
            }

            viePrev = new Vie();
            viePrev.nIdx = (vieList == null) ? nIdxOther : (byte)((vieList.nIdx + 1));
            viePrev.vbvValue = vbvToEncode;
            viePrev.vieNext = vieList;
            vieList = viePrev;
            return viePrev.nIdx;
        }

        public static Vbv vbvForIdx(byte nIdx, Vie vieList)
        {
            if (nIdx == nIdxA)
                return Vbv.vbvA;
            if (nIdx == nIdxB)
                return Vbv.vbvB;

            Vie viePrev = vieList;
            while (viePrev != null)
            {
                if (viePrev.nIdx == nIdx)
                    return viePrev.vbvValue;
                viePrev = viePrev.vieNext;
            }
            throw new ArgumentException();
        }

        public static Vie vieCopyList(Vie vieList)
        {
            if (vieList == null)
                return null;
            Vie viePrev = null;
            Vie vieFirst = null;
            Vie vieIn = vieList;
            while (vieIn != null)
            {
                Vie vieNew = new Vie();
                vieNew.nIdx = vieIn.nIdx;
                vieNew.vbvValue = vieIn.vbvValue;

                if (viePrev != null)
                    viePrev.vieNext = vieNew;
                else
                    vieFirst = vieNew;
                viePrev = vieNew;
                vieIn = vieIn.vieNext;
            }
            return vieFirst;
        }
    }

    /// <summary>
    /// Track the nVblIds assigned to each nVblId in the Vbvs in a solution
    /// </summary>
    public class Nvi
    {
        public Vbv vbvKey;   // vbvA, vbvB or a lower level Vbv
        public sbyte[] rgnIdTable;  // indexed by the nVblId, each entry is the nVblId used in output
        public Nvi nviNext;

        public static sbyte[] rgnIdTableForVbv(Nvi nviFirst, Vbv vbvKey)
        {
            for (Nvi nviPlace = nviFirst; nviPlace != null; nviPlace = nviPlace.nviNext)
            {
                if (nviPlace.vbvKey == vbvKey)
                    return nviPlace.rgnIdTable;
            }
            return null;
        }

        public static sbyte[] rgnIdTableForVbv(ref Nvi nviFirst, Vbv vbvKey, bool fCreate)
        {
            sbyte[] rgnResult = rgnIdTableForVbv(nviFirst, vbvKey);
            if (rgnResult != null)
                return rgnResult;
            if (fCreate)
            {
                sbyte[] rgnTable = Asp.rgnMakeNewId();
                nviAddNew(ref nviFirst, vbvKey, rgnTable);
                return rgnTable;
            }
            return null;
        }

        public static Nvi nviAddNew(ref Nvi nviFirst, Vbv vbvKey, sbyte[] rgnIdTable)
        {
            Nvi nviNew = new Nvi();
            nviNew.vbvKey = vbvKey;
            nviNew.rgnIdTable = rgnIdTable;
            nviNew.nviNext = nviFirst;
            nviFirst = nviNew;
            return nviNew;
        }
    }
}
