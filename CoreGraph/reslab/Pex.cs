
using System;
using System.Collections.Generic;

namespace reslab
{
    /// <summary>
    /// Stack of proof steps in progress, to avoid looping, to match what Spb does
    /// </summary>
    public class Pes
    {
        Vbv vbvExclude;
        Pes pesPrev;

        public Pes(Vbv vbvExclude, Pes pesPrev)
        {
            this.vbvExclude = vbvExclude;
            this.pesPrev = pesPrev;
        }

        public static bool fFind(Pes pesTop, Vbv vbvToFind)
        {
            for (Pes pesPlace = pesTop; pesPlace != null; pesPlace = pesPlace.pesPrev)
            {
                if (pesPlace.vbvExclude == vbvToFind)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Proof dependency data. Used to order the order the proof steps
    /// </summary>
    public class Pdd
    {
        public Vbv vbv;
        public long nVbaId;

        public override int GetHashCode()
        {
            return (int) (vbv.nGetId() + nVbaId);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Pdd))
                return false;
            Pdd pddObj = (Pdd)obj;
            if (vbv != pddObj.vbv)
                return false;
            if (nVbaId != pddObj.nVbaId)
                return false;
            return true;
        }

        public override String ToString()
        {
            return "(" + nVbaId + "_" + vbv + ")";
        }
    }

    /// <summary>
    /// Map element for step: has the info about most recent substituted version of a clause.
    /// </summary>
    public class Mes
    {
        public Vbv vbv;
        public Pvi pviInitial;
        public Pvb pvbLatest;
        public Mes mesPrev;
    }

    /// <summary>
    /// Map vbv to the most recent proof step for that clause
    /// </summary>
    public class Mrs
    {
        Mes mesFirst;
        public Mes mesLeft = new Mes();
        public Mes mesRight = new Mes();
        public readonly Pvm pvmLocal;
        sbyte[] rgnRevisedId;
        sbyte nEndRevisedId;
        public Mvf mvfForCut;
        public byte nAddedNegLiteralsLeft = 0;  // need to shift mask for neg literals added in paramodulation
        public byte nAddedNegLiteralsRight = 0;
        public uint nLeftMask;
        public uint nRightMask;
        public Vbv vbvLeft;
        public Vbv vbvRight;

        public Vbv vbvSoln;   // Vbv that can match vbvA or vbvB

        const int nMaxIds = sbyte.MaxValue;

        public Mrs(Pvm pvm)
        {
            this.pvmLocal = pvm;
        }

        public void Add (Pvb pvb)
        {
            pvmLocal.Add(pvb);
        }

#if false
        /// <summary>
        /// Return true if the rte vbvChild should be skipped at nPosn in vbvPlace, 
        /// for the same reasons that Spb.fSkipReplacement does: if the symbol at nPosn
        /// is the value of a vbl in the pti that is being considered to apply. 
        /// </summary>
        public bool fSkipReplacement(Vbv vbvPlace, ushort nPosn, Vbv vbvChild)
        {

            // find theMof, and see what nPosn in vbvPlace is mapped to, 
            // if it is the result of vba in vbvChild.pti



            ushort nReplaceFrom;
            Vbv vbvSourceOfTerm;

            Mes mesSource = mesLatestForVbv(vbvPlace);
            Spl.MapValue(mesSource.pvbLatest.mofOutput, nPosn,
                         out nReplaceFrom, out vbvSourceOfTerm);

            if (vbvSourceOfTerm == vbvChild)
                return true;
            return false;
        }
#endif

