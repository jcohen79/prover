using System.Collections.Generic;
using System;

namespace reslab.TestUtil
{
    /// <summary>
    /// Define a pattern of Tst, Tob to pattern match a portion of a proof outline
    /// </summary>
    public class Mpt
    {
        public string stName;
        public Psb psbFirst;   // first step, rest are linked via actions
        public Mgp mgpFirstMtg; // list of Mtg_s where this is used
        public Mso msoFirst;    // list of locals/parameters

        public Mpt(string stName)
        {
            this.stName = stName;
        }

        public void AddMso (Mso mso)
        {
            mso.msoNext = msoFirst;
            msoFirst = mso;
        }

        public Mso msoFindLocal (Vrf vrfSymbol, Mpi mpiOccurs)
        {
            for (Mso mso = msoFirst; mso != null; mso = mso.msoNext)
            {
                if (mso.vrfSymbol == vrfSymbol && mso.mpiAppears == mpiOccurs)
                    return mso;
            }
            return null;
        }
    }

    /// <summary>
    /// Tag: Group a set of Mpt to reduce search space for match
    /// </summary>
    public class Mtg
    {
        public string stName;
        public Mgp mgpFirstMpt;   // list of Mpt_s in this tag

        public void AddMpt(Mpt mptToAdd)
        {
            Mgp mgp = new Mgp();
            mgp.mpt = mptToAdd;
            mgp.mgpNextSameMpt = mptToAdd.mgpFirstMtg;
            mptToAdd.mgpFirstMtg = mgp;
            mgp.mgpNextSameMtg = mgpFirstMpt;
            mgpFirstMpt = mgp;
        }
    }

    /// <summary>
    /// Used to track iteration through mpt_s in the tag. One mpt can be on several Mtg
    /// </summary>
    public class Mgp
    {
        public Mgp mgpNextSameMtg;
        public Mgp mgpNextSameMpt;
        public Mpt mpt;
    }

    /// <summary>
    /// Insertion point in Mpt pattern that is expanded into a lower level instance of an Mpt
    /// </summary>
    public class Mpr : Psb
    {
        public Mtg mtgAllowed;      // refers to the set of Mpt that can be applied to this point
        public Mdv mdvFirstBranch;  // list of branches to next steps that pattern has Mds for exit destinations

        public Mpr(Mtg mtgAllowed)
        {
            this.mtgAllowed = mtgAllowed;
        }

        public void AddMdv(Mds mdsSymbol, Psb psbValue)
        {
            Mdv mdv = new Mdv();
            mdv.mdsSymbol = mdsSymbol;
            mdv.psbValue = psbValue;
            mdv.mdvNext = mdvFirstBranch;
            mdvFirstBranch = mdv;
        }
    }

    /// <summary>
    /// A symbol that represents a destination step (e.g. exit from a pattern)
    /// </summary>
    public class Mds : Psb
    {
        public readonly string stName;

        public Mds(string stName)
        {
            this.stName = stName;
        }
    }

    public class Mdv
    {
        public Mds mdsSymbol;   // symbol used
        public Psb psbValue;    // destination value assigned 
        public Mdv mdvNext;     // next mds used in an pattern instantiation, with its assigned value
    }

    /// <summary>
    /// Instance of applying a Mpt
    /// The result of matching is a tree of these
    /// </summary>
    public class Mpi
    {
        public Mpt mptPattern;
        public Mpi mpiFirstChild;  // list of how insertion points are matched
        public Mpr mprPatternRef;
        public Mpi mpiNextChild;
        public int nDepth;
        public Mso msoFirst;   // value holders needed locally or used below
        public Mpi mpiParent;   // this is match of one step pattern for mpiParent
        public Mdv mdvFirstExit;    // list of destination symbols used in this pattern

        public Mpi(Mpt mpt, Mpi mpiParent)
        {
            this.mptPattern = mpt;
            this.mpiParent = mpiParent;
            this.nDepth = mpiParent.nDepth + 1;
        }

        public void AddChild (Mpi mpiChild)
        {
            mpiChild.mpiNextChild = mpiFirstChild;
            mpiFirstChild = mpiChild;
        }

        public Mso msoFindSource (Vrf vrfSymbol, Mpi mpiOccurs)
        {
            for (Mso mso = msoFirst; mso != null; mso = mso.msoNext)
            {
                if (mso.vrfSymbol == vrfSymbol && mso.mpiAppears == mpiOccurs)
                    return mso;
            }
            return null;
        }

