

using System;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Net;      
using System.Net.Sockets;    
using System.Threading;
using OpenHardwareMonitor.Collections;
using OpenHardwareMonitor.Hardware;


namespace odczyt
{

    public partial class Form1 : Form
    {

        
        public TcpListener server;
        public TcpClient client;
        public NetworkStream wysylanie;
        Thread squaresThread;
        bool czy_polaczony = false;
        bool interwal = false;
        string temp_CPU;

        public Form1()
        {
            InitializeComponent();
            InitializeTimer();   //inicjalizacja timera
            label1.Visible = false;
            label2.Visible = false;
            label5.Visible = false;
            label9.Visible = false;
        }


        





        byte[] temperature_byte = new byte[100];   

         void client_Thread()         
        {
            
            Int32 port = 8001;           
            string NazwaTegoKomputera = Dns.GetHostName();
            IPHostEntry AdresyIP_TegoKomputera = Dns.GetHostEntry(NazwaTegoKomputera); 
            IPAddress AdresIP = AdresyIP_TegoKomputera.AddressList[1];
            IPAddress localAddr = AdresIP;
          
             server = new TcpListener(localAddr, port); 
            
            server.Start();  
            try               
            {
                client = server.AcceptTcpClient();   
            }
            catch
            {
                if (wysylanie != null) wysylanie.Close();
                if (client != null) client.Close();
                if (server != null) server.Stop();
                squaresThread.Abort();

            }

            czy_polaczony = true;      

            while (czy_polaczony)
            {

             if(interwal)    
                {
                    interwal = false;        
                 try               
                {
                    temperature_byte = Encoding.Default.GetBytes(temp_CPU + " ");  
                    wysylanie = client.GetStream();  
                    wysylanie.Write(temperature_byte, 0, temperature_byte.Length);  
                    
                }
                catch
                {
                        if (wysylanie != null) wysylanie.Close();
                        if (client != null) client.Close();
                        if (server != null) server.Stop();
                        czy_polaczony = false;
                   
                        timer1.Enabled = false; 
                        squaresThread.Abort();
                 }

              }



            }

           if( wysylanie != null) wysylanie.Close();
           if (client != null) client.Close();
           if (server != null) server.Stop();
           squaresThread.Abort();
        }



        protected readonly ListSet<ISensor> active = new ListSet<ISensor>();

        public string Temperatura_CPU()         //odczyt predkosci procesora
        {
           
           string temp =" ";
            var myComputer = new Computer();
            myComputer.CPUEnabled = true;
            myComputer.Open();

            foreach (var hardwareItem in myComputer.Hardware)
            {
                

                foreach (var sensor in hardwareItem.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                       temp = sensor.Value.ToString();
                       
                    }

              }
            }

            return temp ;
        }


        private void InitializeTimer()
        {
            timer1.Interval = 1000; 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
        }

    

        private void timer1_Tick(object sender, System.EventArgs e)  
        {

            interwal = true;
            temp_CPU = Temperatura_CPU();
            label1.Text = temp_CPU;
            label1.Visible = true;

            
        }


        private void button3_Click(object sender, EventArgs e) 
        {

            Int32 port = 8001;           
            label5.Text = port.ToString();
            label5.Visible = true;
            label9.Text = "Connected";
            label9.Visible = true;
            string NazwaTegoKomputera = Dns.GetHostName();
            IPHostEntry AdresyIP_TegoKomputera = Dns.GetHostEntry(NazwaTegoKomputera); 
            IPAddress AdresIP = AdresyIP_TegoKomputera.AddressList[1];

            label2.Text = AdresIP.ToString();
            label2.Visible = true;
            button3.Enabled = false;

            timer1.Enabled = true;

            squaresThread = new Thread(client_Thread);  
            squaresThread.Start();
            
        }


        private void button4_Click(object sender, EventArgs e) 
        {
           
            timer1.Enabled = false; 
            if (wysylanie != null) wysylanie.Close();
             if (client != null) client.Close();
             if (server != null) server.Stop();
           
            czy_polaczony = false;
       
            label9.Text = "Disconnected";
            button3.Enabled = true;
            label1.Visible = false;
            squaresThread.Abort();
        }


        private void button1_Click(object sender, EventArgs e) 
        {
            Application.Exit();
        }

       
    }
}
