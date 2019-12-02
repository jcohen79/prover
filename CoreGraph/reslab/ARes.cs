#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reslab
{

    /*
     * Class for misc functions that can take nil or a cons. Each fn checks
     * Class can hold symbol list
     */
    public class ARes
    {
        int cTime = 0;

        static string[] rgst_StandardNames = { "X", "Y", "Z", "U", "V", "W", "R", "S", "T", "XX", "YY", "ZZ", "UU", "VV" };
        Lpr lprStandardNames;

        public ARes()
        {
            Lsx lsxPrev = Lsm.lsmNil;
            for (int i = rgst_StandardNames.Length - 1; i >= 0; i--)
            {
                lprStandardNames = new Lpr(new Lsm(rgst_StandardNames[i]), lsxPrev);
                lsxPrev = lprStandardNames;
            }
        }

        public void MakeVariables(Lsx lsxList)
        {
            while (lsxList != Lsm.lsmNil)
            {
                Lpr lpr = (Lpr)lsxList;
                ((Lsm)lpr.lsxCar).MakeVariable();
                lsxList = lpr.lsxCdr;
            }
        }
        public void UnmakeVariables(Lsx lsxList)
        {
            while (lsxList != Lsm.lsmNil)
            {
                Lpr lpr = (Lpr)lsxList;
                ((Lsm)lpr.lsxCar).UnmakeVariable();
                lsxList = lpr.lsxCdr;
            }
        }
        public Lsx lsxEnsemble(Lsx lsxArg)
        {
            if (!(lsxArg is Lpr))
                return Lsm.lsmNil;
            Lpr lprArg = (Lpr)lsxArg;
            if (lprArg.lsxCdr.fMember(lprArg.lsxCar))
                return lsxEnsemble(lprArg.lsxCdr);
            return new Lpr(lprArg.lsxCar, lsxEnsemble(lprArg.lsxCdr));
        }
        public Lsx lsxUnion(Lsx lsxA, Lsx lsxB)
        {
            if (!(lsxA is Lpr))
                return lsxEnsemble(lsxB);
            Lpr lprA = (Lpr)lsxA;
            if (lprA.lsxCdr.fMember(lprA.lsxCar))
                return lsxUnion(lprA.lsxCdr, lsxB);
            if (lsxB.fMember(lprA.lsxCar))
                return lsxUnion(lprA.lsxCdr, lsxB);
            return new Lpr(lprA.lsxCar, lsxUnion(lprA.lsxCdr, lsxB));
        }
        public Lsx lsxDelete(Lsx lsxMem, Lsx lsxList)
        {
            if (!(lsxList is Lpr))
                return Lsm.lsmNil;
            Lpr lprList = (Lpr)lsxList;
            if (lprList.lsxCar.fEqual(lsxMem))
                return lsxDelete(lsxMem, lprList.lsxCdr);
            return new Lpr(lprList.lsxCar, lsxDelete(lsxMem, lprList.lsxCdr));
        }
        public Lsx lsxPower(Lsx lsxList)  // pg 247
        {
            if (!(lsxList is Lpr))
                return Lsm.lsmNil;
            Lpr lprList = (Lpr)lsxList;
            if (!(lprList.lsxCdr is Lpr))
                return new Lpr(lprList, Lsm.lsmNil); // L is singleton list -> {L}
            Lsx lsxPowerRest = lsxPower(lprList.lsxCdr);
            return new Lpr(new Lpr(lprList.lsxCar, Lsm.lsmNil),
                           lsxDist(lprList.lsxCar, lsxPowerRest,
                                   lsxPowerRest));
        }
        public Lsx lsxDist(Lsx lsxHead, Lsx lsxTails, Lsx lsxExtra)  // pg 247
        {
            if (!(lsxTails is Lpr))
                return lsxExtra;
            Lpr lprTails = (Lpr)lsxTails;
            return new Lpr(new Lpr(lsxHead, lprTails.lsxCar),
                           lsxDist(lsxHead, lprTails.lsxCdr, lsxExtra));
        }
        public Lsx lsxComponent(Lsx lsxList, int nPos)  // pg 248
        {
            int c = nPos;
            while (lsxList != Lsm.lsmNil)
            {
                Lpr lpr = (Lpr)lsxList;
                if (c == 1)
                    return lpr.lsxCar;
                lsxList = lpr.lsxCdr;
                c--;
            }
            throw new ArgumentOutOfRangeException();
        }
        public Lsx lsxVariables(Lsx lsxList)  // pg 249
        {
            if (lsxList == Lsm.lsmNil)
                return Lsm.lsmNil;
            if (lsxList is Lsm)
            {
                Lsm lsm = (Lsm)lsxList;
                if (lsm.fVariable())
                    return new Lpr(lsm, Lsm.lsmNil);
                return Lsm.lsmNil;
            }
            if (lsxList == Lsm.lsmNil)
                return Lsm.lsmNil;
            Lpr lprList = (Lpr)lsxList;
            return lsxUnion(lsxVariables(lprList.lsxCar), lsxVariables(lprList.lsxCdr));
        }

        public Lsx lsxAppend(Lsx lsxA, Lsx lsxB)
        {
            if (lsxA == Lsm.lsmNil)
                return lsxB;
            Lpr lprA = (Lpr)lsxA;
            return new Lpr(lprA.lsxCar, lsxAppend(lprA.lsxCdr, lsxB));
        }
        // transform sequence S into a list of sequents obtainable by adding S to one of the clauses in C
        public Lsx lsxInferences(Lsx lsxS, Lsx lsxC)  // pg 254
        {
            if (lsxC == Lsm.lsmNil)
                return Lsm.lsmNil;
            Lpr lprC = (Lpr)lsxC;
            return new Lpr(lsxAppend(lsxS, new Lpr(lprC.lsxCar, Lsm.lsmNil)),
                           lsxInferences(lsxS, lprC.lsxCdr));
        }

        // ListSubsumes: determine if given clause is subsumed by any clause in given list (= sequent) of clauses
        //    walk list of clauses in S, if one of them subsumes C, the result is true
        public bool fListSubsumes(Lsx lsxS, Lsx lsxC)  // pg 256
        {
            while (lsxS != Lsm.lsmNil)
            {
                Lpr lprS = (Lpr)lsxS;
                if (fSubsumes(lprS.lsxCar, lsxC))
                    return true;
                lsxS = lprS.lsxCdr;
            }
            return false;
        }

        // return list of clauses in L that are not subsumed by any clause in S
        public Lsx lsxFilter(Lsx lsxL, Lsx lsxS)  // pg 256
        {
            if (lsxL == Lsm.lsmNil)
                return Lsm.lsmNil;
            Lpr lprL = (Lpr)lsxL;
            if (fListSubsumes(lsxS, lprL.lsxCar))
                return lsxFilter(lprL.lsxCdr, lsxS);
            return new Lpr(lprL.lsxCar,
                           lsxFilter(lprL.lsxCdr, lsxS));
        }

        public Lsx lsxP1Resolve(Lsx lsxC1, Lsx lsxC2)  // pg 259
        {
            Lpr lprC1 = (Lpr)lsxC1;
            Lpr lprC2 = (Lpr)lsxC2;
            Lpr lprC2a = (Lpr)lprC2.lsxCar;
            return lsxQResolve(lprC1, lsxPrune(lsxPower(lprC1.lsxCdr)),
                               lprC2, new Lpr(lprC2a.lsxCar, Lsm.lsmNil));
        }
        public Lsx lsxPrune(Lsx lsxL)  // pg 260
        {
            if (lsxL == Lsm.lsmNil)
                return Lsm.lsmNil;
            Lpr lprL = (Lpr)lsxL;
            if (fUnify(lprL.lsxCar, new Li(cTime++)))
                return new Lpr(lprL.lsxCar, lsxPrune(lprL.lsxCdr));
            return lsxPrune(lprL.lsxCdr);
        }
        public Lsx lsxQResolve(Lsx lsxA, Lsx lsxAList, Lsx lsxB, Lsx lsxBList)  // pg 262
        {
            if (lsxAList == Lsm.lsmNil)
                return Lsm.lsmNil;
            Lpr lprAList = (Lpr)lsxAList;
            Lsx lsxR = lsxPResolve(lsxA, lprAList.lsxCar, lsxB, lsxBList, new Li(cTime++));
            if (lsxR == Lsm.lsmNil)
                return lsxQResolve(lsxA, lprAList.lsxCdr, lsxB, lsxBList);
            return new Lpr(lsxR, lsxQResolve(lsxA, lprAList.lsxCdr, lsxB, lsxBList));
        }
        public bool fTautology(Lsx lsxC)  // pg 263
        {
            Lpr lprC = (Lpr)lsxC;
            if (lprC.lsxCar == Lsm.lsmNil)
                return false;
            Lpr lprCA = (Lpr)lprC.lsxCar;
            if (lprC.lsxCdr.fMember(lprCA.lsxCar))
                return true;
            return fTautology(new Lpr(lprCA.lsxCdr, lprC.lsxCdr));
        }
        public Lsx lsxPResolve(Lsx lsxLeftClause, Lsx lsxLeftField, Lsx lsxRightClause, Lsx lsxRightField, Li liTag)  // pg 263
        {
            if (fUnify(lsxAppend(lsxLeftField, lsxRightField), liTag))
            {
                Lpr lprLeftField = (Lpr)lsxLeftField;
                Lsx lsxK = lsxShow(lprLeftField.lsxCar, liTag);
                Lpr lprLC = (Lpr)lsxShow(lsxLeftClause, liTag);
                Lpr lprRC = (Lpr)lsxShow(lsxRightClause, liTag);
                Li liNewTag = new Li(cTime++);
                Lsx lsxR =
                    new Lrs(lsxNew(lsxUnion(lprLC.lsxCar, lsxDelete(lsxK, lprRC.lsxCar)), liNewTag),
                            lsxNew(lsxUnion(lsxDelete(lsxK, lprLC.lsxCdr), lprRC.lsxCdr), liNewTag),
                            lsxLC: lsxLeftClause,
                            lsxRC: lsxRightClause,
                            lsxK: lprLeftField.lsxCar,
                            liV: liTag,
                            liNew: liNewTag
                            );
                if (fTautology(lsxR))
                    return Lsm.lsmNil;
                return lsxR;
            }
            return Lsm.lsmNil;
        }
        public bool fUnify(Lsx lsxL, Li liTag)  // pg 264
        {
            Lpr lprL = (Lpr)lsxL;
            if (lprL.lsxCdr == Lsm.lsmNil)
                return true;
            Lpr lprLD = (Lpr)lprL.lsxCdr;
            if (fEquate(lprL.lsxCar, lprLD.lsxCar, liTag))
                return fUnify(lprL.lsxCdr, liTag);
            return false;
        }
        // if Lpr, lprV is (in reverse order) a list of substitutions
        public void Sbst(Lsx lsxA, Lsx lsxB, Li liV)  // pg 265
        {
            Lsm lsmA = (Lsm)lsxA;
            if (lsmA == Lsm.lsmNil)
                throw new Exception();
            lsmA.SetValue(lsxB);
            lsmA.liIsBound = liV;
            Lbv lbvNew = new Lbv(lsmA, lsxB, liV.livBound);
            liV.livBound = lbvNew;
        }
        public bool fIsBound(Lsx lsxA, Li liV)  // pg 266
        {
            Lsm lsmA = (Lsm)lsxA;
            if (lsmA.liIsBound == null)
                return false;
            return liV.fExtends(lsmA.liIsBound);
        }
        public Lsx lsxShow(Lsx lsxE, Li liV)  // pg 267
        {
            if (lsxE.fAtom())
            {
                if (fIsBound(lsxE, liV))
                    return lsxShow(lsxE.lsxEval(), liV);
                return lsxE;
            }
            if (lsxE is Lrs)
            {
                Lrs lrsE = (Lrs)lsxE;
                return new Lrs(lsxShow(lrsE.lsxCar, liV),
                               lsxShow(lrsE.lsxCdr, liV),
                               lsxLC: lrsE.lsxLC,
                               lsxRC: lrsE.lsxRC,
                               lsxK: lrsE.lsxK,
                               liV: lrsE.liV,
                               liNew: liV);
            }
            Lpr lprE = (Lpr)lsxE;
            return new Lpr(lsxShow(lprE.lsxCar, liV),
                           lsxShow(lprE.lsxCdr, liV));
        }
        public bool fOccur(Lsx lsxX, Lsx lsxE, Li liTheta)  // pg 268
        {
            if (lsxE == Lsm.lsmNil)
                return false;
            if (lsxE.fAtom())
            {
                if (fIsBound(lsxE, liTheta))
                    return fOccur(lsxX, lsxE.lsxEval(), liTheta);
                return lsxX.fEqual(lsxE);
            }
            Lpr lprE = (Lpr)lsxE;
            if (fOccur(lsxX, lprE.lsxCar, liTheta))
                return true;
            return fOccur(lsxX, lprE.lsxCdr, liTheta);
        }
        public Lsm lsmGensym()  // pg 268
        {
            int c = cTime++;
            return new Lsm(Lsm.stVarPrefix + c.ToString());
        }
        public Lsx lsxNew(Lsx lsxE, Li liV)  // pg 269
        {
            if (lsxE.fAtom())
            {
                if (fIsBound(lsxE, liV))
                    return lsxE.lsxEval();
                if (((Lsm)lsxE).fVariable())
                {
                    Lsm lsmNewVariable = lsmGensym();
                    lsmNewVariable.MakeVariable();
                    Sbst(lsxE, lsmNewVariable, liV);
                    return lsmNewVariable;
                }
                return lsxE;
            }
            Lpr lprE = (Lpr)lsxE;
            return new Lpr(lsxNew(lprE.lsxCar, liV), lsxNew(lprE.lsxCdr, liV));
        }
        public Lsx lsxStandard(Lsx lsxE)  // pg 270
        {
            Li liTheta = new Li(cTime++);
            SbstList(lsxVariables(lsxE), lprStandardNames, liTheta);
            return lsxShow(lsxE, liTheta);
        }
        public void SbstList(Lsx lsxVars, Lpr lprTerms, Li liTag)  // pg 270
        {
            if (lsxVars == Lsm.lsmNil)
                return;
            Lpr lprVars = (Lpr)lsxVars;
            Sbst(lprVars.lsxCar, lprTerms.lsxCar, liTag);
            SbstList(lprVars.lsxCdr, (Lpr)lprTerms.lsxCdr, liTag);
        }
        public Lsx lsxAntiquate(Lsx lsxC)  // pg 273
        {
            Lpr lprC = (Lpr)lsxC;
            return lsxUnion(lsxNegations(lprC.lsxCar), lprC.lsxCdr);
        }
        public Lsx lsxNegations(Lsx lsxL)  // pg 273
        {
            if (lsxL == Lsm.lsmNil)
                return Lsm.lsmNil;
            Lpr lprL = (Lpr)lsxL;
            return new Lpr(new Lpr(Lsm.lsmNot, new Lpr(lprL.lsxCar, Lsm.lsmNil)),
                           lsxNegations(lprL.lsxCdr));
        }

        public bool fEquate(Lsx lsxA, Lsx lsxB, Li liV)  // pg 271
        {
            while (true)
            {
                if (lsxA.fAtom())
                {
                    if (fIsBound(lsxA, liV))
                    {
                        lsxA = lsxA.lsxEval();
                        continue;
                    }
                    else if (lsxB.fAtom())
                    {
                        if (fIsBound(lsxB, liV))
                        {
                            lsxB = lsxB.lsxEval();
                            continue;
                        }
                        else if (lsxA.fEqual(lsxB))
                            return true;
                        else if (((Lsm)lsxA).fVariable())
                        {
                            Sbst(lsxA, lsxB, liV);
                            return true;
                        }
                        else if (((Lsm)lsxB).fVariable())
                        {
                            Sbst(lsxB, lsxA, liV);
                            return true;
                        }
                        else
                            return false;
                    }
                    else if (((Lsm)lsxA).fVariable())
                    {
                        if (fOccur(lsxA, lsxB, liV))
                            return false;
                        Sbst(lsxA, lsxB, liV);
                        return true;
                    }
                    else
                        return false;
                }
                else if (lsxB.fAtom())
                {
                    if (fIsBound(lsxB, liV))
                    {
                        lsxB = lsxB.lsxEval();
                        continue;
                    }
                    else if (((Lsm)lsxB).fVariable())
                    {
                        if (fOccur(lsxB, lsxA, liV))
                            return false;
                        Sbst(lsxB, lsxA, liV);
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    Lpr lprA = (Lpr)lsxA;
                    Lpr lprB = (Lpr)lsxB;
                    if (!fEquate(lprA.lsxCar, lprB.lsxCar, liV))
                        return false;
                    lsxA = lprA.lsxCdr;
                    lsxB = lprB.lsxCdr;
                }
            }
        }

        public class Ems
        {
            public int nJ;
            public Lpr lprA;
            public Lsx lsxBPlace;
            public Li li;
            public Ems emsPrev;
        }

        public bool fEmbed(Lsx lsxA, Lsx lsxB, Li liT)  // pg 277
        {
            if (lsxA == Lsm.lsmNil)
                return true;
            if (lsxB == Lsm.lsmNil)
                return false;
            Ems emsTop = new Ems();
            emsTop.nJ = 1;
            emsTop.lsxBPlace = lsxB;
            emsTop.lprA = (Lpr)lsxA;
            emsTop.li = liT;
            while (emsTop != null)
            {
                if (emsTop.lsxBPlace == Lsm.lsmNil)
                    emsTop = emsTop.emsPrev;
                else
                {
                    Lpr lprBPlace = (Lpr)emsTop.lsxBPlace;
                    Lsx lsxBCur = lprBPlace.lsxCar;
                    Li liNext = new Li(emsTop.nJ, emsTop.li);
                    emsTop.nJ++;
                    emsTop.lsxBPlace = lprBPlace.lsxCdr;

                    if (fEquate(emsTop.lprA.lsxCar, lsxBCur, liNext))
                    {
                        if (emsTop.lprA.lsxCdr == Lsm.lsmNil)
                            return true;
                        Ems emsNew = new Ems();
                        emsNew.nJ = 1;
                        emsNew.lsxBPlace = lsxB;
                        emsNew.lprA = (Lpr) emsTop.lprA.lsxCdr;
                        emsNew.li = liNext;
                        emsNew.emsPrev = emsTop;
                        emsTop = emsNew;
                    }
                }
            }
            return false;
        }

        public bool fSubsumes(Lsx lsxA, Lsx lsxB)  // pg 273
        {
            Lsx lsxBVariables = lsxVariables(lsxB);
            UnmakeVariables(lsxBVariables);
            bool fResult = fEmbed(lsxAntiquate(lsxA), lsxAntiquate(lsxB), new Li(cTime++));
            MakeVariables(lsxBVariables);
            return fResult;
        }
    }
}