        /// <summary>
        /// Return the closest ancestor of this and mpiOther
        /// </summary>
        public Mpi mpiCommonAncestor(Mpi mpiOther)
        {
            Mpi mpiThis = this;
            while (mpiThis != mpiOther)
            {
                if (mpiThis.nDepth > mpiOther.nDepth)
                    mpiThis = mpiThis.mpiParent;
                else if (mpiThis.nDepth < mpiOther.nDepth)
                    mpiOther = mpiOther.mpiParent;
                else
                {
                    mpiThis = mpiThis.mpiParent;
                    mpiOther = mpiOther.mpiParent;
                }
            }
            return mpiThis;
        }

        public void MarkLocalUpToCommon(Mpi mpiCommonAncestor, Mso msoSource)
        {
            Vrf vrfSource = msoSource.vrfSymbol;
            Mpi mpiSource = msoSource.mpiAppears;
            bool fLast = false;
            Mpi mpiPlace = this;
            while (true)
            {
                fLast = mpiPlace == mpiCommonAncestor;
            
                Mso msoLocal = mpiPlace.mptPattern.msoFindLocal(vrfSource, mpiSource);
                if (msoLocal != null)
                {
                    if (msoLocal.kUsage == KMsoUsage.kLocal && !fLast)
                        msoLocal.kUsage = KMsoUsage.kImported;
                }
                else
                {
                    Mso mso = new Mso();
                    mso.kUsage = fLast ? KMsoUsage.kLocal : KMsoUsage.kImported;
                    mso.vrfSymbol = vrfSource;
                    mso.mpiAppears = mpiSource;
                    mpiPlace.mptPattern.AddMso(mso);
                }

                if (fLast)
                    break;
                mpiPlace = mpiPlace.mpiParent;
            }
        }


        /// <summary>
        /// The exit for a pattern is to a symbol that represents the destination of the outline. Check
        /// for conflict if there are multiple occurences.
        /// </summary>
        public Psb psbNextForPattern(Psb psbPatternNext, Psb psbOutlineNext)
        {
            if (!(psbPatternNext is Mds))
                return psbPatternNext;  // is not a symbol
            Psb psbPatternDest = null;
            for (Mdv mdv = mprPatternRef.mdvFirstBranch; mdv != null; mdv = mdv.mdvNext)
            {
                if (mdv.mdsSymbol == psbPatternNext)
                {
                    psbPatternDest = mdv.psbValue;
                    break;
                }
            }
            if (psbPatternDest == null)
                throw new InvalidProgramException("pattern exit not found on Mpr");
            // check for consistent usage
            for (Mdv mdv = mdvFirstExit; mdv != null; mdv = mdv.mdvNext)
            {
                if (mdv.mdsSymbol == psbPatternNext)
                {
                    return (mdv.psbValue == psbOutlineNext) ? psbPatternDest : null;
                }
            }
            // add to list to check for conflict later
            Mdv mdvNew = new Mdv();
            mdvNew.mdsSymbol = (Mds)psbPatternNext;
            mdvNew.psbValue = psbOutlineNext;  // to check for consistent usage
            mdvNew.mdvNext = mdvFirstExit;
            mdvFirstExit = mdvNew;

            return psbPatternDest;
        }
    }

    /// <summary>
    /// A pattern will match or fail as a whole, so group the match info together
    /// How does this relate to ppi?
    /// </summary>
    public class Mfr
    {
        public Mpi mpi = null;
        public Mfr mfrParent = null;
        public Mso msoFirst = null;   // bindings in progress 

        private Mfr() { }

        /// <summary>
        /// Create new Mfr as child of current and save on Msk
        /// This will causes new Mbn bindings to be local to current pattern
        /// </summary>
        public static void Push(Msk msk)
        {
            Mfr mfr = new Mfr();
            mfr.mfrParent = msk.mfrMatchInfo;
            mfr.msoFirst = msk.mfrMatchInfo.msoFirst;
            msk.mfrMatchInfo = mfr;
        }

        /// <summary>
        /// Return where source outline is matched
        /// </summary>
        public void SaveSource (Mso mso)
        {
            mso.msoNext = msoFirst;
            msoFirst = mso;
        }


        /// <summary>
        /// Return the Mso that matched the source for vrfOutline
        /// </summary>
        public Mso msoSource (Vrf vrfOutline)
        {
            for (Mso mso = msoFirst; mso != null; mso = mso.msoNext)
            {
                if (mso.vrfOutline == vrfOutline)
                    return mso;
            }
            return null;
        }
    }

