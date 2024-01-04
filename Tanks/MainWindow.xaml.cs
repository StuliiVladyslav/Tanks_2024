using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

///////////////////////////////////////////
namespace Tanks
{
    class Player
    {
        public int HP = 100;
        int Lifes = 3;
        public double playerX = 372;
        public double playerY = 798;
        public double playerAngle = 0.0;
        public double playerAngle_head = 0.0;
        public Player(int hp, int lifes)
        {
            HP = hp;
            Lifes = lifes;
        }
    }



    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MouseMove += Window_MouseMove;
            Loaded += MainWindow_Loaded;
            Loaded += MainWindow_Loaded2;
            enemyMovementTimer = new DispatcherTimer();
            enemyMovementTimer.Interval = TimeSpan.FromMilliseconds(8); // Интервал обновления позиции врагов (можете изменить по вашему усмотрению)
            enemyMovementTimer.Tick += EnemyMovementTimer_Tick;
            enemyMovementTimer.Start();

        }
        private SemaphoreSlim pauseSemaphore = new SemaphoreSlim(0, 1);
        private bool isPaused = false;

        Player Playr1 = new Player(100, 3);
        public static int score = 0;
        public static int kills = 0;
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            while (true)
            {
                // Обновляем интерфейс в UI-потоке с использованием Dispatcher
                Dispatcher.Invoke(() =>
                {
                    LB1.Content =  score;
                    
                });
               
                await Task.Delay(160);
            }
        }
        private async void MainWindow_Loaded2(object sender, RoutedEventArgs e)
        {

            while (true)
            {
                // Обновляем интерфейс в UI-потоке с использованием Dispatcher
                Dispatcher.Invoke(() =>
                {
                    LB1.Content = score;
                    LB1_Copy1.Content = "Wave: " + wave;
                    Kills.Content = "Kills: " + kills;
                    HP_BR.Value = Playr1.HP;
                    if(Playr1.HP <= 0)
                    {
                        if(isPaused == false)
                        {
                            PlayGameOver();
                        }
                        
                        isPaused = true;
                        Button1.Visibility = Visibility.Visible;
                        GO_Copy1.Visibility = Visibility.Visible;
                        GO_Copy1.Content = "Score:           " + score;
                        GO_Copy2.Visibility = Visibility.Visible;
                        GO_Copy2.Content = "Kills:             " + kills;
                        GO_Copy.Visibility = Visibility.Visible;
                        GO_Copy.Content = "Wave:            " + wave;
                        GO.Visibility = Visibility.Visible;
                    }
                });
                CheckAndSpawnEnemies();
                CheckPlayerEnemyCollisions(Playr1);
                await Task.Delay(160);
            }
        }

        private void CheckPlayerEnemyCollisions(Player Playr1)
        {

            // Перебираем всех врагов
            foreach (Enemy enemy in enemies.ToList()) // Преобразуем список в копию, чтобы избежать ошибок при удалении элементов
            {
                // Получаем координаты и размеры игрока и врага
                double playerLeft = Playr1.playerX;
                double playerTop = Playr1.playerY;
                double playerRight = playerLeft + PlayerImage.ActualWidth;
                double playerBottom = playerTop + PlayerImage.ActualHeight;

                double enemyLeft = enemy.X;
                double enemyTop = enemy.Y;
                double enemyRight = enemyLeft + enemy.Image.ActualWidth;
                double enemyBottom = enemyTop + enemy.Image.ActualHeight;

                // Проверяем пересечение
                if (playerRight >= enemyLeft && playerLeft <= enemyRight &&
                    playerBottom >= enemyTop && playerTop <= enemyBottom)
                {
                    // Если есть пересечение, уменьшаем HP игрока и удаляем врага
                    Playr1.HP -= 10;
                   
                }
            }
        }

        public static Dictionary<Enemy, Image> enemyImages = new Dictionary<Enemy, Image>();
        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isPaused)
            {
                await pauseSemaphore.WaitAsync();
            }

            if (e.Key == Key.A || e.Key == Key.Left)
            {
                
                await RotatePlayerAsync(-10); // Асинхронный вызов метода поворота
            }
            else if (e.Key == Key.D || e.Key == Key.Right) 
            {
                await RotatePlayerAsync(10); // Асинхронный вызов метода поворота
            }
            else if (e.Key == Key.W || e.Key == Key.Up)
            {

                await MoveForwardAsync(); // Асинхронный вызов метода движения вперед
            }
            else if (e.Key == Key.S || e.Key == Key.Down)
            {
                await MoveBackwardAsync(); // Асинхронный вызов метода движения назад
            }
            else if (e.Key == Key.Space)
            {
                //for (int i = 0; i < 1; i++)
                //{
                //    await InitializeEnemies();
                //}
            }
        }
        static int attackSpeed = 500;
        private DateTime lastShotTime = DateTime.MinValue; // отслеживание времени последнего выстрела
        private TimeSpan shotCooldown = TimeSpan.FromMilliseconds(attackSpeed);
        private DispatcherTimer enemyMovementTimer;
        async Task PlayShoot()
        {

            MediaPlayer player = new MediaPlayer();


            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            // Обработчик события MediaEnded, который будет вызван при завершении воспроизведения
            player.MediaEnded += (sender, e) =>
            {
                // Помечаем задачу как завершенную
                tcs.SetResult(true);
            };

            // Открываем звуковой файл
            player.Open(new Uri("C:\\Users\\Kille\\source\\repos\\Tanks\\Tanks\\Res\\shoot.wav", UriKind.Relative));

            // Воспроизводим звук
            player.Play();

            // Ожидаем завершения воспроизведения
            await tcs.Task;

            // Освобождаем ресурсы MediaPlayer
            player.Close();
            // break; // Эта строка не нужна в асинхронном методе
        }
        private async void ResetAndAnimateProgressBar()
        {
            // Устанавливаем начальное значение ProgressBar в 100
            PBR.Value = 100;

            // Создаем первую анимацию (уменьшение значения от 100 до 0 за 1 секунду)
            DoubleAnimation animation1 = new DoubleAnimation();
            animation1.From = 100;
            animation1.To = 0;
            animation1.Duration = TimeSpan.FromSeconds(0.1);

            // Подписываемся на событие завершения первой анимации
            animation1.Completed += async (sender, e) =>
            {
                // Ждем 1 секунду перед запуском второй анимации
              ;

                // Создаем вторую анимацию (увеличение значения от 0 до 100 за 4 секунды)
                DoubleAnimation animation2 = new DoubleAnimation();
                animation2.From = 0;
                animation2.To = 100;
                animation2.Duration = TimeSpan.FromSeconds(0.4);

                // Привязываем анимацию к ProgressBar и запускаем ее
                PBR.BeginAnimation(ProgressBar.ValueProperty, animation2);
            };

            // Привязываем первую анимацию к ProgressBar и запускаем ее
            PBR.BeginAnimation(ProgressBar.ValueProperty, animation1);
        }

        private async Task ShootAsync()
        {
            if (isPaused)
            {
                await pauseSemaphore.WaitAsync();
            }
            PlayShoot();
            ResetAndAnimateProgressBar();
           
            double playerHeadX = Canvas.GetLeft(PlayerImage_Head) + PlayerImage_Head.ActualWidth / 2;
            double playerHeadY = Canvas.GetTop(PlayerImage_Head) + PlayerImage_Head.ActualHeight / 2;

            double gunLength = 70.0;
            double startX = playerHeadX + Math.Cos(Playr1.playerAngle_head * (Math.PI / 180)) * gunLength;
            double startY = playerHeadY + Math.Sin(Playr1.playerAngle_head * (Math.PI / 180)) * gunLength;

            Point mousePosition = Mouse.GetPosition(MyCanva);

            double angleRadians = Math.Atan2(mousePosition.Y - startY, mousePosition.X - startX);
            double angleDegrees = angleRadians * (180 / Math.PI);

            Bullet bullet = new Bullet(startX, startY, angleDegrees, 15.0, enemyImages);
            MyCanva.Children.Add(bullet.Image); 

            await bullet.MoveAsync(MyCanva);

        }
        private async Task RotatePlayerAsync(double angle)
        {
            if (isPaused)
            {
                await pauseSemaphore.WaitAsync();
            }
            Playr1.playerAngle += angle;
            RotateTransform rotateTransform = PlayerImage.RenderTransform as RotateTransform;
            if (rotateTransform == null)
            {
                rotateTransform = new RotateTransform();
                PlayerImage.RenderTransform = rotateTransform;
            }

            rotateTransform.Angle = Playr1.playerAngle;
        }



        private async Task MoveForwardAsync()
        {
            double radians = Playr1.playerAngle * (Math.PI / 180);
            double moveSpeed = 5.0;

            double deltaX = Math.Cos(radians) * moveSpeed;
            double deltaY = Math.Sin(radians) * moveSpeed;

            double newX = Playr1.playerX + deltaX;
            double newY = Playr1.playerY + deltaY;

            // Проверяем, не выходит ли новая позиция за границы канвы
            if (newX >= 0 && newX <= MyCanva.ActualWidth - PlayerImage.ActualWidth &&
                newY >= 0 && newY <= MyCanva.ActualHeight - PlayerImage.ActualHeight)
            {
                Playr1.playerX = newX;
                Playr1.playerY = newY;

                Canvas.SetLeft(PlayerImage, Playr1.playerX);
                Canvas.SetTop(PlayerImage, Playr1.playerY);
                Canvas.SetLeft(PlayerImage_Head, Playr1.playerX);
                Canvas.SetTop(PlayerImage_Head, Playr1.playerY);
            }
        }

        private async Task MoveBackwardAsync()
        {
            double radians = Playr1.playerAngle * (Math.PI / 180);
            double moveSpeed = -5.0;

            double deltaX = Math.Cos(radians) * moveSpeed;
            double deltaY = Math.Sin(radians) * moveSpeed;

            double newX = Playr1.playerX + deltaX;
            double newY = Playr1.playerY + deltaY;

            // Проверяем, не выходит ли новая позиция за границы канвы
            if (newX >= 0 && newX <= MyCanva.ActualWidth - PlayerImage.ActualWidth &&
                newY >= 0 && newY <= MyCanva.ActualHeight - PlayerImage.ActualHeight)
            {
                Playr1.playerX = newX;
                Playr1.playerY = newY;

                Canvas.SetLeft(PlayerImage, Playr1.playerX);
                Canvas.SetTop(PlayerImage, Playr1.playerY);
                Canvas.SetLeft(PlayerImage_Head, Playr1.playerX);
                Canvas.SetTop(PlayerImage_Head, Playr1.playerY);
            }
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(MyCanva); // Получаем позицию мыши относительно PlayerCanvas

            double centerX = Playr1.playerX + PlayerImage.ActualWidth / 2;
            double centerY = Playr1.playerY + PlayerImage.ActualHeight / 2;

            double angleRadians = Math.Atan2(mousePosition.Y - centerY, mousePosition.X - centerX);
            double angleDegrees = angleRadians * (180 / Math.PI);

            Playr1.playerAngle_head = angleDegrees;

            RotateTransform rotateTransform = PlayerImage_Head.RenderTransform as RotateTransform;
            if (rotateTransform == null)
            {
                rotateTransform = new RotateTransform();
                PlayerImage_Head.RenderTransform = rotateTransform;
            }

            rotateTransform.Angle = Playr1.playerAngle_head;
        }
       

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isPaused = false;
            // Удаление врагов с холста и из коллекции
            foreach (Enemy enemy in enemies)
            {
                MyCanva.Children.Remove(enemy.Image); // Удаление из холста
            }
            enemies.Clear(); // Очистка коллекции
            Playr1.HP = 100;
            wave = 0;
            score = 0;
            kills = 0;
            Button1.Visibility = Visibility.Collapsed;
            GO_Copy1.Visibility = Visibility.Collapsed;
            GO_Copy2.Visibility = Visibility.Collapsed;
            GO_Copy.Visibility = Visibility.Collapsed;
            GO.Visibility = Visibility.Collapsed;
            enemySpawn_count = 1;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

            
            if (DateTime.Now - lastShotTime >= shotCooldown)
            {
                
                ShootAsync();
                lastShotTime = DateTime.Now; // Обновляем время последнего выстрела
            }
        }



        async Task PlayGameOver()
        {

            MediaPlayer player = new MediaPlayer();


            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            // Обработчик события MediaEnded, который будет вызван при завершении воспроизведения
            player.MediaEnded += (sender, e) =>
            {
                // Помечаем задачу как завершенную
                tcs.SetResult(true);
            };

            // Открываем звуковой файл
            player.Open(new Uri("C:\\Users\\Kille\\source\\repos\\Tanks\\Tanks\\Res\\go.wav", UriKind.Relative));

            // Воспроизводим звук
            player.Play();

            // Ожидаем завершения воспроизведения
            await tcs.Task;

            // Освобождаем ресурсы MediaPlayer
            player.Close();
            // break; // Эта строка не нужна в асинхронном методе

        }



        public static List<Enemy> enemies = new List<Enemy>();
 
        public static Dictionary<Enemy, EnemyMovement> enemyMovements = new Dictionary<Enemy, EnemyMovement>();
        int enemySpawn_count = 1;
        int wave = 0;
        
        private async Task InitializeEnemies() // Spawn enemy
        {
            if (isPaused)
            {
                await pauseSemaphore.WaitAsync();
            }
            Random random = new Random();
                 Random random_way = new Random();
            wave++;
            for (int i = 0; i < enemySpawn_count; i++) // : создание  врагов
            {
                
                int rw = random_way.Next(0, 3);
                await Task.Delay(100);
                Enemy enemy = null;
                if (rw == 0)
                {
                     enemy = new Enemy
                    {
                        X = random.Next(141, (int)MyCanva.ActualWidth - 141),
                        Y = 1, // Положение вверху за пределами канвы
                        Health = 100
                    };
                }

                if (rw == 1)
                {
                     enemy = new Enemy
                    {
                        X = MyCanva.ActualWidth - 141, // Положение справа за пределами канвы
                        Y = random.Next(141, (int)MyCanva.ActualHeight - 141),
                        Health = 100
                    };
                }

                if (rw == 2)
                {
                     enemy = new Enemy
                    {
                        X = 1, // Положение слева за пределами канвы
                        Y = random.Next(141, (int)MyCanva.ActualHeight - 141),
                        Health = 100
                    };
                }
                //if (rw == 3)
                //{
                //     enemy = new Enemy
                //    {
                //        X = random.Next(141, (int)MyCanva.ActualWidth - 141),
                //        Y = MyCanva.ActualHeight - 141, // Положение снизу за пределами канвы
                //        Health = 100
                //    };
                //}
            
                enemies.Add(enemy);

                Image enemyImage = new Image
                {
                    Width = 100,
                    Height = 100,
                };
                double speed = 1.3; // Начальная скорость врагов 
                double angle = random.Next(0, 360); // Начальный угол движения (случайный угол от 0 до 360 градусов)
                                                    // Текстура enemy
                Random random_tex = new Random();
                int txtr = 0;

                txtr = random_tex.Next(0, 101);
                BitmapImage enemyTexture = null;
                if (txtr >= 0 && txtr <= 68)
                {
                    enemyTexture = new BitmapImage(new Uri("/Res/enemy.png", UriKind.RelativeOrAbsolute));
                    speed = 3;
                }
                if (txtr >= 69 && txtr <= 94)
                {
                    enemyImage.Height = 140;
                    enemyImage.Width = 140;
                    enemyTexture = new BitmapImage(new Uri("/Res/enemy2.png", UriKind.RelativeOrAbsolute));
                    enemy.Health = 300;
                }
                enemy.Movement = new EnemyMovement(speed, angle);
                if (txtr >= 95 && txtr <= 100)
                {
                    enemyImage.Height = 140;
                    enemyImage.Width = 140;
                    enemyTexture = new BitmapImage(new Uri("/Res/enemy3.png", UriKind.RelativeOrAbsolute));
                    speed = 10;
                }
                enemy.Movement = new EnemyMovement(speed, angle);


                enemyImage.Source = enemyTexture;
                enemyImage.RenderTransformOrigin = new Point(0.5, 0.5);
                Canvas.SetLeft(enemyImage, enemy.X);
                Canvas.SetTop(enemyImage, enemy.Y);

                enemyImages.Add(enemy, enemyImage);

                MyCanva.Children.Add(enemyImage);
                enemy.Image = enemyImage;


            }
            enemySpawn_count++;
        }

        private void CheckAndSpawnEnemies()
        {
            if (enemies.Count == 0)
            {
                InitializeEnemies();
            }
            
        }
        private void EnemyMovementTimer_Tick(object sender, EventArgs e)
        {

        foreach (Enemy enemy in enemies)
           {
                if (!isPaused)
                {
                    // Вычисляем новые координаты на основе угла движения в радианах
                    double angleRadians = enemy.Movement.Angle * (Math.PI / 180); // Преобразуем угол в радианы
                    double deltaX = Math.Cos(angleRadians) * enemy.Movement.Speed;
                    double deltaY = Math.Sin(angleRadians) * enemy.Movement.Speed;

                    double newX = enemy.X + deltaX;
                    double newY = enemy.Y + deltaY;

                    // Проверяем, не выходит ли новая позиция за границы канвы
                    if (newX >= 0 && newX <= MyCanva.ActualWidth - enemy.Image.ActualWidth &&
                        newY >= 0 && newY <= MyCanva.ActualHeight - enemy.Image.ActualHeight)
                    {
                        enemy.X = newX;
                        enemy.Y = newY;

                        Canvas.SetLeft(enemy.Image, enemy.X);
                        Canvas.SetTop(enemy.Image, enemy.Y);

                        // Вращаем изображение врага в зависимости от угла движения
                        enemy.Image.RenderTransform = new RotateTransform(enemy.Movement.Angle);
                    }
                    else
                    {
                        // Враг достиг края карты, изменяем угол движения на случайный
                        Random random = new Random();
                        enemy.Movement.Angle = random.Next(0, 360);
                    }

                }

      
    }

}

       
    }
}
