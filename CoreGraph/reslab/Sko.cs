
using GrammarDLL;
using System.Collections.Generic;
using GraphMatching;
using System;
using System.Diagnostics;

namespace reslab
{
    public class Qua : Lsx
    {
        public Qua quaInnerV;
        public Qua quaOuter;
        public Lsm lsmOldVar;
        public Lsx lsxSubst;
        public bool fExists;  // true if this is exists at top level
        public bool fReplaced = false;  // quaInner refers to qua to use instead
        private static int cNum = 0;
        private int nId = cNum++;
        public readonly Qtl qtl;
        public Lsm lsmQName;
        public int nQId = 1;
        Sko sko;

        public Qua quaInner
        {
            get { return quaInnerV; }
            set
            {
                Debug.Assert(this != value);
                quaInnerV = value;
            }
        }

        public Qua(Qua quaInner, Lsm lsmVar, bool fExists, Qtl qtl, Sko sko)
        {
            this.quaInner = quaInner;
            this.lsmOldVar = lsmVar;
            this.fExists = fExists;
            this.qtl = qtl;
            this.sko = sko;
        }

        public override bool fEqual(Lsx lsxArg)
        {
            if (lsxArg == this)
                return true;
            if (!(lsxArg is Qua))
                return false;
            if (fReplaced)
                return quaInner.fEqual(lsxArg);
            Qua quaArg = (Qua)lsxArg;
            if (quaArg.fReplaced)
                return fEqual(quaArg.quaInner);
            return false;
        }


        public override void Format(Fwt sb, Pctl pctl)
        {
            sb.Append("[");
            sb.Append(fExists ? "exists" : "forall");
            sb.Append(" ");
            sb.Append(lsmOldVar.stName);
            if (fReplaced)
            {
                sb.Append(" -> ");
                sb.Append(quaInner.lsmOldVar.stName);
            }
            sb.Append("]");
        }

        public int cExistsDepth()
        {
            Qua qua = this;
            int cDepth = 0;
            while (qua != null)
            {
                if (qua.fExists)
                    return cDepth;
                cDepth++;
                qua = qua.quaInner;
            }
            return int.MaxValue;
        }
        public Lsx lsxGetSubst(ref Sus susVarList)
        {
            Qua quaTerm = this;
            if (quaTerm.lsxSubst == null)
            {
                if (quaTerm.fExists)
                {
                    Lsm lsmSko;
                    string stQuaName;
                    if (quaTerm.lsmQName == null)
                        stQuaName = "Q_" + Sko.cGensymId++;
                    else
                        stQuaName= quaTerm.lsmQName.stName + quaTerm.nQId++;
                    lsmSko = sko.lparse == null ?  new Lsm(stQuaName) : sko.lparse.lsmObtain(stQuaName);
                    lsmSko.MakeSkolemFunction();
                    Lpr lprHead = null;
                    Lpr lprTail = null;
                    Qua quaUpper = quaTerm.quaOuter;
                    while (quaUpper != null)
                    {
                        if (!quaUpper.fExists)
                        {
                            if (lprHead == null)
                            {
                                lprHead = new Lpr(lsmSko, Lsm.lsmNil);
                                lprTail = lprHead;
                            }
                            Sko.AddToResult(quaUpper.lsxGetSubst(ref susVarList), ref lprHead, ref lprTail);
                        }
                        quaUpper = quaUpper.quaOuter;
                    }
                    quaTerm.lsxSubst = (lprHead != null) ? (Lsx) lprHead : lsmSko;
                }
                else
                {
                    Lsm lsmNewVar = new Lsm("A_" + Sko.cGensymId++);
                    lsmNewVar.MakeVariable();
                    quaTerm.lsxSubst = lsmNewVar;
                    susVarList = new Sus(susVarList, (Lsm)quaTerm.lsxSubst, quaTerm.lsxSubst);
                }
            }
            quaTerm.qtl.lsxSubst = quaTerm.lsxSubst;
            return quaTerm.lsxSubst;
        }
    }

    /// <summary>
    /// Describe how a symbol is used in a sequent
    /// </summary>
    public class Sus
    {
        public readonly Lsm lsmSymbol;
        public readonly Lsx lsxReplace;
        public readonly Sus susPrev;
        public readonly bool fGrows;

        public static readonly Sus susRoot = new Sus((Sus)null, null, null, true);
        public static readonly Sus susNone = new Sus((Sus)null, null, null, false);
        public static readonly Sus susVbl = new Sus((Sus)null, null, null, false);

        public Sus(Sus susPrev, Lsm lsmSymbol, Lsx lsxReplace, bool fGrows = true)
        {
            this.susPrev = susPrev;
            this.lsmSymbol = lsmSymbol;
            this.lsxReplace = lsxReplace;
            this.fGrows = fGrows;
        }

