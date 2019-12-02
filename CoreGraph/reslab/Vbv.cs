
using GraphMatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace reslab
{
    /// <summary>
    /// Reference to the origin of a proof step 
    /// </summary>
    public abstract class Vpo : Bid
    {
        public abstract string stId();


    }

    /// <summary>
    /// Hold an element in list of hypotheses used to solve a parent Eqs.
    /// </summary>
    public class Vhy : Vpo
    {
        public readonly Eqs eqsHypothesis;
        public readonly Vhy vhyNext;

        public Vhy (Eqs eqsHypothesis, Vhy vhyNext)
        {
            this.eqsHypothesis = eqsHypothesis;
            this.vhyNext = vhyNext;
        }

        public override string stId()
        {
            return ToString();
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append("[");
            sb.Append(nId);
            sb.Append("? ");
            sb.Append(eqsHypothesis);
            sb.Append("]");
        }
    }

    /// <summary>
    /// Value assigned to vbl
    /// </summary>
    public class Vba : Vpo
    {
        public Vba vbaPrev;
        public sbyte nVblId;   // vblId in local Atb
        public ushort nValue;   // offset is in Atb that is nLevelsUpToValue up
        public Vbv vbvForValue;   // refers to asb where the value term is located (at nValue offset), and the set of vbl instances
                                  // differs from the vbv in vie: vie has origin of each symbol inside the term
        public Vbv vbvUsedAtValue;  // allow vbv.nReplaceAtPosn at nValue of this
                                    // is fix for fixed point

        public Vba()
        {
        }

        public override string stId()
        {
            return ToString();
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            sb.Append("(");
            sb.Append(nId);
            sb.Append(". ");
            sb.Append(nVblId);
            sb.Append(":");
            sb.Append(nValue);
            sb.Append("$");
            if (vbvForValue == Vbv.vbvA)
                sb.Append("A");
            else if (vbvForValue == Vbv.vbvB)
                sb.Append("B");
            else
                sb.Append(vbvForValue.nId);
            sb.Append(")");
        }
    }

    /// <summary>
    /// Hold the variable values used to complete an equate.
    /// </summary>
    public class Vbv : Vpo, Esn
    {
        public Vba rgvbaList;
        Vbv vbvNextSameSize;
        public Asb asb;
        public Vbv vbvFirst;
        public Avc avcOfValue;
        public ushort nReplaceAtPosn;   // where pti was applied
        public ushort nReplaceFromPosn;   // where matching term is coming from (for Pvp, does not appear in result)
        public ushort nReplaceWithPosn;   // where replacement term is coming from
        public Pti pti;     // has literal number where = subst is being done
        public Vbv vbvNext;
        public bool fJoinPti = false;
        public int nRelPosn;      // position within Vbv tree (a temp hack until vbv tree is array)
        // public Vhy vhyFirst;

        public static Vbv vbvA;
        public static Vbv vbvB;

        public enum KMapIds
        {
            kDoMapIds,
            kDontMapIds
        }

        public new static void Reset()
        {
            vbvA = new Vbv((Asb)null);
            vbvB = new Vbv((Asb)null);
        }

        public Vbv(Asb asb)
        {
            this.asb = asb;
            this.nReplaceAtPosn = Pmu.nNoReplace;
        }

        /// <summary>
        /// Insert child in sequence of increasing nReplaceAtPosn, so Equality is simpler
        /// </summary>
        /// <param name="vbvPti"></param>
        public void AddChild(Vbv vbvPti)
        {
            Vbv vbvPrev = null;
            for (Vbv vbvPlace = vbvFirst; vbvPlace != null; vbvPlace = vbvPlace.vbvNext)
            {
                if (vbvPlace.nReplaceAtPosn != Pmu.nNoReplace
                    && vbvPlace.nReplaceAtPosn > vbvPti.nReplaceAtPosn)
                    break;
                vbvPrev = vbvPlace;
            }
            if (vbvPrev == null)
            {
                vbvPti.vbvNext = vbvFirst;
                vbvFirst = vbvPti;
            }
            else
            {
                vbvPti.vbvNext = vbvPrev.vbvNext;
                vbvPrev.vbvNext = vbvPti;
            }
        }

        /// <summary>
        /// Return true if there is already a child at the give nReplaceAt
        /// </summary>
        public bool fHasChildReplaceAt(ushort nChildReplaceAt)
        {
            for (Vbv vbvPlace = vbvFirst; vbvPlace != null; vbvPlace = vbvPlace.vbvNext)
            {
                if (vbvPlace.nReplaceAtPosn == nChildReplaceAt)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Number the vbv_s in a tree so that they can be compared for equality and hash
        /// </summary>
        public int nSetRelPosns(int nStartAt = 0)
        {
            int nNextVal = nStartAt;
            nRelPosn = nNextVal++;
            for (Vbv vbvPlace = vbvFirst; vbvPlace != null; vbvPlace = vbvPlace.vbvNext)
            {
                nNextVal = nSetRelPosns(nNextVal);
            }
            return nNextVal;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vbv))
                return false;
            Vbv vbvO = (Vbv)obj;
            if (nReplaceAtPosn != vbvO.nReplaceAtPosn)
                return false;
            if (nReplaceFromPosn != vbvO.nReplaceFromPosn)
                return false;
            if (nReplaceWithPosn != vbvO.nReplaceWithPosn)
                return false;
            if (fJoinPti != vbvO.fJoinPti)
                return false;
            if (!asb.Equals(vbvO.asb))
                return false;
            //is avcOfValue compare needed?
            if (pti != null)
            {
                if (vbvO.pti == null)
                    return false;
                if (pti.nLitB != vbvO.pti.nLitB)
                    return false;
            }
            else if (vbvO.pti != null)
                return false;

            Vba vbaO = vbvO.rgvbaList;
            for (Vba vba = rgvbaList; vba != null; vba = vba.vbaPrev)
            {
                if (vbaO == null)
                    return false;
                if (vba.nVblId != vbaO.nVblId)
                    return false;
                if (vba.nValue != vbaO.nValue)
                    return false;
                if (vba.vbvForValue.nRelPosn != vbaO.vbvForValue.nRelPosn)
                    return false;
                if (vba.vbvUsedAtValue != null)
                {
                    if (vbaO.vbvUsedAtValue == null)
                        return false;
                    if (vba.vbvUsedAtValue.nRelPosn != vbaO.vbvUsedAtValue.nRelPosn)
                        return false;
                }
                else if (vbaO.vbvUsedAtValue != null)
                    return false;
                vbaO = vbaO.vbaPrev;
            }
            if (vbaO != null)
                return false;

#if false
            no, does not affect equality, just build a list of ngc that need to be updated when soln is accepted
            if (0 != nCompareVhyList(vhyFirst, vbvO.vhyFirst))
                return false;
#endif
            Vbv vbvPlaceO = vbvO.vbvFirst;
            Vbv vbvPrev = null;
            for (Vbv vbvPlace = vbvFirst; vbvPlace != null; vbvPlace = vbvPlace.vbvNext)
            {
#if DEBUG
                if (vbvPrev != null
                    && vbvPrev.nReplaceAtPosn != Pmu.nNoReplace)
                { 
                    if (vbvPrev.nReplaceAtPosn >= vbvPlace.nReplaceAtPosn)
                        throw new ArgumentException();
                }
#endif
                if (!vbvPlace.Equals(vbvPlaceO))
                    return false;
                vbvPlaceO = vbvPlaceO.vbvNext;
            }
            if (vbvPlaceO != null)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            int nValue = nRelPosn;
            nValue = nValue * 7 + nReplaceAtPosn;
            nValue = nValue * 9 + nReplaceFromPosn;
            nValue = nValue * 11 + nReplaceWithPosn;
            if (fJoinPti)
                nValue++;
            if (pti != null)
                nValue = nValue * 13 + (int) pti.nLitB;
            // equality does not depend on order
            for (Vba vba = rgvbaList; vba != null; vba = vba.vbaPrev)
                nValue = nValue + vba.nVblId + vba.nValue + vba.vbvForValue.nRelPosn;
            for (Vbv vbvPlace = vbvFirst; vbvPlace != null; vbvPlace = vbvPlace.vbvNext)
                nValue = nValue + vbvPlace.GetHashCode(); 
            return nValue;
        }

        public override string stId()
        {
            if (this == Vbv.vbvA)
                return "A";
            if (this == Vbv.vbvB)
                return "A";
            else
                return "vbv#" + nId;
        }

        /// <summary>
        /// Return the child that refers to the rhs of a cut
        /// </summary>
        public Vbv vbvBSide()
        {
            Vbv vbvChild = vbvFirst;
            while (vbvChild != null && vbvChild.nReplaceAtPosn != Pmu.nNoReplace)
            {
                vbvChild = vbvChild.vbvNext;
            }
            return vbvChild;
        }

        public Vbv vbvJoin()
        {
            Vbv vbvChild = vbvFirst;
            while (vbvChild != null)
            {
                if (vbvChild.fJoinPti)
                    break;
                vbvChild = vbvChild.vbvNext;
            }
            return vbvChild;
        }

#if false
        Not needed, probably not correct

        /// <summary>
        /// Compare two sequences of Vhy. Assume they are in order by decreasing eqs.nId
        /// </summary>
        public static int nCompareVhyList(Vhy vhyA, Vhy vhyB)
        {
            if (vhyA == null)
            {
                if (vhyB == null)
                    return 0;
                return -1;
            }
            else if (vhyB == null)
                return 1;

            Vhy vhyPlaceB = vhyB;
            for (Vhy vhyPlaceA = vhyA; vhyPlaceA != null; vhyPlaceA = vhyPlaceA.vhyNext)
            {
                if (vhyPlaceB == null)
                    return 1;
                if (vhyPlaceA.eqsHypothesis.nId < vhyPlaceB.eqsHypothesis.nId)
                    return -1;
                if (vhyPlaceA.eqsHypothesis.nId > vhyPlaceB.eqsHypothesis.nId)
                    return 1;
                vhyPlaceB = vhyPlaceB.vhyNext;
            }
            if (vhyPlaceB != null)
                return 1;
            return 0;
        }
#endif

        /// <summary>
        /// Return child that substitutes into the lhs or rhs of an equality
        /// </summary>
        /// <returns></returns>
        public Vbv vbvForJoin()
        {
            Vbv vbvChild = vbvFirst;
            while (vbvChild != null)
            {
                if (vbvChild.fJoinPti)
                    return vbvChild;
                vbvChild = vbvChild.vbvNext;
            }
            throw new ArgumentException();
        }

        /// <summary>
        /// Traverse tree and add all literals in the clauses that are not the equation using in equate.
        /// </summary>
        /// <returns>true if tautology found</returns>
        public static bool fMergeOtherTermsList(Vbv vbvList, Shv shv,
                                           bool fPos, bool fResolveNegSide, bool fHaveRhs)
        {
            for (Vbv rto = vbvList; rto != null; rto = rto.vbvNext)
            {
                if (rto.fMergeOtherTerms(shv, fPos, fResolveNegSide, fHaveRhs))
                   return true;
            }
            return false;
        }

        public bool fMergeOtherTerms(Shv shv, bool fPos, bool fResolveNegSide, bool fHaveRhs)
        {
            if (this != shv.vbvLeft 
                && (!fHaveRhs || this != shv.vbvRight))
            {
                uint nSkipField = 0;
                if (pti != null)
                {
                    if (fPos)
                        nSkipField = (uint)1 << (int)pti.nLitB;
                }

                if (avcOfValue != null)
                {
                    if (shv.fMergeTerms(avcOfValue.aic.asc, fPos, fResolveNegSide, nSkipField, this))
                        return true;
                }
            }
            if (fMergeOtherTermsList(vbvFirst, shv, fPos, fResolveNegSide, fHaveRhs))
                return true;
            return false;
        }


        public bool fNeedsMapping()
        {
            return this == Vbv.vbvA || this == Vbv.vbvB;
        }

        public static void StartGlobalHack(Atp atp)
        {
            Vbv.vbvA.asb = atp;  // HACK
            Vbv.vbvB.asb = atp;

        }

        public static void EndGlobalHack()
        {
            Vbv.vbvA.asb = null;  // HACK
            Vbv.vbvB.asb = null;
        }

        public Vbv vbvMakeCopy(Vpm vpm)
        {
            Vbv vbvCopy = new Vbv(asb);
            if (!fNeedsMapping())
                vpm.Add(this, vbvCopy);

            vbvCopy.avcOfValue = avcOfValue;
            vbvCopy.nReplaceAtPosn = nReplaceAtPosn;
            vbvCopy.nReplaceFromPosn = nReplaceFromPosn;
            vbvCopy.nReplaceWithPosn = nReplaceWithPosn;
            vbvCopy.pti = pti;

            return vbvCopy;
        }

        public Vba vbaFind(sbyte nVblId)
        {
            Vba vba = rgvbaList;
            while (vba != null)
            {
                if (vba.nVblId == nVblId)
                    return vba;
                vba = vba.vbaPrev;
            }
            return null;
        }

        public Vba vbaAdd(sbyte nVblId, ushort nValue, Vbv vbvForValue)
        {
            Vba vbaPrev = null;
            for (Vba vbaNext = rgvbaList; vbaNext != null; 
                 vbaNext = vbaNext.vbaPrev)
            {
                if (vbaNext.nVblId == nVblId)
                    throw new ArgumentException();
                // add the vba in order so Equals does not need to check different order
                if (vbaNext.nVblId > nVblId)
                    break;
                vbaPrev = vbaNext;
            }

            Vba vba = new Vba();
            vba.nVblId = nVblId;
            vba.nValue = nValue;
            vba.vbvForValue = vbvForValue;
            if (vbaPrev == null)
            {
                vba.vbaPrev = rgvbaList;
                rgvbaList = vba;
            }
            else
            {
                vba.vbaPrev = vbaPrev.vbaPrev;
                vbaPrev.vbaPrev = vba;
            }
            return vba;
        }

        public void MapVbaTree(Vpm vpm, Imp impMapper, Vbv vbvOutput, Vbv vbvPti)
        {
            for (Vbv vbvInputChild = vbvFirst;
                 vbvInputChild != null; vbvInputChild = vbvInputChild.vbvNext)
            {
                // don't map vbl ids, because this function applies to child vbv_s only.
                Vbv vbvDest = vpm.vbvValue(vbvInputChild);
                vbvInputChild.MapVbaList(vbvDest, true, vpm, impMapper, false, vbvOutput, vbvPti);

                vbvInputChild.MapVbaTree(vpm, impMapper, vbvOutput, vbvPti);

            }
        }

        public void MapVbaList(Vbv vbvDest, bool fMapValue, Vpm vpm, Imp impMapper,
                               bool fMapVblIds, Vbv vbvOutput, Vbv vbvPti)
        {
            for (Vba vbaIn = rgvbaList; vbaIn != null; vbaIn = vbaIn.vbaPrev)
            {
                ushort nValueNew;
                Vbv vbvForValue;
                if (fMapValue &&
                    (vpm.fNeedsMapping(vbaIn.vbvForValue)
                        || vbaIn.vbvForValue.fNeedsMapping()))
                {
                    impMapper.MapValue(vbaIn.nValue, out nValueNew, out vbvForValue, 
                                       vbvOutput, vbvPti);

                    if (vbvForValue == Vbv.vbvB
                            && vbvPti != null    // Note: this causes vbvB not to appear if vbvPti leaked out Etp.MergeVbv
                            && vbvPti != vbvOutput)
                        vbvForValue = vbvPti;
                    else
                        vbvForValue = vpm.vbvValue(vbvForValue);
                }
                else
                {
                    nValueNew = vbaIn.nValue;
                    vbvForValue = vpm.vbvValue(vbaIn.vbvForValue);
                }

                sbyte nVblIdSource;
                Vbv vbvNewDest;
                if (fMapVblIds)
                    impMapper.MapVblId(vbaIn.nVblId, out nVblIdSource, out vbvNewDest);
                else
                {
                    nVblIdSource = vbaIn.nVblId;
                    vbvNewDest = vbvDest;
                }
                Vbv vbvNewDest2;
                if (vbvNewDest == Vbv.vbvA)
                    vbvNewDest2 = vbvOutput;
                else if (vbvNewDest == Vbv.vbvB)
                    vbvNewDest2 = vbvPti;
                else
                    vbvNewDest2 = vpm.vbvValue(vbvNewDest);

                Vbv vbvUsedAtValue = null;
                if (vbaIn.vbvUsedAtValue != null)
                    // update previous value, if any
                    vbvUsedAtValue = vpm.vbvValue(vbaIn.vbvUsedAtValue);
                // Set a new vbvUsedAtValue if the vba refers to address where replacement occurred.
                Vbv vbvNewUsedAtValue = impMapper.vbvGetUsedAtValue(nVblIdSource, vbvOutput, vbvPti);
                if (vbvNewUsedAtValue != null)
                {
                    if (vbvUsedAtValue == null)
                        vbvUsedAtValue = vbvNewUsedAtValue;
                    else
                        throw new ArgumentException();
                }

                Vba vba = vbvNewDest2.vbaAdd(nVblIdSource, nValueNew, vbvForValue);
                vba.vbvUsedAtValue = vbvUsedAtValue;
            }
        }

        /// <summary>
        /// Replace references to old Vbv with the corresponding new one
        /// </summary>
        public static void UpdateVba(Vpm vpm, Vbv vbvInput)
        {
            Vba vbaIn = vbvInput.rgvbaList;
            while (vbaIn != null)
            {
                Vbv vbvOld = vbaIn.vbvForValue;
                if (vbvOld != Vbv.vbvA && vbvOld != Vbv.vbvB)
                {
                    Vbv vbvNew = vpm.vbvValue(vbvOld);
                    if (vbvNew != null)
                        vbaIn.vbvForValue = vbvNew;

                    if (vbaIn.vbvUsedAtValue != null)
                    {
                        Vbv vbvUsedNew = vpm.vbvValue(vbaIn.vbvUsedAtValue);
                        if (vbvNew != null)
                            vbaIn.vbvUsedAtValue = vbvUsedNew;
                        else
                            throw new ArgumentException();
                    }
                }
                vbaIn = vbaIn.vbaPrev;
            }

            Vbv vbvInputChild = (Vbv)vbvInput.vbvFirst;
            while (vbvInputChild != null)
            {
                UpdateVba(vpm, vbvInputChild);
                vbvInputChild = (Vbv)vbvInputChild.vbvNext;
            }

        }

        public static void CopyVbvChildren(Vbv vbvInput, Vpm vpm, Vbv vbvParent)
        {
            for (Vbv vbvInputChild = vbvInput.vbvFirst; 
                 vbvInputChild != null; vbvInputChild = vbvInputChild.vbvNext)
            {
                Vbv vbvOutputChild = vbvInputChild.vbvMakeCopy(vpm);
                vbvParent.AddChild(vbvOutputChild);
                CopyVbvChildren(vbvInputChild, vpm, vbvOutputChild);
            }
        }

        /// <summary>
        /// Return true if there is a conflict in the solution
        /// </summary>
        /// <returns></returns>
        public bool fCheckConflictingReplaceAtPosn(Vbv vbvOutput, Vbv vbvPti)
        {
            for (Vbv vbvFirstChild = vbvFirst; vbvFirstChild != null;
                 vbvFirstChild = vbvFirstChild.vbvNext)
            {
                // check if the is another vbv with same parent and same nReplaceAtPosn
                for (Vbv vbvOtherChild = vbvFirstChild.vbvNext; vbvOtherChild != null;
                     vbvOtherChild = vbvOtherChild.vbvNext)
                {
                    // found two at same position
                    if (vbvFirstChild.nReplaceAtPosn == vbvOtherChild.nReplaceAtPosn)
                        return true;
                }
                if (vbvFirstChild.fCheckConflictingReplaceAtPosn(vbvOutput, vbvPti))
                    return true;
            }

            Spo spo = new Spo();
            spo.SetupSoln(vbvOutput.asb, vbvPti.asb, vbvOutput, vbvPti);

            // check if there is a vba whose nValue is the location of a pti
            //    (these seems to occur only when pti is repositioned because of mobToOffsetForChild 
            for (Vba vba = rgvbaList; vba != null; vba = vba.vbaPrev)
            {
                Vbv vbvValue = vba.vbvForValue;
                if (vbvValue == vbvA)
                    vbvValue = vbvOutput;
                else if (vbvValue == vbvB)
                    vbvValue = vbvPti;

                // find if there is a vbv under the vbvValue at the offset given by vba
                for (Vbv vbvChildOfValue = vbvValue.vbvFirst;
                     vbvChildOfValue != null; vbvChildOfValue = vbvChildOfValue.vbvNext)
                {
                    if (vbvChildOfValue.nReplaceAtPosn == vba.nValue)
                    {
                        // Allow special case where the pti 'from' is a vbl that is being replaced.
                        // That is allowed because the 'from' vbl is being replaced by the 'to' 
                        // side of the pti, and hence the vbl value is not used now (maybe later).
                        // Check for case where pti 'from' is the vbl for this vba.
                        // if (vbvChildOfValue != this)
                        {
                            // confirm it is the right vbl
                            if (vbvChildOfValue.pti != null)
                            {
                                if (vba.vbvUsedAtValue == vbvChildOfValue)
                                    continue;
                            }
                        }

                        // conflict: vba should not refer to site of pti, except for special case above
                        return true;
                    }
                }

            }
            return false;
        }

        public static bool fFindOccurs(Asb asbLeft, Asb asbRight, Vbv vbvLeft, Vbv vbvRight)
        {
            Spo spo = new Spo();
            spo.SetupSoln(asbLeft, asbRight, vbvLeft, vbvRight);

            if (spo.fFindOccursTree(vbvLeft))
                return true;

            if (vbvRight != null
                && !(vbvRight == vbvLeft.vbvBSide()
                     || vbvRight == vbvLeft.vbvJoin()))
            {
                if (spo.fFindOccursTree(vbvRight))
                    return true;
            }

            return false;
        }


        public static void MapChildrenVba(Vbv vbvInput, bool fMapLocation, bool fMapValue, 
                                          Vpm vpm, Vbv vbvOutput, Vbv vbvPti, Imp impMapper)
        {
            for (Vbv vbvInputChild = vbvInput.vbvFirst; 
                 vbvInputChild != null; vbvInputChild = vbvInputChild.vbvNext)
            {
                Vbv vbvOutputChild = vpm.vbvValue(vbvInputChild);

                if (fMapLocation)
                {
                    if (vbvInputChild.nReplaceAtPosn != Pmu.nNoReplace)
                    {
                        Vbv vbvValue;
                        impMapper.MapValue(vbvInputChild.nReplaceAtPosn,
                            out vbvOutputChild.nReplaceAtPosn, out vbvValue, vbvOutput, vbvPti);

                        if (vbvValue == Vbv.vbvB)
                        {
                            // reparent from vbvOutput to vbvPti
                            vbvOutput.RemoveChild(vbvOutputChild);
                            vbvPti.AddChild(vbvOutputChild);
                        }
                        else if (vbvValue != Vbv.vbvA && vbvValue != vbvOutput)
                        {
                            Vbv vbvValueMapped = vpm.vbvValue(vbvValue);
                            vbvOutput.RemoveChild(vbvOutputChild);
                            vbvValueMapped.AddChild(vbvOutputChild);
                        }
                    }
                }

                vbvInputChild.MapVbaList(vbvOutputChild, fMapValue, vpm, impMapper,
                                         false /* ? */, vbvOutput, vbvPti);

                MapChildrenVba(vbvInputChild, false, fMapValue, vpm, vbvOutput, vbvPti, impMapper);

            }
        }

        void RemoveChild(Vbv vbvChild)
        {
            Vbv vbvPrev = null;
            for (Vbv vbvOldChild = vbvFirst; vbvOldChild != null; vbvOldChild = vbvOldChild.vbvNext)
            {
                if (vbvOldChild == vbvChild)
                {
                    if (vbvPrev == null)
                        vbvFirst = vbvChild.vbvNext;
                    else
                        vbvPrev.vbvNext = vbvChild.vbvNext;
                    return;
                }
                vbvPrev = vbvOldChild;
            }
        }

        /// <summary>
        /// Combine solutions into one new solution.
        /// </summary>
        public static void MergeVbv(out Vbv vbvOutput, out Vbv vbvPti,
                                    Vbv vbvInput, Vbv vbvPrev, Imp impMapper)
        {
            Vpm vpm = new Vpm(vbvInput, vbvPrev);

            if (vbvPrev != null)
            {
                vbvOutput = vbvPrev.vbvMakeCopy(vpm);

                Vbv.CopyVbvChildren(vbvPrev, vpm, vbvOutput);

                vpm.Add(vbvInput, vbvOutput);
            }
            else
                vbvOutput = vbvInput.vbvMakeCopy(vpm);

            vbvPti = impMapper.vbvPtiFromOutput(vbvPrev, vbvOutput); //  (vbvPrev != null) ? vbvOutput.vbvFirst : null;

            Vbv.CopyVbvChildren(vbvInput, vpm, vbvOutput);

            if (vbvPrev != null)
            {
                // Copy (was Map) the solution from the previous step to the new one.
                vbvPrev.MapVbaList(vbvOutput, false, vpm, impMapper, false, vbvOutput, null);
                Vbv.MapChildrenVba(vbvPrev, false, false, vpm, vbvOutput, null, impMapper);
            }

            vbvInput.MapVbaList(vbvOutput, true, vpm, impMapper, true, vbvOutput, vbvPti);
            Vbv.MapChildrenVba(vbvInput, true, true, vpm, vbvOutput, vbvPti, impMapper);

            vbvOutput.asb = impMapper.asbGet();

            if (vbvOutput.fCheckConflictingReplaceAtPosn(vbvOutput, vbvPti))
                vbvOutput = null;   // indicate failure
        }

        public Lsx lsxTo(Asy asy)
        {
            Lpr lprHead = null;
            Lpr lprTail = null;
            Vba vba = rgvbaList;
            while (vba != null)
            {
                int nValPos = vba.nValue;
                Lsx lsxItem = asb.lsxToTerm(ref nValPos, asy);
                Sko.AddToResult(lsxItem, ref lprHead, ref lprTail);
                vba = vba.vbaPrev;
            }
            return lprHead;
        }

        public void NextSameSize(Esn riiNext)
        {
            vbvNextSameSize = (Vbv) riiNext;
        }

        int nSize()
        {
            int nAcc = 1;
            for (Vba vba = rgvbaList; vba != null; vba = vba.vbaPrev)
            {
                nAcc++;
            }
            for (Vbv vbv = vbvFirst; vbv != null; vbv = vbv.vbvNext)
                nAcc += vbv.nSize();
            return nAcc;
        }

        public Ipr iprComplexity()
        {
            return Prn.prnObtain(nSize());
#if false
            int nAcc = 0;
            Vba vba = rgvbaList;
            while (vba != null)
            {
                int nValSize = asb.nTermSize(vba.nValue);
                nAcc += nValSize;
                vba = vba.vbaPrev;
            }
            return nAcc;
#endif
        }

        public Esn riiNextSameSize()
        {
            return vbvNextSameSize;
        }

        public override void stString(Pctl pctl, Fwt sb)
        {
            if (this == vbvA)
            {
                sb.Append("$A");
                return;
            }
            else if (this == vbvB)
            {
                sb.Append("$B");
                return;
            }

            sb.Append("<");
            sb.Append("#");
            sb.Append(nId);
            sb.Append(" ");

            if (nReplaceAtPosn != Pmu.nNoReplace)
            {
                sb.Append(nReplaceAtPosn);
                sb.Append("/");
                sb.Append(nReplaceWithPosn);
                sb.Append(" ");
            }

            if (pctl.fPretty)
                sb.Indent();

            bool fFirstSub = true;
            for (Vbv rtoSub = vbvFirst; rtoSub != null; rtoSub = rtoSub.vbvNext)
            {
                if (pctl.fPretty)
                {
                    sb.Newline();
                    fFirstSub = false;
                }
                rtoSub.stString(pctl, sb);
            }
            if (pctl.fPretty)
            {
                if (!fFirstSub)
                    sb.Newline();
            }

            bool fFirst = true;
            
            for (Vba vba = rgvbaList; vba != null; vba = vba.vbaPrev)
            {
                if (fFirst)
                    fFirst = false;
                else
                    sb.Append(", ");
                sb.Append(vba.nVblId);
                sb.Append(":");
                sb.Append(vba.nValue);
                if (vba.vbvForValue == vbvA)
                    sb.Append("$A");
                else if (vba.vbvForValue == vbvB)
                    sb.Append("$B");
                else
                {
                    sb.Append("$");
                    sb.Append(vba.vbvForValue.nId);
                }
            }
            sb.Append(">");
            if (pctl.fPretty)
                sb.Unindent();

            if (pctl.fVerbose)
            {
                sb.Append("     ");
                asb.stString(Pctl.pctlPlain, sb);
            }
        }

#if false
        public void BuildOrigins(Fwt sb)
        {
            stString(Pctl.pctlPlain, sb);
            sb.Newline();
            sb.Indent();
            LinkedList<Vbv> rgvbvOld = null;
            if (Vpm.mpvbv_rgvbvOrigin.TryGetValue(this, out rgvbvOld))
            {
                foreach (Vbv vbvOld in rgvbvOld)
                {
                    vbvOld.BuildOrigins(sb);
                }
            }
            sb.Unindent();
        }

        /// <summary>
        /// Show the tree of solutions leading to this 
        /// </summary>
        public void Origins()
        {
            Fwt sb = new Fwt();
            BuildOrigins(sb);
            Debug.WriteLine(sb.stString());
        }
#endif
    }
}
