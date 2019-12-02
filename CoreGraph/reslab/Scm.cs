using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab
{

    /// <summary>
    /// Base class for singletons that perform specialized comparisons of Lsx
    /// </summary>
    public abstract class Scm
    {
        private string stName;

        public static Scm scmSequent = new ScmSequent("sequent");
        public static Scm scmQuad = new ScmQuad("quad");
        public static Scm scmExpr = new ScmExpr("expr");
        public static Scm scmQuadExpr = new ScmQuadExpr("quadExpr");

        protected Scm(string stName)
        {
            this.stName = stName;
        }
        public abstract bool fSame(Lsx lsxA, Lsx lsxB, ref Sus sus);

        public bool fSame(Lsx lsxA, Lsx lsxB)
        {
            Sus susBase = Sus.susNone;
            return fSame(lsxA, lsxB, ref susBase);
        }

        public bool fSameNegated(Lsx lsxA, Lsx lsxB)
        {
            Sus susBase = Sus.susNone;
            return fSameNegated(lsxA, lsxB, ref susBase);
        }

        public bool fSameNegated(Lsx lsxA, Lsx lsxB, ref Sus sus)
        {
            if (lsxA is Lpr)
            {
                Lpr lprA = (Lpr)lsxA;
                if (lprA.lsxCar == Lsm.lsmNot && lprA.lsxCdr != Lsm.lsmNil)
                {
                    Lpr lprACdr = (Lpr)lprA.lsxCdr;
                    return fSame(lprACdr.lsxCar, lsxB, ref sus);
                }
            }
            if (lsxB is Lpr)
            {
                Lpr lprB = (Lpr)lsxB;
                if (lprB.lsxCar == Lsm.lsmNot && lprB.lsxCdr != Lsm.lsmNil)
                {
                    Lpr lprBCdr = (Lpr)lprB.lsxCdr;
                    return fSame(lprBCdr.lsxCar, lsxA, ref sus);
                }
            }
            return false;
        }

        public bool fSameInSeq(Lsx lsxA, Lpr lprSeq, bool fNegated)
        {
            Sus susBase = Sus.susNone;
            return fSameInSeq(lsxA, lprSeq, fNegated, ref susBase);
        }

        public bool fSameInSeq(Lsx lsxA, Lpr lprSeq, bool fNegated, ref Sus sus)
        {
            if (lprSeq == null)
                return false;
            Lpr lprPlace = lprSeq;
            while (true)
            {
                Sus susLocal = sus;
                if (fNegated)
                {
                    if (fSameNegated(lsxA, lprPlace.lsxCar, ref susLocal))
                    {
                        sus = susLocal;
                        return true;
                    }
                }
                else if (fSame(lsxA, lprPlace.lsxCar, ref susLocal))
                {
                    sus = susLocal;
                    return true;
                }
                if (lprPlace.lsxCdr == Lsm.lsmNil)
                    return false;
                lprPlace = (Lpr)lprPlace.lsxCdr;
            }
        }
        public bool fSameInSeq2(Lpr lprSeq, Lsx lsxB, bool fNegated, ref Sus sus)
        {
            if (lprSeq == null)
                return false;
            while (true)
            {
                Sus susLocal = sus;
                if (fNegated)
                {
                    if (fSameNegated(lprSeq.lsxCar, lsxB, ref susLocal))
                    {
                        sus = susLocal;
                        return true;
                    }
                }
                else if (fSame(lprSeq.lsxCar, lsxB, ref susLocal))
                {
                    sus = susLocal;
                    return true;
                }
                if (lprSeq.lsxCdr == Lsm.lsmNil)
                    return false;
                lprSeq = (Lpr)lprSeq.lsxCdr;
            }
        }
        public bool fSameSet(Lsx lsxA, Lsx lsxB, bool fNegated, ref Sus sus, bool fResetSusInSet = false)
        {
            if (lsxA == Lsm.lsmNil)
                return (lsxB == Lsm.lsmNil);
            else if (lsxB == Lsm.lsmNil)
                return false;

            Lpr lprA = (Lpr)lsxA;
            Lpr lprB = (Lpr)lsxB;
            Sus susLocal = sus;

            while (true)
            {
                if (!fSameInSeq(lprA.lsxCar, lprB, fNegated, ref susLocal))
                    return false;

                if (lprA.lsxCdr == Lsm.lsmNil)
                    break;
                lprA = (Lpr)lprA.lsxCdr;
                if (fResetSusInSet)
                    susLocal = sus;
            }

            lprA = (Lpr)lsxA;
            while (true)
            {
                if (!fSameInSeq2(lprA, lprB.lsxCar, fNegated, ref susLocal))
                    return false;

                if (lprB.lsxCdr == Lsm.lsmNil)
                    return true;
                lprB = (Lpr)lprB.lsxCdr;
                if (fResetSusInSet)
                    susLocal = sus;
            }
        }
    }

    public class ScmQuadExpr : ScmExpr
    {
        public ScmQuadExpr(string stName) : base(stName) { }

        public override bool fSameCar(Lsx lsxA, Lsx lsxB, ref Sus sus)
        {
            if (lsxA == lsxB)
                return true;
            if (lsxA is Lsm && lsxB is Lsm)
            {
                Lsm lsmA = (Lsm)lsxA;
                Lsm lsmB = (Lsm)lsxB;
                if (lsmA.fSkolemFunction() && lsmA.fSkolemFunction())
                    return base.fSame(lsxA, lsxB, ref sus);
            }
            return fSame(lsxA, lsxB, ref sus);
        }
    }

    public class ScmExpr : Scm
    {
        public ScmExpr(string stName) : base(stName) { }
        public virtual bool fSameCar(Lsx lsxA, Lsx lsxB, ref Sus sus)
        {
            return lsxA == lsxB;
        }

        public override bool fSame(Lsx lsxA, Lsx lsxB, ref Sus sus)
        {
            if (!(lsxB is Lpr))
            {
                if (lsxB is Lsm)
                {
                    if (!(lsxA is Lsm))
                        return false;
                    Lsm lsmA = (Lsm)lsxA;
                    Lsm lsmB = (Lsm)lsxB;
                    if (lsmA.fVariable() || lsmA.fSkolemFunction())
                    {
                        if (lsmA.fVariable() && !lsmB.fVariable() && !lsmB.stName.StartsWith(Lsm.stVarPrefix))
                            return false;
                        Lsx lsxNewB;
                        if (!Sus.fFindValue(sus, lsmB, out lsxNewB))
                        {
                            if (sus == Sus.susNone)
                                return lsmA == lsmB;
                            if (sus == Sus.susVbl)
                                return lsmA == lsmB || lsmA.stName.Equals(lsmB.stName);
                            // allow alias between vbl and vbl or skolem and skolem
                            if (lsmA.fSkolemFunction() != lsmB.fSkolemFunction())
                                return false;
                            sus = new Sus(sus, lsmB, lsxA);
                            return true;
                        }
                        return fSame(lsxA, lsxNewB);
                    }
                    else if (lsxA == lsxB)
                       return true;
                    //else if (lsmA.fSkolemFunction() && lsmA.stName == lsmB.stName)
                    //    return true;
                    else
                        return false;
                }
                return (lsxA == lsxB);
            }
            if (lsxA is Lsm)
                return (lsxA == lsxB);

            Lpr lprA = (Lpr)lsxA;
            Lpr lprB = (Lpr)lsxB;
            Lsx lsxACar = lprA.lsxCar;
            if (!fSameCar (lsxACar, lprB.lsxCar, ref sus))
                return false;
            if (lsxACar == Lsm.lsmAnd
                || lsxACar == Lsm.lsmOr
                || lsxACar == Lsm.lsmList)
            {
                Lsx lsxARest = lprA.lsxCdr;
                Lsx lsxBRest = lprB.lsxCdr;
                if (!(lsxARest is Lpr))
                {
                    if (lsxBRest is Lpr)
                        return false;
                    return true;
                }
                else if (!(lsxBRest is Lpr))
                    return false;

                lprA = (Lpr)lsxARest;
                lprB = (Lpr)lsxBRest;

                return fSameSet(lsxARest, lsxBRest, false, ref sus);
            }
            else if (lsxACar == Lsm.lsmForall
                || lsxACar == Lsm.lsmExists)
            {
                Lsx lsxARest = lprA.lsxCdr;
                Lsx lsxBRest = lprB.lsxCdr;
                Lpr lprAR = (Lpr)lsxARest;
                Lpr lprBR = (Lpr)lsxBRest;
                Lsm lsmA = (Lsm)lprAR.lsxCar;
                Lsm lsmB = (Lsm)lprBR.lsxCar;
                Lpr lprAR2 = (Lpr)lprAR.lsxCdr;
                Lpr lprBR2 = (Lpr)lprBR.lsxCdr;
                Sus susLocal = sus;
                if (sus.fGrows)
                    susLocal = new Sus(susLocal, lsmB, lsmA);
                if (!fSame(lprAR2.lsxCar, lprBR2.lsxCar, ref sus))
                    return false;
                sus = susLocal;
                return true;
            }
            else
            {
                while (true)
                {
                    Lsx lsxARest = lprA.lsxCdr;
                    Lsx lsxBRest = lprB.lsxCdr;
                    if (!(lsxARest is Lpr))
                    {
                        if (lsxBRest is Lpr)
                            return false;
                        return true;
                    }
                    else if (!(lsxBRest is Lpr))
                        return false;
                    lprA = (Lpr)lsxARest;
                    lprB = (Lpr)lsxBRest;
                    if (!fSame(lprA.lsxCar, lprB.lsxCar, ref sus))
                        return false;
                }
            }
        }
    }
    public class ScmQuad : Scm
    {
        public ScmQuad(string stName) : base(stName) { }

        public override bool fSame(Lsx lsxA, Lsx lsxB, ref Sus sus)
        {
            if (lsxA == Lsm.lsmNil)
                return (lsxB == Lsm.lsmNil);
            else if (lsxB == Lsm.lsmNil)
                return false;

            Lpr lprA = (Lpr)lsxA;
            Lpr lprB = (Lpr)lsxB;

            if (!scmQuadExpr.fSameSet(lprA.lsxCar, lprB.lsxCar, false, ref sus))
                return false;
            if (!scmQuadExpr.fSameSet(lprA.lsxCdr, lprB.lsxCdr, false, ref sus))
                return false;

            return true;
        }
    }
    public class ScmSequent : Scm
    {
        public ScmSequent(string stName) : base(stName) { }

        public override bool fSame(Lsx lsxA, Lsx lsxB, ref Sus sus)
        {
            if (lsxA == lsxB)
                return true;
            return scmQuad.fSameSet(lsxA, lsxB, false, ref sus, fResetSusInSet: true);
        }
    }
}