        public static bool fFindValue(Sus sus, Lsx lsxSym, out Lsx lsxValue)
        {
            if (sus == null || !sus.fGrows)
            {
                lsxValue = null;
                return false;
            }
            while (sus != susRoot)
            {
                if (sus.lsmSymbol == lsxSym)
                {
                    lsxValue = sus.lsxReplace;
                    return true;
                }
                sus = sus.susPrev;
            }
            lsxValue = null;
            return false;
        }
    }

    public class Qtl
    {
        public Qtl qtlPrev;
        public readonly Lpr lprQuantifierExpr;
        public Lsx lsxSubst;
        public bool fExists;

        public Qtl (Qtl qtlPrev, Lpr lprQuantifierExpr, bool fExists)
        {
            this.qtlPrev = qtlPrev;
            this.lprQuantifierExpr = lprQuantifierExpr;
            this.fExists = fExists;
        }

        public static Qtl fFindQuantifier(Qtl qtl, Lpr lprQuantifierExpr, bool fExists)
        {
            while (qtl != null)
            {
                if (qtl.lprQuantifierExpr == lprQuantifierExpr) //  && qtl.fExists == fExists)
                {
                    return qtl;
                }
                qtl = qtl.qtlPrev;
            }
            return null;
        }
    }

    public class Sko
    {
        public Qtl qtlList;
        public LParse lparse;

        public static int cGensymId = 0;

        public Lsx lsxClausalTransform(Lsx lsxA, out Sus susList, out int nNumNotNegated, 
                  bool fFlipRhs = false, LParse lparse = null)
        {
            if (lparse != null)
                this.lparse = lparse;

            if (!(lsxA is Lpr))
                throw new System.ArgumentException();
            Lpr lprA = (Lpr)lsxA;
            if (lprA.lsxCar != Lsm.lsmEntails)
                throw new System.ArgumentException();
            if (!(lprA.lsxCdr is Lpr))
                throw new System.ArgumentException();
            Lpr lprCdr = (Lpr)lprA.lsxCdr;
            if (!(lprCdr.lsxCdr is Lpr))
                throw new System.ArgumentException();
            Lpr lprCddr = (Lpr)lprCdr.lsxCdr;
            if (lprCddr.lsxCdr != Lsm.lsmNil)
                throw new System.ArgumentException();
            Lsx lsxR = lprCddr.lsxCar;
            Lsx lsxL = lprCdr.lsxCar;
            return lsxClausalTransform(lsxL, lsxR, out susList, out nNumNotNegated, fFlipRhs);
        }
        public static void AddToResult(Lsx lsxItem, ref Lpr lprHead, ref Lpr lprTail)
        {
            Lpr lprLink = new Lpr(lsxItem, Lsm.lsmNil);
            if (lprTail == null)
                lprHead = lprLink;
            else
                lprTail.lsxCdr = lprLink;
            lprTail = lprLink;
        }
        public static void AddSyms(LParse lparse, ExpressionEvaluatorGrammar grammar)
        {
            lparse.AddSym(Lsm.lsmAdd, grammar.plus);
            lparse.AddSym(Lsm.lsmAnd, grammar.backLand);
            lparse.AddSym(Lsm.lsmDivide, grammar.divide);
            lparse.AddSym(Lsm.lsmEntails, grammar.backRightarrow);
            lparse.AddSym(Lsm.lsmEquals, grammar.equals);
            lparse.AddSym(Lsm.lsmExists, grammar.backExists);
            lparse.AddSym(Lsm.lsmFalse, null);
            lparse.AddSym(Lsm.lsmForall, grammar.backForall);
            lparse.AddSym(Lsm.lsmIff, grammar.backleftrightarrow);
            lparse.AddSym(Lsm.lsmImplies, grammar.backrightarrow);
            lparse.AddSym(Lsm.lsmList, null);
            lparse.AddSym(Lsm.lsmMultiply, grammar.asterisk);
            lparse.AddSym(Lsm.lsmNot, grammar.backNeg);
            lparse.AddSym(Lsm.lsmOr, grammar.backLor);
            lparse.AddSym(Lsm.lsmTrue, null);
            lparse.AddSym(Lsm.lsmSubtract, grammar.minus);

            // set flags describing the usualy usage symbols. Flags elsewhere turn on usage of the flags in the prover.
            Lsm.lsmEquals.fPredicateReflexive = true;
            Lsm.lsmEquals.fCommutative = true;
            Lsm.lsmEquals.fPredicateTransitive = true;
            Lsm.lsmEquals.nArity = Lsm.nArityConst + 2;
        }

