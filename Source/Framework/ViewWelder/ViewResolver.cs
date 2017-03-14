﻿using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using ViewWelder.ViewModels;

namespace ViewWelder
{
    public class ViewResolver : IViewResolver
    {
        private Assembly viewAssembly;
        private ViewResolverInflector viewInflector;
        private IViewBinder viewBinder;

        public ViewResolver(
            Assembly assembly = null,
            ViewResolverInflector inflector = null,
            IViewBinder binder = null)
        {
            if (assembly == null)
                assembly = Assembly.GetEntryAssembly();

            if (inflector == null)
                inflector = new ViewResolverInflector();

            if (binder == null)
                binder = new ViewBinder();

            this.viewAssembly = assembly;
            this.viewInflector = inflector;
            this.viewBinder = binder;
        }

        public FrameworkElement Resolve(ViewModelBase viewModel)
        {
            var viewModelName = viewModel.GetType().FullName;

            var viewName = this.viewInflector.InflectViewName(viewModelName);

            var viewType = this.viewAssembly.GetType(viewName);

            if (viewType == null)
                throw new ViewResolverException($"Unable to find View \"{viewName}\" for ViewModel \"{viewModelName}\".");

            object view = Activator.CreateInstance(viewType);

            if (!(view is FrameworkElement))
                throw new ViewResolverException($"Resolved view \"{viewName}\" for ViewModel \"{viewModelName}\" is not an instance of {nameof(FrameworkElement)}.");

            var initializeComponentMethod = view.GetType().GetMethods().SingleOrDefault(x => x.Name == "InitializeComponent" && !x.GetParameters().Any());

            if (initializeComponentMethod != null)
                initializeComponentMethod.Invoke(view, null);

            var frameworkElement = (FrameworkElement)view;

            this.viewBinder.Bind(viewModel, frameworkElement, this);

            return frameworkElement;
        }
    }
}
