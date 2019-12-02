#define DEBUG
using System;
using System.Collections.Generic;


namespace reslab
{
    /// <summary>
    /// Determine which clauses are right vs left (e.g. positive or semantic resolution)
    /// </summary>
    public abstract class Pmb
    {
        public bool fDoResolution = true;

        public abstract GnpR.KSide kSide(Asc ascNew);

        public abstract sbyte nResTerm(Asc ascNew);

        public abstract bool fUseP1Resolution();

        public virtual void ReadyToRun(Res res)
        {
        }
    }

    /// <summary>
    /// Indicate positive resolution
    /// </summary>
    public class Prm : Pmb
    {
        public static Pmb pmbPositiveOnly;


        protected Prm (bool fDoResolution)
        {
            this.fDoResolution = fDoResolution;
        }

        public static Pmb pmbPositive()
        {
            if (pmbPositiveOnly  == null)
                pmbPositiveOnly = new Prm(true);
            return pmbPositiveOnly;
        }


        public override GnpR.KSide kSide(Asc ascNew)
        {
            return (ascNew.rgnTree[Asc.nPosnNumNegTerms] != 0) ? GnpR.KSide.kRight : GnpR.KSide.kLeft;
        }

        public override sbyte nResTerm(Asc ascNew)
        {
            return (ascNew.rgnTree[Asc.nPosnNumNegTerms] != 0) ? Asc.nHasResolveTerm : Asc.nNoResolveTerm;
        }

        public override bool fUseP1Resolution()
        {
            return true;
        }
    }

    /// <summary>
    /// Set of support strategy
    /// </summary>
    public class Pss : Pmb
    {
        public static Pmb pmbSetOfSupport = new Pss();

        public override bool fUseP1Resolution()
        {
            return false;
        }

        public override GnpR.KSide kSide(Asc ascNew)
        {
            if (ascNew.gfbSource.fIsFromNegation())
                    return GnpR.KSide.kLeft;
            else
                    return GnpR.KSide.kRight;
        }

        public override sbyte nResTerm(Asc ascNew)
        {
            // all inferences after initial entail go on left
            if (ascNew.gfbSource.fIsFromNegation())
                    return Asc.nNoResolveTerm;
            else
                    return Asc.nHasResolveTerm;
        }
    }

    /// <summary>
    /// Object in model 
    /// </summary>
    public class Pso
    {
        public readonly string stName;

        public Pso(string stName)
        {
            this.stName = stName;
        }
    }

    /// <summary>
    /// Base for models for semantic resolution
    /// </summary>
    public abstract class Psm : Pmb
{
        public readonly string stName;
        List<Pso> rgpsoUniverse = new List<Pso>();
        Dictionary<string, FPred> mpst_fPred = new Dictionary<string, FPred>();
        Dictionary<string, PsoFunction> mpst_fFunction = new Dictionary<string, PsoFunction>();
        public override bool fUseP1Resolution()
        {
            return false;   // has to be false, because P1 is a special form of semantic resolution that conflicts with this
        }

        public Pso psoRegObject (Pso pso)
        {
            rgpsoUniverse.Add(pso);
            return pso;
        }

        public delegate bool FPred(List<Pso> rgpsoArgs);
        public delegate Pso PsoFunction(List<Pso> rgpsoArgs);

        public Psm(string stName)
        {
            this.stName = stName;
        }

        public void RegPredicate(string stPredName, FPred fpred)
        {
            mpst_fPred.Add(stPredName, fpred);
        }

        public void RegFunction(string stFnName, PsoFunction psofunction)
        {
            mpst_fFunction.Add(stFnName, psofunction);
        }

        public FPred fpredGet(string stPName)
        {
            FPred fpred;
            if (!mpst_fPred.TryGetValue(stPName, out fpred))
            {
                throw new ArgumentException("model " + stName + " does not have delegate for predicate " + stPName);
            }
            return fpred;
        }

        public PsoFunction psofunctionGet(string stFName, int nArity)
        {
            PsoFunction psofunction;
            if (!mpst_fFunction.TryGetValue(stFName, out psofunction))
            {
                throw new ArgumentException("model " + stName + " does not have delegate for function " + stFName + " arity=" + nArity);
            }
            return psofunction;
        }

        sbyte nIterateAndEval(int nVar, Asc asc, List<Pso> rgpsoValues)
        {
            if (nVar == 0)
            {
                return asc.nEval(rgpsoValues, this);
            }
            else
            {
                nVar--;
#if true
                // find a valuation that falsifies the clause
                sbyte nResTerm = sbyte.MaxValue;
                foreach (Pso psoVal in rgpsoUniverse)
                {
                    List<Pso> rgpsoValuesLocal = new List<Pso>();
                    foreach (Pso pso in rgpsoValues)
                        rgpsoValuesLocal.Add(pso);

                    rgpsoValuesLocal.Add(psoVal);
                    sbyte nThisResTerm = nIterateAndEval(nVar, asc, rgpsoValuesLocal);
                    if (nThisResTerm == Asc.nNoResolveTerm) // clause does not hold in this valuation
                        return Asc.nNoResolveTerm;
                    if (nThisResTerm < nResTerm)
                        nResTerm = nThisResTerm;
                }
                return nResTerm;

#else
                // just pick a value for each var?
                rgpsoValues.Add(rgpsoUniverse[0]);
                sbyte nResTerm = nIterateAndEval(nVar, asc, rgpsoValues);
                return nResTerm;
#endif
            }
        }
        public override GnpR.KSide kSide(Asc ascNew)
        {
            int nNumVars = ascNew.nNumVars();
            List<Pso> rgpsoValues = new List<Pso>();
            sbyte nTerm = nIterateAndEval(nNumVars, ascNew, rgpsoValues);
            return (nTerm == Asc.nNoResolveTerm) ? GnpR.KSide.kLeft : GnpR.KSide.kRight;
        }

        /// <summary>
        /// Used to decide if a clause is a left or a right
        /// </summary>
        /// <param name="ascNew"></param>
        /// <returns></returns>
        public override sbyte nResTerm(Asc ascNew)
        {
            return 0; // should be used
        }

    }

    
    
}
