using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BALDA_V2._0.BALDA;

namespace BALDA_V2._0
{
    public partial class Form1 : Form
    {
        private BALDA game;

        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            this.Size = new Size(650, 550);

            CreateGame();
        }

        public void CreateGame()
        {
            // Центральная таблица
            List<TableButton> tableButtons = new List<TableButton>();

            const int tableRowCount = 5;
            const int tableColCount = 5;
            const int tableCellSize = 60;

            int tableWidth = tableColCount * tableCellSize;
            int tableHeight = tableRowCount * tableCellSize;
            int tableStartX = (this.ClientSize.Width - tableWidth) / 2;

            for (int i = 0; i < tableRowCount; i++)
            {
                for (int j = 0; j < tableColCount; j++)
                {
                    TableButton btn = new TableButton();
                    btn.Width = tableCellSize;
                    btn.Height = tableCellSize;
                    btn.Top = i * tableCellSize + 50;
                    btn.Left = tableStartX + j * tableCellSize;
                    btn.coords = new Tuple<int, int>(i, j);
                    btn.BackColor = Color.White;
                    tableButtons.Add(btn);
                    Controls.Add(btn);
                }
            }

            // Панель с буквами
            List<LetterButton> letterButtons = new List<LetterButton>();

            const int lettersPanelRowCount = 2;
            const int lettersPanelColCount = 16;
            const int lettersPanelCellSize = 25;

            int lettersPanelWidth = lettersPanelColCount * lettersPanelCellSize;
            int lettersPanelHight = lettersPanelRowCount * lettersPanelCellSize;
            int lettersPanelStartX = (this.ClientSize.Width - lettersPanelWidth) / 2;
            int lettersPanelStartY = (this.ClientSize.Height - (this.ClientSize.Height - tableHeight) + 75);

            string[] russianAlphabet =
            {
                "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О",
                "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я"
            };

            int k = 0;

            for (int i = 0; i < lettersPanelRowCount; i++)
            {
                for (int j = 0; j < lettersPanelColCount; j++)
                {
                    LetterButton btn = new LetterButton();
                    btn.Width = lettersPanelCellSize;
                    btn.Height = lettersPanelCellSize;
                    btn.Top = lettersPanelStartY + i * lettersPanelCellSize;
                    btn.Left = lettersPanelStartX + j * lettersPanelCellSize;
                    btn.Text = russianAlphabet[k++];
                    letterButtons.Add(btn);
                    Controls.Add(btn);
                }
            }

            // Панель с кнопками взаимодействия игроков
            const int playerPanelButtonCount = 3;
            const int playerPanelButtonWidth = 100;
            const int playerPanelButtonHeight = 50;

            int playerPanelGap = (lettersPanelWidth - (playerPanelButtonCount * playerPanelButtonWidth)) / 2;
            int PlayerPanelWidth = lettersPanelWidth;
            int PlayerPanelStartX = lettersPanelStartX;
            int PlayerPanelStartY = (this.ClientSize.Height - (this.ClientSize.Height - tableHeight - lettersPanelHight) + 95);

            ClearButton clbtn = new ClearButton();
            clbtn.Width = playerPanelButtonWidth;
            clbtn.Height = playerPanelButtonHeight;
            clbtn.Top = PlayerPanelStartY;
            clbtn.Left = PlayerPanelStartX;
            clbtn.Text = "СТЕРЕТЬ";
            Controls.Add(clbtn);

            ReadyButton rebtn = new ReadyButton();
            rebtn.Width = playerPanelButtonWidth;
            rebtn.Height = playerPanelButtonHeight;
            rebtn.Top = PlayerPanelStartY;
            rebtn.Left = PlayerPanelStartX + playerPanelButtonWidth + playerPanelGap;
            rebtn.Text = "ОТПРАВИТЬ";
            Controls.Add(rebtn);

            SkipButton skbtn = new SkipButton();
            skbtn.Width = playerPanelButtonWidth;
            skbtn.Height = playerPanelButtonHeight;
            skbtn.Top = PlayerPanelStartY;
            skbtn.Left = PlayerPanelStartX + playerPanelButtonWidth * 2 + playerPanelGap * 2;
            skbtn.Text = "ПРОПУСТИТЬ";
            Controls.Add(skbtn);

            // Добавляем Box со словами каждого игрока
            ListBox player1WordBox = new ListBox();
            player1WordBox.Left = (this.Left + 25);
            player1WordBox.Top = 50;
            player1WordBox.Height = 200;
            Controls.Add(player1WordBox);

            ListBox player2WordBox = new ListBox();
            player2WordBox.Left = (this.Right - 160);
            player2WordBox.Top = 50;
            player2WordBox.Height = 200;
            Controls.Add(player2WordBox);

            // Добавляем Label с баллами каждого игрока
            Label player1ScoreBox = new Label();
            player1ScoreBox.Left = player1WordBox.Left + 210;
            player1ScoreBox.Top = 10;
            player1ScoreBox.Width = 50;
            player1ScoreBox.Font = new Font("Arial", 16);
            player1ScoreBox.Text = "0";
            Controls.Add(player1ScoreBox);

            Label player2ScoreBox = new Label();
            player2ScoreBox.Left = player2WordBox.Left - 115;
            player2ScoreBox.Top = 10;
            player2ScoreBox.Width = 50;
            player2ScoreBox.Font = new Font("Arial", 16);
            player2ScoreBox.Text = "0";
            Controls.Add(player2ScoreBox);

            // Добавляем Label с ходом каждого игрока
            Label player1TurnBox = new Label();
            player1TurnBox.Left = player1WordBox.Left;
            player1TurnBox.Top = player1WordBox.Top + 220;
            player1TurnBox.Text = "ХОД ИГРОКА 1";
            Controls.Add(player1TurnBox);

            Label player2TurnBox = new Label();
            player2TurnBox.Left = player2WordBox.Left + 35;
            player2TurnBox.Top = player2WordBox.Top + 220;
            player2TurnBox.Text = "ХОД ИГРОКА 2";
            player2TurnBox.Enabled = false;
            Controls.Add(player2TurnBox);

            // Добавляем Label с таймером
            Timer gameTimer = new Timer();
            gameTimer.Interval = 1000;
            gameTimer.Start();

            Label timerLabel = new Label();
            timerLabel.Top = 10;
            timerLabel.Left = 290;
            timerLabel.Width = 55;
            timerLabel.Font = new Font("Arial", 12);
            timerLabel.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(timerLabel);

            game = new BALDA(tableButtons, letterButtons, rebtn, clbtn, skbtn, player1WordBox, player2WordBox, player1ScoreBox, player2ScoreBox, player1TurnBox, player2TurnBox, gameTimer, timerLabel, this);
        }
    }
}
