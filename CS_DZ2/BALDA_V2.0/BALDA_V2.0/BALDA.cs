using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BALDA_V2._0.BALDA;
using System.Data;
using System.IO;

namespace BALDA_V2._0
{
    internal class BALDA
    {
        public class Player
        {
            public ListBox playerWordBox { get; set; }
            public Label playerScoreBox { get; set; }
            public Label playerTurnBox { get; set; }
        }

        public class TableButton : Button
        {
            public Tuple<int, int> coords;
            public bool placed = false;
        }

        public class LetterButton : Button
        {
        }

        public class ReadyButton : Button
        {
        }

        public class SkipButton : Button
        {
        }

        public class ClearButton : Button
        {
        }

        public BALDA(List<TableButton> tableButtons, 
            List<LetterButton> letterButtons, 
            ReadyButton readyButton, 
            ClearButton clearButton, 
            SkipButton skipButton,
            ListBox player1WordBox,
            ListBox player2WordBox,
            Label player1ScoreBox,
            Label player2ScoreBox,
            Label player1TurnBox,
            Label player2TurnBox,
            Timer gameTimer,
            Label gameTimerBox,
            Form form)
        {
            this.tableButtons = tableButtons;
            this.letterButtons = letterButtons;
            this.readyButton = readyButton;
            this.clearButton = clearButton;
            this.skipButton = skipButton;
            this.player1WordBox = player1WordBox;
            this.player2WordBox = player2WordBox;
            this.player1ScoreBox = player1ScoreBox;
            this.player2ScoreBox = player2ScoreBox;
            this.player1TurnBox = player1TurnBox;
            this.player2TurnBox = player2TurnBox;
            this.gameTimer = gameTimer;
            this.gameTimerBox = gameTimerBox;
            this.form = form;

            ButtonsInit();

            player1 = new Player();
            player2 = new Player();

            player1.playerWordBox = player1WordBox;
            player1.playerScoreBox = player1ScoreBox;
            player1.playerTurnBox = player1TurnBox;

            player2.playerWordBox = player2WordBox;
            player2.playerScoreBox = player2ScoreBox;
            player2.playerTurnBox = player2TurnBox;

            try
            {
                string[] tempDictionary = File.ReadAllLines(filePath);
                dictionary = tempDictionary.Select(word => word.ToUpper()).ToList();
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"Ошибка: Файл не найден. {ex.Message}");
                Environment.Exit(1);
            }

            // Создаем слово в центре
            GenerateStartWord();
        }
        // Родительская форма
        private Form form;

        // Все кнопки центрального поля
        private List<TableButton> tableButtons;

        // Все кнопки с буквами из панели
        List<LetterButton> letterButtons;

        // Кнопка "ОТПРАВИТЬ"
        private ReadyButton readyButton;

        // Кнопка "СТЕРЕТЬ"
        private ClearButton clearButton;

        // Кнопка "ПРОПУСТИТЬ"
        private SkipButton skipButton;

        // Боксы со словами, введеными игроками
        private ListBox player1WordBox;
        private ListBox player2WordBox;

        // Лэйблы с баллами игроков
        private Label player1ScoreBox;
        private Label player2ScoreBox;

        // Лэйблы с индикатором хода игроков
        private Label player1TurnBox;
        private Label player2TurnBox;

        // Таймер и его лэйбл
        private Timer gameTimer;
        private Label gameTimerBox;
        private static int timerSecondsConst = 120;
        private int timerSeconds = timerSecondsConst;

        // Игроки
        Player player1;
        Player player2;
        private bool playerTurn = false;

        // При пропуске хода по таймеру или skip любым игроком получает ++, если он достигает значения 1 -> игра заканчивается
        private int winChecker = 0;

        // Маркер того, что игроку неоюходимо выбрать первую букву для формирования слова
        private bool blockFirstLetter;

        // Маркер того, что игрок заврешил ход (Используется для того чтобы понять, игрок сам завершил ход или нет)
        bool endTurn = false;

        // Маркер того, что игрок разместил букву на поле
        private bool letterPlaced = false;