        public Lsx lsxClausalTransform(Lsx lsxL, Lsx lsxR, out Sus susList, out int nNumNotNegated, bool fFlipRhs = false)
        {
            // replace references to Qua to new Lsm_s
            // also remove Qua_s no longer needed: mark them as they are referenced, remove unreferenced
            Lsx lsxKernel;
            Qua quaQuantList;
            ToClauseSequent(lsxL, lsxR, out lsxKernel, out quaQuantList, out nNumNotNegated, fFlipRhs);
            Sus susVarList = Sus.susRoot;
            Lsx lsxSko = lsxKernel;
            if (lsxKernel is Lpr)
                lsxSko = lsxToSkolemSubst(lsxKernel, ref susVarList);
            susList = susVarList;
            return lsxSko;
        }

        public Lsx lsxToSkolemSubst(Lsx lsxTerm, ref Sus susVarList)
        {
            if (lsxTerm is Lpr)
            {
                Lpr lprTerm = (Lpr)lsxTerm;
                // assume there is no quantifier to hide scope of variable
                Lsx lsxA = lsxToSkolemSubst(lprTerm.lsxCar, ref susVarList);
                Lsx lsxB = lsxToSkolemSubst(lprTerm.lsxCdr, ref susVarList);
                if (lsxA == lprTerm.lsxCar && lsxB == lprTerm.lsxCdr)
                    return lsxTerm;
                return new Lpr(lsxA, lsxB);
            }
            else if (lsxTerm is Qua)
            {
                Qua quaTerm = (Qua)lsxTerm;
                return quaTerm.lsxGetSubst(ref susVarList);
            }
            return lsxTerm;
        }

        private void ToClauseSequent (Lsx lsxL, Lsx lsxR, out Lsx lsxKernel, out Qua quaQuantList, out int nNumNotNegated, bool fFlipRhs = false)
        {
            Qua quaQuantListL = null;
            Qua quaQuantListR = null;
            Lsx lsxKernelL;
            Lsx lsxKernelR;
            ToClausal(lsxL, false, Sus.susRoot, out quaQuantListL, out lsxKernelL);
            ToClausal(lsxR, !fFlipRhs, Sus.susRoot, out quaQuantListR, out lsxKernelR);
            lsxKernel = lsxAndKernels(lsxKernelL, lsxKernelR, out nNumNotNegated);
            quaQuantList = quaMerge(false, quaQuantListL, quaQuantListR);
        }

        /// <summary>
        /// Convert a prefix logic expr to a list of quantifiers and a kernel as a list of quads
        /// </summary>
        /// <param name="lsxA">logic expr</param>
        /// <param name="fNegate">true if the lsxA is inside an odd number of negations</param>
        /// <param name="susDictionary">used to replace lsm_s in lsxA</param>
        /// <param name="lprQuantList">Each quant has: car=child, cdr=quant with same variable</param>
        /// <param name="lprKernel">list of quads. each quad(car=negated,cdr=non-negated) has refs that refer
        /// to quants. Each ref is a lpr(car=lsm,cdr=quant)</param>
        private void ToClausal(Lsx lsxA, bool fNegate, Sus susDictionary, out Qua quaQuantList, out Lsx lsxKernel)
        {
            if (!(lsxA is Lpr))
            {
                if (lsxA == Lsm.lsmFalse || lsxA == Lsm.lsmTrue)
                {
                    quaQuantList = null;
                    if (fNegate)
                        lsxKernel = (lsxA == Lsm.lsmFalse) ? Lsm.lsmTrue : Lsm.lsmFalse;
                    else
                        lsxKernel = lsxA;
                }
                else
                    ToClausalAtom(lsxA, fNegate, susDictionary, out quaQuantList, out lsxKernel);
                return;
            }
            Lpr lprA = (Lpr)lsxA;
            Lsx lsxOp = lprA.lsxCar;
            Lsx lsxACdr = lprA.lsxCdr;
            if (lsxOp == Lsm.lsmNot)
                ToClausalNot(lsxACdr, fNegate, susDictionary, out quaQuantList, out lsxKernel);
            else if (lsxOp == Lsm.lsmForall || lsxOp == Lsm.lsmExists)
                ToClausalQuantifier(lsxACdr, lsxOp == Lsm.lsmExists, fNegate, lprA, susDictionary, out quaQuantList, out lsxKernel);
            else if (lsxOp == Lsm.lsmOr || lsxOp == Lsm.lsmAnd)
                ToClausalConnective(lsxACdr, lsxOp == Lsm.lsmOr, fNegate, susDictionary, out quaQuantList, out lsxKernel);
            else if (lsxOp == Lsm.lsmList)
                ToClausalConnective(lsxACdr, fNegate, fNegate, susDictionary, out quaQuantList, out lsxKernel);
            else if (lsxOp == Lsm.lsmImplies)
                ToClausalImplies(lsxACdr, fNegate, susDictionary, out quaQuantList, out lsxKernel);
            else if (lsxOp == Lsm.lsmIff)
                ToClausalIff(lsxACdr, fNegate, susDictionary, out quaQuantList, out lsxKernel);
            else
                ToClausalTerm(lprA, fNegate, susDictionary, out quaQuantList, out lsxKernel);
        }

