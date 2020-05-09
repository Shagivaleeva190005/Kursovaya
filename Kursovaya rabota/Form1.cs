using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.IO;

namespace Kursovaya_rabota
{
    public partial class Form1 : Form
    {
        // для независимости от десятичного разделителя
        static private CultureInfo _fileCulture = CultureInfo.GetCultureInfo("ru-RU");
        static private NumberStyles _numberStyles = NumberStyles.Float | NumberStyles.AllowThousands;
        
        static private bool TryParseDouble(string str, out double val)
        {
            return Double.TryParse(str.Replace('.', ','), _numberStyles, _fileCulture, out val);
        }

        Graphics animate; // для анимации

        const double g = 9.816; // ускорение свободного падения

        static double m; // масса маятника
        static double l; // длина подвеса
        static double fi_0; // начальный угол

        static double T; // продолжительность моделирования
        static double dt; // период дискретизации
        static double dT; // период вывода в отчет

        static double E_0; // начальная энергия

        List<Moment> moments = new List<Moment>(); // лист выводимых моментов

        // класс момента времени, содержащий все параметры системы
        class Moment
        {
            // текущие время, угол, угловая скорость, угловое ускорение,
            // абсцисса и ординаты, проекции скорости на оси, скорость,
            // энергия, отклонение энергии от начальной
            public double t, fi, omega, eps, x, y, Vx, Vy, V, E, dE;

            // задание параметров и создание первого момента из них 
            public static Moment FirstMoment(Form1 form)
            {
                // пытаемся распарсить все параметры
                if (!(TryParseDouble(form.textBoxm.Text, out m) &
                    TryParseDouble(form.textBoxl.Text, out l) &
                    TryParseDouble(form.textBoxfi_0.Text, out fi_0) &
                    TryParseDouble(form.textBoxT.Text, out T) &
                    TryParseDouble(form.textBoxdt.Text, out dt) &
                    TryParseDouble(form.textBoxdTr.Text, out dT)))
                {
                    // если неудается распарсить, вывести сообщение об ошибке и вернуть null
                    MessageBox.Show("Введенные данные некорректны!");
                    return null;
                }

                // переходим от градусов к радианам
                fi_0 *= Math.PI / 180;

                // проверяем введенные параметры на адекватность
                if (!(m > 0 & l > 0 & T > 0 & dt > 0 & dT > 0 & T > dT & dT > dt))
                {
                    // если неадекватные, вывести сообщение об ошибке и вернуть null
                    MessageBox.Show("Введенные данные некорректны!");
                    return null;
                }

                // возвращаемый начальный момент
                Moment firstMoment = new Moment();

                // время начала моделирования = 0
                firstMoment.t = 0;

                // начальные угол, ускорение и скорость
                firstMoment.fi = fi_0;
                firstMoment.eps = -g * Math.Sin(fi_0) / l;
                firstMoment.omega = 0;

                // начальные координаты
                firstMoment.x = Math.Sin(firstMoment.fi) * l; 
                firstMoment.y = (1 - Math.Cos(firstMoment.fi)) * l;

                // начальная скорость == 0
                firstMoment.V = 0;
                firstMoment.Vx = 0;
                firstMoment.Vy = 0;

                // начальная энергия
                firstMoment.E = m * l * l * firstMoment.omega * firstMoment.omega / 2 +
                    m * g * (1 - Math.Cos(firstMoment.fi)) * l;
                firstMoment.dE = 0;

                // сохраняем начальную энергию в глобальную переменную
                E_0 = firstMoment.E;

                // возвращаем созданный начальный момент
                return firstMoment;
            }

            // рассчет следующего момента по предыдущему
            public static Moment Calculate(Moment prev)
            {
                // возвращаемый момент
                Moment newMoment = new Moment();

                // увеличиваем время на период диср=кретизации
                newMoment.t = prev.t + dt;

                // рассчитываем угловые характеристики
                newMoment.fi = prev.fi + prev.omega * dt;
                newMoment.eps = -g * Math.Sin(newMoment.fi) / l;
                newMoment.omega = prev.omega + newMoment.eps * dt;

                // рассчитываем координаты
                newMoment.x = Math.Sin(newMoment.fi) * l;
                newMoment.y = (1 - Math.Cos(newMoment.fi)) * l;

                // рассчитываем скорость
                newMoment.V = newMoment.omega * l;
                newMoment.Vx = newMoment.V * Math.Cos(newMoment.fi);
                newMoment.Vy = newMoment.V * Math.Sin(newMoment.fi);

                // рассчитываем энергию и отклонение от начальной энергии
                newMoment.E = m * l * l * newMoment.omega * newMoment.omega / 2 +
                    m * g * (1 - Math.Cos(newMoment.fi)) * l;
                newMoment.dE = Math.Abs(newMoment.E - E_0);

                // возвращаем следующий момент
                return newMoment;
            }
        }

