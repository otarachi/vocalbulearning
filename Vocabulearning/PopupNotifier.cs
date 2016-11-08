﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;

namespace Vocabulearning
{
    /// <summary>
    /// Non-visual component to show a notification window in the right lower
    /// corner of the screen.
    /// </summary>
    [ToolboxBitmapAttribute(typeof(PopupNotifier), "Icon.ico")]
    [DefaultEvent("Click")]
    public class PopupNotifier : Component
    {
        /// <summary>
        /// Event that is raised when the text in the notification window is clicked.
        /// </summary>
        public event EventHandler Click;

        /// <summary>
        /// Event that is raised when the notification window is manually closed.
        /// </summary>
        public event EventHandler Close;

        /// <summary>
        /// Event that is raised when the notification Appears.
        /// </summary>
        public event EventHandler Appear;

        /// <summary>
        /// Event that is raised when the notification Dissapers 
        /// </summary>
        public event EventHandler Disappear;

        private bool disposed = false;
        private PopupNotifierForm frmPopup;
        private Timer tmrAnimation;
        private Timer tmrWait;

        private bool isAppearing = true;
        private bool mouseIsOn = false;
        private int maxPosition;
        private double maxOpacity;
        private int posStart;
        private int posStop;
        private double opacityStart;
        private double opacityStop;
        private int realAnimationDuration; 
        private System.Diagnostics.Stopwatch sw;

        #region Properties

        [Category("Appearance"), DefaultValue(typeof(Color), "Control")]
        [Description("Color of the window background.")]
        public Color BodyColor { get; set; }

        [Category("Title"), DefaultValue(typeof(Color), "Gray")]
        [Description("Color of the title text.")]
        public Color TitleColor { get; set; }

        [Category("Content"), DefaultValue(typeof(Color), "ControlText")]
        [Description("Color of the content text.")]
        public Color ContentColor { get; set; }

        [Category("Appearance"), DefaultValue(typeof(Color), "WindowFrame")]
        [Description("Color of the window border.")]
        public Color BorderColor { get; set; }

        [Category("Buttons"), DefaultValue(typeof(Color), "WindowFrame")]
        [Description("Border color of the close and options buttons when the mouse is over them.")]
        public Color ButtonBorderColor { get; set; }

        [Category("Buttons"), DefaultValue(typeof(Color), "Highlight")]
        [Description("Background color of the close and options buttons when the mouse is over them.")]
        public Color ButtonHoverColor { get; set; }
                
        [Category("Content")]
        [Description("Font of the content text.")]
        public Font ContentFont { get; set; }

        [Category("Title")]
        [Description("Font of the title.")]
        public Font TitleFont { get; set; }
        
        [Category("Image")]
        [Description("Size of the icon image.")]
        public Size ImageSize
        {
            get
            {
                if (imageSize.Width == 0)
                {
                    if (Image != null)
                    {
                        return Image.Size;
                    }
                    else
                    {
                        return new Size(0, 0);
                    }
                }
                else
                {
                    return imageSize;
                }
            }
            set { imageSize = value; }
        }

        public void ResetImageSize()
        {
            imageSize = Size.Empty;
        }

        private bool ShouldSerializeImageSize()
        {
            return (!imageSize.Equals(Size.Empty));
        }

        private Size imageSize = new Size(0, 0);

        [Category("Image")]
        [Description("Icon image to display.")]
        public Image Image { get; set; }
        
        [Category("Content")]
        [Description("Content text to display.")]
        public string ContentText { get; set; }

        [Category("Title")]
        [Description("Title text to display.")]
        public string TitleText { get; set; }
        
        [Category("Title")]
        [Description("Padding of title text.")]
        public Padding TitlePadding { get; set; }

        private void ResetTitlePadding()
        {
            TitlePadding = Padding.Empty;
        }

        private bool ShouldSerializeTitlePadding()
        {
            return (!TitlePadding.Equals(Padding.Empty));
        }

        [Category("Content")]
        [Description("Padding of content text.")]
        public Padding ContentPadding { get; set; }

        private void ResetContentPadding()
        {
            ContentPadding = Padding.Empty;
        }

        private bool ShouldSerializeContentPadding()
        {
            return (!ContentPadding.Equals(Padding.Empty));
        }

        [Category("Image")]
        [Description("Padding of icon image.")]
        public Padding ImagePadding { get; set; }

        private void ResetImagePadding()
        {
            ImagePadding = Padding.Empty;
        }

        private bool ShouldSerializeImagePadding()
        {
            return (!ImagePadding.Equals(Padding.Empty));
        }

        [Category("Behavior")]
        [Description("Context menu to open when clicking on the options button.")]
        public ContextMenuStrip OptionsMenu { get; set; }
        
        [Category("Behavior"), DefaultValue(false)]
        [Description("Check is mouse enter message.")]
        public bool IsMouseEnter { get; set; }


        [Category("Behavior"), DefaultValue(3000)]
        [Description("Time in milliseconds the window is displayed.")]
        public int Delay { get; set; }

        [Category("Behavior"), DefaultValue(1000)]
        [Description("Time in milliseconds needed to make the window appear or disappear.")]
        public int AnimationDuration { get; set; }

