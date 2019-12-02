using System;


namespace reslab
{
    public class Eval
    {
        private readonly Lsx lsxNegAssignments;
        private readonly Lsx lsxPosAssignments;
        private readonly Qtl qtlList;

        public Eval(Lsx lsxNegAssignments, Lsx lsxPosAssignments, Qtl qtlList)
        {
            this.lsxNegAssignments = lsxNegAssignments;
            this.lsxPosAssignments = lsxPosAssignments;
            this.qtlList = qtlList;
        }

        public Lsx lsxEval(Lsx lsxA, bool fNegate, Sus susDictionary)
        {
            if (!(lsxA is Lpr))
            {
                if (lsxA == Lsm.lsmFalse || lsxA == Lsm.lsmTrue)
                {
                    if (fNegate)
                        return (lsxA == Lsm.lsmFalse) ? Lsm.lsmTrue : Lsm.lsmFalse;
                    else
                        return lsxA;
                }
                if (fNegate)
                    throw new ArgumentException();   // type checking would be better. vars are not used
                Lsx lsxValue;
                if (Sus.fFindValue(susDictionary, lsxA, out lsxValue))
                    return lsxValue;
                return lsxA;
            }
            Lpr lprA = (Lpr)lsxA;
            Lsx lsxOp = lprA.lsxCar;
            Lsx lsxACdr = lprA.lsxCdr;
            if (lsxOp == Lsm.lsmNot)
                return lsxEvalNot(lsxACdr, fNegate, susDictionary);
            else if (lsxOp == Lsm.lsmForall || lsxOp == Lsm.lsmExists)
            {
                bool fExists = lsxOp == Lsm.lsmExists;
                return lsxEvalQuantifier(lsxACdr, lprA, fExists != fNegate, fNegate, susDictionary);
            }
            else if (lsxOp == Lsm.lsmOr || lsxOp == Lsm.lsmAnd)
                return lsxEvalConnective(lsxACdr, lsxOp == Lsm.lsmOr, fNegate, susDictionary);
            else if (lsxOp == Lsm.lsmList)
                return lsxEvalConnective(lsxACdr, fNegate, fNegate, susDictionary);
            else if (lsxOp == Lsm.lsmImplies)
                return lsxEvalImplies(lsxACdr, true, fNegate, susDictionary);
            else if (lsxOp == Lsm.lsmIff)
                return lsxEvalImplies(lsxACdr, false, fNegate, susDictionary);
            else if (lsxOp == Lsm.lsmEntails)
                return lsxEvalEntails(lsxACdr, false, fNegate, susDictionary);
            else
                return lsxEvalTerm(lprA, fNegate, susDictionary);
        }
        public Lsx lsxEvalNot(Lsx lsxACdr, bool fNegate, Sus susDictionary)
        {
            if (!(lsxACdr is Lpr))
                throw new ArgumentException();
            Lpr lprACdr = (Lpr)lsxACdr;
            return lsxEval(lprACdr.lsxCar, !fNegate, susDictionary);
        }
        private Lsx lsxEvalQuantifier(Lsx lsxACdr, Lpr lprA, bool fExists, bool fNegate, Sus susDictionary)
        {
            if (!(lsxACdr is Lpr))
                throw new ArgumentException();
            Lpr lprACdr = (Lpr)lsxACdr;
            if (!(lprACdr.lsxCdr is Lpr))
                throw new ArgumentException();
            if (!(lprACdr.lsxCar is Lsm))
                throw new ArgumentException();
            Lsm lsmVar = (Lsm)lprACdr.lsxCar;
            Lpr lprACddr = (Lpr)lprACdr.lsxCdr;
            Qtl qtlQuant = Qtl.fFindQuantifier(qtlList, lprA, fExists); // != fNegate
            if (qtlQuant == null)
                throw new ArgumentException();
            if (qtlQuant.lsxSubst == null)   // occurs when not used in Sko, so ignore the value here
                return lsxSkip();
           //  CheckQuantSubst(fExists, fNegate, qtlQuant, susDictionary);
            Sus susSub = new Sus(susDictionary, lsmVar, qtlQuant.lsxSubst);
            return lsxEval(lprACddr.lsxCar, fNegate, susSub);
        }

        Lsx lsxSkip()
        {
            return Lsm.lsmNil;
        }

