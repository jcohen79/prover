using System;
using System.Collections.Generic;

namespace reslab
{
    /// <summary>
    /// Take a list of Quads and provide a series of Quads that satisify all the given Quads.
    /// The result Quad is a conjunction.
    /// </summary>
    /// 

    ///
    /// Iterate an array of n entries over values 0..(nValues-1)
    /// 
    public class Vai
    {
        public int nVars;
        public int[] rgnState;
        public int nValues;
        public int[] rgnVarMap;

        public Vai (int nVars, int nValues, List<int> rgintFreeVarOccurs)
        {
            this.nVars = nVars;
            this.nValues = nValues;
            rgnState = new int[nVars];
            for (int i = 0; i < nVars; i++)
                rgnState[i] = Mdl.nUndefined;  // predicate is unset, function is first value in rglsmValues
            rgnVarMap = rgintFreeVarOccurs.ToArray();
        }

        /// <summary>
        /// Advance to next set of values
        /// </summary>
        /// <returns>true of there is another set of values</returns>
        public bool fNext()
        {
            for (int i = nVars - 1; i >= 0; --i)
            {
                if (rgnState[i] < nValues - 1)
                {
                    rgnState[i]++;
                    for (int j = i + 1; j < nVars; j++)
                        rgnState[j] = Mdl.nUndefined;
                    return true;
                }
            }
            return false;
        }
    }

    ///
    /// Manage values of functions and predicates
    ///
    public class Svt
    {
        public int[] rgnValue;   // index into rglsmValues for function or nUndefined, etc. for predicate
        public Lsm lsmSymbol;   // identify the predicate/function
        public readonly bool fFunction;
        public readonly Mdl mdl;
        public readonly int nNumArgs;

        public Svt(Lsm lsmSymbol, bool fFunction, int nNumArgs, Mdl mdl)
        {
            this.lsmSymbol = lsmSymbol;
            this.fFunction = fFunction;
            this.mdl = mdl;
            this.nNumArgs = nNumArgs;
        }

        public bool fIncrement()
        {
            int nLimit = mdl.rglsmValues.Count;
            for (int i = 0; i < rgnValue.Length; i++)
            {
                int nVal = rgnValue[i];
                if (i < nLimit - 1)
                {
                    rgnValue[i] = nVal + 1;
                    for (int j = 0; j < i; j++)
                        rgnValue[i] = 0;
                }
            }
            return false;
        }

        int nExp(int b, int e)
        {
            int v = 1;
            for (int i = 0; i < e; i++)
                v *= b;
            return v;
        }

        public void Reset()
        {

            rgnValue = new int[nExp(mdl.rglsmValues.Count, nNumArgs)];
            for (int i = 0; i < rgnValue.Length; i++)
                rgnValue[i] = 0;
        }
    }

    /// <summary>
    /// Model: universe of values, functions, predicates
    /// </summary>
    public class Mdl
    {
        public List<Lsm> rglsmValues = new List<Lsm>();
        public List<Svt> rgsvtTables = new List<Svt>();

        public const int nUndefined = 0;
        public const int nFalse = 1;
        public const int nTrue = 2;

        public int nGensym = 0;

        public Mdl()
        {

        }
        public void AddConstant(Lsm lsmOld)
        {
            rglsmValues.Add(lsmOld);
        }

        public void Reset()
        {
            foreach (Svt svt in rgsvtTables)
            {
                // TODO: continue on with using new value.
                svt.Reset();
            }
        }

        public void AddValue()
        {
            Lsm lsmNew = new Lsm("MV_" + nGensym++);
            rglsmValues.Add(lsmNew);
        }

        /// <summary>
        /// Move to next function value
        /// </summary>
        /// <returns>true if successful</returns>
        public bool fIncrementFunctions()
        {
            foreach (Svt svt in rgsvtTables)
            {
                if (!svt.fFunction)
                    continue;
                if (!svt.lsmSymbol.fVariable())
                    continue;
                if (svt.fIncrement())
                {
                    foreach (Svt svt2 in rgsvtTables)
                    {
                        if (!svt2.fFunction)
                            continue;
                        if (svt2 == svt)
                            return true;
                        svt2.Reset();
                    }
                }
            }
            return false;
        }

