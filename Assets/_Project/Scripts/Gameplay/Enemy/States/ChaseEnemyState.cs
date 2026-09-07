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
            if (Brain.Target == null)
            {
                Brain.ChangeState(Brain.IdleEnemyState);
                return;
            }

            Brain.SetDestination(Brain.Target.position);

            if (Brain.DistanceToTarget <= Brain.AttackRange)
            {
                Brain.ChangeState(Brain.AttackEnemyState);
            }
        }

        public override void Exit()
        {
            Brain.StopAgent();
        }
    }
}