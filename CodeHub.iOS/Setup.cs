// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the Setup type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.Binding.BindingContext;
using CodeFramework.Core.ViewModels;
using CodeFramework.iOS;
using MonoTouch.Dialog;
using CodeFramework.iOS.Views;
using UIKit;
using CodeFramework.iOS.Bindings;

namespace CodeHub.iOS
{
    using Cirrious.MvvmCross.Touch.Platform;
    using Cirrious.MvvmCross.Touch.Views.Presenters;
    using Cirrious.MvvmCross.ViewModels;

    /// <summary>
    ///    Defines the Setup type.
    /// </summary>
    public class Setup : MvxTouchSetup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Setup"/> class.
        /// </summary>
        /// <param name="applicationDelegate">The application delegate.</param>
        /// <param name="presenter">The presenter.</param>
        public Setup(MvxApplicationDelegate applicationDelegate, IMvxTouchViewPresenter presenter)
            : base(applicationDelegate, presenter)
        {
        }

        protected override Assembly[] GetViewAssemblies()
        {
            var list = new List<Assembly>();
            list.AddRange(base.GetViewAssemblies());
            list.Add(typeof(StartupView).Assembly);
            return list.ToArray();
        }

        protected override Assembly[] GetViewModelAssemblies()
        {
            var list = new List<Assembly>();
            list.AddRange(base.GetViewModelAssemblies());
			list.Add(typeof(BaseStartupViewModel).Assembly);
            return list.ToArray();
        }

        protected override Cirrious.CrossCore.Platform.IMvxTrace CreateDebugTrace()
        {
			return new Cirrious.CrossCore.Platform.MvxDebugOnlyTrace();
        }

        protected override void FillBindingNames(IMvxBindingNameRegistry obj)
        {
            base.FillBindingNames(obj);
			obj.AddOrOverwrite(typeof(StyledStringElement), "Tapped");
			obj.AddOrOverwrite(typeof(UISegmentedControl), "ValueChanged");
        }

		protected override void FillTargetFactories(Cirrious.MvvmCross.Binding.Bindings.Target.Construction.IMvxTargetBindingFactoryRegistry registry)
		{
			base.FillTargetFactories(registry);
			registry.RegisterFactory(new Cirrious.MvvmCross.Binding.Bindings.Target.Construction.MvxCustomBindingFactory<UISegmentedControl>("ValueChanged", x => new UISegmentControlBinding(x)));
		}

        /// <summary>
        /// Creates the app.
        /// </summary>
        /// <returns>An instance of IMvxApplication</returns>
        protected override IMvxApplication CreateApp()
        {
            this.CreatableTypes(typeof(BaseViewModel).Assembly)
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            this.CreatableTypes(typeof(TouchViewPresenter).Assembly)
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            this.CreatableTypes(typeof(Core.App).Assembly)
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            this.CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            return new Core.App();
        }
    }
}