
#define CHECK_SUBSUMES

using GraphMatching;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Text;

namespace reslab
{
    public abstract class Gnb : Bid
    {
        public readonly Res res;
        public Bas basCurrentLeft;
        public Bas basCurrentRight;
        public readonly Sprc sprcOwner;
        private Ipr iprPriorityCurrentLeft = Npr.nprOnly;
        private Ipr iprPriorityCurrentRight = Npr.nprOnly;
        public bool fRightAtCurrent = true;

        public static int nCount = 0;
        // static int nBadGnp = 256;
        public static int nBadGnp = -7;

        protected Gnb(Res res, Sprc sprcOwner)
        {
            this.res = res;
            this.sprcOwner = sprcOwner;
        }

        public Ipr iprPriority()
        {
            return iprPriorityCurrentLeft.iprMax (iprPriorityCurrentRight);
        }
        public Ipr iprPriority(bool fRight)
        {
            return fRight ? iprPriorityCurrentRight : iprPriorityCurrentLeft;
        }

        public void SetPriority(Ipr iprPriority, bool fRight)
        {
            if (fRight)
                iprPriorityCurrentRight = iprPriority;
            else
                iprPriorityCurrentLeft = iprPriority;
        }

        public abstract Bas babNew(bool fRight, Bas babPrev, Ipr iprSize);

        public abstract Bas babGet(bool fRight);

        public abstract Bas AddBabToFront(bool fRight, Bas babPrev);

        public abstract Asc ascIsSubsumed(bool fRight, Rib ascNew, bool fAdd);

        public abstract void Find(Ifs ifsSearch);

        public abstract void VerboseLefts();
        public abstract void VerboseRights();

        public abstract int nSize();
    }

    /// <summary>
    /// Generalized priority
    /// </summary>
    public abstract class Ipr
    {
        public abstract int nCompare(Ipr iprOther);

        public static int nPrnLevel = 0;
        public static int nPrpLevel = 1;
        public static int nNprLevel = 2;


        public abstract int nTypeLevel();

        protected int nLevelCompare(Ipr iprOther)
        {
            int nOther = iprOther.nTypeLevel();
            int nThis = nTypeLevel();
            if (nThis < nOther)
                return -1;
            if (nThis > nOther)
                return 1;
            return 0;
        }

        public Ipr iprMin(Ipr iprOther)
        {
            int nCmp = nCompare(iprOther);
            if (nCmp <= 0)
                return this;
            return iprOther;
        }

        public Ipr iprMax(Ipr iprOther)
        {
            int nCmp = nCompare(iprOther);
            if (nCmp >= 0)
                return this;
            return iprOther;
        }

        public bool fGreaterThan(Ipr iprOther)
        {
            int nCmp = nCompare(iprOther);
            return nCmp > 0;
        }

        public bool fLessThan(Ipr iprOther)
        {
            int nCmp = nCompare(iprOther);
            return nCmp < 0;
        }
    }

    /// <summary>
    /// Max (last) in priority
    /// </summary>
    public class Npr : Ipr
    {
        public static readonly Npr nprOnly = new Npr();

        protected Npr() { }

        public override int nTypeLevel()
        {
            return Ipr.nNprLevel;
        }

        public override int nCompare(Ipr iprOther)
        {
            if (iprOther == nprOnly)
                return 0;
            return 1;
        }
    }

    public class Prn : Ipr
    {
        public int nValue;
        static Dictionary<int, Prn> mpn_priCache = new Dictionary<int, Prn>();

        public static Prn prnZero = prnObtain(0);

