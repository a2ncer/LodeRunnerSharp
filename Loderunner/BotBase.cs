using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Loderunner
{
public class BotBase
{
protected const string RIGHT = "RIGHT";
protected const string LEFT = "LEFT";
protected const string UP = "UP";
protected const string DOWN = "DOWN";
protected const string ACT_RIGHT = "ACT," + RIGHT;
protected const string ACT_LEFT = "ACT," + LEFT;

    protected int MAX_TARGET_GOLDS = 10;

    #region board description constants
    protected static readonly char[] HeroChars = new[] { HeroStandsLeft, HeroStandsRight, HeroDrillLeft, HeroDrillRight, HeroOnLadder, '[', ']', HeroOnPipeLeft, HeroOnPipeRight, HeroDead };
    protected static readonly char[] HeroOnPipe = new[] { HeroOnPipeLeft, HeroOnPipeRight };
    protected static readonly char[] HeroStands = new[] { HeroStandsLeft, HeroStandsRight };
    protected static readonly char[] HeroDrill = new[] { HeroDrillLeft, HeroDrillRight };
    protected static readonly char[] EnemyChars = new[] { 'Q', '«', '»', '<', '>', 'X' };

    protected const char HeroOnPipeLeft = '{';
    protected const char HeroOnPipeRight = '}';
    protected const char HeroStandsLeft = '◄';
    protected const char HeroStandsRight = '►';
    protected const char HeroDrillLeft = 'Я';
    protected const char HeroDrillRight = 'R';
    protected const char HeroOnLadder = 'Y';
    protected const char HeroDead = 'Ѡ';

    protected const char Gold = '$';
    protected const char Wall = '☼';
    protected const char Brick = '#';
    protected const char Ladder = 'H';
    protected const char Pipe = '~';

    protected const char DrillSpace = '.';
    protected const char DrillPit = '*';
    protected const char Space = ' ';
    protected static readonly char[] PitFill = new[] { '1', '2', '3', '4' };
    #endregion

    protected char[,] _board = new char[0, 0];

    private Tuple<Point, char> _me;
    private List<StepInfo> _route;
    private Point? _heroNextExpectedPoint;
    private string _lastCommand;

    private Dictionary<string, Func<Point, StepInfo>> _stepResolvers = new Dictionary<string, Func<Point, StepInfo>>();

    public BotBase()
    {
        _stepResolvers.Add("L", TryTurnLeft);
        _stepResolvers.Add("R", TryTurnRight);
        _stepResolvers.Add("D", TryTurnDown);
        _stepResolvers.Add("U", TryTurnUp);
        _stepResolvers.Add("RD", TryTurnRightDown);
        _stepResolvers.Add("LD", TryTurnLeftDown);
    }

    public void SetBoard(char[,] board)
    {
        if (board == null)
        {
            throw new ArgumentNullException("board");
        }
        _board = board;
    }

    public Point GetHeroPosition()
    {
        if (_me != null)
        {
            return _me.Item1;
        }
        else
        {
            return new Point(-1, -1);
        }
    }

    public virtual string NextCommand(StringBuilder log)
    {
        BuildRoute(log);

        // TODO: refactor, too complicated...
        if (_route != null)
        {
            var step = _route.First();
            var cmd = step.Commands.Dequeue();
            if (!step.Commands.Any())
            {
                _route.RemoveAt(0);
                _heroNextExpectedPoint = step.Pos;
                _lastCommand = cmd;
            }
            else
            {
                _heroNextExpectedPoint = null;
            }

            if (!_route.Any())
            {
                _route = null; // route is completed
            }

            return cmd;
        }

        return string.Empty;
    }

    protected void BuildRoute(StringBuilder log)
    {
        _me = Search(c => HeroChars.Contains(c));

        if (_me == null) //WTF!??
        {
            _route = null;
            log.AppendLine("Hero not found");
            return;
        }

        if (_me.Item2 == HeroDead)
        {
            log.AppendLine("Hero is dead ... Ѡ ...");
            _route = null;
            return;
        }

        // reset route if previous command is not moved hero to next expected point
        if (_heroNextExpectedPoint != null && _heroNextExpectedPoint != _me.Item1)
        {
            log.AppendLine(string.Format("Command '{0}' failed. Expected Pos {1}. Actual Pos {2}", _lastCommand, _heroNextExpectedPoint.Value, _me.Item1));
            _route = null;
        }

        if (_route != null)
        {
            // reset route if someone stolen the gold
            var target = _route.Last();
            if (target.ExpectedChar != _board[target.Pos.X, target.Pos.Y])
            {
                log.AppendLine("Gold was stolen");
                _route = null;
            }
            else
            {
                // reset route if enemy is on the route near (5 steps)
                foreach (var step in _route.Take(5))
                {
                    if (EnemyChars.Contains(_board[step.Pos.X, step.Pos.Y]))
                    {
                        log.AppendLine("Enemy is near");
                        _route = null;
                        break;
                    }
                }
            }
        }

        if (_route == null)
        {
            log.AppendLine("Evaluate new route...");
            var allGolds = SearchAll((c) => c == Gold)
              .OrderBy(c => GetDistance(c.Item1, _me.Item1))
              .Select(c => c.Item1)
              .ToList();

            // evaluate route to near gold
            _route = EvaluateBestRoute(allGolds.Take(MAX_TARGET_GOLDS).ToList());
            if (_route != null)
            {
                log.AppendLine("Route is found for near point");
            }

            if (_route == null)
            {
                // if can't find route to near - try all other golds (may be slow)
                _route = EvaluateBestRoute(allGolds.Skip(MAX_TARGET_GOLDS).ToList());
                if (_route != null)
                {
                    log.AppendLine("Route is found for far point");
                }
            }
        }

        if (_route != null)
        {
            var target = _route.Last();
            log.AppendLine(string.Format("Target: {0}, ({1}) Steps: {2}", target.ExpectedChar, target.Pos.ToString(), _route.Count));
            foreach (var step in _route)
            {
                log.AppendLine(string.Format("Command: {0}, ExpChar: {1}", string.Join("|", step.Commands), step.ExpectedChar));
            }
        }
    }

    protected Tuple<Point, char> Search(Func<char, bool> whatToSearch)
    {
        for (int y = 0; y < _board.GetLength(1); ++y)
        {
            for (int x = 0; x < _board.GetLength(0); ++x)
            {
                var c = _board[x, y];
                if (whatToSearch(c))
                {
                    return new Tuple<Point, char>(new Point(x, y), c);
                }
            }
        }

        return null;
    }

    protected IEnumerable<Tuple<Point, char>> SearchAll(Func<char, bool> whatToSearch)
    {
        for (int y = 0; y < _board.GetLength(1); ++y)
        {
            for (int x = 0; x < _board.GetLength(0); ++x)
            {
                var c = _board[x, y];
                if (whatToSearch(c))
                {
                    yield return new Tuple<Point, char>(new Point(x, y), c);
                }
            }
        }
    }

    protected static bool CanLocatedOn(char c)
    {
        return c == Space || c == Pipe || c == Gold || c == Ladder || c == DrillSpace || c == DrillPit;
    }

    protected static bool IsGround(char c)
    {
        return c == Wall || c == Brick || c == Ladder;
    }

    protected static bool IsAnchorOrOnTheGround(char c, char bottomChar)
    {
        return c == Pipe || c == Ladder || c == HeroOnLadder || HeroOnPipe.Contains(c) || HeroStands.Contains(c) || HeroDrill.Contains(c)
               ||
               IsGround(bottomChar);
    }

    protected internal bool CanRight(Point from)
    {
        var fromChar = _board[from.X, from.Y];
        var rightChar = _board[from.X + 1, from.Y];
        var bottomChar = _board[from.X, from.Y + 1];

        bool r = CanLocatedOn(rightChar) && IsAnchorOrOnTheGround(fromChar, bottomChar);
        return r;
    }

    protected internal bool CanLeft(Point from)
    {
        var fromChar = _board[from.X, from.Y];
        var leftChar = _board[from.X - 1, from.Y];
        var bottomChar = _board[from.X, from.Y + 1];

        bool r = CanLocatedOn(leftChar) && IsAnchorOrOnTheGround(fromChar, bottomChar);
        return r;
    }

    protected internal bool CanUp(Point from)
    {
        var fromChar = _board[from.X, from.Y];
        var upChar = _board[from.X, from.Y - 1];

        bool r = CanLocatedOn(upChar) && ((fromChar == HeroOnLadder) || fromChar == Ladder);
        return r;
    }

    protected internal bool CanDown(Point from)
    {
        var downChar = _board[from.X, from.Y + 1];

        bool r = CanLocatedOn(downChar);
        return r;
    }

    protected internal bool CanRightDown(Point from)
    {
        var fromChar = _board[from.X, from.Y];
        var rightDownChar = _board[from.X + 1, from.Y + 1];
        var rightChar = _board[from.X + 1, from.Y];
        var bottomChar = _board[from.X, from.Y + 1];

        bool r = (fromChar == Space || fromChar == Gold || HeroStands.Contains(fromChar))
              && IsGround(bottomChar)
              && rightDownChar == Brick
              && (rightChar == Space || rightChar == Gold);

        return r;
    }

    protected internal bool CanLeftDown(Point from)
    {
        var fromChar = _board[from.X, from.Y];
        var leftDownChar = _board[from.X - 1, from.Y + 1];
        var leftChar = _board[from.X - 1, from.Y];
        var bottomChar = _board[from.X, from.Y + 1];

        bool r = (fromChar == Space || fromChar == Gold || HeroStands.Contains(fromChar))
              && IsGround(bottomChar)
              && leftDownChar == Brick
              && (leftChar == Space || leftChar == Gold);

        return r;
    }

    protected static Point GetRightPoint(Point from)
    {
        return new Point(from.X + 1, from.Y);
    }

    protected static Point GetLeftPoint(Point from)
    {
        return new Point(from.X - 1, from.Y);
    }

    protected static Point GetUpPoint(Point from)
    {
        return new Point(from.X, from.Y - 1);
    }

    protected static Point GetDownPoint(Point from)
    {
        return new Point(from.X, from.Y + 1);
    }

    public List<StepInfo> EvaluateBestRoute(List<Point> targets)
    {
        List<List<StepInfo>> routesToTarget = new List<List<StepInfo>>();
        foreach (var target in targets)
        {
            var route = DiscoverRoute(target);
            if (route != null)
            {
                routesToTarget.Add(route);
            }
        }

        var shortestRoute = routesToTarget.OrderBy(route => route.SelectMany(x => x.Commands).Count()).FirstOrDefault();
        return shortestRoute;
    }

    protected char CharAt(Point p)
    {
        return _board[p.X, p.Y];
    }

    static Comparer2<GraphNode> nodeComparer = new Comparer2<GraphNode>((x1, x2) => x1.ApproximateIndex.CompareTo(x2.ApproximateIndex));


    protected List<StepInfo> DiscoverRoute(Point target)
    {
        var treeLeafs = new List<GraphNode>();
        var bestRouteToPos = new Dictionary<Point, GraphNode>();

        var rootNode = new GraphNode(null,
                                     _me.Item1, CharAt(_me.Item1),
                                     new Queue<string>(),
                                     GetDistance(_me.Item1, target));

        treeLeafs.Add(rootNode);

        bestRouteToPos.Add(rootNode.Info.Pos, rootNode);

        GraphNode targetNode = null;

        while (treeLeafs.Count > 0 && targetNode == null)
        {
            var node = treeLeafs.First();

            if (node.CheckedDirections.Count == _stepResolvers.Count)
            {
                treeLeafs.RemoveAt(0); // remove node
                continue;
            }

            foreach (var sr in _stepResolvers.Where(s => !node.CheckedDirections.Contains(s.Key)).ToList())
            {
                node.CheckedDirections.Add(sr.Key);

                var step = sr.Value(node.Info.Pos);

                if (step != null)
                {
                    var nextNode = new GraphNode(node, step, GetDistance(step.Pos, target));

                    if (nextNode.Info.Pos == target)
                    {
                        targetNode = nextNode;
                        break;
                    }

                    // check that this route is better than already discovered route to this point
                    if (!bestRouteToPos.ContainsKey(step.Pos) || bestRouteToPos[step.Pos].ApproximateIndex > nextNode.ApproximateIndex)
                    {
                        //node.Tree.Add(nextNode); // for debug purposes only, not required by algorithm

                        treeLeafs.Add(nextNode);
                        treeLeafs.Sort(nodeComparer);
                        bestRouteToPos[step.Pos] = nextNode;

                        if (nextNode.ApproximateIndex <= node.ApproximateIndex)
                        {
                            // nextNode is approximates to target, go immediately
                            break;
                        }
                    }
                }
            }
        }

        if (targetNode != null)
        {
            var listResult = new List<StepInfo>();
            var iterator = targetNode;

            while (iterator != null)
            {
                listResult.Add(iterator.Info);
                iterator = iterator.Parent;
            }

            listResult.Reverse();
            listResult.RemoveAt(0); // start pos

            return listResult;
        }

        return null;
    }

    protected StepInfo TryTurnRight(Point from)
    {
        if (CanRight(from))
        {
            Point to = GetRightPoint(from);
            char toChar = CharAt(to);
            var commands = new Queue<string>();
            commands.Enqueue(RIGHT);
            var step = new StepInfo(to, toChar, commands);
            return step;
        }

        return null;
    }

    protected StepInfo TryTurnLeft(Point from)
    {
        if (CanLeft(from))
        {
            Point to = GetLeftPoint(from);
            char toChar = CharAt(to);
            var commands = new Queue<string>();
            commands.Enqueue(LEFT);
            var step = new StepInfo(to, toChar, commands);
            return step;
        }

        return null;
    }

    protected StepInfo TryTurnUp(Point from)
    {
        if (CanUp(from))
        {
            Point to = GetUpPoint(from);
            char toChar = CharAt(to);
            var commands = new Queue<string>();
            commands.Enqueue(UP);
            var step = new StepInfo(to, toChar, commands);
            return step;
        }

        return null;
    }

    protected StepInfo TryTurnDown(Point from)
    {
        if (CanDown(from))
        {
            Point to = GetDownPoint(from);
            char toChar = CharAt(to);
            var commands = new Queue<string>();
            commands.Enqueue(DOWN);
            var step = new StepInfo(to, toChar, commands);
            return step;
        }

        return null;
    }

    protected StepInfo TryTurnRightDown(Point from)
    {
        if (CanRightDown(from))
        {
            Point to = new Point(from.X + 1, from.Y + 1);
            char toChar = CharAt(to);
            var commands = new Queue<string>();
            commands.Enqueue(ACT_RIGHT);
            commands.Enqueue(RIGHT);
            commands.Enqueue(DOWN);

            var step = new StepInfo(to, toChar, commands);
            return step;
        }

        return null;
    }

    protected StepInfo TryTurnLeftDown(Point from)
    {
        if (CanLeftDown(from))
        {
            Point to = new Point(from.X - 1, from.Y + 1);
            char toChar = CharAt(to);
            var commands = new Queue<string>();
            commands.Enqueue(ACT_LEFT);
            commands.Enqueue(LEFT);
            commands.Enqueue(DOWN);

            var step = new StepInfo(to, toChar, commands);
            return step;
        }

        return null;
    }

    protected static int GetDistance(Point from, Point to)
    {
        return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
    }
}

public class GraphNode
{
    public readonly StepInfo Info;
    public readonly GraphNode Parent;
    public readonly List<GraphNode> Tree = new List<GraphNode>();
    private int _totalCommandsCount;
    public readonly int ApproximateIndex;
    public readonly List<string> CheckedDirections = new List<string>();

    public GraphNode(GraphNode parent, Point pos, char boardChar, Queue<string> commands, int distanceToTarget)
        : this(parent, new StepInfo(pos, boardChar, commands), distanceToTarget) { }

    public GraphNode(GraphNode parent, StepInfo step, int distanceToTarget)
    {
        Parent = parent;
        Info = step;
        _totalCommandsCount = (parent == null ? 0 : parent._totalCommandsCount) + Info.Commands.Count;
        ApproximateIndex = _totalCommandsCount + distanceToTarget * 2; // distance to target has more priority than commands count
    }

    public override string ToString()
    {
        return "Ind: " + ApproximateIndex.ToString() + " " + Info.ToString();
    }
}

public class StepInfo
{
    public Point Pos { get; private set; }
    public char ExpectedChar { get; private set; }
    public Queue<string> Commands { get; private set; }

    public StepInfo(Point pos, char expectedChar, Queue<string> commands)
    {
        Pos = pos;
        ExpectedChar = expectedChar;
        Commands = commands;
    }

    public override string ToString()
    {
        return string.Format("{0} {1} {2}", string.Join("|", Commands.ToList()), Pos, ExpectedChar);
    }
}


public class Comparer2<T> : Comparer<T>
{
    //private readonly Func<T, T, int> _compareFunction;
    private readonly Comparison<T> _compareFunction;

    #region Constructors

    public Comparer2(Comparison<T> comparison)
    {
        if (comparison == null) throw new ArgumentNullException("comparison");
        _compareFunction = comparison;
    }

    #endregion

    public override int Compare(T arg1, T arg2)
    {
        return _compareFunction(arg1, arg2);
    }
}