        public void DefFunction (Lsm lsmSymbol, bool fFunction, int nNumArgs)
        {
            if (svtFind(lsmSymbol) != null)
                return;
            Svt svt = new Svt(lsmSymbol, fFunction, nNumArgs, this);
            rgsvtTables.Add(svt);
        }

        public Svt svtFind (Lsm lsmSymbol)
        {
            foreach (Svt svt in rgsvtTables)
            {
                if (svt.lsmSymbol == lsmSymbol)
                    return svt;
            }
            return null;
        }

        public int nFindInValues(Lsm lsm)
        {
            int n = 0;
            foreach (Lsm lsmValue in rglsmValues)
            {
                if (lsm == lsmValue)
                    return n;
                n++;
            }
            throw new ArgumentException();
        }

        void GatherFreeVariables(Lsx lsxTerm, ref List<Lsm> rglsmFreeVariables, ref List<int> rgintFreeVarOccurs)
        {
            if (lsxTerm is Lsm)
            {
                Lsm lsmTerm = (Lsm)lsxTerm;
                if (lsmTerm.fVariable())
                {
                    int iPos = 0;
                    foreach (Lsm lsm in rglsmFreeVariables)
                    {
                        if (lsm == lsmTerm)
                        {
                            rgintFreeVarOccurs.Add(iPos);
                            return;
                        }
                        iPos++;
                    }
                    rgintFreeVarOccurs.Add(iPos);
                    rglsmFreeVariables.Add(lsmTerm);
                }
                return;
            }
            bool fFirst = true;
            Lpr lprTerm = (Lpr)lsxTerm;
            while (true)
            {
                Lsx lsxA = lprTerm.lsxCar;
                if (!fFirst || !(lsxA is Lsm))
                    GatherFreeVariables(lsxA, ref rglsmFreeVariables, ref rgintFreeVarOccurs);
                if (lprTerm.lsxCdr == Lsm.lsmNil)
                    break;
                lprTerm = (Lpr)lprTerm.lsxCdr;
                fFirst = false;
            }
        }

        /// <summary>
        /// Called from SAT to indicate if predicate expr value should be true or false for all values.
        /// Determine if predicate table can be set to that state. Return true if it can.
        /// </summary>
        /// <param name="lsxTerm"></param>
        /// <returns>true if expr now has requested value in all case</returns>
        public bool fValueForTerm(Lsx lsxTerm, bool fPos)
        {
            if (lsxTerm is Lsm)
                return (lsxTerm == (fPos ? Lsm.lsmTrue : Lsm.lsmFalse));

            Lpr lprTerm = (Lpr)lsxTerm;
            Lsm lsmOp = (Lsm)lprTerm.lsxCar;
            List<Lsm> rglsmFreeVars = new List<Lsm>();
            List<int> rgintFreeVarOccurs = new List<int>();
            GatherFreeVariables(lprTerm, ref rglsmFreeVars, ref rgintFreeVarOccurs);
            Svt svt = svtFind(lsmOp);

            for (int i = 0; i < 2; i++)
            {
                bool fScan;
                int nVal;
                if (i == 0)
                {
                    fScan = true;
                    nVal = fPos ? nFalse : nTrue;
                }
                else
                {
                    fScan = false;
                    nVal = fPos ? nTrue : nFalse;
                }

                Vai vai = new Vai(rglsmFreeVars.Count, rglsmValues.Count, rgintFreeVarOccurs);
                do
                {
                    int nFreeVar = 0; // each free var is controlled by an index in the vai.
                    int nPos = nPosForExpr(lprTerm, vai, ref nFreeVar);
                    if (fScan)
                    {
                        if (svt.rgnValue[nPos] == nVal)
                            return false;
                    }
                    else
                        svt.rgnValue[nPos] = nVal;
                }
                while (vai.fNext());
            }

            return true;
        }


        /// <summary>
        /// Find Svt array index referenced by expression 
        /// </summary>
        /// <returns>index into rglsmValues for fn; nUndefined, etc for pred </returns>
        public int nPosForExpr(Lpr lprTerm, Vai vai, ref int nFreeVar)
        {
            Lsx lsxNext = lprTerm.lsxCdr;
            int nFactor = rglsmValues.Count;
            int nPos = 0;
            while (lsxNext != Lsm.lsmNil)
            {
                Lpr lprNext = (Lpr)lsxNext;
                int nArgVal = fValueExpr(lprNext.lsxCar, vai, ref nFreeVar);
                nPos = nPos * nFactor + nArgVal;
                lsxNext = lprNext.lsxCdr;
            }
            return nPos;
        }