        private void ToClausalAtom(Lsx lsxA, bool fNegate, Sus susDictionary, out Qua quaQuantList, out Lsx lsxKernel)
        {
            quaQuantList = null;
            Lpr lprQuad = new Lpr(Lsm.lsmNil, Lsm.lsmNil);
            Lpr lprTerm = new Lpr(lsxA, Lsm.lsmNil);
            if (fNegate)
                lprQuad.lsxCar = lprTerm;
            else
                lprQuad.lsxCdr = lprTerm;
            lsxKernel = new Lpr(lprQuad, Lsm.lsmNil);
        }

        private void ToClausalNot(Lsx lsxACdr, bool fNegate, Sus susDictionary, out Qua quaQuantList, out Lsx lsxKernel)
        {
            if (!(lsxACdr is Lpr))
                throw new System.ArgumentException();
            Lpr lprACdr = (Lpr)lsxACdr;
            ToClausal(lprACdr.lsxCar, !fNegate, susDictionary, out quaQuantList, out lsxKernel);
        }

        private void ToClausalImplies(Lsx lsxACdr, bool fNegate, Sus susDictionary, out Qua quaQuantList, out Lsx lsxKernel)
        {
            if (!(lsxACdr is Lpr))
                throw new System.ArgumentException();
            Lpr lprACdr = (Lpr)lsxACdr;
            if (!(lprACdr.lsxCdr is Lpr))
                throw new System.ArgumentException();
            Lpr lprACddr = (Lpr)lprACdr.lsxCdr;
            ClausalImplies(lprACdr.lsxCar, lprACddr.lsxCar, fNegate, susDictionary, out quaQuantList, out lsxKernel);
        }

        private void ClausalImplies(Lsx lsxA, Lsx lsxB, bool fNegate, Sus susDictionary, out Qua quaQuantList, out Lsx lsxKernel)
        {
            Qua quaQuantListL = null;
            Qua quaQuantListR = null;
            Lsx lsxKernelLhs;
            Lsx lsxKernelRhs;
            ToClausal(lsxA, !fNegate, susDictionary, out quaQuantListL, out lsxKernelLhs);
            ToClausal(lsxB, fNegate, susDictionary, out quaQuantListR, out lsxKernelRhs);
            int nNumNotNegated;
            ConnectKernels(ref lsxKernelLhs, lsxKernelRhs, ref quaQuantListL, quaQuantListR, true, fNegate, out nNumNotNegated);
            quaQuantList = quaQuantListL;
            lsxKernel = lsxKernelLhs;
        }

        private void ToClausalIff(Lsx lsxACdr, bool fNegate, Sus susDictionary, out Qua quaQuantList, out Lsx lsxKernel)
        {
            if (!(lsxACdr is Lpr))
                throw new System.ArgumentException();
            Lpr lprACdr = (Lpr)lsxACdr;
            if (!(lprACdr.lsxCdr is Lpr))
                throw new System.ArgumentException();
            Lpr lprACddr = (Lpr)lprACdr.lsxCdr;
            Qua quaQuantListL = null;
            Qua quaQuantListR = null;
            Lsx lsxKernelLhs;
            Lsx lsxKernelRhs;
            ClausalImplies(lprACdr.lsxCar, lprACddr.lsxCar, fNegate, susDictionary, out quaQuantListL, out lsxKernelLhs);
            ClausalImplies(lprACddr.lsxCar, lprACdr.lsxCar, fNegate, susDictionary, out quaQuantListR, out lsxKernelRhs);
            int nNumNotNegated;
            ConnectKernels(ref lsxKernelLhs, lsxKernelRhs, ref quaQuantListL, quaQuantListR, false, fNegate, out nNumNotNegated);
            quaQuantList = quaQuantListL;
            lsxKernel = lsxKernelLhs;
        }

        private void ToClausalTerm(Lpr lprA, bool fNegate, Sus susDictionary, out Qua quaQuantList, out Lsx lsxKernel)
        {
            ToClausalAtom(lsxSubst(lprA, susDictionary), fNegate, susDictionary, out quaQuantList, out lsxKernel);
        }