        // метод вывода момента в элементы интерфейса 
        void OutputToInterface(Moment moment)
        {
            // вывод в таблицу
            dataGridView.Rows.Add(
                Math.Round(moment.t, 4),
                Math.Round(moment.fi * 180 / Math.PI, 4),
                Math.Round(moment.omega * 180 / Math.PI, 4),
                Math.Round(moment.eps * 180 / Math.PI, 4),
                Math.Round(moment.x, 4),
                Math.Round(moment.y, 4),
                Math.Round(moment.Vx, 4),
                Math.Round(moment.Vy, 4),
                Math.Round(moment.V, 4),
                Math.Round(moment.E, 4),
                Math.Round(moment.dE, 4));

            // вывод в графики
            chartAngle.Series[0].Points.AddXY(moment.t, moment.fi * 180 / Math.PI); // угол

            chartAngSpeed.Series[0].Points.AddXY(moment.t, moment.omega * 180 / Math.PI); // угловая скорость

            chartAcceleration.Series[0].Points.AddXY(moment.t, moment.eps * 180 / Math.PI); // угловое ускорение

            chartCoordinates.Series[0].Points.AddXY(moment.t, moment.x); // абсцисса
            chartCoordinates.Series[1].Points.AddXY(moment.t, moment.y); // ордината

            chartSpeed.Series[0].Points.AddXY(moment.t, moment.Vx); // проекция скорости на ось абсцисс
            chartSpeed.Series[1].Points.AddXY(moment.t, moment.Vy); // проекция скорости на ось ординат
            chartSpeed.Series[2].Points.AddXY(moment.t, moment.V); // скорость

            chartEnergy.Series[0].Points.AddXY(moment.t, moment.E); // энергия

            chartdEnergy.Series[0].Points.AddXY(moment.t, moment.dE); // отклонение энергии от начальной

            moments.Add(moment); // сохранение в лист
        }


        // функция инициализации формы
        public Form1()
        {
            InitializeComponent();

            pictureBoxAnimate.Image = new Bitmap(pictureBoxAnimate.Width, pictureBoxAnimate.Height); // создание изобржения и помещение его в пикчербокс
            animate = Graphics.FromImage(pictureBoxAnimate.Image);//создание графики из изображения
        }

        // по клику на кнопку рассчета
        private void button_rasschitat_Click(object sender, EventArgs e)
        {
            // очистить все
            dataGridView.Rows.Clear();
            chartAngle.Series[0].Points.Clear();
            chartAngSpeed.Series[0].Points.Clear();
            chartAcceleration.Series[0].Points.Clear();
            chartCoordinates.Series[0].Points.Clear();
            chartCoordinates.Series[1].Points.Clear();
            chartSpeed.Series[0].Points.Clear();
            chartSpeed.Series[1].Points.Clear();
            chartSpeed.Series[2].Points.Clear();
            chartEnergy.Series[0].Points.Clear();
            chartdEnergy.Series[0].Points.Clear();

            // считать параметры
            Moment temp = Moment.FirstMoment(this);
            
            // если была обнаружена ошибка во введенных параметрах, закончить выполнение
            if (temp == null)
            {
                return;
            }
            
            // вывести начальные параметры в интерфейс 
            OutputToInterface(temp);

            // рассчитать N и M
            int N = (int)(T / dT);
            int M = (int)(dT / dt);

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    // рассчитать значения в этот момент времени  
                    temp = Moment.Calculate(temp);
                }

                // вывод в элементы интерфейса
                OutputToInterface(temp);
            }

            // сообщение о завершении
            MessageBox.Show("Рассчет завершен!");