        /// <summary>
        /// Set up an Mrs for performing an inference
        /// </summary>
        public static Mrs mrsStartForAsc(Pex pex, Pvm pvmMain, Asc ascInferred, Mvf mvfForCut)
        {
            Asc ascLeft = (Asc)ascInferred.ribLeft;
            Asc ascRight = (Asc)ascInferred.ribRight;
            Vbv vbvSoln = (Vbv)ascInferred.esnSolution;

            Mrs mrs = new Mrs(new Pvm());   // hold pvb added in the step, so nVblIds can be revised, once
            mrs.mvfForCut = mvfForCut;
            mrs.nLeftMask = ascInferred.nLeftMask;
            mrs.nRightMask = ascInferred.nRightMask;
            Pvi pviLeft = mrs.pviMake(pex.pvbCreateProofSteps(pvmMain, ascLeft), 
                                      Vbv.vbvA, Vbv.vbvA);
            Pvi pviRight = mrs.pviMake(pex.pvbCreateProofSteps(pvmMain, ascRight),
                                      Vbv.vbvB, Vbv.vbvB);

#if DEBUG
            if (!ascLeft.Equals(pviLeft.pvbPrevStep.ascResult))
                throw new ArgumentException();
            if (!ascRight.Equals(pviRight.pvbPrevStep.ascResult))
                throw new ArgumentException();
#endif
            mrs.mesLeft.pviInitial = pviLeft;
            mrs.mesLeft.pvbLatest = pviLeft;
            mrs.mesRight.pviInitial = pviRight;
            mrs.mesRight.pvbLatest = pviRight;
            mrs.Add(pviRight);
            mrs.Add(pviLeft);
            mrs.vbvSoln = vbvSoln;

            return mrs;
        }

        /// <summary>
        /// Create a Pvi for each Pvb that is used as input to an inference so that non-conflicting vblIds are assigned.
        /// </summary>
        public Pvi pviMake(Pvb pvbPrev, Vbv vbvForVbls, Vpo ipoOrigin)
        {
            Pvi pvi = new Pvi(pvbPrev, mvfForCut, vbvForVbls, ipoOrigin);

            pvi.fPerform(null, null, null);
            return pvi;
        }

        public Mes mesSetPvbForVbv(Vbv vbv, Pvb pvb)
        {
            Mes mes;
            if (vbv == vbvSoln)
                mes = mesLeft;
            else if (vbv == vbvSoln.vbvBSide())
                mes = mesRight;
            else if (vbv == vbvSoln.vbvJoin())
                mes = mesRight;
            else if (vbv.fNeedsMapping())
            {
                if (vbv == Vbv.vbvA)
                    mes = mesLeft;
                else if (vbv == Vbv.vbvB)
                    mes = mesRight;
                else
                    throw new ArgumentException();
            }
            else
            {
                mes = mesFirst;
                while (mes != null)
                {
                    if (mes.vbv == vbv)
                        break;
                    mes = mes.mesPrev;
                }
                if (mes == null)
                {
                    mes = new Mes();
                    mes.mesPrev = mesFirst;
                    mes.vbv = vbv;
                    mesFirst = mes;
                }
            }
            mes.pvbLatest = pvb;
            return mes;
        }

        public Mes mesLatestForVbv(Vbv vbv)
        {
            Mrs mrs = this;

            Mes mes = mesFirst;

            if (vbv == Vbv.vbvA || vbv == mrs.vbvSoln)
                return mesLeft;
            else if (vbv == Vbv.vbvB
                    || vbv == mrs.vbvSoln.vbvBSide()
                    || vbv == mrs.vbvSoln.vbvJoin())
                return mesRight;
            else
            {
                mes = mesFirst;
                while (mes != null)
                {
                    if (mes.vbv == vbv)
                        return mes;
                    mes = mes.mesPrev;
                }

                Pvi pvi = mrs.pviMake(new Pva ((Asc)vbv.asb, vbv), vbv, vbv);
                mrs.pvmLocal.Add(pvi);
                mrs.mesSetPvbForVbv(vbv, pvi);
                mes = mesSetPvbForVbv(vbv, pvi);
                mes.pviInitial = pvi;
                return mes;
            }
        }

        /// <summary>
        /// Obtain sequential id for each 
        /// </summary>
        public void AssignReplacementIds(Asc ascResult)
        {
            sbyte[] rgnTree = ascResult.rgnTree;
            rgnRevisedId = new sbyte[nMaxIds];
            nEndRevisedId = 0;
            mvfForCut.nNextVblId = Atp.nVar;
            for (int nPos = Asc.nClauseLeadingSizeNumbers; nPos < rgnTree.Length; nPos++)
            {
                sbyte nId = rgnTree[nPos];
                if (nId >= Atp.nVar)
                    nReplacementId(nId);
            }
        }

