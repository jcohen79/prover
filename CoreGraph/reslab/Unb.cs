
using System.IO;

namespace reslab
{

    /// <summary>
    /// Base class for object to select the terms in the left and right clauses to use to perform resolution
    /// </summary>
    public abstract class Unb
    {
        public Abt abt;
        public ushort nOffsetA;
        public ushort nOffsetB;
        public Urc urc;

        public Unb(Abt abt, Urc urc)
        {
            this.abt = abt;
            this.urc = urc;
        }

        public abstract bool fMore();

        public abstract Unb unbCopy();

        public abstract uint nGetLeftFieldBits();
    }

    /// <summary>
    /// Unify a single term
    /// </summary>
    public class Unt : Unb
    {
        bool fFirst = true;

        public Unt(Abt abt, Urc urc) : base(abt, urc)
        {
        }

        public void Reset()
        {
            fFirst = true;
        }

        public override bool fMore()
        {
            if (fFirst)
            {
                fFirst = false;
                return true;
            }
            return false;
        }

        public override Unb unbCopy()
        {
            Unt unt = new Unt(abt, urc);
            unt.fFirst = fFirst;
            return unt;
        }

        public override uint nGetLeftFieldBits()
        {
            return 0;
        }

    }

    /// <summary>
    /// Iterate through terms for p1 resolution
    /// Test: unify multiple terms
    /// </summary>
    public class Up1 : Unb
    {
        uint nLeftFieldBits;
        uint nFlipState;
        uint nPosLeft = 0;
        bool fFirst = true;

        public Up1(Abt abt, Prh prh, uint nLeftFieldBits, uint nFlipState, bool fResolveNegSideRight) : base(abt, prh)
        {
            this.nLeftFieldBits = nLeftFieldBits;
            this.nFlipState = nFlipState;
            nPosLeft = (uint) (fResolveNegSideRight ? abt.avcA.aic.nNegLiterals : 0);
            if (abt.avcB != null)  // for testing
                nOffsetB = abt.avcB.aic.rgnTermOffset[abt.avcB.aic.asc.nResolveTerm];
        }
        private Up1(Abt abt, Prh prh, uint nLeftFieldBits, uint nPosLeft, uint nFlipState) : base(abt, prh)
        {
            this.nLeftFieldBits = nLeftFieldBits;
            this.nFlipState = nFlipState;
            this.nPosLeft = nPosLeft;
            nOffsetB = abt.avcB.aic.rgnTermOffset[abt.avcB.aic.asc.nResolveTerm];
        }

        public override uint nGetLeftFieldBits()
        {
            return nLeftFieldBits;
        }

        public override bool fMore()
        {
            // Step through the terms. For each one, the nFlipState will determine if it is flipped.
            while (true)
            {
                if (fFirst)
                    fFirst = false;
                else
                {
                    nPosLeft++;
                }
                uint nCurBit = (uint) (1 << (int) nPosLeft);
                if (nLeftFieldBits  < nCurBit)
                    return false;

                if ((nLeftFieldBits & nCurBit) != 0)
                {
                    nOffsetA = abt.avcA.aic.rgnTermOffset[nPosLeft];

                    return true;  // do next equate of nOffsetA and nOffsetB
                }
            }
        }

        public override Unb unbCopy()
        {
            Up1 up1 = new Up1(abt, (Prh)urc, nLeftFieldBits, nPosLeft,  nFlipState);
            up1.nPosLeft = nPosLeft;
            up1.fFirst = fFirst;
            return up1;
        }
    }

    /// <summary>
    /// Iterate through all terms in both left and right clauses.
    /// For each odd right position { for each odd left position { yield true }
    /// </summary>
    public class Usy : Unb
    {
        public readonly uint nLeftFieldBits;
        public readonly uint nRightFieldBits;
        bool fResolveNegSideRight;
        int nPosRight;
        KState kState = KState.kInit;
        int nPosLeft;
        uint nLeftFieldBitsLocal;
        uint nRightFieldBitsLocal;

        enum KState
        {
            kInit,
            kCheckLeft,
            kNextLeft,
            kNextRight,
        }

        public Usy(Abt abt, Prh prh, uint nLeftFieldBits, uint nRightFieldBits, bool fResolveNegSideRight) : base(abt, prh)
        {
            this.nLeftFieldBits = nLeftFieldBits;
            this.nRightFieldBits = nRightFieldBits;
            this.fResolveNegSideRight = fResolveNegSideRight;
            nPosRight = fResolveNegSideRight ? 0 : abt.avcB.aic.nNegLiterals;
            nRightFieldBitsLocal = nRightFieldBits;
        }

        public override uint nGetLeftFieldBits()
        {
            return nLeftFieldBits;
        }


        public override Unb unbCopy()
        {
            Usy usy = new Usy(abt, (Prh)urc, nLeftFieldBits, nRightFieldBits, fResolveNegSideRight);
            usy.nPosRight = nPosRight;
            usy.kState = kState;
            usy.nPosLeft = nPosLeft;
            usy.nLeftFieldBitsLocal = nLeftFieldBitsLocal;
            usy.nRightFieldBitsLocal = nRightFieldBitsLocal;
            return usy;
        }

        public override bool fMore()
        {
            while (true)
            {
                switch (kState)
                {
                    case KState.kInit:
                        if (nRightFieldBitsLocal == 0)
                            return false;
                        if ((nRightFieldBitsLocal & 1) == 0)
                        {
                            kState = KState.kNextRight;
                            continue;
                        }
                        nOffsetB = abt.avcB.aic.rgnTermOffset[nPosRight];

                        nPosLeft = fResolveNegSideRight ? abt.avcA.aic.nNegLiterals : 0;
                        nLeftFieldBitsLocal = nLeftFieldBits;
                        kState = KState.kCheckLeft;
                        break;

                    case KState.kCheckLeft:
                        if (nLeftFieldBitsLocal != 0)
                        {
                            kState = KState.kNextLeft;
                            if ((nLeftFieldBitsLocal & 1) != 0)
                            {
                                nOffsetA = abt.avcA.aic.rgnTermOffset[nPosLeft];
                                return true;
                            }
                        }
                        else
                            kState = KState.kNextRight;
                        break;

                    case KState.kNextLeft:
                        nPosLeft++;
                        nLeftFieldBitsLocal = nLeftFieldBitsLocal >> 1;
                        kState = KState.kCheckLeft;
                        break;

                    case KState.kNextRight:
                        nPosRight++;
                        nRightFieldBitsLocal = nRightFieldBitsLocal >> 1;
                        kState = KState.kInit;
                        break;
                    default:
                        throw new InvalidDataException();
                }
            }
        }
    }
}
