#addin nuget:?package=Cake.FileHelpers
using System.Net;
using System.Net.Sockets;
using System.Linq;

var TCP_LISTEN_TIMEOUT = 300;

bool IsNonMockBuild() {
    var isNonMock = Argument<bool>("non_mock", false);
    if (isNonMock) {
        Information ("Project will be build and tested with NON_MOCK_AUTH flag.");
    }
    return isNonMock;
}

void DeleteExistingTestResultFile(string fileSearchPattern)
{
    Information("Trying to find the result files from older builds");
    var files = GetFiles(fileSearchPattern);
    if (files != null && files.Count > 0) {
        Information($"Found {files.Count} matching result files");
        var fileArray = files.GetEnumerator().ToIEnumerable().ToArray();
        foreach (var item in fileArray)
        {
            DeleteFile(item);
        }
        Information($"Deleted {files.Count} files");
    }
}

bool TryGettingResultFile(string fileSearchPattern, string resultFilePath)
{
    Information("Checking for the new result file");
    var files = GetFiles(fileSearchPattern);
    if (files != null && files.Count > 0) {
        Information($"Found {files.Count} matching result files");
        var fileArray = files.GetEnumerator().ToIEnumerable().ToArray();
        var result = FileReadText(fileArray[0]);
        System.IO.File.AppendAllText(resultFilePath, result);
        return true;
    }
    return false;
}

public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator<T> enumerator) {
    while (enumerator.MoveNext()) {
        yield return enumerator.Current;
    }
    yield break;
}

Func<IPAddress, int, string, Task> DownloadTcpTextAsync = (IPAddress TCP_LISTEN_HOST, int TCP_LISTEN_PORT, string RESULTS_PATH) => System.Threading.Tasks.Task.Run(() =>
{
    var server = new TcpListener(TCP_LISTEN_HOST, TCP_LISTEN_PORT);
    server.Start();
    var listening = true;

    System.Threading.Tasks.Task.Run(() => {
        // Sleep until timeout elapses or tcp listener stopped after a successful connection
        var elapsed = 0;
        while (elapsed <= TCP_LISTEN_TIMEOUT && listening) {
            System.Threading.Thread.Sleep(1000);
            elapsed++;
        }

        // If still listening, timeout elapsed, stop the listener
        if (listening) {
            server.Stop();
            listening = false;
        }
    });

    try
    {
        TcpClient client = server.AcceptTcpClient();
        NetworkStream stream = client.GetStream();
        StreamReader data_in = new StreamReader(client.GetStream());
        var result = data_in.ReadToEnd();
        System.IO.File.AppendAllText(RESULTS_PATH, result);
        client.Close();
        server.Stop();
        listening = false;
    }
    catch
    {
        Information("Test results listener failed or timed out.");
        throw new Exception();
    }
});

void AnalyseResultFile(string FilePath)
{
    string line;
    var file = new StreamReader(FilePath);
    while ((line = file.ReadLine()) != null)
    {
        foreach (var word in line.Split(' '))
        {
            if (word == @"result=""Failed""")
            {
                throw new Exception("Tests Failed");
            }
        }
    }
    file.Close();
}