        /// <summary>
        /// Evaluate 
        /// </summary>
        /// <returns>index into rglsmValues for fn; nUndefined, etc for pred </returns>
        public int fValueExpr(Lsx lsxTerm, Vai vai, ref int nFreeVar)
        {
            if (lsxTerm is Lsm)
            {
                Lsm lsmTerm = (Lsm)lsxTerm;
                if (!lsmTerm.fVariable())
                    return nFindInValues(lsmTerm);
                return vai.rgnState[vai.rgnVarMap[nFreeVar++]];
            }
            Lpr lprTerm = (Lpr)lsxTerm;
            Lsm lsmOp = (Lsm)lprTerm.lsxCar;
            Svt svt = svtFind(lsmOp);
            int nPos = nPosForExpr(lprTerm, vai, ref nFreeVar);
            return svt.rgnValue[nPos];
        }
    }

    public abstract class Ist
    {

        public int nTautologyTrue;
        public int nTautologyFalse;
        public int nMatched = 0;
        public int nFailed = 0;

        // return false to terminate search
        public abstract bool fPass(Lpr lprQuad);

        public abstract void Failed();
    }

    /// <summary>
    /// State for rolling back during Sat
    /// </summary>
    public class Sst
    {
        public Sst sstPrev;
        public Sst sstNext;
        public Lpr lprCurrentClausePlace;
        public Lsx lsxCurrentTermPlace;
        public bool fPos;
        public Lsx lsxNegAssignments;
        public Lsx lsxPosAssignments;
        Mdl mdl;

        Sst()
        {

        }

        public static Sst sstStart(Lpr lprClauses)
        {
            Sst sstFirst = null;
            Sst sstLast = null;
            while (true)
            {
                Sst sst = new Sst();
                if (sstFirst == null)
                    sstFirst = sst;
                else
                    sstLast.sstNext = sst;
                sst.sstPrev = sstLast;
                sstLast = sst;
                sst.lprCurrentClausePlace = lprClauses;
                sst.StartClause();
                if (lprClauses.lsxCdr == Lsm.lsmNil)
                    break;
                lprClauses = (Lpr) lprClauses.lsxCdr;
            }
            return sstFirst;
        }
        
        Lsx lsxSubst (Lsx lsxTerm, Tms tmsSubsts)
        {
            if (lsxTerm is Lsm)
                return Tms.lsxLookup(tmsSubsts, (Lsm)lsxTerm);

            Lpr lprTerm = (Lpr)lsxTerm;
            Lsx lsxSubCar = lsxSubst(lprTerm.lsxCar, tmsSubsts);
            Lsx lsxSubCdr = lsxSubst(lprTerm.lsxCdr, tmsSubsts);
            if (lsxSubCar == lprTerm.lsxCar
                && lsxSubCdr == lprTerm.lsxCdr)
                return lsxTerm;
            return new Lpr(lsxSubCar, lsxSubCdr);
        }

        public bool fPass(Ist istReport)
        {
            return istReport.fPass(new Lpr(lsxNegAssignments, lsxPosAssignments));
        }

        public void StartClause ()
        {
            if (sstPrev == null)
            {
                lsxNegAssignments = Lsm.lsmNil;
                lsxPosAssignments = Lsm.lsmNil;
            }
            else
            {
                lsxNegAssignments = sstPrev.lsxNegAssignments;
                lsxPosAssignments = sstPrev.lsxPosAssignments;
            }
            Lpr lprCurrentClause = (Lpr) lprCurrentClausePlace.lsxCar;
            lsxCurrentTermPlace = lprCurrentClause.lsxCar;
            fPos = false;
        }

        public Lsx lsxPendingTerm ()
        {
            while (!(lsxCurrentTermPlace is Lpr))
            {
                if (fPos)
                    return Lsm.lsmNil;
                fPos = true;
                Lpr lprClause = (Lpr)lprCurrentClausePlace.lsxCar;
                lsxCurrentTermPlace = lprClause.lsxCdr;
            }
            Lpr lprTermPlace = (Lpr)lsxCurrentTermPlace;
            lsxCurrentTermPlace = lprTermPlace.lsxCdr;
            return lprTermPlace.lsxCar;
        }

