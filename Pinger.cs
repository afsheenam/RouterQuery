using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RouterQuery
{
    class Pinger
    {
        public List<ClientData> Devices = new List<ClientData>();
        public event EventHandler ResultsReady;

        private volatile bool pingLocked = false;
        private volatile object lockObj = new object();
        private volatile int pingsActive;
        private bool pingSendComplete;


        public static string NetworkGateway()
        {
            string ip = null;

            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (f.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                    {
                        ip = d.Address.ToString();
                    }
                }
            }

            return ip;
        }

        private void IncPing()
        {
            lock (lockObj)
            {
                pingsActive++;
            }
        }
        private void DecPing()
        {
            lock (lockObj)
            {
                pingsActive--;
                if (pingSendComplete && pingsActive == 0)
                    ResultsReady?.Invoke(this, null);
            }
        }

        public void PingAll()
        {
            Devices.Clear();
            pingsActive = 0;
            pingSendComplete = true;

            string gate_ip = NetworkGateway();

            //Extracting and pinging all other ip's.
            string[] array = gate_ip.Split('.');

            for (int i = 2; i <= 255; i++)
            {

                string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;

                //time in milliseconds           
                Ping(ping_var, 4, 10000);

            }
        }

        public void Ping(string host, int attempts, int timeout)
        {
            for (int i = 0; i < attempts; i++)
            {
                new Thread(delegate ()
                {
                    try
                    {
                        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                        ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                        IncPing();
                        ping.SendAsync(host, timeout, host);
                    }
                    catch
                    {
                        DecPing();
                        // Do nothing and let it try again until the attempts are exausted.
                        // Exceptions are thrown for normal ping failurs like address lookup
                        // failed.  For this reason we are supressing errors.
                    }
                }).Start();
            }
        }

        public string GetHostName(string ipAddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (SocketException)
            {
                // MessageBox.Show(e.Message.ToString());
            }

            return null;
        }


        //Get MAC address
        public string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process Process = new System.Diagnostics.Process();
            Process.StartInfo.FileName = "arp";
            Process.StartInfo.Arguments = "-a " + ipAddress;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.CreateNoWindow = true;
            Process.Start();
            string strOutput = Process.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                         + "-" + substrings[7] + "-"
                         + substrings[8].Substring(0, 2);
                return macAddress;
            }

            else
            {
                return "OWN Machine";
            }
        }

        private void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            DecPing();
            ClientData newClient;
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                string hostname = GetHostName(ip);
                string macaddres = GetMacAddress(ip);
                string[] arr = new string[3];

                //store all three parameters to be shown on ListView
                //arr[0] = ip;
                //arr[1] = hostname;
                //arr[2] = macaddres;
                newClient = new RouterQuery.ClientData();
                newClient.IP = ip;
                newClient.Name = hostname;
                newClient.MacAddr = macaddres;

                // Logic for Ping Reply Success
                /*ListViewItem item;
                if (this.InvokeRequired)
                {

                    this.Invoke(new Action(() =>
                    {

                        item = new ListViewItem(arr);

                        lstLocal.Items.Add(item);


                    }));
                }


            }
            else
            {
                // MessageBox.Show(e.Reply.Status.ToString());
            }*/
            }
        }
    }

    public struct ClientData
    {
        public string IP;
        public string Name;
        public string MacAddr;
    }

}
