using System;

namespace ParallerParserExample
{
    using System.Collections.Generic;
    using System.Threading;

    class Program
    {
        private static Stack<int> _stackExample = new Stack<int>();

        private static int _currentThreadCount = 0; //Текущее количество работающих потоков.
        private static int _maxThreadValue = 50; //Максимальное количество потоков.
        static void Main(string[] args)
        {
            //Заполняем Stack случайными данными.
            var random = new Random();
            for (int i = 0; i < 1000; i++)
            {
                _stackExample.Push(i);
            }

            //Перебираем все элементы Stack'a.
            while (_stackExample.Count>0)
            {
                //Выставляем задержку, чтобы потоки не создавались сверх указанного нами количества.
                while (_currentThreadCount >= _maxThreadValue)
                {
                    Thread.Sleep(100);
                }

                int tempValue = _stackExample.Pop(); // Обязательно переносим элемент Stack'a в локальную переменную, чтобы поток "успел" её подхвать до начала следующего цикла.
                _currentThreadCount++; //Увеличиваем счётчик текущих потоков.
                new Thread(
                    () =>
                        {
                            ExampleMethod(tempValue); // Вызываем нужный нам метод.
                            _currentThreadCount--; // После окончания работы нужного нам метода не забываем уменьшить счётчик текущих потоков.
                        }).Start();
            }
            //Запускаем ожидание окончания выполнения всех потоков
            while (_currentThreadCount!=0)
            {
                Thread.Sleep(100);
            }

            Console.WriteLine("Работа всех потоков успешно завершена!");
            Console.ReadLine();
        }

        //Тестовый метод. При реализации - заменить на свой
        private static void ExampleMethod(int value)
        {
            Console.WriteLine($"Обработано значение: {value}");
        }
    }
}
