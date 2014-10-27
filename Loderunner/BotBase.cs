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

        protected int MAX_TARGET_GOLDS = 15;

        protected static char[] HeroChars = new[] { '►', '◄', 'Я', 'R', 'Y' , '[', ']', '{', '}', deadHero };
        protected const char deadHero = 'Ѡ';

        protected const char GoldChar = '$';
        protected const char Wall = '☼';
        protected const char Brick = '#';
        protected const char Ladder = 'H';
        protected const char Pipe = '~';

        protected const char DrillSpace = '.';
        protected const char DrillPit = '*';
        protected static char[] PitFill = new[] { '1', '2', '3', '4' };
        
        protected char[,] _board = new char[0, 0];

        protected Tuple<Point, char> me;
             
        protected List<Step> currentRoute;

        public void SetBoard(char[,] board) 
        {
            _board = board;
            me = Search((c) => 
            {
                return HeroChars.Contains(c);
            });
            if (me == null)
            {
                throw new Exception("Hero pos is null");
            }

            var allGolds = SearchAll((c) =>
            {
                return c == GoldChar;
            }).OrderBy(c => 
            {
                return GetDistance(c.Item1,me.Item1);
            }).Select(c=>c.Item1)
              .ToList();
            
            currentRoute = EvaluateBestRoute(allGolds.Take(MAX_TARGET_GOLDS).ToList());

            if (currentRoute != null)
            {


                StringBuilder sb = new StringBuilder();

                sb.AppendLine(string.Format("Target: {0}, ({1},{2})", currentRoute.Last().Info.ExpectedChar, currentRoute.Last().Point.X, currentRoute.Last().Point.Y));

                foreach (var x in currentRoute)
                {
                    sb.AppendLine(string.Format("Command: {0}, ExpChar: {1}", string.Join("|", x.Info.Command), x.Info.ExpectedChar));
                }

                sb.AppendLine("-------------------------------");

                System.Diagnostics.Debug.Write(sb.ToString());
            }
            else
            {
                currentRoute = EvaluateBestRoute(allGolds.Skip(MAX_TARGET_GOLDS).ToList());
            }
            
        }

        public string NextCommand()
        {
            if (currentRoute != null)
            {
                var step = currentRoute.FirstOrDefault();
                if (step != null)
                {
                    
                    var cmd = step.Info.Command.First();
                    step.Info.Command.Remove(cmd);
                    if (!step.Info.Command.Any())
                    {
                        currentRoute.Remove(step);
                    }

                    return cmd;                    
                }
                else
                {
                    currentRoute = null; // route is completed
                }
            }

            return string.Empty;
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
        
        protected bool CanRight(Point from, char fromChar, out string command) 
        {
            var rightItem = _board[from.X + 1, from.Y];
            var bottomItem = _board[from.X, from.Y + 1];
            
            if (
                ((bottomItem == Wall || bottomItem == Brick || bottomItem == GoldChar || fromChar == Pipe || fromChar == Ladder || HeroChars.Contains(fromChar)) && (rightItem == ' ' || rightItem == Ladder || rightItem == Pipe || rightItem == GoldChar)
                )
                )
            {
                command = RIGHT;
                return true;
            }

            command = null;
            return false;
        }

        protected bool CanLeft(Point from, char fromChar, out string command)
        {
            var leftItem = _board[from.X - 1, from.Y];
            var bottomItem = _board[from.X, from.Y + 1];

            if ((bottomItem == Wall || bottomItem == Brick || bottomItem == GoldChar || fromChar == Pipe || fromChar == Ladder || HeroChars.Contains(fromChar)) && (leftItem == ' ' || leftItem == Ladder || leftItem == Pipe || leftItem == GoldChar))
            {
                command = LEFT;
                return true;
            }

            command = null;
            return false;
        }

        protected bool CanUp(Point from, char fromChar, out string command)
        {
            var upItem = _board[from.X, from.Y - 1];
            if ((upItem == Ladder && ((fromChar=='Y') || fromChar==Ladder)) && (upItem == Ladder || upItem == GoldChar || upItem == ' ' || upItem == Pipe))
            {
                command = UP;
                return true;
            }

            command = null;
            return false;
        }

        protected bool CanDown(Point from, out string command)
        {
            var downItem = _board[from.X, from.Y + 1];
            if (downItem == Ladder || downItem == ' ' || downItem == GoldChar || downItem == Pipe)
            {
                command = DOWN;
                return true;
            }

            command = null;
            return false;
        }

        protected bool CanRightDown(Point from, char fromChar, out string[] command)
        {
            var rightDownItem = _board[from.X + 1, from.Y + 1];
            var rightItem = _board[from.X + 1, from.Y];
            string cmd;
            if (CanRight(from, fromChar, out cmd) && fromChar != Pipe && rightDownItem == Brick && (rightItem == ' ' || rightItem == GoldChar))
            {
                command = new string[] { "ACT," + RIGHT, RIGHT, DOWN };
                return true;
            }

            command = null;
            return false;
        }

        protected bool CanLeftDown(Point from, char fromChar, out string[] command)
        {
            var leftDownItem = _board[from.X - 1, from.Y + 1];
            var rightItem = _board[from.X - 1, from.Y];
            string cmd;
            if (CanLeft(from, fromChar, out cmd) && fromChar != Pipe && leftDownItem == Brick && (rightItem == ' ' || rightItem == GoldChar))
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
                var route = EvaluateRouteTo(target);
                if (route != null)
                {
                    routesToTarget.Add(route);
                }
            }

            var shortestRoute = routesToTarget.OrderBy(route=>route.Count).FirstOrDefault();
            return shortestRoute;
        }

        protected List<Step> EvaluateRouteTo(Point target)
        {
            var rootNode = new GraphNode();
            rootNode.Pos = me.Item1;
            rootNode.DistanceToTarget = GetDistance(rootNode.Pos, target);
            rootNode.Info = new StepInfo() { ExpectedChar = _board[me.Item1.X, me.Item1.Y] };

            List<Step> bestRouteReversed = new List<Step>();

            bool routeFound = rrr(rootNode, target, new HashSet<Point>(), bestRouteReversed);

            if (routeFound)
            {
                bestRouteReversed.Reverse();
                return bestRouteReversed;
            }
            return null;
        }



        protected bool rrr(GraphNode prevNode, Point target, HashSet<Point> visitedPoints, List<Step> bestRouteReversed)
        {
            if (prevNode.Pos == target) return true;

            Point prevPoint = prevNode.Pos;
            
            visitedPoints.Add(prevPoint);


            Point p;

            string command;
            string[] commands;
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

            if (prevNode.Tree.Count == 0)
            {
                //visitedPoints.Clear();
                //rrr(prevNode,target,visitedPoints,bestRouteReversed);
            }

            var routeFound = false;
            foreach(var node in prevNode.Tree.OrderBy((x) => x.DistanceToTarget))
            {
                routeFound|= rrr(node,target,visitedPoints, bestRouteReversed);

                if (routeFound)
                {
                    bestRouteReversed.Add(new Step() { Point = node.Pos, Info = node.Info });
                    break;
                }
            }

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
        public bool BestRoute;
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