        public sbyte nReplacementId(sbyte nOldId)
        {
            while (nEndRevisedId <= nOldId)
            {
                rgnRevisedId[nEndRevisedId] = Tde.nNoVblId;
                nEndRevisedId++;
            }

            sbyte nRevised = rgnRevisedId[nOldId];
            if (nRevised == Tde.nNoVblId)
            {
                nRevised = mvfForCut.nNextVblId++;
                rgnRevisedId[nOldId] = nRevised;
            }
            return nRevised;
        }

    }


    /// <summary>
    /// Traverse the results of proof search and generate a series of proof steps.
    /// </summary>
    public class Pex
    {
#if DEBUG
        List<Pvb> rgpvbAll = new List<Pvb>();
#endif
        void MapPosition(Vbv vbvSource, Mrs mrs, ushort nPosnToMap,
                         out Pvb pvbSource, out ushort nMappedPosition, out Vbv vbvMappedSource)
        {
            Mes mesSource = mrs.mesLatestForVbv(vbvSource);
            pvbSource = mesSource.pvbLatest;
            while (pvbSource != null)
            {
                Spl.MapValue(pvbSource.mofOutput, nPosnToMap,
                             out nMappedPosition, out vbvMappedSource);
                if (nMappedPosition != 0)
                {
                    return;
                }
                pvbSource = pvbSource.pvbPrevSameVbv;
            }
            throw new ArgumentException();
        }

        void makeReplacementStep(Vbv vbvInto, Vbv vbvSource, Mrs mrs)
        {
            if (fCheckForDup(vbvInto, vbvSource.nId, mrs))
                return;

            // get pvb for dest (into) term

            Pvb pvbSource;
            ushort nMappedReplaceFrom;
            Vbv vbvSourceOfFrom;
            MapPosition(vbvSource, mrs, vbvSource.nReplaceFromPosn, 
                        out pvbSource, out nMappedReplaceFrom, out vbvSourceOfFrom);

            Pvb pvbInto;
            ushort nMappedReplaceAt;
            Vbv vbvIntoAt;
            MapPosition(vbvInto, mrs, vbvSource.nReplaceAtPosn,
                        out pvbInto, out nMappedReplaceAt, out vbvIntoAt);

            ushort nMappedReplaceWith;
            Vbv vbvSourceOfWith;
            MapPosition(vbvSource, mrs, vbvSource.nReplaceWithPosn,
                        out pvbSource, out nMappedReplaceWith, out vbvSourceOfWith);

            Pvp pvpStep = new Pvp(pvbInto, pvbSource,
                                  nMappedReplaceAt, nMappedReplaceFrom, nMappedReplaceWith,
                                  vbvSource);
            mrs.Add(pvpStep);

            if (!pvpStep.fPerform(mrs, mrs.mvfForCut, vbvInto))
                throw new ArgumentException();

            mrs.mesSetPvbForVbv(vbvInto, pvpStep);

        }
        public void MakeSubstitutionStep(Vbv vbvPlace, Vbv vbvOwnsVba, Vba vba, Mrs mrs)
        {
            // get pvb for dest (into) term
            Mes mesInto = mrs.mesLatestForVbv(vbvPlace);

            Mes mesOwner = mrs.mesLatestForVbv(vbvOwnsVba);

            Vbv vbvPlaceAdj;
            if (vbvPlace == mrs.vbvSoln)
                vbvPlaceAdj = Vbv.vbvA;
            else if (vbvPlace == mrs.vbvSoln.vbvBSide()
                    || vbvPlace == mrs.vbvSoln.vbvJoin())
                vbvPlaceAdj = Vbv.vbvB;
            else
                vbvPlaceAdj = vbvPlace;

            // get the id assigned to vbl that is unique across all inputs to current pvc/pbp proof step
            sbyte nVblIdShared;
            nVblIdShared = mesOwner.pviInitial.rgnSharedId[vba.nVblId];

            // map the sharedId forward through changes that have occured since the pvi
            sbyte vVblIdInto = nVblIdShared;   // there are no changes

            ushort nReplaceFrom;
            Vbv vbvSourceOfTerm;

            Pvb pvbSource;
            MapPosition(vba.vbvForValue, mrs, vba.nValue,
                        out pvbSource, out nReplaceFrom, out vbvSourceOfTerm);

            if (!mesInto.pvbLatest.ascResult.fAppears(vVblIdInto))
                return;
            Pvs pvsStep = 
                 new Pvs(mesInto.pvbLatest, pvbSource, vVblIdInto, nReplaceFrom, 
                         vba, mesOwner.pvbLatest);
            mrs.Add(pvsStep);

            // if vbl maps to left/right, update that to be latest pvb for that side
            // -update the mob and mvb - the resulting rgnTree has different relationship to the vbl ids and offsets in this.
            if (!pvsStep.fPerform(mrs, mrs.mvfForCut, vbvPlace))
                throw new ArgumentException();

            mrs.mesSetPvbForVbv(vbvPlace, pvsStep);


        }

