Public Class SSCaaS
    Public MainTimer As System.Timers.Timer

    Protected Overrides Sub OnStart(ByVal args() As String)
        ' Add code here to start your service. This method should set things
        ' in motion so your service can do its work.

        If System.Diagnostics.EventLog.SourceExists("SSCaaS") = False Then
            System.Diagnostics.EventLog.CreateEventSource("SSCaaS", "Application")
        End If

        LogEvent(String.Format("SSCaaS starts on {0} {1}", System.DateTime.Now.ToString("dd-MMM-yyyy"), System.DateTime.Now.ToString("hh:mm:ss tt")), EventLogEntryType.Information)

        Randomize()
        ' We'll add somewhere between 1 and 10 minutes to the interval here
        Dim TimeSalt As Integer = Int((My.Settings.IntervalVariance * Rnd()) + 1)

        MainTimer = New System.Timers.Timer
        MainTimer.Interval = 1000 * 60 * (My.Settings.MinInterval + TimeSalt)
        MainTimer.AutoReset = True
        AddHandler MainTimer.Elapsed, AddressOf TimeElapsed
        MainTimer.Start()

        Dim HammerThread As New Threading.Thread(AddressOf HammerApi)
        HammerThread.Start()
    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.

        MainTimer.Stop()
        MainTimer.Dispose()
    End Sub

    Private Sub CallSandstormApi()
        'LogEvent("Calling Sandstorm API", EventLogEntryType.Information) 'Verbose
        Dim Req As System.Net.HttpWebRequest
        Dim TargetUri As New Uri(My.Settings.TargetUri)
        Dim Output As System.Net.HttpWebResponse
        Req = DirectCast(System.Net.HttpWebRequest.Create(TargetUri), System.Net.HttpWebRequest)
        Req.UserAgent = "SSCaaS/" & My.Application.Info.Version.ToString
        Req.KeepAlive = False
        Req.Timeout = 10000
        Req.Proxy = Nothing
        Req.ServicePoint.ConnectionLeaseTimeout = 10000
        Req.ServicePoint.MaxIdleTime = 10000

        Dim Username As String = My.Settings.Username
        Dim Password As String = My.Settings.Password
        Dim EncodedCreds As String = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(Username + ":" + Password))
        Req.Headers.Add("Authorization", "Basic " + EncodedCreds)

        Try
            Output = Req.GetResponse()
            LogEvent("Sandstorm API Response: " & CStr(CInt(Output.StatusCode)) & " " & Output.StatusCode.ToString, EventLogEntryType.Information)
            Output.Close()
        Catch WebEx As System.Net.WebException
            If WebEx.Response IsNot Nothing Then
                Using ResStream As System.IO.Stream = WebEx.Response.GetResponseStream()
                    Dim Reader As System.IO.StreamReader = New System.IO.StreamReader(ResStream)
                    Dim OutputStream As String = Reader.ReadToEnd()

                    LogEvent(OutputStream, EventLogEntryType.Error)
                End Using
            Else
                LogEvent(WebEx.ToString, EventLogEntryType.Error)
            End If
        End Try
    End Sub

    Private Sub HammerApi()
        For i As Integer = 1 To My.Settings.NumReps
            CallSandstormApi()
            If i <> My.Settings.NumReps Then
                Threading.Thread.Sleep(My.Settings.WaitTime * 1000)
            End If
        Next
    End Sub

    Private Sub TimeElapsed()
        Dim HammerThread As New Threading.Thread(AddressOf HammerApi)
        HammerThread.Start()

        MainTimer.Stop()
        Randomize()
        ' We'll add somewhere between 1 and 10 minutes to the interval here
        Dim TimeSalt As Integer = Int((My.Settings.IntervalVariance * Rnd()) + 1)

        MainTimer.Interval = 1000 * 60 * (My.Settings.MinInterval + TimeSalt)
        MainTimer.Start()
    End Sub

    Public Shared Sub LogEvent(ByVal LogMessage As String, ByVal LogType As EventLogEntryType)
        Dim EventLog As System.Diagnostics.EventLog = New System.Diagnostics.EventLog

        EventLog.Source = "SSCaaS"
        EventLog.Log = "Application"
        EventLog.WriteEntry("SSCaaS", LogMessage, LogType)
    End Sub

End Class
