using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.Http.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;



namespace Sea_Battle
{



    public partial class Form1 : Form
    {   //������ �����, ������ ������


        TcpClient client = new TcpClient();
        StreamReader? Reader = null;
        StreamReader? Reader2 = null;
        StreamWriter? Writer = null;
        int PlayerId = 0;



        public const int mapSize = 10;
        public int cellSize = 30;
        public string alphabet = "�������" +
            "���";

        public int[,] myMap = new int[mapSize, mapSize]; //�������� ������� 10�10
        public int[,] enemyMap = new int[mapSize, mapSize];//����� ����

        public bool isPlaying = false;

        public Button[,] myButtons = new Button[mapSize, mapSize];// ��������� ������ (������ ����� ) ������
        public Button[,] enemyButtons = new Button[mapSize, mapSize];

        private GroupBox groupBoxLength;
        private GroupBox groupBoxRasp;
        public int selectedShipLength = 1;
        public bool isVertical = true;
        private static Stopwatch? stopwatch;
        RadioButton radioButton1;
        RadioButton radioButton2;
        RadioButton radioButton3;
        RadioButton radioButton4;
        ComboBox comboBox;
        int kol_ship = 0;
        int kol_step = 0;
        int kol_radio1 = 4;
        int kol_radio2 = 3;
        int kol_radio3 = 2;
        int kol_radio4 = 1;


#pragma warning disable CS8618 // ����, �� ����������� �������� NULL, ������ ��������� ��������, �������� �� NULL, ��� ������ �� ������������. ��������, ����� �������� ���� ��� ����������� �������� NULL.
        public Form1()
#pragma warning restore CS8618 // ����, �� ����������� �������� NULL, ������ ��������� ��������, �������� �� NULL, ��� ������ �� ������������. ��������, ����� �������� ���� ��� ����������� �������� NULL.
        {

            InitializeComponent();
            this.Text = "������� ���";
            Init();

        }
        public void Init()
        {
            isPlaying = false;
            CreateMaps();//�������� �����


        }

