using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Alive
{
    public partial class Form1 : Form
    {
        private Panel[] mesPanels;
        private List<Machine> machineList;
        private Machine[] machines;
        private IDictionary<Panel, PictureBox> panelPic = new Dictionary<Panel, PictureBox>();
        private Image Img_ok, Img_nok, Img_waiting;
        private System.Timers.Timer timer;
        private bool loadable = false, unloadable = false, wantReload = false;
        private int panelWidth = 100;
        private int panelHeight = 80;
        private int margin = 10;
        private int numPanels;
        private int numLines, old_numlines;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadable = true;
            /*new System.Threading.Thread(() =>
            {
            System.Threading.Thread.Sleep(1000);*/
            loadForm();
/*
            });*/
            /*System.Threading.Thread.Sleep(1000);
            System.Threading.Thread.Sleep(1000);
            reloadForm();*/
            
        }
        private void reloadForm()
        {
            this.Invoke(new MethodInvoker(delegate
            {
                unloadForm();
                //System.Threading.Thread.Sleep(100);
                loadForm();
                wantReload = false;
            }));
        }
        private void unloadForm()
        {
            if (unloadable)
            {
                unloadable = false;
                timer.Stop();
                //System.Threading.Thread.Sleep(1000);
                foreach (KeyValuePair<Panel, PictureBox> dic in panelPic)
                {
                    dic.Key.Controls.Remove(dic.Value);
                }
                foreach (Panel panel in mesPanels)
                {
                    this.Controls.Remove(panel);
                }

                mesPanels = null;
                panelPic = new Dictionary<Panel, PictureBox>();
                timer = null;
                loadable = true; unloadable = false;
            }

    }


        private void loadForm()
        {
            if (loadable)
            {
                loadable = false;
                this.HorizontalScroll.Maximum = 0;
                this.AutoScroll = false;
                this.VerticalScroll.Visible = false;
                this.AutoScroll = true;


                try
                {
                    if (Img_ok == null) Img_ok = Image.FromFile(@"assets/img/ok.png");
                    if (Img_nok == null) Img_nok = Image.FromFile(@"assets/img/nok.png");
                    if (Img_waiting == null) Img_waiting = Image.FromFile(@"assets/img/waiting.png");
                    if (machineList == null) machineList = LoadMachines("assets/conf/machines.json");
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    DialogResult result = MessageBox.Show("Des fichiers sont manquant, veuillez réinstaller l'application...", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }

                numPanels = machineList.Count;
                
                //int panelWidth = this.Size.Width;
                
                //panelWidth -= 60;




                mesPanels = new Panel[numPanels];
                machines = machineList.ToArray();

                //for (int i = 0, y = margin; i < numPanels; i++, y += (panelHeight + margin))
                int x = margin, y = margin;
                // x += (panelWidth + margin)
                // y += (panelHeight + margin)
                numLines = 0;
                bool numLinesCountStop = false;
                for (int i = 0; i < numPanels; i++)
                {

                    mesPanels[i] = new Panel();


                    // Définition du panel
                    //Trace.WriteLine("Panel Width: " + panelWidth);
                    mesPanels[i].Size = new Size(panelWidth, panelHeight);//taille du panel
                    mesPanels[i].Location = new Point(x, y);// origine du panel
                                                            //mesPanels[i].Parent = panel1;// le coller sur la form

                    // Nom de la machine
                    Label name = new Label();
                    //String text = i.ToString();
                    //Trace.WriteLine("Lorem Ipsum " + i.ToString());
                    name.Text = machines[i].Name;

                    //name.Location = new Point(10, panelHeight / 3);
                    name.Location = new Point(0, margin);
                    // Ajout du name plus tard pour qu'il soit au dessous de l'image
                    name.TextAlign = (ContentAlignment)HorizontalAlignment.Center;

                    //Icone state
                    PictureBox statePicv = new PictureBox();
                    statePicv.Image = Img_waiting;
                    int size = 32;
                    //statePicv.Location = new Point(mesPanels[i].Width - size - margin, panelHeight / 4);
                    statePicv.Location = new Point(mesPanels[i].Width / 3, panelHeight - size - margin);
                    statePicv.Size = new Size(size, size);
                    panelPic.Add(mesPanels[i], statePicv);
                    mesPanels[i].Controls.Add(statePicv);
                    //Ajout du name
                    mesPanels[i].Controls.Add(name);



                    Controls.Add(mesPanels[i]);

                    //mesPanels[i].BackColor = Color.Red;// juste pour l esssai pour pouvoir le voir
                    mesPanels[i].BackColor = Color.White;

                    if ((x + (2 * panelWidth + 2 * margin)) >= this.Size.Width)
                    {
                        y += (panelHeight + margin);
                        x = margin;
                        numLinesCountStop = true;
                    }
                    else
                    {
                        if (!numLinesCountStop) numLines++;
                        x += (panelWidth + margin);
                    }
                }
                //Task ping each Secs
                //numLines /= 2;
                //Trace.WriteLine("Num Line: " + numLines);
                old_numlines = numLines;

                timer = new System.Timers.Timer();
                timer.Interval = 1000;
                timer.Elapsed += Timer_Tick;

                timer.Start();
                loadable = false; unloadable = true;
            }
            
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            
            //new System.Threading.Thread(() =>
            //{
                System.Threading.Thread.CurrentThread.IsBackground = true;
            /* run your code here */
            //Trace.WriteLine("Essai");
            for (int i = 0; i < machineList.Count; i++)
            {
                //Trace.WriteLine("Pinging: " + machines[i].adress);
                try
                {
                    if (ping(machines[i]))
                    {
                        if (panelPic[mesPanels[i]].Image != Img_ok) panelPic[mesPanels[i]].Image = Img_ok;
                    }
                    else
                    {
                        if (panelPic[mesPanels[i]].Image != Img_nok) panelPic[mesPanels[i]].Image = Img_nok;
                    }
                    //Trace.WriteLine("Want reload: " + wantReload);
                    if (wantReload)
                    {
                        //timer.Stop();
                        
                    }
                } catch (Exception)
                {
                    //Trace.WriteLine("Exception dans Ping");
                    //reloadForm();
                    wantReload = true;
                }

                if (wantReload && needReload())
                {
                    reloadForm();
                }
                
            }
            // }).Start();
            
        }

        private bool needReload()
        {

            int x = margin, y = margin, numLines_test = 0;
            bool numLinesCountStop = false;
            for (int i = 0; i < numPanels; i++)
            {
                if ((x + (2 * panelWidth + 2 * margin)) >= this.Size.Width)
                {
                    y += (panelHeight + margin);
                    x = margin;
                    numLinesCountStop = true;
                    break;
                }
                else
                {
                    if (!numLinesCountStop) numLines_test++;
                    x += (panelWidth + margin);
                }
            }
            if(numLines != numLines_test) return true;
            else return false;
        }

        public bool ping(Machine machine)
        {

            Ping pingSender = new Ping();
            //PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128, 
            // but change the fragmentation behavior. 
            // options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted. 
            //string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            // byte[] buffer = Encoding.ASCII.GetBytes(data);
            // int timeout = 120;
            try
            {
                PingReply reply = pingSender.Send(machine.adress, 1000);
                if (reply.Status != IPStatus.Success) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            /* mesPanels = new Panel[15];
             for (int i = 0, y = 10; i < 15; i++, y += 50)
             {
                 mesPanels[i] = new Panel();
                 mesPanels[i].Size = new Size((this.Size.Width - 10) - this.DefaultMargin.Horizontal, 40);//taille du panel
                 mesPanels[i].Location = new Point(10, y);// origine du panel
                 //mesPanels[i].Parent = panel1;// le coller sur la form
                 Controls.Add(mesPanels[i]);

                 mesPanels[i].BackColor = Color.Red;// juste pour l esssai pour pouvoir le voir
             }*/
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            //reloadForm();
            wantReload = true;
        }

        private void Form1_MaximizedBoundsChanged(object sender, EventArgs e)
        {
            //reloadForm();
        }

        FormWindowState LastWindowState = FormWindowState.Minimized;
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            
            // When window state changes
            if (WindowState != LastWindowState)
            {
                LastWindowState = WindowState;


                if (WindowState == FormWindowState.Maximized)
                {
                    //reloadForm();
                    wantReload = true;
                    // Maximized!
                }
                if (WindowState == FormWindowState.Normal)
                {
                    //reloadForm();
                    wantReload = true;
                    // Restored!
                }
            }
            
        }

        static Color SetTransparency(int A, Color color)
        {
            return Color.FromArgb(A, color.R, color.G, color.B);
        }

        /*int altura = this.Size.Height;
int largura = this.Size.Width;

int alturaOffset = 10;
int larguraOffset = 10;
int larguraBotao = 100; //button widht
int alturaBotao = 40;  //button height

for (int i = 0; i < 100; ++i)
{

   if ((larguraOffset + larguraBotao) >= largura)
   {
       larguraOffset = 10;
       alturaOffset = alturaOffset + alturaBotao;
       var button = new Button();
       button.Size = new Size(larguraBotao, alturaBotao);
       button.Name = "" + i + "";
       button.Text = "" + i + "";
       //button.Click += button_Click;//function
       button.Location = new Point(larguraOffset, alturaOffset);
       Controls.Add(button);
       larguraOffset = larguraOffset + (larguraBotao);
   }
   else
   {

       var button = new Button();
       button.Size = new Size(larguraBotao, alturaBotao);
       button.Name = "" + i + "";
       button.Text = "" + i + "";
       //button.Click += button_Click;//function
       button.Location = new Point(larguraOffset, alturaOffset);
       Controls.Add(button);
       larguraOffset = larguraOffset + (larguraBotao);

   }
}*/


        public List<Machine> LoadMachines(string json_path)
        {
            using (StreamReader r = new StreamReader(json_path))
            {
                string raw_json = r.ReadToEnd();
                //Trace.WriteLine("JSON: " + raw_json);
                List<Machine> machines = JsonConvert.DeserializeObject<List<Machine>>(raw_json);
                /*foreach(Machine machine in machines)
                {
                    Trace.WriteLine("Machine: " + machine.Name);
                }*/
                return machines;
            }
        }
    }

    public class Machine
    {
        public string Name;
        public string adress;
    }
}
    
