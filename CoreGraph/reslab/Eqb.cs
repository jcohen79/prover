using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace reslab
{
    public abstract class Eqb : Bid
    {
        public Urc urc;
        public readonly Abt abtToEquate;
        public Res res;
        ushort nOffsetShowTerm;
        public Unb unb;

        public Eqb(Abt abtToEquate, Unb unb, Urc urc, Res res, ushort nOffsetShowTerm)
        {
            this.abtToEquate = abtToEquate;
            this.unb = unb;
            this.urc = urc;
            this.res = res;
            this.nOffsetShowTerm = nOffsetShowTerm;
        }

        /// <summary>
        /// Save an acs that results from paramodulation that needs to be prioritized 
        /// Start with acsA and apply every pti on the eqr stack
        /// </summary>
        public void SaveAsc(Esn esnSolution, Mva mvbMapToVblIdForChild, Moa mobToOffsetForChild)
        {
#if LOG
            Eqm.Log("SaveAsc " + (fFinal ? "final" : "partial"));
#endif
            Cmb cmb = urc.cmbInferencer();
            Shv shv = new Shv(cmb, (Vbv)esnSolution, ((Vbv)esnSolution).vbvBSide(),
                              abtToEquate.avcA, abtToEquate.avcB, 
                              mvbMapToVblIdForChild, mobToOffsetForChild);
            Abt abtResult;
            Urc urcLocal;

            urcLocal = urc;
            abtResult = abtToEquate;
            int nMaxNumTerms = abtResult.avcA.aic.nLiterals;
            if (abtResult.avcB != null)
                nMaxNumTerms += abtResult.avcB.aic.nLiterals;
            shv.MakeRgnTermOffset(nMaxNumTerms);
            if (!fMergeClauses(shv, urcLocal, true))
            {
                shv.fProcessAscb(shv.ascbRes, abtResult, esnSolution, unb);
            }
            urc.GrowUnification(unb); // expand unification even if tautology
        }

        public bool fMergeClauses(Shv shv, Urc urc, bool fHaveRhs)
        {
            shv.nNumResNegTerms = 0;
            shv.nNumResPosTerms = 0;

            urc.MakeResolvantTerm(shv, nOffsetShowTerm);
            shv.ascbRes.DummySizes();
            shv.ascbLiteral = shv.ascbRes.ascbCreateWithSameTde();

            bool fResolveNegSide;
            uint nSkipFieldFF;
            uint nSkipFieldTF;
            uint nSkipFieldFT;
            uint nSkipFieldTT;
            urc.GetMergeParms(out fResolveNegSide, out nSkipFieldFF, out nSkipFieldTF, out nSkipFieldFT, out nSkipFieldTT);
            Vbv vbvFirst = shv.vbvLeft.vbvFirst;

            Asc ascLeft = shv.avcA.aic.asc;
            Asc ascRight = shv.avcB.aic.asc;

            shv.fPtiEnabled = false;
            if (shv.vbvLeft.fMergeOtherTerms(shv, false, fResolveNegSide, fHaveRhs))
                return true;
            shv.fPtiEnabled = true;
            if (shv.fMergeTerms(ascLeft, false, fResolveNegSide, nSkipFieldFF, Vbv.vbvA))
                return true;
            if (fHaveRhs && shv.fMergeTerms(ascRight, false, fResolveNegSide, nSkipFieldTF, Vbv.vbvB))
                return true;
            if (shv.fMergeTerms(ascLeft, true, fResolveNegSide, nSkipFieldFT, Vbv.vbvA))
                return true;
            if (fHaveRhs && shv.fMergeTerms(ascRight, true, fResolveNegSide, nSkipFieldTT, Vbv.vbvB))
                return true;
            shv.fPtiEnabled = false;
            if (shv.vbvLeft.fMergeOtherTerms(shv, true, fResolveNegSide, fHaveRhs))
                return true;
            return false;
        }

    }

    public class SameObject<T> : IEqualityComparer<T> where T : class
    {

        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            return x == y;
        }

        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    /// <summary>
    /// Map Vbv from prev to new version
    /// </summary>
    public class Vpm
    {
        // static SameObject<Vbv> sameVbv = new SameObject<Vbv>();

        Dictionary<long, Vbv> mpvbv_vbvOldToNew = new Dictionary<long, Vbv>();
        Vbv vbvNeedsMapped1;
        Vbv vbvNeedsMapped2;

#if false
        public static Dictionary<Vbv, LinkedList<Vbv>> mpvbv_rgvbvOrigin = new Dictionary<Vbv, LinkedList<Vbv>>();
#endif

        public Vpm(Vbv vbvNeedsMapped1 = null, Vbv vbvNeedsMapped2 = null)
        {
            this.vbvNeedsMapped1 = vbvNeedsMapped1;
            this.vbvNeedsMapped2 = vbvNeedsMapped2;
        }

        public bool fNeedsMapping (Vbv vbv)
        {
            return vbv == vbvNeedsMapped1 || vbv == vbvNeedsMapped2;
        }

        public void Add (Vbv vbvOld, Vbv vbvNew)
        {
            mpvbv_vbvOldToNew.Add(vbvOld.nId, vbvNew);
#if false
            LinkedList<Vbv> rgvbvValue;
            if (!mpvbv_rgvbvOrigin.TryGetValue(vbvNew, out rgvbvValue))
            {
                rgvbvValue = new LinkedList<Vbv>();
                mpvbv_rgvbvOrigin.Add(vbvNew, rgvbvValue);
            }
            rgvbvValue.AddLast(vbvOld);
#endif

        }

        public Vbv vbvValue (Vbv vbvOld)
        {
            if (vbvNeedsMapped2 == null)
            {
                // this occurs during split. Merge will do the replacement.
                if (vbvOld == Vbv.vbvA || vbvOld == Vbv.vbvB)
                    return vbvOld;
            }
            else if (vbvOld == Vbv.vbvA)
                vbvOld = vbvNeedsMapped1;
                // return Vbv.vbvA;
            else if (vbvOld == Vbv.vbvB)
                vbvOld = vbvNeedsMapped2;

            Vbv vbvResult;
            if (mpvbv_vbvOldToNew.TryGetValue(vbvOld.nId, out vbvResult))
                return vbvResult;
            return vbvOld;
        }
    }

}