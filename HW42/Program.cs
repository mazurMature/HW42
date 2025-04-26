using System;
using System.Collections.Generic;

namespace HW42
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Arena arena = new Arena();

            arena.Action();
        }
    }

    public static class Utils
    {
        private static Random s_random = new Random();

        public static int GenerateRandomNumber(int minValue, int maxValue)
        {
            return s_random.Next(minValue, maxValue + 1);
        }

        public static bool TrySuccess(int chancePercent)
        {
            int randomNumber = GenerateRandomNumber(1, 100);
            return randomNumber <= chancePercent;
        }

        public static int GetValidInput(int min, int max)
        {
            int input;

            while (int.TryParse(Console.ReadLine(), out input) == false || input < min || input > max)
            {
                Console.WriteLine($"Введите число от {min} до {max}: ");
            }

            return input;
        }
    }

    public interface IDamageable
    {
        void TakeDamage(int damage);
    }

    public abstract class Fighter : IDamageable
    {
        public Fighter(string name, int health, int damage, int armor)
        {
            Name = name;
            Health = health;
            Damage = damage;
            Armor = armor;
        }

        public string Name { get; protected set; }
        public int Health { get; protected set; }
        public int Damage { get; protected set; }
        public int Armor { get; protected set; }

        public abstract void Attack(IDamageable target);

        public virtual void TakeDamage(int damage)
        {
            int reducedDamage = Math.Max(damage - Armor, 0);
            Health -= reducedDamage;

            Console.WriteLine($"{Name} получает {reducedDamage} урона. Текущее здоровье: {Health}");
        }

        public abstract Fighter Clone();

        public virtual void ShowStats()
        {
            Console.WriteLine($"\n{Name}: Здоровье: {Health}, Урон: {Damage}, Защита: {Armor}");
        }
    }

    public class CriticalHitFighter : Fighter
    {
        private int _criticalChance = 30;

        public CriticalHitFighter(string name) : base(name, 100, 20, 5) { }

        public override void Attack(IDamageable target)
        {
            int damage;

            bool isCritical = Utils.TrySuccess(_criticalChance);

            if (isCritical)
                damage = Damage * 2;
            else
                damage = Damage;

            if (isCritical)
                Console.WriteLine($"{Name} наносит КРИТИЧЕСКИЙ удар!");

            target.TakeDamage(damage);
        }

        public override Fighter Clone()
        {
            return new CriticalHitFighter(Name);
        }
    }

    public class DoubleAttackFighter : Fighter
    {
        private int _attackCounter = 0;

        public DoubleAttackFighter(string name) : base(name, 100, 18, 6) { }

        public override void Attack(IDamageable target)
        {
            _attackCounter++;

            target.TakeDamage(Damage);

            if (_attackCounter % 3 == 0)
            {
                Console.WriteLine($"{Name} наносит двойную атаку!");

                target.TakeDamage(Damage);
            }
            else
            {
                Console.WriteLine($"{Name} атакует!");
            }
        }

        public override Fighter Clone()
        {
            return new DoubleAttackFighter(Name);
        }
    }

    public class RageFighter : Fighter
    {
        private int _rage = 0;
        private int _maxRage = 100;

        public RageFighter(string name) : base(name, 120, 15, 8) { }

        public override void Attack(IDamageable target)
        {
            target.TakeDamage(Damage);
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            _rage += 25;

            if (_rage >= _maxRage && Health > 0)
            {
                Heal();
                _rage = 0;
            }
        }

        public override Fighter Clone()
        {
            return new RageFighter(Name);
        }

        private void Heal()
        {
            int healAmount = 30;
            Health += healAmount;
            Console.WriteLine($"{Name} использует ЯРОСТЬ и восстанавливает {healAmount} очков здоровья! Новое здоровье: {Health}");
        }
    }

    public class MageFighter : Fighter
    {
        private int _mana = 100;
        private int _fireballCost = 30;

        public MageFighter(string name) : base(name, 90, 17, 4) { }

        public override void Attack(IDamageable target)
        {
            if (_mana >= _fireballCost)
            {
                int fireballDamage = Damage + 15;

                Console.WriteLine($"{Name} использует Огненный шар и наносит {fireballDamage} урона!");

                target.TakeDamage(fireballDamage);
                _mana -= _fireballCost;
            }
            else
            {
                Console.WriteLine($"{Name} наносит обычную атаку.");

                target.TakeDamage(Damage);
            }
        }

        public override Fighter Clone()
        {
            return new MageFighter(Name);
        }
    }

    public class DodgeFighter : Fighter
    {
        private int _dodgeChance = 25;

        public DodgeFighter(string name) : base(name, 110, 16, 7) { }

        public override void Attack(IDamageable target)
        {
            target.TakeDamage(Damage);
        }

        public override void TakeDamage(int damage)
        {
            bool dodge = Utils.TrySuccess(_dodgeChance);

            if (dodge)
                Console.WriteLine($"{Name} уклоняется от атаки!");
            else
                base.TakeDamage(damage);
        }

        public override Fighter Clone()
        {
            return new DodgeFighter(Name);
        }
    }

    public class Arena
    {
        const string CommandFight = "1";
        const string CommandExit = "2";

        private List<Fighter> _fighters = new List<Fighter>();

        public Arena()
        {
            FillFighters();
        }

        public void Action()
        {
            bool isWork = true;

            while (isWork)
            {
                Console.Clear();
                Console.WriteLine("Добро пожаловать в Колизей!");
                Console.WriteLine($"{CommandFight}. Посмотреть бой");
                Console.WriteLine($"{CommandExit}. Выход");
                Console.Write("Выберите действие: ");

                string input = Console.ReadLine();

                switch (input)
                {
                    case CommandFight:
                        StartFight();
                        Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
                        Console.ReadKey();
                        break;

                    case CommandExit:
                        isWork = false;
                        Console.WriteLine("Выход из программы...");
                        break;

                    default:
                        Console.WriteLine("Некорректный ввод. Нажмите любую клавишу...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        public void StartFight()
        {
            Console.Clear();
            Console.WriteLine("Выберите первого бойца:");
            Fighter fighter1 = ChooseFighter();

            Console.Clear();
            Console.WriteLine("Выберите второго бойца:");
            Fighter fighter2 = ChooseFighter();

            Console.Clear();
            Console.WriteLine($"Бой между {fighter1.Name} и {fighter2.Name} начинается!\n");
            Battle(fighter1, fighter2);
        }

        private void FillFighters()
        {
            _fighters.Add(new CriticalHitFighter("Берсерк"));
            _fighters.Add(new DoubleAttackFighter("Дуэлянт"));
            _fighters.Add(new RageFighter("Чародей"));
            _fighters.Add(new MageFighter("Маг"));
            _fighters.Add(new DodgeFighter("Убийца"));
        }

        private Fighter ChooseFighter()
        {
            for (int i = 0; i < _fighters.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_fighters[i].Name}");
            }

            int index = Utils.GetValidInput(1, _fighters.Count);

            return _fighters[index - 1].Clone();
        }

        private void Battle(Fighter fighter1, Fighter fighter2)
        {
            while (fighter1.Health > 0 && fighter2.Health > 0)
            {
                fighter1.Attack(fighter2);
                Console.WriteLine();
                fighter2.Attack(fighter1);

                Console.WriteLine();
                fighter1.ShowStats();
                fighter2.ShowStats();

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                Console.Clear();
            }

            if (fighter1.Health > 0)
                Console.WriteLine($"{fighter1.Name} ПОБЕЖДАЕТ!");
            else if (fighter2.Health > 0)
                Console.WriteLine($"{fighter2.Name} ПОБЕЖДАЕТ!");
            else
                Console.WriteLine("НИЧЬЯ!");
        }
    }
}
