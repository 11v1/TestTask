﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TestTask
{
    public class Program
    {
        /// <summary>
        /// Программа принимает на входе 2 пути до файлов.
        /// Анализирует в первом файле кол-во вхождений каждой буквы (регистрозависимо). Например А, б, Б, Г и т.д.
        /// Анализирует во втором файле кол-во вхождений парных букв (не регистрозависимо). Например АА, Оо, еЕ, тт и т.д.
        /// По окончанию работы - выводит данную статистику на экран.
        /// </summary>
        /// <param name="args">Первый параметр - путь до первого файла.
        /// Второй параметр - путь до второго файла.</param>
        static void Main(string[] args)
        {
            IReadOnlyStream inputStream1 = GetInputStream(args[0]);
            IReadOnlyStream inputStream2 = GetInputStream(args[1]);

            IList<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
            IList<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);

            RemoveCharStatsByType(singleLetterStats, CharType.Vowel);
            RemoveCharStatsByType(doubleLetterStats, CharType.Consonants);

            PrintStatistic(singleLetterStats);
            PrintStatistic(doubleLetterStats);

            // TODO : Необходимо дождаться нажатия клавиши, прежде чем завершать выполнение программы.
            Console.WriteLine("Press any key for exit...");
            ManualResetEvent quitEvent = new ManualResetEvent(false);
            new Task(() =>
            {
                Console.ReadKey();
                quitEvent.Set();
            }).Start();
            quitEvent.WaitOne();
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static IReadOnlyStream GetInputStream(string fileFullPath)
        {
            return new ReadOnlyStream(fileFullPath);
        }

        /// <summary>
        /// Определяет является ли указанный симол английской буквой
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool IsLetter(char c)
        {
            return Regex.IsMatch(c.ToString(), "[A-Za-z]");
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения каждой буквы.
        /// Статистика РЕГИСТРОЗАВИСИМАЯ!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillSingleLetterStats(IReadOnlyStream stream)
        {
            stream.ResetPositionToStart();
            Dictionary<char, LetterStats> letterDict = new Dictionary<char, LetterStats>();
            while (!stream.IsEof)
            {
                char c = stream.ReadNextChar();
                if (!IsLetter(c))
                    continue;
                // TODO : заполнять статистику с использованием метода IncStatistic. Учёт букв - регистрозависимый.
                if (letterDict.TryGetValue(c, out var letter))
                    IncStatistic(letter);
                else
                    letterDict.Add(c, new LetterStats()
                    {
                        Letter = c.ToString(),
                        Count = 1
                    });
            }

            return new List<LetterStats>(letterDict.Values);
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения парных букв.
        /// В статистику должны попадать только пары из одинаковых букв, например АА, СС, УУ, ЕЕ и т.д.
        /// Статистика - НЕ регистрозависимая!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillDoubleLetterStats(IReadOnlyStream stream)
        {
            stream.ResetPositionToStart();
            Dictionary<char, LetterStats> letterDict = new Dictionary<char, LetterStats>();
            char? cPrev = null;
            while (!stream.IsEof)
            {
                char c = char.ToUpper(stream.ReadNextChar());
                if (!IsLetter(c))
                {
                    cPrev = null;
                    continue;
                }
                // TODO : заполнять статистику с использованием метода IncStatistic. Учёт букв - НЕ регистрозависимый.
                if (c == cPrev)
                {
                    if (letterDict.TryGetValue(c, out var letter))
                        IncStatistic(letter);
                    else
                        letterDict.Add(c, new LetterStats()
                        {
                            Letter = $"{c}{c}",
                            Count = 1
                        });
                    cPrev = null;
                }
                else
                {
                    cPrev = c;
                }
            }

            return new List<LetterStats>(letterDict.Values);
        }

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные или согласные буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        private static void RemoveCharStatsByType(IList<LetterStats> letters, CharType charType)
        {
            // TODO : Удалить статистику по запрошенному типу букв.
            IEnumerable<LetterStats> lettersRemove = null;
            switch (charType)
            {
                case CharType.Consonants:
                    lettersRemove = letters.Where(ls => Regex.IsMatch(ls.Letter, "[BCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz]"));
                    break;
                case CharType.Vowel:
                    lettersRemove = letters.Where(ls => Regex.IsMatch(ls.Letter, "[AEIOUaeiou]"));
                    break;
            }
            if (lettersRemove != null)
            {
                lettersRemove = lettersRemove.ToList();
                foreach (var r in lettersRemove)
                    letters.Remove(r);
            }
        }

        /// <summary>
        /// Ф-ция выводит на экран полученную статистику в формате "{Буква} : {Кол-во}"
        /// Каждая буква - с новой строки.
        /// Выводить на экран необходимо предварительно отсортировав набор по алфавиту.
        /// В конце отдельная строчка с ИТОГО, содержащая в себе общее кол-во найденных букв/пар
        /// </summary>
        /// <param name="letters">Коллекция со статистикой</param>
        private static void PrintStatistic(IEnumerable<LetterStats> letters)
        {
            // TODO : Выводить на экран статистику. Выводить предварительно отсортировав по алфавиту!
            foreach(var ls in letters)
                Console.WriteLine($"{ls.Letter} : {ls.Count}");
        }

        /// <summary>
        /// Метод увеличивает счётчик вхождений по переданной структуре.
        /// </summary>
        /// <param name="letterStats"></param>
        private static void IncStatistic(LetterStats letterStats)
        {
            letterStats.Count++;
        }
    }
}