        // Буква, которую выбрал игрок
        private string letter = null;

        // Кнопка, в которой находится буква, размещенная игроком
        private TableButton placedButton = null;

        // Кнопка, которую игрок выбирает в процессе составления слова (остальные блокируются относительно неё)
        private TableButton selectedButton = null;

        // Сохраняем первую букву, чтобы в случаее ее удаления из слова, происходил ClearTurn без передачи хода
        private TableButton firstLetter = null;

        // Все кнопки, которые возможны для выбора из selectedButton
        List<TableButton> possibleButtons = new List<TableButton>();

        // Кнопки, которые выбрались при составлении слова
        List<TableButton> wordButtons = new List<TableButton>();

        // Список, содержащий слово, выбираемое игроком
        List<char> word = new List<char>();

        // Начальное слово
        string startWord;

        // Словарь
        string filePath = "Словарь.txt";
        List<string> dictionary;

        private void ButtonsInit()
        {
            foreach (Button button in tableButtons)
            {
                button.Click += OnTableButtonClick;
            }

            foreach (Button button in letterButtons)
            {
                button.Click += OnLetterButtonClick;
            }

            clearButton.Click += OnClearButtonClick;

            skipButton.Click += OnSkipButtonClick;

            readyButton.Click += OnReadyButtonClick;

            gameTimer.Tick += GameTimerTick;
        }


        ///////////////////////////////////////////////////// ТАЙМЕР

        private void GameTimerTick(object sender, EventArgs e)
        {
            // Каждый раз, когда срабатывает таймер
            timerSeconds--;

            // Обновление отображения времени 
            UpdateTimerDisplay();

            // Проверка, достигло ли время нуля
            if (timerSeconds <= 0)
            {
                ClearTurn();
                LeftTurn();
                timerSeconds = timerSecondsConst;
                winChecker++;
            }
        }

        private void UpdateTimerDisplay()
        {
            gameTimerBox.Text = $"{timerSeconds / 60:D2}:{timerSeconds % 60:D2}";
        }

        ///////////////////////////////////////////////////// СОБЫТИЯ С КНОПКАМИ
        private void OnClearButtonClick(object sender, EventArgs e)
        {
            if (letterPlaced != false)
            {
                ClearTurn();
            }
        }

        private void OnTableButtonClick(object sender, EventArgs e)
        {
            // Игрок нажал на кнопку и еще не поставил букву
            if (!letterPlaced && letter != null)
            {
                if (sender is TableButton tableButton)
                {
                    // Кнопка не содержала текста
                    if (tableButton.Text == "")
                    {
                        // Размещаем текст
                        tableButton.Text = letter;

                        tableButton.placed = true;

                        placedButton = tableButton;

                        // Маркер того, что теперь игроку необходимо указать слово
                        letterPlaced = true;

                        // Игрок должен выбрать букву, с которой начнет формироваться слово (Блочатся все пустые клетки)
                        BlockEmptyButtons();

                        // Маркер того, что игроку неоюходимо выбрать первую букву для формирования слова
                        blockFirstLetter = true;

                        BlockLettersButtons();
                    }
                }
            }

            // Игрок выбирает ПЕРВУЮ букву слова
            else if (blockFirstLetter == true)
            {
                blockFirstLetter = false;

                if (sender is TableButton tableButton)
                {
                    // Добавляем первую букву в слово
                    word.Add(tableButton.Text.ToCharArray()[0]);
                    wordButtons.Add(tableButton);
                    tableButton.BackColor = Color.Red;

                    selectedButton = tableButton;

                    firstLetter = tableButton;

                    // Разблочиваем все кнопки
                    EnableWordButtons();

                    // Блочим кнопки относительно selectedButton
                    BlockWordButtons();
                }
            }


            else if (letter == null)
            {
            }

            // Игрок вибирает слово
            else
            {
                if (sender is TableButton tableButton)
                {

                    // Если игрок нажал не на красную кнопку
                    if (tableButton != selectedButton)
                    {
                        tableButton.BackColor = Color.Red;

                        word.Add(tableButton.Text.ToCharArray()[0]);

                        wordButtons.Add(tableButton);

                        selectedButton = tableButton;

                        // Разблочиваем кнопки, чтобы сделать перерасчет
                        EnableWordButtons();

                        // Передаем кнопку, которую выбрал игрок для формирования слова, и отсавляем только доступные из нее кнопки
                        BlockWordButtons();
                    }

                    // Если игрок нажал на красную кнопку
                    else
                    {
                        // Если это была первая буква, которую выбрал игрок
                        if (tableButton == firstLetter)
                        {
                            ClearTurn();
                        }

                        else
                        {
                            tableButton.BackColor = Color.White;

                            word.Remove(tableButton.Text.ToCharArray()[0]);

                            wordButtons.Remove(tableButton);

                            selectedButton = wordButtons.Last();

                            // Разблочиваем кнопки, чтобы сделать перерасчет
                            EnableWordButtons();

                            // Передаем кнопку, которую выбрал игрок для формирования слова, и отсавляем только доступные из нее кнопки
                            BlockWordButtons();
                        }
                    }
                }
            }
        }