        [Category("Behavior"), DefaultValue(10)]
        [Description("Interval in milliseconds used to draw the animation.")]
        public int AnimationInterval { get; set; }

        [Category("Appearance")]
        [Description("Size of the window.")]
        public Size Size { get; set; }

        #endregion

        /// <summary>
        /// Create a new instance of the popup component.
        /// </summary>
        public PopupNotifier()
        {
            // set default values
            //BodyColor = SystemColors.Control;
            TitleColor = SystemColors.ControlText;
            ContentColor = SystemColors.ControlText;
            BorderColor = SystemColors.WindowFrame;
            ButtonBorderColor = SystemColors.WindowFrame;
            ButtonHoverColor = SystemColors.Control;
            ContentFont = SystemFonts.DialogFont;
            TitleFont = SystemFonts.DialogFont;
            TitlePadding = new Padding(0);
            ContentPadding = new Padding(0);
            ImagePadding = new Padding(0);
            Delay = 3000;
            AnimationInterval = 10;
            AnimationDuration = 1000;
            Size = new Size(400, 100);

            frmPopup = new PopupNotifierForm(this);
            frmPopup.TopMost = true;
            frmPopup.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            frmPopup.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            frmPopup.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            frmPopup.MouseEnter += new EventHandler(frmPopup_MouseEnter);
            frmPopup.MouseLeave += new EventHandler(frmPopup_MouseLeave);
            frmPopup.CloseClick += new EventHandler(frmPopup_CloseClick);
            frmPopup.ContextMenuOpened += new EventHandler(frmPopup_ContextMenuOpened);
            frmPopup.ContextMenuClosed += new EventHandler(frmPopup_ContextMenuClosed);            
            frmPopup.VisibleChanged += new EventHandler(frmPopup_VisibleChanged); 

            tmrAnimation = new Timer();
            tmrAnimation.Tick += new EventHandler(tmAnimation_Tick);

            tmrWait = new Timer();
            tmrWait.Tick += new EventHandler(tmWait_Tick);
        }

        /// <summary>
        /// Show the notification window if it is not already visible.
        /// If the window is currently disappearing, it is shown again.
        /// </summary>
        public void Popup()
        {            

            if (!disposed)
            {
                frmPopup.Size = Size;
                
                int curWidthOfContent;
                if (Image != null)
                {
                    curWidthOfContent = frmPopup.Width - ImagePadding.Left - ImageSize.Width - ImagePadding.Right - ContentPadding.Left - ContentPadding.Right - 40 - 15;
                }
                else
                {
                    curWidthOfContent = frmPopup.Width - ContentPadding.Left - ContentPadding.Right - 40;
                }
                
                //TODO: Tính chiều rộng của title
                SizeF sizeTitle;
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    sizeTitle = g.MeasureString(TitleText, TitleFont);
                }                
                int widthOfContent = (int)sizeTitle.Width + 5;
                if (widthOfContent > curWidthOfContent)
                {
                    frmPopup.Width += widthOfContent - curWidthOfContent;
                    curWidthOfContent = widthOfContent;                    
                }
                curWidthOfContent += 10;
                int heightOfContent;
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    heightOfContent = (int)g.MeasureString(ContentText, ContentFont, curWidthOfContent).Height;
                }

                System.Diagnostics.Debug.WriteLine("Animation started. CWidth: " + curWidthOfContent);
                System.Diagnostics.Debug.WriteLine("Animation started. CHeight: " + heightOfContent);

                int heightOfTitle;
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    heightOfTitle = (int)sizeTitle.Height;
                }

                int curHeightOfContent = frmPopup.Height - TitlePadding.Top - heightOfTitle - TitlePadding.Bottom - ContentPadding.Top - ContentPadding.Bottom - 1;

                if (heightOfContent > curHeightOfContent)
                {
                    frmPopup.Height += heightOfContent - curHeightOfContent + 9;
                }

                posStart = Screen.PrimaryScreen.WorkingArea.Bottom - frmPopup.Height - 20;
                posStop = Screen.PrimaryScreen.WorkingArea.Bottom - frmPopup.Height - 20;

                if (!frmPopup.Visible)
                {       
                    opacityStart = 0;
                    opacityStop = 1;

                    frmPopup.Opacity = opacityStart;
                    frmPopup.Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - frmPopup.Width - 20, posStart);
                    frmPopup.Visible = true;
                    isAppearing = true;

