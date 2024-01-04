using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Tanks;
using WpfAnimatedGif;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Animation;

namespace Tanks
{
    internal class Bullet
    {
        public Image Image { get; private set; }
        private double angleRadians;
        private double speed;
        double bulletX;
        double bulletY;
        private readonly Dictionary<Enemy, Image> enemyImages;

        public Bullet(double startX, double startY, double angleDegrees, double speed, Dictionary<Enemy, Image> enemyImages)
        {
            this.angleRadians = angleDegrees * (Math.PI / 180);
            this.speed = speed;
            this.enemyImages = enemyImages;
            Image = new Image
            {
                Width = 25,
                Height = 15,
                RenderTransform = new RotateTransform(angleDegrees)
            };

            BitmapImage imageSource = new BitmapImage(new Uri("/Res/bullet_X.png", UriKind.RelativeOrAbsolute));
            Image.Source = imageSource;

            bulletX = startX - Image.Width / 2;
            bulletY = startY - Image.Height / 2;

            Canvas.SetLeft(Image, bulletX);
            Canvas.SetTop(Image, bulletY);
            this.enemyImages = enemyImages;
        }
        async Task PlayExpl() 
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
            player.Open(new Uri("C:\\Users\\Kille\\source\\repos\\Tanks\\Tanks\\Res\\expl.wav", UriKind.Relative));

            // Воспроизводим звук
            player.Play();

            // Ожидаем завершения воспроизведения
            await tcs.Task;

            // Освобождаем ресурсы MediaPlayer
            player.Close();
            // break; // Эта строка не нужна в асинхронном методе
            
        }
        async Task PlayNoExp()
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
            player.Open(new Uri("C:\\Users\\Kille\\source\\repos\\Tanks\\Tanks\\Res\\rik.wav", UriKind.Relative));

            // Воспроизводим звук
            player.Play();

            // Ожидаем завершения воспроизведения
            await tcs.Task;

            // Освобождаем ресурсы MediaPlayer
            player.Close();
            // break; // Эта строка не нужна в асинхронном методе

        }

        public async Task MoveAsync(Canvas canvas)
        {
            bulletX = Canvas.GetLeft(Image);//!!!!!double bulletX из-за переопределения не менялись значения полей класса
            bulletY = Canvas.GetTop(Image);//!!!!double bulletY

            while (bulletX >= 0 && bulletX <= canvas.ActualWidth && bulletY >= 0 && bulletY <= canvas.ActualHeight)
            {
                await Task.Delay(16);
                bulletX += Math.Cos(angleRadians) * speed;
                bulletY += Math.Sin(angleRadians) * speed;

                Canvas.SetLeft(Image, bulletX);
                Canvas.SetTop(Image, bulletY);
                if (CheckCollisions(canvas))
                {
                    PlayExpl();
                    break;  //выйти из цикла при столкновении
                    

                }

            }

            // !!!!!!!!!!!!Удаляем пулю из Canvas после вылета за его пределы или при столкновении
            canvas.Children.Remove(Image);
        }

         private bool CheckCollisions(Canvas canvas)
        {
            // Получаем текущие координаты пули из полей класса Bullet
            double bulletX = this.bulletX;
            double bulletY = this.bulletY;

          


            // Проверяем столкновения для каждого врага
            foreach (Enemy enemy in MainWindow.enemies.ToList())
            {
                double enemyX = Canvas.GetLeft(enemyImages[enemy]);
                double enemyY = Canvas.GetTop(enemyImages[enemy]);

                if (bulletX >= enemyX &&
                    bulletX <= enemyX + enemyImages[enemy].Width &&
                    bulletY >= enemyY &&
                    bulletY <= enemyY + enemyImages[enemy].Height)
                {
                    enemy.Health -= 100;
                    if(enemy.Health <= 0)
                    {
                        // Удаляем врага из canvas и из словаря
                        canvas.Children.Remove(enemyImages[enemy]);
                        enemyImages.Remove(enemy);

                        // Удаляем врага из списка врагов
                        MainWindow.enemies.Remove(enemy);



                        SpawnAndRemoveImageAsync(canvas, 1, enemyX, enemyY);
                        MainWindow.score = MainWindow.score + 100;
                        MainWindow.kills = MainWindow.kills + 1;
                        return true;
                    }
                    PlayNoExp();
                    SpawnAndRemoveImageAsync2(canvas, 1, enemyX, enemyY);

                    return true;

                }
            }

            return false;
        }
        private async Task SpawnAndRemoveImageAsync(Canvas canvas, double durationInSeconds, double enemyX, double enemyY)
        {
            Image gifImage = new Image();

            // Загрузка анимации GIF с использованием WpfAnimatedGif
            BitmapImage gifBitmap = new BitmapImage(new Uri("C:\\Users\\Kille\\source\\repos\\Tanks\\Tanks\\Res\\boom.gif"));
            ImageBehavior.SetAnimatedSource(gifImage, gifBitmap);
            
            gifImage.Width = 100;
            gifImage.Height = 100;

            Canvas.SetLeft(gifImage, enemyX);
            Canvas.SetTop(gifImage, enemyY);

            canvas.Children.Add(gifImage);

            // Ожидание заданного количества секунд
            await Task.Delay(TimeSpan.FromSeconds(durationInSeconds));

            // Удаление изображения
            canvas.Children.Remove(gifImage);
        }

        private async Task SpawnAndRemoveImageAsync2(Canvas canvas, double durationInSeconds, double enemyX, double enemyY)
        {
            Image gifImage = new Image();

            // Загрузка анимации GIF с использованием WpfAnimatedGif
            BitmapImage gifBitmap = new BitmapImage(new Uri("C:\\Users\\Kille\\source\\repos\\Tanks\\Tanks\\Res\\bam.gif"));
            ImageBehavior.SetAnimatedSource(gifImage, gifBitmap);

            gifImage.Width = 100;
            gifImage.Height = 100;

            Canvas.SetLeft(gifImage, enemyX);
            Canvas.SetTop(gifImage, enemyY);

            canvas.Children.Add(gifImage);

            // Ожидание заданного количества секунд
            await Task.Delay(TimeSpan.FromSeconds(durationInSeconds));

            // Удаление изображения
            canvas.Children.Remove(gifImage);
        }

    }
}

