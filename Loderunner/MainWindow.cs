using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WebSocketSharp;

namespace Loderunner
{
    public partial class MainWindow : Form
    {
        WebSocket ws = new WebSocket("ws://tetrisj.jvmhost.net:12270/codenjoy-contest/ws?user=aaa");

        BotBase bot = new BotBase();
        
        public MainWindow()
        {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);

            ws.OnMessage += ws_OnMessage;
            ws.OnError += ws_OnError;
            ws.Connect();
        }

        void ws_OnMessage(object sender, MessageEventArgs e)
        {           
            string msg = e.Data;
            //msg = "board=☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼$                            ~~~~~~~~~               $$ ☼☼##H########################H#H       H##########H       ☼☼  H                        H######H  H        ( H#☼☼☼☼☼☼☼☼H☼☼#☼☼H    H#########H     H#   $ H#####H#####H##$    $ ☼☼H     H    H $     $ H#####H#)    H     H   $ H $~~     ☼☼H#☼#☼#H    H         H   $  #####H#     H     H  $$~~  $☼☼H $ $ H~~Є~H~~~~~~   H         $ H   H######H## $ $  ~~ ☼☼H     H    H     H###☼☼☼☼☼☼H☼    H~~~H      H          #☼☼H     H    H#####H      »  H     H      H#########H    $☼☼☼###☼##☼##☼H         H###H##    H##     H#       ##     ☼☼☼###☼ $    H   $     H $ H######H######### H###H #####H#☼☼☼$$$☼  (   H   ~~~~~~H   H      H          H# #H  $   H(☼☼########H###☼☼☼☼     H  ############ $ ###### ##########☼☼        H            H                                  ☼☼H##########################H########~~~####H############☼☼H                  $ $     H       $       H            ☼☼#######H#######      $     H###~~~~     $############H  ☼☼       H~~~~~~~~~~         H$  ►         $           H  ☼☼       H    ##H $ #######H##########~~~~~~~H######## H  ☼☼       H    ##H          H        $        H         H  ☼☼##H#####    ########H#######~~~~  ~~~#########~~~~~  H  ☼☼  H            (    H   $          $   $         ~~~~H  ☼☼#########H##########H    $   #☼☼☼☼☼☼# $ ☼☼☼☼☼☼☼      H  ☼☼         H      $   H        ~      ~         «      H  ☼☼☼☼       H~~~~~~~~~~H  (      ######$  ###########   H  ☼☼    H######         #######H           ~~~~~~~~~~~~~~H  ☼☼H☼  H                      H «H####H                 H  ☼☼H☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼###☼☼☼☼☼☼☼☼H☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼#☼☼H$$ $$       ~~H~~~~☼☼☼☼☼☼☼H☼☼☼☼☼☼☼$       H  ~~~~~~~~~H☼☼H~~~~  ######  H       $ H☼H☼H        #####H ☼  (      H☼☼H      $       ##H#######H☼H☼H######H $    ##☼☼☼☼☼☼☼☼ ~H☼☼H#########       H    ~~~H☼H☼H~~~   H~~~~~  #        ~ H☼☼H    $   ###H####H##H     ☼H☼       H     ###☼☼☼☼☼☼ ~  H☼☼H$$  $$ $$  H      #######☼H☼#####  H#####   ~~~~~~~ ~ H☼☼~~~~~~~~~~~~H       H~~~~~☼H☼~~~~~  H             ~ ~  H☼☼$    H«             H     ☼H☼     ##########H          H☼☼ ### #############H H#####☼H☼               H ######## H☼☼H                 H      $☼H☼#######        H          H☼☼H#####         H##H####                ###H#########   H☼☼H   $  H######### H   ############        H            H☼☼H##    H       $$ H~~~~~~                 H #######H## H☼☼~~~~#####H#   ~~~~H         ########H     H        H   H☼☼         H        H      ~~~~~~~~   H     H        H   H☼☼   ########H    ######H##        ##############    H   H☼☼           H          H «      ~~~~~           ##H#####H☼☼H    ###########H     H#####H         H##H       H     H☼☼H###            H  (  H  $  ###########  ##H###  H     U☼☼H  ######  ##H######  H             (      H   ##H###  H☼☼H            H ~~~~~##H###H     #########H##           H☼☼    H########H#       H   ######         H             H☼☼ ###H        H         ~~~~~H      ##H###H####H###     H☼☼    H########H#########     H        H        H     $  H☼☼H   H                    (  H        H        H        H☼☼H  ####H######         #####H#3######H##      H#####   H☼☼H      H      H#######H                       H        H☼☼##############H       H#################################☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼";
                
            var board = ParseBoardMessage(msg);

            UIDispatch(() =>
            {
                StringBuilder sb = new StringBuilder();
                for (int y = 0; y < board.GetLength(1); ++y )
                {
                    if (y > 0)
                    {
                        sb.Append(Environment.NewLine);
                    }
                    for (int x = 0; x < board.GetLength(0); ++x)
                    {
                        sb.Append(board[x,y]);
                    }
                }
                rtbBoard.Text = sb.ToString();                    
            });  

            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            StringBuilder log = new StringBuilder();
            bot.SetBoard(board);
            string cmd = bot.NextCommand(log);
            sw.Stop();
            
            ws.Send(cmd);          
          
            UIDispatch(() =>
            {
                tbEvalTime.Text = sw.ElapsedMilliseconds.ToString();
                rtbLog.Text = log.ToString();
                tbHeroPos.Text = bot.GetHeroPosition().ToString();
            });
        }

        private char[,] ParseBoardMessage(string message)
        {
            char[,] board;
            if (message.StartsWith("board="))
            {
                string boardStr = message.Substring(6, message.Length - 6);
                double d = Math.Sqrt(boardStr.Length);
                int dimention = (int)d;
                if (d != dimention)
                {
                    throw new Exception("Invalid board message length");
                }

                board = new char[dimention, dimention];

                int boardStrIterator = 0;
                for (int y = 0; y < dimention; ++y)
                {
                    for (int x = 0; x < dimention; ++x)
                    {
                        board[x, y] = boardStr[boardStrIterator++];
                    }
                }
            }
            else
            {
                throw new Exception("Invalid message type");
            }

            return board;
        }


        void UIDispatch(Action action)
        {
            if (this.InvokeRequired && !this.IsDisposed && !this.Disposing)
            {
                this.Invoke(action);
            }
            else
            {
                action();
            }
        }

        void ws_OnError(object sender, ErrorEventArgs e)
        {

        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ws.Close();
        }

        private void up_Click(object sender, EventArgs e)
        {
            ws.Send("UP");
        }

        private void down_Click(object sender, EventArgs e)
        {
            ws.Send("DOWN");
        }

        private void right_Click(object sender, EventArgs e)
        {
            ws.Send("RIGHT");
        }

        private void left_Click(object sender, EventArgs e)
        {
            ws.Send("LEFT");
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