        public Lsx lsxSubst(Lsx lsxTerm, Sus susDictionary)
        {
            if (lsxTerm is Lpr)
            {
                Lpr lprTerm = (Lpr)lsxTerm;
                Lsx lsxFirst = lsxSubst(lprTerm.lsxCar, susDictionary);
                if (lprTerm.lsxCdr == Lsm.lsmNil)
                {
                    Debug.Assert(lprTerm.lsxCar is Lsm);
                    return lsxFirst;
                }
                Lpr lprHead = new Lpr(lsxFirst, Lsm.lsmNil);
                Lpr lprTail = lprHead;
                while (lprTerm.lsxCdr != Lsm.lsmNil)
                {
                    // assume there is no quantifier to hide scope of variable
                    lprTerm = (Lpr)lprTerm.lsxCdr;
                    Lsx lsxA = lsxSubst(lprTerm.lsxCar, susDictionary);
                    Sko.AddToResult(lsxA, ref lprHead, ref lprTail);
                }
                return lprHead;
            }
            else
            {
                Lsx lsxValue;
                if (Sus.fFindValue(susDictionary, lsxTerm, out lsxValue))
                    return lsxValue;
                else
                    return lsxTerm;
            }
        }

        private void ToClausalQuantifier(Lsx lsxACdr, bool fExists, bool fNegate, Lpr lprA, Sus susDictionary, out Qua quaQuantList, out Lsx lsxKernel)
        {
            if (!(lsxACdr is Lpr))
                throw new System.ArgumentException();
            Lpr lprACdr = (Lpr)lsxACdr;
            if (!(lprACdr.lsxCdr is Lpr))
                throw new System.ArgumentException();
            Lsm lsmVar;
            Lsm lsmVarQ = null;
            if (lprACdr.lsxCar is Lsm)
                lsmVar = (Lsm)lprACdr.lsxCar;
            else
            {
                Lpr lprId = (Lpr)lprACdr.lsxCar;
                if (!(lprId.lsxCar is Lsm))
                    throw new System.ArgumentException();
                lsmVar = (Lsm)lprId.lsxCar;
                if (!(lprId.lsxCdr is Lpr))
                    throw new System.ArgumentException();
                Lpr lprQ = (Lpr)lprId.lsxCdr;
                if (!(lprQ.lsxCar is Lsm))
                    throw new System.ArgumentException();
                lsmVarQ = (Lsm)lprQ.lsxCar;
            }
            Lpr lprACddr = (Lpr)lprACdr.lsxCdr;
            qtlList = new Qtl(qtlList, lprA, fExists != fNegate);
            Qua quaNew = new Qua(null, lsmVar, fExists != fNegate, qtlList, this);
            quaNew.lsmQName = lsmVarQ;
            Qua quaSubQuantList;
            Sus susSub = new Sus(susDictionary, lsmVar, quaNew);
            ToClausal(lprACddr.lsxCar, fNegate, susSub, out quaSubQuantList, out lsxKernel);
            quaNew.quaInner = quaSubQuantList;
            if (quaSubQuantList != null)
                quaSubQuantList.quaOuter = quaNew;
            quaQuantList = quaNew;
        }

        private void ToClausalConnective(Lsx lsxACdr, bool fOr, bool fNegate, Sus susDictionary, out Qua quaQuantList, out Lsx lsxKernel)
        {
            if (!(lsxACdr is Lpr))
            {
                if (lsxACdr != Lsm.lsmNil)
                    throw new System.ArgumentException();
                quaQuantList = null;
                lsxKernel = (fOr != fNegate) ? Lsm.lsmFalse : Lsm.lsmTrue;
                return;
            }
            Lpr lprNext = (Lpr)lsxACdr;
            Qua quaLhs;
            Lsx lsxKernelLhs;
            ToClausal(lprNext.lsxCar, fNegate, susDictionary, out quaLhs, out lsxKernelLhs);
            while (lprNext.lsxCdr is Lpr)
            {
                if (fOr != fNegate)
                {
                    if (lsxKernelLhs == Lsm.lsmTrue)
                        break;
                }
                else if (lsxKernelLhs == Lsm.lsmFalse)
                    break;

                lprNext = (Lpr)lprNext.lsxCdr;
                Lsx lsxTerm = lprNext.lsxCar;
                Qua quaRhs;
                Lsx lsxKernelRhs;
                ToClausal(lsxTerm, fNegate, susDictionary, out quaRhs, out lsxKernelRhs);
                int nNumNotNegated;
                ConnectKernels(ref lsxKernelLhs, lsxKernelRhs, ref quaLhs, quaRhs, fOr, fNegate, out nNumNotNegated);
            }
            quaQuantList = quaLhs;
            lsxKernel = lsxKernelLhs;
        }

