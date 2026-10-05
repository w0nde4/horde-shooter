using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using NUnit.Framework;

namespace _Project.Scripts.Tests
{
    public class PlayerStatsTests
    {
        private PlayerStatsConfig _mockConfig;

        [SetUp]
        public void SetUp()
        {
            _mockConfig = new PlayerStatsConfig
            {
                baseMaxHealth = 100f,
                baseMoveSpeed = 5f,
                baseDamageMultiplier = 1f,
                baseGrenadeCooldownMultiplier = 1f
            };
        }

        [Test]
        public void ApplyUpgrade_WhenDamageUpgraded_ShouldIncreaseMultiplierBy25Percent()
        {
            // 1. Arrange
            var stats = new PlayerStats(_mockConfig);
            var resolvedDamage = _mockConfig.baseDamageMultiplier;

            stats.StatResolver = (type, fallback) => type == UpgradeType.Damage ? resolvedDamage : fallback;

            stats.OnUpgradeRequested += (type, value) =>
            {
                if (type == UpgradeType.Damage) resolvedDamage = value;
            };

            // 2. Act
            stats.ApplyUpgrade(UpgradeType.Damage);

            // 3. Assert
            Assert.AreEqual(1.25f, stats.DamageMultiplier, "Урон должен увеличиться ровно на 0.25");
        }

        [Test]
        public void ApplyUpgrade_WhenMoveSpeedUpgraded_ShouldFireOnUpgradeRequestedWithCorrectValues()
        {
            // 1. Arrange
            var stats = new PlayerStats(_mockConfig);
            var eventFired = false;
            UpgradeType requestedType = default;
            var requestedValue = 0f;

            stats.OnUpgradeRequested += (type, value) =>
            {
                eventFired = true;
                requestedType = type;
                requestedValue = value;
            };

            // 2. Act
            stats.ApplyUpgrade(UpgradeType.MoveSpeed);

            // 3. Assert
            Assert.IsTrue(eventFired, "Событие OnUpgradeRequested должно было вызваться!");
            Assert.AreEqual(UpgradeType.MoveSpeed, requestedType);
            Assert.AreEqual(7f, requestedValue, "Новая скорость движения должна быть равна 7");
        }

        [Test]
        public void ApplyUpgrade_WhenGrenadeCooldownUpgraded_ShouldDecreaseCooldownMultiplierBy20Percent()
        {
            // 1. Arrange
            var stats = new PlayerStats(_mockConfig);
            var resolvedCooldown = _mockConfig.baseGrenadeCooldownMultiplier;
            stats.StatResolver = (type, fallback) => type == UpgradeType.GrenadeCooldown ? resolvedCooldown : fallback;
        
            stats.OnUpgradeRequested += (type, value) =>
            {
                if (type == UpgradeType.GrenadeCooldown) resolvedCooldown = value;
            };

            // 2. Act
            stats.ApplyUpgrade(UpgradeType.GrenadeCooldown);

            // 3. Assert
            Assert.AreEqual(0.8f, stats.GrenadeCooldownMultiplier, 0.001f, "Кулдаун гранаты должен уменьшиться на 20%");
        }
    }
}
