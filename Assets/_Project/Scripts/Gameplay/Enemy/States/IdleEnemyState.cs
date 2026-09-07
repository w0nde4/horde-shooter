namespace _Project.Scripts.Gameplay.Enemy.States
{
    public class IdleEnemyState : EnemyState
    {
        public IdleEnemyState(EnemyBrain brain) : base(brain) { }

        public override void Enter()
        {
            Brain.StopAgent();
        }

        public override void Update()
        {
            var target = Brain.FindNearestPlayer();
            if (target == null) return;
            Brain.SetTarget(target);
            Brain.ChangeState(Brain.ChaseEnemyState);
        }

        public override void Exit() { }
    }
}