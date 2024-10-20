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

    private Image backgroundImage = Image.FromFile(@"bg.jpg");
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
        this.Name = "TuioDemo";
        this.Text = "TuioDemo";

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
    private double rotationThreshold = 15f;  // Example: 15-degree change
    private double previousRotationAngle = 0f;

    public void checkrotation(List<CActor> objs,Graphics g, TuioObject o )
    {
        if (o.SymbolID == menuMarker) // Assuming marker with SymbolID 0 controls the menu
        {
           

            // Convert the angle to degrees
            double angleDegrees = o.Angle * 180.0 / Math.PI;

            // Normalize the angle to be within 0 to 360 degrees
            if (angleDegrees < 0) angleDegrees += 360;
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
                    MenuObjs[newMenuIndex].color = 1;
                   // Select new menu item
                }
                if (rotationDifference > rotationThreshold)
                {
                    // If the icon has significantly rotated and sound hasn't been played
                    if (!hasPlayedSound)
                    {
                        PlaySoundEffect("menusound.mp3");
                        hasPlayedSound = true;  // Set the flag to prevent repeated plays

                    }
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
    public static void DrawRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using (GraphicsPath path = new GraphicsPath())
        {
            float diameter = radius * 2f;
            SizeF sizeF = new SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(rect.Location, sizeF);

            // Top left arc
            path.AddArc(arc, 180, 90);

            // Top right arc
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right arc
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left arc
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();

            // Fill the rounded rectangle
            g.FillPath(brush, path);
        }
    }

    public void refresh(TuioTime frameTime)
    {
        Invalidate();
    }
    public int MenuIconWidth = 100;
    public int MenuIconHeight = 100;
    public int CountMenuItems = 9;
    public int selectedMenu = 1;
    public int MenuSelectedIndex = 0;
    public List<Point> generatemenu(int n)
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

    public Graphics drawmenu(List<CActor> menuobjs, Graphics g)
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
                selectedIndex = i;
                this.Text = selectedIndex + "";
            }
            
        }
        return g;
    }
    List<CActor> MenuObjs = new List<CActor>();
    public List<CActor> CreateMenuObjects(List<Point> points)
    {
        List <CActor> objs = new List<CActor>();
        for (int i = 0; i < points.Count; i++)
        {
            CActor obj = new CActor();
            obj.X = points[i].X;
            obj.Y = points[i].Y;
            obj.W = MenuIconWidth;
            obj.H = MenuIconHeight;
            obj.color = 0;
            if(i == MenuSelectedIndex)
            {
                obj.color = 1;
            }
            objs.Add(obj);  

        }
        return objs;
    }
    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Getting the graphics object
        Graphics g = pevent.Graphics;
        g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));
        g.Clear(Color.WhiteSmoke);
        g.DrawImage(backgroundImage, new Rectangle(0, 0, width, height));
        SolidBrush brush = new SolidBrush(Color.White);
        System.Drawing.Pen mypen = new System.Drawing.Pen(Color.Black, 5);

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

                /*  g.TranslateTransform(ox, oy);
                    g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
                    g.TranslateTransform(-ox, -oy);

                    g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));

                    g.TranslateTransform(ox, oy);
                    g.RotateTransform(-1 * (float)(tobj.Angle / Math.PI * 180.0f));
                    g.TranslateTransform(-ox, -oy);

                    g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));*/
                    string objectImagePath = "";
                    if (tobj.SymbolID == 15)
                    {
                        mymenupoints = generatemenu(CountMenuItems);
                        MenuObjs = CreateMenuObjects(mymenupoints);
                        checkrotation(MenuObjs, g, tobj);

                        g = drawmenu(MenuObjs, g);
                    }
                    else
                    {
                        g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
                        g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));
                    }
                    foreach (TuioObject obj1 in objectList.Values)
                    {
                        foreach (TuioObject obj2 in objectList.Values)
                        {

                            if (obj1 != obj2 && AreObjectsIntersecting(obj1, obj2))
                            {
                                switch (selectedIndex)
                                {
                                    case 0:
                                        CountMenuItems = 6;
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
