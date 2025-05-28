using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Linq;

namespace cursovaia
{
    public partial class Form1 : Form
    {
        // Игровые переменные
        private Rectangle balloon;
        private List<Rectangle> coins;
        private List<Spike> obstacles = new List<Spike>();
        private Random random;
        private int score;
        private int highScore;
        private bool isGameOver;
        private float baseFallSpeed = 1.5f;
        
        // Кнопки меню
        private Button menuPlayButton;
        private Button menuRecordButton;
        private Button menuExitButton;
        private Button continueButton;
        private Button exitButton;
        
        // Игровые переменные движения
        private float horizontalSpeed;
        private bool isMovingLeft;
        private bool isMovingRight;
        
        // Переменные времени суток и облаков
        private List<Cloud> clouds;
        private float timeOfDay = 0f; // 0 = день, 1 = ночь
        private bool isNightcoming = true;
        private const float TIME_CHANGE_SPEED = 0.001f;  // Скорость изменения времени
        private List<Star> stars = new List<Star>();

        // Цвета неба
        private Color daySkyTop = Color.FromArgb(135, 206, 235);    // Светлое небо
        private Color daySkyBottom = Color.FromArgb(176, 226, 255); // Светлый переход
        private Color nightSkyTop = Color.FromArgb(25, 25, 112);    // Темное небо
        private Color nightSkyBottom = Color.FromArgb(0, 0, 139);   // Темный переход

        private const float HORIZONTAL_SPEED = 5.0f;  // Скорость перемещения по горизонтали
        private float deltaTime = 0.016f;  // Время между кадрами (1/60 секунды)
        private DateTime lastFrame = DateTime.Now;

        private bool isPaused = false; // Флаг паузы для паузы игры

        // Класс UFO
        private class UFO
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Speed { get; set; }
            public bool IsMovingRight { get; set; }
            public float LightPhase { get; set; } // Фаза света
        }

        // Класс UFO
        private UFO currentUFO;
        private bool isUFOActive = false;
        private const float UFO_SPEED = 3.0f;

