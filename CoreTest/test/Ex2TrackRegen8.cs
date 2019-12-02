using System;
using System.Diagnostics;
using reslab.TestUtil;

namespace reslab
{
    class TfcBuiltEx2Regen8: TfcBuilt, Ibo
    {
        public TfcBuiltEx2Regen8(Tfm tfm, PouEx2 pou) : base(tfm, pou) {}
        public override Pou pouBuildOutline(Iss iss)
        {
            Hbv<Epu> bodHbv155= new Hbv<Epu>();
            Hbv<Atp> bodHbv156= new Hbv<Atp>();
            Hbv<Eqs> bodHbv157= new Hbv<Eqs>();
            Hbv<Etp> bodHbv159= new Hbv<Etp>();
            Hbv<Eqs> bodHbv161= new Hbv<Eqs>();
            Hbv<Asc> bodHbv212= new Hbv<Asc>();
            Hbv<Eul> bodHbv2028= new Hbv<Eul>();
            Hbv<Vbv> bodHbv2263= new Hbv<Vbv>();
            Hbv<Eur> bodHbv2269= new Hbv<Eur>();
            Hbv<Eqs> bodHbv2030= new Hbv<Eqs>();
            Hbv<Etp> bodHbv2036= new Hbv<Etp>();
            Hbv<Eqs> bodHbv2038= new Hbv<Eqs>();
            Hbv<Eul> bodHbv2073= new Hbv<Eul>();
            Hbv<Eur> bodHbv2147= new Hbv<Eur>();
            Hbv<Eqs> bodHbv492= new Hbv<Eqs>();
            Hbv<Vbv> bodHbv2155= new Hbv<Vbv>();
            Hbv<Vbv> bodHbv2184= new Hbv<Vbv>();
            Hbv<Eqs> bodHbv1279= new Hbv<Eqs>();
            Hbv<Vbv> bodHbv1280= new Hbv<Vbv>();
            Hbv<Vbv> bodHbv2324= new Hbv<Vbv>();
            Hbv<Vbv> bodHbv2331= new Hbv<Vbv>();
            Hbv<Etp> bodHbv2338= new Hbv<Etp>();
            Hbv<Atp> bodHbv2339= new Hbv<Atp>();
            Hbv<Eqs> bodHbv2340= new Hbv<Eqs>();
            Hbv<Asc> bodHbv2348= new Hbv<Asc>();
            Hbv<Etp> bodHbv2342= new Hbv<Etp>();
            Hbv<Eqs> bodHbv2344= new Hbv<Eqs>();
            Hbv<Epu> bodHbv2359= new Hbv<Epu>();
            Hbv<Atp> bodHbv2360= new Hbv<Atp>();
            Hbv<Eqs> bodHbv2361= new Hbv<Eqs>();
            Hbv<Etp> bodHbv2363= new Hbv<Etp>();
            Hbv<Eqs> bodHbv2365= new Hbv<Eqs>();
            Hbv<Etp> bodHbv2367= new Hbv<Etp>();
            Hbv<Eqs> bodHbv1349= new Hbv<Eqs>();
            Hbv<Vbv> bodHbv1350= new Hbv<Vbv>();
            Hbv<Vbv> bodHbv2393= new Hbv<Vbv>();
            Hbv<Etp> bodHbv2395= new Hbv<Etp>();
            Hbv<Eqs> bodHbv205= new Hbv<Eqs>();
            Hbv<Epu> bodHbv2413= new Hbv<Epu>();
            Hbv<Atp> bodHbv2414= new Hbv<Atp>();
            Hbv<Eqs> bodHbv2415= new Hbv<Eqs>();
            Hbv<Etp> bodHbv2417= new Hbv<Etp>();
            Hbv<Eqs> bodHbv454= new Hbv<Eqs>();
            Hbv<Vbv> bodHbv455= new Hbv<Vbv>();
            Hbv<Vbv> bodHbv2863= new Hbv<Vbv>();
            Hbv<Etp> bodHbv2865= new Hbv<Etp>();
            Hbv<Eqs> bodHbv302= new Hbv<Eqs>();
            Hbv<Vbv> bodHbv303= new Hbv<Vbv>();
            Hbv<Vbv> bodHbv2867= new Hbv<Vbv>();
            Hbv<Vbv> bodHbv2423= new Hbv<Vbv>();
            Hbv<Asc> bodHbv2457= new Hbv<Asc>();
            Hbv<Epu> bodHbv2875= new Hbv<Epu>();
            Hbv<Atp> bodHbv2876= new Hbv<Atp>();
            Hbv<Eqs> bodHbv2877= new Hbv<Eqs>();
            Hbv<Etp> bodHbv2879= new Hbv<Etp>();
            Hbv<Eqs> bodHbv281= new Hbv<Eqs>();
            Hbv<Eul> bodHbv3481= new Hbv<Eul>();
            Hbv<Vbv> bodHbv3761= new Hbv<Vbv>();
            Hbv<Eur> bodHbv3767= new Hbv<Eur>();
            Hbv<Vbv> bodHbv48956= new Hbv<Vbv>();
            Hbv<Vbv> bodHbv49057= new Hbv<Vbv>();
            Hbv<Etp> bodHbv49064= new Hbv<Etp>();
            Hbv<Eqs> bodHbv49066= new Hbv<Eqs>();
            Hbv<Etp> bodHbv49068= new Hbv<Etp>();
            Hbv<Eul> bodHbv49095= new Hbv<Eul>();
            Hbv<Eqs> bodHbv751= new Hbv<Eqs>();
            Hbv<Vbv> bodHbv767= new Hbv<Vbv>();
            Hbv<Vbv> bodHbv49112= new Hbv<Vbv>();
            Hbv<Eur> bodHbv49116= new Hbv<Eur>();
            Hbv<Vbv> bodHbv49118= new Hbv<Vbv>();
            Hbv<Vbv> bodHbv59774= new Hbv<Vbv>();
            Pou pou = pouOutline;

            Tst tifStep0 = new Tst(pouOutline);
            Tst tifStep1 = new Tst(pouOutline);
            Tst tifStep2 = new Tst(pouOutline);
            Tst tifStep3 = new Tst(pouOutline);
            Tst tifStep4 = new Tst(pouOutline);
            Tst tifStep5 = new Tst(pouOutline);
            Tst tifStep6 = new Tst(pouOutline);
            Tst tifStep7 = new Tst(pouOutline);
            Tst tifStep8 = new Tst(pouOutline);
            Tst tifStep9 = new Tst(pouOutline);
            Tst tifStep10 = new Tst(pouOutline);
            Tst tifStep11 = new Tst(pouOutline);
            Tst tifStep12 = new Tst(pouOutline);
            Tst tifStep13 = new Tst(pouOutline);
            Tst tifStep14 = new Tst(pouOutline);
            Tst tifStep15 = new Tst(pouOutline);
            Tst tifStep16 = new Tst(pouOutline);
            Tst tifStep17 = new Tst(pouOutline);
            Tst tifStep18 = new Tst(pouOutline);
            Tst tifStep19 = new Tst(pouOutline);
            Tst tifStep20 = new Tst(pouOutline);
            Tst tifStep21 = new Tst(pouOutline);
            Tst tifStep22 = new Tst(pouOutline);
            Tst tifStep23 = new Tst(pouOutline);
            Tst tifStep24 = new Tst(pouOutline);
            Tst tifStep25 = new Tst(pouOutline);
            Tst tifStep26 = new Tst(pouOutline);
            Tst tifStep27 = new Tst(pouOutline);
            Tst tifStep28 = new Tst(pouOutline);
            Tst tifStep29 = new Tst(pouOutline);
            Tst tifStep30 = new Tst(pouOutline);
            Tst tifStep31 = new Tst(pouOutline);
            Tst tifStep32 = new Tst(pouOutline);
            Tst tifStep33 = new Tst(pouOutline);
            Tst tifStep34 = new Tst(pouOutline);
            Tst tifStep35 = new Tst(pouOutline);
            Tst tifStep36 = new Tst(pouOutline);
            Tst tifStep37 = new Tst(pouOutline);
            Tst tifStep38 = new Tst(pouOutline);
            Tst tifStep39 = new Tst(pouOutline);
            Tst tifStep40 = new Tst(pouOutline);
            Tst tifStep41 = new Tst(pouOutline);
            Tst tifStep42 = new Tst(pouOutline);
            Tst tifStep43 = new Tst(pouOutline);
            Tst tifStep44 = new Tst(pouOutline);
            Tst tifStep45 = new Tst(pouOutline);
            Tst tifStep46 = new Tst(pouOutline);
            Tst tifStep47 = new Tst(pouOutline);
            Tst tifStep48 = new Tst(pouOutline);
            Tst tifStep49 = new Tst(pouOutline);
            Tst tifStep50 = new Tst(pouOutline);
            Tst tifStep51 = new Tst(pouOutline);
            Tst tifStep52 = new Tst(pouOutline);
            Tst tifStep53 = new Tst(pouOutline);
            Tst tifStep54 = new Tst(pouOutline);
            Tst tifStep55 = new Tst(pouOutline);
            Tst tifStep56 = new Tst(pouOutline);
            Tst tifStep57 = new Tst(pouOutline);
            Tst tifStep58 = new Tst(pouOutline);
            Tst tifStep59 = new Tst(pouOutline);
            Tst tifStep60 = new Tst(pouOutline);
            Tst tifStep61 = new Tst(pouOutline);
            Tst tifStep62 = new Tst(pouOutline);
            Tst tifStep63 = new Tst(pouOutline);
            Tst tifStep64 = new Tst(pouOutline);
            Tst tifStep65 = new Tst(pouOutline);
            Tst tifStep66 = new Tst(pouOutline);
            Tst tifStep67 = new Tst(pouOutline);
            Tst tifStep68 = new Tst(pouOutline);
            Tst tifStep69 = new Tst(pouOutline);
            Tst tifStep70 = new Tst(pouOutline);
            Tst tifStep71 = new Tst(pouOutline);
            Tst tifStep72 = new Tst(pouOutline);
            Tst tifStep73 = new Tst(pouOutline);
            Tst tifStep74 = new Tst(pouOutline);
            Tst tifStep75 = new Tst(pouOutline);
            Tst tifStep76 = new Tst(pouOutline);
            Tst tifStep77 = new Tst(pouOutline);
            Tst tifStep78 = new Tst(pouOutline);
            Tst tifStep79 = new Tst(pouOutline);
            Tst tifStep80 = new Tst(pouOutline);
            Tst tifStep81 = new Tst(pouOutline);
            Tst tifStep82 = new Tst(pouOutline);
            Tst tifStep83 = new Tst(pouOutline);
            Tst tifStep84 = new Tst(pouOutline);
            Tst tifStep85 = new Tst(pouOutline);

            Tos tibStmt3 = new Tos(tifStep0);
            tibStmt3.SetLeft (pou.ivbGet<Asc>("ax3"));
            tibStmt3.SetRight (pou.ivbGet<Asc>("ax6"));
            tibStmt3.SetNextTst(tifStep1);
            Toh<Pti> tibStmt4 = new Toh<Pti>(tifStep0);
            tibStmt4.SetName ("pti2LtR");
            tibStmt4.SetTinName ("Pti");
            tibStmt4.SetValue (pou.vrfGet<Pti>("pti2LtR"));	// 3->6 in (nil  (=  (F  @0 E) @0))
            Toh<Pti> tibStmt5 = new Toh<Pti>(tifStep0);
            tibStmt5.SetName ("pti4LtR");
            tibStmt5.SetTinName ("Pti");
            tibStmt5.SetValue (pou.vrfGet<Pti>("pti4LtR"));	// 3->6 in (nil  (=  (F  @0 @0) E))
            Toh<Pti> tibStmt6 = new Toh<Pti>(tifStep0);
            tibStmt6.SetName ("pti5RtL");
            tibStmt6.SetTinName ("Pti");
            tibStmt6.SetValue (pou.vrfGet<Pti>("pti5RtL"));	// 4->3 in (nil  (=  C (F  A B)))
            Tod tibStmt9 = new Tod(tifStep1);
            tibStmt9.kKind = KCapture.kData;
            tibStmt9.hbvPlace = bodHbv155;
            Top tibStmt10 = new Top(tifStep1);
            tibStmt10.SetTarget (bodHbv155);	// Epuㅕ155 2ㅇ2ㅇ(((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  C (F  B A))))ㅑ
            tibStmt10.SetInput (null);	// null
            tibStmt10.SetTcd(Tcd.tcdEqsForEpu);
            tibStmt10.SetNextTst(tifStep2);
            tibStmt10.SetLabel("make Epu from two asc");
            Tod tibStmt13 = new Tod(tifStep2);
            tibStmt13.kKind = KCapture.kData;
            tibStmt13.hbvPlace = bodHbv156;
            Toa tibStmt14 = new Toa(tifStep2);
            tibStmt14.SetTff (TAtpToEqs.tff);
            tibStmt14.SetResult (bodHbv157);	// eqsㅕ157 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  C (F  B A))))ㅑ
            tibStmt14.SetArgs (bodHbv156);	// (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  C (F  B A))))
            Tod tibStmt15 = new Tod(tifStep2);
            tibStmt15.kKind = KCapture.kResult;
            tibStmt15.hbvPlace = bodHbv157;
            Tow tibStmt16 = new Tow(tifStep2);
            tibStmt16.SetToWatch (bodHbv157);	// eqsㅕ157 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  C (F  B A))))ㅑ
            tibStmt16.SetNextTst(tifStep3);
            Top tibStmt23 = new Top(tifStep3);
            tibStmt23.SetTarget (bodHbv157);	// eqsㅕ157 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  C (F  B A))))ㅑ
            tibStmt23.SetInput (pou.vrfGet<Nin>("nin2"));	// <Nin#2>
            tibStmt23.SetTcd(Tcd.tcdRegisterEtpByOffset);
            tibStmt23.SetNextTst(tifStep4);
            tibStmt23.SetLabel("find first child eqs for eqsToSolve");
            Tod tibStmt27 = new Tod(tifStep4);
            tibStmt27.kKind = KCapture.kData;
            tibStmt27.hbvPlace = bodHbv159;
            Toa tibStmt28 = new Toa(tifStep4);
            tibStmt28.SetTff (TfEtpToEqs.tff);
            tibStmt28.SetResult (bodHbv161);	// eqsㅕ161 (((F  @0 (F  @1 @2))) (C))ㅑ
            tibStmt28.SetArgs (bodHbv159);	// Etp#159 2,13 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  C (F  B A))))
            Tod tibStmt29 = new Tod(tifStep4);
            tibStmt29.kKind = KCapture.kResult;
            tibStmt29.hbvPlace = bodHbv161;
            Top tibStmt30 = new Top(tifStep4);
            tibStmt30.SetTarget (bodHbv161);	// eqsㅕ161 (((F  @0 (F  @1 @2))) (C))ㅑ
            tibStmt30.SetInput (pou.vrfGet<Pti>("pti2LtR"));	// 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt30.SetTcd(Tcd.tcdApplyPtiToEqs);
            tibStmt30.SetNextTst(tifStep5);
            tibStmt30.SetLabel("find eul for (F @0 E) => @0");
            Tod tibStmt107 = new Tod(tifStep5);
            tibStmt107.kKind = KCapture.kData;
            tibStmt107.hbvPlace = bodHbv2028;
            Tog tibStmt108 = new Tog(tifStep5);
            tibStmt108.SetTarget (bodHbv2028);	// Eul#2028 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt108.SetTcd(Tcd.tcdLaunchEur);
            tibStmt108.SetNextTst(tifStep6);
            Tod tibStmt117 = new Tod(tifStep6);
            tibStmt117.kKind = KCapture.kRight;
            tibStmt117.hbvPlace = bodHbv2263;
            Tod tibStmt118 = new Tod(tifStep6);
            tibStmt118.kKind = KCapture.kData;
            tibStmt118.hbvPlace = bodHbv2269;
            Toa tibStmt119 = new Toa(tifStep6);
            tibStmt119.SetTff (TEqsSub.tff);
            tibStmt119.SetResult (bodHbv2030);	// eqsㅕ2030 (((F  @0 (F  @1 @2))) ((F  @3 E)))ㅑ
            tibStmt119.SetArgs (bodHbv2028);	// Eul#2028 3->6 in (nil  (=  (F  @0 E) @0))
            Tod tibStmt120 = new Tod(tifStep6);
            tibStmt120.kKind = KCapture.kResult;
            tibStmt120.hbvPlace = bodHbv2030;
            Tow tibStmt121 = new Tow(tifStep6);
            tibStmt121.SetToWatch (bodHbv2030);	// eqsㅕ2030 (((F  @0 (F  @1 @2))) ((F  @3 E)))ㅑ
            tibStmt121.SetNextTst(tifStep7);
            Top tibStmt215 = new Top(tifStep7);
            tibStmt215.SetTarget (bodHbv2030);	// eqsㅕ2030 (((F  @0 (F  @1 @2))) ((F  @3 E)))ㅑ
            tibStmt215.SetInput (pou.vrfGet<Nin>("nin2"));	// <Nin#2>
            tibStmt215.SetTcd(Tcd.tcdRegisterEtpByOffset);
            tibStmt215.SetNextTst(tifStep8);
            tibStmt215.SetLabel("first etp looking for soln using 4");
            Top tibStmt216 = new Top(tifStep7);
            tibStmt216.SetTarget (bodHbv2030);	// eqsㅕ2030 (((F  @0 (F  @1 @2))) ((F  @3 E)))ㅑ
            tibStmt216.SetInput (pou.vrfGet<Nin>("nin3"));	// <Nin#3>
            tibStmt216.SetTcd(Tcd.tcdRegisterEtpByOffset);
            tibStmt216.SetNextTst(tifStep9);
            tibStmt216.SetLabel("first etp looking for soln using 4, 2nd child");
            Tod tibStmt223 = new Tod(tifStep9);
            tibStmt223.kKind = KCapture.kData;
            tibStmt223.hbvPlace = bodHbv2036;
            Toa tibStmt224 = new Toa(tifStep9);
            tibStmt224.SetTff (TfEtpToEqs.tff);
            tibStmt224.SetResult (bodHbv2038);	// eqsㅕ2038 (((F  @0 @1)) (E))ㅑ
            tibStmt224.SetArgs (bodHbv2036);	// Etp#2036 3,8 (((F  @0 (F  @1 @2))) ((F  @3 E)))
            Tod tibStmt225 = new Tod(tifStep9);
            tibStmt225.kKind = KCapture.kResult;
            tibStmt225.hbvPlace = bodHbv2038;
            Tow tibStmt226 = new Tow(tifStep9);
            tibStmt226.SetToWatch (bodHbv2038);	// eqsㅕ2038 (((F  @0 @1)) (E))ㅑ
            tibStmt226.SetNextTst(tifStep10);
            Top tibStmt326 = new Top(tifStep10);
            tibStmt326.SetTarget (bodHbv2038);	// eqsㅕ2038 (((F  @0 @1)) (E))ㅑ
            tibStmt326.SetInput (pou.vrfGet<Pti>("pti4LtR"));	// 3->6 in (nil  (=  (F  @0 @0) E))
            tibStmt326.SetTcd(Tcd.tcdApplyPtiToEqs);
            tibStmt326.SetNextTst(tifStep11);
            tibStmt326.SetLabel("eul for pti 4 to (((F  @0 @1)) (E))");
            Tod tibStmt337 = new Tod(tifStep11);
            tibStmt337.kKind = KCapture.kData;
            tibStmt337.hbvPlace = bodHbv2073;
            Tog tibStmt338 = new Tog(tifStep11);
            tibStmt338.SetTarget (bodHbv2073);	// Eul#2073 3->6 in (nil  (=  (F  @0 @0) E))
            tibStmt338.SetTcd(Tcd.tcdLaunchEur);
            tibStmt338.SetNextTst(tifStep12);
            Tod tibStmt341 = new Tod(tifStep12);
            tibStmt341.kKind = KCapture.kData;
            tibStmt341.hbvPlace = bodHbv2147;
            Toi tibStmt342 = new Toi(tifStep12);

            tibStmt342.SetInput (bodHbv2147);	// Eur#2147 3->6 in (nil  (=  (F  @0 @0) E))
            tibStmt342.SetTcd(Tcd.tcdTransferLeftEqs);
            tibStmt342.SetNextTst(tifStep13);
            Tod tibStmt343 = new Tod(tifStep13);
            tibStmt343.kKind = KCapture.kLeft;
            tibStmt343.hbvPlace = bodHbv492;
            Toi tibStmt345 = new Toi(tifStep13);
            tibStmt345.SetInput (bodHbv492);	// eqsㅕ492 ((E) (E))ㅑ
            tibStmt345.SetTcd(Tcd.tcdEqsToEnt);
            tibStmt345.SetNextTst(tifStep14);
            Top tibStmt346 = new Top(tifStep13);
            tibStmt346.SetTarget (bodHbv2147);	// Eur#2147 3->6 in (nil  (=  (F  @0 @0) E))
            tibStmt346.SetInput (bodHbv492);	// eqsㅕ492 ((E) (E))ㅑ
            tibStmt346.SetTcd(Tcd.tcdEqsToEnt);
            tibStmt346.SetNextTst(tifStep15);
            tibStmt346.SetLabel("solns coming from eqsEE to pti4");
            Top tibStmt353 = new Top(tifStep15);
            tibStmt353.SetTarget (bodHbv2038);	// eqsㅕ2038 (((F  @0 @1)) (E))ㅑ
            tibStmt353.SetInput (bodHbv2147);	// Eur#2147 3->6 in (nil  (=  (F  @0 @0) E))
            tibStmt353.SetTcd(Tcd.tcdEurToNotifyEqs);
            tibStmt353.SetNextTst(tifStep16);
            tibStmt353.SetLabel("soln from eqsEE using pti4 to {{eqs#8103 (((F  @0 @1)) (E))}");
            Tod tibStmt356 = new Tod(tifStep16);
            tibStmt356.kKind = KCapture.kData;
            tibStmt356.hbvPlace = bodHbv2155;
            Top tibStmt357 = new Top(tifStep16);
            tibStmt357.SetTarget (bodHbv2036);	// Etp#2036 3,8 (((F  @0 (F  @1 @2))) ((F  @3 E)))
            tibStmt357.SetInput (bodHbv2155);	// <#2155 <#2156 1/6 >0:4$2156, 1:5$2156>
            tibStmt357.SetTcd(Tcd.tcdSolnToEqs);
            tibStmt357.SetNextTst(tifStep17);
            tibStmt357.SetLabel("process soln from eqsEE using pti4 to {{eqs#8103 (((F  @0 @1)) (E))}");
            Top tibStmt362 = new Top(tifStep17);
            tibStmt362.SetTarget (bodHbv2036);	// Etp#2036 3,8 (((F  @0 (F  @1 @2))) ((F  @3 E)))
            tibStmt362.SetInput (bodHbv2155);	// <#2155 <#2156 1/6 >0:4$2156, 1:5$2156>
            tibStmt362.SetTcd(Tcd.tcdVbvForEtp);
            tibStmt362.SetNextTst(tifStep18);
            tibStmt362.SetLabel("get merged pti4/EE vbv from etp of ax3 and ax1");
            Tod tibStmt365 = new Tod(tifStep18);
            tibStmt365.kKind = KCapture.kData;
            tibStmt365.hbvPlace = bodHbv2184;
            Toa tibStmt366 = new Toa(tifStep18);
            tibStmt366.SetTff (TfFindDup.tff);
            tibStmt366.SetResult (bodHbv2184);	// <#2184 <#2185 3/6 >0:7$2184, 1:4$2185, 2:5$2185>
            tibStmt366.SetArgs (bodHbv2030);	// eqsㅕ2030 (((F  @0 (F  @1 @2))) ((F  @3 E)))ㅑ
            tibStmt366.SetArgs (bodHbv2184);	// <#2184 <#2185 3/6 >0:7$2184, 1:4$2185, 2:5$2185>
            Tow tibStmt368 = new Tow(tifStep18);
            tibStmt368.SetToWatch (bodHbv2184);	// <#2184 <#2185 3/6 >0:7$2184, 1:4$2185, 2:5$2185>
            tibStmt368.SetNextTst(tifStep19);

            Top tibStmt506 = new Top(tifStep19);
            tibStmt506.SetTarget (bodHbv2028);	// Eul#2028 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt506.SetInput (bodHbv2184);	// <#2184 <#2185 3/6 >0:7$2184, 1:4$2185, 2:5$2185>
            tibStmt506.SetTcd(Tcd.tcdMapEuhSoln);
            tibStmt506.SetNextTst(tifStep20);
            tibStmt506.SetLabel("map vbv by eulPti2FirstChild");
            Top tibStmt518 = new Top(tifStep20);
            tibStmt518.SetTarget (bodHbv2028);	// Eul#2028 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt518.SetInput (bodHbv2263);	// <#2263 <#2264 1/6 ><#2265 3/6 >0:4$2264, 1:4$2265, 2:5$2265>
            tibStmt518.SetTcd(Tcd.tcdLaunchEur);
            tibStmt518.SetNextTst(tifStep21);
            tibStmt518.SetLabel("eur from eulPti2FirstChild");
            Top tibStmt527 = new Top(tifStep21);
            tibStmt527.SetTarget (bodHbv2269);	// Eur#2269 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt527.SetInput (bodHbv2263);	// <#2263 <#2264 1/6 ><#2265 3/6 >0:4$2264, 1:4$2265, 2:5$2265>
            tibStmt527.SetTcd(Tcd.tcdStartSubproblem);
            tibStmt527.SetNextTst(tifStep22);
            tibStmt527.SetLabel("eqsSub for eurPti2FirstChild");
            Tod tibStmt530 = new Tod(tifStep22);
            tibStmt530.kKind = KCapture.kData;
            tibStmt530.hbvPlace = bodHbv1279;
            Top tibStmt531 = new Top(tifStep22);
            tibStmt531.SetTarget (bodHbv2269);	// Eur#2269 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt531.SetInput (bodHbv1279);	// eqsㅕ1279 ((C) (@0))ㅑ
            tibStmt531.SetTcd(Tcd.tcdEqsToEnt);
            tibStmt531.SetNextTst(tifStep23);
            tibStmt531.SetLabel("soln for eurPti2FirstChild2");
            Tod tibStmt537 = new Tod(tifStep23);
            tibStmt537.kKind = KCapture.kData;
            tibStmt537.hbvPlace = bodHbv1280;
            Top tibStmt538 = new Top(tifStep23);
            tibStmt538.SetTarget (bodHbv2269);	// Eur#2269 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt538.SetInput (bodHbv1280);	// <#1280 0:1$A>
            tibStmt538.SetTcd(Tcd.tcdMapEuhSoln);
            tibStmt538.SetNextTst(tifStep24);
            tibStmt538.SetLabel("map soln for eurPti2FirstChild2");
            Tod tibStmt541 = new Tod(tifStep24);
            tibStmt541.kKind = KCapture.kData;
            tibStmt541.hbvPlace = bodHbv2324;
            Top tibStmt542 = new Top(tifStep24);
            tibStmt542.SetTarget (bodHbv161);	// eqsㅕ161 (((F  @0 (F  @1 @2))) (C))ㅑ
            tibStmt542.SetInput (bodHbv2269);	// Eur#2269 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt542.SetTcd(Tcd.tcdEurToNotifyEqs);
            tibStmt542.SetNextTst(tifStep25);
            tibStmt542.SetLabel("eurPti2FirstChild2 soln to lhs of 3vs6");

            Top tibStmt546 = new Top(tifStep25);
            tibStmt546.SetTarget (bodHbv159);	// Etp#159 2,13 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  C (F  B A))))
            tibStmt546.SetInput (bodHbv2324);	// <#2324 <#2325 1/6 0:6$2324><#2326 3/6 >0:4$2325, 1:4$2326, 2:5$2326>
            tibStmt546.SetTcd(Tcd.tcdSolnToEqs);
            tibStmt546.SetNextTst(tifStep26);
            tibStmt546.SetLabel("soln to lhs of 3vs6 of eqsFirstChildEqsToSolve");
            Top tibStmt550 = new Top(tifStep26);
            tibStmt550.SetTarget (bodHbv159);	// Etp#159 2,13 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  C (F  B A))))
            tibStmt550.SetInput (bodHbv2324);	// <#2324 <#2325 1/6 0:6$2324><#2326 3/6 >0:4$2325, 1:4$2326, 2:5$2326>
            tibStmt550.SetTcd(Tcd.tcdVbvForEtp);
            tibStmt550.SetNextTst(tifStep27);
            tibStmt550.SetLabel("combine soln to rhs of 3vs6 of eqsFirstChildEqsToSolve");
            Tod tibStmt553 = new Tod(tifStep27);
            tibStmt553.kKind = KCapture.kData;
            tibStmt553.hbvPlace = bodHbv2331;
            Top tibStmt554 = new Top(tifStep27);
            tibStmt554.SetTarget (bodHbv159);	// Etp#159 2,13 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  C (F  B A))))
            tibStmt554.SetInput (bodHbv2331);	// <#2331 <#2332 2/6 0:13$B><#2333 4/6 >0:4$2332, 1:4$2333, 2:5$2333>
            tibStmt554.SetTcd(Tcd.tcdRegisterEtpByVbv);
            tibStmt554.SetNextTst(tifStep28);
            tibStmt554.SetLabel("report soln to lhs of 3vs6 of eqsFirstChildEqsToSolve");

            Tod tibStmt557 = new Tod(tifStep28);
            tibStmt557.kKind = KCapture.kData;
            tibStmt557.hbvPlace = bodHbv2338;
            Toa tibStmt558 = new Toa(tifStep28);
            tibStmt558.SetTff (TfAtpChild.tff);
            tibStmt558.SetResult (bodHbv2339);	// (((F  (F  C @0) @0)) ((F  B A)))
            tibStmt558.SetArgs (bodHbv2338);	// Etp#2338 7,14 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  C (F  B A))))
            Tod tibStmt559 = new Tod(tifStep28);
            tibStmt559.kKind = KCapture.kResult;
            tibStmt559.hbvPlace = bodHbv2339;
            Tog tibStmt560 = new Tog(tifStep28);
            tibStmt560.SetTarget (bodHbv2339);	// (((F  (F  C @0) @0)) ((F  B A)))
            tibStmt560.SetTcd(Tcd.tcdNewEqs);
            tibStmt560.SetNextTst(tifStep29);
            Tod tibStmt562 = new Tod(tifStep29);
            tibStmt562.kKind = KCapture.kRight;
            tibStmt562.hbvPlace = bodHbv2340;
            Tog tibStmt563 = new Tog(tifStep29);
            tibStmt563.SetTarget (bodHbv2340);	// eqsㅕ2340 (((F  (F  C @0) @0)) ((F  B A)))ㅑ
            tibStmt563.SetTcd(Tcd.tcdEqsToNgc);
            tibStmt563.SetNextTst(tifStep30);
            Tow tibStmt568 = new Tow(tifStep30);
            tibStmt568.SetToWatch (bodHbv2340);	// eqsㅕ2340 (((F  (F  C @0) @0)) ((F  B A)))ㅑ
            tibStmt568.SetNextTst(tifStep31);
            Top tibStmt762 = new Top(tifStep31);
            tibStmt762.SetTarget (bodHbv2340);	// eqsㅕ2340 (((F  (F  C @0) @0)) ((F  B A)))ㅑ
            tibStmt762.SetInput (pou.vrfGet<Nin>("nin2"));	// <Nin#2>
            tibStmt762.SetTcd(Tcd.tcdRegisterEtpByOffset);
            tibStmt762.SetNextTst(tifStep32);
            tibStmt762.SetLabel("find etp posn where ngc needed");
            Tod tibStmt769 = new Tod(tifStep32);
            tibStmt769.kKind = KCapture.kData;
            tibStmt769.hbvPlace = bodHbv2342;
            Toa tibStmt770 = new Toa(tifStep32);
            tibStmt770.SetTff (TfEtpToEqs.tff);
            tibStmt770.SetResult (bodHbv2344);	// eqsㅕ2344 (((F  C @0)) (B))ㅑ
            tibStmt770.SetArgs (bodHbv2342);	// Etp#2342 2,7 (((F  (F  C @0) @0)) ((F  B A)))
            Tod tibStmt771 = new Tod(tifStep32);
            tibStmt771.kKind = KCapture.kResult;
            tibStmt771.hbvPlace = bodHbv2344;
            Tow tibStmt772 = new Tow(tifStep32);
            tibStmt772.SetToWatch (bodHbv2344);	// eqsㅕ2344 (((F  C @0)) (B))ㅑ
            tibStmt772.SetNextTst(tifStep33);
            Tog tibStmt971 = new Tog(tifStep33);
            tibStmt971.SetTarget (bodHbv2344);	// eqsㅕ2344 (((F  C @0)) (B))ㅑ
            tibStmt971.SetTcd(Tcd.tcdEqsToNgc);
            tibStmt971.SetNextTst(tifStep34);
            Tod tibStmt974 = new Tod(tifStep34);
            tibStmt974.kKind = KCapture.kData;
            tibStmt974.hbvPlace = bodHbv2348;
            Toh<Asc> tibStmt975 = new Toh<Asc>(tifStep34);
            tibStmt975.SetName ("NgcFC0B");
            tibStmt975.SetTinName ("Asc");
            tibStmt975.SetValue (bodHbv2348);	// (((=  (F  C @0) B)))
            Tos tibStmt976 = new Tos(tifStep34);
            tibStmt976.SetLeft (pou.ivbGet<Asc>("ax3"));
            tibStmt976.SetRight (pou.ivbGet<Asc>("NgcFC0B"));
            tibStmt976.SetNextTst(tifStep35);
            Tod tibStmt989 = new Tod(tifStep35);
            tibStmt989.kKind = KCapture.kData;
            tibStmt989.hbvPlace = bodHbv2359;
            Top tibStmt990 = new Top(tifStep35);
            tibStmt990.SetTarget (bodHbv2359);	// Epuㅕ2359 2ㅇ2ㅇ(((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  (F  C @3) B)))ㅑ
            tibStmt990.SetInput (null);	// null
            tibStmt990.SetTcd(Tcd.tcdEqsForEpu);
            tibStmt990.SetNextTst(tifStep36);
            tibStmt990.SetLabel("make Epu from two asc");
            Tod tibStmt994 = new Tod(tifStep36);
            tibStmt994.kKind = KCapture.kData;
            tibStmt994.hbvPlace = bodHbv2360;
            Toa tibStmt995 = new Toa(tifStep36);
            tibStmt995.SetTff (TAtpToEqs.tff);
            tibStmt995.SetResult (bodHbv2361);	// eqsㅕ2361 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  (F  C @3) B)))ㅑ
            tibStmt995.SetArgs (bodHbv2360);	// (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  (F  C @3) B)))
            Tod tibStmt996 = new Tod(tifStep36);
            tibStmt996.kKind = KCapture.kResult;
            tibStmt996.hbvPlace = bodHbv2361;
            Tow tibStmt997 = new Tow(tifStep36);
            tibStmt997.SetToWatch (bodHbv2361);	// eqsㅕ2361 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  (F  C @3) B)))ㅑ
            tibStmt997.SetNextTst(tifStep37);
            Top tibStmt1216 = new Top(tifStep37);
            tibStmt1216.SetTarget (bodHbv2361);	// eqsㅕ2361 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  (F  C @3) B)))ㅑ
            tibStmt1216.SetInput (pou.vrfGet<Nin>("nin2"));	// <Nin#2>
            tibStmt1216.SetTcd(Tcd.tcdRegisterEtpByOffset);
            tibStmt1216.SetNextTst(tifStep38);
            tibStmt1216.SetLabel("find first child eqs for eqsToSolve");
            Tod tibStmt1220 = new Tod(tifStep38);
            tibStmt1220.kKind = KCapture.kData;
            tibStmt1220.hbvPlace = bodHbv2363;
            Toa tibStmt1221 = new Toa(tifStep38);
            tibStmt1221.SetTff (TfEtpToEqs.tff);
            tibStmt1221.SetResult (bodHbv2365);	// eqsㅕ2365 (((F  @0 (F  @1 @2))) ((F  C @3)))ㅑ
            tibStmt1221.SetArgs (bodHbv2363);	// Etp#2363 2,13 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  (F  C @3) B)))
            Tod tibStmt1222 = new Tod(tifStep38);
            tibStmt1222.kKind = KCapture.kResult;
            tibStmt1222.hbvPlace = bodHbv2365;
            Tow tibStmt1223 = new Tow(tifStep38);
            tibStmt1223.SetToWatch (bodHbv2365);	// eqsㅕ2365 (((F  @0 (F  @1 @2))) ((F  C @3)))ㅑ
            tibStmt1223.SetNextTst(tifStep39);
            Top tibStmt1444 = new Top(tifStep39);
            tibStmt1444.SetTarget (bodHbv2365);	// eqsㅕ2365 (((F  @0 (F  @1 @2))) ((F  C @3)))ㅑ
            tibStmt1444.SetInput (pou.vrfGet<Nin>("nin2"));	// <Nin#2>
            tibStmt1444.SetTcd(Tcd.tcdRegisterEtpByOffset);
            tibStmt1444.SetNextTst(tifStep40);
            tibStmt1444.SetLabel("find first child lsh eqs of eqsToSolve");
            Tod tibStmt1447 = new Tod(tifStep40);
            tibStmt1447.kKind = KCapture.kData;
            tibStmt1447.hbvPlace = bodHbv2367;
            Toa tibStmt1448 = new Toa(tifStep40);
            tibStmt1448.SetTff (TfEtpToEqs.tff);
            tibStmt1448.SetResult (bodHbv1349);	// eqsㅕ1349 ((@0) (C))ㅑ
            tibStmt1448.SetArgs (bodHbv2367);	// Etp#2367 2,7 (((F  @0 (F  @1 @2))) ((F  C @3)))
            Tod tibStmt1449 = new Tod(tifStep40);
            tibStmt1449.kKind = KCapture.kResult;
            tibStmt1449.hbvPlace = bodHbv1349;
            Tow tibStmt1450 = new Tow(tifStep40);
            tibStmt1450.SetToWatch (bodHbv1349);	// eqsㅕ1349 ((@0) (C))ㅑ
            tibStmt1450.SetNextTst(tifStep41);
            Tow tibStmt1678 = new Tow(tifStep40);
            tibStmt1678.SetToWatch (bodHbv2367);	// Etp#2367 2,7 (((F  @0 (F  @1 @2))) ((F  C @3)))
            tibStmt1678.SetNextTst(tifStep42);
            Top tibStmt1902 = new Top(tifStep42);
            tibStmt1902.SetTarget (bodHbv2367);	// Etp#2367 2,7 (((F  @0 (F  @1 @2))) ((F  C @3)))
            tibStmt1902.SetInput (bodHbv1349);	// eqsㅕ1349 ((@0) (C))ㅑ
            tibStmt1902.SetTcd(Tcd.tcdEqsToEnt);
            tibStmt1902.SetNextTst(tifStep43);
            tibStmt1902.SetLabel("apply soln from eqs0C to second arg");
            Tod tibStmt1921 = new Tod(tifStep43);
            tibStmt1921.kKind = KCapture.kData;
            tibStmt1921.hbvPlace = bodHbv1350;
            Top tibStmt1922 = new Top(tifStep43);
            tibStmt1922.SetTarget (bodHbv2367);	// Etp#2367 2,7 (((F  @0 (F  @1 @2))) ((F  C @3)))
            tibStmt1922.SetInput (bodHbv1350);	// <#1350 0:2$B>
            tibStmt1922.SetTcd(Tcd.tcdVbvForEtp);
            tibStmt1922.SetNextTst(tifStep44);
            tibStmt1922.SetLabel("apply soln to etpSecondArg");
            Tod tibStmt1925 = new Tod(tifStep44);
            tibStmt1925.kKind = KCapture.kData;
            tibStmt1925.hbvPlace = bodHbv2393;
            Top tibStmt1926 = new Top(tifStep44);
            tibStmt1926.SetTarget (bodHbv2367);	// Etp#2367 2,7 (((F  @0 (F  @1 @2))) ((F  C @3)))
            tibStmt1926.SetInput (bodHbv2393);	// <#2393 0:7$B>
            tibStmt1926.SetTcd(Tcd.tcdRegisterEtpByVbv);
            tibStmt1926.SetNextTst(tifStep45);
            tibStmt1926.SetLabel("apply soln from eqs0C to second arg");
            Tod tibStmt1929 = new Tod(tifStep45);
            tibStmt1929.kKind = KCapture.kData;
            tibStmt1929.hbvPlace = bodHbv2395;
            Toa tibStmt1930 = new Toa(tifStep45);
            tibStmt1930.SetTff (TfEtpToEqs.tff);
            tibStmt1930.SetResult (bodHbv205);	// eqsㅕ205 (((F  @0 @1)) (@2))ㅑ
            tibStmt1930.SetArgs (bodHbv2395);	// Etp#2395 3,8 (((F  @0 (F  @1 @2))) ((F  C @3)))
            Tod tibStmt1931 = new Tod(tifStep45);
            tibStmt1931.kKind = KCapture.kResult;
            tibStmt1931.hbvPlace = bodHbv205;
            Tow tibStmt1932 = new Tow(tifStep45);
            tibStmt1932.SetToWatch (bodHbv205);	// eqsㅕ205 (((F  @0 @1)) (@2))ㅑ
            tibStmt1932.SetNextTst(tifStep46);
            Tog tibStmt1953 = new Tog(tifStep46);
            tibStmt1953.SetTarget (bodHbv205);	// eqsㅕ205 (((F  @0 @1)) (@2))ㅑ
            tibStmt1953.SetTcd(Tcd.tcdEqsToNgc);
            tibStmt1953.SetNextTst(tifStep47);
            Tod tibStmt1956 = new Tod(tifStep47);
            tibStmt1956.kKind = KCapture.kData;
            tibStmt1956.hbvPlace = bodHbv212;
            Toh<Asc> tibStmt1957 = new Toh<Asc>(tifStep47);
            tibStmt1957.SetName ("NgcF012");
            tibStmt1957.SetTinName ("Asc");
            tibStmt1957.SetValue (bodHbv212);	// (((=  (F  @0 @1) @2)))
            Tos tibStmt1958 = new Tos(tifStep47);
            tibStmt1958.SetLeft (pou.ivbGet<Asc>("axR"));
            tibStmt1958.SetRight (pou.ivbGet<Asc>("NgcF012"));
            tibStmt1958.SetNextTst(tifStep48);
            Tod tibStmt2195 = new Tod(tifStep48);
            tibStmt2195.kKind = KCapture.kData;
            tibStmt2195.hbvPlace = bodHbv2413;
            Top tibStmt2196 = new Top(tifStep48);
            tibStmt2196.SetTarget (bodHbv2413);	// Epuㅕ2413 5ㅇ2ㅇ(((=  @0 @1)) ((=  (F  @2 @3) @4)))ㅑ
            tibStmt2196.SetInput (null);	// null
            tibStmt2196.SetTcd(Tcd.tcdEqsForEpu);
            tibStmt2196.SetNextTst(tifStep49);
            tibStmt2196.SetLabel("make Epu from two asc");
            Tod tibStmt2198 = new Tod(tifStep49);
            tibStmt2198.kKind = KCapture.kData;
            tibStmt2198.hbvPlace = bodHbv2414;
            Toa tibStmt2199 = new Toa(tifStep49);
            tibStmt2199.SetTff (TAtpToEqs.tff);
            tibStmt2199.SetResult (bodHbv2415);	// eqsㅕ2415 (((=  @0 @1)) ((=  (F  @2 @3) @4)))ㅑ
            tibStmt2199.SetArgs (bodHbv2414);	// (((=  @0 @1)) ((=  (F  @2 @3) @4)))
            Tod tibStmt2200 = new Tod(tifStep49);
            tibStmt2200.kKind = KCapture.kResult;
            tibStmt2200.hbvPlace = bodHbv2415;
            Tow tibStmt2201 = new Tow(tifStep49);
            tibStmt2201.SetToWatch (bodHbv2415);	// eqsㅕ2415 (((=  @0 @1)) ((=  (F  @2 @3) @4)))ㅑ
            tibStmt2201.SetNextTst(tifStep50);
            Top tibStmt2466 = new Top(tifStep50);
            tibStmt2466.SetTarget (bodHbv2415);	// eqsㅕ2415 (((=  @0 @1)) ((=  (F  @2 @3) @4)))ㅑ
            tibStmt2466.SetInput (pou.vrfGet<Nin>("nin2"));	// <Nin#2>
            tibStmt2466.SetTcd(Tcd.tcdRegisterEtpByOffset);
            tibStmt2466.SetNextTst(tifStep51);
            tibStmt2466.SetLabel("find first child eqs for eqsToSolve");
            Tod tibStmt2469 = new Tod(tifStep51);
            tibStmt2469.kKind = KCapture.kData;
            tibStmt2469.hbvPlace = bodHbv2417;
            Toa tibStmt2470 = new Toa(tifStep51);
            tibStmt2470.SetTff (TfEtpToEqs.tff);
            tibStmt2470.SetResult (bodHbv454);	// eqsㅕ454 ((@0) ((F  @1 @2)))ㅑ
            tibStmt2470.SetArgs (bodHbv2417);	// Etp#2417 2,5 (((=  @0 @1)) ((=  (F  @2 @3) @4)))
            Tod tibStmt2471 = new Tod(tifStep51);
            tibStmt2471.kKind = KCapture.kResult;
            tibStmt2471.hbvPlace = bodHbv454;
            Top tibStmt2472 = new Top(tifStep51);
            tibStmt2472.SetTarget (bodHbv2417);	// Etp#2417 2,5 (((=  @0 @1)) ((=  (F  @2 @3) @4)))
            tibStmt2472.SetInput (bodHbv454);	// eqsㅕ454 ((@0) ((F  @1 @2)))ㅑ
            tibStmt2472.SetTcd(Tcd.tcdEqsToEnt);
            tibStmt2472.SetNextTst(tifStep52);
            tibStmt2472.SetLabel("match first arg");
            Tod tibStmt2490 = new Tod(tifStep52);
            tibStmt2490.kKind = KCapture.kData;
            tibStmt2490.hbvPlace = bodHbv455;
            Top tibStmt2491 = new Top(tifStep52);
            tibStmt2491.SetTarget (bodHbv2417);	// Etp#2417 2,5 (((=  @0 @1)) ((=  (F  @2 @3) @4)))
            tibStmt2491.SetInput (bodHbv455);	// <#455 0:2$B>
            tibStmt2491.SetTcd(Tcd.tcdVbvForEtp);
            tibStmt2491.SetNextTst(tifStep53);
            tibStmt2491.SetLabel("merge soln from skip 4 and 2");
            Tod tibStmt2494 = new Tod(tifStep53);
            tibStmt2494.kKind = KCapture.kData;
            tibStmt2494.hbvPlace = bodHbv2863;
            Top tibStmt2495 = new Top(tifStep53);
            tibStmt2495.SetTarget (bodHbv2417);	// Etp#2417 2,5 (((=  @0 @1)) ((=  (F  @2 @3) @4)))
            tibStmt2495.SetInput (bodHbv2863);	// <#2863 0:5$B>
            tibStmt2495.SetTcd(Tcd.tcdRegisterEtpByVbv);
            tibStmt2495.SetNextTst(tifStep54);
            tibStmt2495.SetLabel("second arg after 0~(F23)");
            Tod tibStmt2498 = new Tod(tifStep54);
            tibStmt2498.kKind = KCapture.kData;
            tibStmt2498.hbvPlace = bodHbv2865;
            Toa tibStmt2499 = new Toa(tifStep54);
            tibStmt2499.SetTff (TfEtpToEqs.tff);
            tibStmt2499.SetResult (bodHbv302);	// eqsㅕ302 ((@0) (@1))ㅑ
            tibStmt2499.SetArgs (bodHbv2865);	// Etp#2865 3,8 (((=  @0 @1)) ((=  (F  @2 @3) @4)))
            Tod tibStmt2500 = new Tod(tifStep54);
            tibStmt2500.kKind = KCapture.kResult;
            tibStmt2500.hbvPlace = bodHbv302;
            Toa tibStmt2501 = new Toa(tifStep54);
            tibStmt2501.SetTff (TfVbvMinimalSoln.tff);
            tibStmt2501.SetResult (bodHbv303);	// <#303 0:2$B>
            tibStmt2501.SetArgs (bodHbv302);	// eqsㅕ302 ((@0) (@1))ㅑ
            Tod tibStmt2502 = new Tod(tifStep54);
            tibStmt2502.kKind = KCapture.kResult;
            tibStmt2502.hbvPlace = bodHbv303;
            Top tibStmt2503 = new Top(tifStep54);
            tibStmt2503.SetTarget (bodHbv2865);	// Etp#2865 3,8 (((=  @0 @1)) ((=  (F  @2 @3) @4)))
            tibStmt2503.SetInput (bodHbv303);	// <#303 0:2$B>
            tibStmt2503.SetTcd(Tcd.tcdVbvForEtp);
            tibStmt2503.SetNextTst(tifStep55);
            tibStmt2503.SetLabel("immediate vbv after 0~(F23)");
            Tod tibStmt2506 = new Tod(tifStep55);
            tibStmt2506.kKind = KCapture.kData;
            tibStmt2506.hbvPlace = bodHbv2867;
            Toa tibStmt2507 = new Toa(tifStep55);
            tibStmt2507.SetTff (TfEpuToEqs.tff);
            tibStmt2507.SetResult (bodHbv2415);	// eqsㅕ2415 (((=  @0 @1)) ((=  (F  @2 @3) @4)))ㅑ
            tibStmt2507.SetArgs (bodHbv2413);	// Epuㅕ2413 5ㅇ2ㅇ(((=  @0 @1)) ((=  (F  @2 @3) @4)))ㅑ
            Toa tibStmt2509 = new Toa(tifStep55);
            tibStmt2509.SetTff (TfFindDup.tff);
            tibStmt2509.SetResult (bodHbv2423);	// <#2423 0:5$2423, 1:8$2423>
            tibStmt2509.SetArgs (bodHbv2415);	// eqsㅕ2415 (((=  @0 @1)) ((=  (F  @2 @3) @4)))ㅑ
            tibStmt2509.SetArgs (bodHbv2867);	// <#2867 0:5$2867, 1:8$2867>
            Tod tibStmt2510 = new Tod(tifStep55);
            tibStmt2510.kKind = KCapture.kResult;
            tibStmt2510.hbvPlace = bodHbv2423;
            Tow tibStmt2511 = new Tow(tifStep55);
            tibStmt2511.SetToWatch (bodHbv2423);	// <#2423 0:5$2423, 1:8$2423>
            tibStmt2511.SetNextTst(tifStep56);
            Top tibStmt2784 = new Top(tifStep56);
            tibStmt2784.SetTarget (bodHbv2413);	// Epuㅕ2413 5ㅇ2ㅇ(((=  @0 @1)) ((=  (F  @2 @3) @4)))ㅑ
            tibStmt2784.SetInput (bodHbv2423);	// <#2423 0:5$2423, 1:8$2423>
            tibStmt2784.SetTcd(Tcd.tcdSolnToEqs);
            tibStmt2784.SetNextTst(tifStep57);
            tibStmt2784.SetLabel("merged vbv to epuR2");
            Top tibStmt2794 = new Top(tifStep57);
            tibStmt2794.SetTarget (pou.vrfGet<Asc>("axR"));	// (((=  @0 @1)) (=  @1 @0))
            tibStmt2794.SetInput (bodHbv212);	// (((=  (F  @0 @1) @2)))
            tibStmt2794.SetTcd(Tcd.tcdNewAscResolve);
            tibStmt2794.SetNextTst(tifStep58);
            tibStmt2794.SetLabel("ascR from epuR2");
            Tod tibStmt2797 = new Tod(tifStep58);
            tibStmt2797.kKind = KCapture.kData;
            tibStmt2797.hbvPlace = bodHbv2457;
            Toh<Asc> tibStmt2798 = new Toh<Asc>(tifStep58);
            tibStmt2798.SetName ("0_F12");
            tibStmt2798.SetTinName ("Asc");
            tibStmt2798.SetValue (bodHbv2457);	// (((=  @0 (F  @1 @2))))
            Tos tibStmt2799 = new Tos(tifStep58);
            tibStmt2799.SetLeft (pou.ivbGet<Asc>("ax3"));
            tibStmt2799.SetRight (pou.ivbGet<Asc>("0_F12"));
            tibStmt2799.SetNextTst(tifStep59);
            Tod tibStmt2834 = new Tod(tifStep59);
            tibStmt2834.kKind = KCapture.kData;
            tibStmt2834.hbvPlace = bodHbv2875;
            Top tibStmt2835 = new Top(tifStep59);
            tibStmt2835.SetTarget (bodHbv2875);	// Epuㅕ2875 2ㅇ2ㅇ(((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  @3 (F  @4 @5))))ㅑ
            tibStmt2835.SetInput (null);	// null
            tibStmt2835.SetTcd(Tcd.tcdEqsForEpu);
            tibStmt2835.SetNextTst(tifStep60);
            tibStmt2835.SetLabel("make Epu from two asc");
            Tod tibStmt2838 = new Tod(tifStep60);
            tibStmt2838.kKind = KCapture.kData;
            tibStmt2838.hbvPlace = bodHbv2876;
            Toa tibStmt2839 = new Toa(tifStep60);
            tibStmt2839.SetTff (TAtpToEqs.tff);
            tibStmt2839.SetResult (bodHbv2877);	// eqsㅕ2877 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  @3 (F  @4 @5))))ㅑ
            tibStmt2839.SetArgs (bodHbv2876);	// (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  @3 (F  @4 @5))))
            Tod tibStmt2840 = new Tod(tifStep60);
            tibStmt2840.kKind = KCapture.kResult;
            tibStmt2840.hbvPlace = bodHbv2877;
            Tow tibStmt2841 = new Tow(tifStep60);
            tibStmt2841.SetToWatch (bodHbv2877);	// eqsㅕ2877 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  @3 (F  @4 @5))))ㅑ
            tibStmt2841.SetNextTst(tifStep61);
            Top tibStmt3166 = new Top(tifStep61);
            tibStmt3166.SetTarget (bodHbv2877);	// eqsㅕ2877 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  @3 (F  @4 @5))))ㅑ
            tibStmt3166.SetInput (pou.vrfGet<Nin>("nin2"));	// <Nin#2>
            tibStmt3166.SetTcd(Tcd.tcdRegisterEtpByOffset);
            tibStmt3166.SetNextTst(tifStep62);
            tibStmt3166.SetLabel("find first child eqs for eqsToSolve");
            Tod tibStmt3169 = new Tod(tifStep62);
            tibStmt3169.kKind = KCapture.kData;
            tibStmt3169.hbvPlace = bodHbv2879;
            Toa tibStmt3170 = new Toa(tifStep62);
            tibStmt3170.SetTff (TfEtpToEqs.tff);
            tibStmt3170.SetResult (bodHbv281);	// eqsㅕ281 (((F  @0 (F  @1 @2))) (@3))ㅑ
            tibStmt3170.SetArgs (bodHbv2879);	// Etp#2879 2,13 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  @3 (F  @4 @5))))
            Tod tibStmt3171 = new Tod(tifStep62);
            tibStmt3171.kKind = KCapture.kResult;
            tibStmt3171.hbvPlace = bodHbv281;
            Tow tibStmt3172 = new Tow(tifStep62);
            tibStmt3172.SetToWatch (bodHbv281);	// eqsㅕ281 (((F  @0 (F  @1 @2))) (@3))ㅑ
            tibStmt3172.SetNextTst(tifStep63);
            Top tibStmt3204 = new Top(tifStep63);
            tibStmt3204.SetTarget (bodHbv281);	// eqsㅕ281 (((F  @0 (F  @1 @2))) (@3))ㅑ
            tibStmt3204.SetInput (pou.vrfGet<Pti>("pti2LtR"));	// 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt3204.SetTcd(Tcd.tcdApplyPtiToEqs);
            tibStmt3204.SetNextTst(tifStep64);
            tibStmt3204.SetLabel("find eul for (F @0 E) => @0");
            Tod tibStmt3525 = new Tod(tifStep64);
            tibStmt3525.kKind = KCapture.kData;
            tibStmt3525.hbvPlace = bodHbv3481;
            Tog tibStmt3526 = new Tog(tifStep64);
            tibStmt3526.SetTarget (bodHbv3481);	// Eul#3481 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt3526.SetTcd(Tcd.tcdLaunchEur);
            tibStmt3526.SetNextTst(tifStep65);
            Tod tibStmt3535 = new Tod(tifStep65);
            tibStmt3535.kKind = KCapture.kRight;
            tibStmt3535.hbvPlace = bodHbv3761;
            Tod tibStmt3536 = new Tod(tifStep65);
            tibStmt3536.kKind = KCapture.kData;
            tibStmt3536.hbvPlace = bodHbv3767;
            Top tibStmt3537 = new Top(tifStep65);
            tibStmt3537.SetTarget (bodHbv3767);	// Eur#3767 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt3537.SetInput (bodHbv3761);	// <#3761 <#3762 1/6 ><#3763 3/6 >0:4$3762, 1:4$3763, 2:5$3763>
            tibStmt3537.SetTcd(Tcd.tcdStartSubproblem);
            tibStmt3537.SetNextTst(tifStep66);
            tibStmt3537.SetLabel("phase3 map vbv by eulPti2FirstChild");
            Tow tibStmt3541 = new Tow(tifStep66);
            tibStmt3541.SetToWatch (bodHbv3767);	// Eur#3767 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt3541.SetNextTst(tifStep67);
            Top tibStmt3904 = new Top(tifStep67);
            tibStmt3904.SetTarget (bodHbv3767);	// Eur#3767 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt3904.SetInput (bodHbv302);	// eqsㅕ302 ((@0) (@1))ㅑ
            tibStmt3904.SetTcd(Tcd.tcdEqsToEnt);
            tibStmt3904.SetNextTst(tifStep68);
            tibStmt3904.SetLabel("get eurPti2FirstChild3");
            Top tibStmt4571 = new Top(tifStep68);
            tibStmt4571.SetTarget (bodHbv3767);	// Eur#3767 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt4571.SetInput (bodHbv303);	// <#303 0:2$B>
            tibStmt4571.SetTcd(Tcd.tcdMapEuhSoln);
            tibStmt4571.SetNextTst(tifStep69);
            tibStmt4571.SetLabel("get merged vbv with P3 soln to 01");
            Tod tibStmt4574 = new Tod(tifStep69);
            tibStmt4574.kKind = KCapture.kData;
            tibStmt4574.hbvPlace = bodHbv48956;
            Top tibStmt4575 = new Top(tifStep69);
            tibStmt4575.SetTarget (bodHbv281);	// eqsㅕ281 (((F  @0 (F  @1 @2))) (@3))ㅑ
            tibStmt4575.SetInput (bodHbv3767);	// Eur#3767 3->6 in (nil  (=  (F  @0 E) @0))
            tibStmt4575.SetTcd(Tcd.tcdEurToNotifyEqs);
            tibStmt4575.SetNextTst(tifStep70);
            tibStmt4575.SetLabel("send notify to eqsFirstChildEqsToSolve3");
            Top tibStmt4579 = new Top(tifStep70);
            tibStmt4579.SetTarget (bodHbv2879);	// Etp#2879 2,13 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  @3 (F  @4 @5))))
            tibStmt4579.SetInput (bodHbv48956);	// <#48956 <#48957 1/6 ><#48958 3/6 >0:4$48957, 1:4$48958, 2:5$48958, 3:6$48957>
            tibStmt4579.SetTcd(Tcd.tcdVbvForEtp);
            tibStmt4579.SetNextTst(tifStep71);
            tibStmt4579.SetLabel("get merged vbv for etp using F0F12_3");
            Tod tibStmt4583 = new Tod(tifStep71);
            tibStmt4583.kKind = KCapture.kData;
            tibStmt4583.hbvPlace = bodHbv49057;
            Top tibStmt4584 = new Top(tifStep71);
            tibStmt4584.SetTarget (bodHbv2879);	// Etp#2879 2,13 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  @3 (F  @4 @5))))
            tibStmt4584.SetInput (bodHbv49057);	// <#49057 <#49058 2/6 ><#49059 4/6 >0:4$49058, 1:4$49059, 2:5$49059, 3:6$49058>
            tibStmt4584.SetTcd(Tcd.tcdRegisterEtpByVbv);
            tibStmt4584.SetNextTst(tifStep72);
            tibStmt4584.SetLabel("second etp for F0F12_3");
            Tod tibStmt4587 = new Tod(tifStep72);
            tibStmt4587.kKind = KCapture.kData;
            tibStmt4587.hbvPlace = bodHbv49064;
            Toa tibStmt4589 = new Toa(tifStep72);
            tibStmt4589.SetTff (TfEtpToEqs.tff);
            tibStmt4589.SetResult (bodHbv49066);	// eqsㅕ49066 (((F  (F  @0 @1) @1)) ((F  @2 @3)))ㅑ
            tibStmt4589.SetArgs (bodHbv49064);	// Etp#49064 7,14 (((=  (F  @0 (F  @1 @2)) (F  (F  @0 @1) @2))) ((=  @3 (F  @4 @5))))
            Tod tibStmt4590 = new Tod(tifStep72);
            tibStmt4590.kKind = KCapture.kResult;
            tibStmt4590.hbvPlace = bodHbv49066;
            Tow tibStmt4591 = new Tow(tifStep72);
            tibStmt4591.SetToWatch (bodHbv49066);	// eqsㅕ49066 (((F  (F  @0 @1) @1)) ((F  @2 @3)))ㅑ
            tibStmt4591.SetNextTst(tifStep73);
            Top tibStmt5639 = new Top(tifStep73);
            tibStmt5639.SetTarget (bodHbv49066);	// eqsㅕ49066 (((F  (F  @0 @1) @1)) ((F  @2 @3)))ㅑ
            tibStmt5639.SetInput (pou.vrfGet<Nin>("nin2"));	// <Nin#2>
            tibStmt5639.SetTcd(Tcd.tcdRegisterEtpByOffset);
            tibStmt5639.SetNextTst(tifStep74);
            tibStmt5639.SetLabel("find first child eqs for eqsFor3rdRhs");
            Tod tibStmt5642 = new Tod(tifStep74);
            tibStmt5642.kKind = KCapture.kData;
            tibStmt5642.hbvPlace = bodHbv49068;
            Toa tibStmt5643 = new Toa(tifStep74);
            tibStmt5643.SetTff (TfEtpToEqs.tff);
            tibStmt5643.SetResult (bodHbv205);	// eqsㅕ205 (((F  @0 @1)) (@2))ㅑ
            tibStmt5643.SetArgs (bodHbv49068);	// Etp#49068 2,7 (((F  (F  @0 @1) @1)) ((F  @2 @3)))
            Tow tibStmt5645 = new Tow(tifStep74);
            tibStmt5645.SetToWatch (bodHbv205);	// eqsㅕ205 (((F  @0 @1)) (@2))ㅑ
            tibStmt5645.SetNextTst(tifStep75);
            Tov tibStmt5668 = new Tov(tifStep75);
            tibStmt5668.SetKValue(KTis.kEqsAllowEprNonVbl);
            tibStmt5668.SetValue (bodHbv205);	// eqsㅕ205 (((F  @0 @1)) (@2))ㅑ
            Top tibStmt5669 = new Top(tifStep75);
            tibStmt5669.SetTarget (bodHbv205);	// eqsㅕ205 (((F  @0 @1)) (@2))ㅑ
            tibStmt5669.SetInput (pou.vrfGet<Pti>("pti5RtL"));	// 4->3 in (nil  (=  C (F  A B)))
            tibStmt5669.SetTcd(Tcd.tcdApplyPtiToEqs);
            tibStmt5669.SetNextTst(tifStep76);
            tibStmt5669.SetLabel("look for on pti of Ax5 applied to 3rd phase");
            Tod tibStmt6707 = new Tod(tifStep76);
            tibStmt6707.kKind = KCapture.kData;
            tibStmt6707.hbvPlace = bodHbv49095;
            Tog tibStmt6708 = new Tog(tifStep76);
            tibStmt6708.SetTarget (bodHbv49095);	// Eul#49095 4->3 in (nil  (=  C (F  A B)))
            tibStmt6708.SetTcd(Tcd.tcdStartSubproblem);
            tibStmt6708.SetNextTst(tifStep77);
            Tod tibStmt6710 = new Tod(tifStep77);
            tibStmt6710.kKind = KCapture.kData;
            tibStmt6710.hbvPlace = bodHbv751;
            Tow tibStmt6711 = new Tow(tifStep77);
            tibStmt6711.SetToWatch (bodHbv751);	// eqsㅕ751 (((F  @0 @1)) ((F  A B)))ㅑ
            tibStmt6711.SetNextTst(tifStep78);
            Top tibStmt7782 = new Top(tifStep77);
            tibStmt7782.SetTarget (bodHbv49095);	// Eul#49095 4->3 in (nil  (=  C (F  A B)))
            tibStmt7782.SetInput (bodHbv767);	// <#767 0:5$767, 1:6$767>
            tibStmt7782.SetTcd(Tcd.tcdMapEuhSoln);
            tibStmt7782.SetNextTst(tifStep80);
            tibStmt7782.SetLabel("soln of F01_FAB to eulEqs3rdAx5");
            Tog tibStmt6767 = new Tog(tifStep78);
            tibStmt6767.SetTarget (bodHbv751);	// eqsㅕ751 (((F  @0 @1)) ((F  A B)))ㅑ
            tibStmt6767.SetTcd(Tcd.tcdImmediateVbv);
            tibStmt6767.SetNextTst(tifStep79);
            Tod tibStmt6771 = new Tod(tifStep79);
            tibStmt6771.kKind = KCapture.kData;
            tibStmt6771.hbvPlace = bodHbv767;
            Tod tibStmt7785 = new Tod(tifStep80);
            tibStmt7785.kKind = KCapture.kData;
            tibStmt7785.hbvPlace = bodHbv49112;
            Top tibStmt7786 = new Top(tifStep80);
            tibStmt7786.SetTarget (bodHbv49095);	// Eul#49095 4->3 in (nil  (=  C (F  A B)))
            tibStmt7786.SetInput (bodHbv49112);	// <#49112 0:5$49113, 1:6$49113>
            tibStmt7786.SetTcd(Tcd.tcdLaunchEur);
            tibStmt7786.SetNextTst(tifStep81);
            tibStmt7786.SetLabel("eur after F01_FAB to eulEqs3rdAx5");
            Tod tibStmt7789 = new Tod(tifStep81);
            tibStmt7789.kKind = KCapture.kData;
            tibStmt7789.hbvPlace = bodHbv49116;
            Tog tibStmt7790 = new Tog(tifStep81);
            tibStmt7790.SetTarget (bodHbv49116);	// Eur#49116 4->3 in (nil  (=  C (F  A B)))
            tibStmt7790.SetTcd(Tcd.tcdMapEuhSoln);
            tibStmt7790.SetNextTst(tifStep82);
            Tod tibStmt7793 = new Tod(tifStep82);
            tibStmt7793.kKind = KCapture.kData;
            tibStmt7793.hbvPlace = bodHbv49118;
            Top tibStmt7794 = new Top(tifStep82);
            tibStmt7794.SetTarget (bodHbv49068);	// Etp#49068 2,7 (((F  (F  @0 @1) @1)) ((F  @2 @3)))
            tibStmt7794.SetInput (bodHbv49118);	// <#49118 <#49119 1/3 >0:5$49119, 1:6$49119, 2:3$49119>
            tibStmt7794.SetTcd(Tcd.tcdVbvForEtp);
            tibStmt7794.SetNextTst(tifStep83);
            tibStmt7794.SetLabel("merge soln from F01_FAB to etpFirstChild3rdRhs");
            Tod tibStmt7950 = new Tod(tifStep83);
            tibStmt7950.kKind = KCapture.kData;
            tibStmt7950.hbvPlace = bodHbv59774;
            Top tibStmt7951 = new Top(tifStep83);
            tibStmt7951.SetTarget (bodHbv49068);	// Etp#49068 2,7 (((F  (F  @0 @1) @1)) ((F  @2 @3)))
            tibStmt7951.SetInput (bodHbv59774);	// <#59774 <#59775 2/3 >0:5$59775, 1:6$59775, 2:3$59775>
            tibStmt7951.SetTcd(Tcd.tcdRegisterEtpByVbv);
            tibStmt7951.SetNextTst(tifStep84);
            tibStmt7951.SetLabel("next in etp after F01_FAB to etpFirstChild3rdRhs");
            Tos tibStmt7955 = new Tos(tifStep84);
            tibStmt7955.SetLeft (pou.ivbGet<Asc>("ax3"));
            tibStmt7955.SetRight (pou.ivbGet<Asc>("NgcFC0B"));
            tibStmt7955.SetNextTst(tifStep85);
            Toa tibStmt7959 = new Toa(tifStep85);
            tibStmt7959.SetTff (TffSave.tff);
            tibStmt7959.SetResult (null);	// null
            return pou;
        }
    }
}

