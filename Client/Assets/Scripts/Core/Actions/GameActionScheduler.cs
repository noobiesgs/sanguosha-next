#nullable enable

using Cysharp.Threading.Tasks;
using Noobie.SanGuoSha.Triggers;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Noobie.SanGuoSha.Actions
{
    public class GameActionScheduler
    {
        private const ushort MaximumActionLevel = 40960;

        private GameAction? _current;
        private readonly ILogger _logger;

        public GameActionScheduler(ILogger logger)
        {
            _logger = logger;
        }

        internal GameAction? CurrentAction
        {
            get => _current;
            set
            {
                if (_current == value) return;
                _logger.LogDebug("Current Action: {currentAction}, Level: {level}", value?.ToString() ?? "NULL, ", value?.Level.ToString() ?? " -- ");
                _current = value;
            }
        }

        internal void Schedule(IEnumerator<ActionState> routine, string name, bool interruptible = true)
        {
            var action = new GameAction(routine, name, CurrentAction, interruptible);
            CurrentAction!.SubActions.Enqueue(action);
        }

        internal void Schedule(Trigger trigger, GameEvent gameEvent, GameEventArgs args, bool interruptible = true)
        {
            var action = new GameAction(trigger, gameEvent, args, CurrentAction, interruptible);
            CurrentAction!.SubActions.Enqueue(action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootTrigger"></param>
        /// <param name="gameEvent"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public UniTask RunAsync(Trigger rootTrigger, GameEvent gameEvent, GameEventArgs args)
        {
            if (CurrentAction?.IsCompleted != false)
            {
                CurrentAction = new GameAction(rootTrigger, gameEvent, args, null, false);
                return RunAsync();
            }

            throw new InvalidOperationException("There is currently an action being executed");
        }

        private async UniTask RunAsync()
        {
            do
            {
                Debug.Assert(CurrentAction != null);

                if (CurrentAction.Level > MaximumActionLevel)
                {
                    CurrentAction.Abort();
                    var parent = CurrentAction.Parent;
                    while (parent != null)
                    {
                        parent.Abort();
                        parent = parent.Parent;
                    }
                    break;
                }

                switch (CurrentAction)
                {
                    case { IsCompleted: false, SubActions: { Count: > 0 } }:
                        CurrentAction = CurrentAction.SubActions.Dequeue();
                        continue;
                    case { IsCompleted: true, Parent: not null }:
                        CurrentAction = CurrentAction.Parent;
                        continue;
                }

                var moveNext = CurrentAction.MoveNext();
                if (!moveNext)
                {
                    var parent = FindFirstAvailableParent(CurrentAction);
                    if (parent != null)
                    {
                        CurrentAction = parent;
                    }
                    continue;
                }

                var state = CurrentAction.Current;
                if (state != null)
                {
                    switch (state.Type)
                    {
                        case ActionStateType.None:
                            break;
                        case ActionStateType.WaitTask:
                            await state.Task!.Value;
                            break;
                        case ActionStateType.AbortParents:
                            {
                                var parents = GetParents(CurrentAction);
                                foreach (var gameAction in parents.Where(gameAction => state.AbortParentsPredicate!(CurrentAction, gameAction)))
                                {
                                    gameAction.Abort();
                                }
                                break;
                            }
                        case ActionStateType.AbortSiblings:
                            {
                                CurrentAction.Parent?.SubActions.Clear();
                                break;
                            }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            } while (!CurrentAction.IsCompleted);

            CurrentAction = null;
        }

        private static IEnumerable<GameAction> GetParents(GameAction current)
        {
            var parent = current.Parent;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }

        private static GameAction? FindFirstAvailableParent(GameAction current)
        {
            var parent = current.Parent;
            while (parent != null)
            {
                if (!parent.IsCompleted)
                {
                    break;
                }
                parent = parent.Parent;
            }
            return parent;
        }
    }
}