        public async Task ServerConnection()
        {
            try
            {
                //await tcpClient.ConnectAsync("127.0.0.1", 8080);

                client.Connect("127.0.0.1", 8080);
                Reader = new StreamReader(client.GetStream());
                Reader2 = new StreamReader(client.GetStream());
                Writer = new StreamWriter(client.GetStream());
                string message = "";
                for (int i = 1; i < mapSize; i++)
                {
                    for (int j = 1; j < mapSize; j++)
                    {

                        message += $"{myMap[i, j]} ";
                    }
                }


                await Writer.WriteLineAsync(message);
                await Writer.FlushAsync();
                Task.Run(() => StartGame());

                //MessageBox.Show("����������� �����������");

            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public async Task StartGame()
        {
            string? message = await Reader.ReadLineAsync();
            PlayerId = Convert.ToInt32(message);
            while (message != "������ ����") {
                message = await Reader.ReadLineAsync();
                continue;
            }

            if (PlayerId == 1)
            {
                MessageBox.Show("���� �������� ��� ���");
                isPlaying = true;
                //Task.Run(() => OnGame(Reader!));
                return;
            } else if (PlayerId == 2)
            {
                isPlaying = false;
                MessageBox.Show("���� �������� �������� ���� ����������");
                Task.Run(() => OnGame(Reader!));
                return;
            }


        }

        public async Task OnGame(StreamReader reader)
        {
           
            while (true)
            {
                try
                {

                    //��������� ����� � ���� ������
                    string? message = await Reader.ReadLineAsync();
                    //MessageBox.Show(message.ToString());
                    //���� ������ �����, ������ �� ������� �� �������
                    if (string.IsNullOrEmpty(message)) continue;
                    //Print(message);//����� ���������


                    if (message == "����� ����� 1 �����" && message != "���������" && PlayerId == 1)
                    {
                        isPlaying = true;
                        return;
                    }
                    else if (message != "����� ����� 1 �����" && message != "���������" && PlayerId == 1)
                    {
                        string[] shoot = message.Split(' ');
                        int i = Convert.ToInt32(shoot[0]);
                        int j = Convert.ToInt32(shoot[1]);
                        if (myMap[i,j] == 0)
                        {
                            (Controls[$"btn{i}{j}"] as Button).BackColor = Color.Gray;
                        }
                        else if (myMap[Convert.ToInt32(shoot[0]), Convert.ToInt32(shoot[1])] == 1)
                        {
                            bool destroid = IsShipDestroyed(myMap, i, j);
                            (Controls[$"btn{i}{j}"] as Button).BackColor = Color.Red;
                            (Controls[$"btn{i}{j}"] as Button).Text = "X";
                            //if (destroid)
                            //{
                            //    DrawShipBordersMy(myMap, i, j);
                            //}
                        }
                        
                    } else if (message != "����� ����� 1 �����" && message == "���������" && PlayerId == 1)
                    {
                        stopwatch.Stop();
                        TimeSpan elapsed = stopwatch.Elapsed;
                        MessageBox.Show("���������!!!\n ������ �������: " + elapsed.ToString(@"hh\:mm\:ss") + "\n ������� �����: " + kol_step + "/ 100");
                        Application.Exit();
                        Controls.Clear();
                        Init();
                        Application.Exit();
                        return;
                    }
                    if (message == "����� ����� 2 �����" && message != "���������" && PlayerId == 2)
                    {
                        isPlaying = true;
                        return;
                    }
                    else if (message != "����� ����� 2 �����" && message != "���������" && PlayerId == 2)
                    {
                        string[] shoot = message.Split(' ');
                        int i = Convert.ToInt32(shoot[0]);
                        int j = Convert.ToInt32(shoot[1]);
                        if (myMap[i, j] == 0)
                        {
                            (Controls[$"btn{i}{j}"] as Button).BackColor = Color.Gray;
                        }
                        else if (myMap[Convert.ToInt32(shoot[0]), Convert.ToInt32(shoot[1])] == 1)
                        {
                            bool destroid = IsShipDestroyed(myMap, i, j);
                            (Controls[$"btn{i}{j}"] as Button).BackColor = Color.Red;
                            (Controls[$"btn{i}{j}"] as Button).Text = "X";
                            //if (destroid)
                            //{
                            //    DrawShipBordersMy(myMap, i, j);
                            //}
                        }
                    }else if (message != "����� ����� 2 �����" && message == "���������" && PlayerId == 2)
                    {
                        stopwatch.Stop();
                        TimeSpan elapsed = stopwatch.Elapsed;
                        MessageBox.Show("���������!!!\n ������ �������: " + elapsed.ToString(@"hh\:mm\:ss") + "\n ������� �����: " + kol_step + "/ 100");
                        Application.Exit();
                        Controls.Clear();
                        Init();
                       
                        return;
                       
                    }

                }
                catch
                {
                    break;
                }
            }
        }

        //public async Task IsOnGame(StreamReader reader)
        //{
        //    MessageBox.Show("sadas");

        //    while (true)
        //    {
        //        try
        //        {
        //            string? message = await reader.ReadLineAsync();
        //            //���� ������ �����, ������ �� ������� �� �������
        //            if (string.IsNullOrEmpty(message)) continue;
        //            string[] cell = message.Split(",");
        //            MessageBox.Show(message);
        //            if (myMap[Convert.ToInt32(cell[0]), Convert.ToInt32(cell[1])] != 0)
        //            {
        //                await Writer.WriteLineAsync("true");
        //                await Writer.FlushAsync();
        //            }
        //            else
        //            {
        //                await Writer.WriteLineAsync("false");
        //                await Writer.FlushAsync();
        //            }
        //        }
        //        catch
        //        {
        //            break;
        //        }
        //    }
        //}

        public void CreateMaps()
        {
            this.Width = mapSize * 2 * cellSize + 70;//������ � ������ ����� � ����������� ��  �����
            this.Height = (mapSize + 3) * cellSize + 150;
            for (int i = 0; i < mapSize; i++)//����� ������
            {
                for (int j = 0; j < mapSize; j++)
                {
                    myMap[i, j] = 0;

                    Button button = new Button();
                    button.Location = new Point(j * cellSize, i * cellSize);
                    button.Size = new Size(cellSize, cellSize);
                    button.Name = $"btn{i}{j}";
                    button.BackColor = Color.White;

                    if (j == 0 || i == 0)//����� ���� � ������� � �������
                    {
                        button.BackColor = Color.Pink;
                        if (i == 0 && j > 0)
                            button.Text = alphabet[j - 1].ToString();//����
                        if (j == 0 && i > 0)
                            button.Text = i.ToString();//�����
                    }
                    else
                    {
                        button.Click += new EventHandler(ConfigureShips!);
                    }
                    myButtons[i, j] = button;// ��������� ������ ������ ������
                    this.Controls.Add(button);
                }
            }
            for (int i = 0; i < mapSize; i++) //����� ����������
            {
                for (int j = 0; j < mapSize; j++)
                {
                    myMap[i, j] = 0;
                    enemyMap[i, j] = 0;


                    Button button = new Button();
                    button.Location = new Point(320 + j * cellSize, i * cellSize);
                    button.Size = new Size(cellSize, cellSize);
               
                    button.BackColor = Color.White;
                  

                    if (j == 0 || i == 0) //����� ���� � ������� � �������
                    {
                        button.BackColor = Color.Pink;
                        if (i == 0 && j > 0)
                            button.Text = alphabet[j - 1].ToString();
                        if (j == 0 && i > 0)
                            button.Text = i.ToString();
                    }
                    else
                    {
                        button.Click += new EventHandler(PlayerShoot!);
                    }
                    enemyButtons[i, j] = button;// ��������� ������ ������ ����������
                    this.Controls.Add(button);
                }
            }
            //������� ��� ������� + ��������� ������ ����������
            Label map1 = new Label();
            map1.Text = "����� ������";
            map1.Location = new Point(mapSize * cellSize / 2 - 30, mapSize * cellSize + 10);
            this.Controls.Add(map1);

            Label map2 = new Label();
            map2.Text = "����� ����������";
            map2.Location = new Point(300 + mapSize * cellSize / 2, mapSize * cellSize + 10);
            this.Controls.Add(map2);

            Button startButton = new Button();
            startButton.Text = "������";
            startButton.Click += new EventHandler(Start!);
            startButton.Location = new Point(280, mapSize * cellSize + 90);
            this.Controls.Add(startButton);

            //Button connectButton = new Button();
            //connectButton.Text = "�����";
            //connectButton.Click += new EventHandler(Connect!);
            //connectButton.Location = new Point(280, mapSize * cellSize + 130);
            //this.Controls.Add(connectButton);


            // �������� ������ ��� ����������� ������ ����� �������
            groupBoxLength = new GroupBox();
            groupBoxLength.Text = "�������� ����� �������";
            groupBoxLength.Size = new Size(200, 130);// ������ � ������
            groupBoxLength.Location = new Point(60, mapSize * cellSize + 50);

            //comboBox = new ComboBox();
            //comboBox.Location = new Point(270, mapSize * cellSize + 130);
            //comboBox.Size = new Size(100, 100);
            //comboBox.BackColor = Color.White;
            //this.Controls.Add(comboBox);

            // �������� ����������� ��� ������ ����� �������
            radioButton1 = new RadioButton();
            radioButton1.Text = "����� 1";
            radioButton1.Checked = true;
            radioButton1.CheckedChanged += LengthRadioButton_CheckedChanged!;
            radioButton1.Location = new Point(10, 20);

            radioButton2 = new RadioButton();
            radioButton2.Text = "����� 2";
            radioButton2.CheckedChanged += LengthRadioButton_CheckedChanged!;
            radioButton2.Location = new Point(10, 45);

            radioButton3 = new RadioButton();
            radioButton3.Text = "����� 3";
            radioButton3.CheckedChanged += LengthRadioButton_CheckedChanged!;
            radioButton3.Location = new Point(10, 70);

            radioButton4 = new RadioButton();
            radioButton4.Text = "����� 4";
            radioButton4.CheckedChanged += LengthRadioButton_CheckedChanged!;
            radioButton4.Location = new Point(10, 95);

            // ���������� ����������� ������ ����� ������� � ������
            groupBoxLength.Controls.Add(radioButton1);
            groupBoxLength.Controls.Add(radioButton2);
            groupBoxLength.Controls.Add(radioButton3);
            groupBoxLength.Controls.Add(radioButton4);
            this.Controls.Add(groupBoxLength);

            // �������� ������ ��� ����������� ������ ����� �������
            groupBoxRasp = new GroupBox();
            groupBoxRasp.Text = "�������� ������������ �������";
            groupBoxRasp.Size = new Size(230, 130);// ������ � ������
            groupBoxRasp.Location = new Point(380, mapSize * cellSize + 50);

            // �������� ����������� ��� ������ ����� �������
            RadioButton radioButtonV = new()
            {
                Text = "�����������",
                Checked = true
            };
            radioButtonV.CheckedChanged += LengthRadioButton_CheckedChanged!;
            radioButtonV.Location = new Point(10, 20);

            RadioButton radioButtonH = new()
            {
                Text = "�������������"
            };
            radioButtonH.CheckedChanged += LengthRadioButton_CheckedChanged!;
            radioButtonH.Location = new Point(10, 45);

            // ���������� ����������� ������������ ����� ������� � ������
            groupBoxRasp.Controls.Add(radioButtonV);
            groupBoxRasp.Controls.Add(radioButtonH);
            this.Controls.Add(groupBoxRasp);
        }

        private void LengthRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton selectedButton = (RadioButton)sender;

            if (selectedButton.Checked)
            {
                if (groupBoxLength.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked) is RadioButton selectedLengthRadioButton)
                {
                    selectedShipLength = int.Parse(selectedLengthRadioButton.Text.Split(' ')[1]); //����� �������
                }

                // �������� ��������� ������������ �������
                if (groupBoxRasp.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked) is RadioButton selectedRaspRadioButton)
                {
                    isVertical = selectedRaspRadioButton.Text == "�����������"; // ������������ �������
                }
            }
        }



