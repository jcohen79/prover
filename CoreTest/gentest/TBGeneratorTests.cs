// #define DEBUG_REGISTER_ACTIONS

using System;
using System.Collections;
using System.Collections.Generic;
using Debug = System.Diagnostics.Debug;



using NUnit.Framework;

namespace WBS
{
#if false
    public class TBRun : TB
    {
        public void RunTest(Node node)
        {
            using (Tbs tbs = ReturnNode(node, xid: ID.WBS.xidShell.N, dtmmiWait: 10, ktb: Ktb.Edit))
            {
            }
        }
    }
    public static class TBTestCommon
    {
        public readonly static Tqdv tqdvApp = new Tqdv(typeof(SEApp));


        public readonly static Tqdv tqdvCxPt = new Tqdv(typeof(SECxPt));


        public readonly static Tqdv tqdvName = new Tqdv(typeof(SEName));


        public readonly static Tqdv tqdvNumberLit = new Tqdv(typeof(SENumberLit));


        public readonly static Tqdv tqdvOutline = new Tqdv(typeof(GEN.SEOutline));


        public readonly static Tqdv tqdvOutlineItem = new Tqdv(typeof(GEN.SEOutlineItem));


        public readonly static Tqdv tqdvParagraph = new Tqdv(typeof(GEN.SEParagraph));


        public readonly static Tqdv tqdvParagraphText = new Tqdv(typeof(GEN.SEParagraphText));


        public readonly static Tqdv tqdvStringAtom = new Tqdv(typeof(SEStringAtom));


        public readonly static Tqdv tqdvTitle = new Tqdv(typeof(SETitle));


        public readonly static Tqdv tqdvWb = new Tqdv(typeof(GEN.SEWhiteboardPage));


        public readonly static Tqdv tqdvWbd = new Tqdv(typeof(SEWbd));


        public readonly static Tqdv tqdvWbds = new Tqdv(typeof(SEWbds));


        public readonly static Tqdv tqdvmain = new Tqdv(ID.Kernel.main.N);


        public readonly static Tqdv tqdvnames = new Tqdv(ID.Kernel.names.N);


        public readonly static Tqdv tqdvptwbOrigin = new Tqdv(ID.WBS.Wbi.ptwbOrigin.N);


        public readonly static Tqdv tqdvtitle = new Tqdv(ID.WBS.Wb.title.N);


        public readonly static Tqdv tqdvrgrefwbRecent = new Tqdv(ID.WBS.Wbd.rgrefwbRecent.N);


        public readonly static Tqdv tqdvwbds = new Tqdv(ID.WBS.Wbd.wbds.N);


        public readonly static Tqdv tqdvTrue = new Tqdv("true", true);

        // Not used.

        public readonly static Tqdv tqdvTrueNode = new Tqdv("trueNode", ID.Kernel.True.B());

        /*
         * Allow switching between constructing Node and Sdi
         */

        public static Tqd tqdnMake(string stName, Tqd tqdIsa, Tqd tqdPayload, params Tqdt[] rgtqdt)
        {
            return new Tqds(stName, tqdIsa, tqdPayload, rgtqdt);
        }


        public static Tqd tqdnMake(string stName, Tqd tqdIsa, params Tqdt[] rgtqdt)
        {
            return new Tqds(stName, tqdIsa, null, rgtqdt);
        }


        public static Tqd TextFromSt(string st)
        {
            // from TU.TextFromSt
            if (String.IsNullOrEmpty(st))
                return null;
            // ID.Kernel.StringAtom.B(st);
            return tqdnMake("TextFromSt " + st, tqdvStringAtom,
                            new Tqdv("st " + st, st));
        }


        public static Tqd TqdFhWb(int ifh)
        {
            return tqdnMake("Wb" + ifh, tqdvWb);
        }


        public static Tqd TqdFhWbWithText(int ifh)
        {
            return tqdnMake("Wb" + ifh, tqdvWb,
                            new Tqdtrv(tqdvmain, TqdFhWboWithText()));
        }


        public static Tqd TqdFhWbd()
        {
            return tqdBuildWbd(1);
        }


        public static Tqd TqdApp()
        {
            Tqd tqdnApp = Tqds.tqdBuild("App", typeof(SEApp))
                             .Trait(typeof(NidWbd), TqdFhWbd());
            return tqdnApp;
        }