                    tmrWait.Interval = Delay;
                    tmrAnimation.Interval = AnimationInterval;
                    realAnimationDuration = AnimationDuration;
                    tmrAnimation.Start();
                    sw = System.Diagnostics.Stopwatch.StartNew();
                    System.Diagnostics.Debug.WriteLine("Animation started.");
                    System.Diagnostics.Debug.WriteLine("Animation started. Width: " + frmPopup.Size.Width);
                    System.Diagnostics.Debug.WriteLine("Animation started. Height: " + frmPopup.Size.Height);
                }
                else
                {
                    if (!isAppearing)
                    {                 
                        opacityStart = frmPopup.Opacity;
                        opacityStop = 1;
                        isAppearing = true;
                        realAnimationDuration = Math.Max((int) sw.ElapsedMilliseconds,1);
                        sw.Restart();
                        System.Diagnostics.Debug.WriteLine("Animation direction changed.");
                    }
                    frmPopup.Invalidate();
                }
            }
        }

        /// <summary>
        /// Hide the notification window.
        /// </summary>
        public void Hide()
        {
            System.Diagnostics.Debug.WriteLine("Animation stopped.");
            System.Diagnostics.Debug.WriteLine("Wait timer stopped.");
            tmrAnimation.Stop();
            tmrWait.Stop();
            frmPopup.Hide();
        }

        /// <summary>
        /// The custom options menu has been closed. Restart the timer for
        /// closing the notification window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmPopup_ContextMenuClosed(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Menu closed.");
            if (!mouseIsOn)
            {
                tmrWait.Interval = Delay;
                tmrWait.Start();
                System.Diagnostics.Debug.WriteLine("Wait timer started.");
                IsMouseEnter = false;
            }
        }


        /// <summary>
        /// The custom options menu has been opened. The window must not be closed
        /// as long as the menu is open.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmPopup_ContextMenuOpened(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Menu opened.");
            tmrWait.Stop();
            System.Diagnostics.Debug.WriteLine("Wait timer stopped.");
            IsMouseEnter = true;
        }

        /// <summary>
        /// The close button has been clicked. Hide the notification window
        /// and raise the 'Close' event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmPopup_CloseClick(object sender, EventArgs e)
        {
            Hide();
            if (Close != null)
            {
                Close(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Visibility has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmPopup_VisibleChanged(object sender, EventArgs e)
        {
            if (frmPopup.Visible)
            {
                if (Appear != null) Appear(this, EventArgs.Empty);
            }
            else
            {
                if (Disappear != null) Disappear(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Update form position and opacity to show/hide the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmAnimation_Tick(object sender, EventArgs e)
        {
            long elapsed = sw.ElapsedMilliseconds;

            int posCurrent = (int)(posStart + ((posStop - posStart) * elapsed / realAnimationDuration));
            bool neg = (posStop - posStart) < 0;
            if ((neg && posCurrent < posStop) ||
                (!neg && posCurrent > posStop))
            {
                posCurrent = posStop;
            }

            double opacityCurrent = opacityStart + ((opacityStop - opacityStart) * elapsed / realAnimationDuration);
            neg = (opacityStop - opacityStart) < 0;
            if ((neg && opacityCurrent < opacityStop) ||
                (!neg && opacityCurrent > opacityStop))
            {
                opacityCurrent = opacityStop;
            }

            frmPopup.Top = posCurrent;
            frmPopup.Opacity = opacityCurrent;
            
            // animation has ended
            if (elapsed > realAnimationDuration)
            {

                sw.Reset();
                tmrAnimation.Stop();
                System.Diagnostics.Debug.WriteLine("Animation stopped.");

                if (isAppearing)
                {                    
                    posStart = Screen.PrimaryScreen.WorkingArea.Bottom - frmPopup.Height - 20;
                    posStop = Screen.PrimaryScreen.WorkingArea.Bottom - frmPopup.Height - 20;
                    opacityStart = 1;
                    opacityStop = 0;

                    realAnimationDuration = AnimationDuration;

                    isAppearing = false;
                    maxPosition = frmPopup.Top;
                    maxOpacity = frmPopup.Opacity;
                    if (!mouseIsOn)
                    {
                        tmrWait.Stop();
                        tmrWait.Start();
                        System.Diagnostics.Debug.WriteLine("Wait timer started.");
                    }
                }
                else
                {
                    frmPopup.Hide();
                }
            }
        }

        /// <summary>
        /// The wait timer has elapsed, start the animation to hide the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmWait_Tick(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Wait timer elapsed.");
            tmrWait.Stop();
            tmrAnimation.Interval = AnimationInterval;
            tmrAnimation.Start();
            sw.Restart();
            System.Diagnostics.Debug.WriteLine("Animation started.");
        }

        /// <summary>
        /// Start wait timer if the mouse leaves the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmPopup_MouseLeave(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MouseLeave");
            if (frmPopup.Visible && (OptionsMenu == null || !OptionsMenu.Visible))
            {
                tmrWait.Interval = Delay;
                tmrWait.Start();
                System.Diagnostics.Debug.WriteLine("Wait timer started.");
            }
            IsMouseEnter = false;
            mouseIsOn = false;
        }

        /// <summary>
        /// Stop wait timer if the mouse enters the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmPopup_MouseEnter(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MouseEnter");
            if (!isAppearing)
            {
                frmPopup.Top = maxPosition;
                frmPopup.Opacity = maxOpacity;
                tmrAnimation.Stop();
                System.Diagnostics.Debug.WriteLine("Animation stopped.");
            }

            tmrWait.Stop();
            System.Diagnostics.Debug.WriteLine("Wait timer stopped.");
            IsMouseEnter = true;
            mouseIsOn = true;
        }

        /// <summary>
        /// Dispose the notification form.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && frmPopup != null)
                {
                    frmPopup.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