        private void OnSkipButtonClick(object sender, EventArgs e)
        {
            LeftTurn();
            ClearTurn();
            winChecker++;
        }

        private void OnReadyButtonClick(object sender, EventArgs e)
        {
            if (sender is ReadyButton readyButton)
            {
                // Игрок не может завершить зод, не поставив букву
                if (placedButton != null)
                {
                    // Буква поставленная игроком должна использоваться в слове
                    if (LetterCheck() == true)
                    {
                        // Слово не должно использоваться ранее
                        if (WordBoxCheck() == true)
                        {
                            // Пытаемся найти слово в словаре
                            string foundWord = dictionary.Find(item => item == (new string(word.ToArray())));

                            // Если такое слово нашлось
                            if (foundWord != null)
                            {
                                // Получить текущего игрока
                                Player CurrentPlayer = GetCurrentPlayer();

                                // Добавляем слово в Box соответствующего игрока
                                CurrentPlayer.playerWordBox.Items.Add(new string(word.ToArray()));

                                // Добавляем игроку баллы
                                if (CurrentPlayer.playerScoreBox.Text == "")
                                {
                                    CurrentPlayer.playerScoreBox.Text = foundWord.Length.ToString();
                                }
                                else
                                {
                                    int tempScore = int.Parse(CurrentPlayer.playerScoreBox.Text);
                                    tempScore += foundWord.Length;
                                    CurrentPlayer.playerScoreBox.Text = tempScore.ToString();
                                }

                                winChecker = 0;

                                // Передаем ход
                                endTurn = true;
                                LeftTurn();
                                ClearTurn();
                                endTurn = false;
                            }

                            // Если такое слово не нашлось
                            else
                            {
                                ClearTurn();
                            }
                        }

                        else
                        {
                            ClearTurn();
                            MessageBox.Show("Это слово уже использовалось ранее!");
                        }
                    }

                    else
                    {
                        ClearTurn();
                    }
                }
            }
        }

        private void OnLetterButtonClick(object sender, EventArgs e)
        {
            if (sender is LetterButton letterButton)
            {
                letter = letterButton.Text;
                BlockButtons();
            }
        }

        ///////////////////////////////////////////////////// БЛОКИРОВАНИЕ

        private void BlockEmptyButtons()
        {
            // Блокируем все клетки кроме тех, в которых есть буква
            foreach (Button button in tableButtons)
            {
                if (button.Text == "")
                {
                    button.Enabled = false;
                }
            }
        }

