/*
	TUIO C# Demo - part of the reacTIVision project
	Copyright (c) 2005-2016 Martin Kaltenbrunner <martin@tuio.org>

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using TUIO;
using System.IO;
using NAudio.Wave;
using System.Drawing.Drawing2D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
//using Newtonsoft.Json;
//using Dental3DViewer; // Namespace of your WPF project

public class TuioDemo : Form, TuioListener
{
    private TuioClient client;
    private Dictionary<long, TuioObject> objectList;
    private Dictionary<long, TuioCursor> cursorList;
    private Dictionary<long, TuioBlob> blobList;


    public static int width, height, selectedIndex = -1;
    private int window_width = Screen.PrimaryScreen.Bounds.Width;
    private int window_height = Screen.PrimaryScreen.Bounds.Height;
    private int window_left = 0;
    private int window_top = 0;
    private int screen_width = Screen.PrimaryScreen.Bounds.Width;
    private int screen_height = Screen.PrimaryScreen.Bounds.Height;


    private bool fullscreen;
    private bool verbose;

    private Image backgroundImage = Image.FromFile(@"BG_3.jpg");
    private Image backgroundImage2 = Image.FromFile(@"bg.jpg");
    Font font = new Font("Times New Roman", 30.0f);
    SolidBrush fntBrush = new SolidBrush(Color.Black);
    SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(255, 255, 64));
    SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
    SolidBrush SelectedItemBrush = new SolidBrush(Color.SeaGreen);
    SolidBrush MenuItemBrush = new SolidBrush(Color.White);
    SolidBrush objBrush = new SolidBrush(Color.Silver);
    SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
    Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);

    List<Point> mymenupoints = new List<Point>();
  

    //var viewerControl = new Dental3DViewerControl();
    //elementHost1.Child = viewerControl;

    Bitmap off;
    public class CActor
    {
        //public int X, Y;
        public Rectangle rcDst;
        public int rowid, colid;
        public Rectangle rcSrc;
        public Bitmap img;
        public int color = 0;
        public int X,Y,W,H;

    }
    public TuioDemo(int port)
    {

        verbose = true;
        fullscreen = false;
        width = window_width;
        height = window_height;
            
        this.ClientSize = new System.Drawing.Size(width, height);
        this.Name = "Crown Preparation Application"; 
        this.Name = "Crown Preparations Interactive App";
        //Button button = new Button();
        //button.Text = "START";
        //button.FlatAppearance.Equals(FlatStyle.Flat);
        //button.Location.X.Equals(this.ClientSize.Width / 2);
        //button.Location.Y.Equals(this.ClientSize.Height / 2);
        //button.Visible = true;
        //button.Enabled = true;

        this.Load += TuioDemo_Load;
        this.Closing += new CancelEventHandler(Form_Closing);
        this.KeyDown += new KeyEventHandler(Form_KeyDown);


        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                        ControlStyles.UserPaint |
                        ControlStyles.DoubleBuffer, true);

        objectList = new Dictionary<long, TuioObject>(128);
        cursorList = new Dictionary<long, TuioCursor>(128);
        blobList = new Dictionary<long, TuioBlob>(128);

        client = new TuioClient(port);
        client.addTuioListener(this);
        
        client.connect();
    }

    private void TuioDemo_Load(object sender, EventArgs e)
    {
/*        string audiofilepath = ("01 - Track 01.mp3");
        PlayBackgroundMusic(audiofilepath);*/
        off = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
        this.InitializeComponent();
    }

    private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {

        if (e.KeyData == Keys.F1)
        {
            if (fullscreen == false)
            {

                width = screen_width;
                height = screen_height;

                window_left = this.Left;
                window_top = this.Top;

                this.FormBorderStyle = FormBorderStyle.None;
                this.Left = 0;
                this.Top = 0;
                this.Width = screen_width;
                this.Height = screen_height;

                fullscreen = true;
            }
            else
            {

                width = window_width;
                height = window_height;

                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.Left = window_left;
                this.Top = window_top;
                this.Width = window_width;
                this.Height = window_height;

                fullscreen = false;
            }
        }
        else if (e.KeyData == Keys.Escape)
        {
            this.Close();

        }
        else if (e.KeyData == Keys.V)
        {
            verbose = !verbose;
        }

    }

    private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        client.removeTuioListener(this);

        client.disconnect();
        System.Environment.Exit(0);
    }

    public void addTuioObject(TuioObject o)
    {
        lock (objectList)
        {
            objectList.Add(o.SessionID, o);
        }
        if (verbose) Console.WriteLine("add obj " + o.SymbolID + " (" + o.SessionID + ") " + o.X + " " + o.Y + " " + o.Angle);
    }
    public AudioFileReader soundEffect;
    public WaveOutEvent soundEffectOutput;
    public void PlaySoundEffect(string audioFilePath)
    {
        soundEffect = new AudioFileReader(audioFilePath);
        // Create a wave output device to play the sound effect
        soundEffectOutput = new WaveOutEvent();

        // Set the output device to use the sound effect as its input
        soundEffectOutput.Init(soundEffect);

        // Play the sound effect
        soundEffectOutput.Play();
        //soundEffect.Volume = 0.8f;
        soundEffectOutput.Volume = 0.8f;
    }
    public int menuMarker = 15; 
    private bool hasPlayedSound = false;
    // Declare a threshold for rotation change (adjust based on your use case)
    private double rotationThreshold = 10f;  // Example: 15-degree change
    private double previousRotationAngle = 0f;

    public void checkrotation(List<CActor> objs, Graphics g, TuioObject o)
    {
        if (o.SymbolID == menuMarker) // Assuming marker with SymbolID 0 controls the menu
        {
            // Convert the angle to degrees
            double angleDegrees = o.Angle * 180.0 / Math.PI;

            // Normalize the angle to be within 0 to 360 degrees
            if (angleDegrees < 0) angleDegrees += 360;

            // Reverse the angle direction for correct item selection
            angleDegrees = 360 - angleDegrees;

            double rotationDifference = Math.Abs(angleDegrees - previousRotationAngle);
            previousRotationAngle = angleDegrees;

            // Divide the full circle (360 degrees) into equal sections for each menu item
            double anglePerItem = 360.0 / CountMenuItems;

            // Calculate which menu item should be selected
            int newMenuIndex = (int)Math.Floor(angleDegrees / anglePerItem) % CountMenuItems;

            // Update the menu selection only if the new index is different from the current one
            if (newMenuIndex != MenuSelectedIndex)
            {
                if (MenuSelectedIndex >= 0 && MenuSelectedIndex < CountMenuItems)
                {
                    MenuObjs[MenuSelectedIndex].color = 0;  // Deselect previous menu item
                }

                // Set color of the new menu item
                if (newMenuIndex >= 0 && newMenuIndex < CountMenuItems)
                {
                    MenuObjs[newMenuIndex].color = 1; // Select new menu item
                }

                // If the icon has significantly rotated and sound hasn't been played
                if (!hasPlayedSound && rotationDifference > rotationThreshold)
                {
                    PlaySoundEffect("menusound_swipe.mp3");
                    hasPlayedSound = true;  // Set the flag to prevent repeated plays
                }
                else
                {
                    // If the rotation is not significant, reset the flag
                    hasPlayedSound = false;
                }

                // Update the selected menu index
                MenuSelectedIndex = newMenuIndex;

                // Trigger a repaint with the updated menu
                Invalidate();
            }

            // Optional verbose logging
            if (verbose)
            {
                Console.WriteLine("MenuSelectedIndex: " + MenuSelectedIndex);
            }
        }
    }

    public void updateTuioObject(TuioObject o)
    {
       
        // Existing verbose logging for other object data
        if (verbose)
        {
            Console.WriteLine("set obj " + o.SymbolID + " (" + o.SessionID + ") " + o.X + " " + o.Y + " " + o.Angle + " " + o.MotionSpeed + " " + o.RotationSpeed + " " + o.MotionAccel + " " + o.RotationAccel);
        }
    }

    /* public Graphics drawmenu(List<CActor> menuobjs, Graphics g)
     {
         int cornerRadius = 10;
         for (int i = 0; i < menuobjs.Count; i++)
         {
             Rectangle rect = new Rectangle(menuobjs[i].X, menuobjs[i].Y, menuobjs[i].W, menuobjs[i].H);
             if (menuobjs[i].color == 0)
             {
                 DrawRoundedRectangle(g, MenuItemBrush, rect, cornerRadius);
             }
             else
             {
                 DrawRoundedRectangle(g, SelectedItemBrush, rect, cornerRadius);
             }

         }
         return g;
     }
 */
    public void removeTuioObject(TuioObject o)
    {
        lock (objectList)
        {
            objectList.Remove(o.SessionID);
        }
        if (verbose) Console.WriteLine("del obj " + o.SymbolID + " (" + o.SessionID + ")");
    }

    public void addTuioCursor(TuioCursor c)
    {
        lock (cursorList)
        {
            cursorList.Add(c.SessionID, c);
        }
        if (verbose) Console.WriteLine("add cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y);
    }

    public void updateTuioCursor(TuioCursor c)
    {
        if (verbose) Console.WriteLine("set cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y + " " + c.MotionSpeed + " " + c.MotionAccel);
    }

    public void removeTuioCursor(TuioCursor c)
    {
        lock (cursorList)
        {
            cursorList.Remove(c.SessionID);
        }
        if (verbose) Console.WriteLine("del cur " + c.CursorID + " (" + c.SessionID + ")");
    }

    public void addTuioBlob(TuioBlob b)
    {
        lock (blobList)
        {
            blobList.Add(b.SessionID, b);
        }
        if (verbose) Console.WriteLine("add blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area);
    }

    public void updateTuioBlob(TuioBlob b)
    {

        if (verbose) Console.WriteLine("set blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area + " " + b.MotionSpeed + " " + b.RotationSpeed + " " + b.MotionAccel + " " + b.RotationAccel);
    }

    public void removeTuioBlob(TuioBlob b)
    {
        lock (blobList)
        {
            blobList.Remove(b.SessionID);
        }
        if (verbose) Console.WriteLine("del blb " + b.BlobID + " (" + b.SessionID + ")");
    }
    public static List<string> imagePaths = new List<string>();
    public int SelectedMenuFlag = 0;
    public void DrawRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius, int index)
    {
        using (GraphicsPath path = new GraphicsPath())
        {
            float diameter = radius * 2f;
            SizeF sizeF = new SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(rect.Location, sizeF);

            // Add rounded corners
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            // Draw the rounded rectangle background
            g.FillPath(brush, path);

            // Draw the image only if SelectedMenuFlag > 1
            if (SelectedMenuFlag > 1 && imagePaths.Count > index)
            {
                try
                {
                    using (Image image = Image.FromFile(imagePaths[index]))
                    {
                        // Calculate the aspect ratio of the image and the destination rectangle
                        float imageAspectRatio = (float)image.Width / image.Height;
                        float rectAspectRatio = (float)rect.Width / rect.Height;

                        int destWidth, destHeight;

                        // Scale the image to fit within the destination rectangle while maintaining the aspect ratio
                        if (imageAspectRatio > rectAspectRatio)
                        {
                            destWidth = rect.Width;
                            destHeight = (int)(rect.Width / imageAspectRatio);
                        }
                        else
                        {
                            destHeight = rect.Height;
                            destWidth = (int)(rect.Height * imageAspectRatio);
                        }

                        // Calculate the position to center the image within the rectangle
                        int destX = rect.X + (rect.Width - destWidth) / 2;
                        int destY = rect.Y + (rect.Height - destHeight) / 2;
                        Rectangle destRect = new Rectangle(destX, destY, destWidth, destHeight);

                        // Clip to the rounded rectangle path and draw the image centered and scaled
                        Region originalClip = g.Clip;
                        g.SetClip(path, CombineMode.Replace);
                        g.DrawImage(image, destRect);
                        g.Clip = originalClip;
                    }
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("Image file not found: " + imagePaths[index]);
                }
            }
        }
    }


    public void refresh(TuioTime frameTime)
    {
        Invalidate();
    }
    public int MenuIconWidth = 100;
    public int MenuIconHeight = 150;
    public int CountMenuItems = 2;
    public int MenuSelectedIndex = 0; //item selection
    List<CActor> MenuObjs = new List<CActor>();
    public List<Point> generatemenu(int n) //generate points
    {
        MenuSelectedIndex = n-1;
        List<Point> myicons = new List<Point>();

        // Define the center of the circular menu
        int centerX = ClientSize.Width / 2;
        int centerY = ClientSize.Height / 2;

        // Define the radius of the circle (adjust as necessary)
        int radius = Math.Min(ClientSize.Width, ClientSize.Height) / 4;

        // Calculate angle between each icon
        double angleIncrement = 360.0 / n;

        for (int i = 0; i < n; i++)
        {
            // Calculate the angle in radians
            double angleInRadians = (angleIncrement * i) * (Math.PI / 180);

            // Calculate the x and y coordinates using polar to Cartesian conversion
            int x = (int)(centerX + radius * Math.Cos(angleInRadians) - MenuIconWidth / 2);
            int y = (int)(centerY + radius * Math.Sin(angleInRadians) - MenuIconHeight / 2);

            // Add the point to the list
            myicons.Add(new Point(x, y));
        }

        return myicons;
    }
    public List<CActor> CreateMenuObjects(List<Point> points)
    {
        int padding = 20;
        int availableWidth = ClientSize.Width - (padding * 4);

        int maxWidth = ((availableWidth / CountMenuItems) - 200);
        if(SelectedMenuFlag != 0)
        {
            maxWidth= (availableWidth / CountMenuItems) - 750;
            MenuIconHeight = 250;
        }
        List<CActor> objs = new List<CActor>();

        for (int i = 0; i < points.Count; i++)
        {
            CActor obj = new CActor();

            // Adjust width based on the number of items

            obj.W = maxWidth;
            obj.H = MenuIconHeight;

            // Center the rectangle horizontally with padding on each side
            obj.X = ClientSize.Width / 2 - (CountMenuItems * maxWidth) / 2 + i * (maxWidth + padding);
            obj.Y = ClientSize.Height / 2 - MenuIconHeight / 2;

            // Highlight selected menu item
            obj.color = (i == MenuSelectedIndex) ? 1 : 0;
            objs.Add(obj);
        }
        return objs;
    }
   
                       
    public Graphics drawmenu(List<CActor> menuobjs, Graphics g)
    {
        int cornerRadius = 10;
        int padding = 10;
        bool drawTextBelow;
        // Set a base font and adjust only once if needed
        Font subFont = new Font("Segoe UI", 16, FontStyle.Bold);
        SolidBrush textBrush = new SolidBrush(Color.Black);

        for (int i = 0; i < menuobjs.Count; i++)
        {
            Rectangle rect = new Rectangle(menuobjs[i].X, menuobjs[i].Y, menuobjs[i].W, menuobjs[i].H);

            // Draw background of menu items
           

            // Define text based on menu item
            string itemText;
            if (SelectedMenuFlag == 0)
            {
                DrawRoundedRectangle(g, (menuobjs[i].color == 0) ? MenuItemBrush : SelectedItemBrush, rect, cornerRadius,i);
                itemText = (i == 0) ? "Extracoronal \r\n restorations" : "Intracoronal \r\n restorations";
                drawTextBelow = false;
            }
            else
            {
                DrawRoundedRectangle(g, (menuobjs[i].color == 0) ? MenuItemBrush : SelectedItemBrush, rect, cornerRadius, i);
                itemText = (i == 0) ? "Test \r\n restorations" : "Test 2 \r\n restorations";
                drawTextBelow = true;
            }

            // Adjust the text position based on drawTextBelow flag
            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            if (drawTextBelow)
            {
                // Draw text below the rectangle
                Rectangle textRect = new Rectangle(
                    rect.X,
                    rect.Bottom - padding, // Position below the menu item with padding
                    rect.Width,
                    rect.Height // Set height as per need for text
                );
                Font BelowFont = new Font("Segoe UI", 26, FontStyle.Bold);
                
                g.DrawString(itemText, BelowFont, textBrush, textRect, format);
            }
            else
            {
                // Draw text inside the rectangle
                g.DrawString(itemText, subFont, textBrush, rect, format);
            }
        }

        return g;
    }



    /*    private async Task<string> getTeethData(string symbolId)
        {
            try
            {
                using (TcpClient client = new TcpClient("localhost", 5000))
                {
                    client.ReceiveTimeout = 2000;
                    client.SendTimeout = 2000;
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] dataToSend = Encoding.ASCII.GetBytes(symbolId);
                        await stream.WriteAsync(dataToSend, 0, dataToSend.Length);

                        byte[] dataToReceive = new byte[4096];
                        int bytesRead = await stream.ReadAsync(dataToReceive, 0, dataToReceive.Length);
                        string responseData = Encoding.ASCII.GetString(dataToReceive, 0, bytesRead);

                        var patientInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);
                        Console.WriteLine(patientInfo["image"]);

                        if (patientInfo != null && patientInfo.ContainsKey("image"))
                        {
                            string imageUrlOrBase64 = patientInfo["image"].ToString();
                            //await DisplayPatientImageAsync(imageUrlOrBase64);
                        }

                        return responseData;
                    }


                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Request canceled.");
                return null;
            }
            catch (SocketException se)
            {
                Console.WriteLine("Socket error: " + se.Message);
                return null;
            }
            catch (TimeoutException te)
            {
                Console.WriteLine("Timeout: " + te.Message);
                return null;
            }
            catch (JsonException je)
            {
                Console.WriteLine("JSON error: " + je.Message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }
    */
    public int mainmenuflag = 1;
    
    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Getting the graphics object
        //getTeethData("15");
        
        Graphics g = pevent.Graphics;
        //g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));
        g.Clear(Color.WhiteSmoke);

        ////////////////////////////////////////////////////
        ///////////////////////
        // Define a rectangle for the title text
        Font titleFont = new Font("Segoe UI", 35, FontStyle.Bold);
        SolidBrush textBrush = new SolidBrush(Color.Black);

        // Define the dimensions and position for the semi-transparent rounded rectangle
        int boxWidth = 800; // Adjust width as needed
        int boxHeight = 200; // Adjust height as needed
        int boxX = (this.window_width / 2) - (boxWidth / 2);
        int boxY = this.ClientRectangle.Top + 20;

        // Create a semi-transparent white brush
        SolidBrush boxBrush = new SolidBrush(Color.FromArgb(150, Color.White));

        // Draw the rounded rectangle
        GraphicsPath roundedRectPath = new GraphicsPath();
        int cornerRadius = 20;
        roundedRectPath.AddArc(boxX, boxY, cornerRadius, cornerRadius, 180, 90);
        roundedRectPath.AddArc(boxX + boxWidth - cornerRadius, boxY, cornerRadius, cornerRadius, 270, 90);
        roundedRectPath.AddArc(boxX + boxWidth - cornerRadius, boxY + boxHeight - cornerRadius, cornerRadius, cornerRadius, 0, 90);
        roundedRectPath.AddArc(boxX, boxY + boxHeight - cornerRadius, cornerRadius, cornerRadius, 90, 90);
        roundedRectPath.CloseFigure();
        ///////////////////////
        ////////////////////////////////////////////////////

        SolidBrush brush = new SolidBrush(Color.White);
        System.Drawing.Pen mypen = new System.Drawing.Pen(Color.Black, 5);
        Font f = new Font("Calibri", 35, FontStyle.Bold);
        if (mainmenuflag == 1)
        {
            // g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));
           
            g.DrawImage(backgroundImage, new Rectangle(0, 0, width, height));
            g.FillPath(boxBrush, roundedRectPath);
            RectangleF textRect = new RectangleF(boxX + 10, boxY + 10, boxWidth - 20, boxHeight - 20);

            // Create a StringFormat for centered alignment
            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Center,      // Center horizontally
                LineAlignment = StringAlignment.Center   // Center vertically
            };
            g.DrawString("Interactive Application for Crown Preparation Learners", titleFont, textBrush, textRect, format);
            mainmenuflag= checkmainmenu();
            if(mainmenuflag == 2)
            {
                this.Controls.Remove(mainMenuButton);
                this.mainMenuButton.Dispose();
            }
        }
        else if(mainmenuflag == 2)
        {
            g.DrawImage(backgroundImage2, new Rectangle(0, 0, width, height));
            if (cursorList.Count > 0)
        {
            lock (cursorList)
            {
                foreach (TuioCursor tcur in cursorList.Values)
                {
                    List<TuioPoint> path = tcur.Path;
                    TuioPoint current_point = path[0];

                    for (int i = 0; i < path.Count; i++)
                    {
                        TuioPoint next_point = path[i];
                        g.DrawLine(curPen, current_point.getScreenX(width), current_point.getScreenY(height), next_point.getScreenX(width), next_point.getScreenY(height));
                        current_point = next_point;
                    }
                    g.FillEllipse(curBrush, current_point.getScreenX(width) - height / 100, current_point.getScreenY(height) - height / 100, height / 50, height / 50);
                    g.DrawString(tcur.CursorID + "", font, fntBrush, new PointF(tcur.getScreenX(width) - 10, tcur.getScreenY(height) - 10));
                }
            }
        }


        // draw the objects
        if (objectList.Count > 0)
        {
            lock (objectList)
            {
                foreach (TuioObject tobj in objectList.Values)
                {
                    int ox = tobj.getScreenX(width);
                    int oy = tobj.getScreenY(height);
                    int size = height / 10;

               
                    string objectImagePath = "";
                    if (tobj.SymbolID == 15)
                    {
                        mymenupoints = generatemenu(CountMenuItems);
                        MenuObjs = CreateMenuObjects(mymenupoints);
                        checkrotation(MenuObjs, g, tobj);

                        g = drawmenu(MenuObjs, g);
                        

                    }
                    foreach (TuioObject obj1 in objectList.Values)
                    {
                        foreach (TuioObject obj2 in objectList.Values)
                        {

                            if (obj1.SymbolID == 15 && obj2.SymbolID == 12 && AreObjectsIntersecting(obj1,obj2))
                            {

                                switch (SelectedMenuFlag) // which menu are you at
                                {
                                    case 0://if you're at the first menu 
                                            if (MenuSelectedIndex == 0) //if you select the first option
                                            {
                                                CountMenuItems = 2;
                                                SelectedMenuFlag = 1;
                                                imagePaths = new List<string>{
                                                          "All ceramic crown preparation.png",
                                                        "Anterior three quarter crown.png",
                                                        "Full veneer crown.png",
                                                        "Inlay.png",
                                                        "Pin-Modified three quarter crown.png",
                                                        "Posterior three quarter crown.png",
                                                        "Seven-eighth Crown.png",
                                                        "Seven-eighth Crown.png",
                                                        "Seven-eighth Crown.png"
                                                        };


                                            }
                                            else
                                            {
                                                CountMenuItems = 1;
                                                SelectedMenuFlag = 2; 
                                                imagePaths = new List<string>{
                                                    @"./Crown Dental APP/2d illustrations/Inlay.png",
                                                        };
                                            }
                                        break;
                                    case 1:
                                        CountMenuItems = 4;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }


        // draw the blobs
        if (blobList.Count > 0)
        {
            lock (blobList)
            {
                foreach (TuioBlob tblb in blobList.Values)
                {
                    int bx = tblb.getScreenX(width);
                    int by = tblb.getScreenY(height);
                    float bw = tblb.Width * width;
                    float bh = tblb.Height * height;

                    g.TranslateTransform(bx, by);
                    g.RotateTransform((float)(tblb.Angle / Math.PI * 180.0f));
                    g.TranslateTransform(-bx, -by);

                    g.FillEllipse(blbBrush, bx - bw / 2, by - bh / 2, bw, bh);

                    g.TranslateTransform(bx, by);
                    g.RotateTransform(-1 * (float)(tblb.Angle / Math.PI * 180.0f));
                    g.TranslateTransform(-bx, -by);

                    g.DrawString(tblb.BlobID + "", font, fntBrush, new PointF(bx, by));
                }
            }
        }
        }



    }

    public int checkmainmenu()
    {
        if (objectList.Count > 0)
        {
            lock (objectList)
            {
                foreach (TuioObject tobj in objectList.Values)
                {
                    int ox = tobj.getScreenX(width);
                    int oy = tobj.getScreenY(height);
                    int size = height / 10;

                    /*  g.TranslateTransform(ox, oy);
                        g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
                        g.TranslateTransform(-ox, -oy);

                        g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));

                        g.TranslateTransform(ox, oy);
                        g.RotateTransform(-1 * (float)(tobj.Angle / Math.PI * 180.0f));
                        g.TranslateTransform(-ox, -oy);

                        g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));*/
                    string objectImagePath = "";
                    if (tobj.SymbolID == 12)
                    {
                        return 2;
                    }
                 
                }
            }
        }
        return 1;

    }

    void DrawDubb(Graphics g)
    {
        using (Graphics g2 = Graphics.FromImage(off))
        {
            // Draw on the off-screen buffer (bitmap)
            OnPaintBackground(new PaintEventArgs(g2, new Rectangle(0, 0, off.Width, off.Height)));

            // Draw the off-screen buffer to the on-screen Graphics object
            g.DrawImage(off, 0, 0);
        }

    }
    private Button mainMenuButton;
    private void InitializeComponent()
    {
        this.DoubleBuffered= true;
        // 
        // buttonRJ1
        // 
        // 
        // TuioDemo
        // 
        //this.ClientSize = new System.Drawing.Size(1344, 709);


        this.Text = "Crown Preparations Interactive App";
        mainMenuButton = new Button();
        mainMenuButton.Size = new Size(350, 100);
        mainMenuButton.Text = "START"; mainMenuButton.Font = new Font("Calibri", 35, FontStyle.Bold);
        mainMenuButton.ForeColor = Color.White;

        mainMenuButton.FlatAppearance.Equals(FlatStyle.Flat);
        mainMenuButton.Location = new Point((screen_width / 2 )- (350/2), (screen_height / 2) - (50));
        mainMenuButton.TabIndex = 1;
        mainMenuButton.BackColor= Color.Transparent; // Set the desired location
        mainMenuButton.Click += new EventHandler(MainMenuButton_Click);

        // Add the button to the form only if mainmenuflag == 1
        if (mainmenuflag == 1)
        {
            this.Controls.Add(mainMenuButton);
        }

    }

    private void MainMenuButton_Click(object sender, EventArgs e)
    {

        mainmenuflag = 2;

        this.Controls.Remove(mainMenuButton);
        this.mainMenuButton.Dispose();
    }

    public bool AreObjectsIntersecting(TuioObject obj1, TuioObject obj2)
    {
        int obj1X = obj1.getScreenX(width);
        int obj1Y = obj1.getScreenY(height);
        int obj1Size = 260; 

        int obj2X = obj2.getScreenX(width);
        int obj2Y = obj2.getScreenY(height);
        int obj2Size = 100; 

        //this.Text = "(" + obj1X + " , " + obj1Y + ") (" + obj2X + " , " + obj2Y + ") " + "Size: " + obj1Size;

        return obj1X < obj2X + obj2Size && obj1X + obj1Size > obj2X &&
               obj1Y < obj2Y + obj2Size && obj1Y + obj1Size > obj2Y && (obj1.SymbolID == 15 || obj2.SymbolID == 15);
    }

    public static void Main(String[] argv)
    {
        int port = 0;
        switch (argv.Length)
        {
            case 1:
                port = int.Parse(argv[0], null);
                if (port == 0) goto default;
                break;
            case 0:
                port = 3333;
                break;
            default:
                Console.WriteLine("usage: mono TuioDemo [port]");
                System.Environment.Exit(0);
                break;
        }

        TuioDemo app = new TuioDemo(port);
        Application.Run(app);
    }
}
