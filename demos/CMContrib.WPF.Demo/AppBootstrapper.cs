using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

namespace Caliburn.Micro.Contrib.Demo
{
    public class AppBootstrapper : Bootstrapper<IShell>
    {
        CompositionContainer container;

        /// <summary>
        /// By default, we are configured to use MEF
        /// </summary>
        protected override void Configure()
        {
            // CMContrib Config stuff
            FrameworkExtensions.Message.Attach.AllowExtraSyntax(MessageSyntaxes.SpecialValueProperty | MessageSyntaxes.XamlBinding);
            FrameworkExtensions.ActionMessage.EnableFilters();
            Localizer.CustomResourceManager = Properties.Demo.ResourceManager;
            // Namespace mapping for custom dialog view
            ViewLocator.AddSubNamespaceMapping("Dialogs", "Demo.Views");
            // or alternatively
            // ViewLocator.AddNamespaceMapping("Caliburn.Micro.Contrib.Dialogs", "Caliburn.Micro.Contrib.Demo.Views");

            var catalog = new AggregateCatalog(
                AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()
                );

            container = new CompositionContainer(catalog);

            var batch = new CompositionBatch();

            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(container);
            batch.AddExportedValue(catalog);

            container.Compose(batch);

            LogManager.GetLog = t => new ConsoleLog(t);
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return base.SelectAssemblies().Concat(new Assembly[] { typeof(ResultExtensions).Assembly });
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = container.GetExportedValues<object>(contract);

            if (exports.Count() > 0)
                return exports.First();

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            container.SatisfyImportsOnce(instance);
        }
    }
}