        public async void Start(object sender, EventArgs e)//������� �� ������ ������
        {
            if (CheckIfMapIsNotEmpty())
            {

                groupBoxRasp.Visible = false;
                groupBoxLength.Visible = false;
                stopwatch = new Stopwatch();
                stopwatch.Reset(); // ���������� ���������� ����� ��������
                stopwatch.Start();
                await ServerConnection();
            }
            else
            {
                MessageBox.Show("����� ������� ���� ���������� ���������� ��� �������");
            }

        }

        public void Confirm(object sender, EventArgs e)
        {

        }
        public bool CheckIfMapIsNotEmpty()
        {

            int count = 0;
            for (int i = 1; i < mapSize; i++)
            {
                for (int j = 1; j < mapSize; j++)
                {
                    if (myMap[i, j] != 0)
                        count++;

                }
            }
            if (count != 20)
                return false;
            else return true;
        }



        public void ConfigureShips(object sender, EventArgs e)
        {
            Button? pressedButton = sender as Button;

            if (!isPlaying)
            {
                int row = pressedButton.Location.Y / cellSize;
                int col = pressedButton.Location.X / cellSize;

                // ���������, ��� ��������� ������ ��������� �� ������� ����
                if (row > 0 && col > 0)
                {
                    // ���������, ��� ��������� ������ �����
                    if (myMap[row, col] == 0)
                    {
                        // ������������� ������� � ����������� �� ��������� �����������
                        int shipLength = selectedShipLength;
                        int endRow = row;
                        int endCol = col;

                        // ���������, ��� ������� ���������� �� ����
                        if (isVertical)
                        {
                            if (endRow + shipLength - 1 < mapSize)
                            {
                                endRow += shipLength - 1;

                            }
                            else
                            {
                                MessageBox.Show("������� �� ���������� �� ����.");
                                return;
                            }
                        }
                        else
                        {
                            if (endCol + shipLength - 1 < mapSize)
                            {
                                endCol += shipLength - 1;
                            }
                            else
                            {
                                MessageBox.Show("������� �� ���������� �� ����.");
                                return;
                            }
                        }


                        // ���������, ��� ��� ������, ���������� ��������, �����
                        bool isValidPlacement = true;
                        if (isValidPlacement)
                        {
                            for (int r = row - 1; r <= endRow + 1; r++)
                            {
                                for (int c = col - 1; c <= endCol + 1; c++)
                                {
                                    if (r >= 0 && r < mapSize && c >= 0 && c < mapSize)
                                    {
                                        if (myMap[r, c] != 0)
                                        {
                                            isValidPlacement = false;
                                            break;
                                        }
                                    }
                                }

                                if (!isValidPlacement)
                                    break;
                            }
                        }

                        // ���� ��� �������� �������� �������, ������������� �������
                        if (isValidPlacement)
                        {
                            for (int r = row; r <= endRow; r++)
                            {
                                for (int c = col; c <= endCol; c++)
                                {
                                    myMap[r, c] = 1;
                                    myButtons[r, c].BackColor = Color.Black;
                                }
                            }
                            if (selectedShipLength == 4)
                            {
                                kol_radio4 = 0;
                            }
                            if (selectedShipLength == 3)
                            {
                                kol_radio3 -= 1;
                            }
                            if (selectedShipLength == 2)
                            {
                                kol_radio2 -= 1;
                            }
                            if (selectedShipLength == 1)
                            {
                                kol_radio1 -= 1;
                            }
                            if (kol_radio1 == 0) radioButton1.Enabled = false;
                            if (kol_radio2 == 0) radioButton2.Enabled = false;
                            if (kol_radio3 == 0) radioButton3.Enabled = false;
                            if (kol_radio4 == 0) radioButton4.Enabled = false;
                        }
                        else
                        {
                            MessageBox.Show("������� �� ����� ���� �������� � ���� �������.");
                        }
                    }
                }
            }
        }
        public void PlayerShoot(object sender, EventArgs e) // �������, ������� ��������
        {

            Button? pressedButton = sender as Button; //������� ������
            _ = Shoot(enemyMap, pressedButton: pressedButton, stopwatch); // ���������� ��������

            //if (!playerTurn) // �������� ���������� ������� ������
            // bot.Shoot();

            /*if (!CheckIfMapIsNotEmpty())
            {
                this.Controls.Clear();
                Init();
            }*/
        }
        public bool IsShipDestroyed(int[,] map, int row, int col) // �������� (���� ������� ��� ���)
        {
            int shipId = map[row, col];

            // ��������� �������� ������ �� �������
            if (row - 1 >= 0 && map[row - 1, col] == shipId)
                return false;

            if (row + 1 < map.GetLength(0) && map[row + 1, col] == shipId)
                return false;

            if (col - 1 >= 0 && map[row, col - 1] == shipId)
                return false;

            if (col + 1 < map.GetLength(1) && map[row, col + 1] == shipId)
                return false;

            return true; // ���� ������� ���������
        }

