using FileFlow.Views;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FileFlow.ViewModels
{
    public class ContextControlService
    {
        private List<ContextItem> items = new();

        public ContextControlService()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(ContextItem))))
            {
                items.Add((ContextItem)Activator.CreateInstance(type));
            }
        }

        public IEnumerable<ContextItem> GetContextItems(StorageElement selectedElement)
        {
            foreach (ContextItem item in items)
            {
                if (item.CanBeApplied(selectedElement))
                {
                    yield return item;
                }
            }
        }
    }
    public abstract class ContextItem
    {
        public abstract string Text { get; }

        public abstract bool CanBeApplied(StorageElement target);
        public abstract void Apply(StorageElement target);
    }
    public class RenameContextItem : ContextItem
    {
        public override string Text => "Переименовать";

        [Inject] public ExplorerControl explorer { get; }

        public override bool CanBeApplied(StorageElement target)
        {
            return true;
        }
        public override void Apply(StorageElement elemet)
        {
            explorer.ShowFileCreationView(!elemet.IsFolder, FileCreationView.Action.Rename);
        }
    }
    public class CreateProjectContextItem : ContextItem
    {
        public override string Text => "Создать проект";

        public override bool CanBeApplied(StorageElement target)
        {
            return target.IsFolder;
        }
        public override void Apply(StorageElement elemet)
        {
            throw new NotImplementedException();
        }
    }
    public class DeleteProjectContextItem : ContextItem
    {
        public override string Text => "Удалить проект";

        public override bool CanBeApplied(StorageElement target)
        {
            return target.IsFolder;
        }
        public override void Apply(StorageElement elemet)
        {
            throw new NotImplementedException();
        }
    }
}
