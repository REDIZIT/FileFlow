using FileFlow.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Zenject;

namespace FileFlow.ViewModels
{
    public class ContextService
    {
        private List<ContextItem> items = new();

        public ContextService()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(ContextItem))))
            {
                items.Add((ContextItem)Activator.CreateInstance(type));
            }
        }

        public IEnumerable<ContextItem> GetContextItems(DiContainer container, ContextControl control, ContextWorkspace workspace)
        {
            foreach (ContextItem item in items)
            {
                var sub = container.CreateSubContainer();
                sub.Bind<ContextControl>().FromInstance(control);

                sub.Inject(item);

                if (item.CanBeApplied(workspace))
                {
                    yield return item;
                }
            }
        }
    }
    public class ContextWorkspace
    {
        public StorageElement parent;
        public StorageElement selected;
    }
}
