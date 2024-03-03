using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;
using System.Collections;



ServerObject server = new ServerObject();// создаем сервер
await server.ListenAsync(); // запускаем сервер


class ServerObject
{


    TcpListener tcpListener = new TcpListener(IPAddress.Any, 8080); // сервер для прослушивания
    public List<ClientObject> clients = new List<ClientObject>(); // все подключения
    Queue<ClientObject> queue = new Queue<ClientObject>();// Очередь поиска опонентов 
    protected internal void RemoveConnection(string id)
    {
        // получаем по id закрытое подключение
        ClientObject? client = clients.FirstOrDefault(c => c.Id == id);
        // и удаляем его из списка подключений
        if (client != null) clients.Remove(client);
        client?.Close();
    }
    // прослушивание входящих подк  лючений
    protected internal async Task ListenAsync()
    {
        try
        {
            tcpListener.Start();
            Console.WriteLine("Сервер запущен. Ожидание игроков...");

            while (true)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();

                ClientObject clientObject = new ClientObject(tcpClient, this);
                clients.Add(clientObject);
                Task.Run(clientObject.ProcessAsync);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Disconnect();
        }
    }

    // трансляция сообщения подключенным клиентам
    protected internal async Task BroadcastMessageAsync(int i, int j, string id,string message)
    {
        bool hit = false;
        bool distroyed = false;
        bool loose = false;
        foreach (var client in clients)
        {
            if (client.Id != id) // если id клиента равно id отправителя
            {
                if(client.myMap[i, j] == 1)
                {
                    hit = true;
                    client.myMap[i, j] = 2;
                }
                if (hit)
                {
                    client.Ships--;
                    int shipId = client.myMap[i, j];
                    distroyed = true; // Весь корабль уничтожен

                    // Проверяем соседние ячейки по границе
                    if (i - 1 > 0 && client.myMap[i - 1, j] == 1)
                        distroyed = false;
                    if (i + 1 < 10 && client.myMap[i + 1, j] == 1)
                        distroyed = false;

                    if (j - 1 > 0 && client.myMap[i, j - 1] == 1)
                        distroyed = false;

                    if (j + 1 < 10 && client.myMap[i, j + 1] == 1)
                        distroyed = false;
                    
                    
                    
                    
                   
                }
                if (client.Ships == 0)
                {

                    loose = true;

                }
            }  
           
        }
        foreach (var client in clients)
        {
            if (client.Id == id) // если id клиента равно id отправителя
            {
                if(hit && distroyed && loose)
                {
                    await client.Writer.WriteLineAsync("Победа"); //передача данных
                    await client.Writer.FlushAsync();
                }
                 else if (hit && distroyed && !loose)
                {
                    await client.Writer.WriteLineAsync("Попал,КорабльУничтожен"); //передача данных
                    await client.Writer.FlushAsync();

                }
                else if (hit && !distroyed && !loose)
                {
                    await client.Writer.WriteLineAsync("Попал,КорабльНеУничтожен"); //передача данных
                    await client.Writer.FlushAsync();
                }else if (!hit)
                {
                    await client.Writer.WriteLineAsync("Не попал,КорабльНеУничтожен"); //передача данных
                    await client.Writer.FlushAsync();
                }
            }

            if (client.Id != id)
            {
                if (loose)
                {
                    Console.WriteLine("Поражение");
                    await client.Writer.WriteLineAsync("Поражение"); //передача данных
                    await client.Writer.FlushAsync();
                }
                else
                {
                    await client.Writer.WriteLineAsync(message); //передача данных
                    await client.Writer.FlushAsync();
                }
               
            }
        }
    }

    protected internal async Task Change(string message,string id)
    {
        foreach (var client in clients)
        {
            if (client.Id != id) // если id клиента  равно id отправителя
            {
                Console.WriteLine(message);
                await client.Writer.WriteLineAsync(message); //передача данных
                await client.Writer.FlushAsync();

            }


        }
    }
    protected internal async Task WaitOponent(string id)
    {
        foreach (var client in clients)
        {
            if (client.Id == id) // если id клиента  равно id отправителя
            {
                Console.WriteLine("sadasdas");
                await client.Writer.WriteLineAsync(clients.Count.ToString()); //передача данных
                await client.Writer.FlushAsync();
              
            }


        }
        if (clients.Count == 2)
        {
            Console.WriteLine(clients.Count.ToString());
            await StartGame(id);
        }

    }