        void CheckQuantSubst(bool fExists, bool fNegate, Qtl qtlQuant, Sus susDictionary)
        {
            Lsx lsxPosn;
            if (qtlQuant.lsxSubst == null)
                return;  // occurs when exists free var is not used
            if (fExists)
            {
                if (!(qtlQuant.lsxSubst is Lpr))
                    throw new ArgumentException();
                Lpr lprS = (Lpr)qtlQuant.lsxSubst;
                lsxPosn = lprS.lsxCdr;  // skip skolem predicate
            }
            else
            {
                if (!(qtlQuant.lsxSubst is Lsm))
                    throw new ArgumentException();
                return;
                // Lsm lsmS = (Lsm)qtlQuant.lsxSubst;
            }
            // check that number of args in skolem expr match free variables
            Sus sus = susDictionary;
            while (sus != null)
            {
                // TODO: adjust for rule on pg 145 that allows quantified vars to be shared
                if (sus.lsxReplace is Lsm)  // forall
                {
                    if (lsxPosn == Lsm.lsmNil)
                        throw new ArgumentException();
                    lsxPosn = ((Lpr)lsxPosn).lsxCdr;
                }
                sus = sus.susPrev;
            }
            if (lsxPosn != Lsm.lsmNil)
                throw new ArgumentException();
        }


        private Lsx lsxEvalConnective(Lsx lsxACdr, bool fOr, bool fNegate, Sus susDictionary)
        {
            if (!(lsxACdr is Lpr))
            {
                if (lsxACdr != Lsm.lsmNil)
                    throw new System.ArgumentException();
                return (fOr != fNegate) ? Lsm.lsmFalse : Lsm.lsmTrue;
            }
            Lpr lprNext = (Lpr)lsxACdr;
            while (true)
            {
                Lsx lsxValueLhs = lsxEval(lprNext.lsxCar, fNegate, susDictionary);
                if (fOr != fNegate)
                {
                    if (lsxValueLhs == Lsm.lsmTrue)
                        return lsmBool(true, false);   // was fNegate
                }
                else if (lsxValueLhs == Lsm.lsmFalse)
                    return lsmBool(false, false);  // was fNegate

                if (!(lprNext.lsxCdr is Lpr))
                    break;
                lprNext = (Lpr)lprNext.lsxCdr;
            }
            return lsmBool(!fOr, fNegate);
        }

        public static Lsm lsmBool (bool fVal, bool fNegate)
        {
            return (fVal != fNegate) ? Lsm.lsmTrue : Lsm.lsmFalse;
        }
#if false
        private Lsx lsxEvalImplies(Lsx lsxACdr, bool fImplies, bool fNegate, Sus susDictionary)
        {
            if (!(lsxACdr is Lpr))
                throw new System.ArgumentException();
            Lpr lprNext = (Lpr)lsxACdr;
            Lsx lsxValueLhs = lsxEval(lprNext.lsxCar, fNegate, susDictionary);
            if (!(lprNext.lsxCdr is Lpr))
                throw new System.ArgumentException();
            lprNext = (Lpr)lprNext.lsxCdr;
            Lsx lsxValueRhs = lsxEval(lprNext.lsxCar, fNegate, susDictionary);
            if (lsxValueLhs == Lsm.lsmNil || lsxValueRhs == Lsm.lsmNil)
                return Lsm.lsmNil;
            if (fImplies)
            {
                return lsmBool(lsxValueLhs == Lsm.lsmFalse || lsxValueRhs == Lsm.lsmTrue, fNegate);
            }
            else
            {
                return lsmBool(lsxValueLhs == lsxValueRhs, fNegate);
            }
        }
#else
        private Lsx lsxEvalImplies(Lsx lsxACdr, bool fImplies, bool fNegate, Sus susDictionary)
        {
            if (!(lsxACdr is Lpr))
                throw new System.ArgumentException();
            Lpr lprNext = (Lpr)lsxACdr;
            Lsx lsxLhs = lprNext.lsxCar;
            if (!(lprNext.lsxCdr is Lpr))
                throw new System.ArgumentException();
            lprNext = (Lpr)lprNext.lsxCdr;
            Lsx lsxRhs = lprNext.lsxCar;
            if (fImplies)
            {
                return lsxEvalImpliesAux(lsxLhs, lsxRhs, fNegate, susDictionary);
            }
            else
            {
                // do this twice to match what Sko does
                Lsx lsxFirst = lsxEvalImpliesAux(lsxLhs, lsxRhs, fNegate, susDictionary);
                if (lsxFirst != Lsm.lsmTrue)
                    return lsxFirst;
                Lsx lsxSecond = lsxEvalImpliesAux(lsxRhs, lsxLhs, fNegate, susDictionary);
                return lsxSecond;
            }
        }

