using System.Drawing;
using CoreGraphics;
using System;

namespace MonoTouch.Dialog
{
	public static class GraphicsUtil {
		
		/// <summary>
		///    Creates a path for a rectangle with rounded corners
		/// </summary>
		/// <param name="rect">
		/// The <see cref="RectangleF"/> rectangle bounds
		/// </param>
		/// <param name="radius">
		/// The <see cref="System.Single"/> size of the rounded corners
		/// </param>
		/// <returns>
		/// A <see cref="CGPath"/> that can be used to stroke the rounded rectangle
		/// </returns>
        public static CGPath MakeRoundedRectPath (CGRect rect, float radius)
		{
			var minx = rect.Left;
            var midx = rect.Left + (rect.Width)/2;
            var maxx = rect.Right;
            var miny = rect.Top;
            var midy = rect.Y+rect.Size.Height/2;
            var maxy = rect.Bottom;

			var path = new CGPath ();
			path.MoveToPoint (minx, midy);
			path.AddArcToPoint (minx, miny, midx, miny, radius);
			path.AddArcToPoint (maxx, miny, maxx, midy, radius);
			path.AddArcToPoint (maxx, maxy, midx, maxy, radius);
			path.AddArcToPoint (minx, maxy, minx, midy, radius);		
			path.CloseSubpath ();
			
			return path;
        }
		
        public static void FillRoundedRect (CGContext ctx, CGRect rect, float radius)
		{
				var p = GraphicsUtil.MakeRoundedRectPath (rect, radius);
				ctx.AddPath (p);
				ctx.FillPath ();
		}

		public static CGPath MakeRoundedPath (nfloat size, nfloat radius)
		{
			nfloat hsize = size/2;
			
			var path = new CGPath ();
			path.MoveToPoint (size, hsize);
			path.AddArcToPoint (size, size, hsize, size, radius);
			path.AddArcToPoint (0, size, 0, hsize, radius);
			path.AddArcToPoint (0, 0, hsize, 0, radius);
			path.AddArcToPoint (size, 0, size, hsize, radius);
			path.CloseSubpath ();
			
			return path;
		}
	}
}

