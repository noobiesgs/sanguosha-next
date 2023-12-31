﻿using Noobie.SanGuoSha.Actions;
using System.Collections.Generic;

namespace Noobie.SanGuoSha.Triggers
{
    public class Trigger
    {
        public virtual string Name => GetType().Name;

        public virtual IEnumerator<ActionState> Run(GameEvent gameEvent, GameEventArgs gameEventArgs)
        {
            yield break;
        }
    }
}
