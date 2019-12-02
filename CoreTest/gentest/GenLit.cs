using System;
using System.Collections.Generic;
using WBS;

namespace reslab.test
{
    public class GL
    {
        public Tqdv tqdvAnd = new Tqdv("and", Lsm.lsmAnd);
        public Tqdv tqdvEntails = new Tqdv("entails", Lsm.lsmEntails);
        public Tqdv tqdvExists = new Tqdv("exists", Lsm.lsmExists);
        public Tqdv tqdvFalse = new Tqdv("false", Lsm.lsmFalse);
        public Tqdv tqdvForall = new Tqdv("forall", Lsm.lsmForall);
        public Tqdv tqdvIff = new Tqdv("iff", Lsm.lsmIff);
        public Tqdv tqdvImplies = new Tqdv ("implies", Lsm.lsmImplies);
        public Tqdv tqdvList = new Tqdv("list", Lsm.lsmList);
        public Tqdv tqdvNil = new Tqdv("nil", Lsm.lsmNil);
        public Tqdv tqdvNot = new Tqdv("not", Lsm.lsmNot);
        public Tqdv tqdvOr = new Tqdv("or", Lsm.lsmOr);
        public Tqdv tqdvTrue = new Tqdv("true", Lsm.lsmTrue);

        Lsm lsmAdd(LParse res, string stName)
        {
            Lsm lsmVal = new Lsm(stName);
            res.AddSym(lsmVal, null);
            return lsmVal;
        }
        
        public Lsm lsmA;
        public Lsm lsmB;
        public Lsm lsmC;
        public Lsm lsmD;
        public Lsm lsmE;
        public Lsm lsmF1;
        public Lsm lsmG1;
        public Lsm lsmH1;
        public Lsm lsmF2;
        public Lsm lsmG2;
        public Lsm lsmH2;
        public Lsm lsmF3;
        public Lsm lsmG3;
        public Lsm lsmH3;
        public Lsm lsmI;
        public Lsm lsmJ;
        public Lsm lsmK;
        public Lsm lsmL;
        public Lsm lsmM;
        public Lsm lsmN;
        public Lsm lsmP1;
        public Lsm lsmQ1;
        public Lsm lsmR1;
        public Lsm lsmP2;
        public Lsm lsmQ2;
        public Lsm lsmR2;
        public Lsm lsmP3;
        public Lsm lsmQ3;
        public Lsm lsmR3;
        public Lsm lsmX;
        public Lsm lsmY;
        public Lsm lsmZ;

        public Tqdv tqdvA;
        public Tqdv tqdvB;
        public Tqdv tqdvC;
        public Tqdv tqdvD;
        public Tqdv tqdvE;
        public Tqdv tqdvF1;
        public Tqdv tqdvG1;
        public Tqdv tqdvH1;
        public Tqdv tqdvF2;
        public Tqdv tqdvG2;
        public Tqdv tqdvH2;
        public Tqdv tqdvF3;
        public Tqdv tqdvG3;
        public Tqdv tqdvH3;
        public Tqdv tqdvI;
        public Tqdv tqdvJ;
        public Tqdv tqdvK;
        public Tqdv tqdvL;
        public Tqdv tqdvM;
        public Tqdv tqdvN;
        public Tqdv tqdvP1;
        public Tqdv tqdvQ1;
        public Tqdv tqdvR1;
        public Tqdv tqdvP2;
        public Tqdv tqdvQ2;
        public Tqdv tqdvR2;
        public Tqdv tqdvP3;
        public Tqdv tqdvQ3;
        public Tqdv tqdvR3;
        public Tqdv tqdvX;
        public Tqdv tqdvY;
        public Tqdv tqdvZ;

        public Tqdv[] rgtqdvVars;
        public Tqdv[] rgtqdvBinOps;
        public Tqdv[] rgtqdvNaryOps;
        public Tqdv[] rgtqdvUnaryOps;
        public Tqdv[] rgtqdvQuantifiers;
        
        private Tqda tqdaVars;
        public Tqdq tqdaVarRefs;
        private Tqda tqdaBinOps;
        private Tqda tqdaNaryOps;
        private Tqda tqdaUnaryOps;
        private Tqda tqdaQuantifiers;

        public Tqdha tqdhaConstants;
        public Tqdr tqdrConstants;
        public Tqdha tqdhaFnNames1;
        private Tqdr tqdrFnNames1;
        public Tqdha tqdhaFnNames2;
        private Tqdr tqdrFnNames2;
        public Tqdha tqdhaFnNames3;
        private Tqdr tqdrFnNames3;
        public Tqdha tqdhaPredNames1;
        private Tqdr tqdrPredNames1;
        public Tqdha tqdhaPredNames2;
        private Tqdr tqdrPredNames2;
        public Tqdha tqdhaPredNames3;
        private Tqdr tqdrPredNames3;