    public enum KMsoUsage
    {
        kLocal,    // 
        kImported,
    }

    /// <summary>
    /// Symbol occurence: is qualified by the Mpi in which it occurs (to allow reused of Mpa)
    /// </summary>
    public class Mso
    {
        public Vrf vrfSymbol;
        public Mpi mpiAppears;
        public KMsoUsage kUsage;
        public Vrf vrfOutline;
        public Mso msoNext;

        public Mso()
        {
        }
    }

    public enum KMskState
    {
        kMatchArgs,     // process args of an action
        kNextAction,   // proceed to the next action
        kStartStep,    // check for pattern or start set of actions
        kNextPattern,  // try next alternative pattern
        kAcceptPattern,  // when all branches of pattern have been matched
    }

    /// <summary>
    /// Stack for backtracking through matches of a sequence
    /// </summary>
    public class Msk
    {
        public Psb psbPatternStep = null;
        public Psb psbOutlinePlace = null;
        public Pst pstPatternAction = null;
        public Pst pstOutlineAction = null;
        public Mgp mgpCurrent = null;

        public Mfr mfrMatchInfo;
        public Msk mskBack;  // pop to backtrack

        public KMskState kState;

        private Msk()
        {
            kState = KMskState.kMatchArgs;
        }

        public Msk mskPop()
        {
            return mskBack;
        }

        // Pop back to exit pattern
        public Msk mskFail(KMskState kPopToState)
        {
            Msk mskPlace = this;
            do
            {
                mskPlace = mskPlace.mskBack;
                if (mskPlace.kState == kPopToState)
                    return mskPlace;
            }
            while (mskPlace != null);
            throw new InvalidProgramException();
        }

        public static Msk mskStart(Mtg mtgInitial, Psb psbOutlinePlace)
        {
            Msk mskNew = new Msk();
            mskNew.mgpCurrent = mtgInitial.mgpFirstMpt;
            mskNew.psbOutlinePlace = psbOutlinePlace;
            mskNew.kState = KMskState.kNextPattern;

            return mskNew;
        }

        public Msk mskPush()
        {
            Msk mskNew = new Msk();
            mskNew.mskBack = this;
            mskNew.mfrMatchInfo = mfrMatchInfo;

            return mskNew;
        }

        /// <summary>
        /// Push a msk that searchs to match the branch destination on the current pattern and outline
        /// </summary>
        public Msk mskMatchDest()
        {
            Psb psbPatternNext = ((Tob) this.pstPatternAction).psbNext;
            Psb psbOutlineNext = ((Tob) this.pstOutlineAction).psbNext;

            // replace a pattern dest that is a Msd with the corresponding value from enclosing Mpi
            Psb psbPatternRepl = mfrMatchInfo.mpi.psbNextForPattern(psbPatternNext, psbOutlineNext);
            if (psbPatternRepl == null)
                return mskFail(KMskState.kNextPattern);

            Msk mskNew;
            if (pstPatternAction.pobNext == null)
            {
                // this is last action in step, no need for push
                if (pstOutlineAction.pobNext != null)
                    return mskFail(KMskState.kNextPattern);
                kState = KMskState.kNextAction;
                mskNew = this;
            }
            else
            {
                // push to handle branch before doing remaining actions this step
                if (pstOutlineAction.pobNext == null)
                    return mskFail(KMskState.kNextPattern);
                mskNew = mskPush();
            }
            mskNew.kState = KMskState.kStartStep;
            mskNew.psbPatternStep = psbPatternRepl;
            mskNew.psbOutlinePlace = psbOutlineNext;

            return mskNew;
        }
    }

    /// <summary>
    /// Base class for handler for each type of Tob
    /// </summary>
    public abstract class Mah
    {
        public readonly System.Type tActionType;
        public bool fMatchType(Pst pstAction)
        {
            return pstAction.GetType() == tActionType;
        }

        // Check if arguments match pattern so far
        // Return new msk if there is a dest to process, or same msk to proceed, if null doesn't match
        public abstract Msk mskMatchArguments(Msk msk);

        public static Mah mahTod = new MahTod();
        public static Mah mahTop = new MahTop();
        public static Mah mahTos = new MahTos();
        public static Mah mahToi = new MahToi();
        public static Mah mahTow = new MahTow();
        public static Mah mahTov = new MahTov();
        public static Mah mahToa = new MahToa();
        public static Mah mahTohAsc = new MahToh<Asc>();
        public static Mah mahTohPti = new MahToh<Pti>();