        public void AddAssignment(Lsx lsxTerm, bool fPos)
        {
            if (fPos)
            {
                Lpr lprItem = new Lpr(lsxTerm, lsxPosAssignments);
                lsxPosAssignments = lprItem;
            }
            else
            {
                Lpr lprItem = new Lpr(lsxTerm, lsxNegAssignments);
                lsxNegAssignments = lprItem;
            }
        }

        public enum KMatchKind
        {
            kNotFound,
            kConflict,
            kMatched,
        }

        /// <summary>
        /// Record that variable has been matched to a symbol
        /// </summary>
        public class Tms
        {
            public Tms tmsPrev;
            public Lsm lsmVar;
            public Lsx lsxValue;

            public Tms(Tms tmsPrev, Lsm lsmVar, Lsx lsxValue)
            {
                this.tmsPrev = tmsPrev;
                this.lsmVar = lsmVar;
                this.lsxValue = lsxValue;
            }

            public static Lsx lsxLookup (Tms tmsSubsts, Lsm lsmVar)
            {
                while (tmsSubsts != null)
                {
                    if (tmsSubsts.lsmVar == lsmVar)
                        return tmsSubsts.lsxValue;
                    tmsSubsts = tmsSubsts.tmsPrev;
                }
                return lsmVar;
            }

            public static void ModifyValue(Tms tmsSubsts, Lsm lsmVar, Lsx lsxValue)
            {
                while (tmsSubsts != null)
                {
                    if (tmsSubsts.lsmVar == lsmVar)
                    {
                        tmsSubsts.lsxValue = lsxValue;
                        return;
                    }
                    tmsSubsts = tmsSubsts.tmsPrev;
                }
                throw new ArgumentException();
            }

        }

