using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleApplication1;
using Newtonsoft.Json.Linq;

namespace GraphMatching
{



    public class VertexMatch : Base
    {
        public Vertex vPattern { get; private set; }
        public Vertex[] vaData { get; private set; }

        public VertexMatch(Vertex vPattern, Vertex vData)
        {
            this.vPattern = vPattern;
            vaData = new Vertex[1];
            vaData[0] = vData;
        }

        public VertexMatch(Vertex vPattern, Vertex[] vaData)
        {
            this.vPattern = vPattern;
            this.vaData = vaData;
        }

        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddToJObject(obj, "vPattern", vPattern);
                AddListToJObject(obj, "vaData", vaData);
            }
        }

        public bool Matches(VertexMatch vmRequirement)
        {
            if (vPattern != vmRequirement.vPattern)
                return false;
            if (vaData.Count() != vmRequirement.vaData.Count())
                return false;
            foreach (Vertex v in vaData)
            {
                bool found = false;
                foreach (Vertex vReq in vmRequirement.vaData)
                {
                    if (v == vReq)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            return true;
        }


        public override void Format(PrettyFormatter sb)
        {
            base.Format(sb);
            sb.FieldString("pat", vPattern.Name);
            sb.FieldNameList("data:", vaData);
        }
    }

    public class EdgeMatch : Base
    {
        public Edge ePattern { get; private set; }
        public Edge[] eaData { get; private set; }

        public EdgeMatch(Edge ePattern, Edge eData)
        {
            this.ePattern = ePattern;
            eaData = new Edge[1];
            eaData[0] = eData;
        }

        public EdgeMatch(Edge ePattern, Edge[] eaData)
        {
            this.ePattern = ePattern;
            this.eaData = eaData;
        }
        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddToJObject(obj, "ePattern", ePattern);
                AddListToJObject(obj, "eaData", eaData);
            }
        }
        public bool Matches(EdgeMatch emRequirement)
        {
            if (!ePattern.Matches(emRequirement.ePattern))
                return false;
            if (eaData.Count() != emRequirement.eaData.Count())
                return false;
            foreach (Edge e in eaData)
            {
                bool found = false;
                foreach (Edge eReq in emRequirement.eaData)
                {
                    if (e.Matches(eReq))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            return true;
        }

        public override void Format(PrettyFormatter sb)
        {
            base.Format(sb);
            sb.FieldValue("pat", ePattern);
            sb.FieldList("data", eaData);
        }
    }

    public class GraphMatch : Base
    {
        public VertexMatch[] vmaVertices { get; private set; }
        public EdgeMatch[] emaEdges { get; private set; }

        public GraphMatch(VertexMatch[] vmaVertices, EdgeMatch[] emaEdges)
        {
            this.vmaVertices = vmaVertices;
            this.emaEdges = emaEdges;
        }

        public Vertex GetDataVertex(Vertex vPattern)
        {
            foreach (VertexMatch vm in vmaVertices)
            {
                if (vm.vPattern == vPattern)
                    return vm.vaData[0];
            }
            return null;
        }
        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddListToJObject(obj, "vmaVertices", vmaVertices);
                AddListToJObject(obj, "emaEdges", emaEdges);
            }
        }

        public bool Matches(GraphMatch gmRequirement)
        {
            if (vmaVertices.Count() != gmRequirement.vmaVertices.Count())
                return false;
            if (emaEdges.Count() != gmRequirement.emaEdges.Count())
                return false;
            foreach (VertexMatch vm in vmaVertices)
            {
                bool found = false;
                foreach (VertexMatch vmReq in gmRequirement.vmaVertices)
                {
                    if (vm.Matches(vmReq))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            foreach (EdgeMatch em in emaEdges)
            {
                bool found = false;
                foreach (EdgeMatch emReq in gmRequirement.emaEdges)
                {
                    if (em.Matches(emReq))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            return true;
        }
        public override void AddToJReference(JObject vobj)
        {
            vobj.Add(LogEntry.LOG_REF_TEXT, "numVertices=" + vmaVertices.Length + " numEdges=" + emaEdges.Length);
        }

        public override void Format(PrettyFormatter sb)
        {
            foreach (var vm in vmaVertices)
            {
                vm.FormatSubobject(sb);
            }
            foreach (var em in emaEdges)
            {
                em.FormatSubobject(sb);
            }
        }
    }

    /// <summary>
    /// Compare two lists of GraphMatch to see if they are the same.
    /// </summary>
    public class GraphMatchListComparison : Base
    {
        public List<GraphMatch> Lhs { get; private set; }
        public string LhsName { get; private set; }
        public List<GraphMatch> Rhs { get; private set; }
        public string RhsName { get; private set; }
        public bool Same { get; private set; }
        public string Reason { get; private set; }

        public GraphMatchListComparison(string lhsName, List<GraphMatch> lhs, string rhsName, List<GraphMatch> rhs)
        {
            this.LhsName = lhsName;
            this.Lhs = lhs;
            this.RhsName = rhsName;
            this.Rhs = new List<GraphMatch>(rhs);
            Compare();
        }
        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddListToJObject(obj, "Lhs", Lhs);
                AddListToJObject(obj, "Rhs", Rhs);
                AddToJObject(obj, "LhsName", LhsName);
                AddToJObject(obj, "RhsName", RhsName);
                AddToJObject(obj, "Same", Same);
                AddToJObject(obj, "Reason", Reason);
            }
        }

        private void Compare()
        {
            Reason = "";
            Same = true;

            foreach (GraphMatch gmLhs in Lhs)
            {
                GraphMatch match = null;
                foreach (GraphMatch gmRhs in Rhs)
                {
                    if (gmLhs.Matches(gmRhs))
                    {
                        match = gmRhs;
                        break;
                    }
                }
                if (match != null)
                {
                    Rhs.Remove(match);
                }
                else
                {
                    Same = false;
                    Reason += "could not match " + LhsName + " " + gmLhs + "\n";
                }
            }
            foreach (GraphMatch gmRhs in Rhs)
            {
                Same = false;
                Reason += "could not match " + RhsName + " " + gmRhs + "\n";
            }
        }
    }

    /// <summary>
    /// Base class for steps to perform when doing graph matching.
    /// </summary>
    public abstract class MatchTask : Base
    {
        public PatternFinderBase MPatternFinder { get; private set; }
        protected TraceNode trace;

        protected MatchTask(PatternFinderBase patternFinder, TraceNode trace)
        {
            MPatternFinder = patternFinder;
            this.trace = trace;
            // this.trace = trace.MakeChild(this);
        }

        public abstract void Perform();

        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            AddToJObject(obj, "MPatternFinder", MPatternFinder);
        }
    }

    public class PatternEdgeInfo
    {
        public const int NOT_YET = -1;

        public Edge E;
        public int iTail = NOT_YET;   // index into array of vaPattern where vertex at tail was first seen, or NOT_YET
        public int iHead = NOT_YET;   // index into array of vaPattern where vertex at head was first seen, or NOT_YET

        public PatternEdgeInfo(Edge e)
        {
            this.E = e;
        }

        override public string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("<");
            sb.Append(GetType().Name);
            sb.Append(" ");
            sb.Append(E.ToString());
            sb.Append(" ");
            sb.Append(iTail == NOT_YET ? "X" : iTail.ToString());
            sb.Append("~");
            sb.Append(iHead == NOT_YET ? "X" : iHead.ToString());
            sb.Append(">");
            return sb.ToString();
        }
    }


    /// <summary>
    /// Cache info needed to match data edges to pattern edges. Every pattern has one of these for each edge in it.
    /// Instances of this class are indexed eHighest. The cached data is a sequence of the remaining pattern edges
    /// that can be used to iterate through corresponding data edges, if any.
    /// 
    /// requirements for unit tests:
    ///   for each EdgeMatchInfo, except first
    ///      either tail or head or both matches a earlier vertex
    ///      where the iTail/iHead matches earlier, the vertex on the edge is same as earlier vertex
    ///   vertex idx goes up by 1 if one vertex is new, up by 0 if neither is new (2 for first only)
    ///       the new vertex in vaPattern is same as the new one on the edge (if any)
    ///   dVertexIndex matches what is in vaPattern
    ///   add edges are in eaPattern
    ///   TODO: write unit tests

    /// </summary>
    public class PatternGraphInfo
    {
        public PatternFinderBase patternFinder { get; private set; }
        public Graph gPattern { get; private set; }
        public PatternGraphInfo Next { get; private set; }

        public PatternEdgeInfo[] eaPattern { get; private set; }  // all edges starting with eHighest, in an order where each edge is connected to an earlier vertex
        public Vertex[] vaPattern { get; private set; }  // each pattern vertex appears in this list, starting with vertices for eHighest,
        // in order described by eaPattern

        public Edge eStart { get; private set; }
        public int iNumVertices { get; private set; }   // next vacant place in vaPattern
        // private Dictionary<Vertex, int> dVertexIndex = new Dictionary<Vertex, int>(); // value is position of vertex in vaPattern
        private int ieUnprocessed;  // next edge in eaPattern to visit to connect to other edges
        private int ieUnseen;  // first edge that has not been connected to an earlier vertex yet

        /// <summary>
        /// Move up the edge from where it is, i, to ieUnseen, then advance ieUnseen
        /// </summary>
        /// <param name="isHead"></param>
        /// <param name="iEdge"></param>
        /// <param name="v"></param>
        private void VisitEdge(bool isHead, Edge e, int iEdge, Vertex v)
        {
            PatternEdgeInfo em = eaPattern[iEdge];
            if (iEdge >= ieUnseen + 1)
            {
                // swap the eaPattern at i and ieUnseen
                PatternEdgeInfo w = eaPattern[ieUnseen];
                eaPattern[ieUnseen] = em;
                eaPattern[iEdge] = w;
            }
            if (iEdge >= ieUnseen)  // false if closing a loop or if iEdge happens to be = ieUnseen
                ieUnseen++;
            if (isHead)
                em.iHead = iNumVertices;
            else
                em.iTail = iNumVertices;
        }

        private void VisitVertex(Vertex v)
        {
            // if (dVertexIndex.ContainsKey(v)) caller assures this is false
            //    return;
            vaPattern[iNumVertices] = v;
            // dVertexIndex[v] = ivDone;
            // Look for all edges that should refer to this vertex as having been seen.
            // This is needed so that later it won't try to use a different data vertex to match another edge with same pattern vertex
            // the inner loop makes this n^2, but its simpler than allocating a new structure to iterate over for topo sort
            // Start from ieUnprocessed instead of ieUnseen to find possible closure of loop.
            for (int i = ieUnprocessed; i < gPattern.NumEdges; i++)
            {
                Edge e = eaPattern[i].E;
                if (e.vTail == v)
                    VisitEdge(false, e, i, v);
                else if (e.vHead == v)
                    VisitEdge(true, e, i, v);
            }
            iNumVertices++;
        }

        public PatternGraphInfo(PatternFinderBase patternFinder, Edge eStart, PatternGraphInfo prev)
        {
            this.patternFinder = patternFinder;
            this.gPattern = patternFinder.gPattern;
            this.eStart = eStart;
            this.Next = prev;

            // build arrays of edges of and vertices in order that they should be matched.
            // Edges to be listed in an order such that either tail or head vertex was mentioned earlier.
            eaPattern = new PatternEdgeInfo[gPattern.NumEdges];
            iNumVertices = 0;
            vaPattern = new Vertex[gPattern.NumEdges * 2]; // gPattern.NumVertices];
            eaPattern[0] = new PatternEdgeInfo(eStart);
            // put the other edges into eaPattern and later topo sort them in place
            int i = 1;
            for (EdgeLink el = gPattern.elFirst; el != null; el = el.next)
            {
                if (el.e != eStart)
                    eaPattern[i++] = new PatternEdgeInfo(el.e);
            }
            ieUnprocessed = 0;
            ieUnseen = 1;
            while (ieUnprocessed < gPattern.NumEdges)
            {
                PatternEdgeInfo ei = eaPattern[ieUnprocessed++];
                Edge e = ei.E;
                if (ei.iTail == PatternEdgeInfo.NOT_YET)
                    VisitVertex(e.vTail);
                if (ei.iHead == PatternEdgeInfo.NOT_YET)
                    VisitVertex(e.vHead);
            }
        }
    }

    /// <summary>
    /// Task that iterates through the data edges to see if an edge of the pattern template matches there.
    /// There is one insteance of this class for each pattern.
    /// Need to iterate over edges so that when edges are added to the data graph, can just resume.
    /// A corr can add a edge between two previously matched vertices, 
    /// but a coor cannot add vertex without also adding edges.
    /// </summary>
    public class IterateAllDataEdgesTask : MatchTask
    {
        private EdgeLink elData = null;  // where in the list of data edges we visited last

        private PatternGraphInfo eilFirstEdgeInfo;   // list for the pattern from patternFinder
        private PatternGraphInfo eilCurrentEdgeInfo = null;  // current place in iteration for this data edge
        private Restrictor contextPattern;
        private Restrictor contextData;

        public IterateAllDataEdgesTask(PatternFinderBase patternFinder, Restrictor contextPattern, Restrictor contextData, TraceNode trace)
            : base(patternFinder, trace)
        {
            this.contextPattern = contextPattern;
            this.contextData = contextData;
            // precompute edge sequences to match. One for each edge in the pattern, so each new data edge
            // can be matched to each edge in the pattern.
            eilFirstEdgeInfo = null;
            for (EdgeLink el = patternFinder.gPattern.elFirst; el != null; el = el.next)
            {
                eilFirstEdgeInfo = new PatternGraphInfo(patternFinder, el.e, eilFirstEdgeInfo);
            }
        }

        public override void Format(PrettyFormatter sb)
        {
            base.Format(sb);
            if (elData != null)
            {
                sb.Push();
                sb.Identifier("elData");
                sb.Colon(":");
                sb.Push();
                elData.e.FormatSubobject(sb);
                sb.Pop();
                sb.Pop();
            }
            if (eilCurrentEdgeInfo != null)
            {
                sb.Push();
                sb.Identifier("eilCurrentEdgeInfo");
                sb.Colon(":");
                sb.Push();
                eilCurrentEdgeInfo.eStart.FormatSubobject(sb);
                sb.Pop();
                sb.Pop();
            }
        }

        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddToJObject(obj, "elData", elData);
            }
        }

        public bool Ready()
        {
            if (eilCurrentEdgeInfo != null)
            {
                if (eilCurrentEdgeInfo.Next != null)
                    return true;
            }
            if (elData == null)
                return MPatternFinder.gData.elFirst != null;
            return (elData.next != null);
        }

        public override void Perform()
        {
            // Each call resumes the iteration through vlData.
            bool more = true;
            while (more)
            {
                if (elData == null)
                {
                    elData = MPatternFinder.gData.elFirst;
                    if (elData == null)
                        return;
                    eilCurrentEdgeInfo = eilFirstEdgeInfo;
                }
                else if (eilCurrentEdgeInfo.Next == null)
                {
                    if (elData.next == null)
                        return;
                    elData = elData.next;
                    eilCurrentEdgeInfo = eilFirstEdgeInfo;
                }
                else
                    eilCurrentEdgeInfo = eilCurrentEdgeInfo.Next;

                if (elData.e.Id == 373)
                    Console.WriteLine("trap elData.e.Id");

                var subtask = new IterateEdgeMatchesTask(elData.e, eilCurrentEdgeInfo, contextPattern, contextData, trace);
                MPatternFinder.AddTaskFirst(this);  // requeue to get next edge
                MPatternFinder.AddTaskFirst(subtask);
                break;
                /* OLD
                Edge ePattern = elPattern.e;

                // Iterate first over all edges in the pattern to match the current data edge.
                // Then we will iterate over permutations of the remaining edges in the pattern, and
                // match each one to a data edge, according to the connectivity of corresponding vertices.
                var subtask = new PatternEdgeIteratorTask(MPatternFinder, MPatternFinder.gPattern, MPatternFinder.gData, ePattern, eData, trace);
                try
                {
                    if (subtask.IsPossible())
                    {
                        MPatternFinder.AddTaskFirst(subtask);
                        break;
                    }
                }
                finally
                {
                }
                 * */
            }
        }
    }

    /// <summary>
    /// Hold inforrmation used when iterating through data edges during a match.
    /// The is one of these per pattern edge to iterate through the edges on the matched data vertex.
    /// </summary>
    public class DataMatchInfo
    {
        public Edge eData { get; set; }
        public IEnumerator<Edge> ieData { get; set; }
        public int iMatchedVertices;      // num valid entries in vaData

        public DataMatchInfo()
        {
        }
    }

    /// <summary>
    /// Iterate through all matches where eData is the highest data edge and other data edges match in the order
    /// described pmiPattern.
    /// </summary>
    public class IterateEdgeMatchesTask : MatchTask
    {
        public Edge eHighestData { get; private set; }
        public PatternGraphInfo pmiPattern { get; private set; }
        private Restrictor contextPattern;
        private Restrictor contextData;

        private Vertex[] vaData;  // data vertices that correspond to pmiPattern.vaPattern
        private DataMatchInfo[] dmiaData;  // data edge info that correponds to pmiPattern.eaPattern

        private int iEdgePosn; // state machine for iteration: which data edge index to iterate next
        private int iNumEdges;
        private int iNumVertices;

        public IterateEdgeMatchesTask(Edge eHighestData, PatternGraphInfo pmiPattern,
                                      Restrictor contextPattern, Restrictor contextData, TraceNode ptrace)
            : base(pmiPattern.patternFinder, ptrace)
        {
            this.eHighestData = eHighestData;
            this.pmiPattern = pmiPattern;
            this.contextPattern = contextPattern;
            this.contextData = contextData;
            iNumEdges = pmiPattern.eaPattern.Length;
            iNumVertices = pmiPattern.iNumVertices;
            vaData = new Vertex[iNumVertices];
            dmiaData = new DataMatchInfo[iNumEdges];
            for (int i = 0; i < iNumEdges; i++)
                dmiaData[i] = new DataMatchInfo();
            dmiaData[0].eData = eHighestData;
            vaData[0] = eHighestData.vTail;
            vaData[1] = eHighestData.vHead;
            iEdgePosn = 1; // start iterating from where pattern edge 1 is attached to edge 0
            dmiaData[0].iMatchedVertices = 2;
        }

        enum ActionType
        {
            first,
            back,
            next
        }

        public override void Perform()
        {
            Edge eHighestPattern = pmiPattern.eaPattern[0].E;
            if (eHighestData.Desc != null && !eHighestData.Desc.Satisfies(eHighestPattern.Desc))
                return;
            if (!vaData[0].Satisfies(eHighestPattern.vTail))
                return;
            if (!vaData[1].Satisfies(eHighestPattern.vHead))
                return;
            ActionType action = (iEdgePosn < iNumEdges) ? ActionType.first : ActionType.back;
            while (true)
            {
                bool needEdge = true;
                switch (action)
                {
                    case ActionType.first:
                        {
                            // Go forward and find edge or edge enumerator
                            DataMatchInfo dmiDataF = dmiaData[iEdgePosn];
                            PatternEdgeInfo eiPatternF = pmiPattern.eaPattern[iEdgePosn];
                            Edge ePattern = eiPatternF.E;
                            if (eiPatternF.iTail == PatternEdgeInfo.NOT_YET)
                            {
                                Vertex vDataHead = vaData[eiPatternF.iHead];
                                dmiDataF.ieData = vDataHead.GetEnumerator(DirectionType.Incoming, contextData, ePattern.Desc, ePattern.vTail);
                            }
                            else if (eiPatternF.iHead == PatternEdgeInfo.NOT_YET)
                            {
                                Vertex vDataTail = vaData[eiPatternF.iTail];
                                dmiDataF.ieData = vDataTail.GetEnumerator(DirectionType.Outgoing, contextData, ePattern.Desc, ePattern.vHead);
                            }
                            else
                            {
                                // no iteration: just find the data edge that matches the vertices
                                dmiDataF.ieData = null;
                                Vertex vDataHead = vaData[eiPatternF.iHead];
                                Vertex vDataTail = vaData[eiPatternF.iTail];
                                // vertices have already been checked for Satisfies
                                Edge eDataF = vDataTail.GetEdge(DirectionType.Outgoing, contextData, eiPatternF.E.Desc, vDataHead);
                                if (eDataF == null)
                                {
                                    action = ActionType.back;
                                    continue;
                                }
                                else if (eDataF.Id >= eHighestData.Id)
                                {
                                    action = ActionType.back;
                                    continue;
                                }
                                else
                                {
                                    dmiDataF.eData = eDataF;
                                    needEdge = false;
                                }
                            }
                            break;
                        }
                    case ActionType.back:
                        {
                            // back up to previous enumerator
                            iEdgePosn--;
                            if (iEdgePosn <= 0)
                            {
                                if (iNumEdges == 1)   // wierd edge case
                                    FoundMatch();
                                return;
                            }
                            break;
                        }
                    case ActionType.next:
                        break;
                }
                if (needEdge)
                {
                    // get next edge from enumerator
                    DataMatchInfo dmiData = dmiaData[iEdgePosn];
                    PatternEdgeInfo eiPattern = pmiPattern.eaPattern[iEdgePosn];
                    if (dmiData.ieData == null || !dmiData.ieData.MoveNext())
                    {
                        action = ActionType.back;
                        continue;
                    }
                    Edge eData = dmiData.ieData.Current;
                    if (eData.Id >= eHighestData.Id)
                    {
                        action = ActionType.next;
                        continue;
                    }
                    // check if the vertex visited by this edge has already been matched here
                    Vertex vOther = (eiPattern.iTail == PatternEdgeInfo.NOT_YET) ? eData.vTail : eData.vHead;
                    bool found = false;
                    int numv = dmiaData[iEdgePosn - 1].iMatchedVertices;
                    for (int i = 0; i < numv; i++)
                    {
                        if (vaData[i] == vOther)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        action = ActionType.next;
                        continue;
                    }
                    // don't worry about duplicate edge. that can only happen if there are duplicate vertices also.
                    vaData[numv] = vOther;
                    dmiaData[iEdgePosn].iMatchedVertices = 1 + numv;
                    dmiData.eData = eData;
                }
                if (iEdgePosn + 1 == iNumEdges)
                {
                    FoundMatch();
                    action = ActionType.next;
                }
                else
                {
                    iEdgePosn++;
                    action = ActionType.first;
                }
            }
        }

        private void FoundMatch()
        {
            var vmaVertices = new VertexMatch[iNumVertices];
            for (int i = 0; i < iNumVertices; i++)
            {
                vmaVertices[i] = new VertexMatch(pmiPattern.vaPattern[i], vaData[i]);
            }
            var emaEdges = new EdgeMatch[iNumEdges];
            for (int i = 0; i < iNumEdges; i++)
            {
                emaEdges[i] = new EdgeMatch(pmiPattern.eaPattern[i].E, dmiaData[i].eData);
            }
            var gm = new GraphMatch(vmaVertices, emaEdges);

            pmiPattern.patternFinder.FoundMatch(gm);
        }
    }


    public enum DirectionType
    {
        Incoming,
        Outgoing
    }

    /// <summary>
    /// Object that holds to top level information for finding instances of a pattern in a data graph.
    /// </summary>
    public abstract class PatternFinderBase : Base
    {
        public Graph gPattern { get; private set; }
        public Graph gData { get; private set; }
        private List<MatchTask> mtlQueue = new List<MatchTask>();
        protected TraceNode trace;
        private IterateAllDataEdgesTask iteratorTask;

        public abstract void FoundMatch(GraphMatch match);

        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            AddToJObject(obj, "gPattern", gPattern);
            AddToJObject(obj, "gData", gData);
            AddListToJObject(obj, "mtlQueue", mtlQueue);
        }

        public PatternFinderBase(Graph pattern, Graph data, TraceNode trace)
        {
            gPattern = pattern;
            gData = data;
            this.trace = trace;
            // this.trace = trace.MakeChild(this);
        }

        public void AddTaskFirst(MatchTask task)
        {
            mtlQueue.Insert(0, task);
        }

        public void Start()
        {
            // start the queue by setting up task to scan all the vertices in the data
            iteratorTask = new IterateAllDataEdgesTask(this, gPattern.context, gData.context, trace);
        }

        public bool Ready()
        {
            // The iteratorTask is handled specially so it can be restarted easily after the
            // graph that it is iterating over is extended.
            return mtlQueue.Any() || iteratorTask.Ready();
        }

        public void Step()
        {
            if (mtlQueue.Any())
            {
                MatchTask task = mtlQueue[0];
                mtlQueue.RemoveAt(0);
                task.Perform();
            }
            else
            {
                iteratorTask.Perform();
            }
        }
    }

    /// <summary>
    /// Build a list of all matches found by PatternFinderBase.
    /// </summary>
    public class PatternFinder : PatternFinderBase
    {
        public List<GraphMatch> gmlMatches { get; private set; }

        public PatternFinder(Graph pattern, Graph data, TraceNode trace)
            : base(pattern, data, trace)
        {
            gmlMatches = new List<GraphMatch>();
        }
        public override void FoundMatch(GraphMatch match)
        {
            gmlMatches.Add(match);
        }

        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddListToJObject(obj, "gmlMatches", gmlMatches);
            }
        }

        /// <summary>
        /// Search all data for an instance of pattern.
        /// </summary>
        /// <returns></returns>
        public List<GraphMatch> Match()
        {
            if (gPattern == null)
            {
                throw new ArgumentError("no pattern");
            }
            if (gPattern.vlFirst == null)
            {
                throw new ArgumentError("no vertices in pattern");
            }
            if (gPattern.elFirst == null)
            {
                throw new ArgumentError("no edges in pattern");
            }
            if (gData == null)
            {
                throw new ArgumentError("no data to be matched");
            }

            Start();

            // main loop the processes at tasks that are placed in the queue
            while (Ready())
            {
                Step();
            }

            return gmlMatches;
        }
    }

    /// <summary>
    /// Helper to find out why pattern does not match expected vertex
    /// </summary>
    public class PatternFinderSingle : PatternFinder
    {
        protected PatternFinderSingle(Graph pattern, Graph data, TraceNode trace)
            : base(pattern, data, trace)
        {
        }
        public static readonly Restrictor cSingleRestrictor = new Restrictor("single", null);

        public static PatternFinderSingle make(Graph pattern, Vertex expected, TraceNode trace)
        {
            Graph data = new Graph(cSingleRestrictor);
            data.Add(expected);
            return new PatternFinderSingle(pattern, data, trace);
        }
    }

    public class PatternFinderCorr : PatternFinderBase
    {
        private Correspondence corr;
        private CorrespondenceSetMatcher setMatcher;

        public PatternFinderCorr(Correspondence c, Graph pattern, Graph data, CorrespondenceSetMatcher setMatcher,
                                 TraceNode trace)
            : base(pattern, data, trace)
        {
            this.corr = c;
            this.setMatcher = setMatcher;
        }
        public override void FoundMatch(GraphMatch match)
        {
            setMatcher.AddMatch(corr, match);
            trace.Log("corrMatch").A("corr", corr).A("match", match).A("gData", gData);
        }
        public override void Format(PrettyFormatter sb)
        {
            base.Format(sb);
            sb.FieldString("corr", corr.Name);
        }

        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddToJObject(obj, "corr", corr);
                AddToJObject(obj, "setMatcher", setMatcher);
            }
        }
    }


    public class CorrespondenceMatchHandler
    {
        private SymbolTable stSubst;
        private Correspondence.ReplacementDirectionType direction;
        private Restrictor inputContext;
        private Restrictor outputContext;
        private Graph gOutput;

        public CorrespondenceMatchHandler(SymbolTable stSubst, Correspondence.ReplacementDirectionType direction,
                        Restrictor inputContext, Restrictor outputContext, Graph gOutput)
        {
            this.stSubst = stSubst;
            this.direction = direction;
            this.inputContext = inputContext;
            this.outputContext = outputContext;
            this.gOutput = gOutput;
        }

        public void Matched(Correspondence corr, GraphMatch match)
        {
            corr.Perform(stSubst, match, direction, inputContext, outputContext, gOutput);
        }
    }

    /// <summary>
    /// Take an input graph and a collection of Correspondences and apply the correspondences.
    /// </summary>
    public class CorrespondenceSetMatcher : Base
    {
        public List<PatternFinderCorr> pflMatchers { get; private set; }
        public Graph gData { get; private set; }
        public Correspondence.ReplacementDirectionType Direction { get; private set; }
        // private TraceNode trace;
        private CorrespondenceMatchHandler matchHandler;

        public void AddMatch(Correspondence corr, GraphMatch match)
        {
            matchHandler.Matched(corr, match);
        }
        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            AddListToJObject(obj, "cmlMatchers", pflMatchers);
            AddToJObject(obj, "gData", gData);
            AddToJObject(obj, "Direction", Direction);
        }

        public CorrespondenceSetMatcher(Graph data, SymbolTable stCorrespondences, Correspondence.ReplacementDirectionType direction,
                                        CorrespondenceMatchHandler matchHandler, TraceNode trace)
        {
            gData = data;
            this.Direction = direction;
            this.matchHandler = matchHandler;
            // this.trace = trace.MakeChild (this);
            pflMatchers = new List<PatternFinderCorr>();
            foreach (var entry in stCorrespondences.Contents)
            {
                if (entry is Correspondence)
                {
                    var c = entry as Correspondence;
                    var gPattern = (direction == Correspondence.ReplacementDirectionType.Forward) ? c.gFirst : c.gSecond;
                    var patternFinder = new PatternFinderCorr(c, gPattern, gData, this, trace);
                    pflMatchers.Add(patternFinder);
                }
            }
        }

        public void Match()
        {
            if (pflMatchers.Count == 0)
            {
                throw new ArgumentError("no correspondences");
            }
            if (gData == null)
            {
                throw new ArgumentError("no data to be matched");
            }
            if (gData.vlFirst == null)
            {
                throw new ArgumentError("no vertices in data");
            }

            // Iteration: each in list of CorrespondenceMatcher needs to be stepped.
            foreach (var cm in pflMatchers)
            {
                cm.Start();
            }
            while (true)
            {
                bool done = true;
                foreach (var pf in pflMatchers)
                {
                    if (pf.Ready())
                    {
                        done = false;
                        // trace.Log("cmStep").A("cm", cm);
                        pf.Step();
                    }
                }
                if (done)
                    break;
            }
        }
    }

    public class Session : Base
    {
        private static Dictionary<Graph, EdgeLink> latestEdgeSent = new Dictionary<Graph, EdgeLink>();

        public Session()
            : base()
        {

        }
    }

    /*
     * display state at each iteration
  contents of queue
  match tree: show match, increase indent, then call on lower level
*/
#if false
    class Program
    {
        static void OldMain(string[] args)
        {
            /*
            var gmt = new ConsoleApplication1.test.GraphMatcher();
            gmt.Init();
            gmt.V2E1();
            */

            /*
            var ex = new ConsoleApplication1.test.Compiler();
            ex.Init();
            ex.AssignToIL();
            */

            var edSimpleRed = new EdgeDescriptor("", ColorDef.Red);
            var edSimpleGreen = new EdgeDescriptor("", ColorDef.Green);
            var edS = new EdgeDescriptor("", ColorDef.Red);
            var edT = new EdgeDescriptor("", ColorDef.Green);
            var sA = "a";
            var sB = "b";

            var dict = new SymbolTable(null);
            dict.Add("edSimpleRed", edSimpleRed);
            dict.Add("edSimpleGreen", edSimpleGreen);
            dict.Add("edS", edS);
            dict.Add("edT", edT);
            dict.Add("sA", sA);
            dict.Add("sB", sB);

            var ee = new ExpressionEvaluator();
            //Graph gP = ee.EvaluateGraph("a@sA, b@, a~b^edSimpleRed", dict);
            //Graph gD = ee.EvaluateGraph("c@sA, d@, c~d^edSimpleRed", dict);
            Graph gP = ee.EvaluateGraph("R@, F@Name1, S@Name2, R~F^edS, F~S^edS", dict);
            string data = "listRoot@, treeRoot@, a@\"this is a\", b@\"this is b\", listRoot~treeRoot^edT, listRoot~a^edS, a~b^edS";
            Graph gD = ee.EvaluateGraph(data, dict);

            var m = new PatternFinder(gP, gD, TraceNode.full);
            List<GraphMatch> matches = m.Match();
            Console.WriteLine("Matches");
            foreach (GraphMatch gm in matches)
            {
                Console.WriteLine(" " + gm);
            }

            // List<GraphMatch> gmlExpected = ee.EvaluateGraphMatchList("{ a{c}, b{d}, a~b{c~d} }", dict);
            List<GraphMatch> gmlExpected = ee.EvaluateGraphMatchList("{ R{listRoot}, F{a}, S{b}, R~F{listRoot~a}, F~S{a~b} }", dict);
            var gmlc = new GraphMatchListComparison("actual", matches, "expected", gmlExpected);
            if (!gmlc.Same)
            {
                Console.WriteLine("not what was expected: " + gmlc.Reason);
            }
            else
            {
                Console.WriteLine("matched expected");
            }
            Console.ReadLine();
        }


        public static int _tests;
        public static int _passed;
        public static int _failed;

        public static void Test<T>()
        {
            TestResults results = Tester.RunTestsInClass<T>();

            _tests += results.NumberOfResults;
            _passed += results.NumberOfPasses;
            _failed += results.NumberOfFails;

            HandleResults<T>(results);

        }

        static void HandleResults<T>(TestResults results)
        {
            if (results.NumberOfFails == 0)
            {
                Console.WriteLine("{0}: PASSED", typeof(T));
            }
            DocumentFails(ref results);
        }

        static void DocumentFails(ref TestResults results)
        {
            if (results.NumberOfFails > 0)
            {
                Console.WriteLine("---------------------------------");

                foreach (TestResult result in results)
                {
                    if (result.Result == TestResult.Outcome.Pass)
                        continue;
                    Console.WriteLine("Test {0}: {1} {2}", result.MethodName, result.Result, result.Result == TestResult.Outcome.Fail ? "\r\n" + result.Message : "");
                }
                Console.WriteLine("---------------------------------");
            }

            Console.WriteLine();
        }

    }

}
#endif
}