        public static Mah[] rgmahList =
            {
                mahTod,
                mahTop,
                mahTos,
                mahToi,
                mahTow,
                mahTov,
                mahToa,
                mahTohAsc,
                mahTohPti,
            };

        public Mah(System.Type tActionType)
        {
            this.tActionType = tActionType;
        }

        /// <summary>
        /// Associate pattern variable with outline.
        /// Return false if conflict.
        /// </summary>
        public static bool fBindSource(Msk msk, Vrf vrfPattern, Vrf vrfOutline)
        {
            // check if vrfPattern is already bound
            Mpi mpiCurrent = msk.mfrMatchInfo.mpi;
            if (mpiCurrent.mptPattern.msoFindLocal(vrfPattern, mpiCurrent) != null)
                return false;

            Mso mso = new Mso();
            mso.kUsage = KMsoUsage.kLocal;
            mso.vrfSymbol = vrfPattern;
            mso.mpiAppears = msk.mfrMatchInfo.mpi;
            mso.vrfOutline = vrfOutline;
            msk.mfrMatchInfo.SaveSource(mso);

            return true;
        }

        /// <summary>
        /// Find or create a binding for the pattern symbol to the outline symbol
        /// </summary>
        /// <returns>True if binding is successful</returns>
        public bool fBindArgument(Msk msk, Vrf vrfPattern, Vrf vrfOutline, bool fSource)
        {
            // Find the Mso where the source of vrfOutline was matched, under current pattern match state
            Mso msoSource = msk.mfrMatchInfo.msoSource(vrfOutline);

            Mpi mpiSource = msoSource.mpiAppears;
            Mpi mpiDest = msk.mfrMatchInfo.mpi;
            if (mpiDest.mptPattern.msoFindLocal(vrfPattern, mpiDest) != null)
                return false;

            Mpi mpiCommonAncestor = mpiSource.mpiCommonAncestor(mpiDest);
            // loop up from both source and here, mark all but top as imported
            mpiSource.MarkLocalUpToCommon(mpiCommonAncestor, msoSource);
            mpiDest.MarkLocalUpToCommon(mpiCommonAncestor, msoSource);
            return true;
        }
    }

    class MahTop : Mah
    {
        public MahTop() : base(typeof(Top)) { }
        public override Msk mskMatchArguments(Msk msk)
        {
            Top topPattern = (Top)msk.pstPatternAction;
            Top topOutline = (Top)msk.pstOutlineAction;
            if (topPattern.tcd != topOutline.tcd)
                return msk.mskFail(KMskState.kNextPattern);
            if (!fBindArgument(msk, topPattern.ibdTarget, topOutline.ibdTarget, false))
                return msk.mskFail(KMskState.kNextPattern);
            if (!fBindArgument(msk, topPattern.ibdInput, topOutline.ibdInput, false))
                return msk.mskFail(KMskState.kNextPattern);
            // If there is a dest on this type of action, push new msk and return that
            msk.kState = KMskState.kNextAction;
            return msk.mskMatchDest();
        }
    }

    class MahTod : Mah
    {
        public MahTod() : base(typeof(Tod)) { }
        public override Msk mskMatchArguments(Msk msk)
        {
            Tod todPattern = (Tod)msk.pstPatternAction;
            Tod todOutline = (Tod)msk.pstOutlineAction;
            // TODO: check for different order of multiple Tod with tKind exchanged
            if (todPattern.kKind != todOutline.kKind)
                return msk.mskFail(KMskState.kNextPattern);
            if (!fBindSource(msk, todPattern.hbvPlace, todOutline.hbvPlace))
                return msk.mskFail(KMskState.kNextPattern);
            msk.kState = KMskState.kNextAction;
            return msk.mskMatchDest();
        }
    }

    class MahTog : Mah
    {
        public MahTog() : base(typeof(Tog)) { }
        public override Msk mskMatchArguments(Msk msk)
        {
            Tog togPattern = (Tog)msk.pstPatternAction;
            Tog togOutline = (Tog)msk.pstOutlineAction;
            if (togPattern.tcd != togOutline.tcd)
                return msk.mskFail(KMskState.kNextPattern);
            if (!fBindArgument(msk, togPattern.ibdTarget, togOutline.ibdTarget, false))
                return msk.mskFail(KMskState.kNextPattern);
            msk.kState = KMskState.kNextAction;
            return msk.mskMatchDest();
        }
    }

