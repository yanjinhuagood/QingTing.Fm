﻿using System.Windows;

namespace QingTing.Fm
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static double Wdith
        {
            get { return SystemParameters.WorkArea.Width / 1.5; }
        }
        public static double Height
        {
            get { return SystemParameters.WorkArea.Height / 1.5; }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}
