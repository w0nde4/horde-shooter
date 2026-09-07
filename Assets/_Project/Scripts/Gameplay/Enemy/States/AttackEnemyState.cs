namespace _Project.Scripts.Gameplay.Enemy.States
{
    public class AttackEnemyState : EnemyState
    {
        public AttackEnemyState(EnemyBrain brain) : base(brain) { }

        public override void Enter()
        {
            Brain.StopAgent();
        }

        public override void Update()
        {
            if (Brain.Target == null)
            {
                Brain.ChangeState(Brain.IdleEnemyState);
                return;
            }

            Brain.FaceTarget();

            Brain.Combat.TryAttack(Brain.Target);

            if (Brain.DistanceToTarget > Brain.AttackRange)
            {
                Brain.ChangeState(Brain.ChaseEnemyState);
            }
        }

        public override void Exit() { }
    }
}