    class MahTos : Mah
    {
        public MahTos() : base(typeof(Tos)) { }
        public override Msk mskMatchArguments(Msk msk)
        {
            Tos tosPattern = (Tos)msk.pstPatternAction;
            Tos tosOutline = (Tos)msk.pstOutlineAction;
            if (tosPattern.hascLeft != tosOutline.hascLeft)
                return msk.mskFail(KMskState.kNextPattern);
            if (tosPattern.hascRight != tosOutline.hascRight)
                return msk.mskFail(KMskState.kNextPattern);
            // don't check tspPhase, it is set in fPerform
            msk.kState = KMskState.kNextAction;
            return msk.mskMatchDest();
        }
    }

    class MahToi : Mah
    {
        public MahToi() : base(typeof(Toi)) { }
        public override Msk mskMatchArguments(Msk msk)
        {
            Toi toiPattern = (Toi)msk.pstPatternAction;
            Toi toiOutline = (Toi)msk.pstOutlineAction;
            if (toiPattern.tcd != toiOutline.tcd)
                return msk.mskFail(KMskState.kNextPattern);
            if (!fBindArgument(msk, toiPattern.ibdInput, toiOutline.ibdInput, false))
                return msk.mskFail(KMskState.kNextPattern);
            msk.kState = KMskState.kNextAction;
            return msk.mskMatchDest();
        }
    }

    class MahTow : Mah
    {
        public MahTow() : base(typeof(Tow)) { }
        public override Msk mskMatchArguments(Msk msk)
        {
            Tow towPattern = (Tow)msk.pstPatternAction;
            Tow towOutline = (Tow)msk.pstOutlineAction;
            if (!fBindArgument(msk, towPattern.ibdToWatch, towOutline.ibdToWatch, false))
                return msk.mskFail(KMskState.kNextPattern);
            msk.kState = KMskState.kNextAction;
            return msk.mskMatchDest();
        }
    }

    class MahTov : Mah
    {
        public MahTov() : base(typeof(Tov)) { }
        public override Msk mskMatchArguments(Msk msk)
        {
            Tov tovPattern = (Tov)msk.pstPatternAction;
            Tov tovOutline = (Tov)msk.pstOutlineAction;
            if (tovPattern.kValue != tovOutline.kValue)
                return msk.mskFail(KMskState.kNextPattern);
            if (!fBindArgument(msk, tovPattern.ibdValue, tovOutline.ibdValue, false))
                return msk.mskFail(KMskState.kNextPattern);
            
            msk.kState = KMskState.kNextAction;
            return msk.mskMatchDest();
        }
    }

    class MahToa : Mah
    {
        public MahToa() : base(typeof(Toa)) { }
        public override Msk mskMatchArguments(Msk msk)
        {
            Toa toaPattern = (Toa)msk.pstPatternAction;
            Toa toaOutline = (Toa)msk.pstOutlineAction;
            if (toaPattern.tff != toaOutline.tff)
                return msk.mskFail(KMskState.kNextPattern);
            if (!fBindArgument(msk, toaPattern.ibdResult, toaOutline.ibdResult, false))
                return msk.mskFail(KMskState.kNextPattern);

            for (int i = 0; i < toaPattern.rgibdfArgs.Count; i++)
            {
                if (!fBindArgument(msk, toaPattern.rgibdfArgs[i], toaOutline.rgibdfArgs[i], false))
                    return msk.mskFail(KMskState.kNextPattern);
            }
            msk.kState = KMskState.kNextAction;
            return msk.mskMatchDest();
        }
    }

    class MahToh<T> : Mah where T : Ibd
    {
        public MahToh() : base(typeof(Toh<T>)) { }
        public override Msk mskMatchArguments(Msk msk)
        {
            Toh<T> tohPattern = (Toh<T>)msk.pstPatternAction;
            Toh<T> tohOutline = (Toh<T>)msk.pstOutlineAction;
            if (!tohPattern.stVhdName.Equals(tohOutline.stVhdName))
                return msk.mskFail(KMskState.kNextPattern);
            if (tohPattern.tin != tohOutline.tin)
                return msk.mskFail(KMskState.kNextPattern);
            if (!fBindArgument(msk, tohPattern.ibdValue, tohOutline.ibdValue, false))
                return msk.mskFail(KMskState.kNextPattern);
            msk.kState = KMskState.kNextAction;
            return msk.mskMatchDest();
        }
    }