        // Блокирует все клетки, куда нельзя сходить из selectedButton
        private void BlockWordButtons()
        {
            possibleButtons.Clear();

            // Рассчитываем все возможноые варианты хода из данной клетки
            foreach (TableButton button in tableButtons)
            {
                if (button == selectedButton)
                {
                    possibleButtons.Add(button);
                }

                if (button.coords.Item1 == selectedButton.coords.Item1 - 1 && button.coords.Item2 == selectedButton.coords.Item2 && button.Text != "" && !wordButtons.Contains(button))
                {
                    possibleButtons.Add(button);
                }
                if (button.coords.Item1 == selectedButton.coords.Item1 + 1 && button.coords.Item2 == selectedButton.coords.Item2 && button.Text != "" && !wordButtons.Contains(button))
                {
                    possibleButtons.Add(button);
                }
                if (button.coords.Item1 == selectedButton.coords.Item1 && button.coords.Item2 == selectedButton.coords.Item2 - 1 && button.Text != "" && !wordButtons.Contains(button))
                {
                    possibleButtons.Add(button);
                }
                if (button.coords.Item1 == selectedButton.coords.Item1 && button.coords.Item2 == selectedButton.coords.Item2 + 1 && button.Text != "" && !wordButtons.Contains(button))
                {
                    possibleButtons.Add(button);
                }
            }

            foreach (TableButton button in tableButtons)
            {
                if (!possibleButtons.Contains(button))
                {
                    button.Enabled = false;
                }
            }
        }

        // Блокирует все клетки, куда нельзя разместить букву
        private void BlockButtons()
        {
            foreach (TableButton button in tableButtons)
            {
                button.Enabled = false;
            }

            foreach (TableButton button in tableButtons)
            {
                if (button.Text != "")
                {
                    foreach (TableButton button1 in tableButtons)
                    {
                        if (button1.coords.Item1 == button.coords.Item1 - 1 && button1.coords.Item2 == button.coords.Item2)
                        {
                            button1.Enabled = true;
                        }
                        if (button1.coords.Item1 == button.coords.Item1 + 1 && button1.coords.Item2 == button.coords.Item2)
                        {
                            button1.Enabled = true;
                        }
                        if (button1.coords.Item1 == button.coords.Item1 && button1.coords.Item2 == button.coords.Item2 - 1)
                        {
                            button1.Enabled = true;
                        }
                        if (button1.coords.Item1 == button.coords.Item1 && button1.coords.Item2 == button.coords.Item2 + 1)
                        {
                            button1.Enabled = true;
                        }
                    }
                }
            }
        }

        // Блокирует панель с буквами
        private void BlockLettersButtons()
        {
            foreach (LetterButton button in letterButtons)
            {
                button.Enabled = false;
            }
        }

        private void EnableLettersButtons()
        {
            foreach (LetterButton button in letterButtons)
            {
                button.Enabled = true;
            }
        }

        private void EnableWordButtons()
        {
            foreach (TableButton button in tableButtons)
            {
                button.Enabled = true;
            }
        }

        ///////////////////////////////////////////////////// ПЕРЕДАЧА ХОДА

        // Чистим поле и память
        private void ClearTurn()
        {
            foreach (Button button in wordButtons)
            {
                button.BackColor = Color.White;
            }

            letterPlaced = false;

            word.Clear();

            letter = null;

            // Если игрок в течении хода поставил букву, и не завершил ход, то нужно ее удалить
            if (placedButton != null && endTurn == false)
            {
                placedButton.Text = "";
            }

            placedButton = null;

            wordButtons.Clear();

            EnableLettersButtons();

            EnableWordButtons();
        }

        private void LeftTurn()
        {
            CheckWin();

            // Ходит первый игрок
            if (playerTurn == false)
            {
                player1.playerTurnBox.Enabled = false;
                player2.playerTurnBox.Enabled = true;
            }

            else
            {
                player2.playerTurnBox.Enabled = false;
                player1.playerTurnBox.Enabled = true;
            }

            playerTurn = !playerTurn;

            letter = null;

            timerSeconds = timerSecondsConst;

            foreach (TableButton button in tableButtons)
            {
                if (button.placed == true)
                {
                    button.placed = false;
                }
            }
        }

        ///////////////////////////////////////////////////// ПРОВЕРКИ

        // Проверяет, содержит ли слово букву, которую разместил игрок
        private bool LetterCheck()
        {
            foreach (Button button in wordButtons)
            {
                if (wordButtons.Contains(placedButton))
                {
                    return true;
                }
            }

            return false;
        }