        private void ConnectKernels(ref Lsx lsxKernelLhs, Lsx lsxKernelRhs, ref Qua quaLhs, Qua quaRhs, bool fOr, bool fNegate, out int nNumNotNegated)
        {
            if (fOr != fNegate)
            {
                lsxKernelLhs = lsxDistributeKernels(lsxKernelLhs, lsxKernelRhs);
                nNumNotNegated = Asc.nNotCounted;  // not expected to be used
            }
            else
                lsxKernelLhs = lsxAndKernels(lsxKernelLhs, lsxKernelRhs, out nNumNotNegated);
            quaLhs = quaMerge(fOr != fNegate, quaLhs, quaRhs);
        }

        private Lsx lsxAndKernels(Lsx lsxKernelLhs, Lsx lsxKernelRhs, out int nNumNotNegated)
        {
            nNumNotNegated = lsxKernelLhs.nLength();
            if (!(lsxKernelLhs is Lpr))
            {
                if (lsxKernelLhs == Lsm.lsmTrue)
                    return lsxKernelRhs;
                if (lsxKernelLhs == Lsm.lsmFalse)
                    return Lsm.lsmFalse;
                throw new System.ArgumentException();
            }
            if (!(lsxKernelRhs is Lpr))
            {
                if (lsxKernelRhs == Lsm.lsmTrue)
                    return lsxKernelLhs;
                if (lsxKernelRhs == Lsm.lsmFalse)
                    return Lsm.lsmFalse;
                throw new System.ArgumentException();
            }
            Lpr lprKernelLhs = (Lpr)lsxKernelLhs;
            Lpr lprKernelRhs = (Lpr)lsxKernelRhs;

            Lpr lprHead = null;
            Lpr lprTail = null;

            if (!fAndNonSubsumed(lprKernelLhs, lprKernelRhs, true, ref lprHead, ref lprTail))
                return Lsm.lsmFalse;
            nNumNotNegated = (lprKernelLhs == null) ? 0 : lprKernelLhs.nLength();
            if (!fAndNonSubsumed(lprKernelRhs, lprKernelLhs, false, ref lprHead, ref lprTail))
                return Lsm.lsmFalse;
            return lprHead;
        }

        /// <summary>
        /// Append items in lprKernelLhs that are not subsumed by an item in lprKernelRhs.
        /// Return false if two opposite terms are found.
        /// </summary>
        /// <returns></returns>
        private bool fAndNonSubsumed(Lpr lprKernelLhs, Lpr lprKernelRhs, bool fAllowSame, ref Lpr lprHead, ref Lpr lprTail)
        {
            while (true)
            {
                Lpr lprClauseLhs = (Lpr)lprKernelLhs.lsxCar;
                if (fClauseIsOpposed(lprKernelRhs, lprClauseLhs))
                    return false;
                if (!fClauseIsSubsumed(lprKernelRhs, lprClauseLhs, fAllowSame))
                    AddToResult(lprClauseLhs, ref lprHead, ref lprTail);

                if (!(lprKernelLhs.lsxCdr is Lpr))
                    break;
                lprKernelLhs = (Lpr)lprKernelLhs.lsxCdr;
            }
            return true;
        }

        private bool fSingleTerm (Lpr lprClause)
        {
            if ((lprClause.lsxCar is Lpr))
            {
                if (((Lpr) (lprClause.lsxCar)).lsxCdr == Lsm.lsmNil)
                {
                    return lprClause.lsxCdr == Lsm.lsmNil;
                }
            }
            else if ((lprClause.lsxCdr is Lpr))
            {
                if (((Lpr)(lprClause.lsxCdr)).lsxCdr == Lsm.lsmNil)
                {
                    return lprClause.lsxCar == Lsm.lsmNil;
                }
            }
            return false;
        }

        private bool fClauseIsSubsumed(Lpr lprKernel, Lpr lprClause, bool fAllowSame)
        {
            Lpr lprClauseLhs;
            KComparison kComp;
            while (true)
            {
                lprClauseLhs = (Lpr)lprKernel.lsxCar;
                kComp = kClauseIncludes(lprClauseLhs, lprClause);
                if (kComp != KComparison.kDifferent)
                    break;
                if (!(lprKernel.lsxCdr is Lpr))
                    break;
                lprKernel = (Lpr)lprKernel.lsxCdr;
            }
            if (kComp == KComparison.kSame)
                return !fAllowSame;
            if (kComp == KComparison.kBIncludesA)
                return true;
            return false;
        }