    /// <summary>
    /// Perform a match of a proof outline to a set of patterns and produce a tree
    /// </summary>
    public class Mch
    {
        /// <summary>
        /// Perform match on top level pattern to entire outline
        public static Mpi ppiMatchSeq(Mtg mtgInitial, Psb psbOutlineStart)
        {
            Msk msk = Msk.mskStart(mtgInitial, psbOutlineStart);
            while (true)
            {
                if (msk == null)
                    return null;  // failed to match
                switch (msk.kState)
                {
                    case KMskState.kMatchArgs:
                        if (msk.pstPatternAction == null)
                        {
                            throw new InvalidProgramException();
                        }
                        bool fFoundHandler = false;
                        // find matcher based on action type (there is one per action type)
                        foreach (Mah mah in Mah.rgmahList)
                        {
                            if (mah.fMatchType(msk.pstPatternAction))
                            {
                                fFoundHandler = true;
                                // check if action in outline and pattern match
                                // push, pop, or reuse the msk
                                msk = mah.mskMatchArguments(msk);
                                break;  // there is only one Mah that will match
                            }
                        }
                        if (!fFoundHandler)
                            throw new InvalidProgramException();
                        break;
                    case KMskState.kNextAction:
                        msk.pstPatternAction = msk.pstPatternAction.pobNext;
                        msk.pstOutlineAction = msk.pstOutlineAction.pobNext;
                        if (msk.pstOutlineAction != null)
                            msk.kState = KMskState.kMatchArgs;
                        else if (msk.pstOutlineAction == null)
                        {
                            // outline is completely matched
                            msk = msk.mskPop();
                            // TODO capture match info
                        }
                        else
                        {
                            // this pattern branch is matched. go to pending branch, or full pattern matched
                            // TODO gather match info from this branch
                            // TODO where to accept pattern (don't proceed to next, instead pop from step)
                            msk = msk.mskPop();
                        }
                        break;
                    case KMskState.kStartStep:
                        // start processing a pair of steps from pattern and outline
                        if (msk.psbPatternStep is Mpr)
                        {
                            // set state to accept match when all branches have completed
                            msk.kState = KMskState.kAcceptPattern;

                            // push msk to process each possible pattern
                            Mpr mipPattern = (Mpr)msk.psbPatternStep;
                            msk = msk.mskPush();

                            msk.kState = KMskState.kNextPattern;
                            // get first alternative pattern for Min
                            Mtg mtgAllowed = mipPattern.mtgAllowed;
                            msk.mgpCurrent = mtgAllowed.mgpFirstMpt;
                            Mpi mpiAbove = msk.mfrMatchInfo.mpi;
                            Mfr.Push(msk);
                            msk.mfrMatchInfo.mpi = new Mpi(msk.mgpCurrent.mpt, mpiAbove);
                            msk.mfrMatchInfo.mpi.mprPatternRef = mipPattern;
                        }
                        else if (msk.psbPatternStep is Tst)
                        {
                            msk.kState = KMskState.kMatchArgs;
                            Tst tstPatternPlace = (Tst)msk.psbPatternStep;
                            msk.pstPatternAction = tstPatternPlace.pobHead;
                            msk.pstOutlineAction = msk.psbOutlinePlace.pobHead;
                        }
                        else
                            throw new System.Exception();
                        break;
                    case KMskState.kNextPattern:
                        // advance if prev pattern failed, or accept pattern when all branches done
                        if (msk.mgpCurrent != null)
                        {
                            // start processing next pattern for Min
                            msk.kState = KMskState.kStartStep;
                            msk.psbPatternStep = msk.mgpCurrent.mpt.psbFirst;
                            if (msk.psbPatternStep == null)
                                throw new InvalidProgramException("pattern missing first step");
                            msk.mgpCurrent = msk.mgpCurrent.mgpNextSameMtg;
                        }
                        else
                        {
                            // no more choices for this Min, so its parent pattern fails
                            msk = msk.mskFail(KMskState.kNextPattern); 
                        }
                        break;
                    case KMskState.kAcceptPattern:
                        // Min is accepted, pop and resume previous state
                        Mpi mpiChild = msk.mfrMatchInfo.mpi;
                        msk = msk.mskPop();
                        Mpi mpiParent = msk.mfrMatchInfo.mpi;
                        mpiParent.AddChild(mpiChild);
                        break;
                    default:
                        throw new InvalidProgramException();
                }

                     // TODO: how to collect the match info?
                    // Mpi mpiChild = new Mdi(mptPattern, msk.mfrMatchInfo.mbn, msk.mfrMatchInfo.mns);
            }
        }
    }
}
