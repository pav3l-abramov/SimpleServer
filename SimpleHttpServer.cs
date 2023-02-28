using System;
using System.Net;
using System.Reflection.Metadata;
using System.Text;

class SimpleHttpServer
{
    private const string UriPrefix = "http://127.0.0.1:8080/";
    private readonly string _baseFolder;
    private readonly HttpListener listener = new HttpListener();
    private readonly StreamWriter logWriter;

    static void Main(string[] args)
    {
        var server = new SimpleHttpServer(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName);
        server.Start();
    }
    public SimpleHttpServer(string baseFolder)
    {
        _baseFolder = baseFolder;
        var logFilePath = Path.Combine(_baseFolder, "log.txt");
        logWriter = new StreamWriter(logFilePath, true, Encoding.UTF8);
        listener.Prefixes.Add(UriPrefix);
    }
    public void Start()
    {
        listener.Start();
        Console.WriteLine("server started");
        while (true)
        {
            var context = listener.GetContext();
            Process(context);
        }
    }

    private void Process(HttpListenerContext context)
    {
        var filename = context.Request.Url.AbsolutePath;
        Console.WriteLine($"request: {filename} from {context.Request.RemoteEndPoint.Address}");
        

        var filePath = Path.Combine(_baseFolder, filename.TrimStart('/'));
        if (File.Exists(filePath))
        {
            context.Response.StatusCode = 200;
            SendFile(context.Response, filePath);
            logWriter.WriteLine($"{DateTime.Now} {context.Request.RemoteEndPoint.Address} {filename} {context.Response.StatusCode}");
            logWriter.Flush();
        }
        else
        {
            context.Response.StatusCode = 404;
            logWriter.WriteLine($"{DateTime.Now} {context.Request.RemoteEndPoint.Address} {filename} {context.Response.StatusCode}");
            logWriter.Flush();
            context.Response.Close();
        }
    }

    private void SendFile(HttpListenerResponse response, string filePath)
    {
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            stream.CopyTo(response.OutputStream);
        }

        response.Close();
    }

}