        /// <summary>
        /// Perform some handling around replacement, then make call to create the step
        /// </summary>
        void applyReplacementStep(Vbv vbvInto, Vbv vbvSource, Mrs mrs, Pes pesExclude)
        {
            Pes pesSource = new Pes(vbvSource, pesExclude);

            // do replacements at site of the replacement
            if (vbvSource.nReplaceAtPosn != Pmu.nNoReplace)
            {
                Pvb pvbInto;
                ushort nMappedReplaceAt;
                Vbv vbvSourceOfAt;
                MapPosition(vbvInto, mrs, vbvSource.nReplaceAtPosn,  
                            out pvbInto, out nMappedReplaceAt, out vbvSourceOfAt);

                ApplyInTerm(vbvInto, pvbInto.ascResult, nMappedReplaceAt, mrs,
                            pesSource);
            }

            // do replacements in from part of pti so that there is no conflict
            // do the mapping each time, in case some intermediate step moves things around
            Pvb pvbSource;
            ushort nMappedReplaceFrom;
            Vbv vbvSourceOfFrom;
            MapPosition(vbvSource, mrs, vbvSource.nReplaceFromPosn, 
                        out pvbSource, out nMappedReplaceFrom, out vbvSourceOfFrom);
            ApplyInTerm(vbvSource, pvbSource.ascResult, nMappedReplaceFrom, mrs,
                        pesSource);

            Pvb pvbReplaceWith;
            ushort nMappedReplaceWith;
            Vbv vbvSourceOfWith;
            MapPosition(vbvSource, mrs, vbvSource.nReplaceWithPosn, 
                        out pvbReplaceWith, out nMappedReplaceWith, out vbvSourceOfWith);

            ApplyInTerm(vbvSource, pvbReplaceWith.ascResult, nMappedReplaceWith, mrs,
                        pesSource);

            makeReplacementStep(vbvInto, vbvSource, mrs);
        }

        /// <summary>
        /// Create a step that corresponds to a Vba
        /// </summary>
        void ApplyVba(Vbv vbvPlace, Vbv vbvOwnsVba, Vba vbaPlace, Mrs mrs, Pes pesExclude)
        {
            if (fCheckForDup(vbvPlace, vbaPlace.nId, mrs))
                return;

            Vbv vbvValue = vbaPlace.vbvForValue;

            Mes mesForValue = mrs.mesLatestForVbv(vbvValue);

            ushort nMappedValue;
            Vbv vbvMapped;
            mesForValue.pvbLatest.mofOutput.Lookup(vbaPlace.nValue, out nMappedValue, out vbvMapped);

            Vbv vbvForVbas;
            if (vbvMapped == Vbv.vbvA)
            {
                if (vbvValue == Vbv.vbvA)
                    vbvForVbas = mrs.vbvSoln;
                else
                    vbvForVbas = vbvValue;   // not vbvSoln, the 0 index in mof just means self
            }
            else if (vbvMapped == Vbv.vbvB)
                vbvForVbas = mrs.vbvSoln.vbvBSide();
            else
                vbvForVbas = vbvMapped;

            ApplyInTerm(vbvForVbas, mesForValue.pvbLatest.ascResult, nMappedValue, mrs, pesExclude);

            // on way back up, do actual substitution for this vba
            MakeSubstitutionStep(vbvPlace, vbvOwnsVba, vbaPlace, mrs);
        }