        public static Tqd TqdFhWbd(int cWb)
        {
            Debug.Assert(cWb > 0);

            Tqd[] rgtqdValue = new Tqd[cWb];
            for (int i = cWb - 1; i >= 0; --i)
                rgtqdValue[i] = TqdFhWb(i);

            return tqdnMake("Wbd", tqdvWbd,
                            TqdtrvWbdsForWbd(),
                            new Tqdtrv(tqdvmain, rgtqdValue));
        }


        public static Tqd TqdFhWbdWithText()
        {
            return tqdnMake("Wbd", tqdvWbd,
                            TqdtrvWbdsForWbd(),
                            new Tqdtrv(tqdvmain, TqdFhWbWithText(0)));
        }


        public static Tqd TqdFhWboWithText()
        {
            return tqdnMake("Outline", tqdvOutline,
                            new Tqdtrv(tqdvptwbOrigin, tqdnMake("CxPt", tqdvCxPt,
                                                                 new Tqdtrv(tqdvmain, tqdnMake("NumberLitX", tqdvNumberLit, new Tqdv("X", 880.0)),
                                                                                      tqdnMake("NumberLitY", tqdvNumberLit, new Tqdv("Y", 500.0))))),
                            new Tqdtrv(tqdvmain, tqdnMake("Outline Item", tqdvOutlineItem,
                                                          new Tqdtrv(tqdvmain, tqdnMake("ParagraphText", tqdvParagraphText,
                                                                                        new Tqdtrv(tqdvmain, TextFromSt("This is a text! How cool!")))))));
        }


        public static Tqdtrv TqdtrvWbdsForWbd()
        {
            return new Tqdtrv(tqdvwbds, tqdnMake("Wbds", tqdvWbds));
        }


        public static Tqd tqdBuildWbWithText(int ifh)
        {
            Tqd tqdnFhWb = Tqds.tqdBuild("FhWb" + ifh, typeof(GEN.SEWhiteboardPage))
                               .Field("location", tqdvScreenDimensions)
                               .Trait(ID.Kernel.main.N, TqdFhWboWithText());
            return tqdnFhWb;
        }


        public static Tqd tqdBuildWbdWithText(int cWb)
        {
            Debug.Assert(cWb > 0);

            Tqd[] rgtqdValue = new Tqd[cWb];
            for (int i = cWb - 1; i >= 0; --i)
                rgtqdValue[i] = tqdBuildWbWithText(i);

            return Tqds.tqdBuild("Wbd", typeof(SEWbd))
                       .Trait(ID.Kernel.main.N, rgtqdValue)
                       .Trait(ID.WBS.Wbd.wbds.N, Tqds.tqdBuild("Wbds", typeof(SEWbds)));
        }


        public static Tqd tqdBuildWbdWithText()
        {
            return tqdBuildWbdWithText(1);
        }

        private static Tqdv tqdvScreenDimensions = new Tqdv("screen dimensions", Parameters.serScreenDimensions);


        public static Tqd tqdBuildWb(int ifh)
        {
            Tqd tqdnFhWb0 = Tqds.tqdBuild("FhWb" + ifh, typeof(GEN.SEWhiteboardPage))
                                .Field("location", tqdvScreenDimensions);
            return tqdnFhWb0;
        }

        // potential replacement for TqdMakeWbd()


        public static Tqd tqdBuildWbd(int cWb)
        {
            Debug.Assert(cWb > 0);

            Tqd[] rgtqdValue = new Tqd[cWb];
            for (int i = cWb - 1; i >= 0; --i)
                rgtqdValue[i] = tqdBuildWb(i);

            return Tqds.tqdBuild("Wbd", typeof(SEWbd))
                       .Trait(ID.Kernel.main.N, rgtqdValue)
                       .Trait(ID.WBS.Wbd.wbds.N, Tqds.tqdBuild("Wbds", typeof(SEWbds)));
        }
    }

    [TestFixture, NoPreview]
    [Owner(Kuser.GJG)]
    public class TBGeneratorTests
    {

        private readonly static Tqdv tqdvA = new Tqdv("A", "a");


        private readonly static Tqdv tqdvB = new Tqdv("B", "b");


        private readonly static Tqdv tqdvC = new Tqdv("C", "c");


        private readonly static Tqdv tqdvD = new Tqdv("D", "d");


        private readonly static Tqda tqdaAb = new Tqda("aAB", tqdvA, tqdvB);


