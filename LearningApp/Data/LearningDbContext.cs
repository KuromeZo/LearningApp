using LearningApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.Data;

public class LearningDbContext : DbContext
{
    public LearningDbContext(DbContextOptions<LearningDbContext> options) : base(options) { }
    
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>()
            .HasOne(e => e.Topic)
            .WithMany(e => e.Exercises)
            .HasForeignKey(e => e.TopicId);
        
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Topic>().HasData(
            new Topic { Id = 1, Name = "Переменные и типы данных", 
                Description = "Основы работы с переменными в C++", 
                OrderIndex = 1 },
            new Topic { Id = 2, Name = "Условные операторы", 
                Description = "If, else, switch в C++", 
                OrderIndex = 2 }
        );

        modelBuilder.Entity<Exercise>().HasData(
            new Exercise 
            { 
                Id = 1, TopicId = 1, 
                Title = "Объявление переменных",
                Description = "Объявите переменную типа int с именем 'age' и присвойте ей значение 25",
                StarterCode = "// Напишите ваш код здесь\n",
                ExpectedOutput = "25",
                Solution = "int age = 25;\ncout << age;",
                Difficulty = 1
            }
        );
    }
}