        // Класс облака
        private class Cloud
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Speed { get; set; }
            public int Size { get; set; }
            public float Opacity { get; set; }
        }

        // Класс звезды
        private class Star
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Size { get; set; }
            public float Brightness { get; set; }
            public float TwinkleSpeed { get; set; }
            public float TwinklePhase { get; set; }
        }

        // Класс космического объекта
        private class SpaceObject
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Speed { get; set; }
            public bool IsMovingRight { get; set; }
            public float Rotation { get; set; }
            public float Size { get; set; }
            public SpaceObjectType Type { get; set; }
            public float Angle { get; set; } // Угол поворота
        }

        private enum SpaceObjectType
        {
            BlackHole,
            Meteor
        }

        // Список космических объектов
        private List<SpaceObject> spaceObjects = new List<SpaceObject>();
        private const float SPACE_OBJECT_SPEED = 1.0f;
        private const float COMET_SPEED = 5.0f;
        private const float METEOR_SPEED = 8.0f;

        // Класс косяка
        private class Spike
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Width { get; set; }
            public float Height { get; set; }

            // Метод получения прямоугольника для косяка
            public Rectangle GetRectangle()
            {
                return new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
            }

            // Метод получения верхней границы косяка
            public float Top => Y;
        }

        public Form1()
        {
            InitializeComponent();
            
            // Настройка формы
            this.Text = "Balloon Odyssey";
            this.ClientSize = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true; // Добавляем это свойство для обработки клавиш
            
            // Инициализация игровых объектов
            balloon = new Rectangle(Width / 2 - 25, Height - 100, 50, 50);
            coins = new List<Rectangle>();
            obstacles = new List<Spike>();
            random = new Random();
            clouds = new List<Cloud>();
            stars = new List<Star>();
            
            // Инициализация облаков и звезд
            InitializeClouds();
            InitializeStars();
            
            // Настройка кнопок меню
            SetupMenuButtons();
            SetupPauseButtons();
            
            // Показываем меню при запуске
            ShowMenu();
            
            // Настройка таймера
            gameTimer = new Timer();
            gameTimer.Interval = 16; // примерно 60 FPS
            gameTimer.Tick += GameTimer_Tick;
            
            // Подключаем обработчики клавиш
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        private void SetupMenuButtons()
        {
            // Кнопка "Играть"
            menuPlayButton = new Button
            {
                Text = "Играть",
                Size = new Size(200, 50),
                Location = new Point(300, 200),
                Font = new Font("Arial", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(100, Color.Black),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            menuPlayButton.Click += MenuPlayButton_Click;
            this.Controls.Add(menuPlayButton);

            // Кнопка "Рекорд"
            menuRecordButton = new Button
            {
                Text = "Рекорд",
                Size = new Size(200, 50),
                Location = new Point(300, 270),
                Font = new Font("Arial", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(100, Color.Black),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            menuRecordButton.Click += MenuRecordButton_Click;
            this.Controls.Add(menuRecordButton);

            // Кнопка "Выход"
            menuExitButton = new Button
            {
                Text = "Выход",
                Size = new Size(200, 50),
                Location = new Point(300, 340),
                Font = new Font("Arial", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(100, Color.Black),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            menuExitButton.Click += MenuExitButton_Click;
            this.Controls.Add(menuExitButton);
        }

        private void SetupPauseButtons()
        {
            // Кнопка "Продолжить"
            continueButton = new Button
            {
                Text = "Продолжить",
                Size = new Size(200, 50),
                Location = new Point(300, 200),
                Font = new Font("Arial", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(100, Color.Black),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            continueButton.Click += ContinueButton_Click;
            this.Controls.Add(continueButton);

            // Кнопка "Выход" для паузы
            exitButton = new Button
            {
                Text = "Выход",
                Size = new Size(200, 50),
                Location = new Point(300, 270),
                Font = new Font("Arial", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(100, Color.Black),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            exitButton.Click += ExitButton_Click;
            this.Controls.Add(exitButton);
        }

        private void ShowMenu()
        {
            menuPlayButton.Visible = true;
            menuRecordButton.Visible = true;
            menuExitButton.Visible = true;
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

        private void StartGame()
        {
            // Скрываем меню
            HideMenu();
            
            // Инициализация игры
            balloon = new Rectangle(Width / 2 - 25, Height - 100, 50, 50);
            coins = new List<Rectangle>();
            obstacles = new List<Spike>();
            score = 0;
            isGameOver = false;
            isMovingLeft = false;
            isMovingRight = false;
            horizontalSpeed = 0;
            isPaused = false;
            
            // Инициализация звезд и облаков
            InitializeStars();
            InitializeClouds();
            
            // Запускаем таймер
            lastFrame = DateTime.Now;
            gameTimer.Start();
            
            // Устанавливаем фокус на форму
            this.Focus();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!isPaused && !isGameOver)
            {
                // Обновляем deltaTime
                DateTime currentFrame = DateTime.Now;
                deltaTime = (float)(currentFrame - lastFrame).TotalSeconds;
                lastFrame = currentFrame;

                // Обновляем время суток
                if (isNightcoming)
                {
                    timeOfDay += TIME_CHANGE_SPEED;
                    if (timeOfDay >= 1f)
                    {
                        timeOfDay = 1f;
                        isNightcoming = false;
                    }
                }
                else
                {
                    timeOfDay -= TIME_CHANGE_SPEED;
                    if (timeOfDay <= 0f)
                    {
                        timeOfDay = 0f;
                        isNightcoming = true;
                    }
                }

                // Обновляем мерцание звезд
                foreach (var star in stars)
                {
                    star.TwinklePhase += star.TwinkleSpeed;
                    if (star.TwinklePhase > Math.PI * 2)
                        star.TwinklePhase -= (float)(Math.PI * 2);
                }

                // Обновляем движение шара
                if (isMovingLeft && balloon.Left > 0)
                    horizontalSpeed = -HORIZONTAL_SPEED;
                else if (isMovingRight && balloon.Right < Width)
                    horizontalSpeed = HORIZONTAL_SPEED;
                else
                    horizontalSpeed = 0;

                balloon.X += (int)(horizontalSpeed * deltaTime * 60);

                // Обновляем монеты
                if (random.Next(30) == 0)
                {
                    coins.Add(new Rectangle(
                        random.Next(Width - 20), 
                        -20, 
                        Math.Max(20, Width / 40),  // ??????????? ?????? 20
                        Math.Max(20, Width / 40)
                    ));
                }

                // Обновляем препятствия
                if (random.Next(50) == 0)
                {
                    obstacles.Add(new Spike
                    {
                        X = random.Next(Width - 30),
                        Y = -30,
                        Width = Math.Max(20, Width / 30),
                        Height = Math.Max(20, Width / 30)
                    });
                }

                // Проверяем столкновение с шаром
                Rectangle hitbox = GetBalloonHitbox();

                // Обновляем скорость падения
                float fallSpeed = baseFallSpeed + (score / 20); // Увеличиваем скорость на 1 за каждые 20 очков

                // Обновляем монеты
                for (int i = coins.Count - 1; i >= 0; i--)
                {
                    Rectangle coin = coins[i];
                    coin.Y += (int)(fallSpeed * deltaTime * 60);
                    coins[i] = coin;

                    if (coin.IntersectsWith(hitbox))
                    {
                        score += 5;
                        coins.RemoveAt(i);
                    }
                    else if (coin.Top > Height + 100)
                    {
                        coins.RemoveAt(i);
                    }
                }

                // Обновляем препятствия
                for (int i = obstacles.Count - 1; i >= 0; i--)
                {
                    Spike obstacle = obstacles[i];
                    obstacle.Y += (int)(fallSpeed * deltaTime * 60); // ?????????? ?????????? ????????
                    obstacles[i] = obstacle;

                    if (obstacle.GetRectangle().IntersectsWith(hitbox))
                    {
                        GameOver();
                    }
                    else if (obstacle.Top > Height + 100)
                    {
                        obstacles.RemoveAt(i);
                    }
                }

                // Обновляем позицию шара
                if (balloon.Left < 0)
                    balloon.X = 0;
                if (balloon.Right > Width)
                    balloon.X = Width - balloon.Width;

                // Обновляем облака
                for (int i = clouds.Count - 1; i >= 0; i--)
                {
                    clouds[i].X -= clouds[i].Speed * deltaTime * 60;
                    if (clouds[i].X + clouds[i].Size < -50)
                    {
                        clouds[i].X = Width + 50;
                        clouds[i].Y = random.Next(0, Height);
                    }
                }

                // Обновляем UFO
                SpawnUFO();
                if (isUFOActive)
                {
                    currentUFO.LightPhase += 0.1f;
                    if (currentUFO.IsMovingRight)
                    {
                        currentUFO.X += currentUFO.Speed * deltaTime * 60;
                        if (currentUFO.X > Width + 100)
                        {
                            isUFOActive = false;
                        }
                    }
                    else
                    {
                        currentUFO.X -= currentUFO.Speed * deltaTime * 60;
                        if (currentUFO.X < -100)
                        {
                            isUFOActive = false;
                        }
                    }
                }

                // Обновляем космические объекты
                SpawnSpaceObject();

                // Обновляем метеориты
                for (int i = spaceObjects.Count - 1; i >= 0; i--)
                {
                    var obj = spaceObjects[i];
                    if (obj.Type == SpaceObjectType.Meteor)
                    {
                        obj.X += obj.Speed * deltaTime * 60;
                        if (obj.X > Width + 100)
                            spaceObjects.RemoveAt(i);
                    }
                }
            }

            // Обновляем экран
            Refresh();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true; // Добавляем это для предотвращения системных звуков
            
            switch (e.KeyCode)
            {
                case Keys.Left:
                    isMovingLeft = true;
                    break;
                case Keys.Right:
                    isMovingRight = true;
                    break;
                case Keys.Escape:
                    if (!isGameOver)
                    {
                        isPaused = !isPaused;
                        if (isPaused)
                        {
                            gameTimer.Stop();
                            continueButton.Visible = true;
                            exitButton.Visible = true;
                        }
                        else
                        {
                            lastFrame = DateTime.Now;
                            gameTimer.Start();
                            continueButton.Visible = false;
                            exitButton.Visible = false;
                        }
                        this.Focus(); // Возвращаем фокус форме
                    }
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true; // Добавляем это для предотвращения системных звуков
            
            switch (e.KeyCode)
            {
                case Keys.Left:
                    isMovingLeft = false;
                    break;
                case Keys.Right:
                    isMovingRight = false;
                    break;
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            // ????????? ??????????
            Application.Exit();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // ???????????? ?????? ????
            Color currentSkyTop = InterpolateColor(daySkyTop, nightSkyTop, timeOfDay);
            Color currentSkyBottom = InterpolateColor(daySkyBottom, nightSkyBottom, timeOfDay);

            // ?????? ??????????? ????
            using (LinearGradientBrush skyBrush = new LinearGradientBrush(
                new Point(0, 0),
                new Point(0, Height),
                currentSkyTop,
                currentSkyBottom))
            {
                g.FillRectangle(skyBrush, ClientRectangle);
            }

            // ?????? ?????? ?????? ?????
            if (timeOfDay > 0.5f)
            {
                float starBrightness = Math.Min(1, (timeOfDay - 0.5f) * 2);
                foreach (var star in stars)
                {
                    // ????????? ????????
                    float twinkle = (float)(Math.Sin(star.TwinklePhase) * 0.3 + 0.7);
                    float finalBrightness = star.Brightness * starBrightness * twinkle;

                    // ?????? ???????? ??????
                    using (GraphicsPath starGlow = new GraphicsPath())
                    {
                        float glowSize = star.Size * 3;
                        starGlow.AddEllipse(
                            star.X - glowSize/2,
                            star.Y - glowSize/2,
                            glowSize,
                            glowSize);

                        using (PathGradientBrush glowBrush = new PathGradientBrush(starGlow))
                        {
                            glowBrush.CenterColor = Color.FromArgb(
                                (int)(100 * finalBrightness), Color.White);
                            glowBrush.SurroundColors = new Color[] { 
                                Color.FromArgb(0, Color.White) };
                            g.FillPath(glowBrush, starGlow);
                        }
                    }

                    // ?????? ???? ??????
                    using (SolidBrush starBrush = new SolidBrush(
                        Color.FromArgb((int)(255 * finalBrightness), Color.White)))
                    {
                        g.FillEllipse(starBrush, 
                            star.X - star.Size/2,
                            star.Y - star.Size/2,
                            star.Size,
                            star.Size);
                    }
                }
            }

            // ?????? ??????
            using (SolidBrush cloudBrush = new SolidBrush(Color.White))
            {
                foreach (var cloud in clouds)
                {
                    cloudBrush.Color = Color.FromArgb((int)(255 * cloud.Opacity), Color.White);
                    DrawCloud(g, cloudBrush, cloud);
                }
            }

            // ?????? ?????? ? ???????? ????????
            foreach (Rectangle coin in coins)
            {
                // ??????? ????????
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(coin);
                    using (PathGradientBrush glow = new PathGradientBrush(path))
                    {
                        glow.CenterColor = Color.FromArgb(100, Color.Yellow);
                        glow.SurroundColors = new Color[] { Color.FromArgb(0, Color.Yellow) };
                        g.FillPath(glow, path);
                    }
                }
                // ???? ??????
                g.FillEllipse(Brushes.Gold, coin);
                // ???? ?? ??????
                g.FillEllipse(new SolidBrush(Color.FromArgb(100, Color.White)),
                    new Rectangle(coin.X + coin.Width/4, coin.Y + coin.Height/4,
                                coin.Width/4, coin.Height/4));
            }

            // ?????? ???? ? ??????????
            foreach (var spike in obstacles)
            {
                DrawSpike(g, spike);
            }

            // ?????? ????????? ??? ? ?????????? ????????
            DrawBalloon(g, balloon);

            // ?????? ???? ? ?????
            using (Font scoreFont = new Font("Arial", 20, FontStyle.Bold))
            {
                string scoreText = $"Score: {score}";
                string recordText = $"Total score: {highScore}";
                
                // ???? ??? ??????
                g.DrawString(scoreText, scoreFont, new SolidBrush(Color.FromArgb(100, Color.Black)), 12, 12);
                g.DrawString(recordText, scoreFont, new SolidBrush(Color.FromArgb(100, Color.Black)), 12, 42);
                
                // ??? ?????
                g.DrawString(scoreText, scoreFont, Brushes.White, 10, 10);
                g.DrawString(recordText, scoreFont, Brushes.White, 10, 40);
            }

            // ????????? ????????? ? ?????
            if (isPaused)
            {
                using (Font pauseFont = new Font("Arial", 30, FontStyle.Bold))
                {
                    string pauseText = "PAUSE";
                    SizeF textSize = e.Graphics.MeasureString(pauseText, pauseFont);

                    // ??????? ?????????????? ???
                    using (SolidBrush overlay = new SolidBrush(Color.FromArgb(128, Color.Black)))
                    {
                        e.Graphics.FillRectangle(overlay, 0, 0, Width, Height);
                    }

                    // ?????? ????? ? ?????
                    float x = (Width - textSize.Width) / 2;
                    float y = (Height - textSize.Height) / 2;

                    e.Graphics.DrawString(pauseText, pauseFont, 
                        new SolidBrush(Color.FromArgb(100, Color.Black)), 
                        x + 2, y + 2);
                    e.Graphics.DrawString(pauseText, pauseFont, 
                        Brushes.White, x, y);
                }
            }

            // ?????? ???
            if (isUFOActive && timeOfDay > 0.5f)
            {
                DrawUFO(g, currentUFO);
            }

            // ?????? ??????????? ???????
            if (timeOfDay > 0.8f)
            {
                // ?????? ???????
                foreach (var obj in spaceObjects.Where(x => x.Type == SpaceObjectType.Meteor))
                {
                    DrawMeteor(g, obj);
                }
            }
        }

        private void DrawCloud(Graphics g, Brush brush, Cloud cloud)
        {
            float x = cloud.X;
            float y = cloud.Y;
            float size = cloud.Size;

            g.FillEllipse(brush, x, y, size, size * 0.6f);
            g.FillEllipse(brush, x + size * 0.4f, y - size * 0.2f, size * 0.8f, size * 0.6f);
            g.FillEllipse(brush, x + size * 0.8f, y, size * 0.6f, size * 0.4f);
        }

        private void DrawBalloon(Graphics g, Rectangle rect)
        {
            // ???? ?????????? ????
            using (GraphicsPath shadowPath = new GraphicsPath())
            {
                shadowPath.AddEllipse(new Rectangle(rect.X + 5, rect.Y + 5, rect.Width, rect.Height - 20));
                using (PathGradientBrush shadow = new PathGradientBrush(shadowPath))
                {
                    shadow.CenterColor = Color.FromArgb(50, Color.Black);
                    shadow.SurroundColors = new Color[] { Color.FromArgb(0, Color.Black) };
                    g.FillPath(shadow, shadowPath);
                }
            }

            // ??????? ? ??????????
            Rectangle basket = new Rectangle(rect.X + 10, rect.Bottom - 20, rect.Width - 20, 20);
            using (LinearGradientBrush basketBrush = new LinearGradientBrush(
                basket,
                Color.SaddleBrown,
                Color.Brown,
                90f))
            {
                g.FillRectangle(basketBrush, basket);
            }

            // ??? ? ??????????
            Rectangle ball = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height - 20);
            using (LinearGradientBrush balloonBrush = new LinearGradientBrush(
                ball,
                Color.Red,
                Color.DarkRed,
                45f))
            {
                g.FillEllipse(balloonBrush, ball);
            }

            // ???? ?? ????
            using (GraphicsPath highlightPath = new GraphicsPath())
            {
                Rectangle highlight = new Rectangle(
                    ball.X + ball.Width/4,
                    ball.Y + ball.Height/4,
                    ball.Width/3,
                    ball.Height/3);
                highlightPath.AddEllipse(highlight);
                using (PathGradientBrush highlightBrush = new PathGradientBrush(highlightPath))
                {
                    highlightBrush.CenterColor = Color.FromArgb(60, Color.White);
                    highlightBrush.SurroundColors = new Color[] { Color.FromArgb(0, Color.White) };
                    g.FillPath(highlightBrush, highlightPath);
                }
            }

            // ???????
            using (Pen ropePen = new Pen(Color.Black, 2))
            {
                g.DrawLine(ropePen, rect.X + 10, rect.Bottom - 20, rect.X + 10, rect.Y + rect.Height / 2);
                g.DrawLine(ropePen, rect.Right - 10, rect.Bottom - 20, rect.Right - 10, rect.Y + rect.Height / 2);
            }
        }

        private Rectangle GetBalloonHitbox()
        {
            // ??????? ??????? ??????? ? ?????? ????
            int hitboxSize = 30;
            return new Rectangle(
                balloon.X + (balloon.Width - hitboxSize) / 2,
                balloon.Y + (balloon.Height - hitboxSize) / 2,
                hitboxSize,
                hitboxSize
            );
        }

        private void GameOver()
        {
            isGameOver = true;
            gameTimer.Stop();
            highScore = Math.Max(highScore, score);

            DialogResult result = MessageBox.Show(
                $"Игра окончена!\nСчет: {score}\nРекорд: {highScore}\n\nХотите сыграть еще раз?",
                "Игра окончена",
                MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                StartGame();
            }
            else
            {
                ShowMenu(); // Показываем меню при выходе из игры
            }
        }

        private Color InterpolateColor(Color c1, Color c2, float factor)
        {
            return Color.FromArgb(
                (int)(c1.R + (c2.R - c1.R) * factor),
                (int)(c1.G + (c2.G - c1.G) * factor),
                (int)(c1.B + (c2.B - c1.B) * factor));
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // ???????? ??????? ??????????? ?? ?????? Windows
                return cp;
            }
        }

        // ????????? ????? ???????? ???
        private void SpawnUFO()
        {
            // ????????? ???? ????????? ???
            if (!isUFOActive && timeOfDay > 0.8f && random.Next(400) == 0) // ???? ????????? ?????
            {
                isUFOActive = true;
                bool movingRight = random.Next(2) == 0;
                currentUFO = new UFO
                {
                    X = movingRight ? -100 : Width + 100,
                    Y = random.Next(20, Height / 6), // ?????? ????, ? ??????? ?????? ????? ??????
                    Speed = UFO_SPEED,
                    IsMovingRight = movingRight,
                    LightPhase = 0
                };
            }
        }

        // ????????? ????? ????????? ???
        private void DrawUFO(Graphics g, UFO ufo)
        {
            int ufoWidth = 60; // ????????? ??????
            int ufoHeight = 20;
            
            // ?????? ???????? ???
            using (GraphicsPath glowPath = new GraphicsPath())
            {
                glowPath.AddEllipse(ufo.X - ufoWidth/2 - 15, ufo.Y - ufoHeight/2 - 15,
                    ufoWidth + 30, ufoHeight + 30);
                
                using (PathGradientBrush glowBrush = new PathGradientBrush(glowPath))
                {
                    glowBrush.CenterColor = Color.FromArgb(30, Color.Aqua); // ????? ?????? ????????
                    glowBrush.SurroundColors = new Color[] { Color.FromArgb(0, Color.Aqua) };
                    g.FillPath(glowBrush, glowPath);
                }
            }

            // ?????? ?????? ???
            using (GraphicsPath ufoPath = new GraphicsPath())
            {
                // ??????? ????? (?????)
                RectangleF dome = new RectangleF(ufo.X - ufoWidth/3, ufo.Y - ufoHeight/2,
                    ufoWidth*2/3, ufoHeight/2);
                ufoPath.AddArc(dome, 0, 180);

                // ?????? ????? (??????)
                RectangleF body = new RectangleF(ufo.X - ufoWidth/2, ufo.Y,
                    ufoWidth, ufoHeight/3);
                ufoPath.AddArc(body, 180, 180);
                ufoPath.CloseFigure();

                // ??????? ??????? ? ??????????
                using (LinearGradientBrush ufoBrush = new LinearGradientBrush(
                    new PointF(ufo.X, ufo.Y - ufoHeight/2),
                    new PointF(ufo.X, ufo.Y + ufoHeight/2),
                    Color.FromArgb(200, 220, 220), // ????? ????????????? ????
                    Color.FromArgb(100, 100, 120)))
                {
                    g.FillPath(ufoBrush, ufoPath);
                }

                // ????????? ???? ?? ??????
                using (GraphicsPath highlightPath = new GraphicsPath())
                {
                    RectangleF highlight = new RectangleF(
                        ufo.X - ufoWidth/6,
                        ufo.Y - ufoHeight/2,
                        ufoWidth/3,
                        ufoHeight/4);
                    highlightPath.AddEllipse(highlight);
                    using (PathGradientBrush highlightBrush = new PathGradientBrush(highlightPath))
                    {
                        highlightBrush.CenterColor = Color.FromArgb(40, Color.White);
                        highlightBrush.SurroundColors = new Color[] { Color.FromArgb(0, Color.White) };
                        g.FillPath(highlightBrush, highlightPath);
                    }
                }
            }

            // ?????? ????????? ????
            float[] lightPositions = { -0.7f, -0.35f, 0f, 0.35f, 0.7f };
            foreach (float pos in lightPositions)
            {
                float brightness = (float)(Math.Sin(ufo.LightPhase + pos * Math.PI) * 0.5 + 0.5);
                using (GraphicsPath lightPath = new GraphicsPath())
                {
                    lightPath.AddEllipse(
                        ufo.X + pos * ufoWidth/2 - 3,
                        ufo.Y + ufoHeight/4,
                        6, 4);

                    using (PathGradientBrush lightBrush = new PathGradientBrush(lightPath))
                    {
                        lightBrush.CenterColor = Color.FromArgb(
                            (int)(255 * brightness),
                            Color.FromArgb(255, 200, 0)); // ????? ?????? ???? ?????
                        lightBrush.SurroundColors = new Color[] { Color.FromArgb(0, Color.Yellow) };
                        g.FillPath(lightBrush, lightPath);
                    }
                }
            }
        }

        private void SpawnSpaceObject()
        {
            if (timeOfDay > 0.8f) // ?????? ???????? ?????
            {
                // ????? ????????? ?????? ????????
                if (!spaceObjects.Any(x => x.Type == SpaceObjectType.BlackHole) && random.Next(2000) == 0)
                {
                    SpawnBlackHole();
                }
                else if (random.Next(300) == 0) // ??????????? ???? ????????? ????????
                {
                    SpawnMeteor();
                }
            }
        }

        private void SpawnMeteor()
        {
            bool movingRight = random.Next(2) == 0;
            float angle = (float)(random.NextDouble() * Math.PI / 4 - Math.PI / 8); // ???? ?? -22.5 ?? 22.5 ????????
            spaceObjects.Add(new SpaceObject
            {
                X = movingRight ? -20 : Width + 20,
                Y = random.Next(10, Height / 3),
                Speed = METEOR_SPEED,
                IsMovingRight = movingRight,
                Size = random.Next(8, 12),
                Type = SpaceObjectType.Meteor,
                Angle = angle // ????????????? ????
            });
        }

        private void DrawMeteor(Graphics g, SpaceObject meteor)
        {
            // ?????? ??????? ?????????? ????
            using (GraphicsPath trailPath = new GraphicsPath())
            {
                float trailLength = meteor.Size * 15; // ??????????? ????? ?????
                float trailX = meteor.IsMovingRight ? meteor.X - trailLength : meteor.X + trailLength;
                
                // ??????? ????? ?????? ? ??????? ????
                PointF[] trailPoints = new PointF[]
                {
                    new PointF(meteor.X, meteor.Y),
                    new PointF(meteor.X, meteor.Y + meteor.Size/4),
                    new PointF(trailX, meteor.Y + meteor.Size/8),
                    new PointF(trailX, meteor.Y - meteor.Size/8)
                };
                trailPath.AddClosedCurve(trailPoints, 0.5f);

                // ??????? ??????????? ??????? ??? ?????
                using (PathGradientBrush trailBrush = new PathGradientBrush(trailPath))
                {
                    trailBrush.CenterColor = Color.White; // ????? ????? ?????
                    trailBrush.CenterPoint = new PointF(meteor.X, meteor.Y);
                    
                    // ??????? ??????????? ????????? ?????
                    Color[] surroundColors = new Color[trailPoints.Length];
                    for (int i = 0; i < surroundColors.Length; i++)
                    {
                        surroundColors[i] = Color.FromArgb(0, Color.White);
                    }
                    trailBrush.SurroundColors = surroundColors;
                    
                    g.FillPath(trailBrush, trailPath);
                }

                // ????????? ?????????????? ????????
                using (GraphicsPath glowPath = new GraphicsPath())
                {
                    glowPath.AddClosedCurve(trailPoints, 0.7f);
                    using (PathGradientBrush glowBrush = new PathGradientBrush(glowPath))
                    {
                        glowBrush.CenterColor = Color.FromArgb(100, Color.White);
                        glowBrush.CenterPoint = new PointF(meteor.X, meteor.Y);
                        glowBrush.SurroundColors = new Color[] { Color.FromArgb(0, Color.White) };
                        g.FillPath(glowBrush, glowPath);
                    }
                }
            }

            // ?????? ????? ????? ? ?????? ?????
            using (GraphicsPath meteorPath = new GraphicsPath())
            {
                float headSize = meteor.Size / 2;
                meteorPath.AddEllipse(
                    meteor.X - headSize/2,
                    meteor.Y - headSize/2,
                    headSize,
                    headSize);

                using (PathGradientBrush meteorBrush = new PathGradientBrush(meteorPath))
                {
                    meteorBrush.CenterColor = Color.White;
                    meteorBrush.SurroundColors = new Color[] { Color.FromArgb(0, Color.White) };
                    g.FillPath(meteorBrush, meteorPath);
                }
            }
        }

        private void DrawSpike(Graphics g, Spike spike)
        {
            // ?????? ??????????? ??? ????, ???????????? ????? ??????
            PointF[] spikePoints = new PointF[]
            {
                new PointF(spike.X, spike.Y + spike.Height), // ??????? (?????? ?????)
                new PointF(spike.X - spike.Width / 2, spike.Y), // ????? ????
                new PointF(spike.X + spike.Width / 2, spike.Y)  // ?????? ????
            };

            using (Brush spikeBrush = new SolidBrush(Color.Red)) // ???? ?????
            {
                g.FillPolygon(spikeBrush, spikePoints);
            }
        }

        // ?????? ?????????? ?????
        private void AddSpike()
        {
            obstacles.Add(new Spike
            {
                X = random.Next(0, Width),
                Y = random.Next(Height / 2, Height - 20), // ?????????, ??? ???? ?? ??????? ?? ??????? ??????
                Width = 20,
                Height = 30
            });
        }

        private void SpawnBlackHole()
        {
            spaceObjects.Add(new SpaceObject
            {
                X = Width / 2,
                Y = Height / 4,
                Size = 40,
                Type = SpaceObjectType.BlackHole,
                Rotation = 0
            });
        }

        private void DrawBlackHole(Graphics g, SpaceObject blackHole)
        {
            // ??????? ?????? ??????????? ????????????
            using (GraphicsPath distortionPath = new GraphicsPath())
            {
                float size = blackHole.Size * 3;
                distortionPath.AddEllipse(
                    blackHole.X - size/2,
                    blackHole.Y - size/2,
                    size, size);

                using (PathGradientBrush distortionBrush = new PathGradientBrush(distortionPath))
                {
                    Color[] colors = new Color[8];
                    for (int i = 0; i < 8; i++)
                    {
                        colors[i] = Color.FromArgb(0, Color.Purple);
                    }
                    distortionBrush.SurroundColors = colors;
                    distortionBrush.CenterColor = Color.FromArgb(50, Color.Purple);
                    g.FillPath(distortionBrush, distortionPath);
                }
            }

            // ?????? ????? ?????? ????
            using (GraphicsPath holePath = new GraphicsPath())
            {
                holePath.AddEllipse(
                    blackHole.X - blackHole.Size/2,
                    blackHole.Y - blackHole.Size/2,
                    blackHole.Size, blackHole.Size);

                using (PathGradientBrush holeBrush = new PathGradientBrush(holePath))
                {
                    holeBrush.CenterColor = Color.Black;
                    holeBrush.SurroundColors = new Color[] { Color.FromArgb(100, 0, 0, 0) };
                    g.FillPath(holeBrush, holePath);
                }
            }
        }

        private void ContinueButton_Click(object sender, EventArgs e)
        {
            isPaused = false;
            lastFrame = DateTime.Now; // ?????????? ????? ??? ?????????????
            gameTimer.Start();
            continueButton.Visible = false;
            exitButton.Visible = false;
            this.Focus();
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
                    Size = random.Next(30, 50),
                    Opacity = (float)random.NextDouble()
                });
            }
        }

        private void InitializeStars()
        {
            stars.Clear();
            Random random = new Random();
            for (int i = 0; i < 50; i++) // Создаем 50 звезд
            {
                stars.Add(new Star
                {
                    X = random.Next(0, Width),
                    Y = random.Next(0, Height / 2),
                    Size = random.Next(2, 4),
                    Brightness = (float)random.NextDouble(),
                    TwinkleSpeed = 0.1f + (float)random.NextDouble() * 0.2f,
                    TwinklePhase = (float)(random.NextDouble() * Math.PI * 2)
                });
            }
        }
    }
}