        public Tqda tqdaValue;
        public Tqda tqdaBool;

        private Tqda tqdaExprList;

        public Tqdl tqdlFnCall1;
        private Tqdl tqdlFnCall2;
        private Tqdl tqdlFnCall3;
        public Tqdl tqdlPredCall1;
        private Tqdl tqdlPredCall2;
        private Tqdl tqdlPredCall3;
        public Tqdl tqdlBinExpr;
        public Tqdl tqdlQuantExpr;
        public Tqdl tqdlUnaryExpr;
        public Tqdl tqdlNaryExpr1;
        public Tqdl tqdlNaryExpr2;
        private Tqdl tqdlNaryExpr3;
        private Tqdl tqdlExprList0;
        public Tqdl tqdlExprList1;
        private Tqdl tqdlExprList2;
        private Tqdl tqdlExprList3;
        public Tqdl tqdlSequent;

        Gid gid;

        public bool fIsQuantifier (Tqs tqsPlace)
        {
            if (tqsPlace.tqdDef == tqdlQuantExpr)
                return true;
            return false;
        }

        void AppendSelect<T>(List<T> rgtqd, int nName, T tqdElement)
        {
            if (gid.fFlag(nName))
                rgtqd.Add(tqdElement);
        }

        public void BuildTqs(Gid gid)
        {
            this.gid = gid ?? new Gid(2, false);
#if false
            List<Tqdha> rgtqdha = new List<Tqdha>();
            AppendSelect<Tqdha>(rgtqdha, Gid.nFConstant, tqdhaConstants);
            AppendSelect<Tqdha>(rgtqdha, Gid.nFFnName1, tqdhaFnNames1);
            AppendSelect<Tqdha>(rgtqdha, Gid.nFFnName2, tqdhaFnNames2);
            AppendSelect<Tqdha>(rgtqdha, Gid.nFPredName1, tqdhaFnNames1);
            AppendSelect<Tqdha>(rgtqdha, Gid.nFPredName2, tqdhaFnNames2);
#endif

            List<Tqd> rgtqdExprList = new List<Tqd>();
            AppendSelect<Tqd>(rgtqdExprList, Gid.nFExprList1, tqdlExprList1);
            AppendSelect<Tqd>(rgtqdExprList, Gid.nFExprList2, tqdlExprList2);
            AppendSelect<Tqd>(rgtqdExprList, Gid.nFExprList3, tqdlExprList3);
            tqdaExprList = new Tqda("exprlist", rgtqdExprList.ToArray());

            tqdlSequent = new Tqdlr("sequent",
                                    tqdvEntails,
                                    new Tqd[] { tqdaExprList, tqdaExprList },
                                    // rgtqdha.ToArray());
                                    new Tqdha[] {
                                        tqdhaConstants, tqdhaFnNames1, tqdhaFnNames2, tqdhaFnNames3,
                                        tqdhaPredNames1, tqdhaPredNames2, tqdhaPredNames3
                                        });

            List<Tqd> rgtqdValue = new List<Tqd>();
            AppendSelect<Tqd>(rgtqdValue, Gid.nFConstant, tqdrConstants);
            rgtqdValue.Add(tqdaVarRefs);
            AppendSelect<Tqd>(rgtqdValue, Gid.nFFnName1, tqdlFnCall1);
            AppendSelect<Tqd>(rgtqdValue, Gid.nFFnName2, tqdlFnCall2);
            tqdaValue.SetRg(rgtqdValue.ToArray());
            // tqdrConstants, tqdaVarRefs, tqdlFnCall1, tqdlFnCall2 // , tqdlFnCall3
            // );

            List<Tqd> rgtqdBool = new List<Tqd>();
            AppendSelect<Tqd>(rgtqdBool, Gid.nFPredCall1, tqdlPredCall1);
            AppendSelect<Tqd>(rgtqdBool, Gid.nFPredCall2, tqdlPredCall3);
            AppendSelect<Tqd>(rgtqdBool, Gid.nFPredCall3, tqdlPredCall3);
            AppendSelect<Tqd>(rgtqdBool, Gid.nFQuantifier, tqdlQuantExpr);
            AppendSelect<Tqd>(rgtqdBool, Gid.nFBinExpr, tqdlBinExpr);
            AppendSelect<Tqd>(rgtqdBool, Gid.nFUnaryExpr, tqdlUnaryExpr);
            AppendSelect<Tqd>(rgtqdBool, Gid.nFNaryExpr1, tqdlNaryExpr1);
            AppendSelect<Tqd>(rgtqdBool, Gid.nFNaryExpr2, tqdlNaryExpr2);
            AppendSelect<Tqd>(rgtqdBool, Gid.nFNaryExpr2, tqdlNaryExpr2);
            tqdaBool.SetRg(rgtqdBool.ToArray());
#if false
            // tqdvFalse, tqdvTrue,
            tqdlPredCall1, tqdlPredCall2, // tqdlPredCall3,
                            tqdlQuantExpr,
                            tqdlBinExpr,
                            tqdlUnaryExpr,
                            tqdlNaryExpr1, tqdlNaryExpr2 // ,tqdlNaryExpr3
                            );
#endif

        }