        private readonly static Tqda tqdaCd = new Tqda("aCD", tqdvC, tqdvD);


        private readonly static Tqda tqdaAbcd = new Tqda("aABCD", tqdaAb, tqdaCd);


        private readonly static Tqde tqdeAB = new Tqde("eAB", tqdvA, tqdvB);


        private readonly static Tqde tqdeCD = new Tqde("eCD", tqdvC, tqdvD);
        

        private readonly static Tqde tqdeABxCD = new Tqde("eABxCD", tqdaAb, tqdaCd);
        

        private readonly static Tqde tqdeABCD = new Tqde("eABCD", tqdeAB, tqdeCD);

        [Test]
        public void _01_TestAlternatives()
        {
            Ivd ivdData = null;

            Tqs tqsMain = tqdaAbcd.TqsMake(null);
            Assert.AreEqual(tqsMain.fInitialize(ivdData), KInitializeResult.Succeeded);
            Assert.AreEqual("a", tqsMain.objCurrent(null));
            Assert.AreEqual(tqsMain.fMoveNext(ivdData), KVisitResult.Stop);
            Assert.AreEqual("b", tqsMain.objCurrent(null));
            Assert.AreEqual(tqsMain.fMoveNext(ivdData), KVisitResult.Stop);
            Assert.AreEqual("c", tqsMain.objCurrent(null));
            Assert.AreEqual(tqsMain.fMoveNext(ivdData), KVisitResult.Stop);
            Assert.AreEqual("d", tqsMain.objCurrent(null));
            Assert.AreEqual(tqsMain.fMoveNext(ivdData), KVisitResult.Continue);
        }

        //GJG: Null?
        public static void CheckEnum(IEnumerable en_obj, params object[] expected)
        {
            IEnumerator rgobj = en_obj.GetEnumerator();

            foreach (object obj in expected)
            {
                Assert.IsTrue(rgobj.MoveNext());
                Assert.AreEqual(obj, rgobj.Current);
            }
            Assert.IsFalse(rgobj.MoveNext());         
        }

        [Test]
        public void _02_TestEnumerationAB()
        {
            Ivd ivdData = null;

            Tqs tqsMain = tqdeAB.TqsMake(null);
            Assert.AreEqual(tqsMain.fInitialize(ivdData), KInitializeResult.Succeeded);
            IEnumerable en_obj = (IEnumerable)tqsMain.objCurrent(null);
            CheckEnum(en_obj, "a", "b");
        }

        [Test]
        public void _03_TestEnumerationeABCD()
        {
            Ivd ivdData = null;

            Tqs tqsMain = tqdeABCD.TqsMake(null);
            Assert.AreEqual(tqsMain.fInitialize(ivdData), KInitializeResult.Succeeded);
            IEnumerable en_obj = (IEnumerable)tqsMain.objCurrent(null);
            CheckEnum(en_obj, "a", "b", "c", "d");
        }

        [Test]
        public void _04_TestEnumerationABxCD()
        {
            Ivd ivdData = null;
            Tqs tqsMain = tqdeABxCD.TqsMake(null);
            IEnumerable en_obj = tqsMain.en_objGenerate(ivdData);
            IEnumerator rgobj = en_obj.GetEnumerator();

            Assert.IsTrue(rgobj.MoveNext());
            CheckEnum((IEnumerable)rgobj.Current, "a", "c");
            Assert.IsTrue(rgobj.MoveNext());
            CheckEnum((IEnumerable)rgobj.Current, "a", "d");
            Assert.IsTrue(rgobj.MoveNext());
            CheckEnum((IEnumerable)rgobj.Current, "b", "c");
            Assert.IsTrue(rgobj.MoveNext());
            CheckEnum((IEnumerable)rgobj.Current, "b", "d");

            Assert.IsFalse(rgobj.MoveNext());
        }

        public static void RunWBTest(Tqd tqdWbd)
        {
            Ivd ivdData = null;
            Tqs tqsMain = tqdWbd.TqsMake(null);
            Assert.AreEqual(tqsMain.fInitialize(ivdData), KInitializeResult.Succeeded);

            SEWbd seWbd = (SEWbd)tqsMain.objCurrent(null);
            Node nodeWbd = seWbd.nodeBuildTree();

            TBRun run = new TBRun();
            run.RunTest(nodeWbd);
        }