        public void DrShBord(int[,] map, int row, int col)
        {
            int row1 = 0;
            int col1 = 0;
            if (row > 0 && col > 0 &&  map[row - 1, col] != 2 && map[row + 1, col] != 2 && map[row, col - 1] != 2 && map[row, col + 1] != 2)
            {
                if (row - 1 != 0 && col - 1 != 0 && row +1 != 10 && col +1 !=10)
                {
                    for (int i = row - 1; i <= row + 1; i++)
                    {
                        for (int j = col - 1; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    return;
                }else if (row - 1 != 0 && col - 1 != 0 && row + 1 == 10 && col + 1 != 10)
                {
                    for (int i = row - 1; i <= row; i++)
                    {
                        for (int j = col - 1; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    return;
                }
                else if (row - 1 != 0 && col - 1 != 0 && row + 1 != 10 && col + 1 == 10)
                {
                    for (int i = row - 1; i <= row+1; i++)
                    {
                        for (int j = col - 1; j <= col; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    return;
                }

                else if (row - 1 != 0 && col - 1 != 0 && row + 1 == 10 && col + 1 == 10)
                {
                    for (int i = row - 1; i <= row; i++)
                    {
                        for (int j = col - 1; j <= col; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    return;
                }

                else if (row - 1 == 0 && col - 1 != 0 && row + 1 != 10 && col + 1 == 10)
                {
                    for (int i = row; i <= row+1; i++)
                    {
                        for (int j = col - 1; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    return;
                }


                else if (row - 1 == 0 && col - 1 == 0 && row + 1 != 10 && col + 1 != 10)
                {
                    for (int i = row; i <= row+1; i++)
                    {
                        for (int j = col; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    return;
                }


                else if (row - 1 != 0 && col - 1 == 0 && row + 1 == 10 && col + 1 != 10)
                {
                    for (int i = row; i <= row + 1; i++)
                    {
                        for (int j = col - 1; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    return;
                }

                else if (col - 1 != 0 && row - 1 == 0)
                {
                    for (int i = row; i <= row + 1; i++)
                    {
                        for (int j = col - 1; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    return;
                }
                else if (col - 1 == 0 && row - 1 != 0)
                {

                    for (int i = row - 1; i <= row + 1; i++)
                    {
                        for (int j = col; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                }
                return;
            }
            else if (row > 0 && col > 0 && map[row - 1, col] == 2 && map[row + 1, col] != 2 && map[row, col - 1] != 2 && map[row, col + 1] != 2)
            {
                if (col - 1 != 0 && col +1 !=10 && row+1!=10)
                {
                    row1 = row - 1;
                    col1 = col;
                    for (int i = row; i <= row + 1; i++)
                    {
                        for (int j = col - 1; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    DrShBord(map, row1, col1);
                }
                else if (col - 1 == 0 && row+1!=10)
                {
                    row1 = row - 1;
                    col1 = col;
                    for (int i = row; i <= row + 1; i++)
                    {
                        for (int j = col; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    DrShBord(map, row1, col1);
                }
                else if (col - 1 == 0 && row + 1 == 10)
                {
                    row1 = row - 1;
                    col1 = col;
                    for (int i = row; i <= row; i++)
                    {
                        for (int j = col; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    DrShBord(map, row1, col1);
                }

            }
            else if (row > 0 && col > 0 && row +1 != 10 && map[row - 1, col] != 2 && map[row + 1, col] == 2 && map[row, col - 1] != 2 && map[row, col + 1] != 2)
            {
                if (row - 1 != 0 && col - 1 != 0 && col + 1 != 10 )
                {
                    row1 = row + 1;
                    col1 = col;
                    for (int i = row - 1; i <= row; i++)
                    {
                        for (int j = col - 1; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    DrShBord(map, row1, col1);
                }
                else if (col - 1 != 0 && row - 1 == 0)
                {
                    row1 = row + 1;
                    col1 = col;
                    for (int i = row; i <= row; i++)
                    {
                        for (int j = col - 1; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    DrShBord(map, row1, col1);
                }
                else if (col - 1 == 0 && row - 1 != 0)
                {
                    row1 = row + 1;
                    col1 = col;
                    for (int i = row - 1; i <= row; i++)
                    {
                        for (int j = col; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    DrShBord(map, row1, col1);
                }

            }
            else if (row > 0 && col > 0 && map[row - 1, col] == 2 && map[row + 1, col] == 2 && map[row, col - 1] != 2 && map[row, col + 1] != 2)
            {
                row1 = row - 1;
                col1 = col;
                if (col - 1 != 0)
                {
                    if (enemyButtons[row, col + 1].BackColor == Color.Gray && enemyButtons[row, col - 1].BackColor == Color.Gray)
                    {
                        DrShBord(map, row1, col1);
                    }
                    else
                    {
                        enemyButtons[row, col + 1].BackColor = Color.Gray;
                        enemyButtons[row, col - 1].BackColor = Color.Gray;
                        DrShBord(map, row1, col1);
                    }
                }
                else
                {
                    if (enemyButtons[row, col + 1].BackColor == Color.Gray)
                    {
                        DrShBord(map, row1, col1);
                    }
                    else
                    {
                        enemyButtons[row, col + 1].BackColor = Color.Gray;
                        DrShBord(map, row1, col1);

                    }
                }
            }
            else if (row > 0 && col > 0 && map[row - 1, col] != 2 && map[row + 1, col] != 2 && map[row, col - 1] == 2 && map[row, col + 1] != 2)
            {
                if (row - 1 != 0)
                {
                    row1 = row;
                    col1 = col - 1;
                    for (int i = row - 1; i <= row + 1; i++)
                    {
                        for (int j = col; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    DrShBord(map, row1, col1);
                }
                else if (row - 1 == 0)
                {
                    row1 = row;
                    col1 = col - 1;
                    for (int i = row; i <= row + 1; i++)
                    {
                        for (int j = col; j <= col + 1; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                }
                DrShBord(map, row1, col1);
            }
            else if (row > 0 && col > 0 && map[row - 1, col] != 2 && map[row + 1, col] != 2 && map[row, col - 1] != 2 && map[row, col + 1] == 2)
            {
                if (row - 1 != 0 && col - 1 != 0)
                {
                    row1 = row;
                    col1 = col + 1;
                    for (int i = row - 1; i <= row + 1; i++)
                    {
                        for (int j = col - 1; j <= col; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    DrShBord(map, row1, col1);
                }
                else if (col - 1 != 0 && row - 1 == 0)
                {
                    row1 = row;
                    col1 = col + 1;
                    for (int i = row; i <= row + 1; i++)
                    {
                        for (int j = col - 1; j <= col; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    DrShBord(map, row1, col1);
                }
                else if (col - 1 == 0 && row - 1 != 0)
                {
                    row1 = row;
                    col1 = col + 1;
                    for (int i = row - 1; i <= row + 1; i++)
                    {
                        for (int j = col; j <= col; j++)
                        {
                            if (i == row && j == col)
                            {
                                continue;
                            }
                            enemyButtons[i, j].BackColor = Color.Gray;
                        }
                    }
                    DrShBord(map, row1, col1);
                }

            }
            else if (row > 0 && col > 0 && map[row - 1, col] != 2 && map[row + 1, col] != 2 && map[row, col - 1] == 2 && map[row, col + 1] == 2)
            {
                row1 = row;
                col1 = col - 1;
                if (row - 1 != 0)
                {
                    if (enemyButtons[row + 1, col].BackColor == Color.Gray && enemyButtons[row - 1, col].BackColor == Color.Gray)
                    {
                        DrShBord(map, row1, col1);
                    }
                    else
                    {
                        enemyButtons[row + 1, col].BackColor = Color.Gray;
                        enemyButtons[row - 1, col].BackColor = Color.Gray;
                        DrShBord(map, row1, col1);
                    }
                }
                else
                {
                    if (enemyButtons[row + 1, col].BackColor == Color.Gray)
                    {
                        DrShBord(map, row1, col1);
                    }
                    else
                    {
                        enemyButtons[row + 1, col].BackColor = Color.Gray;
                        DrShBord(map, row1, col1);

                    }
                }
            }
        }

        public void DrawShipBorders(int[,] map, int row, int col) // ��������� �������� ����� ��������
        {
            bool tmp = false;
            int row1 = 0;
            int col1 = 0;

            // ��������� �������� ������
            if (row > 0 && map[row - 1, col] != 2)
                if (row - 1 != 0)
                    enemyButtons[row - 1, col].BackColor = Color.Gray;
                else
                    enemyButtons[row - 1, col].BackColor = Color.Pink;
            else
            {
                tmp = true;
                row1 = row - 1;
                col1 = col;
            }

            if (row + 1 < map.GetLength(0) && map[row + 1, col] != 2)
                enemyButtons[row + 1, col].BackColor = Color.Gray;
            else
            {
                tmp = true;
                row1 = row + 1;
                col1 = col;
            }

            if (col > 0 && map[row, col - 1] != 2)
                if (col - 1 != 0)
                    enemyButtons[row, col - 1].BackColor = Color.Gray;
                else
                    enemyButtons[row, col - 1].BackColor = Color.Pink;
            else
            {
                tmp = true;
                row1 = row;
                col1 = col - 1;
            }

            if (col + 1 < map.GetLength(1) && map[row, col + 1] != 2)
                enemyButtons[row, col + 1].BackColor = Color.Gray;
            else
            {
                tmp = true;
                row1 = row;
                col1 = col + 1;
            }

            if (tmp)
                DrawShipBorders(map, row1, col1);

        }

        public void DrawShipBordersMy(int[,] map, int row, int col) // ��������� �������� ����� ��������
        {
            bool tmp = false;
            int row1 = 0;
            int col1 = 0;

            // ��������� �������� ������
            if (row > 0 && map[row - 1, col] != 2)
                if (row - 1 != 0)
                    myButtons[row - 1, col].BackColor = Color.Gray;
                else
                    myButtons[row - 1, col].BackColor = Color.Pink;
            else
            {
                tmp = true;
                row1 = row - 1;
                col1 = col;
            }

            if (row + 1 < map.GetLength(0) && map[row + 1, col] != 2)
                myButtons[row + 1, col].BackColor = Color.Gray;
            else
            {
                tmp = true;
                row1 = row + 1;
                col1 = col;
            }

            if (col > 0 && map[row, col - 1] != 2)
                if (col - 1 != 0)
                    myButtons[row, col - 1].BackColor = Color.Gray;
                else
                    myButtons[row, col - 1].BackColor = Color.Pink;
            else
            {
                tmp = true;
                row1 = row;
                col1 = col - 1;
            }

            if (col + 1 < map.GetLength(1) && map[row, col + 1] != 2)
                myButtons[row, col + 1].BackColor = Color.Gray;
            else
            {
                tmp = true;
                row1 = row;
                col1 = col + 1;
            }

            if (tmp)
                DrawShipBorders(map, row1, col1);

        }

        public async Task Shoot(int[,] map, Button pressedButton, Stopwatch? stopwatch)//�������� (���������� ����� � ������� ������)
        {
            bool hit = false;// false - �� ������, tru  e - ������
            bool isShipDestroyed = false;
                if (isPlaying)
            {
                int delta = 0;//��������

                if (pressedButton.Location.X > 320) // ���� ������ ������ �� ������� ����������
                    delta = 320;
                isPlaying = false;

                await Writer.WriteLineAsync($"{pressedButton.Location.Y / cellSize} {(pressedButton.Location.X - delta) / cellSize}");
                await Writer.FlushAsync();

                string? message = await Reader.ReadLineAsync();

                //while (message != "�����,����������������" || message != "�����,������������������" || message != "�� �����,������������������")
                //{
                //    try
                //    {


                //        message = await Reader.ReadLineAsync();
                //    }
                //    catch {
                //        continue;
                //    }
                //}

                if (message == "�����,����������������")
                {
                    MessageBox.Show(message);
                    hit = true;
                    isShipDestroyed = true;
                }
                else if (message == "�����,������������������")
                {
                    MessageBox.Show(message);
                    hit = true;
                    isShipDestroyed = false;
                }
                else if (message == "�� �����,������������������")
                {
                    MessageBox.Show(message);
                    hit = false;
                    isShipDestroyed = false;
                }else if ( message == "������")
                {
                    kol_step++;
                    stopwatch.Stop();
                    TimeSpan elapsed = stopwatch.Elapsed;
                    MessageBox.Show("������!!!\n ������ �������: " + elapsed.ToString(@"hh\:mm\:ss") + "\n ������� �����: " + kol_step + "/ 100");
                    Controls.Clear();
                    Init();
                    Application.Exit();
                    return;
                }

                if (hit) //���� ���-�� ����
                {

                    int row = pressedButton.Location.Y / cellSize;
                    int col = (pressedButton.Location.X - delta) / cellSize;

                    kol_step += 1;
                    if (isShipDestroyed)
                    {
                        // ����������� ������� ������� � �����
                        //DrawShipBorders(map, row, col);
                        //kol_ship -= 1;
                        //if (kol_ship == 0)
                        //{
                        //    stopwatch.Stop();
                        //    TimeSpan elapsed = stopwatch.Elapsed;
                        //    MessageBox.Show("������!!!\n ������ �������: " + elapsed.ToString(@"hh\:mm\:ss") + "\n ������� �����: " + kol_step + "/ 100");
                        //    this.Controls.Clear();
                        //    Init();
                        //}
                        isPlaying = true;
                    }
                    isPlaying = true;
                    map[pressedButton.Location.Y / cellSize, (pressedButton.Location.X - delta) / cellSize] = 2;//����� - 2
                    pressedButton.BackColor = Color.Red;
                    pressedButton.Text = "X";
                }
                else if (!hit && PlayerId == 1)
                {
                    pressedButton.BackColor = Color.Gray;
                    await Writer.WriteLineAsync("����� ����� 2 �����");
                    await Writer.FlushAsync();
                    Task.Run(() => OnGame(Reader!));
                }
                else if (!hit && PlayerId == 2)
                {
                    pressedButton.BackColor = Color.Gray;
                    await Writer.WriteLineAsync("����� ����� 1 �����");
                    await Writer.FlushAsync();
                    Task.Run(() => OnGame(Reader!));
                }
            }
        }

    }
}
