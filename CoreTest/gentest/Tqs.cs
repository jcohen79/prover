// OWNER: GJG
// #define DEBUG_PATH

using reslab;
using reslab.test;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace WBS
{
    public interface Iqs
    {
        KInitializeResult fInitialize(Ivd ivdData);

        object objCurrent(Imc imc);

        KVisitResult fMoveNext(Ivd ivdData);

    }

    /*
     * State of enumeration through possible rules.
     */
    public abstract class Tqs : Iqs
    {

        public readonly Tqs tqsParent;  // used to get node being built, and for arg for functional rgsdiValue


        public readonly Tqd tqdDef;

        public bool fLocked;
        public int nDepth;

        public Tqr tqrRandomizer;

        protected bool fInitial;

        protected Tqs(Tqs tqsParent, Tqd tqdDef)
        {
            this.tqsParent = tqsParent;
            this.tqdDef = tqdDef;
            fLocked = false;
            tqrRandomizer = null;
            fInitial = true;
            if (tqsParent != null)
                nDepth = tqsParent.nDepth;
        }

        /*
         * Advance to the next state.
         * Returns Stop if succeeded in getting a new state. (Continue means go to a neighbor or the parent and try there)
         */
        public virtual KVisitResult fMoveNext(Ivd ivdData) { return Accept(TqvMoveNext.tqvOnly, ivdData); }

        /*
         * Ready in the first state.
         * Returns Failed if failed in getting a first state.
         */
        public KInitializeResult fInitialize(Ivd ivdData)
        {
            KVisitResult kRes = Accept(TqvInitialize.tqvOnly, ivdData);
            return (kRes == KVisitResult.Stop) ? KInitializeResult.Failed : KInitializeResult.Succeeded;
        }


        public abstract object objCurrent(Imc imc);

        /*
         * Make it easy for sequences to be flattened instead of being lists of lists of lists.
         * Returns true if parent sequence should merge this list in.
         */
        public virtual bool fMergable() { return false; }


        public IEnumerable en_objGenerate(Ivd ivdData)
        {
            if (fInitialize(ivdData) == KInitializeResult.Failed)
                yield break;  // empty list is ok
            do
            {
                yield return objCurrent(ImcGet(ivdData));
            } while (fMoveNext(ivdData) == KVisitResult.Stop);
        }

        /*
         * traverse the tree using the visitor.
         * Returns true if caller can stop work on siblings of this.
         */

        public abstract KVisitResult Accept(Tqv tqvVisitor, Ivd ivdData);

        /**
         * Functions used by visitor classes to operate on this separate from its children.
         */
        public void DoLock() { fLocked = true; }
        public void DoUnlock() { fLocked = false; }
        public void DoRandomize(TqvRandomize tqv) { tqrRandomizer = tqv.tqr; }
        public void DoSequential() { tqrRandomizer = null; }

        /*
         * If this can try to get a new state independent of children, then override .
         */
        public virtual KVisitResult fDoMoveNext(Ivd ivdData)
        {
#if false
            if (fInitial)
            {
                fInitial = false;
                return KVisitResult.Stop;
            }
            // look elsewhere
#endif
            return KVisitResult.Continue;
        }

        /*
         * If this needs work independent of children, then override.
         * Returns true if failed in getting a first state.
         */
        public virtual KInitializeResult fDoInitialize(Ivd ivdData)
        {
            return KInitializeResult.Succeeded;
        }

        #region Utility

        protected static KVisitResult AcceptRgtqs<T>(Tqs tqsPlace, Tqv tqvVisitor, IList<T> rgtqsChildren, Ivd ivdData) where T : Tqs
        {
            for (int i = rgtqsChildren.Count - 1; i >= 0; i--)
            {
                if (rgtqsChildren[i].Accept(tqvVisitor, ivdData) == KVisitResult.Stop)
                // if (tqvVisitor.Visit(rgtqsChildren[i]) == KVisitResult.Stop)
                {
                    tqsPlace.ResetTqdha(ivdData);
                    if (CleanupRgtqs(tqvVisitor, rgtqsChildren, i + 1, ivdData) == KInitializeResult.Succeeded)
                        return KVisitResult.Stop;
                }
            }
            return KVisitResult.Continue;
        }

        /// <summary>
        /// Set flags to look like a moveNext has occured at top level.
        /// </summary>
        private void ResetTqdha(Ivd ivdData)
        {
            Tqs tqsAncestor = tqsParent;
            while (tqsAncestor != null)
            {
                if (tqsAncestor is Tqslr)
                {
                    Tqslr tqslr = (Tqslr)tqsAncestor;
                    foreach (Tqsha tqsgha in tqslr.rgtqsha)
                    {
                        tqsgha.fDoMoveNext(ivdData);
                    }
                    return;
                }
                tqsAncestor = tqsAncestor.tqsParent;
            }
        }


        protected static KInitializeResult CleanupRgtqs<T>(Tqv tqvVisitor, IList<T> rgtqsChildren, int iStart, Ivd ivdData) where T : Tqs
        {
            for (int j = rgtqsChildren.Count - 1; j >= iStart; j--)
                if (tqvVisitor.Cleanup(rgtqsChildren[j], ivdData) == KInitializeResult.Failed)
                    return KInitializeResult.Failed;
            return KInitializeResult.Succeeded;
        }
        #endregion

        protected static Imc ImcGet(Ivd ivdData)
        {
            return ivdData != null ? ivdData.imcGet() : null;
        }
    }

    public class Tqsv : Tqs
    {
        public Tqsv(Tqs tqsParent, Tqdv tqdDef) : base(tqsParent, tqdDef)
        {
        }


        public override KVisitResult Accept(Tqv tqvVisitor, Ivd ivdData)
        {
            return tqvVisitor.Visit(this, ivdData);
        }


        public override object objCurrent(Imc imc)
        {
            Debug.Assert(tqdDef != null);
            return ((Tqdv)tqdDef).objValue;
        }
    }

    /*
     * Consider a list of iterators and choose one at a time to iterate over.
     */
    public class Tqsa : Tqs
    {

        public Tqs tqsActive;

        protected int iPosition;

        private const int iInit = -1;

        public Tqsa(Tqs tqsParent, Tqd tqda)
            : base(tqsParent, tqda)
        {
            tqsActive = null;
            iPosition = iInit;
        }

        public override bool fMergable()
        {
            return tqsActive != null && tqsActive.fMergable();
        }


        public override KVisitResult Accept(Tqv tqvVisitor, Ivd ivdData)
        {
            if (tqsActive != null)
            {
                if (tqvVisitor.Visit(tqsActive, ivdData) == KVisitResult.Stop)
                    return KVisitResult.Stop;
            }
            return tqvVisitor.Visit(this, ivdData);
        }

        public override KVisitResult fDoMoveNext(Ivd ivdData)
        {
            Tqda tqda = (Tqda)tqdDef;
            Debug.Assert(tqda != null);
            while (iPosition + 1 < tqda.rgtqd.Length)
            {
                iPosition++;
                tqsActive = tqda.rgtqd[iPosition].TqsMake(this);
                if (tqsActive == null)
                    continue;
                if (tqsActive.fInitialize(ivdData) == KInitializeResult.Succeeded)
                    return KVisitResult.Stop;  // found one, stop
            }
            return KVisitResult.Continue;  // keep going at higher level. caller needs to initialize again to start over.
        }

        public override KInitializeResult fDoInitialize(Ivd ivdData)
        {
            iPosition = iInit;
            return fDoMoveNext(ivdData) == KVisitResult.Continue ? KInitializeResult.Failed : KInitializeResult.Succeeded;
        }


        public override object objCurrent(Imc imc)
        {
            Debug.Assert(tqsActive != null);
            return tqsActive.objCurrent(imc);
        }
    }

    public class Tqse : Tqs
    {
        private readonly List<Tqs> rgtqsChildren;

        public Tqse(Tqs tqsParent, Tqde tqde)
            : base(tqsParent, tqde)
        {
            rgtqsChildren = new List<Tqs>(tqde.rgtqd.Length);
            foreach (Tqd tqd in tqde.rgtqd)
                rgtqsChildren.Add(tqd.TqsMake(this));
        }

        public override KVisitResult fDoMoveNext(Ivd ivdData)
        {
            return KVisitResult.Continue;
        }

        public override bool fMergable()
        {
            return true;
        }


        public override KVisitResult Accept(Tqv tqvVisitor, Ivd ivdData)
        {
            if (AcceptRgtqs<Tqs>(this, tqvVisitor, rgtqsChildren, ivdData) == KVisitResult.Stop)
                return KVisitResult.Stop;
            return tqvVisitor.Visit(this, ivdData);
        }


        public override object objCurrent(Imc imc)
        {
            return en_objCurrent(imc);
        }


        public IEnumerable en_objCurrent(Imc imc)
        {
            foreach (Tqs tqsChild in rgtqsChildren)
                if (tqsChild.fMergable())
                    foreach (object objNested in (IEnumerable)tqsChild.objCurrent(imc))
                        yield return objNested;
                else
                    yield return tqsChild.objCurrent(imc);
        }
    }

    public class Tqsq : Tqs
    {
        // current location in stack of Tqs to find ones with quantifiers 
        private Tqs tqsPlace;

        public Tqsq(Tqs tqsParent, Tqdq tqdq)
            : base(tqsParent, tqdq)
        {
            tqsPlace = this;
        }

        public override KVisitResult Accept(Tqv tqvVisitor, Ivd ivdData)
        {
            return tqvVisitor.Visit(this, ivdData);
        }

        public override object objCurrent(Imc imc)
        {
            Tqdq tqdq = (Tqdq)tqdDef;
            Tqsl tqsl = (Tqsl)tqsPlace;
            Tqs tqsChild = tqsl.rgtqsChildren[tqdq.nChildPos];
            return tqsChild.objCurrent(imc);
        }

        public override KInitializeResult fDoInitialize(Ivd ivdData)
        {
            if (fDoMoveNext(ivdData) == KVisitResult.Stop)
                return KInitializeResult.Succeeded;
            return KInitializeResult.Failed;
        }

        public override KVisitResult fDoMoveNext(Ivd ivdData)
        {
            Tqdq tqdq = (Tqdq)tqdDef;
            if (tqsPlace != null)
            {
                tqsPlace = tqsPlace.tqsParent;
                while (tqsPlace != null)
                {
                    if (tqdq.fnfScopeFn(tqsPlace))
                        break;
                    tqsPlace = tqsPlace.tqsParent;
                }
                if (tqsPlace != null)
                    return KVisitResult.Stop;
            }
            return KVisitResult.Continue;  // keep going at higher level. caller needs to initialize again to start over.
        }
    }

    public class Tqsl : Tqs
    {

        protected readonly Tqs tqsOp;


        public List<Tqs> rgtqsChildren;

        public Tqsl(Tqs tqsParent, Tqdl tqdl)
            : base(tqsParent, tqdl)
        {
            if (tqsParent != null)
            nDepth = tqsParent.nDepth + 1;

            Tqd[] rgtqd = tqdl.rgtqd();
            rgtqsChildren = new List<Tqs>();
            for (int i = rgtqd.Length - 1; i >= 0; i--)
            {
                Tqd tqd = rgtqd[i];
                rgtqsChildren.Insert(0, tqd.TqsMake(this));
            }

            tqsOp = tqdl.tqdOp().TqsMake(this);   // after nested Tqs: is slower
        }

        protected KVisitResult Advance(Tqv tqvVisitor, Ivd ivdData)
        {
            if (AcceptRgtqs<Tqs>(this, tqvVisitor, rgtqsChildren, ivdData) == KVisitResult.Stop)
                return KVisitResult.Stop;

            if (tqsOp != null
                && tqsOp.Accept(tqvVisitor, ivdData) == KVisitResult.Stop
                && CleanupRgtqs(tqvVisitor, rgtqsChildren, 0, ivdData) == KInitializeResult.Succeeded)
                return KVisitResult.Stop;

            return KVisitResult.Continue;
        }

        public override KVisitResult Accept(Tqv tqvVisitor, Ivd ivdData)
        {
            KVisitResult kResult = Advance(tqvVisitor, ivdData);
            if (kResult == KVisitResult.Stop)
                return KVisitResult.Stop;

            return tqvVisitor.Visit(this, ivdData);
        }

        public override KVisitResult fDoMoveNext(Ivd ivdData)
        {
            return Advance(TqvMoveNext.tqvOnly, ivdData);
        }


        public override object objCurrent(Imc imc)
        {
            //if (imc != null)
            //    imc.CreateInstance(objTypeId, sdi);
            Lpr lprHead = new Lpr();
            Lpr lprTail = lprHead;

            object objOp = tqsOp.objCurrent(imc);
            lprHead.lsxCar = (Lsx)objOp;

            foreach (Tqs tqsChild in rgtqsChildren)
            {
                object objChild = tqsChild.objCurrent(imc);
                Sko.AddToResult((Lsx)objChild, ref lprHead, ref lprTail);
            }

            return lprHead;
        }
    }

    public class Tqslr : Tqsl
    {
        public readonly List<Tqsha> rgtqsha;

        public Tqslr(Tqs tqsParent, Tqdl tqdl, List<Tqsha> rgtqsha)
            : base(tqsParent, tqdl)
        {
            this.rgtqsha = rgtqsha;
        }
        public override KVisitResult fMoveNext(Ivd ivdData)
        {
            KVisitResult r = base.fMoveNext(ivdData);
            foreach (Tqsha tqsha in rgtqsha)
                tqsha.fDoMoveNext(ivdData);
            return r;
        }
    }

    public class Tqsha
    {
        public readonly Tqdha tqdha;
        public Tqsr tqsrLatestThisCycle; // slowest Tqsr created this cycle
        public Tqsr tqsrFastestFromBefore;  // updated when faster ones are removed. will point to latest one current cycle

        public Tqsha(Tqdha tqdha)
        {
            this.tqdha = tqdha;
        }

        public KInitializeResult fInitialize(Ivd ivdData)
        {
            if (tqdha.rgLsmSymbols.Length > 0)
            {
                return KInitializeResult.Succeeded;
            }
            return KInitializeResult.Failed;
        }
        public KVisitResult fDoMoveNext(Ivd ivdData)
        {
            if (tqsrLatestThisCycle != null)
                tqsrFastestFromBefore = tqsrLatestThisCycle;
            tqsrLatestThisCycle = null;
            return KVisitResult.Stop;
        }
    }

    public class Tqsr : Tqs
    {
        private int nNext;
        public int nMax;
        private Lsm lsmCurrent;
        public Tqdha tqdha;
        private Tqsha tqsha;

        private Tqsr tqsrFaster;
        private Tqsr tqsrSlower;

        static int cCount = 0;
        int nId = cCount++;

        public Tqsr(Tqs tqsParent, Tqdr tqdr)
            : base(tqsParent, tqdr)
        {
            tqdha = tqdr.tqdha;
            fFindTqsha();

            if (tqsha.tqsrLatestThisCycle != null)
            {
                tqsha.tqsrLatestThisCycle.tqsrSlower = this;
            }
            tqsrFaster = tqsha.tqsrLatestThisCycle;
            tqsha.tqsrLatestThisCycle = this;

            int nNewMax;
            if (tqsha.tqsrFastestFromBefore != null)
            {
                tqsha.tqsrFastestFromBefore.tqsrFaster = this;
                tqsrSlower = tqsha.tqsrFastestFromBefore;
                nNewMax = tqsha.tqsrFastestFromBefore.nMax;
                if (nNewMax < tqsha.tqdha.rgLsmSymbols.Length)
                {
                    if (tqsha.tqsrFastestFromBefore.nNext + 1 == nNewMax)
                        nNewMax++;
                }
                SetRemainingMax(this, nNewMax);
            }
            else
            {
                nMax = 1;
                nNewMax = 1;
                if (nNewMax < tqsha.tqdha.rgLsmSymbols.Length)
                    nNewMax++;
                SetRemainingMax(tqsrFaster, nNewMax);
            }
        }

        private void SetRemainingMax (Tqsr tqsrPrev, int nNewMax)
        {
            while (tqsrPrev != null)
            {
                tqsrPrev.nMax = nNewMax;
                tqsrPrev = tqsrPrev.tqsrFaster;
            }
        }

        private KInitializeResult fFindTqsha()
        {
            Tqs tqsAncestor = tqsParent;
            while (tqsAncestor != null)
            {
                if (tqsAncestor is Tqslr)
                {
                    Tqslr tqslr = (Tqslr)tqsAncestor;
                    foreach (Tqsha tqsghaFind in tqslr.rgtqsha)
                    {
                        if (tqsghaFind.tqdha == tqdha)
                        {
                            tqsha = tqsghaFind;
                            return KInitializeResult.Succeeded;
                        }
                    }
                    break;
                }
                tqsAncestor = tqsAncestor.tqsParent;
            }
            return KInitializeResult.Failed;
        }


        public override KInitializeResult fDoInitialize(Ivd ivdData)
        {
            nNext = 0;

            if (tqdha.rgLsmSymbols.Length == 0)
                return KInitializeResult.Failed;
            lsmCurrent = tqdha.rgLsmSymbols[0];
            return KInitializeResult.Succeeded;
        }

        public override object objCurrent(Imc imc)
        {
            if (imc is Smf)
            {
                Smf smf = (Smf)imc;
                return smf.lsmReplace(lsmCurrent);
            }
            return lsmCurrent;
        }

        public override KVisitResult fDoMoveNext(Ivd ivdData)
        {
            nNext++;
            if (nNext < nMax)
            {
                lsmCurrent = tqdha.rgLsmSymbols[nNext];

                if (tqsrFaster != null && (nNext + 1) == nMax)
                {
                    int nNewMax = nNext + 1;
                    if (nNewMax <= tqsha.tqdha.rgLsmSymbols.Length)
                        SetRemainingMax(tqsrFaster, nNewMax);
                }
                return KVisitResult.Stop;
            }
            if (nMax == 1)
            {
                // restart series of variables
                tqsha.tqsrFastestFromBefore = null;
                tqsha.tqsrLatestThisCycle = null;

                // tqsha.fDoMoveNext(null);
            }
            else
            {
                // remove this and faster from chain
                if (tqsrSlower != null)
                    tqsrSlower.tqsrFaster = null;
                tqsha.tqsrFastestFromBefore = tqsrSlower;
                tqsha.tqsrLatestThisCycle = null;
            }
            return KVisitResult.Continue;

        }

        public override KVisitResult Accept(Tqv tqvVisitor, Ivd ivdData)
        {
            return tqvVisitor.Visit(this, ivdData);
        }
    }

}