        // Возвращает true если слово не встречается не у одного из игроков
        private bool WordBoxCheck()
        {
            if (new string(word.ToArray()) == startWord)
            {
                return false;
            }

            int player1WordsCount = player1.playerWordBox.Items.Count;

            if (player1WordsCount != 0)
            {
                for (int i = 0; i < player1WordsCount; i++)
                {
                    string tempWord = player1.playerWordBox.Items[i].ToString();

                    if (tempWord == new string(word.ToArray()) || tempWord == startWord)
                    {
                        return false;
                    }
                }
            }

            int player2WordsCount = player2.playerWordBox.Items.Count;

            if (player2WordsCount != 0)
            {
                for (int i = 0; i < player2WordsCount; i++)
                {
                    string tempWord = player2.playerWordBox.Items[i].ToString();

                    if (tempWord == new string(word.ToArray()) || tempWord == startWord)
                    {
                        return false;
                    }
                }
            }


            return true;
        }

        private void CheckWin()
        {
            // Если оба игрока пропустили ход
            if (winChecker == 1)
            {
                gameTimer.Stop();

                // Проверяем, у какого игрока больше баллов
                int player1Score = int.Parse(player1.playerScoreBox.Text);
                int player2Score = int.Parse(player2.playerScoreBox.Text);

                if (player1Score > player2Score)
                {
                    MessageBox.Show("Игрок 1 победил!");
                }
                else if (player2Score > player1Score)
                {
                    MessageBox.Show("Игрок 2 победил!");

                }
                else
                {
                    MessageBox.Show("Ничья! Оба игрока набрали одинаковое количество баллов.");
                }

                form.Close();
            }

            // Елси закончились все свободные кнопки
            else
            {
                int flag = 0;
                foreach (Button button in tableButtons)
                {
                    if (button.Text == "")
                    {
                        flag = 1;
                    }
                }

                if (flag != 1)
                {

                    gameTimer.Stop();

                    // Проверяем, у какого игрока больше баллов
                    int player1Score = int.Parse(player1.playerScoreBox.Text);
                    int player2Score = int.Parse(player2.playerScoreBox.Text);

                    if (player1Score > player2Score)
                    {
                        MessageBox.Show("Игрок 1 победил!");
                    }
                    else if (player2Score > player1Score)
                    {
                        MessageBox.Show("Игрок 2 победил!");

                    }
                    else
                    {
                        MessageBox.Show("Ничья! Оба игрока набрали одинаковое количество баллов.");
                    }

                    form.Close();
                }
            }
        }

        ///////////////////////////////////////////////////// ОПЕРАЦИИ СО СЛОВАРЕМ

        private void GenerateStartWord()
        {
            // Берет рандомно слово из словоря 
            startWord = FindRandomWordByLength(5);
            int pointer = 0;

            int i = 2;
            int j = 0;

            // Ищем центральные кнопки по их <i, j> и записываем в них слово
            foreach (TableButton button in tableButtons)
            {
                if (button.coords.Equals(Tuple.Create(i, j)))
                {
                    button.Text = startWord.ElementAt(pointer++).ToString();
                    j++;
                }
            }
        }

        private string FindRandomWordByLength(int length)
        {
            // Создаем генератор случайных чисел
            Random random = new Random();

            // Фильтруем слова по длине
            var wordsWithTargetLength = dictionary.FindAll(word => word.Length == length);

            // Если есть слова нужной длины, выбираем случайное из них
            if (wordsWithTargetLength.Count > 0)
            {
                int randomIndex = random.Next(wordsWithTargetLength.Count);
                return wordsWithTargetLength[randomIndex];
            }

            // Возвращаем null, если слово не найдено
            return null;
        }

        /////////////////////////////////////////////////////

        private Player GetCurrentPlayer()
        {
            if (playerTurn == false)
            {
                return player1;
            }
            else return player2;
        }

        public void Form1_Load(object sender, EventArgs e) { }
    }
}
