using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Loderunner;
using Point=System.Drawing.Point;

namespace Tests
{
    [TestClass]
    public class DirectionAccessibilityTests
    {
        BotBase bot = new BotBase();

        public DirectionAccessibilityTests()
        {
            
        }

        [TestMethod]
        public void CanMoveRight()
        {
            AssertCanRight(new [] {
                                    ' ',' ',' ',
                                    ' ','►',' ',
                                    ' ','#',' ',
                                  }
                                  , true); 
            AssertCanRight(new [] {
                                    ' ',' ',' ',
                                    ' ','►','$',
                                    ' ','#',' ',
                                  }
                                  , true); 
            AssertCanRight(new [] {
                                    ' ',' ',' ',
                                    ' ','►','~',
                                    ' ','#',' ',
                                  }
                                  , true); 
            AssertCanRight(new [] {
                                    ' ',' ',' ',
                                    ' ','►','H',
                                    ' ','#',' ',
                                  }
                                  , true); 
            AssertCanRight(new [] {
                                    ' ',' ',' ',
                                    ' ','R',' ',
                                    ' ','#','#',
                                  }
                                  , true); 
            AssertCanRight(new [] {
                                    ' ',' ',' ',
                                    ' ','Я',' ',
                                    ' ','#','#',
                                  }
                                  , true);    
            AssertCanRight(new [] {
                                    ' ',' ',' ',
                                    ' ','Y',' ',
                                    ' ','#','#',
                                  }
                                  , true); 
            AssertCanRight(new [] {
                                    ' ',' ',' ',
                                    ' ','►','#',
                                    ' ','#',' ',
                                  }
                                  , false); 
            AssertCanRight(new [] {
                                    ' ',' ',' ',
                                    ' ','►','☼',
                                    ' ','#',' ',
                                  }
                                  , false);
            AssertCanRight(new [] {
                                    ' ',' ',' ',
                                    ' ','~',' ',
                                    ' ',' ',' ',
                                  }
                                  , true);
        }

        private void AssertCanRight(char[] boardArray, bool expectedResult)
        {
            var board = board3x3(boardArray);
            var c = board[1,1];
            var p = new Point(1,1);
            bot.SetBoard(board);
            string cmd;
            var r = bot.CanRight(p,c,out cmd);
            Assert.AreEqual(expectedResult,r);
        }

        private static char[,] board3x3(char[] boardArray)
        {
            int dim = 3;
            char[,] board = new char[dim, dim];
            for(int y=0;y<dim;++y)
            {
                for(int x=0;x<dim;++x)
                {
                    board[x,y]=boardArray[y*dim+x];
                }
            }
            return board;
        }
    }
}
