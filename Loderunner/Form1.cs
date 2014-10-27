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
    public partial class Form1 : Form
    {
        WebSocket ws = new WebSocket("ws://tetrisj.jvmhost.net:12270/codenjoy-contest/ws?user=AVAG");

        BotBase bot = new BotBase();
        
        public Form1()
        {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);
            
            ws.OnMessage += (sender, e) =>
            {
                string msg = e.Data;
                //msg = "board=☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼$                            ~~~~~~~~~               $$ ☼☼##H########################H#H       H##########H       ☼☼  H                        H######H  H        ( H#☼☼☼☼☼☼☼☼H☼☼#☼☼H    H#########H     H#   $ H#####H#####H##$    $ ☼☼H     H    H $     $ H#####H#)    H     H   $ H $~~     ☼☼H#☼#☼#H    H         H   $  #####H#     H     H  $$~~  $☼☼H $ $ H~~Є~H~~~~~~   H         $ H   H######H## $ $  ~~ ☼☼H     H    H     H###☼☼☼☼☼☼H☼    H~~~H      H          #☼☼H     H    H#####H      »  H     H      H#########H    $☼☼☼###☼##☼##☼H         H###H##    H##     H#       ##     ☼☼☼###☼ $    H   $     H $ H######H######### H###H #####H#☼☼☼$$$☼  (   H   ~~~~~~H   H      H          H# #H  $   H(☼☼########H###☼☼☼☼     H  ############ $ ###### ##########☼☼        H            H                                  ☼☼H##########################H########~~~####H############☼☼H                  $ $     H       $       H            ☼☼#######H#######      $     H###~~~~     $############H  ☼☼       H~~~~~~~~~~         H$  ►         $           H  ☼☼       H    ##H $ #######H##########~~~~~~~H######## H  ☼☼       H    ##H          H        $        H         H  ☼☼##H#####    ########H#######~~~~  ~~~#########~~~~~  H  ☼☼  H            (    H   $          $   $         ~~~~H  ☼☼#########H##########H    $   #☼☼☼☼☼☼# $ ☼☼☼☼☼☼☼      H  ☼☼         H      $   H        ~      ~         «      H  ☼☼☼☼       H~~~~~~~~~~H  (      ######$  ###########   H  ☼☼    H######         #######H           ~~~~~~~~~~~~~~H  ☼☼H☼  H                      H «H####H                 H  ☼☼H☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼###☼☼☼☼☼☼☼☼H☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼#☼☼H$$ $$       ~~H~~~~☼☼☼☼☼☼☼H☼☼☼☼☼☼☼$       H  ~~~~~~~~~H☼☼H~~~~  ######  H       $ H☼H☼H        #####H ☼  (      H☼☼H      $       ##H#######H☼H☼H######H $    ##☼☼☼☼☼☼☼☼ ~H☼☼H#########       H    ~~~H☼H☼H~~~   H~~~~~  #        ~ H☼☼H    $   ###H####H##H     ☼H☼       H     ###☼☼☼☼☼☼ ~  H☼☼H$$  $$ $$  H      #######☼H☼#####  H#####   ~~~~~~~ ~ H☼☼~~~~~~~~~~~~H       H~~~~~☼H☼~~~~~  H             ~ ~  H☼☼$    H«             H     ☼H☼     ##########H          H☼☼ ### #############H H#####☼H☼               H ######## H☼☼H                 H      $☼H☼#######        H          H☼☼H#####         H##H####                ###H#########   H☼☼H   $  H######### H   ############        H            H☼☼H##    H       $$ H~~~~~~                 H #######H## H☼☼~~~~#####H#   ~~~~H         ########H     H        H   H☼☼         H        H      ~~~~~~~~   H     H        H   H☼☼   ########H    ######H##        ##############    H   H☼☼           H          H «      ~~~~~           ##H#####H☼☼H    ###########H     H#####H         H##H       H     H☼☼H###            H  (  H  $  ###########  ##H###  H     U☼☼H  ######  ##H######  H             (      H   ##H###  H☼☼H            H ~~~~~##H###H     #########H##           H☼☼    H########H#       H   ######         H             H☼☼ ###H        H         ~~~~~H      ##H###H####H###     H☼☼    H########H#########     H        H        H     $  H☼☼H   H                    (  H        H        H        H☼☼H  ####H######         #####H#3######H##      H#####   H☼☼H      H      H#######H                       H        H☼☼##############H       H#################################☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼";
                
                var board = ParseBoardMessage(msg);

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                bot.SetBoard(board);
                var cmd = bot.NextCommand();
                sw.Stop();

                UIDispatch(() =>
                {
                    richTextBox2.Text = sw.ElapsedMilliseconds.ToString();
                });


                ws.Send(cmd);

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

                    richTextBox1.Text = sb.ToString();
                });
            };

            ws.OnError += new EventHandler<ErrorEventArgs>(ws_OnError);

            ws.Connect();



        }

        char[,] ParseBoardMessage(string message)
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
            if (this.InvokeRequired)
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