        private bool fClauseIsOpposed(Lpr lprKernel, Lpr lprClause)
        {
            bool fClauseSingle = fSingleTerm(lprClause);
            Lpr lprClauseLhs;

            // TODO: could convert ((b)) and (nil b c) to (c) etc. More generally if there are other single terms ANDed

            while (true)
            {
                lprClauseLhs = (Lpr)lprKernel.lsxCar;

                if (fClauseSingle && fSingleTerm(lprClauseLhs))
                {
                    KComparison kCar = kListIncludes(lprClauseLhs.lsxCar, lprClause.lsxCdr);
                    if (kCar == KComparison.kSame)
                    {
                        KComparison kCdr = kListIncludes(lprClauseLhs.lsxCdr, lprClause.lsxCar);
                        if (kCdr == KComparison.kSame)
                            return true;
                    }
                }

                if (!(lprKernel.lsxCdr is Lpr))
                    break;
                lprKernel = (Lpr)lprKernel.lsxCdr;
            }
            return false;
        }

        public enum KComparison
        {
            kAIncludesB,
            kBIncludesA,
            kSame,
            kDifferent
        }

        /// <summary>
        /// Return true if A includes everything that B does
        /// </summary>
        /// <param name="lprA"></param>
        /// <param name="lprB"></param>
        /// <returns></returns>
        public KComparison kClauseIncludes(Lpr lprA, Lpr lprB)
        {
            KComparison kCar = kListIncludes(lprA.lsxCar, lprB.lsxCar);
            if (kCar == KComparison.kDifferent)
                return KComparison.kDifferent;
            KComparison kCdr = kListIncludes(lprA.lsxCdr, lprB.lsxCdr);
            if (kCar == KComparison.kSame)
                return kCdr;
            if (kCdr == KComparison.kDifferent)
                return KComparison.kDifferent;
            if (kCdr == KComparison.kSame)
                return kCar;
            if (kCdr == kCar)
                return kCar;
            return KComparison.kDifferent;
        }

        /// <summary>
        /// Check if A contains everything that B does or vice versa
        /// </summary>
        public KComparison kListIncludes(Lsx lsxA, Lsx lsxB)
        {
            if (!(lsxB is Lpr))
            {
                if (!(lsxA is Lpr))
                    return KComparison.kSame;
                else
                    return KComparison.kAIncludesB;
            }
            if (!(lsxA is Lpr))
                return KComparison.kBIncludesA;
            Lpr lprA = (Lpr)lsxA;
            Lpr lprB = (Lpr)lsxB;
            bool fAincB = fAIncludesB(lprA, lprB);
            bool fBincA = fAIncludesB(lprB, lprA);
            if (fAincB)
            {
                if (fBincA)
                    return KComparison.kSame;
                else
                    return KComparison.kAIncludesB;
            }
            if (fBincA)
                return KComparison.kBIncludesA;
            else
                return KComparison.kDifferent;
        }
        public bool fAIncludesB(Lpr lprA, Lpr lprB)
        {
            while (true)
            {
                Lsx lsxInB = lprB.lsxCar;
                if (!fOccurs (lprA, lsxInB))
                    return false;
                if (!(lprB.lsxCdr is Lpr))
                    return true;
                lprB = (Lpr)lprB.lsxCdr;
            }
        }

        private bool fOccurs (Lsx lsxSeq, Lsx lsxA)
        {
            if (lsxSeq == Lsm.lsmNil)
                return false;
            Lpr lprSeq = (Lpr)lsxSeq;
            while (true)
            {
                Lsx lsxInA = lprSeq.lsxCar;
                if (lsxInA.fEqual(lsxA))
                    return true;
                if (!(lprSeq.lsxCdr is Lpr))
                    break;
                lprSeq = (Lpr)lprSeq.lsxCdr;
            }
            return false;
        }

        /// <summary>
        /// Merge each lhs clause with each rhs clause. Remove negated terms and subsumed clauses.
        /// </summary>
        private Lsx lsxDistributeKernels(Lsx lsxA, Lsx lsxB)
        {
            if (!(lsxB is Lpr))
            {
                if (lsxB == Lsm.lsmFalse)
                    return lsxA;
                if (lsxB == Lsm.lsmTrue)
                    return Lsm.lsmTrue;
                throw new System.ArgumentException();
            }
            if (!(lsxA is Lpr))
            {
                if (lsxA == Lsm.lsmFalse)
                    return lsxB;
                if (lsxA == Lsm.lsmTrue)
                    return Lsm.lsmTrue;
                throw new System.ArgumentException();
            }

            Lpr lprHead = null;
            Lpr lprTail = null;
            Lpr lprA = (Lpr)lsxA;
            Lpr lprB = (Lpr)lsxB;
            while (true)
            {
                Lpr lprInB = (Lpr)lprB.lsxCar;
                Lpr lprA2 = lprA;
                while (true)
                {
                    Lpr lprInA = (Lpr)lprA2.lsxCar;

                    // was Lsx lsxMerged = lsxMergeClauses(lprInB, lprInA);
                    Lsx lsxMerged = lsxMergeClauses(lprInA, lprInB);
                    if (lsxMerged != Lsm.lsmTrue)
                    {
                        Lpr lprMerged = (Lpr)lsxMerged;
                        if (lprHead != null
                            && fClauseIsOpposed(lprHead, lprMerged))
                        { }
                        else if (lprHead == null || !fClauseIsSubsumed(lprHead, lprMerged, false))
                            AddToResult(lprMerged, ref lprHead, ref lprTail);
                    }
                    if (!(lprA2.lsxCdr is Lpr))
                        break;
                    lprA2 = (Lpr)lprA2.lsxCdr;
                }
                if (!(lprB.lsxCdr is Lpr))
                    break;
                lprB = (Lpr)lprB.lsxCdr;
            }
            if (lprHead == null)
                return Lsm.lsmTrue;
            return lprHead;
        }

