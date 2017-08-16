﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfIATCSharp
{
    /// <summary>
    /// Feedback.xaml 的交互逻辑
    /// </summary>
    public partial class Feedback : Window
    {
        delegate void MyDelegate(string value);
        private bool first_time = true;
        public string last_character;
        public string service;
        private string global_value = "";
        private int row = 0;
        private bool wait = false;
        private int i = 0;
        private int maxLength = 0;

        private void RefreshWindow(double width, double height)
        {
            Left = 0;
            //Left = System.Windows.SystemParameters.PrimaryScreenWidth - (System.Windows.SystemParameters.PrimaryScreenWidth / 2) - width / 2;
            Top = System.Windows.SystemParameters.PrimaryScreenHeight - height - 40 - 2;
        }

        public Feedback()
        {
            InitializeComponent();

            RefreshWindow(500,145);
            Width = System.Windows.SystemParameters.PrimaryScreenWidth; //originally this line no exist

        /**/this.ShowInTaskbar = false;
            this.Topmost = true;
            this.MouseDown += new MouseButtonEventHandler(Window_MouseDown);
            //this.MouseMove += new MouseEventHandler(Window_MouseMove);
            //this.MouseLeave += new MouseEventHandler(Window_MouseLeave);
            
            //this.txtContent.TextChanged += new TextChangedEventHandler(txtContent_changeHeight); //梦龙：Need to be improved!

            ContextMenu mMenu = new ContextMenu();
            MenuItem closeMenu = new MenuItem();
            closeMenu.Header = "关闭";
            closeMenu.Click += closeMenu_Click;
            mMenu.Items.Add(closeMenu);
            txtContent.ContextMenu = mMenu;
            
            Thread receiveDataThread = new Thread(new ThreadStart(ReceiveDataFromClient));
            receiveDataThread.IsBackground = true;
            receiveDataThread.Start();
        }

        public void closeMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void setValue(string value)
        {
            //this.txtContent.Text = value;
            
            //Option A Movies sub
            if (i > 1)
            {
                this.txtContent.LineDown();
            }
            else
            {
                if (first_time) { this.txtContent.Clear(); first_time = false; this.txtContent.AppendText(value); }
                else { this.txtContent.AppendText("\n" + value); this.txtContent.LineDown(); }
            }
        }

        private void ReceiveDataFromClient()
        {
            MyDelegate d = new MyDelegate(setValue);
            while (true)
            {
                try
                {
                    NamedPipeServerStream _pipeServer = new NamedPipeServerStream("closePipe", PipeDirection.InOut, 2);
                    _pipeServer.WaitForConnection();
                    StreamReader sr = new StreamReader(_pipeServer);
                    string recData = sr.ReadLine();

                    if (recData.Length > 1)
                    {
                        Debug.WriteLine(recData);
                        this.Dispatcher.Invoke(d, recData);

                        //if (service == "SessionBeginTranslateCntoEn") { maxLength = 51; } //Maximum English characters my screen can show in one line 
                        //else if (service == "SessionBeginTranslateEntoCn") { maxLength = 26; } //Maximum Chinese characters my screen can show in one line
                        maxLength = 51;

                        i = recData.Length / maxLength + 1;
                        Debug.WriteLine("Length: {0} \nRows: {1}",recData.Length,i);
                        if (i > 1)
                        {
                            Thread.Sleep(3000);
                            while (i > 1) { this.Dispatcher.Invoke(d, recData); Thread.Sleep(3000); i--; }
                        }
                    }

                    Thread.Sleep(1000);
                    sr.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var ColorBrush = new SolidColorBrush(Color.FromScRgb(0, 255, 255, 255));
            this.Background = ColorBrush;
        }

        //Window_MouseLeave
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            var ColorBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            this.Background = ColorBrush;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void txtContent_changeHeight(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                Debug.WriteLine("global_value.Lenght = "+global_value.Length);
                if ((sender as TextBox).Text != string.Empty && global_value.Length / 51 > 2) //梦龙: 51 is the max. # of characters one line can show (actually is a bit more but to be safe)
                {
                    row = global_value.Length / 51  + 1;
                    this.Height = row * (145 / 2);

                    RefreshWindow(500, this.Height);
                    wait = true;
                }
                else if (row >= 3)
                {
                    this.Height = row * (145 / 2);
                    RefreshWindow(500, this.Height);
                    wait = true;
                }
                else
                {
                    this.Height = 145;
                    //this.Width = 500;
                    RefreshWindow(500, 145);
                    wait = false;
                }
            }
        }

        private void txtContent_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.Background = Brushes.Blue;
                this.Opacity = 0.3;

                DragMove();

                var height = System.Windows.SystemParameters.PrimaryScreenHeight;
                var width = System.Windows.SystemParameters.PrimaryScreenWidth;

                if (this.Left < 0)
                    this.Left = 0;
                if (this.Top < 0)
                    this.Top = 0;
                if (this.Top + this.Height > height)
                    this.Top = height - this.Height;
                if (this.Left + this.Width > width)
                    this.Left = width - this.Width;
            }

            Thread.Sleep(400);

            this.Background = null;
            this.Opacity = 1;
        }
    }
}
