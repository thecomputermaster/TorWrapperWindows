using Microsoft.Win32;
using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace WindowsTorWrapper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            {
                MessageBox.Show("Heads up the tor process will stay running even if this window is closed. To terminate, kill the tor.exe process.");
                // Start BackgroundWorker
                backgroundWorkerupdateorinstall.RunWorkerAsync();
            }

        }
        
        // For all tor process related connections, we use background workers so our GUI does not get frozen waiting for the tor process to respond.

        private void backgroundWorkerupdateorinstall_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static string config;
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            config = richTextBox1.Text;
            richTextBox1.EnableContextMenu();

        }

        private void button1_Click(object sender, EventArgs e)
        {
        // Here, we start the tor process with the generated server configuration the user typed in the text box.
            string configtorrc = richTextBox1.Text;
            MessageBox.Show("Torrc file created. Now launching your relay. Please see the above window for the progress. If you see 'Testing indicates your ORPort is reachable. Publishing server descriptor, all is good. If you see a failure message, either your tor configuration is incorrect or the connection is being blocked. I will configure Windows Defender Firewall to allow the relay traffic autommatically, but any other firewall will need to be manually configured to allow Tor.");
            System.IO.File.WriteAllText(@"C:\torrc.txt", configtorrc);
            backgroundWorker1.RunWorkerAsync();

        }




        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
        //On the user clicking this button, we add a registry key so tor server will start automatically when Windows boots. Does so by adding the tool to the Windows launch agent, which in turn auto starts tor with the relay configuration.

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("Start Tor Relay", "\"" + Application.ExecutablePath + "\"");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
//Button to remove tor relay from Windows launch agent.
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue("Start Tor Relay", false);
            }
        }



        private void richTextBox2_TextChanged_1(object sender, EventArgs e)
        {
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
        }



        [Obsolete]
        private void ipmonitor_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Now capturing connections to your relay. They will be saved in torforfiltering.txt on the root of your c:\\ drive. Search for tor.exe in that file. Cheers.");
            DisplayIP.RunWorkerAsync();

        }
            
            
   private void button5_Click(object sender, EventArgs e)
        {
            using (Process p = new Process())
            {
                ProcessStartInfo ps = new ProcessStartInfo();
                ps.Arguments = "-ab";
                ps.FileName = "netstat.exe";
                ps.UseShellExecute = false;
                ps.WindowStyle = ProcessWindowStyle.Hidden;
                ps.RedirectStandardInput = true;
                ps.RedirectStandardOutput = true;
                ps.RedirectStandardError = true;
                p.StartInfo = ps;
                p.Start();
                StreamReader stdOutput = p.StandardOutput;
                StreamReader stdError = p.StandardError;
                string content = stdOutput.ReadToEnd() + stdError.ReadToEnd();
                string exitStatus = p.ExitCode.ToString();

                int first = content.IndexOf("tor.exe") + "methods".Length;
                int last = content.LastIndexOf("tor.exe");
                string str3 = content.Substring(first, last - first);

                System.IO.File.WriteAllText(@"C:\torconnectionshere.txt", $"Substring between \"methods\" and \"methods\": '{str3}'");
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs c)
        {

// As part of the auto configuration, we add a rule to the Windows Firewall to allow tor traffic.
            PowerShell ps2 = PowerShell.Create();
            string scriptfirewallrule = string.Format("netsh advfirewall firewall add rule name = 'Tor Relay' dir =in action = allow program = 'C:\\ProgramData\\chocolatey\\lib\\tor\\tools\\Tor\\tor.exe' Enable = yes");
            PowerShell psout = PowerShell.Create();
            string scriptfirewallruleout = string.Format("netsh advfirewall firewall add rule name = 'Tor Relay' dir =out action = allow program = 'C:\\ProgramData\\chocolatey\\lib\\tor\\tools\\Tor\\tor.exe' Enable = yes");
            psout.AddScript(scriptfirewallruleout);
            psout.Invoke();
            PowerShell ps3 = PowerShell.Create();
            string starttor = string.Format("tor.exe -f C:\\torrc.txt");
            ps3.AddScript(starttor);
            ps3.Invoke();
            MessageBox.Show("To see tor progress, open the notices.log file. It will be where you set it to be in the torrc configuration above.");



        }

        private void backgroundWorkerupdateorinstall_DoWork(object sender, DoWorkEventArgs e)
        {
        //On starting, we check to see if tor is running the newest version. If not, we update tor to the newest version automatically for the user.
        
            MessageBox.Show("I am now making sure Tor is running the newest version. If it is not, I will autmatically update tor to the newest verison. Please one minute. You may see CMD flash at you");
            PowerShell ps = PowerShell.Create();
            string script = string.Format("Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))");
            ps.AddScript(script);
            ps.Invoke();
            if (System.IO.File.Exists(@"c:\\ProgramData\\chocolatey\\lib\\tor\\tools\\Tor\\tor.exe"))
            {

                MessageBox.Show("You  have tor installed, I am not making sure your running the newest version. Powershell will open, it might ask if you want to run a script. Type y and press enter. If there's a newer version of tor I will update to it for you.");
                ProcessStartInfo info = new ProcessStartInfo("powershell.exe");
                info.Verb = "runas";
                info.Arguments = "choco upgrade tor";
                info.UseShellExecute = true;

                Process.Start(info);

            }
            else
            {
                using (Process p = new Process())
                {
                //If tor is not installed, we run a script which installs it.

                    MessageBox.Show("It looks like you don't have tor installed. Let's fix that. Windows Powershell will now open asking you if you want to run a script. This script will install tor. Type y and press enter.");
                    ProcessStartInfo info = new ProcessStartInfo("powershell.exe");
                    info.Verb = "runas";
                    info.Arguments = "choco install tor";
                    info.UseShellExecute = true;

                    Process.Start(info);


                }

            }
            //Here, we check to see if a tor relay configuration already exists (by checking if there is already a torrc file). If it does, then we start tor with the exisitng torrc configration automatically.
            if (System.IO.File.Exists(@"C:\\torrc.txt"))
            {
            
                MessageBox.Show("It looks like you already have a torrc file written. I will start tor now with these configurations. To change the torrc file, use the following window.");
                PowerShell psprestart = PowerShell.Create();
                string starttor = string.Format("tor.exe -f C:\\torrc.txt");
                psprestart.AddScript(starttor);
                psprestart.Invoke();

            }
        }


        private void backgroundWorkerStartAlreadyConfigured_DoWork(object sender, DoWorkEventArgs e)
        {
            PowerShell ps3 = PowerShell.Create();
            string starttor = string.Format("tor.exe -f C:\\torrc.txt");
            ps3.AddScript(starttor);
            ps3.Invoke();
            MessageBox.Show("To see tor progress, open the notices.log file. It will be where you set it to be in the torrc configuration above.");


        }
      
        private void DisplayIP_DoWork(object sender, DoWorkEventArgs e)
        {
            using (Process p = new Process())
            {
                ProcessStartInfo ps = new ProcessStartInfo();
                ProcessStartInfo info = new ProcessStartInfo("powershell.exe");
                info.Verb = "runas";
                info.Arguments = "netstat -ab | Out-File C:\\torforfiltering.txt";
                info.UseShellExecute = true;


                Process.Start(info);

                

            }

        }
    }
}