        private Lsx lsxEvalImpliesAux (Lsx lsxLhs, Lsx lsxRhs, bool fNegate, Sus susDictionary)
        { 
            Lsx lsxValueLhs = lsxEval(lsxLhs, !fNegate, susDictionary);
            Lsx lsxValueRhs = lsxEval(lsxRhs, fNegate, susDictionary);
            if (!fNegate)
            {
                if (lsxValueLhs == Lsm.lsmTrue && lsxValueRhs == Lsm.lsmTrue)
                    return Lsm.lsmTrue;
            }
            else if (fNegate)
            {
                if (lsxValueLhs == Lsm.lsmFalse || lsxValueRhs == Lsm.lsmFalse)
                    return Lsm.lsmTrue;
            }
            if (lsxValueLhs == Lsm.lsmNil || lsxValueRhs == Lsm.lsmNil)
                return Lsm.lsmNil;
            return lsmBool(false, fNegate);
        }
#endif

        public Lsx lsxEvalEntails(Lsx lsxACdr, bool fImplies, bool fNegate, Sus susDictionary, bool fFlipRhs = false)
        {
            if (!(lsxACdr is Lpr))
                throw new System.ArgumentException();
            Lpr lprNext = (Lpr)lsxACdr;
            Lsx lsxValueLhs;
            Lsx lsxValueRhs;
            Lsx lsxLhs = lprNext.lsxCar;
            if (lsxLhs is Lpr)
            {
                Lpr lprLhs = (Lpr)lsxLhs;
                if (lprLhs.lsxCar != Lsm.lsmList)
                    throw new System.ArgumentException();
                lsxValueLhs = lsxEvalConnective(lprLhs.lsxCdr, false, fNegate, susDictionary);
            }
            else
                lsxValueLhs = Lsm.lsmTrue;
            if (!(lprNext.lsxCdr is Lpr))
                throw new System.ArgumentException();
            lprNext = (Lpr)lprNext.lsxCdr;
            Lsx lsxRhs = lprNext.lsxCar;
            if (lsxRhs is Lpr)
            {
                Lpr lprRhs = (Lpr)lsxRhs;
                if (lprRhs.lsxCar != Lsm.lsmList)
                    throw new System.ArgumentException();
                lsxValueRhs = lsxEvalConnective(lprRhs.lsxCdr, !fFlipRhs, fNegate == fFlipRhs, susDictionary);
            }
            else
                lsxValueRhs = Lsm.lsmTrue;
            // Lsx lsxValueRhs = lsxEvalConnective(lprNext.lsxCar, true, fNegate, susDictionary);
            if (lsxValueLhs == Lsm.lsmFalse)
                return Lsm.lsmTrue;
            if (lsxValueRhs == Lsm.lsmTrue)   // was false
                return Lsm.lsmTrue;
            if (lsxValueLhs == Lsm.lsmNil || lsxValueRhs == Lsm.lsmNil)
                return Lsm.lsmNil;
            return Lsm.lsmFalse;
        }


        bool fFindTerm (Lsx lsxA, Lsx lsxAssignments)
        {
            if (lsxAssignments == Lsm.lsmNil)
                return false;
            Lpr lprNext = (Lpr)lsxAssignments;
            while (true)
            {
                if (lprNext.lsxCar.Equals(lsxA))
                    return true;

                if (!(lprNext.lsxCdr is Lpr))
                    break;
                lprNext = (Lpr)lprNext.lsxCdr;
            }
            return false;
        }

        private Lsx lsxSubstTerm(Lsx lsxA, Sus susDictionary)
        {
            if (lsxA is Lsm)
            {
                Lsm lsmA = (Lsm)lsxA;
                Lsx lsxValue;
                if (!Sus.fFindValue(susDictionary, lsmA, out lsxValue))
                    return lsmA;
                return lsxValue;
            }
            else
            {
                Lpr lprA = (Lpr)lsxA;
                Lsx lsxL = lsxSubstTerm(lprA.lsxCar, susDictionary);
                Lsx lsxR = lsxSubstTerm(lprA.lsxCdr, susDictionary);
                if (lsxL == lprA.lsxCar && lsxR == lprA.lsxCdr)
                    return lsxA;
                return new Lpr(lsxL, lsxR);
            }
        }

        private Lsx lsxEvalTerm(Lsx lsxA, bool fNegate, Sus susDictionary)
        {
            Lsx lsxT = lsxSubstTerm(lsxA, susDictionary);
            if (fFindTerm(lsxT, lsxNegAssignments))
                return fNegate ? Lsm.lsmTrue : Lsm.lsmFalse;
            if (fFindTerm(lsxT, lsxPosAssignments))
                return fNegate ? Lsm.lsmFalse : Lsm.lsmTrue;
            return Lsm.lsmNil;  // allow ignoring parts of logical exprs where the value is determined by another SAT assignment
        }
    }
}