        /// <summary>
        /// Iterate through symbols in value term and create Pvb for each substitution or replacement
        /// that needs to be performed first.
        /// </summary>
        void ApplyInTerm(Vbv vbvPlace, Asb asbForValue, ushort nTermOffset, Mrs mrs, Pes pesExclude)
        {
            ushort nTermLimit = (ushort) (nTermOffset + asbForValue.nTermSize(nTermOffset));

            bool fSkipPtiOnFirst = true;

            for (ushort nPosn = nTermOffset; nPosn < nTermLimit; )
            {
                // if (nPosn > nTermOffset)
                if (!fSkipPtiOnFirst || nPosn != nTermOffset)
                {
                    bool fReplaced = false;
                    for (Vbv vbvChild = vbvPlace.vbvFirst; vbvChild != null;
                         vbvChild = vbvChild.vbvNext)
                    {
                        Pvb pvbChild;
                        ushort nMappedReplaceAt;
                        Vbv vbvSourceOfAt;
                        MapPosition(vbvPlace, mrs, vbvChild.nReplaceAtPosn, 
                                    out pvbChild, out nMappedReplaceAt, out vbvSourceOfAt);

                        if (!Pes.fFind(pesExclude, vbvChild)
                            && nMappedReplaceAt == nPosn)
                            // && !mrs.fSkipReplacement(vbvPlace, nPosn, vbvChild))
                        {
                            applyReplacementStep(vbvPlace, vbvChild, mrs, pesExclude);
                            nPosn += asbForValue.nTermSize(nPosn);
                            fReplaced = true;
                            break;
                        }
                    }
                    if (fReplaced)
                        continue;
                }

                sbyte nValueId = asbForValue.rgnTree[nPosn];
                if (nValueId >= Atp.nVar)
                {
                    sbyte nVblIdSoln;
                    Vbv vbvSource;
                    Mvfi.fMapOutputToSource(mrs.mvfForCut.mvfiList, nValueId,
                                            out nVblIdSoln, out vbvSource);

                    Vba vbaForVbl = vbvPlace.vbaFind(nVblIdSoln);
                    if (vbaForVbl != null)
                    {
                        ApplyVba(vbvPlace, vbvPlace, vbaForVbl, mrs, pesExclude);
                    }
                }
                nPosn++;
            }

        }

        bool fCheckForDup (Vbv vbvPlace, long nStepId, Mrs mrs)
        {
            Pdd pdd = new Pdd();
            pdd.vbv = vbvPlace;
            pdd.nVbaId = nStepId;
            if (mrs.mvfForCut.rgpddVisited.Contains(pdd))
                return true;
            mrs.mvfForCut.rgpddVisited.Add(pdd);
            return false;
        }


        void ApplyVbaToTree(Vbv vbvOwnsVba, Vba vbaToApply, Mrs mrs, Vbv vbvPlace, Pes pesExclude)
        {
            ApplyVba(vbvPlace, vbvOwnsVba, vbaToApply, mrs, pesExclude);
            for (Vbv vbvChild = vbvPlace.vbvFirst; vbvChild != null; vbvChild = vbvChild.vbvNext)
            {
                ApplyVbaToTree(vbvOwnsVba, vbaToApply, mrs, vbvChild, pesExclude);
            }
        }

