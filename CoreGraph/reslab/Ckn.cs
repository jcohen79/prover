using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab
{
    /// <summary>
    /// Hold info needed to do a subsumes check. Instead of passing lots of parameters around.
    /// </summary>
    public class Cks
    {
        public Asc ascNew;
        public bool fOldSubsumesNew;
        public Aic aicNew;
        public int nOffsetOfMismatch;   // where embed stopped
        public Asc ascWithOffset;       // clause to which nOffsetOfMismatch bytes are shared with ascNew
        public Res res;
    }

    /// <summary>
    /// Node in a tree of clause keys: prefixes of the tree array
    /// Organize the list of clauses to make it faster to find ones that can subsume or be subsumed by a given clause.
    /// </summary>
    public class Ckn
    {
        public Asc ascWithPrefix;   // use this for prefix and the rglsmData to decode. If cknFirstExtension is null, then acsWithPrefix is the indexed asc
        public int nPrefixLength;      // length of prefix contained in rgnPrefix (starting at 0)
        public Ckn cknFirstExtension;   // first in series of extensions of this. First is the one with the same ascWithPrefix.
        public Ckn cknNextSibling;      // next extension of the predecessor to this

        public Ckn(Asc ascFirst)
        {
            this.ascWithPrefix = ascFirst;
        }

        public static Ckn cknRoot(Asc ascFirst)
        {
            Ckn cknNew = new Ckn(ascFirst);
            cknNew.nPrefixLength = ascFirst.rgnTree.Length;
            return cknNew;
        }

        private void SetAcs(Asc ascNew)
        {
            this.ascWithPrefix = ascNew;
        }

        /// <summary>
        /// Extend the tree of Ckn to include the given Asc.
        /// </summary>
        public void AddAsc(Asc ascNew)
        {
            Ckn cknPlace = this;
            while (true)
            {
                int nSharedPrefixLength = cknPlace.nCommonPrefix(cknPlace.ascWithPrefix, ascNew);
                if (cknPlace.cknFirstExtension == null || nSharedPrefixLength < cknPlace.nPrefixLength)  // assume ascNew is not already present
                {
                    // split this
                    Ckn cknOld = new Ckn(cknPlace.ascWithPrefix);
                    cknOld.cknFirstExtension = cknPlace.cknFirstExtension;
                    cknOld.nPrefixLength = cknPlace.nPrefixLength;
                    Ckn cknNew = new Ckn(ascNew);
                    cknNew.nPrefixLength = ascNew.rgnTree.Length;
                    cknOld.cknNextSibling = cknNew;

                    cknPlace.nPrefixLength = nSharedPrefixLength;
                    cknPlace.cknFirstExtension = cknOld;
                }
                else
                {
                    if (ascNew.rgnTree.Length == cknPlace.nPrefixLength)
                    {
                        // the ascNew has the same initial set of terms, but fewer of them. Replace the old one with the new one
                        cknPlace.SetAcs (ascNew);
                        cknPlace.cknFirstExtension = null; // all longer ones are subsumed
                        return;
                    }

                    // the prefix is shared with another ext
                    sbyte nNewByte = ascNew.rgnTree[cknPlace.nPrefixLength];
                    Lsm lsmNewNext = (nNewByte >= Asb.nVar) ? null : ascNew.rglsmData[Asb.nLsmId - nNewByte];
                    Ckn cknExt = cknPlace.cknFirstExtension;
                    while (cknExt != null)
                    {
                        Asc ascExt = cknExt.ascWithPrefix;

                        if (ascExt.rgnTree.Length > cknPlace.nPrefixLength)  // otherwise cknExt has nothing past prefix, so it has no extension
                        {

                            sbyte nExtByte = ascExt.rgnTree[cknPlace.nPrefixLength];
                            if (nNewByte >= Asb.nVar)
                            {
                                // if (nExtByte >= Asb.nVar)
                                if (nExtByte == nNewByte)
                                {
                                        cknPlace = cknExt;
                                    break;
                                }
                            }
                            else if (nExtByte >= Asb.nVar)
                            { }
                            else
                            {
                                Lsm lsmExtNext = ascExt.rglsmData[Asb.nLsmId - nExtByte];
                                if (lsmNewNext == lsmExtNext)
                                {
                                    cknPlace = cknExt;
                                    break;
                                }
                            }
                        }

                        cknExt = cknExt.cknNextSibling;
                    }
                    if (cknExt != null)   // cknPlace was updated
                        continue;   

                    Ckn cknNew = new Ckn(ascNew);
                    cknNew.nPrefixLength = ascNew.rgnTree.Length;
                    cknNew.cknNextSibling = cknPlace.cknFirstExtension.cknNextSibling;
                    cknPlace.cknFirstExtension.cknNextSibling = cknNew;  // keep cknFirstExtension the same
                }
                break;
            }
        }

        /// <summary>
        /// Determine the number of symbols in common.
        /// </summary>
        /// <param name="rgnA"></param>
        /// <param name="rgnA"></param>
        /// <returns></returns>
        int nCommonPrefix (Asc ascA, Asc ascB)
        {
            sbyte[] rgnA = ascA.rgnTree;
            sbyte[] rgnB = ascB.rgnTree;
            int nLenA = rgnA.Length;
            int nLenB = rgnB.Length;
            int nPosnFirstPosA = ascA.nPosnFirstPos();
            int nPosnFirstPosB = ascB.nPosnFirstPos();

            int nLenMin = (nLenA < nLenB) ? nLenA : nLenB;
            for (int nPos = Asb.nClauseLeadingSizeNumbers; nPos != nLenMin; nPos++)
            {
                if (nPos == nPosnFirstPosA || nPos == nPosnFirstPosB)
                {
                    if (nPosnFirstPosA != nPosnFirstPosB)
                        return nPos;
                }

                sbyte nByteA = rgnA[nPos];
                sbyte nByteB = rgnB[nPos];
                if (nByteA >= Asb.nVar || nByteB >= Asb.nVar)
                {
                    if (nByteB == nByteA)
                        continue;
                    return nPos;
                }

                Lsm lsmA = ascA.rglsmData[Asb.nLsmId - nByteA];
                Lsm lsmB = ascB.rglsmData[Asb.nLsmId - nByteB];
                if (lsmA != lsmB)
                    return nPos;
            }
            return nLenMin;
        }

        bool fEmbed(Asc ascOld, Asc ascNew, Res res)
        {
            Abt abt = new Abt(ascOld, ascNew, res, ascNoVar: ascNew);
            if (abt.fEmbed())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Return true if one of the existing Asc_ subsumes the ascNew
        /// </summary>
        /// <param name="ascNew"></param>
        /// <returns></returns>
        public Asc ascSubsumes(Asc ascNew, bool fOldSubsumesNew, Res res)
        {
            Cks cks = new Cks();
            cks.ascNew = ascNew;
            cks.aicNew = new Aic(ascNew);
            cks.fOldSubsumesNew = fOldSubsumesNew;
            cks.res = res;
            return ascSubsumesAux(cks);
        }

        public Asc ascSubsumesAux(Cks cks)
        {
            Ckn cknPlace = this;
            if (cknPlace.cknFirstExtension == null)
            {
                if (cks.fOldSubsumesNew)
                {
                    if (fEmbed(cknPlace.ascWithPrefix, cks.ascNew, cks.res))
                        return cknPlace.ascWithPrefix;
                }
                else
                {
                    if (fEmbed(cks.ascNew, cknPlace.ascWithPrefix, cks.res))
                        return cknPlace.ascWithPrefix;
                }
                return null;
            }
            Ckn cknExt = cknPlace.cknFirstExtension;
            while (cknExt != null)
            {
                if (cknExt.fCompatibleWithSubsumes(cknPlace, cks))
                {
                    Asc ascExt = cknExt.ascSubsumesAux(cks);
                    if (ascExt != null)
                        return ascExt;
                }

                cknExt = cknExt.cknNextSibling;
            }
            return null;
        }

        /// <summary>
        /// Determine if any Ckn starting from this extension can subsume/by ascNew
        /// </summary>
        bool fCompatibleWithSubsumes(Ckn cknPrev, Cks cks)
        {
            if (cks.fOldSubsumesNew)
            {
                if (cks.ascWithOffset != ascWithPrefix)
                {
                    Abt abt = new Abt(ascWithPrefix, cks.ascNew, cks.res, ascNoVar: cks.ascNew);
                    if (abt.fEmbed())
                    {
                        return true;
                    }
                    cks.nOffsetOfMismatch = abt.nMaxOffsetA;
                    cks.ascWithOffset = ascWithPrefix;
                }
                if (nPrefixLength <= cks.nOffsetOfMismatch)
                    return true;
                return false;
            }


            Aic aicOld = new Aic(ascWithPrefix);
            int nOldPos = Asb.nClauseLeadingSizeNumbers;   // start at beginning to skip terms
            int nNewPos = nOldPos;
            while (true)
            {
                if (nOldPos >= nPrefixLength)
                    return true;
                if (nNewPos >= cks.ascNew.rgnTree.Length)
                    return true;

                sbyte nOldByte = ascWithPrefix.rgnTree[nOldPos];
                sbyte nNewByte = cks.ascNew.rgnTree[nNewPos];
                if (nOldByte >= Asb.nVar)
                {
                    nOldPos++;
                    if (nNewByte >= Asb.nVar)
                    {
                        nNewPos++;
                        continue;  // doesn't try to figure out variables yet
                    }
                    if (!cks.fOldSubsumesNew)
                        return false;
                    // skip corresponding term in new (later: can check if there is conflict with earlier binding)
                    nNewPos = nNewPos + cks.aicNew.rgnTermSize[nNewPos];
                    continue;
                }
                if (nNewByte >= Asb.nVar)
                {
                    nNewPos++;
                    if (cks.fOldSubsumesNew)
                        return false;
                    nOldPos = nOldPos + aicOld.rgnTermSize[nOldPos];
                    continue;  // doesn't try to figure out variables yet
                }

                Lsm lsmNewNext = cks.ascNew.rglsmData[Asb.nLsmId - nNewByte];
                Lsm lsmOldNext = ascWithPrefix.rglsmData[Asb.nLsmId - nOldByte];
                if (lsmNewNext != lsmOldNext)
                    return false;
                nOldPos++;
                nNewPos++;
            }
        }

        public void Validate(Ckn cknParent)
        {
            if (cknParent != null)
            {
                Debug.Assert(cknParent.nPrefixLength < nPrefixLength);
            }
            Ckn cknExt = cknFirstExtension;
            while (cknExt != null)
            {
                cknExt.Validate(this);
                cknExt = cknExt.cknNextSibling;
            }
        }
    }
}
