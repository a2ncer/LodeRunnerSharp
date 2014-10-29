using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace Loderunner
{
    public class BotBase
    {
        protected const string RIGHT = "RIGHT";
        protected const string LEFT = "LEFT";
        protected const string UP = "UP";
        protected const string DOWN = "DOWN";

        protected int MAX_TARGET_GOLDS = 5;

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
        private List<Step> _route;
        private Point? _heroNextExpectedPoint;
        private string _lastCommand;
        
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
            if (_me!=null)
            {
                return _me.Item1;
            }
            else
            {
                return new Point(-1,-1);
            }
        }

        public virtual string NextCommand(StringBuilder log)
        {
            BuildRoute(log);            

            // TODO: refactor, too complicated...
            if (_route != null)
            {
                var step = _route.First();

                var cmd = step.Info.Command.First();
                step.Info.Command.RemoveAt(0);
                if (!step.Info.Command.Any())
                {
                    _route.RemoveAt(0);
                    _heroNextExpectedPoint = step.Point;
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
            if (_heroNextExpectedPoint!=null && _heroNextExpectedPoint!=_me.Item1)
            {
                log.AppendLine(string.Format("Command '{0}' failed. Expected Pos {1}. Actual Pos {2}", _lastCommand, _heroNextExpectedPoint.Value, _me.Item1));
                _route = null;
            }

            if (_route!=null)
            {
                // reset route if someone stolen the gold
                var target = _route.Last();
                if (target.Info.ExpectedChar != _board[target.Point.X, target.Point.Y])
                {
                    log.AppendLine("Gold was stolen");
                    _route = null;
                }
                else
                {
                    // reset route if enemy is on the route near (5 steps)
                    foreach(var point in _route.Take(5))
                    {
                        if (EnemyChars.Contains(_board[point.Point.X, point.Point.Y]))
                        {
                            log.AppendLine("Enemy is near");
                            _route = null;
                            break;
                        }
                    }
                }
            }

            if (_route==null)
            {
                log.AppendLine("Evaluate new route...");
                var allGolds = SearchAll((c) => c == Gold)
                  .OrderBy(c => GetDistance(c.Item1,_me.Item1))
                  .Select(c=>c.Item1)
                  .ToList();            
            
                // evaluate route to near gold
                _route = EvaluateBestRoute(allGolds.Take(MAX_TARGET_GOLDS).ToList());
                if (_route!=null)
                {
                    log.AppendLine("Route is found for near point");                
                }

                if (_route == null)
                {
                    // if can't find route to near - try all other golds (may be slow)
                    _route = EvaluateBestRoute(allGolds.Skip(MAX_TARGET_GOLDS).ToList());
                    if (_route!=null)
                    {
                        log.AppendLine("Route is found for far point");  
                    }
                }
            }
            
            if (_route !=null)
            {
                var target = _route.Last();
                log.AppendLine(string.Format("Target: {0}, ({1}) Steps: {2}", target.Info.ExpectedChar, target.Point.ToString(), _route.Count));
                foreach (var x in _route)
                {
                    log.AppendLine(string.Format("Command: {0}, ExpChar: {1}", string.Join("|", x.Info.Command), x.Info.ExpectedChar));
                }
            }
        }

        protected Tuple<Point, char> Search(Func<char,bool> whatToSearch)
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
        
        protected bool CanLocatedOn(char c)
        {
            return c == Space || c == Pipe || c == Gold || c == Ladder || c == DrillSpace || c == DrillPit;
        }

        protected bool IsGround(char c)
        {
            return c == Wall || c == Brick || c == Ladder;
        }

        protected bool IsAnchorOrOnTheGround(char c, char bottomChar)
        {
            return c==Pipe || c==Ladder || c==HeroOnLadder || HeroOnPipe.Contains(c) || HeroStands.Contains(c) || HeroDrill.Contains(c)
                   || 
                   IsGround(bottomChar);
        }

        protected internal bool CanRight(Point from, char fromChar, out string command) 
        {
            var rightChar = _board[from.X + 1, from.Y];
            var bottomChar = _board[from.X, from.Y + 1];
            
            if (CanLocatedOn(rightChar) && IsAnchorOrOnTheGround(fromChar, bottomChar))
            {
                command = RIGHT;
                return true;
            }

            command = null;
            return false;
        }

        protected internal bool CanLeft(Point from, char fromChar, out string command)
        {
            var leftChar = _board[from.X - 1, from.Y];
            var bottomChar = _board[from.X, from.Y + 1];
            
            if (CanLocatedOn(leftChar) && IsAnchorOrOnTheGround(fromChar, bottomChar))
            {
                command = LEFT;
                return true;
            }

            command = null;
            return false;
        }

        protected internal bool CanUp(Point from, char fromChar, out string command)
        {
            var upItem = _board[from.X, from.Y - 1];
            if (CanLocatedOn(upItem) && ((fromChar=='Y') || fromChar==Ladder))
            {
                command = UP;
                return true;
            }

            command = null;
            return false;
        }

        protected internal bool CanDown(Point from, out string command)
        {
            var downItem = _board[from.X, from.Y + 1];
            if (CanLocatedOn(downItem))
            {
                command = DOWN;
                return true;
            }

            command = null;
            return false;
        }

        protected internal bool CanRightDown(Point from, char fromChar, out string[] command)
        {
            var rightDownChar = _board[from.X + 1, from.Y + 1];
            var rightChar = _board[from.X + 1, from.Y];
            var bottomChar = _board[from.X, from.Y+1];

            string cmd;
            if ((fromChar == ' ' || fromChar == Gold || HeroStands.Contains(fromChar)) && IsGround(bottomChar) && rightDownChar == Brick && (rightChar == ' ' || rightChar == Gold))
            {
                command = new string[] { "ACT," + RIGHT, RIGHT, DOWN };
                return true;
            }

            command = null;
            return false;
        }

        protected internal bool CanLeftDown(Point from, char fromChar, out string[] command)
        {
            var leftDownChar = _board[from.X - 1, from.Y + 1];
            var leftChar = _board[from.X - 1, from.Y];
            var bottomChar = _board[from.X, from.Y + 1];

            string cmd;
            if ((fromChar == ' ' || fromChar == Gold || HeroStands.Contains(fromChar)) && IsGround(bottomChar) && leftDownChar == Brick && (leftChar == ' ' || leftChar == Gold))
            {
                command = new string[] { "ACT," + LEFT, LEFT, DOWN };
                return true;
            }

            command = null;
            return false;
        }

        protected Point GetRightPoint(Point from)
        {
            return new Point(from.X + 1, from.Y);
        }

        protected Point GetLeftPoint(Point from)
        {
            return new Point(from.X - 1, from.Y);
        }
        
        protected Point GetUpPoint(Point from)
        {
            return new Point(from.X, from.Y-1);
        }
        
        protected Point GetDownPoint(Point from)
        {
            return new Point(from.X, from.Y+1);
        }
        
        public List<Step> EvaluateBestRoute(List<Point> targets)
        {
            List<List<Step>> routesToTarget = new List<List<Step>>();
            foreach (var target in targets)
            {
                var route = EvaluateBestRouteTo(target);
                if (route != null)
                {
                    routesToTarget.Add(route);
                }
            }

            var shortestRoute = routesToTarget.OrderBy(route=>route.SelectMany(x=>x.Info.Command).Count()).FirstOrDefault();
            return shortestRoute;
        }

        protected List<Step> EvaluateBestRouteTo(Point target)
        {
            var rootNode = new GraphNode();
            rootNode.Pos = _me.Item1;
            rootNode.DistanceToTarget = GetDistance(rootNode.Pos, target);
            rootNode.Info = new StepInfo() { ExpectedChar = _board[_me.Item1.X, _me.Item1.Y] };

            SearchTargetRecursive(rootNode, target, new HashSet<Point>());

            List<Step> outRoute = new List<Step>();
            if (rootNode.RouteFound)
            {
                GetShortestRoute(rootNode, target, outRoute);
                outRoute.Reverse();
                return outRoute.Skip(1).ToList();
            }

            return null;
        }

        /// <summary>
        /// Traverse tree until fisrt endpoint found 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="target"></param>
        /// <param name="outListReversed"></param>
        /// <returns></returns>
        private bool GetShortestRoute(GraphNode node, Point target, List<Step> outListReversed)
        {
            if (node.Pos==target)
            {
                outListReversed.Add(new Step() {Point=node.Pos, Info = node.Info});
                return true;
            }

            foreach(var n in node.Tree.Where(s=>s.RouteFound))
            {
                if (GetShortestRoute(n, target, outListReversed))
                {
                    outListReversed.Add(new Step() {Point=node.Pos, Info = node.Info});
                    return true;
                }
            }

            return false;
        }
        
        protected bool SearchTargetRecursive(GraphNode prevNode, Point target, HashSet<Point> visitedPoints)
        {
            if (prevNode.Pos == target) 
            {
                prevNode.RouteFound = true;
                return true;
            }

            Point prevPoint = prevNode.Pos;            
            visitedPoints.Add(prevPoint);

            Point p;
            string command;
            string[] commands;

            p = GetUpPoint(prevPoint);
            if (!visitedPoints.Contains(p) && CanUp(prevPoint, prevNode.Info.ExpectedChar, out command))
            {
                var node = new GraphNode();
                node.Pos = p;
                node.DistanceToTarget = GetDistance(node.Pos, target);
                node.Info = new StepInfo();
                node.Info.Command = new[] { command }.ToList();
                node.Info.ExpectedChar = _board[node.Pos.X, node.Pos.Y];

                prevNode.Tree.Add(node);
            }

            p = GetRightPoint(prevPoint);            
            if (!visitedPoints.Contains(p) && CanRight(prevPoint, prevNode.Info.ExpectedChar, out command))
            {
                var node = new GraphNode();
                node.Pos = p;
                node.DistanceToTarget = GetDistance(node.Pos, target);
                node.Info = new StepInfo();
                node.Info.Command = new[] { command }.ToList();
                node.Info.ExpectedChar = _board[node.Pos.X, node.Pos.Y];

                prevNode.Tree.Add(node);
            }

            p = GetLeftPoint(prevPoint);
            if (!visitedPoints.Contains(p) && CanLeft(prevPoint, prevNode.Info.ExpectedChar, out command))
            {
                var node = new GraphNode();
                node.Pos = p;
                node.DistanceToTarget = GetDistance(node.Pos, target);
                node.Info = new StepInfo();
                node.Info.Command = new[] { command }.ToList();
                node.Info.ExpectedChar = _board[node.Pos.X, node.Pos.Y];

                prevNode.Tree.Add(node);
            }
            
            p = GetDownPoint(prevPoint);
            if (!visitedPoints.Contains(p) && CanDown(prevPoint, out command))
            {
                var node = new GraphNode();
                node.Pos = p;
                node.DistanceToTarget = GetDistance(node.Pos, target);
                node.Info = new StepInfo();
                node.Info.Command = new[] { command }.ToList();
                node.Info.ExpectedChar = _board[node.Pos.X, node.Pos.Y];

                prevNode.Tree.Add(node);
            }

            p = GetDownPoint(prevPoint);
            p = new Point(p.X+1, p.Y); //down right
            if (!visitedPoints.Contains(p) && CanRightDown(prevPoint,prevNode.Info.ExpectedChar, out commands))
            {
                var node = new GraphNode();
                node.Pos = p;
                node.DistanceToTarget = GetDistance(node.Pos, target);
                node.Info = new StepInfo();
                node.Info.Command = commands.ToList();
                node.Info.ExpectedChar = _board[node.Pos.X, node.Pos.Y];

                prevNode.Tree.Add(node);
            }

            p = GetDownPoint(prevPoint);
            p = new Point(p.X - 1, p.Y); //down left
            if (!visitedPoints.Contains(p) && CanLeftDown(prevPoint,prevNode.Info.ExpectedChar, out commands))
            {
                var node = new GraphNode();
                node.Pos = p;
                node.DistanceToTarget = GetDistance(node.Pos, target);
                node.Info = new StepInfo();
                node.Info.Command = commands.ToList();
                node.Info.ExpectedChar = _board[node.Pos.X, node.Pos.Y];

                prevNode.Tree.Add(node);
            }

            var routeFound = false;
            foreach(var node in prevNode.Tree.OrderBy(x => x.Info.Command.Count + x.DistanceToTarget))
            {
                routeFound |= SearchTargetRecursive(node,target, visitedPoints);
            }

            prevNode.RouteFound = routeFound;

            return routeFound;
        }

        protected int GetDistance(Point from, Point to)
        {
            return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
        }
    }

    public class GraphNode
    {
        public StepInfo Info;
        public Point Pos;
        public int DistanceToTarget;
        public List<GraphNode> Tree = new List<GraphNode>(); // max three nodes
        public bool RouteFound;
    }

    public class Step
    {
        public StepInfo Info;
        public Point Point;
    }

    public class StepInfo
    {
        public char ExpectedChar { get; set; }        
        public List<string> Command { get; set; }
        
    }
}