        [Test, NoPreview, Description("Document with a single Whiteboard")]
        public void _05_TB_Whiteboards_Single_Whiteboard()
        {
            RunWBTest(TBTestCommon.tqdBuildWbd(1));
        }

        [Test, NoPreview, Description("Document with Multiple Whiteboards")]
        public void _06_TB_Whiteboards_Multi_Whiteboard()
        {
            RunWBTest(TBTestCommon.tqdBuildWbd(6));
        }

        [Test]
        public void _08_Copy()
        {
            Sdi.Init();
            const double xmax1 = 11;
            const double xmax2 = 21;

            SEWbd sewdbS = new SEWbd();
            GEN.SEWhiteboardPage sewdS1 = new GEN.SEWhiteboardPage();
            sewdS1.location = new Ser(12, xmax1, 13, 14);
            GEN.SEWhiteboardPage sewdS2 = new GEN.SEWhiteboardPage();
            sewdS2.location = new Ser(22, xmax2, 23, 24);
            sewdbS.svlMain.Add(sewdS1);
            sewdbS.svlMain.Add(sewdS2);

            Sdi.Init();  // copy is only used now to go from one state to the next
            SEWbd sewdbD = (SEWbd)sewdbS.SdiCopy(null);
            GEN.SEWhiteboardPage sewdD1 = (GEN.SEWhiteboardPage)sewdbD.svlMain[0];
            GEN.SEWhiteboardPage sewdD2 = (GEN.SEWhiteboardPage)sewdbD.svlMain[1];

            Debug.Assert(sewdD1.location != null);
            Assert.AreEqual(sewdD1.location.x.Max, xmax1);
            Debug.Assert(sewdD2.location != null);
            Assert.AreEqual(sewdD2.location.x.Max, xmax2);
        }

        [Sdv(typeof(ID.WBS.Wbd))]
        public class SESmall : Sdi, IModelRoot, IPtPosition
        {
            // Set by Sdi.Sdi() using reflection.
    
            [MaList(typeof(ID.Kernel.main))]
            public readonly Svl svlMain;

    
            public readonly Ser location;

            public bool fMenuOpen;
            public bool fMenuOpened;   // fields need to be public to be copied ( because GetType().GetFields() )
            public int cUpdates;

            // Disable all actions except those needed to cleanly close the app.
            public bool fInShutdownMode = false;

            public SESmall()
            {
                svlMain = new Svl(this, new MaList(typeof(ID.Kernel.main)));
                location = Screen.serRegionScreen();
                fMenuOpen = false;
                fMenuOpened = false;
                cUpdates = 0;
            }

            public SESmall(object objPayload, params Sdi[] rgsdiMain)
                : this()
            {
                Payload = objPayload;
                foreach (Sdi sdi in rgsdiMain)
                    svlMain.Add(sdi);
            }

            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                SESmall ses = (SESmall)obj;
                Debug.Assert(ses != null);
                return svlMain.Equals(ses.svlMain);
            }

            public override int GetHashCode()  // suppress error message
            {
                return base.GetHashCode();
            }

            public void EnterShutdownMode()
            {
                fInShutdownMode = true;
            }

    
            public static Cmd cmdCloseMenuSmall = new Cmd("CloseMenuSmall", typeof(SESmall), CmdCloseMenu);

    
            public static Cmd cmdOpenMenuSmall = new Cmd("OpenMenuSmall", typeof(SESmall), CmdOpenMenu);

    
            public static Cmd cmdUpdateSmall = new Cmd("UpdateSmall", typeof(SESmall), CmdUpdate);

            public override void RegisterActions(Sdtl sdtl)
            {
                Debug.Assert(Payload != null);

#if DEBUG_REGISTER_ACTIONS
                Console.WriteLine("RegisterActions begin");
                Console.WriteLine("  Node {0}", sdtl.sdi.iNode);
#endif

                if (Payload.Equals("a"))
                {
#if DEBUG_REGISTER_ACTIONS
                    Console.WriteLine("RegisterActions end");
#endif
                    return;
                }

                if (fMenuOpen)
                {
                    sdtl.Add(this, cmdCloseMenuSmall, G3.smePrimaryTap, smpClickSmall);
#if DEBUG_REGISTER_ACTIONS
                    Console.WriteLine("  Menu close");
#endif
                }
                else if (!fMenuOpened)
                {
                    sdtl.Add(this, cmdOpenMenuSmall, G3.smeScenario, new SmpScenario());
#if DEBUG_REGISTER_ACTIONS
                    Console.WriteLine("  Menu open");
#endif
                }

                if (Payload.Equals("c"))
                {
                    sdtl.Add(this, cmdUpdateSmall, G3.smePrimaryTap, smpClickSmall);
#if DEBUG_REGISTER_ACTIONS
                    Console.WriteLine("  Update");
#endif
                }

#if DEBUG_REGISTER_ACTIONS
                Console.WriteLine("RegisterActions end");
#endif

                base.RegisterActions(sdtl);
            }

