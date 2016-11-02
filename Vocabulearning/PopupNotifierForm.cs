﻿using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Vocabulearning
{
    /// <summary>
    /// This is the form of the actual notification window.
    /// </summary>
    internal class PopupNotifierForm : System.Windows.Forms.Form
    {
    /// <summary>
    /// This prevents the Popup from Activating
    /// </summary>
        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                //make sure Top Most property on form is set to false
                //otherwise this doesn't work
                int WS_EX_TOPMOST = 0x00000008;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOPMOST;
                return cp;
            }
        }

        /// <summary>
        /// Event that is raised when the notification window is manually closed.
        /// </summary>
        public event EventHandler CloseClick;

        internal event EventHandler ContextMenuOpened;
        internal event EventHandler ContextMenuClosed;

        private bool mouseOnClose = false;
        private bool mouseOnOptions = false;
        private int heightOfTitle;
        private int widthOfContent;

        #region GDI objects

        private bool gdiInitialized = false;
        private Rectangle rcBody;
        private Rectangle rcForm;
        private LinearGradientBrush brushBody;
        private Brush brushButtonHover;
        private Pen penButtonBorder;
        private Pen penContent;
        private Pen penBorder;
        private Brush brushForeColor;
        private Brush brushContent;
        private Brush brushTitle;

        #endregion

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="parent">PopupNotifier</param>
        public PopupNotifierForm(PopupNotifier parent)
        {
            Parent = parent;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.ShowInTaskbar = false;

            this.VisibleChanged += new EventHandler(PopupNotifierForm_VisibleChanged);
            this.MouseMove += new MouseEventHandler(PopupNotifierForm_MouseMove);
            this.MouseUp += new MouseEventHandler(PopupNotifierForm_MouseUp);
            this.Paint += new PaintEventHandler(PopupNotifierForm_Paint);
        }

        /// <summary>
        /// The form is shown/hidden.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopupNotifierForm_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                mouseOnClose = false;
                mouseOnOptions = false;
            }
        }

        /// <summary>
        /// Used in design mode.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(392, 66);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PopupNotifierForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Gets or sets the parent control.
        /// </summary>
        public new PopupNotifier Parent { get; set; }

        /// <summary>
        /// Add two values but do not return a value greater than 255.
        /// </summary>
        /// <param name="input">first value</param>
        /// <param name="add">value to add</param>
        /// <returns>sum of both values</returns>
        private int AddValueMax255(int input, int add)
        {
            return (input + add < 256) ? input + add : 255;
        }

        /// <summary>
        /// Subtract two values but do not returns a value below 0.
        /// </summary>
        /// <param name="input">first value</param>
        /// <param name="ded">value to subtract</param>
        /// <returns>first value minus second value</returns>
        private int DedValueMin0(int input, int ded)
        {
            return (input - ded > 0) ? input - ded : 0;
        }
               
        /// <summary>
        /// Gets the rectangle of the content text.
        /// </summary>
        private RectangleF RectContentText
        {
            get
            {
                if (Parent.Image != null)
                {
                    widthOfContent = this.Width - Parent.ImagePadding.Left - Parent.ImageSize.Width - Parent.ImagePadding.Right - Parent.ContentPadding.Left - Parent.ContentPadding.Right - 16 - 5;
                    return new RectangleF(
                        Parent.ImagePadding.Left + Parent.ImageSize.Width + Parent.ImagePadding.Right + Parent.ContentPadding.Left,
                        heightOfTitle + Parent.TitlePadding.Bottom + Parent.ContentPadding.Top,
                        widthOfContent, this.Height - Parent.TitlePadding.Top - heightOfTitle - Parent.TitlePadding.Bottom - Parent.ContentPadding.Top - Parent.ContentPadding.Bottom - 1);
                }
                else
                {
                    widthOfContent = this.Width - Parent.ContentPadding.Left - Parent.ContentPadding.Right - 16 - 5;
                    return new RectangleF(
                        25 + Parent.ContentPadding.Left,
                        heightOfTitle + Parent.TitlePadding.Bottom + Parent.ContentPadding.Top,
                        widthOfContent, this.Height - Parent.TitlePadding.Top - heightOfTitle - Parent.TitlePadding.Bottom - Parent.ContentPadding.Top - Parent.ContentPadding.Bottom - 1);
                }
            }
        }

        /// <summary>
        /// gets the rectangle of the close button.
        /// </summary>
        private Rectangle RectClose
        {
            get { return new Rectangle(this.Width - 19, 1, 18, 18); }
        }

        /// <summary>
        /// Gets the rectangle of the options button.
        /// </summary>
        private Rectangle RectOptions
        {
            get { return new Rectangle(this.Width - 19 - 19, 1, 18, 18); }
        }

        /// <summary>
        /// Update form to display hover styles when the mouse moves over the notification form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopupNotifierForm_MouseMove(object sender, MouseEventArgs e)
        {            
            mouseOnClose = RectClose.Contains(e.X, e.Y);
            mouseOnOptions = RectOptions.Contains(e.X, e.Y);
            Invalidate();
        }

        /// <summary>
        /// A mouse button has been released, check if something has been clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopupNotifierForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (RectClose.Contains(e.X, e.Y) && (CloseClick != null))
                {
                    CloseClick(this, EventArgs.Empty);
                }                
                if (RectOptions.Contains(e.X, e.Y) && (Parent.OptionsMenu != null))
                {
                    if (ContextMenuOpened != null)
                    {
                        ContextMenuOpened(this, EventArgs.Empty);
                    }
                    Parent.OptionsMenu.Show(this, new Point(RectOptions.Right - Parent.OptionsMenu.Width, RectOptions.Bottom));
                    Parent.OptionsMenu.Closed += new ToolStripDropDownClosedEventHandler(OptionsMenu_Closed);
                }
            }
        }

        /// <summary>
        /// The options popup menu has been closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OptionsMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            Parent.OptionsMenu.Closed -= new ToolStripDropDownClosedEventHandler(OptionsMenu_Closed);

            if (ContextMenuClosed != null)
            {
                ContextMenuClosed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Create all GDI objects used to paint the form.
        /// </summary>
        private void AllocateGDIObjects()
        {
            brushButtonHover = new SolidBrush(Parent.ButtonHoverColor);
            penButtonBorder = new Pen(Parent.ButtonBorderColor);
            penContent = new Pen(Parent.ContentColor, 2);
            penBorder = new Pen(Parent.BorderColor);
            brushForeColor = new SolidBrush(ForeColor);
            brushContent = new SolidBrush(Parent.ContentColor);
            brushTitle = new SolidBrush(Parent.TitleColor);

            gdiInitialized = true;
        }

        /// <summary>
        /// Free all GDI objects.
        /// </summary>
        private void DisposeGDIObjects()
        {
            if (gdiInitialized)
            {
                brushBody.Dispose();
                brushButtonHover.Dispose();
                penButtonBorder.Dispose();
                penContent.Dispose();
                penBorder.Dispose();
                brushForeColor.Dispose();
                brushContent.Dispose();
                brushTitle.Dispose();
            }
        }

        /// <summary>
        /// Draw the notification form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopupNotifierForm_Paint(object sender, PaintEventArgs e)
        {
            rcBody = new Rectangle(0, 0, this.Width, this.Height);
            rcForm = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            brushBody = new LinearGradientBrush(rcBody, Parent.BodyColor, Parent.BodyColor, LinearGradientMode.Vertical);

            if (!gdiInitialized)
            {
                AllocateGDIObjects();
            }

            // draw window
            e.Graphics.FillRectangle(brushBody, rcBody);
            e.Graphics.DrawRectangle(penBorder, rcForm);
            
            if (mouseOnClose)
            {
                e.Graphics.FillRectangle(brushButtonHover, RectClose);
                e.Graphics.DrawRectangle(penButtonBorder, RectClose);
            }
            e.Graphics.DrawLine(penContent, RectClose.Left + 4, RectClose.Top + 4, RectClose.Right - 4, RectClose.Bottom - 4);
            e.Graphics.DrawLine(penContent, RectClose.Left + 4, RectClose.Bottom - 4, RectClose.Right - 4, RectClose.Top + 4);
           
            if (mouseOnOptions)
            {
                e.Graphics.FillRectangle(brushButtonHover, RectOptions);
                e.Graphics.DrawRectangle(penButtonBorder, RectOptions);
            }
            Rectangle myRectangle = new Rectangle(RectOptions.Left + 4, RectOptions.Top + 4, 11, 11);

            e.Graphics.DrawRectangle(penContent, myRectangle);

            // draw icon
            if (Parent.Image != null)
            {
                e.Graphics.DrawImage(Parent.Image, Parent.ImagePadding.Left + 1, 1 + Parent.ImagePadding.Top, Parent.ImageSize.Width, Parent.ImageSize.Height);
            }

            // calculate height of title
            heightOfTitle = (int)e.Graphics.MeasureString("A", Parent.TitleFont).Height;
            int titleX = Parent.TitlePadding.Left + 25;
            if (Parent.Image != null)
            {
                titleX += Parent.ImagePadding.Left + Parent.ImageSize.Width + Parent.ImagePadding.Right - 25;
            }

            // draw title
            e.Graphics.DrawString(Parent.TitleText, Parent.TitleFont, brushTitle, titleX, Parent.TitlePadding.Top);

            // draw content text, optionally with a bold part
            e.Graphics.DrawString(Parent.ContentText, Parent.ContentFont, brushContent, RectContentText);            
        }

        /// <summary>
        /// Dispose GDI objects.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeGDIObjects();
            }
            base.Dispose(disposing);
        }
    }
}