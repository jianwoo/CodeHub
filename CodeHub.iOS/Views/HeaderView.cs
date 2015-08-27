using System;
using CoreGraphics;
using MonoTouch.Dialog.Utilities;
using UIKit;
using Foundation;

namespace CodeFramework.iOS.Views
{
    public class HeaderView : UIView, IImageUpdated
    {
        private const float XPad = 14f;
        private const float YPad = 10f;

        public static UIFont TitleFont = UIFont.BoldSystemFontOfSize(16);
        public static UIFont SubtitleFont = UIFont.SystemFontOfSize(13);
        public static CGGradient Gradient;
        private string _title;
        private string _subtitle;
        private UIImage _image;

        static HeaderView ()
        {
            using (var rgb = CGColorSpace.CreateDeviceRGB()){
                nfloat [] colorsBottom = {
                    1, 1, 1, 1f,
                    0.97f, 0.97f, 0.97f, 1f
                };
                Gradient = new CGGradient (rgb, colorsBottom, null);
            }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; SetNeedsDisplay(); }
        }

        public string Subtitle
        {
            get { return _subtitle; }
            set { _subtitle = value; SetNeedsDisplay(); }
        }

        public UIImage Image
        {
            get { return _image; }
            set
            {
                if (_image == value)
                    return;
                _image = value; 
                SetNeedsDisplay();
            }
        }

        public string ImageUri
        {
            get { return string.Empty; }
            set 
            {
				_image = string.IsNullOrEmpty(value) ? null : ImageLoader.DefaultRequestImage(new Uri(value), this); 
                SetNeedsDisplay(); 
            }
        }

        public bool ShadowImage { get; set; }

		public UIColor SeperatorColor { get; set; }
        
		public HeaderView()
			: base(new CGRect(0, 0, 0, 60f))
        {
            ShadowImage = true;
            BackgroundColor = UIColor.Clear;
			SeperatorColor = UIColor.FromRGB(199, 199, 204);
//            Layer.MasksToBounds = false;
//            Layer.ShadowColor = UIColor.Gray.CGColor;
//            Layer.ShadowOpacity = 0.4f;
//            Layer.ShadowOffset = new SizeF(0, 1f);
        }

        public void UpdatedImage(Uri uri)
        {
            Image = ImageLoader.DefaultRequestImage(uri, this);
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            var context = UIGraphics.GetCurrentContext();
            nfloat titleY = string.IsNullOrWhiteSpace(Subtitle) ? rect.Height / 2 - TitleFont.LineHeight / 2 : YPad;
            nfloat contentWidth = rect.Width - XPad * 2;
            var midx = rect.Width/2;

            UIColor.White.SetColor ();
            context.FillRect (rect);
            context.DrawLinearGradient (Gradient, new CGPoint (midx, 0), new CGPoint (midx, rect.Height), 0);

            if (Image != null)
            {
                var height = Image.Size.Height > 36 ? 36 : Image.Size.Height;
                var width = Image.Size.Width > 36 ? 36 : Image.Size.Width;
                var top = rect.Height / 2 - height / 2;
                var left = rect.Width - XPad - width;

//                if (ShadowImage)
//                {
//                    context.SaveState();
//                    context.SetFillColor(UIColor.White.CGColor);
//                    context.TranslateCTM(left, top);
//                    context.SetLineWidth(1.0f);
//                    context.SetShadowWithColor(new SizeF(0, 0), 5, UIColor.LightGray.CGColor);
//                    context.AddPath(GraphicsUtil.MakeRoundedPath(width, 4));
//                    context.FillPath();
//                    context.RestoreState();
//                }
//

                Image.Draw(new CGRect(left, top, width, height));
                contentWidth -= (width + XPad * 2); 
            }


            if (!string.IsNullOrEmpty(Title))
            {
                Theme.CurrentTheme.MainTitleColor.SetColor();
                new NSString(Title).DrawString(
                        new CGRect(XPad, titleY, contentWidth, TitleFont.LineHeight),
                        TitleFont,
                        UILineBreakMode.TailTruncation
                );
            }

            if (!string.IsNullOrWhiteSpace(Subtitle))
            {
                Theme.CurrentTheme.MainSubtitleColor.SetColor();
                new NSString(Subtitle).DrawString(
                        new CGRect(XPad, YPad + TitleFont.LineHeight + 2f, contentWidth, SubtitleFont.LineHeight),
                    SubtitleFont,
                    UILineBreakMode.TailTruncation
                );
            }

			context.SetStrokeColor(SeperatorColor.CGColor);
			context.SetLineWidth(1.0f);

			context.BeginPath();
			context.MoveTo(0, 0);
			context.AddLineToPoint(rect.Width, 0);
			context.StrokePath();

			context.BeginPath();
			context.MoveTo(0, rect.Bottom);
			context.AddLineToPoint(rect.Width, rect.Bottom);
			context.StrokePath();
        }
    }
}