            public override void RegisterActionsAux(Sdtl sdtl)
            {
                if (fInShutdownMode)
                    RegisterShutdownActions(sdtl);
                else
                    RegisterActions(sdtl);
            }

            // Not used.
            public Pt ptPosition(EnPos enposHor, EnPos enposVert)
            {
                return location.ptPosition(enposHor, enposVert);
            }

            public static void CmdOpenMenu(Sdi sdiPlace, Sde sde)
            {
                ((SESmall)sdiPlace).fMenuOpen = true;
                ((SESmall)sdiPlace).fMenuOpened = true;
            }

            public static void CmdCloseMenu(Sdi sdiPlace, Sde sde)
            {
                ((SESmall)sdiPlace).fMenuOpen = false;
            }

            public static void CmdUpdate(Sdi sdiPlace, Sde sde)
            {
                ((SESmall)sdiPlace).cUpdates++;
            }
        }


        public readonly static SmpClickSmall smpClickSmall = new SmpClickSmall();

        public class SmpClickSmall : Smp
        {
            public SmpClickSmall(Scp scp = null) : base(scp ?? SmpXY.scpDefault) {}
        }


        public readonly static Tqdv tqdvSmall = new Tqdv(typeof(SESmall));


        public readonly static SESmall sesChild1 = new SESmall("child1");


        public readonly static SESmall sesChild2 = new SESmall("child2");

        [Test]
        public void _10_CheckTqss0()
        {
            Ivd ivdData = null;
            Sdi.Init();
            Tqdtrv[] rgtqdt = new Tqdtrv[0];
            Tqd tqdSmall = new Tqds("small", tqdvSmall, tqdaAb, rgtqdt);
            Tqs tqsMain = tqdSmall.TqsMake(null);

            Assert.AreEqual(tqsMain.fInitialize(ivdData), KInitializeResult.Succeeded);

            Assert.AreEqual(new SESmall("a"), tqsMain.objCurrent(null));
            Assert.AreEqual(tqsMain.fMoveNext(ivdData), KVisitResult.Stop);
            Assert.AreEqual(new SESmall("b"), tqsMain.objCurrent(null));

            Assert.AreEqual(tqsMain.fMoveNext(ivdData), KVisitResult.Continue);
        }

        [Test]
        public void _11_CheckTqss1()
        {
            Ivd ivdData = null;
            Sdi.Init();
            Tqdtrv[] rgtqdt = new Tqdtrv[1];
            rgtqdt[0] = new Tqdtrv(TBTestCommon.tqdvmain, new Tqdv("vchild", sesChild1));
            Tqd tqdSmall = new Tqds("small", tqdvSmall, tqdaAb, rgtqdt);
            Tqs tqsMain = tqdSmall.TqsMake(null);

            Assert.AreEqual(tqsMain.fInitialize(ivdData), KInitializeResult.Succeeded);

            Assert.AreEqual(new SESmall("a", sesChild1), tqsMain.objCurrent(null));
            Assert.AreEqual(tqsMain.fMoveNext(ivdData), KVisitResult.Stop);
            Assert.AreEqual(new SESmall("b", sesChild1), tqsMain.objCurrent(null));

            Assert.AreEqual(tqsMain.fMoveNext(ivdData), KVisitResult.Continue);
        }

