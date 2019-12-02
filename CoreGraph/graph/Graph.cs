using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GraphMatching
{

    /// <summary>
    /// Building block of a graph. Can represent trait/field on a Node or fields in a structure
    /// </summary>
    public class Edge : Base
    {
        private readonly Vertex _vHead;
        private readonly Vertex _vTail;
        private readonly Restrictor _context;
        private readonly EdgeDescriptor _desc;

        public Vertex vHead { get { return _vHead; } }
        public Vertex vTail { get { return _vTail; } }
        public Restrictor context { get { return _context; } }
        public EdgeDescriptor Desc { get { return _desc; } }

        public bool EdgeSatisfies(EdgeDescriptor edRequirement)
        {
            if (edRequirement == null)
                return Desc == null;
            if (Desc == null)
                return false;
            return Desc.Satisfies(edRequirement);
        }

        public Edge(EdgeDescriptor desc, Vertex vTail, Vertex vHead, Restrictor context)
        {
            _desc = desc;
            _vTail = vTail;
            _vHead = vHead;
            _context = context;
            vHead.AddIncoming(this);
            vTail.AddOutgoing(this);
        }

        public bool Matches(Edge eRequirement)
        {
            if (vTail != eRequirement.vTail)
                return false;
            if (vHead != eRequirement.vHead)
                return false;
            if (Desc == eRequirement.Desc)
                return true;
            if (eRequirement.Desc != null)  // TODO add more checking of descriptor
            {
                if (Desc != eRequirement.Desc)
                    return false;
            }
            return true;
        }

        public Vertex getRemoteVertex(DirectionType direction)
        {
            return direction == DirectionType.Incoming ? vTail : vHead;
        }

        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
                var nodes = new JArray();
                var edges = new JArray();
                var nodeTable = new List<Vertex>();
                vTail.MakeNodeForVertex(nodeTable, nodes, obj);
                vHead.MakeNodeForVertex(nodeTable, nodes, obj);
                MakeLinkForEdge(nodes, edges, nodeTable, obj);
                obj.Add("nodes", nodes);
                obj.Add("edges", edges);
            }
            else
            {
                AddToJObject(obj, "vHead", vHead);
                AddToJObject(obj, "vTail", vTail);
                AddToJObject(obj, "Desc", Desc);
            }
        }

        public override void AddToJReference(JObject obj)
        {
            obj.Add("tailId", vTail.Id);
            obj.Add("headId", vHead.Id);
        }

        public override string GetDesc()
        {
            var sb = new StringBuilder();
            sb.Append(Id);
            sb.Append(": ");
            sb.Append(vTail.Name);
            sb.Append("->");
            sb.Append(vHead.Name);
            if (Desc != null)
            {
                sb.Append(", ");
                sb.Append(Desc.Label);
            }
            return sb.ToString();
        }

        public override void Format(PrettyFormatter sb)
        {
            sb.Identifier(Id.ToString());
            sb.Identifier(GetDesc());
        }

        public void MakeLinkForEdge(JArray nodes, JArray edges, List<Vertex> nodeTable, JsonLog obj)
        {
            var nobj = new JObject();
            vTail.FindOrCreateNodeForVertex(nodeTable, nodes, obj);
            vHead.FindOrCreateNodeForVertex(nodeTable, nodes, obj);
            nobj.Add("id", Id);
            nobj.Add("sourceId", vTail.Id);
            nobj.Add("targetId", vHead.Id);
            if (Desc == null)
                nobj.Add("color", EdgeDescriptor.DefaultEdgeColor.RGB);
            else
                Desc.SetLinkProperties(this, nobj);
            nobj.Add("nameTip", GetDesc());
            edges.Add(nobj);
        }
    }

    /// <summary>
    /// Information about an edge, used to determine which edges can match based on information about them.
    /// </summary>
    public class EdgeDescriptor : Named
    {
        private readonly ColorDef _color;  // one of several potential ways to describe/display this
        public ColorDef Color { get { return _color; } }

        public static ColorDef DefaultEdgeColor = ColorDef.Gray;
        public static ColorDef ValueEdgeColor = ColorDef.Black;

        public EdgeDescriptor(string stName, ColorDef color) : base(stName)
        {
            _color = color;
        }

        public bool Satisfies(EdgeDescriptor edRequirement)
        {
            return edRequirement == null || edRequirement == this;   // TODO allow inheritance
        }

        public string Label
        {
            get
            {
                if (Name != null && Name.Length > 0)
                    return Name;
                return Color.Name; 
            }
        }

        public override void Format(PrettyFormatter sb)
        {
            sb.Identifier(Label);
        }

        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                base.AddToJObject(obj, "Color", Color);
            }
        }

        public virtual bool SetLinkProperties(Edge e, JObject link)
        {
            try
            {
                link.Add("color", Color.RGB);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("EdgeDescriptor.SetLinkProperties caught " + ex.Message);
                // Console.Out.WriteLine(ex.StackTrace.ToString());
            }
            return true;
        }
    }

    public class ColorDef : Named
    {
        public string RGB { get; private set; }

        public ColorDef(string name, string rgb)
            : base(name)
        {
            RGB = rgb;
        }

        public static ColorDef Gray = new ColorDef("gray", "#808080");
        public static ColorDef Red = new ColorDef("red", "#800000");
        public static ColorDef Green = new ColorDef("green", "#008000");
        public static ColorDef Black = new ColorDef("black", "#000000");
        public static ColorDef Yellow = new ColorDef("green", "#808000");
        public static ColorDef Blue = new ColorDef("green", "#000080");
        public static ColorDef Purple = new ColorDef("green", "#400040");
        public static ColorDef Cyan = new ColorDef("green", "#008080");
    }

    /// <summary>
    /// A Restrictor allows a vertex to be used in different graphs, without those graphs becoming combined.
    /// Restrictors can inherit from a base Restrictor, that allows an edge to be added to a base graph but
    /// it is on visible in the derived Restriction.
    /// </summary>
    public class Restrictor : Named
    {
        private readonly Restrictor _cBase;
        public Restrictor cBase { get { return _cBase; } }

        public Restrictor(string name, Restrictor cBase)
            : base(name)
        {
            this._cBase = cBase;
        }

        public bool IsVisibleTo(Restrictor other)
        {
            Restrictor c = this;
            while (c != null)
            {
                if (c == other)
                    return true;
                c = c.cBase;
            }
            return false;
        }

        public List<Edge> FilterList(List<Edge> edges)
        {
            var r = new List<Edge>();
            foreach (var c in edges)
            {
                if (c.context.IsVisibleTo(this))
                    r.Add(c);
            }
            return r;
        }

        public List<Edge> FilterList(List<Edge> edges, EdgeDescriptor ed)
        {
            var r = new List<Edge>();
            foreach (var c in edges)
            {
                if (!c.context.IsVisibleTo(this))
                    continue;
                if (!c.Desc.Satisfies(ed))
                    continue;
                r.Add(c);
            }
            return r;
        }

        public Edge FilterOnly(List<Edge> edges, EdgeDescriptor ed)
        {
            Edge eOut = null;
            foreach (var c in edges)
            {
                if (!c.context.IsVisibleTo(this))
                    continue;
                if (!c.Desc.Satisfies(ed))
                    continue;
                if (eOut != null)
                    throw new ArgumentError("only one edge allowed");
                eOut = c;
            }
            if (eOut == null)
                throw new ArgumentError("one edge required");
            return eOut;
        }

        public Edge FilterOpt(List<Edge> edges, EdgeDescriptor ed)
        {
            Edge eOut = null;
            foreach (var c in edges)
            {
                if (!c.context.IsVisibleTo(this))
                    continue;
                if (!c.Desc.Satisfies(ed))
                    continue;
                if (eOut != null)
                    throw new ArgumentError("only one edge allowed");
                eOut = c;
            }
            return eOut;
        }

        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                base.AddToJObject(obj, "cBase", cBase);
            }
        }
    }



    /// <summary>
    /// A value represents a Node, a value, an expression that describes a value, or an object in an external structure.
    /// </summary>
    public class Vertex : Named
    {
        private readonly List<Edge> clIncoming;
        private readonly List<Edge> clOutgoing;
        public Expression Value { get; set; }

        public List<Edge> eaIncoming(Restrictor context)
        {
            return context.FilterList(clIncoming);
        }
        public Edge eaInOnly(Restrictor context, EdgeDescriptor ed)
        {
            return context.FilterOnly(clIncoming, ed);
        }
        public List<Edge> eaOutgoing(Restrictor context)
        {
            return context.FilterList(clOutgoing);
        }
        public List<Edge> eaOutgoing(Restrictor context, EdgeDescriptor ed)
        {
            return context.FilterList(clOutgoing, ed);
        }
        public Edge eaOutOnly(Restrictor context, EdgeDescriptor ed)
        {
            return context.FilterOnly(clOutgoing, ed);
        }
        public Edge eaOutOpt(Restrictor context, EdgeDescriptor ed)
        {
            return context.FilterOpt(clOutgoing, ed);
        }

        public Vertex(string name, Expression value = null)
            : base(name)
        {
            clIncoming = new List<Edge>();
            clOutgoing = new List<Edge>();
            this.Value = value;
        }

        public override void AddToJObject(JsonLog obj)
        {
            if (obj.IsDiagram())
            {
                var nodes = new JArray();
                var edges = new JArray();
                var nodeTable = new List<Vertex>();
                MakeNodeForVertex(nodeTable, nodes, obj);
                foreach (var e in clIncoming)
                {
                    e.MakeLinkForEdge(nodes, edges, nodeTable, obj);
                }
                foreach (var e in clOutgoing)
                {
                    e.MakeLinkForEdge(nodes, edges, nodeTable, obj);
                }
                obj.Add("nodes", nodes);
                obj.Add("edges", edges);
            }
            else
            {
                base.AddToJObject(obj);
                AddToJObject(obj, "Value", Value);
                AddListToJObject(obj, "clIncoming", clIncoming);
                AddListToJObject(obj, "clOutgoing", clOutgoing);
            }
        }

        public void AddIncoming(Edge e)
        {
            clIncoming.Add(e);
        }

        public void AddOutgoing(Edge e)
        {
            clOutgoing.Add(e);
        }

        public IEnumerator<Edge> GetEnumerator(DirectionType direction, Restrictor context, EdgeDescriptor ed, Vertex vPattern)
        {
            List<Edge> el = (direction == DirectionType.Outgoing) ? clOutgoing : clIncoming;
            foreach (var e in el)
            {
                if (!e.context.IsVisibleTo(context))
                    continue;
                if (e.Desc != null && !e.Desc.Satisfies(ed))
                    continue;
                Vertex vOther = (direction == DirectionType.Outgoing) ? e.vHead : e.vTail;
                if (!vOther.Satisfies(vPattern))
                    continue;
                yield return e;
            }
        }

        public Edge GetEdge(DirectionType direction, Restrictor context, EdgeDescriptor ed, Vertex vOther)
        {
            List<Edge> el = (direction == DirectionType.Outgoing) ? clOutgoing : clIncoming;
            foreach (var e in el)
            {
                if (!e.context.IsVisibleTo(context))
                    continue;
                if (e.Desc != null && !e.Desc.Satisfies(ed))
                    continue;
                Vertex v = (direction == DirectionType.Outgoing) ? e.vHead : e.vTail;
                if (v == vOther)
                    return e;
            }
            return null;
        }

        public bool Satisfies(Vertex vConstraint)
        {
            if (vConstraint.Value == null)
                return true;
            return vConstraint.Value.SatisfiedBy(Value);
        }

        /// <summary>
        /// Return true if other vertex is the same as this. Edges are defined relationally, but vertices
        /// are defined by their identity. This function is currently only used in testing.
        /// This function just helps make it clear where this semantics is used.
        /// </summary>
        /// <param name="vOther"></param>
        /// <returns></returns>
        public bool SameAs(Vertex vOther)
        {
            return this == vOther;
        }

        public override void Format(PrettyFormatter sb)
        {
            base.Format(sb);
            if (Value != null)
            {
                sb.Operator("=");
                Value.FormatSubobject(sb);
            }
            sb.Push();
            sb.Keyword("in");
            sb.Colon(":");
            sb.Push();
            foreach (var edge in clIncoming)
            {
                sb.Identifier(edge.context.Name);
                sb.Dot(".");
                sb.Identifier(edge.vTail.Name);
                if (edge.Desc != null && edge.Desc.Color != null)
                {
                    sb.Dot("^");
                    sb.Identifier(edge.Desc.Label);
                }
            }
            sb.Pop();
            sb.Pop();

            sb.Push();
            sb.Keyword("out");
            sb.Colon(":");
            sb.Push();
            foreach (var edge in clOutgoing)
            {
                sb.Identifier(edge.context.Name);
                sb.Dot(".");
                sb.Identifier(edge.vHead.Name);
                if (edge.Desc != null)
                {
                    sb.Dot("^");
                    sb.Identifier(edge.Desc.Label);
                }
            }
            sb.Pop();
            sb.Pop();
        }

        public void MakeNodeForVertex(List<Vertex> nodeTable, JArray nodes, JsonLog obj)
        {
            var nobj = new JObject();
            nobj.Add("name", Name);
            nobj.Add("id", Id);
            nobj.Add("nameTip", GetDesc());
            bool descSet = false;

            foreach (var edge in clIncoming)
            {
                var desc = edge.Desc;
                if (desc != null)
                {
                    if (obj.SetNodeProperties(this, DirectionType.Incoming, desc, nobj))
                    {
                        descSet = true;
                        break;
                    }
                }
            }

            if (!descSet)
            {
                foreach (var edge in clOutgoing)
                {
                    var desc = edge.Desc;
                    if (desc != null)
                        if (obj.SetNodeProperties(this, DirectionType.Outgoing, desc, nobj))
                        {
                            descSet = true;
                            break;
                        }
                }
            }

            nodes.Add(nobj);
            nodeTable.Add(this);
        }

        public int FindOrCreateNodeForVertex(List<Vertex> nodeTable, JArray nodes, JsonLog obj)
        {
            int i = 0;
            foreach (var node in nodeTable)
            {
                if (node == this)
                {
                    return i;
                }
                i++;
            }
            MakeNodeForVertex(nodeTable, nodes, obj);
            return i;
        }
    }

    /// <summary>
    /// Describe a value like the payload on a Node or the value of a field on an external structure
    /// </summary>
    public abstract class ValueBase : Expression
    {
    }

    public class ValueLiteral : ValueBase
    {
        public object V { get; private set; }

        public ValueLiteral(object v)
        {
            V = v;
        }

        public override object Evaluate(SymbolTable dict, Restrictor context)
        {
            return this;
            // return V;  breaks another test
        }

        override public bool SatisfiedBy(Expression valCandidate)
        {
            if (!(valCandidate is ValueLiteral))
                return false;
            var vl = (ValueLiteral)valCandidate;
            return (V == null) ? (vl.V == null) : V.Equals(vl.V);
        }
        public override void Format(PrettyFormatter sb)
        {
            if (V == null)
                sb.Keyword("null");
            else
                sb.Identifier(V.ToString());
        }
        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddToJObject(obj, "V", V);
            }
        }
    }

    class SymbolReference : ValueBase
    {
        public string SymbolName { get; private set; }

        public SymbolReference(string id)
        {
            SymbolName = id;
        }

        public override object Evaluate(SymbolTable dict, Restrictor context)
        {
            return dict.Lookup(SymbolName);
        }

        override public bool SatisfiedBy(Expression valCandidate)
        {
            if (!(valCandidate is SymbolReference))
                return false;
            var sym = (SymbolReference)valCandidate;
            if (sym.SymbolName != SymbolName)
                return false;
            return true;
        }
        public override void Format(PrettyFormatter sb)
        {
            sb.Identifier(SymbolName);
        }
        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddToJObject(obj, "SymbolName", SymbolName);
            }
        }
    }

    /// <summary>
    /// Base class for objects that have a name
    /// </summary>
    public class Named : Base
    {
        public string Name { get; private set; }

        public Named(string name)
        {
            Name = name;
        }
        public override void Format(PrettyFormatter sb)
        {
            sb.Identifier(Name);
        }
        public override void AddToJReference(JObject vobj)
        {
            vobj.Add(LogEntry.LOG_REF_TEXT, Name);
        }

        public override string GetDesc()
        {
            return Id.ToString() + ": " + Name;
        }

        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            AddToJObject(obj, "Name", Name);
        }
    }

    public abstract class Expression : Base
    {
        virtual public bool SatisfiedBy(Expression valCandidate)
        {
            throw new NotImplementedException();
        }

        public abstract object Evaluate(SymbolTable dict, Restrictor context);
    }

    /// <summary>
    /// Lookup a symbol using a vertex, then navigate using a seq of edgedefs
    /// </summary>
    public class LookupExpression : Expression
    {
        private Vertex vKey;
        private EdgeDescriptor[] rged;

        public LookupExpression (Vertex vKey, params EdgeDescriptor[] rged)
        {
            this.vKey = vKey;
            this.rged = rged;
        }

        public override object Evaluate(SymbolTable dict, Restrictor context)
        {
            Vertex vVal = dict.vLookup(vKey);
            foreach (EdgeDescriptor ed in rged)
            {
                if (vVal == null)
                    break;
                Edge eVal = vVal.eaOutOpt(context, ed);
                if (eVal == null)
                    return null;
                vVal = eVal.vHead;
            }
            return vVal;
        }
    }

    /// <summary>
    /// Operators appear in expressions that are used find vertices look for matching edges or to create edges to.
    /// </summary>
    public abstract class Operator : Named
    {
        public static Operator forward = new OperatorForward("/");
        public static Operator reverse = new OperatorReverse("\\");

        public Operator(string name)
            : base(name)
        {
        }

        public abstract object Apply(object lhs, object rhs, Restrictor context);

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Utility function that 
        /// </summary>
        /// <param name="lhs">value on left side of operator: vertex to start edge traversal from</param>
        /// <param name="rhs">value on right side of operator: identifies the descriptor for the edge to traverse</param>
        /// <param name="direction">traverse an outgoing edge or incoming</param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected object ApplyGetEdge(object lhs, object rhs, DirectionType direction, Restrictor context)
        {
            if (!(lhs is Vertex))
                throw new ArgumentError("vertex required for " + Name);
            if (!(rhs is EdgeDescriptor))
                throw new ArgumentError("edgeDescriptor required for " + Name);
            var vertex = (Vertex)lhs;
            var edgeDesc = (EdgeDescriptor)rhs;
            List<Edge> edges = (direction == DirectionType.Incoming) ? vertex.eaIncoming(context) : vertex.eaOutgoing(context);
            foreach (Edge edge in edges)
            {
                if (edge.Desc == edgeDesc)
                {
                    return (direction == DirectionType.Incoming) ? edge.vTail : edge.vHead;
                }
            }
            return null;
        }

    }

    class OperatorForward : Operator
    {
        public OperatorForward(string name)
            : base(name)
        {
        }

        public override object Apply(object lhs, object rhs, Restrictor context)
        {
            return ApplyGetEdge(lhs, rhs, DirectionType.Outgoing, context);
        }
    }

    class OperatorReverse : Operator
    {
        public OperatorReverse(string name)
            : base(name)
        {
        }

        public override object Apply(object lhs, object rhs, Restrictor context)
        {
            return ApplyGetEdge(lhs, rhs, DirectionType.Outgoing, context);
        }
    }


    public class VertexExpression : Expression
    {
        public Vertex vRef { get; set; }

        public override object Evaluate(SymbolTable dict, Restrictor context)
        {
            throw new ArgumentError("not implemented");
        }

        public override void Format(PrettyFormatter sb)
        {
            vRef.FormatSubobject(sb);
        }

        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddToJObject(obj, "vRef", vRef);
            }
        }
    }

    public class EdgeExpression : Expression
    {
        public Edge eRef { get; set; }

        public override object Evaluate(SymbolTable dict, Restrictor context)
        {
            throw new ArgumentError("not implemented");
        }

        public override void Format(PrettyFormatter sb)
        {
            eRef.FormatSubobject(sb);
        }

        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddToJObject(obj, "eRef", eRef);
            }
        }
    }

    public class EdgeDescriptorExpression : Expression
    {
        public EdgeDescriptor edRef { get; set; }

        public override object Evaluate(SymbolTable dict, Restrictor context)
        {
            throw new ArgumentError("not implemented");
        }

        public override void Format(PrettyFormatter sb)
        {
            edRef.FormatSubobject(sb);
        }
        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddToJObject(obj, "edRef", edRef);
            }
        }
    }

    public class BinaryExpression : Expression
    {
        public Expression exLhs { get; set; }
        public Operator op { get; set; }
        public Expression exRhs { get; set; }

        public override object Evaluate(SymbolTable dict, Restrictor context)
        {
            var vLhs = exLhs.Evaluate(dict, context);
            var eRhs = exRhs.Evaluate(dict, context);
            return op.Apply(vLhs, eRhs, context);
        }

        public override void Format(PrettyFormatter sb)
        {
            sb.Push();
            exLhs.FormatSubobject(sb);
            op.FormatSubobject(sb);
            exRhs.FormatSubobject(sb);
            sb.Pop();
        }
        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            if (obj.IsDiagram())
            {
            }
            else
            {
                AddToJObject(obj, "exLhs", exLhs);
                AddToJObject(obj, "op", op);
                AddToJObject(obj, "exRhs", exRhs);
            }
        }

    }

    /// <summary>
    /// An assignment defines the lhs symbol as a value that is obtained from evaluating
    /// an expression defined using symbols on the other side. So instead of that vertex being
    /// created when the destination graph is instantiated, an existing vertex is found with that
    /// expression. The edges can be created to/from that vertex.
    /// </summary>
    public class Assignment : Base
    {
        public Vertex vLhs { get; set; }
        public Expression exExpr { get; set; }

        public override string ToString()
        {
            return vLhs.ToString() + "=" + exExpr.ToString();
        }
        public override void AddToJObject(JsonLog obj)
        {
            base.AddToJObject(obj);
            AddToJObject(obj, "vLhs", vLhs);
            AddToJObject(obj, "exExpr", exExpr);
        }
    }

    /// <summary>
    /// Link in a chain of vertices. Used so that iterators can proceed as new vertices are added to graphs.
    /// </summary>
    public class VertexLink
    {
        public Vertex v { get; private set; }
        public VertexLink next { get; set; }

        public VertexLink(Vertex v, VertexLink next)
        {
            this.v = v;
            this.next = next;
        }
    }


    /// <summary>
    /// Link in a chain of edges. Used so that iterators can proceed as new edges are added to graphs.
    /// </summary>
    public class EdgeLink
    {
        public Edge e { get; private set; }
        public EdgeLink next { get; set; }

        public EdgeLink(Edge e, EdgeLink next)
        {
            this.e = e;
            this.next = next;
        }
    }

    /// <summary>
    /// A graph is a set of vertices and edges.
    /// </summary>
    public class Graph : Base
    {
        public VertexLink vlFirst { get; private set; }
        public VertexLink vlLast { get; private set; }
        public int NumVertices { get; private set; }
        public EdgeLink elFirst { get; private set; }
        public EdgeLink elLast { get; private set; }
        public int NumEdges { get; private set; }
        public Restrictor context { get; private set; }

        /// <summary>
        /// When transforming into this graph, the nodes identified by assignments are obtained by evaluating those assignments.
        /// Then the remain vertices are created new and then connected by creating the edges that those vertices are associated with.
        /// </summary>
        public List<Assignment> vaAssignments { get; set; }

        public Graph(Restrictor context)
        {
            this.context = context;
        }

        public Graph(VertexLink vlFirst, VertexLink vlLast, EdgeLink elFirst, EdgeLink elLast, Restrictor context)
        {
            this.vlFirst = vlFirst;
            this.vlLast = vlLast;
            this.elFirst = elFirst;
            this.elLast = elLast;
            this.context = context;
            NumVertices = 0;
            for (VertexLink v = vlFirst; v != null; v = v.next)
                NumVertices++;
            NumEdges = 0;
            for (EdgeLink e = elFirst; e != null; e = e.next)
                NumEdges++;
        }

        public int GetVertexIndexNum(Vertex v)
        {
            int i = 0;
            for (var vl = vlFirst; vl != null; vl = vl.next)
            {
                if (vl.v == v)
                    return i;
                i++;
            }
            return -1;
        }

        public override void ExtendDiagram(JArray nodes, JArray edges, List<Vertex> nodeTable, JsonLog obj)
        {
            for (var vl = vlFirst; vl != null; vl = vl.next)
            {
                vl.v.MakeNodeForVertex(nodeTable, nodes, obj);
            }
            for (var el = elFirst; el != null; el = el.next)
            {
                el.e.MakeLinkForEdge(nodes, edges, nodeTable, obj);
            }
        }

        public override void AddToJObject(JsonLog obj)
        {
            if (obj.IsDiagram())
            {
                CreateDiagram(obj);
            }
            else
            {
                base.AddToJObject(obj);
                AddToJObject(obj, "numVertices", NumVertices);
                AddToJObject(obj, "numEdges", NumEdges);
                AddToJObject(obj, "context", context);

                if (vlFirst != null)
                {
                    JArray list = new JArray();
                    for (var vl = vlFirst; vl != null; vl = vl.next)
                    {
                        list.Add(vl.v.MakeJReference(null));
                    }
                    obj.Add("vl", list);
                }

                if (elFirst != null)
                {
                    JArray list = new JArray();
                    for (var el = elFirst; el != null; el = el.next)
                    {
                        list.Add(el.e.MakeJReference(null));
                    }
                    obj.Add("el", list);
                }
            }
        }

        public void Add(Vertex v)
        {
            NumVertices++;
            var link = new VertexLink(v, null);
            if (vlFirst == null)
                vlFirst = link;
            else
                vlLast.next = link;
            vlLast = link;
        }

        public void Add(Edge e)
        {
            NumEdges++;
            var link = new EdgeLink(e, null);
            if (elFirst == null)
                elFirst = link;
            else
                elLast.next = link;
            elLast = link;
        }

        /// <summary>
        /// Find all occurences of pattern in data
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /*
        public static List<GraphMatch> FindPatternInData(Graph pattern, Graph data, TraceNode trace)
        {
            return new PatternFinder(pattern, data, trace).Match();
        }
        */

        /// <summary>
        /// Return true if the other graph is the same as this.
        /// </summary>
        /// <param name="gOther"></param>
        /// <returns></returns>
        public bool SameAs(Graph gOther)
        {
            if (NumVertices != gOther.NumVertices)
                return false;
            if (NumEdges != gOther.NumEdges)
                return false;
            for (VertexLink vLink = vlFirst; vLink != null; vLink = vLink.next)
            {
                bool found = false;
                for (VertexLink vLinkOther = gOther.vlFirst; vLinkOther != null; vLinkOther = vLinkOther.next)
                {
                    if (vLink.v.SameAs(vLinkOther.v))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            for (EdgeLink eLink = elFirst; eLink != null; eLink = eLink.next)
            {
                bool found = false;
                for (EdgeLink eLinkOther = gOther.elFirst; eLinkOther != null; eLinkOther = eLinkOther.next)
                {
                    if (eLink.e.Matches(eLinkOther.e))
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
            sb.Push();
            for (VertexLink vLink = vlFirst; vLink != null; vLink = vLink.next)
            {
                vLink.v.FormatSubobject(sb);
            }
            sb.Pop();
            sb.Push();
            for (EdgeLink eLink = elFirst; eLink != null; eLink = eLink.next)
            {
                eLink.e.FormatSubobject(sb);
            }
            sb.Pop();
        }
        public override void AddToJReference(JObject vobj)
        {
            vobj.Add(LogEntry.LOG_REF_TEXT, "numVertices=" + NumVertices + " numEdges=" + NumEdges + " " + context);
        }
    }

    /// <summary>
    /// Store key/value pairs with inheritance from a higher level scope.
    /// </summary>
    public class SymbolTable : ValueBase
    {
        private Dictionary<string, object> mpst_objDict;
        private Dictionary<Vertex, Vertex> mpv_vDict;
        public SymbolTable parent;

        override public bool SatisfiedBy(Expression valCandidate)
        {
            return false;
        }
        override public object Evaluate(SymbolTable dict, Restrictor context)
        {
            return this;
        }

        public SymbolTable(SymbolTable parent)
        {
            this.parent = parent;
        }

        public void Add(string key, object value)
        {
            try
            {
                if (mpst_objDict == null)
                    mpst_objDict = new Dictionary<string, object>();
                mpst_objDict.Add(key, value);
            }
            catch (System.ArgumentException e)
            {
                throw e;
            }
        }

        public void Add(Vertex key, Vertex value)
        {
            try
            {
                if (mpv_vDict == null)
                    mpv_vDict = new Dictionary<Vertex, Vertex>();
                mpv_vDict.Add(key, value);
            }
            catch (System.ArgumentException e)
            {
                throw e;
            }
        }

        public object Lookup(string key)
        {
            if (mpst_objDict == null)
                return null;
            SymbolTable st = this;
            while (st != null)
            {
                object v;
                if (st.mpst_objDict.TryGetValue(key, out v))
                    return v;
                st = st.parent;
            }
            return null;
        }

        public Vertex vLookup(Vertex key)
        {
            if (mpv_vDict == null)
                return null;
            SymbolTable st = this;
            while (st != null)
            {
                Vertex v;
                if (st.mpv_vDict.TryGetValue(key, out v))
                    return v;
                st = st.parent;
            }
            return null;
        }

        public IEnumerable<object> Contents
        {
            get
            {
                if (mpst_objDict != null)
                    return mpst_objDict.Values;
                if (mpv_vDict != null)
                    return mpv_vDict.Values;
                return null;
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
                AddToJObject(obj, "parent", parent);
                AddToJObject(obj, "dict", mpst_objDict);
            }
        }
    }

    public class Correspondence : Named
    {
        public Graph gFirst { get; set; }
        public Graph gSecond { get; set; }
        public Graph gUsing { get; set; }

        public SymbolTable stFirst { get; private set; }
        public SymbolTable stSecond { get; private set; }
        public SymbolTable stUsing { get; private set; }

        public Correspondence(String name, SymbolTable stCorrespondence)
            : base(name)
        {
            stFirst = new SymbolTable(stCorrespondence);
            stSecond = new SymbolTable(stCorrespondence);
            stUsing = new SymbolTable(stCorrespondence);
        }
        public override void AddToJObject(JsonLog obj)
        {
            if (obj.IsDiagram())
            {
                CreateDiagram(obj);
            }
            else
            {
                base.AddToJObject(obj);
                AddToJObject(obj, "gFirst", gFirst);
                AddToJObject(obj, "gSecond", gSecond);
                AddToJObject(obj, "gUsing", gUsing);
            }
        }
        public override void ExtendDiagram(JArray nodes, JArray edges, List<Vertex> nodeTable, JsonLog obj)
        {
            gFirst.ExtendDiagram(nodes, edges, nodeTable, obj);
            gSecond.ExtendDiagram(nodes, edges, nodeTable, obj);
            gUsing.ExtendDiagram(nodes, edges, nodeTable, obj);
        }

        public override void Format(PrettyFormatter sb)
        {
            gFirst.FormatSubobject(sb);
            sb.FieldValue("to", gSecond);
            sb.FieldValue("using", gUsing);
        }

        public enum ReplacementDirectionType
        {
            Forward,
            Reverse
        }

        /// <summary>
        /// Using the given correspondences in the GraphMatch, migrate the data from the data graph
        /// to a new output graph, by first mapping the data values to the pattern values via the match.
        /// </summary>
        /// <param name="dict">symbol table of source vertices: name->vertex</param>
        /// <param name="match">contains matches that map pattern vertices and edges to data</param>
        /// <param name="direction">map from first to second or backwards</param>
        /// <param name="rOutput">the restriction to use to tag the edges in the output graph</param>
        /// <param name="gOutput">optional graph to collect the vertices and edges created in this step</param>
        /// <returns></returns>
        public void Perform(SymbolTable dict, GraphMatch match, ReplacementDirectionType direction, Restrictor rInput, Restrictor rOutput, Graph gOutput)
        {
            SymbolTable stLocal = new SymbolTable(dict);
            Graph gSource = (direction == ReplacementDirectionType.Forward) ? gFirst : gSecond;
            Graph gDest = (direction == ReplacementDirectionType.Forward) ? gSecond : gFirst;

            // build a map of vertices named in graph to the ones found by assignment or created for output.
            var dvRefToValue = new Dictionary<Vertex, Vertex>();

            for (VertexLink vLink = gSource.vlFirst; vLink != null; vLink = vLink.next)
            {
                var vSource = vLink.v;
                // enter symbols for payload
                if (vSource.Value is SymbolReference)
                {
                    var val = match.GetDataVertex(vSource).Value;
                    stLocal.Add(((SymbolReference)vSource.Value).SymbolName, val);
                }
                // Later we will evaluate assignment expression on rhs where refs to vertices in the dest graph are interpreted 
                // as as vertex name in the source graph, then mapped to the corresponding data. 
                // So need symbol table entries that map source graph vertices in the rhs expr to data vertices.
                Vertex vMatchedToSource = match.GetDataVertex(vSource);
                stLocal.Add(vSource.Name, vMatchedToSource);
                dvRefToValue.Add(vSource, vMatchedToSource);
            }

            foreach (Assignment assignment in gDest.vaAssignments)
            {
                Vertex vDestRef = assignment.vLhs;
                var vVal = (Vertex)assignment.exExpr.Evaluate(stLocal, rInput);
                dvRefToValue.Add(vDestRef, vVal);
            }
            // foreach vertex in the dest graph,
            //    find or create the output vertex
            for (VertexLink vLink = gDest.vlFirst; vLink != null; vLink = vLink.next)
            {
                var vDest = vLink.v;
                Vertex vOutput;
                if (!dvRefToValue.TryGetValue(vDest, out vOutput))
                {
                    vOutput = new Vertex(vDest.Name);
                    dvRefToValue.Add(vDest, vOutput);
                }
                if (vDest.Value != null)
                {
                    if (vDest.Value is ValueLiteral)
                        vOutput.Value = vDest.Value;
                    else if (vDest.Value is SymbolReference)
                    {
                        var sr = (SymbolReference)vDest.Value;
                        try
                        {
                            var val = stLocal.Lookup(sr.SymbolName);
                            vOutput.Value = (ValueBase)val;
                        }
                        catch (Exception)
                        {
                            throw new ArgumentError("could not find value for " + sr.SymbolName);
                        }
                    }
                    else
                        throw new ArgumentError("unexpected type of value " + vDest.Value);
                }
                if (gOutput != null)
                    gOutput.Add(vOutput);
                //  maybe:    change syntax to v:expr, remove v@ if not needed, can have v@name:expr
            }
            for (EdgeLink eLink = gDest.elFirst; eLink != null; eLink = eLink.next)
            {
                AddEdge(eLink.e, rOutput, gOutput, dvRefToValue);
            }
            for (EdgeLink eLink = gUsing.elFirst; eLink != null; eLink = eLink.next)
            {
                AddEdge(eLink.e, rOutput, gOutput, dvRefToValue);
            }
            // call associated 'after' statements
        }

        private void AddEdge(Edge ePattern, Restrictor rOutput, Graph gOutput, Dictionary<Vertex, Vertex> dvRefToValue)
        {
            try
            {
                // Create edges
                var vOutputTail = dvRefToValue.ContainsKey(ePattern.vTail) ? dvRefToValue[ePattern.vTail] : ePattern.vTail;
                var vOutputHead = dvRefToValue.ContainsKey(ePattern.vHead) ? dvRefToValue[ePattern.vHead] : ePattern.vHead;
                var eOutput = new Edge(ePattern.Desc, vOutputTail, vOutputHead, rOutput);
                vOutputTail.AddOutgoing(eOutput);
                vOutputHead.AddIncoming(eOutput);
                if (gOutput != null)
                    gOutput.Add(eOutput);                    // todo: set some flag to say which edges need to be created and which are just for matching
            }
            catch (Exception e)
            {
                throw new ArgumentError("could not find vertex for edge: " + e);
            }
        }
    }

    /// <summary>
    /// Base class for referring to a field, property or method on an external structure.
    /// </summary>
    class ExternalAcessor : Named
    {
        public ExternalAcessor(string name)
            : base(name)
        {
        }
    }

    /// <summary>
    /// Identify a field on an external data structure.
    /// </summary>
    class ExternalField : ExternalAcessor
    {
        //   public FieldInfo Info { get; set; }

        public ExternalField(string name)
            : base(name)
        {
        }
    }

    /// <summary>
    /// Identify an external data structure to be matched against
    /// </summary>
    class Structure : Named
    {
        public System.Type Type { get; set; }

        public Structure(string name)
            : base(name)
        {
        }
    }
}
