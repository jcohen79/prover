using System.Collections.Generic;
using reslab.test;
using NUnit.Framework;
using reslab.TestUtil;

namespace reslab
{

    public class TfcBuiltTree : TfcBuilt, Ibo
    {

        public TfcBuiltTree(Tfm tfm, PouEx2 pou) : base(tfm, pou)
        {
        }

        public override Pou pouBuildOutline(Iss iss)
        {
            TpProof tpProof = new TpProof(pou);
            TpResolve tpRes1 = new TpResolve("ax3", "ax6");
            tpRes1.fNeedWatch156 = true;
            tpProof.tpbInsertMember(tpRes1);
            tpProof.NotePti();
            {
                TpEtp tpEtp1 = tpRes1.tpEqs.InsertEtp(new TpEtp(2)); // equate terms two axioms
                {
                    TpEqs tpEqs1_0 = tpEtp1.hlpChild(0).tpeqsChild;   // first term pair: LHS of =
                    TpEul tpEul1_0_2 = new TpEul();
                    tpEul1_0_2.stPtiName = "pti2LtR";
                    tpEqs1_0.InsertEul(tpEul1_0_2);          // use  pti2LtR  for LHS of =
                    TpGetEqsPhase1Pti2 tpGetEqsPhase1Pti2 = new TpGetEqsPhase1Pti2();
                    tpEul1_0_2.InsertGetEqs(tpGetEqsPhase1Pti2);    // get Eqs for RHS of pti2LtR vs LHS of ax3
                    TpEtp tpEtp1_0_R = new TpEtp(2);
                    tpEtp1_0_R.hlpChild(0).fSkipEqs = true;
                    tpEtp1_0_R.hlpChild(1).fNeedWatch160 = true;
                    tpGetEqsPhase1Pti2.tpEqs.InsertEtp(tpEtp1_0_R);    // use Etp
                    {
                        TpEqs tpEqs1_0_2 = tpEtp1_0_R.hlpChild(1).tpeqsChild;   // E in pti2LtR vs (F @1 @2)
                        TpEul tpEul1_0_2_L = new TpEul();   //tibStmt326, 294      
                        tpEul1_0_2_L.stPtiName = "pti4LtR";
                        tpEqs1_0_2.InsertEul(tpEul1_0_2_L);    // use pti4LtR for E vs (F @1 @2)

                       // TpGetEqsPhase1Pti4 tpGetEqsPhase1Pti4 = new TpGetEqsPhase1Pti4();  // trivial
                        TpEtp tpEtp1_0_L_R = new TpEtp(2);   // for (F @0 @0) vs (F @1 @2)
                        tpEtp1_0_L_R.hlpChild(0).fSkipTerm = true;  // var to var
                        tpEtp1_0_L_R.hlpChild(1).fSkipTerm = true;  // var to var 
                        tpEul1_0_2_L.InsertGetEqs(tpEtp1_0_L_R);
                        TpGetEqsPhase1Pti4Up tpPti4Up = new TpGetEqsPhase1Pti4Up();
                        tpEtp1_0_L_R.InsertWrapUp(tpPti4Up);
                    }
                }
            }

            TpResolve tpRes2 = tpProof.tpbInsertMember(new TpResolve("ax3", "NgcFC0B"));

            TpResolve tpRes3 = tpProof.tpbInsertMember(new TpResolve("axR", "NgcF012"));

            TpResolve tpRes4 = tpProof.tpbInsertMember(new TpResolve("ax3", "0_F12"));

            TpResolve tpRes5 = tpProof.tpbInsertMember(new TpResolve("ax3", "NgcFC0B"));  // wrong

            return pou;
        }

        public static TfcBuiltTree tfcMake(string stClassName, string stExample)
        {
            PouEx2 puoOutline = new PouEx2(stClassName);

            Tfm tfm = new Tfm();
            TfcBuiltTree tes = new TfcBuiltTree(tfm, puoOutline);
            tfm.SetTestHook(new Adh(), tes);
            tfm.pmbResolution = new Prtfc(tes);
            tfm.InstrumentedProof(tes, stExample);
            return tes;

        }
    }

    
    [TestFixture]
    [Category("Proof Examples Slow")]
    class ProofSections
    {
        [Test]
        public void Ex2Regen6()
        {
            PouEx2 pou = new PouEx2(null);

            Tfm tfm = new Tfm();
            string stFilename = @"C:\Users\user\projects\prover\CoreTest\test\Ex2TrackRegen7.cs";
            tfm.tscOutput = new Tsc(stFilename, "TfcBuiltEx2Regen7");
            TfcBuilt tes = new TfcBuiltEx2Regen6(tfm, pou);
            tfm.SetTestHook(new Adh(), tes);
            tfm.pmbResolution = new Prtfc(tes);
            tfm.InstrumentedProof(tes, Ex2.stEx2_plusPhases);
        }

        [Test]
        public void Ex2Regen7()
        {
            PouEx2 pou = new PouEx2(null);

            Tfm tfm = new Tfm();
            string stFilename = @"C:\Users\user\projects\prover\CoreTest\test\Ex2TrackRegen8.cs";
            tfm.tscOutput = new Tsc(stFilename, "TfcBuiltEx2Regen8");
            TfcBuilt tes = new TfcBuiltEx2Regen7(tfm, pou);
            tfm.SetTestHook(new Adh(), tes);
            tfm.pmbResolution = new Prtfc(tes);
            tfm.InstrumentedProof(tes, Ex2.stEx2_plusPhases);
        }

        [Test]
        public void Ex2Regen8()
        {
            PouEx2 pou = new PouEx2(null);

            Tfm tfm = new Tfm();
            TfcBuilt tes = new TfcBuiltEx2Regen8(tfm, pou);
            tfm.SetTestHook(new Adh(), tes);
            tfm.pmbResolution = new Prtfc(tes);
            tfm.InstrumentedProof(tes, Ex2.stEx2_plusPhases);
        } 

        [Test]
        public void Ex2Tree()
        {
            TfcBuiltTree tfcMake = TfcBuiltTree.tfcMake("TfcBuiltEx2GenTree", Ex2.stEx2_plusPhases);
            // problem: difficult to keep track of reused section of proof
            // using pattern matching to do that work

            // compare output from proof tree with existing  outline
        }

        [Test]
        public void Ex2Match()
        {
            PouEx2 pou = new PouEx2(null);

            Tfm tfm = new Tfm();
            TfcBuilt tes = new TfcBuiltEx2Regen6(tfm, pou);
            Mpf.Build(pou);
            Mch.ppiMatchSeq(Mpf.mtgProof, tes.pouOutline.psbFirst);
        }
    }
   
}