        public GL(LParse res, Gid gid, int nDepth = 4)
        {
            lsmA = lsmAdd(res, "a");
            lsmB = lsmAdd(res, "b");
            lsmC = lsmAdd(res, "c");
            lsmD = lsmAdd(res, "d");
            lsmE = lsmAdd(res, "e");
            lsmF1 = lsmAdd(res, "f1");
            lsmG1 = lsmAdd(res, "g1");
            lsmH1 = lsmAdd(res, "h1");
            lsmF2 = lsmAdd(res, "f2");
            lsmG2 = lsmAdd(res, "g2");
            lsmH2 = lsmAdd(res, "h2");
            lsmF3 = lsmAdd(res, "f3");
            lsmG3 = lsmAdd(res, "g3");
            lsmH3 = lsmAdd(res, "h3");
            lsmI = lsmAdd(res, "i");
            lsmJ = lsmAdd(res, "j");
            lsmK = lsmAdd(res, "k");
            lsmL = lsmAdd(res, "l");
            lsmM = lsmAdd(res, "m");
            lsmN = lsmAdd(res, "n");
            lsmP1 = lsmAdd(res, "p1");
            lsmQ1 = lsmAdd(res, "q1");
            lsmR1 = lsmAdd(res, "r1");
            lsmP2 = lsmAdd(res, "p2");
            lsmQ2 = lsmAdd(res, "q2");
            lsmR2 = lsmAdd(res, "r2");
            lsmP3 = lsmAdd(res, "p3");
            lsmQ3 = lsmAdd(res, "q3");
            lsmR3 = lsmAdd(res, "r3");
            lsmX = lsmAdd(res, "x");
            lsmY = lsmAdd(res, "y");
            lsmZ = lsmAdd(res, "z");

            tqdvA = new Tqdv("a", lsmA);
            tqdvB = new Tqdv("b", lsmB);
            tqdvC = new Tqdv("c", lsmC);
            tqdvD = new Tqdv("d", lsmD);
            tqdvE = new Tqdv("e", lsmE);
            tqdvF1 = new Tqdv("f1", lsmF1);
            tqdvG1 = new Tqdv("g1", lsmG1);
            tqdvH1 = new Tqdv("h1", lsmH1);
            tqdvF2 = new Tqdv("f2", lsmF2);
            tqdvG2 = new Tqdv("g2", lsmG2);
            tqdvH2 = new Tqdv("h2", lsmH2);
            tqdvF3 = new Tqdv("f3", lsmF3);
            tqdvG3 = new Tqdv("g3", lsmG3);
            tqdvH3 = new Tqdv("h3", lsmH3);
            tqdvI = new Tqdv("i", lsmI);
            tqdvJ = new Tqdv("j", lsmJ);
            tqdvK = new Tqdv("k", lsmK);
            tqdvL = new Tqdv("l", lsmL);
            tqdvM = new Tqdv("m", lsmM);
            tqdvN = new Tqdv("n", lsmN);
            tqdvP1 = new Tqdv("p1", lsmP1);
            tqdvQ1 = new Tqdv("q1", lsmQ1);
            tqdvR1 = new Tqdv("r1", lsmR1);
            tqdvP2 = new Tqdv("p2", lsmP2);
            tqdvQ2 = new Tqdv("q2", lsmQ2);
            tqdvR2 = new Tqdv("r2", lsmR2);
            tqdvP3 = new Tqdv("p3", lsmP3);
            tqdvQ3 = new Tqdv("q3", lsmQ3);
            tqdvR3 = new Tqdv("r3", lsmR3);
            tqdvX = new Tqdv("x", lsmX);
            tqdvY = new Tqdv("y", lsmY);
            tqdvZ = new Tqdv("z", lsmZ);


            rgtqdvVars = new Tqdv[] { tqdvX  , tqdvY /* , tqdvZ */ }; // , tqdvI, tqdvJ, tqdvK, tqdvL, tqdvM, tqdvN };

            tqdhaConstants = new Tqdha("constantsH", lsmA, lsmB, lsmC);
            tqdrConstants = new Tqdr("constantsR", tqdhaConstants);

            tqdhaFnNames1 = new Tqdha("fnNames1H", lsmF1, lsmG1, lsmH1);
            tqdrFnNames1 = new Tqdr("fnNames1R", tqdhaFnNames1);
            tqdhaFnNames2 = new Tqdha("fnNames2H", lsmF2, lsmG2, lsmH2);
            tqdrFnNames2 = new Tqdr("fnNames2R", tqdhaFnNames2);
            tqdhaFnNames3 = new Tqdha("fnNames3H", lsmF3, lsmG3, lsmH3);
            tqdrFnNames3 = new Tqdr("fnNames3R", tqdhaFnNames3);

            tqdhaPredNames1 = new Tqdha("PredNames1H", lsmP1, lsmQ1, lsmR1);
            tqdrPredNames1 = new Tqdr("PredNames1R", tqdhaPredNames1);
            tqdhaPredNames2 = new Tqdha("PredNames2H", lsmP2, lsmQ2, lsmR2);
            tqdrPredNames2 = new Tqdr("PredNames2R", tqdhaPredNames2);
            tqdhaPredNames3 = new Tqdha("PredNames3H", lsmP3, lsmQ3, lsmR3);
            tqdrPredNames3 = new Tqdr("PredNames3R", tqdhaPredNames3);

            rgtqdvBinOps = new Tqdv[] { tqdvIff, tqdvImplies };
            rgtqdvNaryOps = new Tqdv[] { tqdvAnd, tqdvOr };
            rgtqdvUnaryOps = new Tqdv[] { tqdvNot };
            rgtqdvQuantifiers = new Tqdv[] { tqdvExists, tqdvForall };

            tqdaVars = new Tqda("vars", rgtqdvVars);
            tqdaVarRefs = new Tqdq("varref", fIsQuantifier, 0);   // op is not a child

            tqdaBinOps = new Tqda("binops", rgtqdvBinOps);
            tqdaNaryOps = new Tqda("naryops", rgtqdvNaryOps);
            tqdaUnaryOps = new Tqda("unaryops", rgtqdvUnaryOps);
            tqdaQuantifiers = new Tqda("quantifiers", rgtqdvQuantifiers);

            tqdaValue = new Tqda("value");
            tqdaBool = new Tqda("bool");

            tqdlFnCall1 = new Tqdl("fnCall1", tqdrFnNames1, tqdaValue);
            tqdlFnCall2 = new Tqdl("fnCall2", tqdrFnNames2, tqdaValue, tqdaValue);
            tqdlFnCall3 = new Tqdl("fnCall3", tqdrFnNames3, tqdaValue, tqdaValue, tqdaValue);

            tqdlPredCall1 = new Tqdl("predCall1", tqdrPredNames1, tqdaValue);
            tqdlPredCall2 = new Tqdl("predCall2", tqdrPredNames2, tqdaValue, tqdaValue);
            tqdlPredCall3 = new Tqdl("predCall3", tqdrPredNames3, tqdaValue, tqdaValue, tqdaValue);

            tqdlBinExpr = new Tqdl("binexpr", tqdaBinOps, tqdaBool, tqdaBool);
            tqdlUnaryExpr = new Tqdl("unaryexpr", tqdaUnaryOps, tqdaBool);
            tqdlNaryExpr1 = new Tqdl("naryexpr1", tqdaNaryOps, tqdaBool);
            tqdlNaryExpr2 = new Tqdl("naryexpr2", tqdaNaryOps, tqdaBool, tqdaBool);
            tqdlNaryExpr3 = new Tqdl("naryexpr3", tqdaNaryOps, tqdaBool, tqdaBool, tqdaBool);
            tqdlQuantExpr = new Tqdl("quantexpr", tqdaQuantifiers, tqdaVars, tqdaBool);

            tqdlExprList0 = new Tqdl("exprlist0", tqdvList);
            tqdlExprList1 = new Tqdl("exprlist1", tqdvList, tqdaBool);
            tqdlExprList2 = new Tqdl("exprlist2", tqdvList, tqdaBool, tqdaBool);
            tqdlExprList3 = new Tqdl("exprlist3", tqdvList, tqdaBool, tqdaBool, tqdaBool);

            BuildTqs(gid);
            SetDepth(nDepth);
        }

        public void SetDepth (int nDepth)
        {
            Tpi tpiDepth = new Tpi(nDepth);
            tqdaValue.SetTpiLimitChildren(tpiDepth);
            tqdaBool.SetTpiLimitChildren(tpiDepth);
        }
    }


}