        KMatchKind kTermMatches (Lsx lsxTerm, Lsx lsxHasValue, ref Tms tmsSubsts)
        {
            if (lsxTerm is Lpr)
            {
                if (!(lsxHasValue is Lpr))
                    return KMatchKind.kConflict;
                Lpr lprTerm = (Lpr)lsxTerm;
                Lpr lprHasValue = (Lpr)lsxHasValue;
                if (lprTerm.lsxCar != lprHasValue.lsxCar)
                    return KMatchKind.kConflict;
                Lsx lsxRestTerm = lprTerm.lsxCdr;
                Lsx lsxResthasValue = lprHasValue.lsxCdr;
                KMatchKind kKind = KMatchKind.kMatched;
                while (lsxRestTerm != Lsm.lsmNil)
                {
                    if (lsxResthasValue == Lsm.lsmNil)
                        return KMatchKind.kConflict;
                    lprTerm = (Lpr)lsxRestTerm;
                    lprHasValue = (Lpr)lsxResthasValue;
                    KMatchKind kItemKind = kTermMatches(lprTerm.lsxCar, lprHasValue.lsxCar, ref tmsSubsts);
                    switch (kItemKind)
                    {
                        case KMatchKind.kConflict:
                            return KMatchKind.kConflict;
                        case KMatchKind.kMatched:
                            break;
                        default:
                            throw new ArgumentException();
                    }
                    lsxRestTerm = lprTerm.lsxCdr;
                    lsxResthasValue = lprHasValue.lsxCdr;
                }
                if (lsxResthasValue != Lsm.lsmNil)
                    return KMatchKind.kConflict;
                return kKind;
            }
            Lsm lsmTerm = (Lsm)lsxTerm;
            if (!(lsxHasValue is Lsm))
            {
                return kMatchVal(lsmTerm, lsxHasValue, ref tmsSubsts);
            }
            Lsm lsmHasValue = (Lsm)lsxHasValue;
            if (lsmTerm == lsmHasValue)
                return KMatchKind.kMatched;
            /* Cases: cross product of following:
                constant
                unassigned var
                var assigned to same constant
                var assigned to different constant
                var assigned to same var
                var assigned to another var (2 cycle)
                var in cycle with other vars (2 cycle)
            */
            if (lsmTerm.fVariable())
            {
                if (lsmHasValue.fVariable())
                {
                    Lsx lsxTermVal = Tms.lsxLookup(tmsSubsts, lsmTerm);
                    Lsx lsxHasValueVal = Tms.lsxLookup(tmsSubsts, lsmHasValue);

                    if (lsxTermVal == lsmTerm)
                    {
                        if (lsxHasValueVal == lsmHasValue)
                        {
                            // point to each other
                            tmsSubsts = new Tms(tmsSubsts, lsmHasValue, lsmTerm);
                            tmsSubsts = new Tms(tmsSubsts, lsmTerm, lsmHasValue);
                            return KMatchKind.kMatched;
                        }
                        else
                        {
                            // override old value to point to new var
                            tmsSubsts = new Tms(tmsSubsts, lsmHasValue, lsmTerm);
                            // new var points to old value
                            tmsSubsts = new Tms(tmsSubsts, lsmTerm, lsxHasValueVal);
                        }
                    }
                    else if (lsxHasValueVal == lsmHasValue)
                    {
                        tmsSubsts = new Tms(tmsSubsts, lsmHasValue, lsxTermVal);
                    }
                    else if (lsmTerm == lsxHasValueVal)   // pointing at each other, and var is term and is not a non-var
                        return KMatchKind.kMatched;
                    else if (lsxTermVal != lsxHasValueVal) // other hasValue is already in use with another value
                    {
                        // check for one or two loops of variables that could be joined
                        if (lsxTermVal is Lsm && ((Lsm)lsxTermVal).fVariable())
                        {
                            if (lsxHasValueVal is Lsm && ((Lsm)lsxHasValueVal).fVariable())
                            {
                                // two var loops
                                Tms.ModifyValue(tmsSubsts, lsmTerm, lsxHasValueVal);
                                Tms.ModifyValue(tmsSubsts, lsmHasValue, lsxTermVal);
                            }
                            else
                            {
                                // lsmTerm is loop, lsmHasValue is not loop
                                Tms.ModifyValue(tmsSubsts, lsmTerm, lsxHasValueVal);
                                tmsSubsts = new Tms(tmsSubsts, lsmHasValue, lsxTermVal);
                            }
                        }
                        else if (lsxHasValueVal is Lsm && ((Lsm)lsxHasValueVal).fVariable())
                        {
                            // lsmTerm is not loop, lsmHasValue is loop
                            Tms.ModifyValue(tmsSubsts, lsmHasValue, lsmTerm);
                            tmsSubsts = new Tms(tmsSubsts, lsmTerm, lsxHasValueVal);
                        }
                        else
                            return KMatchKind.kConflict;
                    }
                    return KMatchKind.kMatched;   // term is var, is assigned to same val as hasValue
                }
                else
                {
                    return kMatchVal(lsmTerm, lsmHasValue, ref tmsSubsts);
                }
            }
            else if (lsmHasValue.fVariable())
            {
                return kMatchVal(lsmHasValue, lsmTerm, ref tmsSubsts);
            }
            else
                return KMatchKind.kConflict;
        }

        KMatchKind kMatchVal (Lsm lsmVar, Lsx lsxNonVar, ref Tms tmsSubsts)
        {
            Lsx lsxVarValue = Tms.lsxLookup(tmsSubsts, lsmVar);
            if (lsxVarValue == lsmVar)
            {
                tmsSubsts = new Tms(tmsSubsts, lsmVar, lsxNonVar);
                return KMatchKind.kMatched;
            }
            else if (lsxNonVar == lsxVarValue)
                return KMatchKind.kMatched;
            else if (lsxVarValue is Lsm && ((Lsm)lsxVarValue).fVariable())
            {
                Lsm lsmVarValue = (Lsm)lsxVarValue;
                while (true)
                {
                    Lsx lsxVarVarValue = Tms.lsxLookup(tmsSubsts, lsmVarValue);
                    tmsSubsts = new Tms(tmsSubsts, lsmVarValue, lsxNonVar);
                    if (lsmVar == lsmVarValue)
                        break;
                    lsmVarValue = (Lsm)lsxVarVarValue;
                }
                return KMatchKind.kMatched;
            }
            else
                return KMatchKind.kConflict;
        }