    protected internal async Task StartGame(string id)
    {
        try
        {
            Console.WriteLine($"{id}");
            await clients[0].Writer.WriteLineAsync("Начало игры"); //передача данных
            await clients[0].Writer.FlushAsync();

            await clients[1].Writer.WriteLineAsync("Начало игры"); //передача данных
            await clients[1].Writer.FlushAsync();
        }
        catch(SocketException ex)
        {
            Console.WriteLine(ex.Message    );
        }
    }

    //protected internal async Task FindOponents(string id)
    //{
    //    string enemyId = "";
    //    while (enemyId == "")
    //    {
    //        foreach (var client in clients)
    //        {
    //            if (client.Id != id && client.Status == "1")
    //            {
    //                enemyId = client.Id;
    //                client.EnemyId = id;
    //            }
    //        }
    //        foreach (var client in clients)
    //        {
    //            if (client.Id == id && client.Status == "1")
    //            {
    //                client.EnemyId = enemyId;

    //            }
    //        }
    //    }
    //    await OnGame();

    //}

    protected internal async Task OnGame()
    {
       
        await clients[0].Writer.WriteLineAsync("Опонент найден ты ходишь 1"); //передача данных
        await clients[0].Writer.FlushAsync();

        await clients[1].Writer.WriteLineAsync("Опонент найден ты ходишь 2"); //передача данных
        await clients[1].Writer.FlushAsync();


    }

    // отключение всех клиентов
    protected internal void Disconnect()
    {
        foreach (var client in clients)
        {
            client.Close(); //отключение клиента
        }
        tcpListener.Stop(); //остановка сервера
    }
}
class ClientObject
{
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal int[,] myMap = new int[10, 10];
    protected internal int Ships { get; set; } = 20;
    protected internal StreamWriter Writer { get; }
    protected internal StreamReader Reader { get; }

    TcpClient client;
    ServerObject server; // объект сервера

    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        client = tcpClient;
        server = serverObject;
        // получаем NetworkStream для взаимодействия с сервером
        var stream = client.GetStream();
        // создаем StreamReader для чтения данных
        Reader = new StreamReader(stream);
        // создаем StreamWriter для отправки данных
        Writer = new StreamWriter(stream);
    }

    public async Task ProcessAsync()
    {
        try
        {
            // получаем имя пользователя
            //string? userName = await Reader.ReadLineAsync();
            string? message = $"Игрок - {Id} подключился";
            //Status = 0;
            // посылаем сообщение о входе в чат всем подключенным пользователям
            Console.WriteLine(message);
            // в бесконечном цикле получаем сообщения от клиента
            while (true)
            {
                try
                {
                    message = await Reader.ReadLineAsync();
                    if (message == null) continue;
                 
            
                    if (message.Length == 162)
                    {
                        string[] map = message.Split(" ");
                        for (int i = 1; i < 10; i++) {
                            for (int j = 1; j <  10; j++)
                            {
                                myMap[i,j] = Convert.ToInt32(map[((i-1)*9) + (j-1)]);
                            }
                        }

                        server.WaitOponent(Id).Wait();
                     
                    }else if(message == "Пусть ходит 1 игрок" || message == "Пусть ходит 2 игрок")
                    {
                        server.Change(message,Id).Wait();
                    }
                    else
                    {
                        string[] shoot = message.Split(' ');
                        await server.BroadcastMessageAsync(Convert.ToInt32(shoot[0]), Convert.ToInt32(shoot[1]), Id, message);
                    }



                    Console.WriteLine($"{message}");
                    //await server.BroadcastMessageAsync(message, Id);
                }
                catch
                {
                    message = $"{Id} отключается";
                    Console.WriteLine(message);
                    //await server.BroadcastMessageAsync(message, Id);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        //finally
        //{
        //    // в случае выхода из цикла закрываем ресурсы
        //    server.RemoveConnection(Id);
        //}
    }


    // закрытие подключения
    protected internal void Close()
    {
        Writer.Close();
        Reader.Close();
        client.Close();
    }
}
