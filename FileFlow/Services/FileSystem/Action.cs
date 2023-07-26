using System;
using Zenject;

namespace FileFlow.Services
{
    public abstract class Action
    {
        public bool IsCompleted { get; private set; }
        public bool IsConstructed { get; private set; }

        [Inject]
        private void Construct()
        {
            IsConstructed = true;
        }

        /// <summary>
        /// Perform action if valid and not completed yet. Returns <see langword="false"/> if invalid or already completed.
        /// </summary>
        public bool TryPerform()
        {
            if (IsConstructed == false)
            {
                throw new Exception($"You can't perform action before construct. Probably, use tried to perform action out of {nameof(IFileSystemService)}.");
            }

            if (IsCompleted) return false;
            if (IsValid() == false) return false;

            IsCompleted = Perform();
            return IsCompleted;
        }
        /// <summary>
        /// Undo action if valid and already completed. Returns <see langword="false"/> if invalid or not completed.
        /// </summary>
        public bool TryUndo()
        {
            if (IsConstructed == false)
            {
                throw new Exception($"You can't undo action before construct. Probably, use tried to undo action out of {nameof(IFileSystemService)}.");
            }

            if (IsCompleted == false) return false;
            if (IsValid() == false) return false;

            IsCompleted = !Undo();
            return IsCompleted;
        }

        /// <summary>
        /// Is Action arguments valid and perform may be executed
        /// </summary>
        public abstract bool IsValid();
        /// <summary>
        /// Action perform logic function. Will be executed if <see cref="IsValid"/> and <b>not</b> <see cref="IsCompleted"/>
        /// </summary>
        protected abstract bool Perform();
        /// <summary>
        /// Action undo logic function. Will be executed if <see cref="IsValid"/> and <see cref="IsCompleted"/>
        /// </summary>
        protected abstract bool Undo();
    }
}