        /// <summary>
        /// Walk down list of existing assignments and find one that can be unified with given term
        /// </summary>
        public bool fFindMatchingTerm (bool fConflict, Lsx lsxTerm, out Tms tmsSubsts)
        {
            Lsx lsxValues = (fConflict == fPos) ? lsxNegAssignments : lsxPosAssignments;
            while (lsxValues != Lsm.lsmNil)
            {
                Lpr lprValueItem = (Lpr)lsxValues;
                tmsSubsts = null;
                KMatchKind kKind = kTermMatches(lsxTerm, lprValueItem.lsxCar, ref tmsSubsts);
                lsxValues = lprValueItem.lsxCdr;
                switch (kKind)
                {
                    case KMatchKind.kNotFound:
                        break;
                    case KMatchKind.kConflict:
                        break;
                    case KMatchKind.kMatched:
                        return true;
                    default:
                        throw new ArgumentException();
                }
            }
            tmsSubsts = null;
            return false;
        }

        void AddExprToModel(Lsx lsxTerm, bool fFunction)
        {
            if (lsxTerm is Lsm)
            {
                Lsm lsmTerm = (Lsm)lsxTerm;
                if (!lsmTerm.fVariable())
                    mdl.AddConstant(lsmTerm);
            }
            else
            {
                bool fFirst = true;
                while (lsxTerm != Lsm.lsmNil)
                {
                    Lpr lprTerm = (Lpr)lsxTerm;
                    Lsx lsxA = lprTerm.lsxCar;
                    if (fFirst)
                    {
                        mdl.DefFunction((Lsm)lsxA, fFunction, lprTerm.lsxCdr.nLength());
                        fFirst = false;
                    }
                    else
                        AddExprToModel(lsxA, true);
                    lsxTerm = lprTerm.lsxCdr;
                }
            }
        }

        void AddTermsToMdl(Lsx lsxTerms)
        {
            while (lsxTerms != Lsm.lsmNil)
            {
                Lpr lprTerms = (Lpr)lsxTerms;
                AddExprToModel(lprTerms.lsxCar, false);

                lsxTerms = lprTerms.lsxCdr;
            }
        }

        Mdl CreateMdl(Lpr lprSequent)
        {
            mdl = new Mdl();
            while (true)
            {
                Lpr lprClause = (Lpr) lprSequent.lsxCar;
                AddTermsToMdl(lprClause.lsxCar);
                AddTermsToMdl(lprClause.lsxCdr);
                if (lprSequent.lsxCdr == Lsm.lsmNil)
                    break;
                lprSequent = (Lpr)lprSequent.lsxCdr;
            }
            if (mdl.rglsmValues.Count == 0)
                mdl.AddValue();


            mdl.Reset();
            return mdl;
        }

        /// <summary>
        /// Find conjunctions of term assignments (true/false) that make the sequent true
        /// Iterate through the clauses and report all combinations that do not conflict.
        /// This means making one term true/false (depending on side) from each clause.
        /// Skip if there is a conflict. 
        /// </summary>
        public static void Perform(Lpr lprSequent, Ist istReport, int nMaxNewValues)
        {
            Sst sstCur = Sst.sstStart(lprSequent);
            Mdl mdl = sstCur.CreateMdl(lprSequent);
            int nNewValuesLeft = nMaxNewValues;
            bool fPassed = false;

            while (true)
            {
                Lsx lsxTerm = sstCur.lsxPendingTerm();
                if (lsxTerm == Lsm.lsmNil)
                {
                    if (null == sstCur.sstPrev)
                    {
                        if (!mdl.fIncrementFunctions())
                        {
                            if (nNewValuesLeft > 0)
                            {
                                nNewValuesLeft--;
                                mdl.AddValue();
                            }
                            else
                            {
                                if (!fPassed)
                                    istReport.Failed();
                                return;
                            }
                        }
                    }
                    else
                        sstCur = sstCur.sstPrev;
                    continue;
                }

                Tms tmsSubsts;
                if (sstCur.fFindMatchingTerm(true, lsxTerm, out tmsSubsts))
                    continue;

                // TODO: could delete duplicates (or more general of two)
                // if (!sstCur.fFindMatchingTerm(false, lsxTerm, out tmsSubsts))

                if (!mdl.fValueForTerm(lsxTerm, sstCur.fPos))
                    continue;
                
                sstCur.AddAssignment(lsxTerm, sstCur.fPos);
                if (sstCur.sstNext == null)
                {
                    fPassed = true;
                    if (!sstCur.fPass(istReport))
                        break;
                }
                else
                {
                    sstCur = sstCur.sstNext;
                    sstCur.StartClause();
                }
            }
        }
    }
}
