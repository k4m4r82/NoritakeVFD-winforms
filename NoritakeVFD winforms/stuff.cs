﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoritakeVFD_winforms
{
    class Stuff
    {
        public static byte left = 0, right = 0;
        public static int cursorPosition = 0;
        static bool flag = true;

        //converts the cursor position in dec ascii format, based on the textbox's length. 
        //example: 1 = 49 / 15 = 49, 53
        public static void CurPos2Hex(int curPos)
        {
            int left = 0, right = 0;

            if (curPos < 10)    //if the length is less than 9...
            {
                left = curPos + 49; //...convert it to ascii 
                if (left == 58)     //if the position is 9, the ascii equivalent isn't 0 but ':'
                {
                    left = 49;      //time to fix that 
                    right = 48;
                }
            }
            else if (curPos > 9 && curPos < 20)
            {
                left = 49;
                right = (curPos % 10) + 49;
                if (right == 58)
                {
                    left = 50;
                    right = 48;
                }
            }
            else
            {
                left = 50;
                right = 48;

            }

            Stuff.left = (byte)left;
            Stuff.right = (byte)right;
            cursorPosition = curPos++;
        }

        public class Serial
        {
            public static bool connected = false;
            public static SerialPort uart = new SerialPort();
            public static System.Collections.ArrayList portlist = new System.Collections.ArrayList();

            public static List<byte[]> charsets = new List<byte[]>
            {
                new byte [] { 0x1B, 0x52, 0x00 },
                new byte [] { 0x1B, 0x52, 0x01 },
                new byte [] { 0x1B, 0x52, 0x02 },
                new byte [] { 0x1B, 0x52, 0x03 },
                new byte [] { 0x1B, 0x52, 0x04 },
                new byte [] { 0x1B, 0x52, 0x05 },
                new byte [] { 0x1B, 0x52, 0x06 },
                new byte [] { 0x1B, 0x52, 0x07 },
                new byte [] { 0x1B, 0x52, 0x08 },
                new byte [] { 0x1B, 0x52, 0x09 },
                new byte [] { 0x1B, 0x52, 0x0A },
                new byte [] { 0x1B, 0x52, 0x0B },
                new byte [] { 0x1B, 0x52, 0x0C },
                new byte [] { 0x1B, 0x52, 0x30 },
                new byte [] { 0x1B, 0x52, 0x31 },
                new byte [] { 0x1B, 0x52, 0x32 },
                new byte [] { 0x1B, 0x52, 0x33 },
                new byte [] { 0x1B, 0x52, 0x35 },
                new byte [] { 0x1B, 0x52, 0x37 },
                new byte [] { 0x1B, 0x52, 0x36 },
                new byte [] { 0x1B, 0x52, 0x38 },
                new byte [] { 0x1B, 0x52, 0x63 }
            };


            public static void GetPorts()
            {
                foreach (var port in SerialPort.GetPortNames())
                {
                    portlist.Add(port);
                }
            }

            public static void Connect()
            {
                Form1.form1.portBox.Enabled = false;
                Form1.form1.baudBox.Enabled = false;
                Form1.form1.openport.Text = "Close Port";
                try
                {
                    uart.PortName = Form1.form1.portBox.Text;            //open port and send "ping"
                    uart.BaudRate = int.Parse(Form1.form1.baudBox.Text);
                    uart.Open();

                    Form1.form1.openport.BackColor = System.Drawing.Color.LightGreen;
                }

                catch (System.Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message + '\n' + "Is the display connected in port " + uart.PortName + "?");
                }
            }

            public static void Disconnect()
            {
                uart.Close();
                Form1.form1.openport.BackColor = System.Drawing.Color.Transparent;
                Form1.form1.portBox.Enabled = true;
                Form1.form1.baudBox.Enabled = true;
                Form1.form1.openport.Text = "Connect";
                connected = false;
            }

            public static void DisplayBackspace()
            {
                byte[] command = new byte[1] { 0x08 };

                if (cursorPosition == 20)
                {
                    uart.Write(" ");
                }
                else
                {
                    uart.Write(command, 0, 1);
                    uart.Write(" ");
                    uart.Write(command, 0, 1);
                }
            }
            public static void DisplaySetCurPos(byte line, byte col)
            {
                byte[] command = new byte[6] { 0x1B, 0x5B, line, 0x3B, col, 0x48 }; //command: ESC[<line>;<column number>H
                uart.Write(command, 0, 6);
            }
            public static void DisplaySetCurPos(byte line, byte colH, byte colL)
            {
                byte[] command = new byte[7] { 0x1B, 0x5B, line, 0x3B, colH, colL, 0x48 }; //command: ESC[<line>;<column number 1><column number 2>H
                uart.Write(command, 0, 7);
            }
            public static void DisplayClearScreen()
            {
                byte[] command = new byte[4] { 0x1B, 0x5B, 0x32, 0x4A };
                uart.Write(command, 0, 4);
                DisplaySetCursorToLine1();

                Form1.form1.textBox1.Clear();
                Form1.form1.textBox2.Clear();

                Form1.form1.textBox1.Focus();
            }
            public static void DisplaySetCursorToLine1()
            {
                byte[] command = new byte[6] { 0x1B, 0x5B, 0x31, 0x3B, 0x31, 0x48 };
                uart.Write(command, 0, 6);

                Form1.form1.textBox1.Focus();
            }
            public static void DisplaySetCursorToLine2()
            {
                byte[] command = new byte[6] { 0x1B, 0x5B, 0x32, 0x3B, 0x31, 0x48 };
                uart.Write(command, 0, 6);

                Form1.form1.textBox2.Focus();
            }
            /// <param name="spaces">Number of " " between messages.</param>
            /// <param name="direction">true for left to right, false for the opposite.</param>
            public static void DisplayScrollMessage(int spaces, bool direction, string Line1Message, string Line2Message)
            {

            }
            /// <param name="message">Include the spaces in the string plox.</param>
            public static void DisplayFlashMessage(string message)
            {
                if (flag)
                {
                    uart.Write(message);

                    flag = !flag;
                }
                else
                {
                    DisplayClearScreen();

                    flag = !flag;
                }
            }
            public static void DisplayFlashMessage(string Line1Message, string Line2Message)
            {
                if (flag)
                {
                    uart.Write(Line1Message);
                    DisplaySetCursorToLine2();
                    uart.Write(Line2Message);
                    
                    flag = !flag;
                }
                else
                {
                    DisplayClearScreen();

                    flag = !flag;
                }
            }
        }
    }
}
