﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace RDPSlave
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// creates viewmodel and passes provided startupArgs, shows mainwindow if needed, or shuts application down, after hidden operations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            RDPSlaveViewModel model = new RDPSlaveViewModel(this, e.Args);
            
            if (!model.isSilentProcessing)
            {
                MainWindow mw = new MainWindow();

                mw.DataContext = model;
                mw.Show();
            }
            else
            {
                this.Shutdown();
            }
        }
    }
}