        public override int nTypeLevel()
        {
            return Ipr.nPrnLevel;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Prn))
                return false;
            return nValue == ((Prn)obj).nValue;
        }

        public static Prn prnObtain(int n)
        {
            Prn prnOld;
            if (!mpn_priCache.TryGetValue(n, out prnOld))
            {
                prnOld = new Prn();
                prnOld.nValue = n;
                mpn_priCache.Add(n, prnOld);
            }
            return prnOld;
        }

        public override int nCompare(Ipr iprOther)
        {
            int nLevelCmp = nLevelCompare(iprOther);
            if (nLevelCmp != 0)
                return nLevelCmp;
            Prn prnOther = (Prn)iprOther;
            if (nValue < prnOther.nValue)
                return -1;
            if (nValue > prnOther.nValue)
                return 1;
            return 0;
        }

        public override string ToString()
        {
            return "<" + GetType().Name + " " + nValue + ">";
        }
    }

    public class Prp : Ipr
    {
        Ipr iprMain;
        Ipr iprSecond;

        public Prp (Ipr iprMain, Ipr iprSecond)
        {
            this.iprMain = iprMain;
            this.iprSecond = iprSecond;
        }

        public override int nTypeLevel()
        {
            return Ipr.nPrpLevel;
        }

        /// <summary>
        /// Return a value less than this.
        /// </summary>
        public static Prp prpBehind(Ipr iprOther)
        {
            System.Type typeOther = iprOther.GetType();
            if (typeOther != typeof(Prp))
            {
                Prp prpNew = new Prp(Prn.prnZero, iprOther);
                return prpNew;
            }
            else
            {
                Prp prpOther = (Prp)iprOther;
                if (!(prpOther.iprMain is Prn))
                    throw new NotImplementedException();
                Prn prnMain = (Prn)prpOther.iprMain;
                Prp prpNew = new Prp(Prn.prnObtain(prnMain.nValue + 1), prpOther.iprSecond);
                return prpNew;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Prp))
                return false;
            Prp prpOther = (Prp)obj;
            return iprMain.Equals(prpOther.iprMain) && iprSecond.Equals(prpOther.iprSecond);
        }


        public override int nCompare(Ipr iprOther)
        {
            int nLevelCmp = nLevelCompare(iprOther);
            if (nLevelCmp != 0)
                return nLevelCmp;

            Prp prpOther = (Prp)iprOther;
            int nMain = iprMain.nCompare(prpOther.iprMain);
            if (nMain != 0)
                return nMain;
            int nSecond = iprSecond.nCompare(prpOther.iprSecond);
            return nSecond;
        }

        public override string ToString()
        {
            return "<" + GetType().Name + " " + iprMain + ", " + iprSecond + ">";
        }

    }

    public interface Rib : Isi, Ibd
    {


        /// <summary>
        /// Objects are grouped by complexity in Bas. Complexity does not change.
        /// </summary>
        Ipr iprComplexity();
   }

    /// <summary>
    /// Get next pair of Asc for resolution. Uses lists of buckets by size
    /// </summary>
    public abstract class Gnp<TmemL, TmemR> : Gnb, Ipp<TmemL,TmemR>
        where TmemL : class, Rib
        where TmemR : class, Rib
    {
        public Bas basRights;
        public Bas basLefts;

        public Gnp(Res res, Sprc sprcOwner) : base(res, sprcOwner)
        {
        }

        public override Bas babGet(bool fRight)
        {
            return fRight ? basRights : basLefts;
        }
        public override Bas AddBabToFront(bool fRight, Bas babNew)
        {
            Bas babPrev;
            if (fRight)
            {
                babPrev = basRights;
                basRights = babNew;
                basCurrentRight = babNew;
            }
            else
            {
                babPrev = basLefts;
                basLefts = babNew;
                basCurrentLeft = babNew;
            }
            Ipr iprSize = babNew.iprSize;
            if (iprSize.fLessThan(iprPriority()))
                SetPriority(iprSize, fRight);
            return babPrev;
        }

        public override int nSize()
        {
            int nLeft = 0;
            int nRight = 0;
            for (Bas basLeft = basLefts; basLeft != null; basLeft = basLeft.basNext)
                nLeft += Bas.nNumClauses(basLeft);
            for (Bas basRight = basRights; basRight != null; basRight = basRight.basNext)
                nRight += Bas.nNumClauses(basRight);
            return nLeft * nRight;
        }

        public void AddToOneSide(Bas basForNewSize, bool fRight, Rib ribNew)
        {
            Bas basPrev;
            Ipr iprSizeNew = ribNew.iprComplexity();
            Bas.FindBasForSize(iprSizeNew, ref basForNewSize, out basPrev);
            Bas.AddToFiltered(fRight, this, ribNew, basForNewSize, basPrev, true);

            if (nId == nBadGnp)
                Debug.WriteLine("gnp add " + (fRight ? "R " : "L ") + iprSizeNew.ToString());
        }

        public abstract void AddToFiltered(bool fRight, Asc ascFiltered);



        public bool fGetNextPair(out TmemL ascLeft, out TmemR ascRight)
        {
            if (nId == nBadGnp)
                nCount++;
            //    Debug.WriteLine("\nenter gnp.fGetNextPair " + (fRightAtCurrent ? "R" : "L"));

            if (basCurrentLeft != null && basCurrentRight != null)
            {
                while (basCurrentLeft != null || basCurrentRight != null)
                {

                    // Compare strips at a time from the current Bas up. 
                    // Current bas goes back down when new bel is added.

                    Bas basCurrentSide = fRightAtCurrent ? basCurrentRight : basCurrentLeft;
                    if (basCurrentSide == null)
                        continue;
                    basCurrentSide.fPerformFilter();

                    Bel belLeft;
                    Bel belRight;
                    Bas basOther;
                    bool fNewFiltered;
                    // process a strip, starting from start of other bas to the current position
                    Pqi pqiFound = basCurrentSide.pqiGetPairFromBas(basCurrentSide.iprSize, fRightAtCurrent, fRightAtCurrent, 
                                                          out belLeft, out belRight, out basOther, out fNewFiltered);
                    if (pqiFound != null)
                    {
                        ascLeft =  (TmemL)belLeft.ribVal;
                        ascRight = (TmemR)belRight.ribVal;
                        if (res != null && res.irr != null)
                            res.irr.GotPair(this, belLeft, belRight, pqiFound);
                        if (fNewFiltered && fRightAtCurrent)
                            fRightAtCurrent = !fRightAtCurrent;   // go back and 

                        if (nId == nBadGnp)
                            Debug.WriteLine("exit T gnp.fGetNextPair "
                                   + (fRightAtCurrent ? "R " : "L ") + nCount + " "
                                   + ascLeft.iprComplexity() + " " + ascRight.iprComplexity());
                        return true;
                    }

                    // don't skip past the point where basOther stopped
                    Bas basNextLeft = fRightAtCurrent ? basOther : basCurrentLeft.basNext;
                    Bas basNextRight = fRightAtCurrent ? basCurrentRight.basNext : basOther;
                    if (basNextLeft == null)
                    {
                        if (basNextRight == null)
                            break;
                        if (fRightAtCurrent
                             || !basCurrentSide.iprSize.fGreaterThan(basNextRight.iprSize))
                        {
                            fRightAtCurrent = true;
                            basCurrentRight = basNextRight;
                            SetPriority(basCurrentRight.iprSize, true);
                            continue;
                        }
                        break;
                    }
                    Ipr iprNextLeft = basNextLeft.iprSize;
                    if (basNextRight == null)
                    {
                        if ((!fRightAtCurrent)
                             || basCurrentSide.iprSize.fLessThan(basNextLeft.iprSize))
                        {
                            fRightAtCurrent = false;
                            basCurrentLeft = basNextLeft;
                            SetPriority(basCurrentLeft.iprSize, false);
                            continue;
                        }
                        break;
                    }
                    Ipr iprNextRight = basNextRight.iprSize;

                    fRightAtCurrent = iprNextLeft.fGreaterThan(iprNextRight);
                    SetPriority (fRightAtCurrent ? iprNextRight : iprNextLeft, fRightAtCurrent);
                    if (fRightAtCurrent)
                        basCurrentRight = basNextRight;
                    else
                        basCurrentLeft = basNextLeft;
                }
            }
            ascLeft = null;
            ascRight = null;
            if (nId == nBadGnp)
                Debug.WriteLine("exit F gnp.fGetNextPair " + (fRightAtCurrent ? "R " : "L " + nCount));
            return false;
        }

        /// <summary>
        /// Perform a single step. Return false if no step found. 
        /// </summary>
        public bool fPerformStep()
        {
            TmemL ascLeft;
            TmemR ascRight;
            if (!fGetNextPair(out ascLeft, out ascRight))
            {
                return false;
            }
            ProcessPair(ascLeft, ascRight);

            return true;
        }

        public abstract void ProcessPair(TmemL ascLeft, TmemR ascRight);

        /// <summary>
        /// for testing
        /// </summary>
        public override void Find(Ifs ifsSearch)
        {
            bool fRight = ifsSearch.fRight();
            if (fRight)
               Bas.Find(ifsSearch, basRights);
            else
               Bas.Find(ifsSearch, basLefts);
        }

        public bool fFind(Asc ascTerm)
        {
            Ffs ffsFinder = new Ffs(ascTerm);
            Find(ffsFinder);
            return ffsFinder.fWasFound();
        }

#if CHECK_SUBSUMES

        public Asc ascSubsumed(bool fRight, Asc ascNew, out Bab basSize, out Bab basPrev)
        {
            basSize = null;
            basPrev = null;
            Ipr nLen = ascNew.iprComplexity(); // rgnTree.Length;
            Bab basPlace = fRight ? (Bab)basRights : basLefts;
            while (basPlace != null)
            {
                if (!nLen.fLessThan(basPlace.iprSize))
                {
                    if (nLen == basPlace.iprSize)
                    {
                        basSize = basPlace;
                    }
                    else
                        basPrev = basPlace;
                }
                Asc ascOld = basPlace.ascEmbedded(ascNew);
                if (ascOld != null)
                    return ascOld;
                basPlace = basPlace.babNext();
            }
            return null;
        }
#endif

        /// <summary>
        /// For testing
        /// </summary>
        public int nNumClauses()
        {
            return Bas.nNumClauses(basRights)
                + Bas.nNumClauses(basLefts);
        }

        public void ValidateSide(Bas basPlace)
        {
            Bas basPrev = null;
            while (basPlace != null)
            {
                basPlace.Validate(basPrev);
                basPrev = basPlace;
                basPlace = basPlace.basNext;
            }
        }

        public void Validate()
        {
            ValidateSide(basRights);
            ValidateSide(basLefts);
        }
        public override void VerboseRights()
        {
            Fwt sb = new Fwt();
            stString(Pctl.pctlLefts, sb);
            Debug.WriteLine(sb.stString());
        }
        
        public override void VerboseLefts()
        {
            Fwt sb = new Fwt();
            stString(Pctl.pctlRights, sb);
            Debug.WriteLine(sb.stString());
        }
        
        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append("<");
            sb.Append(GetType().Name);
            sb.Append("#");
            sb.Append(nId);
            if (pctl.fVerbose)
            {
                sb.Append(this.fRightAtCurrent ? " R" : "L");
                bool fRight = false;
                do
                {
                    sb.Newline();
                    sb.Indent();
                    sb.Append(fRight ? "Rights:" : "Lefts:");
                    sb.Newline();
                    for (Bas basList = fRight ? basRights : basLefts;
                         basList != null;
                         basList = basList.basNext)
                    {
                        if (fRight ? pctl.fRights : pctl.fLefts)
                        basList.stString(pctl, sb);
                        sb.Indent();
                        if (pctl.fContents)
                        {
                            for (Bel bel = basList.belFirstInBucket; bel != null; bel = bel.belNext)
                            {
                                sb.Newline();
                                bel.ribVal.stString(Pctl.pctlPlain, sb);    
                            }
                        }
                        else
                        {
                            // if (fRight)
                            {
                                for (Pqi pqiPlace = basList.pqiFirstInBucket;
                                     pqiPlace != null;
                                     pqiPlace = fRight ? pqiPlace.pqiNextInBucketSameRight
                                                       : pqiPlace.pqiNextInBucketSameLeft)
                                {
                                    sb.Newline();
                                    pqiPlace.stString(pctl, sb);
                                }
                            }
                        }
                        sb.Unindent();
                        sb.Newline();
                    }
                    sb.Unindent();
                    fRight = !fRight;
                }
                while (fRight);
            }
            sb.Append(">");
        }
    }

    /// <summary>
    /// Support searching for objects
    /// </summary>
    public interface Ifs
    {
        bool fRight();

        bool fCheckBas(Bas basHaystack);

        /// <summary>
        /// Return true to stop search.
        /// </summary>
        bool fCheck(Rib ribArg);
    }

    /// <summary>
    /// Visitor to find an object in Gnp
    /// </summary>
    public class Ffs : Ifs
    {
        protected Rib ribNeedle;
        protected bool fFound = false;

        public Ffs (Rib ribNeedle)
        {
            this.ribNeedle = ribNeedle;  
        }

        public virtual bool fCheck(Rib ribArg)
        {
            fFound = ribNeedle.Equals(ribArg);
            return fFound;
        }

        public virtual bool fRight()
        {
            if (ribNeedle is Pti)
                return true;
            else
                return ((Asc)ribNeedle).fRight();
        }

        public virtual bool fCheckBas(Bas basHaystack)
        {
            return (basHaystack.iprSize.Equals(ribNeedle.iprComplexity()));
        }

        public bool fWasFound()
        {
            return fFound;
        }
    }

    public abstract class Bab
    {
        public HashSet<Rib> mpascUnfiltered;
        public Ipr iprSize;
        protected bool fRight;
        public abstract Bab babNext();
        public abstract Asc ascEmbedded(Asc ascNew);
        public abstract void stString(Pctl pctl, Fwt sb);
    }

    /// <summary>
    /// List the items in a Bas
    /// </summary>
    public class Bel
    {
#if DEBUG
        static long cId = 0;
        public long nId = cId++;
#endif
        public Bel() { }

        public Bel belNext;
        public Rib ribVal;

        public override string ToString()
        {
            return "<" + GetType().Name
#if DEBUG
                + "#" + nId
#endif
                // + " " + ribVal.iprComplexity()
                + ">";
        }
    }

    /// <summary>
    /// Bucket Acs by size
    /// </summary>
    public abstract class Bas : Bab
    {
        public Bas basNext;
        public Pqi pqiFirstInBucket;
        public Bel belFirstInBucket;
        public Bel belLastInBucket;
        public Gnb gnp;

        protected Bas(Gnb gnp, bool fRight, Bab basPrev, Ipr iprSize)
        {
            this.gnp = gnp;
            this.fRight = fRight;
            this.iprSize = iprSize;

            if (basPrev != null)
            {
                Bas basPrevL = (Bas)basPrev;
                basNext =  basPrevL.basNext;
                basPrevL.basNext = this;
            }
            else
            {
                basNext = (Bas)gnp.AddBabToFront(fRight, this);
            }
        }

        public override Bab babNext()
        {
            return basNext;
        }

        public void Remove(Rib ribToRemove)
        {

            // TODO: not finished. Need to find the Pqi for the ribToRemove.
            // Pqi remove not tested.
            // when a rib is moved to another Bas/Pqi, it will repeat all that was done before. Save that info?

            Bel belPrev = null;
            for (Bel belPlace = belFirstInBucket; belPlace != null; belPlace = belPlace.belNext)
            {
                if (belPlace.ribVal == ribToRemove)
                {
                    Bel belNext = belPlace.belNext;
                    if (belPrev != null)
                        belPrev.belNext = belPlace.belNext;
                    else
                        belFirstInBucket = belNext;
                    if (belNext == null)
                        belLastInBucket = belPrev;
                    return;
                }
                belPrev = belPlace;
            }
            throw new ArgumentException();
        }

        public static void FindBasForSize(Ipr iprTermLen, ref Bas basPlace, out Bas basPrev)
        {
            basPrev = null;
            while (basPlace != null)
            {
                int nCmp = iprTermLen.nCompare(basPlace.iprSize);
                if (nCmp == 0)
                    return;
                if (nCmp < 0)
                {
                    basPlace = null;
                    return;
                }
                basPrev = basPlace;
                basPlace = basPlace.basNext;
            }
        }

        static void MakeIfNeeded(bool fRight, Gnb gnp, Ipr iprSize, 
                        ref Bas basSize, 
                        Bas basPrev, 
                        Rib ascNew)
        {
            // create bas if needed
            if (basSize == null)
            {
                Bab babNew = gnp.babNew(fRight, basPrev, iprSize);
                basSize = (Bas)babNew;
            }

        }

        /// <summary>
        /// Go through the unfiltered list and filter them: remove the ones that are subsumed by ones already in list.
        /// Return true if a clause passed the filter.
        /// </summary>
        public bool fPerformFilter()
        {
            if (mpascUnfiltered == null)
                return false;
            HashSet<Rib> mpLocal = mpascUnfiltered;
            mpascUnfiltered = null;
            bool fPassed = false;
            foreach (Asc ascUnfiltered in mpLocal)
            {
                if (gnp.ascIsSubsumed(fRight, ascUnfiltered, true) == null)
                {
                    gnp.res.AddToFiltered(fRight, ascUnfiltered);
                    fPassed = true;
                }
            }
            return fPassed;
        }

        public static void AdjustCurrent (bool fRight, Gnb gnp, Bas basSize, Ipr iprNewSize)
        {
            if (iprNewSize == null)
                return;

            Bas basCurrent = fRight ? gnp.basCurrentRight : gnp.basCurrentLeft;
            if (basCurrent == null
                     || iprNewSize.fLessThan(basCurrent.iprSize))
            {
                if (fRight)
                    gnp.basCurrentRight = basSize;
                else
                    gnp.basCurrentLeft = basSize;
                if (iprNewSize.fLessThan(gnp.iprPriority()))
                    gnp.SetPriority(iprNewSize, fRight);
            }
            if (fRight != gnp.fRightAtCurrent)
            {
                Bas basOther = fRight ? gnp.basCurrentLeft : gnp.basCurrentRight;
                bool fSwitch;
                if (basOther == null)
                    fSwitch = true;
                else if (fRight)
                    fSwitch = iprNewSize.fLessThan(basOther.iprSize);
                else
                    fSwitch = !iprNewSize.fGreaterThan(basOther.iprSize);
                if (fSwitch)
                {
                    gnp.fRightAtCurrent = fRight;
                    if (fRight)
                        gnp.basCurrentRight = basSize;
                    else
                        gnp.basCurrentLeft = basSize;
                    gnp.SetPriority(iprNewSize, fRight);
                }
            }
        }

        public void AddToBas (Rib ribNew)
        {
            Bel belNew = new Bel();
            belNew.ribVal = ribNew;

            if (belLastInBucket == null)
                belFirstInBucket = belNew;
            else
                belLastInBucket.belNext = belNew;
            belLastInBucket = belNew;
        }

        public static void AddToFiltered(bool fRight, Gnb gnp, Rib ribNew, Bas basSize, Bas basPrev, bool fFiltered)
        {
            Ipr iprNewSize = ribNew.iprComplexity();
            MakeIfNeeded(fRight, gnp, iprNewSize, ref basSize, basPrev, ribNew);

            if (fFiltered)
            {
                // de-dup here to avoid clutter. 
                // This is a linear scan because Qsc does the iteration linearly.
                // Maybe have to optimize this?
                for (Bel belPrev = basSize.belLastInBucket; belPrev != null; belPrev = belPrev.belNext)
                {
                    if (belPrev.ribVal.Equals(ribNew))
                        return;  // is redundant
                }
            }
            else
            {
                if (basSize.mpascUnfiltered == null)
                    basSize.mpascUnfiltered = new HashSet<Rib>();
                basSize.mpascUnfiltered.Add(ribNew);
            }

            AdjustCurrent(fRight, gnp, basSize, iprNewSize);

            if (fFiltered)
                basSize.AddToBas(ribNew);

            // make sure there is a pqi for each Bas on the other side (rights vs lefts)
            Bas basOtherSide = gnp.babGet(!fRight);
            Pqi pqiFromThisSide = basSize.pqiFirstInBucket;
            Pqi pqiPrev = null;
            while (basOtherSide != null)
            {
                if (pqiFromThisSide == null)
                {
                    pqiPrev = Pqi.pqiMake(fRight, basSize, basOtherSide, pqiPrev);
                    basOtherSide = basOtherSide.basNext;
                }
                else
                {
                    Bas basOtherSideOfPic = 
                        fRight ?
                            pqiFromThisSide.basLeft
                            : pqiFromThisSide.basRight;
                    if (basOtherSide.iprSize.fLessThan(basOtherSideOfPic.iprSize))
                    {
                        pqiFromThisSide = Pqi.pqiMake(fRight, basSize, basOtherSide, pqiPrev);
                        basOtherSide = basOtherSide.basNext;
                        continue;
                    }
                    if (basOtherSide == basOtherSideOfPic)
                    {
                        basOtherSide = basOtherSide.basNext;
                        pqiFromThisSide.Added(fRight);
                    }
                    pqiPrev = pqiFromThisSide;
                    pqiFromThisSide = fRight ? pqiFromThisSide.pqiNextInBucketSameRight : pqiFromThisSide.pqiNextInBucketSameLeft;
                }
            }
#if VALIDATE
            gnp.Validate();
#endif
        }

#if CHECK_SUBSUMES

        /// <summary>
        /// If an old Asc can be embedded into ascNew, then return that old Asc.
        /// </summary>
        public override Asc ascEmbedded(Asc ascNew)
        {
            for (Bel belOld = belFirstInBucket; belOld != null; belOld = belOld.belNext)
            {
                // if (gnp.fCheckSubsumes)
                {
                    Asc ascOld = (Asc)belOld.ribVal;
                    Abt abt = new Abt(ascOld, ascNew, gnp.res, ascNoVar: ascOld);
                    if (abt.fEmbed())
                    {
                        return ascOld;
                    }
                }
            }

            return null;
        }
#endif

        /// <summary>
        /// Compare a strip up to a point or a match is found
        /// </summary>
        public Pqi pqiGetPairFromBas(Ipr nMaxSize, bool fSameRight, bool fAllowTie,
                                       out Bel belLeft, out Bel belRight, out Bas basOther, out bool fNewFiltered)
        {
            Pqi pqiPlace = pqiFirstInBucket;
            basOther = null;
            fNewFiltered = false;
            while (pqiPlace != null)
            {
                basOther = fSameRight ? pqiPlace.basLeft : pqiPlace.basRight;
                Ipr nPqiSize = basOther.iprSize;
                int nComparison = nMaxSize.nCompare(nPqiSize);
                if (fAllowTie)
                {
                    if (nComparison < 0)
                        break;
                }
                else
                {
                    if (nComparison <= 0)
                        break;
                }

                if (basOther.fPerformFilter())
                {
                    fNewFiltered = true;
                }
                if (pqiPlace.fGetPairFromPqi(out belLeft, out belRight))
                {
                    return pqiPlace;
                }
                pqiPlace = fSameRight ? pqiPlace.pqiNextInBucketSameRight : pqiPlace.pqiNextInBucketSameLeft;
#if DEBUG
                if (fNewFiltered && pqiPlace != null)
                    throw new InvalidOperationException();
#endif
            }
            if (pqiPlace == null)
                basOther = null;
            belLeft = null;
            belRight = null;
            return null;
        }

        /// <summary>
        /// for testing
        /// </summary>
        public static void Find(Ifs ifsSearch, Bas basPlace)
        {
            for (; basPlace != null; basPlace = basPlace.basNext)
            {
                if (ifsSearch.fCheckBas(basPlace))
                {
                    basPlace.fPerformFilter();
                    for (Bel belPlace = basPlace.belFirstInBucket; belPlace != null; belPlace = belPlace.belNext)
                    {
                        if (ifsSearch.fCheck(belPlace.ribVal))
                            return;
                    }
                    return;
                }
                
            }
        }
        public static int nNumClauses(Bas basPlace)
        {
            int nCount = 0;
            for (; basPlace != null; basPlace = basPlace.basNext)
            {
                for (Bel belPlace = basPlace.belFirstInBucket; belPlace != null; belPlace = belPlace.belNext)
                {
                    nCount++;
                }
            }
            return nCount;
        }

        public int cCountBefore(Rib ribNeedle)
        {
            int nCount = 0;
            Bel belPlace = belFirstInBucket;
            while (belPlace != null && belPlace.ribVal.Equals(ribNeedle))
            {
                nCount++;
                belPlace = belPlace.belNext;
            }
            return nCount;
        }

        public void ValidateOne(Bab babOther, Bab babPrev)
        {
            Pqi pqi = pqiFirstInBucket;
            while (babOther != null)
            {
                Debug.Assert(babOther == (fRight ?
                    pqi.basLeft
                    : pqi.basRight));
                Debug.Assert(this == (fRight ?
                    pqi.basRight
                    : pqi.basLeft));
                Qsc qsbOther = fRight ? (pqi.qscLefts) : pqi.qscRights;
                Qsc qscOther = qsbOther;
                pqi = fRight ? pqi.pqiNextInBucketSameRight : pqi.pqiNextInBucketSameLeft;
                babOther = babOther.babNext();
            }
            Debug.Assert(pqi == null);
        }

        public void Validate(Bab basPrev)
        {
            Debug.Assert(basPrev == null || basPrev.iprSize.fLessThan(iprSize));
            ValidateOne(gnp.babGet(fRight), basPrev);
        }

        int cCountFiltered()
        {
            int cCount = 0;
            for (Bel belPlace = belFirstInBucket; belPlace != null; belPlace = belPlace.belNext)
            {
                cCount++;
            }
            return cCount;
        }

        public int cCountUnfiltered()
        {
            int cCount = 0;
            if (mpascUnfiltered != null)
            {
                foreach (var x in mpascUnfiltered)
                {
                    cCount++;
                }
            }
            return cCount;
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append("<");
            sb.Append(GetType().Name);
            sb.Append(fRight ? " R " : " L ");
            sb.Append(iprSize);
            if (pctl.fVerbose)
            {
                sb.Append(" filtered=");
                sb.Append(cCountFiltered());
                sb.Append(" un=");
                sb.Append(cCountUnfiltered());
            }
            sb.Append(">");
        }

        public override string ToString()
        {
            Fwt sb = new Fwt();
            stString(Pctl.pctlPlain, sb);
            return sb.stString();
        }
    }

    /// <summary>
    /// Each Bas has a list of elements. A Pqi has reference to a left and a right Bas.
    /// The pqi keeps track of pairing every Bel element of the left and right Bas.
    /// Each Pqi holds two Qsc to control the iteration through the lists of objects
    /// held on its two Bas.
    /// </summary>
    public class Pqi : Bid
    {
        protected bool fRightMajor = false;

        public Bas basLeft;
        public Bas basRight;

        public Qsc qscRights;
        public Qsc qscLefts;

        public Pqi pqiNextInBucketSameLeft;
        public Pqi pqiNextInBucketSameRight;

        private Pqi ()
        {
        }

        public static Pqi pqiMake(bool fRight,
                Bas basSize,
                Bas basOtherSide,
                Pqi pqiPrev)
        {
            Pqi pqi = new Pqi();
            if (fRight)
            {
                pqi.basLeft = basOtherSide;
                pqi.basRight = basSize;
                if (pqiPrev == null)
                    pqi.pqiNextInBucketSameRight = basSize.pqiFirstInBucket;
                else
                    pqi.pqiNextInBucketSameRight = pqiPrev.pqiNextInBucketSameRight;
            }
            else
            {
                pqi.basLeft = basSize;
                pqi.basRight = basOtherSide;
                if (pqiPrev == null)
                    pqi.pqiNextInBucketSameLeft = basSize.pqiFirstInBucket;
                else
                    pqi.pqiNextInBucketSameLeft = pqiPrev.pqiNextInBucketSameLeft;
            }
            if (pqiPrev == null)
                basSize.pqiFirstInBucket = pqi;
            else if (fRight)
                pqiPrev.pqiNextInBucketSameRight = pqi;
            else
                pqiPrev.pqiNextInBucketSameLeft = pqi;
            pqi.LinkOtherSide(fRight, basSize, basOtherSide);

            pqi.Setup();

#if false
            if (pqi.basLeft.gnp.nId == nGnpId)
            {
                Debug.WriteLine("Pqi Created " + (fRight ? "right" : "left"));
                pqi.Verbose();
            }
#endif

            return pqi;
        }

        void LinkOtherSide(bool fRight, Bas basSize, Bas basOtherSide)
        {
            Pqi pqiFind = basOtherSide.pqiFirstInBucket;
            if (pqiFind == null)
            {
                basOtherSide.pqiFirstInBucket = this;
            }
            else
            {
                Pqi pqiPrev = null;
                while (pqiFind != null)
                {
                    if (fRight)
                    {
                        if (pqiFind.basRight.iprSize.fGreaterThan (basSize.iprSize))
                            break;
                    }
                    else
                    {
                        if (pqiFind.basLeft.iprSize.fGreaterThan(basSize.iprSize))
                            break;
                    }
                    pqiPrev = pqiFind;
                    pqiFind = fRight ? pqiFind.pqiNextInBucketSameLeft : pqiFind.pqiNextInBucketSameRight;
                }


                if (fRight)
                    pqiNextInBucketSameLeft = (pqiPrev == null) ? basOtherSide.pqiFirstInBucket : pqiPrev.pqiNextInBucketSameLeft;
                else
                    pqiNextInBucketSameRight = (pqiPrev == null) ? basOtherSide.pqiFirstInBucket : pqiPrev.pqiNextInBucketSameRight;

                if (pqiPrev == null)
                    basOtherSide.pqiFirstInBucket = this;
                else if (fRight)
                    pqiPrev.pqiNextInBucketSameLeft = this;
                else
                    pqiPrev.pqiNextInBucketSameRight = this;
            }
        }

        public void Added(bool fRight)
        {
            if (fRight)
                qscRights.Added();
            else
                qscLefts.Added();
#if false
            if (basLeft.gnp.nId == nGnpId)
            {
                Debug.WriteLine("Pqi Added " + (fRight ? "right" : "left"));
                Verbose();
            }
#endif
        }


        public void Setup()
        {
            Irr irr = basLeft.gnp.res != null ? basLeft.gnp.res.irr : null;
            qscRights = new Qsc( this, true, "Rights", irr);
            qscLefts = new Qsc( this, false, "Lefts", irr);
        }

        int nCount = 0;
        static int nGnpId = 256;

        public bool fGetPairFromPqi(out Bel belLeft, out Bel belRight)
        {
#if false
            if (basLeft.gnp.nId == nGnpId)
            {
                nCount++;
                Debug.Write("  pqi getPair pqi#" + nId + " " + nCount);
                Verbose();
            }
#endif

            while (true)
            {
                Qsc qscMajor = fRightMajor ? qscRights : qscLefts;
                Qsc qscMinor = fRightMajor ? qscLefts : qscRights;
                Bel belMajor = qscMajor.belGet();
                if (belMajor != null)
                {
                    Bel belMinor = qscMinor.belNext();
                    if (belMinor != null)
                    {
                        belLeft = fRightMajor ? belMinor : belMajor;
                        belRight = fRightMajor ? belMajor : belMinor;
                        return true;
                    }
                }
                if (qscMinor.fEmpty())
                    break;
                if (!qscMajor.fNextMajor(qscMinor))
                {
                    if (qscMajor.fSwap(qscMinor))
                        fRightMajor = !fRightMajor;
                    else if (qscMajor.fContinue())
                        qscMajor.fNextMajor(qscMinor);
                    else
                        break;
                }
            }
            belLeft = null;
            belRight = null;
            return false;
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append("<");
            sb.Append(GetType().Name);
            sb.Append("#");
            sb.Append(nId);
            if (pctl.fVerbose)
            {
                sb.Append(fRightMajor ? " R" : " L");
                sb.Newline();
                sb.Indent();
                sb.Append("L=");
                qscLefts.stPrint(pctl, sb);
                sb.Newline();
                sb.Append("R=");
                qscRights.stPrint(pctl, sb);
                sb.Unindent();
            }
            sb.Append(">");
        }

    }

    /// <summary>
    /// Each Pqi has two Qsc.
    /// 
    /// Allow adding and removing Bel to a queue that is being iterated over.
    /// Used to walk along the twp growing edges of rectangle of pairs.
    /// Each Pqi has a pair of these.
    /// </summary>
        /*
        *         ^                        ^    last
        *         |                        |     
        *         +------------------+-----+--  stopAt
        *         |                  |     |
        *         |                  |     |
        *         +------------------+-----+--  current
        *         |                  |     |
        *         |                  |     |
        *         |                  |     |
        *         |                  |     |
        *         +------------------+-----+--  first
        */

    public class Qsc
    {
        protected string stName;
        protected Irr irr;
        protected bool fRight;
        private Bas basSide;

        Bel belCurrent;
        Bel belStopAt;

        public Qsc(Pqi pqi, bool fRight, string stName, Irr irr)
        {
            this.fRight = fRight;
            this.stName = stName;
            this.irr = irr;
            basSide = fRight ? pqi.basRight : pqi.basLeft;
        }

        Bel belFirst()
        {
            return basSide.belFirstInBucket;
        }

        Bel belLast()
        {
            return basSide.belLastInBucket;
        }

        public bool fEmpty()
        {
            return belFirst() == null;
        }

        public Bel belGet()
        {
            return belCurrent;
        }

        public bool fNextMajor(Qsc qscMinor)
        {
            if (null == belNext())
                return false;
            qscMinor.Reset();
            return true;
        }

        void Reset()
        {
            belCurrent = null;
        }

        /// <summary>
        /// Allow major to continue of minor cannot become major
        /// </summary>
        /// <returns></returns>
        public bool fContinue()
        {
            Bel belNextStop = belLast();
            if (belStopAt == belNextStop)
                return false;
            belStopAt = belNextStop;
            return true;
        }

        /// <summary>
        /// Major continues to the stopAt so that other side gets a chance before major continues.
        /// If the minor has more, configure this as minor and other as major
        /// </summary>
        /// <param name="qscMinor"></param>
        /// <returns></returns>
        public bool fSwap(Qsc qscOldMinor)
        {
            if (qscOldMinor.belLast() == qscOldMinor.belStopAt)
                return false;
            belStopAt = belCurrent;   // extending oldMinor, not oldMajor
            belCurrent = null;
            qscOldMinor.belStopAt = qscOldMinor.belLast();
            qscOldMinor.belNext();  // because next toop will get current for major
            return true;
        }

        public void StartMinor()
        {
            belCurrent = null;
        }

        public Bel belNext()
        {
            if (belCurrent == belStopAt)
                return null;
            Bel belNext = (belCurrent == null) ? belFirst() : belCurrent.belNext;
            if (belNext == null)
                return null;
            belCurrent = belNext;
            return belCurrent;
        }

        public void Added()
        {

        }

        public int cPosition(Bel belPlace)
        {
            int cPos = 0;
            for (Bel bel = belFirst(); bel != null; bel = bel.belNext)
            {
                if (belPlace == bel)
                {
                    return cPos;
                }
                cPos++;
            }
            return - cPos;
        }

        public void stPrint(Pctl pctl, Fwt sb)
        {
            sb.Append("<");
            sb.Append(GetType().Name);

            sb.Append(" ");
            sb.Append(stName);
            sb.Append(" ");
            sb.Append(fRight ? "R" : "L");
            sb.Append(" ");
            sb.Append(basSide.iprSize);
            sb.Append(" ");
            if (belCurrent != null)
                sb.Append(belCurrent.nId);
            else
                sb.Append("null");
            sb.Append(" ");
            if (belStopAt != null)
                sb.Append(belStopAt.nId);
            else
                sb.Append("null");
            sb.Append(">");
        }
    }
}