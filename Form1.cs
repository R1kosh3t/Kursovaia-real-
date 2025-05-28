using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class Form1 : Form
{
    // Скорость падения
    private float baseSpeed = 1.0f;  // Базовая скорость падения
    private float currentSpeed;      // Текущая скорость
    private const float speedIncrease = 0.0002f;  // Увеличение скорости за каждый кадр
    private float horizontalSpeed;
    private bool isMovingLeft;
    private bool isMovingRight;
    
    // Переменные для смены времени суток
    private List<Cloud> clouds = new List<Cloud>(); // Инициализируем список облаков
    private float timeOfDay = 0f; // 0 = день, 1 = ночь

    public Form1()
    {
        InitializeComponent();
        
        // Настройка формы
        this.Text = "Balloon Odyssey";
        this.ClientSize = new Size(800, 600);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        
        // Инициализация облаков
        InitializeClouds();
        
        // Настройка кнопок меню
        SetupMenuButtons();
        
        // Показываем меню при запуске
        ShowMenu();
        
        // Подключаем обработчики клавиш
        this.KeyDown += Form1_KeyDown;
        this.KeyUp += Form1_KeyUp;
    }

    private void InitializeClouds()
    {
        Random random = new Random();
        for (int i = 0; i < 5; i++)
        {
            clouds.Add(new Cloud
            {
                X = random.Next(0, Width),
                Y = random.Next(0, Height / 2),
                Speed = random.Next(1, 3),
                Size = random.Next(50, 100),
                Opacity = (float)random.NextDouble() * 0.5f + 0.5f
            });
        }
    }

    private void Form1_KeyDown(object sender, KeyEventArgs e)
    {
        // Implementation of key down event
    }

    private void Form1_KeyUp(object sender, KeyEventArgs e)
    {
        // Implementation of key up event
    }

    private void ShowMenu()
    {
        // Implementation of showing menu
    }

    private void SetupMenuButtons()
    {
        // Implementation of setting up menu buttons
    }

    private void StartGame()
    {
        // Инициализация скорости
        currentSpeed = baseSpeed;
        
        // Скрываем меню
        HideMenu();
        
        // Запускаем игровой цикл
        gameTimer.Start();
    }

    private void GameTimer_Tick(object sender, EventArgs e)
    {
        // Увеличиваем скорость со временем
        currentSpeed += speedIncrease;
        
        // Обновляем позиции облаков
        foreach (var cloud in clouds)
        {
            cloud.X -= cloud.Speed;
            if (cloud.X + cloud.Size < 0)
            {
                cloud.X = Width;
            }
        }
        
        // Обновляем время суток
        timeOfDay += 0.0001f;
        if (timeOfDay > 1f) timeOfDay = 0f;
        
        // Перерисовываем форму
        Invalidate();
    }

    private void HideMenu()
    {
        menuPlayButton.Visible = false;
        menuRecordButton.Visible = false;
        menuExitButton.Visible = false;
    }

    private void MenuPlayButton_Click(object sender, EventArgs e)
    {
        StartGame();
    }

    private void MenuRecordButton_Click(object sender, EventArgs e)
    {
        MessageBox.Show($"Ваш рекорд: {highScore}", "Рекорд", MessageBoxButtons.OK);
    }

    private void MenuExitButton_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }
} 