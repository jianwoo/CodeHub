// Composer.cs:
//    Views and ViewControllers for composing messages
//
// Copyright 2010 Miguel de Icaza
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using CoreGraphics;
using CodeFramework.iOS.Views;
using Foundation;
using UIKit;

namespace CodeFramework.iOS.ViewControllers
{
	
	/// <summary>
	///   Composer is a singleton that is shared through the lifetime of the app,
	///   the public methods in this class reset the values of the composer on 
	///   each invocation.
	/// </summary>
	public class Composer : UIViewController
	{
	    readonly ComposerView _composerView;
	    readonly UINavigationBar _navigationBar;
	    readonly UINavigationItem _navItem;
		internal UIBarButtonItem SendItem;
		UIViewController _previousController;
        public Action<string> ReturnAction;

        public bool EnableSendButton
        {
            get { return SendItem.Enabled; }
            set { SendItem.Enabled = value; }
        }

        private class ComposerView : UIView 
        {
            internal readonly UITextView TextView;
            
            public ComposerView (CGRect bounds) : base (bounds)
            {
                TextView = new UITextView (CGRect.Empty) {
                    Font = UIFont.SystemFontOfSize (18),
                };
                
                // Work around an Apple bug in the UITextView that crashes
                if (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR)
                    TextView.AutocorrectionType = UITextAutocorrectionType.No;

                AddSubview (TextView);
            }

            
            internal void Reset (string text)
            {
                TextView.Text = text;
            }
            
            public override void LayoutSubviews ()
            {
                Resize (Bounds);
            }
            
            void Resize (CGRect bounds)
            {
                TextView.Frame = new CGRect (0, 0, bounds.Width, bounds.Height);
            }
            
            public string Text { 
                get {
                    return TextView.Text;
                }
                set {
                    TextView.Text = value;
                }
            }
        }
		
		public Composer () : base (null, null)
		{
            Title = "New Comment".t();
			EdgesForExtendedLayout = UIRectEdge.None;
			// Navigation Bar
			_navigationBar = new UINavigationBar(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 64))
		                         {AutoresizingMask = UIViewAutoresizing.FlexibleWidth, AutosizesSubviews = true};
		    _navItem = new UINavigationItem ("");

			var close = new UIBarButtonItem (Theme.CurrentTheme.CancelButton, UIBarButtonItemStyle.Plain, (s, e) => CloseComposer());
			_navItem.LeftBarButtonItem = close;
			SendItem = new UIBarButtonItem (Theme.CurrentTheme.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => PostCallback());
			_navItem.RightBarButtonItem = SendItem;

			_navigationBar.PushNavigationItem (_navItem, false);
			
			// Composer
			_composerView = new ComposerView (ComputeComposerSize (CGRect.Empty));
			
			View.AddSubview (_composerView);
			View.AddSubview (_navigationBar);
		}

        public string Text
        {
            get { return _composerView.Text; }
            set { _composerView.Text = value; }
        }

        public string ActionButtonText 
        {
            get { return _navItem.RightBarButtonItem.Title; }
            set { _navItem.RightBarButtonItem.Title = value; }
        }

		public void CloseComposer ()
		{
			SendItem.Enabled = true;
			_previousController.DismissViewController(true, null);
        }

		void PostCallback ()
		{
			SendItem.Enabled = false;
            if (ReturnAction != null)
                ReturnAction(Text);
		}
		
		void KeyboardWillShow (NSNotification notification)
		{
		    var nsValue = notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey) as NSValue;
		    if (nsValue == null) return;
		    var kbdBounds = nsValue.RectangleFValue;
		    _composerView.Frame = ComputeComposerSize (kbdBounds);
		}

	    CGRect ComputeComposerSize (CGRect kbdBounds)
		{
			var view = View.Bounds;
			var nav = _navigationBar.Bounds;

			return new CGRect (0, nav.Height, view.Width, view.Height-kbdBounds.Height-nav.Height);
		}
       
        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
			_navigationBar.Frame = new CGRect (0, 0, View.Bounds.Width, 64);
        }

        [Obsolete]
        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            return true;
        }
		
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillShowNotification"), KeyboardWillShow);
            _composerView.TextView.BecomeFirstResponder ();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
        }
		
		public void NewComment (UIViewController parent, Action<string> action)
		{
            _navItem.Title = Title;
            ReturnAction = action;
            _previousController = parent;
            _composerView.TextView.BecomeFirstResponder ();
            parent.PresentViewController(this, true, null);
		}
	}
}
