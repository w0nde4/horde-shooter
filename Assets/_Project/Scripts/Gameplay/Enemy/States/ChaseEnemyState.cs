namespace _Project.Scripts.Gameplay.Enemy.States
{
    public class ChaseEnemyState : EnemyState
    {
        public ChaseEnemyState(EnemyBrain brain) : base(brain) { }

        public override void Enter()
        {
            Brain.ResumeAgent();
        }

        public override void Update()
        {
            var nearest = Brain.FindNearestPlayer();

            if (nearest == null)
            {
                Brain.SetTarget(null);
                Brain.ChangeState(Brain.IdleEnemyState);
                return;
            }
            
            if (nearest != Brain.Target)
                Brain.SetTarget(nearest);

            Brain.SetDestination(Brain.Target.position);

            if (Brain.DistanceToTarget <= Brain.AttackRange)
                Brain.ChangeState(Brain.AttackEnemyState);
        }

        public override void Exit()
        {
            Brain.StopAgent();
        }
    }
}