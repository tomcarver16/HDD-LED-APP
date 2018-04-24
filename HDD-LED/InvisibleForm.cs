using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Specialized;
using System.Threading;

namespace HDD_LED
{
    public partial class InvisibleForm : Form
    {
        #region Globals

        NotifyIcon hddLedIcon;
        Icon activeIcon;
        Icon idleIcon;
        Thread hddGrab;

        #endregion

        #region Main Form

        public InvisibleForm()
        {
            //Hide Form
            InitializeComponent();

            //Grab ico files
            activeIcon = new Icon("HDD_Busy.ico");
            idleIcon = new Icon("HDD_Idle.ico");

            //Set Icons 
            hddLedIcon = new NotifyIcon();
            hddLedIcon.Icon = idleIcon;
            hddLedIcon.Visible = true;

            //Create menu items for the sys tray icon
            MenuItem progNameMenuItem = new MenuItem("HDD Status Indicator - v1.0.0 - Tom Carver");
            MenuItem quitMenuItem = new MenuItem("Quit");

            //Create a menu object for the items to go into then add the menu items to that object
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(progNameMenuItem);
            contextMenu.MenuItems.Add(quitMenuItem);

            //Set the previously created icon to load the context menu
            hddLedIcon.ContextMenu = contextMenu;

            quitMenuItem.Click += QuitMenuItem_Click;

            hddGrab = new Thread(new ThreadStart(HddActivityThread));
            hddGrab.Start();

            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false; 
        }

        #endregion

        #region Event Handlers

        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            hddGrab.Abort();
            hddLedIcon.Dispose();
            this.Close();
        }

        #endregion

        #region Therads

        public void HddActivityThread()
        {
            ManagementClass driveDataClass = new ManagementClass("Win32_PerfFormattedData_PerfDisk_PhysicalDisk");
            try
            {
                while (true)
                {
                    ManagementObjectCollection driveDataClassCollection = driveDataClass.GetInstances();
                    foreach (ManagementObject obj in driveDataClassCollection)
                    {
                        if (obj["Name"].ToString() == "_Total")
                        {
                            if (Convert.ToUInt64(obj["DiskBytesPersec"]) > 0)
                            {
                                //Show Busy
                                hddLedIcon.Icon = activeIcon;
                            }
                            else
                            {
                                //Show Idle
                                hddLedIcon.Icon = idleIcon;
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }
            catch (ThreadAbortException tbe)
            {
                driveDataClass.Dispose();
            }
        }

        #endregion
    }
}