        [Test]
        public void _12_CheckTqss2()
        {
            Ivd ivdData = null;
            Sdi.Init();
            Tqdtrv[] rgtqdt = new Tqdtrv[1];
            rgtqdt[0] = new Tqdtrv(TBTestCommon.tqdvmain, new Tqda("child12", new Tqdv("vchild1", sesChild1), new Tqdv("vchild2", sesChild2)));
            Tqd tqdSmall = new Tqds("small", tqdvSmall, tqdaAb, rgtqdt);
            Tqs tqsMain = tqdSmall.TqsMake(null);

            Assert.AreEqual(tqsMain.fInitialize(ivdData), KInitializeResult.Succeeded);

            Assert.AreEqual(new SESmall("a", sesChild1), tqsMain.objCurrent(null));
            Assert.AreEqual(tqsMain.fMoveNext(ivdData), KVisitResult.Stop);
            Assert.AreEqual(new SESmall("a", sesChild2), tqsMain.objCurrent(null));
            Assert.AreEqual(tqsMain.fMoveNext(ivdData), KVisitResult.Stop);
            Assert.AreEqual(new SESmall("b", sesChild1), tqsMain.objCurrent(null));
            Assert.AreEqual(tqsMain.fMoveNext(ivdData), KVisitResult.Stop);
            Assert.AreEqual(new SESmall("b", sesChild2), tqsMain.objCurrent(null));

            Assert.AreEqual(tqsMain.fMoveNext(ivdData), KVisitResult.Continue);
        }

        // Like Tqdp, but use TqspTest instead
        public class TqdpeOnePath : Tqdp
        {
            public TqdpeOnePath(string stName, int cMaxPathLength)
                : base(stName, null, cMaxPathLength)
            {
            }

            protected override Tqsp tqspMake(Tqs tqsParent)
            {
                return new TqspeOnePath(tqsParent, this);
            }
        }

        // Like Tqdp, but use TqspTest instead
        public class TqdprOnePath : Tqdp
        {
            public TqdprOnePath(string stName, int cMaxPathLength)
                : base(stName, null, cMaxPathLength)
            {
            }

            protected override Tqsp tqspMake(Tqs tqsParent)
            {
                return new TqsprOnePath(tqsParent, this);
            }
        }

        // Like Tqsp, but just check for expected inputs.
        // Produces only one rgPath, but check that 
        public class TqspeOnePath : Tqspe
        {
            private int cPaths;

            public TqspeOnePath(Tqs tqsParent, Tqdp tqdp) : base(tqsParent, tqdp)
            {
                cPaths = -1;
            }

            public override KInitializeResult fDoInitialize(Ivd ivdData)
            {
                cPaths = 0;
                base.fDoInitialize(ivdData);
                return KInitializeResult.Succeeded;
            }

            public override KVisitResult fDoMoveNext(Ivd ivdData)
            {
                cPaths++;
                return KVisitResult.Continue; // no more here
            }

            public override object objCurrent(Imc mc)
            {
                Assert.AreEqual(1, cPaths);
                return Report();
            }
        }

        // Like Tqsp, but just check for expected inputs.
        // Produces only one rgPath, but check that 
        public class TqsprOnePath : Tqspr
        {
            private int cPaths;

            public TqsprOnePath(Tqs tqsParent, Tqdp tqdp)
                : base(tqsParent, tqdp)
            {
                cPaths = -1;
            }

            public override KInitializeResult fDoInitialize(Ivd ivdData)
            {
                cPaths = 0;
                base.fDoInitialize(ivdData);
                return KInitializeResult.Succeeded;
            }

            public override KVisitResult fDoMoveNext(Ivd ivdData)
            {
                cPaths++;
                return KVisitResult.Continue; // no more here
            }

            public override object objCurrent(Imc mc)
            {
                Assert.AreEqual(1, cPaths);
                return Report();
            }
        }

        [Test]
        public void _20_CheckExhaustiveIterationSetup()
        {
            Sdi.Init();
            // Use TqdpTest to just check that iteration over TBs and pathIterators is correctly set up
            Ivd ivdData = new Smcv();
            Tqdtrv[] rgtqdt = new Tqdtrv[0];
            Tqd tqdSmall = new Tqds("small", tqdvSmall, tqdaAb, rgtqdt);

            Tqdp tqdPath = new TqdpeOnePath("rgPath", 2);  // generate one dummy rgPath
            Tqdv tqdvPath = new Tqdv("pathGen", tqdPath);  // a need level of indirection to provide a specific Tqdp
            Tqdm tqdm = new Tqdm("top", tqdSmall, tqdvPath);  // provide sdiTB to the rgPath iterator
            Tqsm tqsCase = (Tqsm)tqdm.TqsMake(null);

            Assert.AreEqual(tqsCase.fInitialize(ivdData), KInitializeResult.Succeeded);

            Assert.AreEqual(new List<object> { "a", 2 }, tqsCase.objCurrent(null));
            Assert.AreEqual(tqsCase.fMoveNext(ivdData), KVisitResult.Stop);
            Assert.AreEqual(new List<object> { "b", 2 }, tqsCase.objCurrent(null));

            Assert.AreEqual(tqsCase.fMoveNext(ivdData), KVisitResult.Continue);
        }