        /// <summary>
        /// Walk down tree and create an ordered list of substitutions, so that each expansion does
        /// not depend on vbl that has not yet been expanded.
        /// </summary>
        /// <param name="vbvPlace"></param>
        void ApplySolution(Vbv vbvParent, Vbv vbvPlace, Mrs mrs, bool fFinalPass, Pes pesExclude)
        {
#if false
            // needed only for ProveEq1Para?
            if (!fFinalPass)
            {
                if (vbvPlace.nReplaceAtPosn != Pmu.nNoReplace)
                {
                    ApplyInTerm(vbvParent, vbvParent.asb, vbvPlace.nReplaceAtPosn, mrs,
                                new Pes(vbvPlace, pesExclude));
                    ApplyInTerm(vbvPlace, vbvPlace.asb, vbvPlace.nReplaceFromPosn, mrs, pesExclude);
                    ApplyInTerm(vbvPlace, vbvPlace.asb, vbvPlace.nReplaceWithPosn, mrs, pesExclude);

                }
            }
#endif

            for (Vbv vbvChild = vbvPlace.vbvFirst; vbvChild != null; vbvChild = vbvChild.vbvNext)
            {
                // if (!vbvChild.fJoinPti)
                    ApplySolution(vbvPlace, vbvChild, mrs, fFinalPass, pesExclude);
            }

            // afterwards, pick up all vba that were not already referenced.
            if (fFinalPass)
            {
                for (Vba vbaPlace = vbvPlace.rgvbaList; vbaPlace != null; vbaPlace = vbaPlace.vbaPrev)
                    //ApplyVba(mrs.vbvSoln, vbvPlace, vbaPlace, mrs, pesExclude);
                   ApplyVbaToTree(vbvPlace, vbaPlace, mrs, mrs.vbvSoln, pesExclude);

                if (!vbvPlace.fJoinPti
                    &&
                    vbvPlace.nReplaceAtPosn != Pmu.nNoReplace)
                {
                    // is this redudant with the other call now?
                    applyReplacementStep(vbvParent, vbvPlace, mrs, pesExclude);
                }
            }
        }

        /// <summary>
        /// Make sure that Pvi are created for each vbv, because that will cause
        /// mvf.nNextVblId to include all vbls.
        /// </summary>
        static void CreateRemainingPvi(Mrs mrs, Vbv vbvPlace)
        {
            for (Vbv vbvChild = vbvPlace.vbvFirst; vbvChild != null; vbvChild = vbvChild.vbvNext)
            {
                mrs.mesLatestForVbv(vbvChild);
                CreateRemainingPvi(mrs, vbvChild);
            }
        }

        public Pvb pvbCreateProofSteps(Pvm pvmMain, Asc ascInferred)
        {
            Pvb pvbPrev = pvmMain.pvbFind(ascInferred);
            if (pvbPrev != null)
                return pvbPrev;

            Vbv vbvSoln = (Vbv)ascInferred.esnSolution;
            if (ascInferred.gfbSource.fIsGiven())
            {
                Pva pva = new Pva(ascInferred, vbvSoln);
                pvmMain.Add(pva, ascInferred);
                return pva;
            }

            Mvf mvfForCut = new Mvf();
            Mrs mrs = Mrs.mrsStartForAsc(this, pvmMain, ascInferred, mvfForCut);
            bool fCut = vbvSoln.vbvBSide() != null;
            mrs.vbvLeft = vbvSoln;
            mrs.vbvRight = fCut ? vbvSoln.vbvBSide() : vbvSoln.vbvJoin();

            CreateRemainingPvi(mrs, vbvSoln);

            ApplySolution(null, vbvSoln, mrs, false, null);
            ApplySolution(null, vbvSoln, mrs, true, null);

            // check again
            pvbPrev = pvmMain.pvbFind(ascInferred);
            if (pvbPrev != null)
                return pvbPrev;

            Pvb pvbInferred;
            if (fCut)
                pvbInferred = new Pvc(mrs, ascInferred, vbvSoln);
            else
                pvbInferred = Pvp.pvpForJoin(mrs, vbvSoln);

            if (!pvbInferred.fPerform(null, mvfForCut, null))   // get ascResult
                throw new ArgumentException();
            if (!fCut)
                pvbInferred.NormalizeVblIds();
            if (!pvbInferred.ascResult.Equals(ascInferred))
                throw new ArgumentException();

            mrs.AssignReplacementIds(pvbInferred.ascResult);
            mrs.pvmLocal.ApplyRevisedIds(mrs);

            pvmMain.Move(mrs.pvmLocal);


            if (ascInferred.fMatches(pvbInferred.ascResult, true))
                pvmMain.Add(pvbInferred, ascInferred);
            else
            {
                pvmMain.Add(pvbInferred);
                pvbInferred = new Pva(ascInferred, vbvSoln);
                pvmMain.Add(pvbInferred, ascInferred);
            }

            return pvbInferred;
        }

        public void Verify(Pvm pvmMain)
        {
            if (!pvmMain.fPerform())
                throw new ArgumentException();
            if (!pvmMain.ascResult.fEmpty())
                throw new ArgumentException();
        }
    }
}
