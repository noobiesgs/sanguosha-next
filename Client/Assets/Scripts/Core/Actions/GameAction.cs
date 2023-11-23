#nullable enable
using Noobie.SanGuoSha.Triggers;
using System;
using System.Collections.Generic;

namespace Noobie.SanGuoSha.Actions
{
    public class GameAction
    {
        private IEnumerator<ActionState>? _routine;
        private readonly bool _interruptible;
        private readonly List<string> _abortedSubActions;

        public GameAction(IEnumerator<ActionState> routine, string name, GameAction? parent = null, bool interruptible = true)
        {
            _routine = routine ?? throw new ArgumentNullException(nameof(routine));
            _interruptible = interruptible;
            Parent = parent;
            Name = name;
            SubActions = new Queue<GameAction>();
            Level = (parent?.Level ?? -1) + 1;
            _abortedSubActions = new List<string>();
        }

        public GameAction(IEnumerator<ActionState> routine, GameEvent gameEvent, GameAction? parent = null, bool interruptible = true) : this(routine, gameEvent.Name, parent, interruptible)
        {
            GameEvent = gameEvent ?? throw new ArgumentNullException(nameof(gameEvent));
        }

        internal readonly Queue<GameAction> SubActions;

        internal int Level { get; }

        public GameAction? Parent { get; }

        public GameEvent? GameEvent { get; }

        public string Name { get; }

        public List<string> AbortedSubActions => new(_abortedSubActions);

        internal bool MoveNext()
        {
            if (_routine != null)
            {
                var canMoveNext = _routine.MoveNext();
                if (!canMoveNext)
                {
                    _routine = null;
                    IsCompleted = true;
                }
                return canMoveNext;
            }

            return false;
        }

        internal bool IsCompleted { get; private set; }

        internal void Abort()
        {
            if (!_interruptible) { return; }
            _routine = null;
            IsCompleted = true;
            SubActions.Clear();
            Parent?._abortedSubActions.Add(Name);
        }

        internal ActionState? Current => _routine?.Current;

        public override string ToString()
        {
            var name = Name;
            var parent = Parent;
            while (parent != null)
            {
                name = $"{parent.Name}->{name}";
                parent = parent.Parent;
            }
            return name;
        }
    }
}