        [Test]
        public void _21_CheckRandomIterationSetup()
        {
            Sdi.Init();
            // Use TqdpTest to just check that iteration over TBs and pathIterators is correctly set up
            Ivd ivdData = new Smcv();
            Tqdtrv[] rgtqdt = new Tqdtrv[0];
            Tqd tqdSmall = new Tqds("small", tqdvSmall, tqdaAb, rgtqdt);

            Tqdp tqdPath = new TqdprOnePath("rgPath", 2);  // generate one dummy rgPath
            Tqdv tqdvPath = new Tqdv("pathGen", tqdPath);  // a need level of indirection to provide a specific Tqdp
            Tqdm tqdm = new Tqdm("top", tqdSmall, tqdvPath);  // provide sdiTB to the rgPath iterator
            Tqsm tqsCase = (Tqsm)tqdm.TqsMake(null);

            Assert.AreEqual(tqsCase.fInitialize(ivdData), KInitializeResult.Succeeded);

            Assert.AreEqual(new List<object> { "a", 2 }, tqsCase.objCurrent(null));
            Assert.AreEqual(tqsCase.fMoveNext(ivdData), KVisitResult.Stop);
            Assert.AreEqual(new List<object> { "b", 2 }, tqsCase.objCurrent(null));

            Assert.AreEqual(tqsCase.fMoveNext(ivdData), KVisitResult.Continue);
        }

        [Test]
        public void _30_SmcSequence()
        {
            Sdi.Init();
            Ivd ivdData = new Smcv();

            Tqdtrv[] rgtqdt = new Tqdtrv[0];
            // Note: SESmall.RegisterTransitions defines different transitions, depending on the initial Payload
            Tqd tqdSmall = new Tqds("small", tqdvSmall, tqdaAbcd, rgtqdt);

            Tqdp tqdPath = new Tqdpe("rgPath", null, 3);  // construct SM and generate paths
            Tqdv tqdvPath = new Tqdv("pathGen", tqdPath);  // a need level of indirection to provide a specific Tqdp
            Tqdm tqdm = new Tqdm("top", tqdSmall, tqdvPath);  // provide sdiTB to the rgPath iterator
            Tqsm tqsCase = (Tqsm)tqdm.TqsMake(null);

            int[] rgcCountCases = new int[4];
            Assert.AreEqual(tqsCase.fInitialize(ivdData), KInitializeResult.Succeeded);
            do
            {
                Smc smcCase = (Smc)ivdData.imcGet();
                string stPayload = (string)((Scdp)(smcCase.rgIss[smcCase.rgIss.Count-1])).objPayload;
                Debug.Assert(stPayload != null);
                rgcCountCases[stPayload[0] - 'a']++;
                if (stPayload.Equals("a"))
                {
                    Assert.AreEqual(0, smcCase.nPathLength);
                }
                else if (stPayload.Equals("b"))
                {
                    Assert.AreEqual(2, smcCase.nPathLength);
                }
                else if (stPayload.Equals("c"))
                {
                    // O C U
                    // O U C
                    // O U U
                    // U O C
                    // U O U
                    // U U O
                    // U U U
                    Assert.AreEqual(3, smcCase.nPathLength);
                }
                else if (stPayload.Equals("d"))
                {
                    Assert.AreEqual(2, smcCase.nPathLength);
                }
                else
                    Assert.Fail("unexpected payload");
            } while (tqsCase.fMoveNext(ivdData) == KVisitResult.Stop);
            Assert.AreEqual(1, rgcCountCases[0]);
            Assert.AreEqual(1, rgcCountCases[1]);
            Assert.AreEqual(7, rgcCountCases[2]);
            Assert.AreEqual(1, rgcCountCases[3]);
        }

        [Test]
        public void _40_Build()
        {
            RunWBTest(TBTestCommon.tqdBuildWbd(5));
        }
    }
#endif
}
