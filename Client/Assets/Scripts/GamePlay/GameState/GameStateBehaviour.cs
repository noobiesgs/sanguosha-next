using UnityEngine;
using VContainer.Unity;

namespace Noobie.SanGuoSha.GamePlay.GameState
{
    public enum GameState
    {
        MainMenu,
        Lobby,
        Game
    }

    /// <summary>
    /// A special component that represents a discrete game state and its dependencies. The special feature it offers is
    /// that it provides some guarantees that only one such GameState will be running at a time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Q: what is the relationship between a GameState and a Scene?
    /// <br>A: There is a 1-to-many relationship between states and scenes. That is, every scene corresponds to exactly one state,
    ///    but a single state can exist in multiple scenes.</br>
    /// </para> 
    /// <para>
    /// Q: How do state transitions happen?
    /// <br>A: They are driven implicitly by calling NetworkManager.SceneManager.LoadScene in server code. This is
    ///    important, because if state transitions were driven separately from scene transitions, then states that cared what
    ///    scene they ran in would need to carefully synchronize their logic to scene loads.</br>
    /// </para> 
    /// <para>Q: How many GameStateBehaviours are there?
    /// <br>A: Exactly one on the client.</br>
    /// </para> 
    /// <para>
    /// Q: If these are MonoBehaviours, how do you have a single state that persists across multiple scenes?
    /// <br>A: Set your Persists property to true. If you transition to another scene that has the same gamestate, the
    ///    current GameState object will live on, and the version in the new scene will auto-destruct to make room for it.</br>
    /// </para>
    /// Important Note: We assume that every Scene has a GameState object. If not, then it's possible that a Persisting game state
    /// will outlast its lifetime (as there is no successor state to clean it up).
    /// </remarks>
    public abstract class GameStateBehaviour : LifetimeScope
    {
        /// <summary>
        /// Does this GameState persist across multiple scenes?
        /// </summary>
        public virtual bool Persists => false;

        /// <summary>
        /// What GameState this represents. Server and client specializations of a state should always return the same enum.
        /// </summary>
        public abstract GameState ActiveState { get; }

        /// <summary>
        /// This is the single active GameState object. There can be only one.
        /// </summary>
        private static GameObject _activeStateObject;

        protected override void Awake()
        {
            base.Awake();
            if (Parent != null)
            {
                Parent.Container.Inject(this);
            }
        }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            if (_activeStateObject != null)
            {
                if (_activeStateObject == gameObject)
                {
                    //nothing to do here, if we're already the active state object.
                    return;
                }

                var previousState = _activeStateObject.GetComponent<GameStateBehaviour>();
                if (previousState.Persists && previousState.ActiveState == ActiveState)
                {
                    Destroy(gameObject);
                    return;
                }

                Destroy(_activeStateObject);
            }

            _activeStateObject = gameObject;
            if (Persists)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected override void OnDestroy()
        {
            if (!Persists)
            {
                _activeStateObject = null;
            }
        }
    }
}