        private void CopyNotExcluded(Lsx lsxA, Lsx lsxExclude, Lsx lsxExclude2, ref Lpr lprHead, ref Lpr lprTail)
        {
            if (lsxA == Lsm.lsmNil)
                return;
            
            Lpr lprA = (Lpr)lsxA;
            while (true)
            {
                if (!fOccurs(lsxExclude, lprA.lsxCar))
                {
                    if (!fOccurs(lsxExclude2, lprA.lsxCar))
                        AddToResult(lprA.lsxCar, ref lprHead, ref lprTail);
                }
                if (!(lprA.lsxCdr is Lpr))
                    break;
                lprA = (Lpr)lprA.lsxCdr;
            }
            
        }

        private Lsx lsxMergeClauses(Lpr lprA, Lpr lprB)
        {
            Lpr lprHeadL = null;
            Lpr lprTailL = null;
            CopyNotExcluded(lprA.lsxCar, lprB.lsxCdr, Lsm.lsmNil, ref lprHeadL, ref lprTailL);
            CopyNotExcluded(lprB.lsxCar, lprA.lsxCdr, lprA.lsxCar, ref lprHeadL, ref lprTailL);

            Lpr lprHeadR = null;
            Lpr lprTailR = null;
            CopyNotExcluded(lprA.lsxCdr, lprB.lsxCar, Lsm.lsmNil, ref lprHeadR, ref lprTailR);
            CopyNotExcluded(lprB.lsxCdr, lprA.lsxCar, lprA.lsxCdr, ref lprHeadR, ref lprTailR);

            if (lprHeadL == null && lprHeadR == null)
                return Lsm.lsmTrue;
            Lsx lsxLhs = (lprHeadL == null) ? Lsm.lsmNil : (Lsx)lprHeadL;
            Lsx lsxRhs = (lprHeadR == null) ? Lsm.lsmNil : (Lsx) lprHeadR;

            return new Lpr(lsxLhs, lsxRhs);
        }

        /// <summary>
        /// Create combined list of quantifiers from the child lists.
        /// Combine quantifiers where possible
        /// </summary>
        private Qua quaMerge(bool fOr, Qua quaLhs, Qua quaRhs)  
        {
            Qua quaHead = null;
            Qua quaTail = null;
            bool fMergeAllowed = true;

            while (true)
            {
                if (quaLhs == null)
                {
                    if (quaRhs != null)
                        AddQuaToResult(quaRhs, ref quaHead, ref quaTail);
                    break;
                }
                if (quaRhs == null)
                {
                    AddQuaToResult(quaLhs, ref quaHead, ref quaTail);
                    break;
                }

                if (fMergeAllowed && quaLhs.fExists == quaRhs.fExists)
                {
                    Qua quaNextRhs = quaRhs.quaInner;
                    quaRhs.fReplaced = true;
                    quaRhs.quaInner = quaLhs;
                    quaRhs = quaNextRhs;
                    continue;
                }
                fMergeAllowed = false;
                if (quaLhs.cExistsDepth() <= quaRhs.cExistsDepth())
                {
                    Qua quaNextLhs = quaLhs.quaInner;
                    AddQuaToResult(quaLhs, ref quaHead, ref quaTail);
                    quaLhs = quaNextLhs;
                }
                else
                {
                    Qua quaNextRhs = quaRhs.quaInner;
                    AddQuaToResult(quaRhs, ref quaHead, ref quaTail);
                    quaRhs = quaNextRhs;
                }
            }
            return quaHead;
        }

        private void AddQuaToResult(Qua quaA, ref Qua quaHead, ref Qua quaTail)
        {
            if (quaTail == null)
                quaHead = quaA;
            else
                quaTail.quaInner = quaA;
            quaTail = quaA;
        }

    }
}