            timerAnimate.Enabled = true;
            timerAnimate.Interval = (int)(dT * 10000) + 1;
        }

        private void button_sohranit_Click(object sender, EventArgs e)
        {
            // создание папки для отчета
            Directory.CreateDirectory("Отчет");

            // сохранение графиков
            chartAngle.SaveImage("Отчет\\угол.jpeg",
                    System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Jpeg);
            chartAngSpeed.SaveImage("Отчет\\угловая скорость.jpeg",
                    System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Jpeg);
            chartAcceleration.SaveImage("Отчет\\ускорение.jpeg",
                    System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Jpeg);
            chartCoordinates.SaveImage("Отчет\\координаты.jpeg",
                    System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Jpeg);
            chartSpeed.SaveImage("Отчет\\скорость.jpeg",
                    System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Jpeg);
            chartEnergy.SaveImage("Отчет\\энергия.jpeg",
                    System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Jpeg);
            chartdEnergy.SaveImage("Отчет\\отклонение энергии от начальной.jpeg",
                    System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Jpeg);

            //создание файла с результатами
            StreamWriter sw = new StreamWriter("Отчет\\результаты моделирования.csv", false, Encoding.UTF8);

            // запись заголовков
            sw.WriteLine("t, с; φ, °; ω, °/с; ε, °/с²; x, м; y, м; Vx, м/с; Vy,м/с; V, м/с; E, Дж; dE, Дж");

            // запись всех моментов
            foreach (var i in moments)
            {
                sw.WriteLine(Math.Round(i.t, 4) + ";" +
                Math.Round(i.fi * 180 / Math.PI, 4) + ";" +
                Math.Round(i.omega * 180 / Math.PI, 4) + ";" +
                Math.Round(i.eps * 180 / Math.PI, 4) + ";" +
                Math.Round(i.x, 4) + ";" +
                Math.Round(i.y, 4) + ";" +
                Math.Round(i.Vx, 4) + ";" +
                Math.Round(i.Vy, 4) + ";" +
                Math.Round(i.V, 4) + ";" +
                Math.Round(i.E, 4) + ";" +
                Math.Round(i.dE, 4));
            }

            // закрытие файла
            sw.Close();


            // создание файла с отчетом
            sw = new StreamWriter("Отчет\\отчет.txt");

            // запись заголовков
            sw.WriteLine("Моделирование колебаний маятника\n\n" +
                "Физические параметры\n" + 
                "Масса груза: " + m + " кг\n" +
                "Длина маятника: " + l + " м\n" +
                "Начальный угол: " + (fi_0 * 180 / Math.PI) + "°\n\n" +
                "Параметры моделирования\n" +
                "Продолжительность моделирования: " + T + " с\n" +
                "Период дискретизации: " + dt + " с\n" +
                "Период вывода в отчет: " + dT + " с\n\n" +
                "Результаты моделирования\n\n" +
                "t, с; φ, °; ω, °/с; ε, °/с²; x, м; y, м; Vx, м/с; Vy,м/с; V, м/с; E, Дж; dE, Дж");

            // запись всех моментов
            foreach (var i in moments)
            {
                sw.WriteLine(Math.Round(i.t, 4) + "\t" +
                Math.Round(i.fi * 180 / Math.PI, 4) + "\t" +
                Math.Round(i.omega * 180 / Math.PI, 4) + "\t" +
                Math.Round(i.eps * 180 / Math.PI, 4) + "\t" +
                Math.Round(i.x, 4) + "\t" +
                Math.Round(i.y, 4) + "\t" +
                Math.Round(i.Vx, 4) + "\t" +
                Math.Round(i.Vy, 4) + "\t" +
                Math.Round(i.V, 4) + "\t" +
                Math.Round(i.E, 4) + "\t" +
                Math.Round(i.dE, 4));
            }
            
            // закрытие файла
            sw.Close();

            // сообщение о завершении
            MessageBox.Show("Отчет сохранен!");
        }

        // анимирование
        int i = 1; // номер момента
        private void timerAnimate_Tick(object sender, EventArgs e)
        {
            // если достигли конца листа, начать сначала
            if (i >= moments.Count)
            {
                i = 1;
            }

            // очистиить рисунок
            animate.Clear(Color.White);

            // коэффициент масштабирования
            double k = pictureBoxAnimate.Height / l / 2;

            // отрисовка маятника
            animate.DrawLine(new Pen(Color.Black, 2),
                (float)(pictureBoxAnimate.Width / 2), (float)(k * l), 
                (float)(pictureBoxAnimate.Width / 2 + k * moments[i].x), (float)(k * (2*l - moments[i].y)));

            // отрисовка массы
            animate.FillEllipse(Brushes.Red, 
                (float)(pictureBoxAnimate.Width / 2 + k * moments[i].x) - 4, (float)(k * (2 * l - moments[i].y) - 4),
                8, 8);

            //перерисовываем
            pictureBoxAnimate.Invalidate(); 

            // переход к следующему моменту
            i++;
        }
    }
}
