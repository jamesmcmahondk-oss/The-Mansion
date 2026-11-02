using System;

namespace The_Mansion.Models
{
    /*
     * Someone the player can fight. Written in Data/Enemies.cs.
     */
    public class Enemy
    {
        public string Id { get; set; } = "";

        // Name shown in the fight, e.g. "King Hautain".
        public string Name { get; set; } = "";

        // Added to the enemy's 2d6 each round (the player adds Strength / 2
        // plus their weapon bonus). 3 = feeble, 5 = fair, 7 = tough, 9 = deadly.
        public int Skill { get; set; }

        // Hits take 10 (20 for a double six), so 30 Health = about 3 hits.
        public int Health { get; set; }

        // Picture and placement (same as dialogue portraits).
        public string Portrait { get; set; } = "";
        public double PortraitX { get; set; }
        public double PortraitY { get; set; }
        public double PortraitHeight { get; set; }
        public bool DimScene { get; set; } = true;

        public bool CanFlee { get; set; } = true;

        // Shown when the fight starts / is won.
        public string IntroText { get; set; } = "";
        public string WinText { get; set; } = "";

        // Optional: runs when the player wins / flees.
        public Action<Player>? OnWin { get; set; }
        public Action<Player>? OnFlee { get; set; }
    }

    /*
     * A fight in progress (given to items used during a fight).
     */
    public class Fight
    {
        public Fight(Enemy enemy)
        {
            Enemy = enemy;
            EnemyHealth = enemy.Health;
        }

        public Enemy Enemy { get; }

        public int EnemyHealth { get; set; }

        public bool EnemyDefeated => EnemyHealth <= 0;

        public void HurtEnemy(int damage) => EnemyHealth = Math.Max(0, EnemyHealth - damage);
    }
}
