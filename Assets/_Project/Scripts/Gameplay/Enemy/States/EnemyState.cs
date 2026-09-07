namespace _Project.Scripts.Gameplay.Enemy.States
{
    public abstract class EnemyState
    {
        protected readonly EnemyBrain Brain;
        protected EnemyState(EnemyBrain brain)
        {
            this.Brain = brain;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
    }
}