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

        public void ShowStats()
        {
            Console.WriteLine($"\n{Name}: Здоровье: {Health}, Урон: {Damage}, Защита: {Armor}");
        }

        public virtual void TakeDamage(int damage)
        {
            int reducedDamage = Math.Max(damage - Armor, 0);
            Health -= reducedDamage;

            Console.WriteLine($"{Name} получает {reducedDamage} урона. Текущее здоровье: {Health}");
        }

        public abstract Fighter Clone();
    }

    public class CriticalHitFighter : Fighter 
    {
        private int _criticalChance = 30;
        private int _criticalHitMultiply = 2;

        public CriticalHitFighter(string name, int health, int damage, int armor) : base(name, health, damage, armor) { }

        public override void Attack(IDamageable target)
        {
            int damage;

            bool isCritical = Utils.TrySuccess(_criticalChance);

            if (isCritical)
                damage = Damage * _criticalHitMultiply;
            else
                damage = Damage;

            if (isCritical)
                Console.WriteLine($"{Name} наносит КРИТИЧЕСКИЙ удар!");

            target.TakeDamage(damage);
        }

        public override Fighter Clone()
        {
            return new CriticalHitFighter(Name, Health, Damage, Armor);
        }
    }

    public class DoubleAttackFighter : Fighter
    {
        private int _attackCounter = 0;
        private int _attackCountForDoubleAttack = 3;
        public DoubleAttackFighter(string name, int health, int damage, int armor) : base(name, health, damage, armor) { }

        public override void Attack(IDamageable target)
        {
            _attackCounter++;

            target.TakeDamage(Damage);

            if (_attackCounter % _attackCountForDoubleAttack == 0)
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
            return new DoubleAttackFighter(Name, Health, Damage, Armor);
        }
    }

    public class RageFighter : Fighter
    {
        private int _rage = 0;
        private int _maxRage = 100;
        private int _ragePerDamage = 25;

        public RageFighter(string name, int health, int damage, int armor) : base(name, health, damage, armor) { }

        public override void Attack(IDamageable target)
        {
            target.TakeDamage(Damage);
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            _rage += _ragePerDamage;

            if (_rage >= _maxRage && Health > 0)
            {
                Heal();
                _rage = 0;
            }
        }

        public override Fighter Clone()
        {
            return new RageFighter(Name, Health, Damage, Armor);
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
        private int _fireballDamage = 15;

        public MageFighter(string name, int health, int damage, int armor) : base(name, health, damage, armor) { }

        public override void Attack(IDamageable target)
        {
            if (_mana >= _fireballCost)
            {
                int increasedDamage = Damage + _fireballDamage;

                Console.WriteLine($"{Name} использует Огненный шар и наносит {increasedDamage} урона!");

                target.TakeDamage(increasedDamage);
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
            return new MageFighter(Name, Health, Damage, Armor);
        }
    }

    public class DodgeFighter : Fighter
    {
        private int _dodgeChance = 25;

        public DodgeFighter(string name, int health, int damage, int armor) : base(name, health, damage, armor) { }

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
            return new DodgeFighter(Name ,Health ,Damage , Armor);
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
            _fighters.Add(new CriticalHitFighter("Берсерк", 100, 20, 5));
            _fighters.Add(new DoubleAttackFighter("Дуэлянт", 100, 18, 6));
            _fighters.Add(new RageFighter("Чародей", 120, 15, 8));
            _fighters.Add(new MageFighter("Маг", 90, 17, 4));
            _fighters.Add(new DodgeFighter("Убийца", 110, 16, 7));
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
